using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Hyperledger.Aries.Contracts;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Services;
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
        public MainViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            IEventAggregator eventAggregator,
            ConnectionsViewModel connectionsViewModel,
            CredentialsViewModel credentialsViewModel,
            AccountViewModel accountViewModel,            
            CreateInvitationViewModel createInvitationViewModel,
            ProofRequestsViewModel proofRequestsViewModel
            ) : base(nameof(MainViewModel), userDialogs, navigationService)
        {
            Connections = connectionsViewModel;
            Credentials = credentialsViewModel;
            Account = accountViewModel;
            CreateInvitation = createInvitationViewModel;
            ProofRequests = proofRequestsViewModel;
            _eventAggregator = eventAggregator;
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
            InitializeNotificationEventListeners();
            await base.InitializeAsync(navigationData);
            if (Preferences.Get(AppConstant.PinAuthEnabled, false))
                await NavigationService.NavigateToAsync<PinAuthViewModel>();           
        }

        private void InitializeNotificationEventListeners()
        {
           _eventAggregator.GetEventByType<ApplicationEvent>()
                        .Where(_ => _.Type == ApplicationEventType.ConnectionRequestReceived)
                        .Subscribe(_ => TriggerNotification("Connection Request", "You recieved a new conection request"));
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

        #endregion
    }
}
