using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.BasicMessage;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Models.Events;
using Hyperledger.Aries.Storage;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.ViewModels.Account;
using Osma.Mobile.App.ViewModels.Connections;
using Osma.Mobile.App.ViewModels.CreateInvitation;
using Osma.Mobile.App.ViewModels.Credentials;
using Osma.Mobile.App.ViewModels.PinAuth;
using Osma.Mobile.App.ViewModels.Proofs;
using ReactiveUI;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels
{
    public class MainViewModel : ABaseViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IWalletRecordService _walletRecordService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly ILifetimeScope _lifetimeScope;

        public MainViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            IAgentProvider agentContextProvider,
            IWalletRecordService walletRecordService,
            ILifetimeScope lifetimeScope,
            ConnectionsViewModel connectionsViewModel,
            CredentialsViewModel credentialsViewModel,
            AccountViewModel accountViewModel,            
            CreateInvitationViewModel createInvitationViewModel,
            ProofRequestsViewModel proofRequestsViewModel,
            ScanInvitationViewModel scanInvitationViewModel
            ) : base(nameof(MainViewModel), userDialogs, navigationService)
        {
            Connections = connectionsViewModel;
            Credentials = credentialsViewModel;
            Account = accountViewModel;
            CreateInvitation = createInvitationViewModel;
            ProofRequests = proofRequestsViewModel;
            ScanInvitation = scanInvitationViewModel;
            _eventAggregator = eventAggregator;
            _walletRecordService = walletRecordService;
            _agentContextProvider = agentContextProvider;
            _lifetimeScope = lifetimeScope;
            //for prompting dialog on connection events
            //WalletEventService.Init(navigationService);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await Connections.InitializeAsync(null);
            await Credentials.InitializeAsync(null);
            await Account.InitializeAsync(null);
            await CreateInvitation.InitializeAsync(null);
            await ProofRequests.InitializeAsync(null);
            await ScanInvitation.InitializeAsync(null);
            InitializeNotificationEventListeners();
            await base.InitializeAsync(navigationData);
            if (Preferences.Get(AppConstant.PinAuthEnabled, false))
                await NavigationService.NavigateToAsync<PinAuthViewModel>();

            //string[] passphrase = new string[] { "hello", "world", "this", "is", "the", "generated", "passphrase", "this", "is", "the" };
            //var testVm = _lifetimeScope.Resolve<VerifyPasswordViewModel>(new NamedParameter("passwordArray", passphrase));
            //await NavigationService.NavigateToAsync(testVm);
        }

        private void InitializeNotificationEventListeners()
        {
            _eventAggregator.GetEventByType<ServiceMessageProcessingEvent>()
                         .Where
                         (_ => 
                             _.MessageType == MessageTypes.BasicMessageType ||
                             _.MessageType == MessageTypes.ConnectionRequest ||
                             _.MessageType == MessageTypes.IssueCredentialNames.OfferCredential ||
                             _.MessageType == MessageTypes.IssueCredentialNames.IssueCredential
                         )
                         .Subscribe(_ => BuildNotification(_.MessageType, _.RecordId));            
        }

        private async void BuildNotification(string messageEvent, string recordId)
        {
            var context = await _agentContextProvider.GetContextAsync();
            switch(messageEvent)
            {
                case MessageTypes.BasicMessageType:
                case MessageTypesHttps.BasicMessageType:
                    var msgRecord = await _walletRecordService.GetAsync<BasicMessageRecord>(context.Wallet, recordId);
                    var connectionRecord = await _walletRecordService.GetAsync<ConnectionRecord>(context.Wallet, msgRecord.ConnectionId);
                    TriggerNotification("New message", "New message from " + connectionRecord.Alias.Name);
                    break;
                case MessageTypes.ConnectionRequest:
                case MessageTypesHttps.ConnectionRequest:
                    TriggerNotification("New Connection Request", "You received a connectino request");
                    break;
                case MessageTypes.IssueCredentialNames.OfferCredential:
                case MessageTypesHttps.IssueCredentialNames.OfferCredential:
                    TriggerNotification("New Connection Request", "You received a connectino request");
                    break;
            }
        }
                
        public void TriggerNotification(string title, string message)
        {
            INotificationManager notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.SendNotification(title, message);
        }        

        #region Bindable Properties

        private ConnectionsViewModel _connections;
        public ConnectionsViewModel Connections
        {
            get => _connections;
            set => this.RaiseAndSetIfChanged(ref _connections, value);
        }

        private CredentialsViewModel _credentials;
        public CredentialsViewModel Credentials
        {
            get => _credentials;
            set => this.RaiseAndSetIfChanged(ref _credentials, value);
        }

        private AccountViewModel _account;
        public AccountViewModel Account
        {
            get => _account;
            set => this.RaiseAndSetIfChanged(ref _account, value);
        }

        private CreateInvitationViewModel _createInvitation;
        public CreateInvitationViewModel CreateInvitation
        {
            get => _createInvitation;
            set => this.RaiseAndSetIfChanged(ref _createInvitation, value);
        }

        private ProofRequestsViewModel _proofRequests;
        public ProofRequestsViewModel ProofRequests
        {
            get => _proofRequests;
            set => this.RaiseAndSetIfChanged(ref _proofRequests, value);
        }

        private ScanInvitationViewModel _scanInvitation;
        public ScanInvitationViewModel ScanInvitation
        {
            get => _scanInvitation;
            set => this.RaiseAndSetIfChanged(ref _scanInvitation, value);
        }

        #endregion
    }
}
