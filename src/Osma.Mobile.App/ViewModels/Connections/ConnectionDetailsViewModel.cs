using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.Discovery;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class ConnectionDetailsViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentContextProvider;
        private readonly IMessageService _messageService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiscoveryService _discoveryService;
        private readonly IConnectionService _connectionService;
        private string _connectionId;

        public ConnectionDetailsViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            IAgentProvider agentContextProvider,
            IMessageService messageService,
            IEventAggregator eventAggregator,
            IDiscoveryService discoveryService,
            IConnectionService connectionService
            ) : base(
                nameof(ConnectionDetailsViewModel),
                userDialogs,
                navigationService
                ) 
        {
            _agentContextProvider = agentContextProvider;
            _messageService = messageService;
            _discoveryService = discoveryService;
            _connectionService = connectionService;
            _eventAggregator = eventAggregator;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            _connectionId = navigationData.ToString();
            var context = await _agentContextProvider.GetContextAsync();
            var con = await _connectionService.GetAsync(context, _connectionId);

            MyDid = con.MyDid;
            TheirDid = con.TheirDid;

            await base.InitializeAsync(navigationData);
        }

        #region Bindable command
        public ICommand DeleteConnectionCommand => new Command(async () =>
        {
            var dialog = DialogService.Loading("Deleting");
            var context = await _agentContextProvider.GetContextAsync();

            await _connectionService.DeleteAsync(context, _connectionId);

            _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionsUpdated });

            if (dialog.IsShowing)
            {
                dialog.Hide();
                dialog.Dispose();
            }

            //await NavigationService.RemoveLastFromBackStackAsync();
            await NavigationService.NavigateBackAsync();
        });
        #endregion

        #region Bindable Properties
        private string _connection;
        public string Connection
        {
            get => _connection;
            set => this.RaiseAndSetIfChanged(ref _connection, value);
        }

        private string _myDid;
        public string MyDid
        {
            get => _myDid;
            set => this.RaiseAndSetIfChanged(ref _myDid, value);
        }

        private string _theirDid;
        public string TheirDid
        {
            get => _theirDid;
            set => this.RaiseAndSetIfChanged(ref _theirDid, value);
        }

        private string _connectionImageUrl;
        public string ConnectionImageUrl
        {
            get => _connectionImageUrl;
            set => this.RaiseAndSetIfChanged(ref _connectionImageUrl, value);
        }
        #endregion
    }
}
