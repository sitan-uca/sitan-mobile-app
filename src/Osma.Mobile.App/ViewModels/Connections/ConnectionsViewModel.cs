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
using Hyperledger.Aries.Features.BasicMessage;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Models.Events;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Utils;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Extensions;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Utilities;
using Osma.Mobile.App.ViewModels.CreateInvitation;
using Osma.Mobile.App.Views.Components;
using Osma.Mobile.App.Views.Connections;
using ReactiveUI;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using static Osma.Mobile.App.Views.Components.SegmentedBarControl;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class ConnectionsViewModel : ABaseViewModel
    {
        private readonly IConnectionService _connectionService;
        private readonly IWalletRecordService _walletRecordService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IProvisioningService _provisioningService;
        private readonly ILifetimeScope _scope;

        public ConnectionsViewModel(IUserDialogs userDialogs,
                                    INavigationService navigationService,
                                    IConnectionService connectionService,
                                    IWalletRecordService walletRecordService,
                                    IAgentProvider agentContextProvider,
                                    IEventAggregator eventAggregator,
                                    IProvisioningService provisioningService,
                                    ILifetimeScope scope) :
                                    base("Connections", userDialogs, navigationService)
        {
            _connectionService = connectionService;
            _walletRecordService = walletRecordService;
            _agentContextProvider = agentContextProvider;
            _eventAggregator = eventAggregator;
            _provisioningService = provisioningService;
            _scope = scope;
            _selectedConnectionsFilter = nameof(ConnectionState.Connected);
            _searchTermIsEmpty = string.IsNullOrWhiteSpace(_searchTerm) || string.IsNullOrEmpty(_searchTerm);

            //this.WhenAnyValue(x => x.SearchTerm)
            //            .Throttle(TimeSpan.FromMilliseconds(200))
            //            .InvokeCommand(RefreshCommand);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await RefreshConnections();

            _eventAggregator.GetEventByType<ApplicationEvent>()
                            .Where(_ => _.Type == ApplicationEventType.ConnectionsUpdated)
                            .Subscribe(async _ => await RefreshConnections());

            _eventAggregator.GetEventByType<ServiceMessageProcessingEvent>()
                         .Where
                         (_ =>
                             _.MessageType == MessageTypes.BasicMessageType ||
                             _.MessageType == MessageTypes.ConnectionRequest ||
                             _.MessageType == MessageTypes.ConnectionResponse
                         )
                         .Subscribe(async _ => await NotifyAndRefresh(_));

            await base.InitializeAsync(navigationData);
        }

        private async Task NotifyAndRefresh(ServiceMessageProcessingEvent _event)
        {
            var context = await _agentContextProvider.GetContextAsync();
            ConnectionRecord connectionRecord;
            switch (_event.MessageType)
            {
                case MessageTypes.BasicMessageType:                
                    var msgRecord = await _walletRecordService.GetAsync<BasicMessageRecord>(context.Wallet, _event.RecordId);
                    connectionRecord = await _walletRecordService.GetAsync<ConnectionRecord>(context.Wallet, msgRecord.ConnectionId);
                    NotificationService.TriggerNotification(connectionRecord.Alias.Name, msgRecord.Text);
                    break;
                case MessageTypes.ConnectionRequest:
                    await RefreshConnections();
                    connectionRecord = await _walletRecordService.GetAsync<ConnectionRecord>(context.Wallet, _event.RecordId);
                    NotificationService.TriggerNotification("Connection Request", connectionRecord.Alias.Name + "would like to establish pairwise connection with you");
                    break;
                case MessageTypes.ConnectionResponse:         
                    await RefreshConnections();
                    break;
            }
        }

        public async Task RefreshConnections()
        {
            RefreshingConnections = true;            

            var context = await _agentContextProvider.GetContextAsync();
            IList<ConnectionViewModel> connectionVms = new List<ConnectionViewModel>();
            List<ConnectionRecord> records = null;
            switch (SelectedConnectionsFilter)
            {
                case nameof(ConnectionState.Connected):
                    records = await _connectionService.ListConnectedConnectionsAsync(context);
                    break;
                case nameof(ConnectionState.Negotiating):
                    records = await _connectionService.ListNegotiatingConnectionsAsync(context);
                    break;
                case nameof(ConnectionState.Invited):
                    records = await _connectionService.ListInvitedConnectionsAsync(context);
                    break;
            }

            foreach (var record in records)
            {
                if (record.Id == Preferences.Get(AppConstant.MediatorConnectionIdTagName, string.Empty))
                    continue;

                var connection = _scope.Resolve<ConnectionViewModel>(new NamedParameter("record", record));
                connectionVms.Add(connection);
            }

            IList<ConnectionViewModel> filteredConnectionsVms = FilterConnectionsSearchTerm(SearchTerm, connectionVms);

            //TODO need to compare with the currently displayed connections rather than disposing all of them
            Connections.Clear();
            Connections.InsertRange(filteredConnectionsVms);
            HasConnections = filteredConnectionsVms.Any();

            RefreshingConnections = false;
        }

        private IList<ConnectionViewModel> FilterConnectionsSearchTerm(string term, IList<ConnectionViewModel> connections)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return connections;
            }
            // Basic search
            var filtered = connections.Where(connectionViewModel => connectionViewModel.ConnectionName.ToLower().Contains(term.ToLower())).ToList();
            return filtered;
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

        //public ICommand ConnectionSubTabChange => new Command<object>(async (tab) =>
        //{
        //await UserDialogs.Instance.AlertAsync($"{tab}");
        //await RefreshConnections(tab.ToString());
        //});

        public ICommand FilterConnections => new Command(async () =>
        {
            await PopupNavigation.Instance.PushAsync(new FilterConnectionsPopupPage
            {
                FilterValue = SelectedConnectionsFilter,
                PrimaryCommand = new Command(async (filterValue) =>
                {
                    SelectedConnectionsFilter = !string.IsNullOrEmpty(filterValue?.ToString()) ? filterValue.ToString() : _selectedConnectionsFilter;
                    await RefreshConnections();
                    await PopupNavigation.Instance.PopAsync();
                }),
                SecondaryCommand = new Command(async () => await PopupNavigation.Instance.PopAsync())
            });
        });

        public ICommand ToolbarSearchCommand => new Command(async () =>
        {
            await PopupNavigation.Instance.PushAsync(new SearchBarPopupComponentPage
            {
                SearchTermAttr = SearchTerm,
                //PrimaryCommand = new Command((searchTerm) =>
                //{
                //    SearchTerm = searchTerm?.ToString();
                //}),
                SecondaryCommand = new Command(async () => await PopupNavigation.Instance.PopAsync()),
                OnTextChangedEvent = async (sender, e) => 
                {
                    SearchTerm = e.NewTextValue;
                    await RefreshConnections();
                }
            });
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

        private string _selectedConnectionsFilter;
        public string SelectedConnectionsFilter
        {
            get => _selectedConnectionsFilter;
            set => this.RaiseAndSetIfChanged(ref _selectedConnectionsFilter, value);
        }

        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set 
            {
                this.RaiseAndSetIfChanged(ref _searchTerm, value);
                SearchTermIsEmpty = string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
            }
        }

        private bool _searchTermIsEmpty;
        public bool SearchTermIsEmpty
        {
            get => _searchTermIsEmpty;
            set => this.RaiseAndSetIfChanged(ref _searchTermIsEmpty, value);
        }

        #endregion
    }
}
