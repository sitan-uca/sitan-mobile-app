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
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Models.Events;
using Hyperledger.Aries.Storage;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.ViewModels.Account;
using Osma.Mobile.App.ViewModels.Connections;
using Osma.Mobile.App.ViewModels.CreateInvitation;
using Osma.Mobile.App.ViewModels.Credentials;
using Osma.Mobile.App.ViewModels.PinAuth;
using Osma.Mobile.App.ViewModels.Proofs;
using Osma.Mobile.App.ViewModels.ScanQrCode;
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
            ScanQrCodeViewModel scanQrCodeViewModel
            ) : base(nameof(MainViewModel), userDialogs, navigationService)
        {
            Connections = connectionsViewModel;
            Credentials = credentialsViewModel;
            Account = accountViewModel;
            CreateInvitation = createInvitationViewModel;
            ProofRequests = proofRequestsViewModel;
            ScanQrCode = scanQrCodeViewModel;
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
            await ScanQrCode.InitializeAsync(null);            
            await base.InitializeAsync(navigationData);
            if (Preferences.Get(AppConstant.PinAuthEnabled, false))
                await NavigationService.NavigateToAsync<PinAuthViewModel>();       
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

        private ScanQrCodeViewModel _scanQrCode;
        public ScanQrCodeViewModel ScanQrCode
        {
            get => _scanQrCode;
            set => this.RaiseAndSetIfChanged(ref _scanQrCode, value);
        }

        #endregion
    }
}
