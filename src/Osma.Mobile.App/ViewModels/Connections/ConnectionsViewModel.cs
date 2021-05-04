using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Utils;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Extensions;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Utilities;
using Osma.Mobile.App.ViewModels.CreateInvitation;
using ReactiveUI;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using static Osma.Mobile.App.Views.Components.SegmentedBarControl;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class ConnectionsViewModel : ABaseViewModel
    {
        private readonly IConnectionService _connectionService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IProvisioningService _provisioningService;
        private readonly ILifetimeScope _scope;

        public ConnectionsViewModel(IUserDialogs userDialogs,
                                    INavigationService navigationService,
                                    IConnectionService connectionService,
                                    IAgentProvider agentContextProvider,
                                    IEventAggregator eventAggregator,
                                    IProvisioningService provisioningService,
                                    ILifetimeScope scope) :
                                    base("Connections", userDialogs, navigationService)
        {
            _connectionService = connectionService;
            _agentContextProvider = agentContextProvider;
            _eventAggregator = eventAggregator;
            _provisioningService = provisioningService;
            _scope = scope;            
        }

        public override async Task InitializeAsync(object navigationData)
        {            
            await RefreshConnections();

            _eventAggregator.GetEventByType<ApplicationEvent>()
                            .Where(_ => _.Type == ApplicationEventType.ConnectionsUpdated)
                            .Subscribe(async _ => await RefreshConnections());

            await base.InitializeAsync(navigationData);
        }


        public async Task RefreshConnections(string tab = null)
        {
            RefreshingConnections = true;

            var context = await _agentContextProvider.GetContextAsync();
            IList<ConnectionViewModel> connectionVms = new List<ConnectionViewModel>();
            List<ConnectionRecord> records = null;
            
            if (tab == null || tab.Equals(nameof(ConnectionState.Connected)))
                records = await _connectionService.ListConnectedConnectionsAsync(context);
            else if (tab.Equals(nameof(ConnectionState.Negotiating)))
                records = await _connectionService.ListNegotiatingConnectionsAsync(context);
            else if (tab.Equals(nameof(ConnectionState.Invited)))
                records = await _connectionService.ListInvitedConnectionsAsync(context);          

            foreach (var record in records)
            {
                if (record.Id == Preferences.Get(AppConstant.MediatorConnectionIdTagName, string.Empty))
                    continue;

                var connection = _scope.Resolve<ConnectionViewModel>(new NamedParameter("record", record));
                connectionVms.Add(connection);
            }

            //TODO need to compare with the currently displayed connections rather than disposing all of them
            Connections.Clear();
            Connections.InsertRange(connectionVms);
            HasConnections = connectionVms.Any();

            RefreshingConnections = false;
        }

        public async Task ScanInvite()
        {
            var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
            var opts = new ZXing.Mobile.MobileBarcodeScanningOptions { PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat } };

            var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            var result = await scanner.Scan(opts);
            if (result == null) return;

            ConnectionInvitationMessage invitation;

            try
            {
                invitation = await MessageDecoder.ParseMessageAsync(result.Text) as ConnectionInvitationMessage
                    ?? throw new Exception("Unknown message type");
            }
            catch (Exception)
            {
                DialogService.Alert("Invalid invitation!");
                return;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                await NavigationService.NavigateToAsync<AcceptInviteViewModel>(invitation, NavigationType.Modal);
            });
        }

        public async Task SelectConnection(ConnectionViewModel connection) => await NavigationService.NavigateToAsync(connection);       

        #region Bindable Command
        public ICommand RefreshCommand => new Command(async () => await RefreshConnections());

        public ICommand ScanInviteCommand => new Command(async () => await ScanInvite());

        public ICommand CreateInvitationCommand => new Command(async () => await NavigationService.NavigateToAsync<CreateInvitationViewModel>(null, NavigationType.Modal));

        public ICommand SelectConnectionCommand => new Command<ConnectionViewModel>(async (connection) =>
        {
            if (connection != null)
                await SelectConnection(connection);
        });

        public ICommand ConnectionSubTabChange => new Command<object>(async (tab) =>
        {
            //await UserDialogs.Instance.AlertAsync($"{tab}");
            await RefreshConnections(tab.ToString());
        });        

        #endregion

        #region Bindable Properties
        private RangeEnabledObservableCollection<ConnectionViewModel> _connections = new RangeEnabledObservableCollection<ConnectionViewModel>();
        public RangeEnabledObservableCollection<ConnectionViewModel> Connections
        {
            get => _connections;
            set => this.RaiseAndSetIfChanged(ref _connections, value);
        }

        private bool _hasConnections;
        public bool HasConnections
        {
            get => _hasConnections;
            set => this.RaiseAndSetIfChanged(ref _hasConnections, value);
        }

        private bool _refreshingConnections;
        public bool RefreshingConnections
        {
            get => _refreshingConnections;
            set => this.RaiseAndSetIfChanged(ref _refreshingConnections, value);
        }       

        #endregion
    }
}
