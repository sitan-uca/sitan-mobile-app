using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using Xamarin.Forms;
using Xamarin.Essentials;
using Osma.Mobile.App.Views.Legal;
using Osma.Mobile.App.ViewModels.PinAuth;
using System;
using System.Diagnostics;
using Osma.Mobile.App.Views.PinAuth;
using System.ComponentModel;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Osma.Mobile.App.Events;
using System.Reactive.Linq;
using Osma.Mobile.App.Converters;

namespace Osma.Mobile.App.ViewModels.Account
{
    
    public class AccountViewModel : ABaseViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly IProvisioningService _provisioningService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IEventAggregator _eventAggregator;

        public AccountViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            IProvisioningService provisioningService,
            IAgentProvider agentContextProvider,
            IEventAggregator eventAggregator
        ) : base(
            "Account",
            userDialogs,
            navigationService
        )
        {
            _appVersion = AppInfo.VersionString;
            _buildVersion = AppInfo.BuildString;
            _provisioningService = provisioningService;
            _agentContextProvider = agentContextProvider;
            _eventAggregator = eventAggregator;
                                    
            //#if DEBUG
            //            _fullName = "Jamie Doe";
            //            _avatarUrl = "http://i.pravatar.cc/100";
            //#endif

            _authSwitchToggled = Preferences.Get(AppConstant.PinAuthEnabled, false);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            _eventAggregator.GetEventByType<ApplicationEvent>()
                            .Where(_ => _.Type == ApplicationEventType.ProvisioningRecordUpdated)
                            .Subscribe(async _ => await InitializeAgentInfo());

            await InitializeAgentInfo();
            await base.InitializeAsync(navigationData);
        }

        public async Task InitializeAgentInfo()
        {
            var context = await _agentContextProvider.GetContextAsync();
            var proviosioningAgent = await _provisioningService.GetProvisioningAsync(context.Wallet);
            FullName = proviosioningAgent.Owner.Name;
            AvatarUrl = proviosioningAgent.Owner.ImageUrl ?? "account_icon.png";            
            AgentImageSource = Base64StringToImageSource.Base64StringToImage(proviosioningAgent.Owner.ImageUrl);
     
        }

        public async Task NavigateToBackup()
        {
            await DialogService.AlertAsync("Navigate to Backup");
        }

        public async Task NavigateToAuthentication()
        {
            //await DialogService.AlertAsync("Navigate to authentication");
            if (Preferences.Get(AppConstant.PinAuthEnabled, false))
                await NavigationService.NavigateToAsync<PinAuthViewModel>(nameof(CreatePinAuthViewModel));
            else
                await NavigationService.NavigateToAsync<CreatePinAuthViewModel>();
        }

        //TODO: the html page is not reteieved from resources
        public async Task NavigateToLegalPage()
        {
            var legalPage = new LegalPage();
            await NavigationService.NavigateToAsync(legalPage, NavigationType.Modal);
        }

        public async Task NavigateToDebug()
        {
            await DialogService.AlertAsync("Navigate to debug page");
        }

        #region Bindable Command        

        public ICommand NavigateToBackupCommand => new Command(async () => await NavigateToBackup());

        public ICommand NavigateToAuthenticationCommand => new Command(async () => await NavigateToAuthentication());

        public ICommand NavigateToLegalPageCommand => new Command(async () => await NavigateToLegalPage());

        public ICommand NavigateToDebugCommand => new Command(async () => await NavigateToDebug());

        public ICommand NotifySwitchToggle => new Command(() => {
            _authSwitchToggled = Preferences.Get(AppConstant.PinAuthEnabled, false);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AuthSwitchToggled"));
        });

        public EventHandler<ToggledEventArgs> ToggleAuthentication => async (sender, e) =>
        {
            if (e.Value)
            {
                //Debug.WriteLine("\n\nONONONON\n\n");
                await NavigateToAuthentication();
            }
            else
            {
                //Debug.WriteLine("\n\nOFFOFFOFFOFFOFF\n\n");
                await NavigationService.NavigateToAsync<PinAuthViewModel>(nameof(AccountViewModel));
            }
        };

        //public EventHandler ProfileInfoTapped => async (sender, e) =>
        //{
        //    await NavigationService.NavigateToAsync<ProfileViewModel>();
        //};

        public ICommand NavigateToProfile => new Command(async () => await NavigationService.NavigateToAsync<ProfileViewModel>());

        #endregion

        #region Bindable Properties

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set 
            { 
                this.RaiseAndSetIfChanged(ref _fullName, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FullName"));
            }
        }

        private string _avatarUrl;
        public string AvatarUrl
        {
            get => _avatarUrl;
            set
            {
                this.RaiseAndSetIfChanged(ref _avatarUrl, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AvatarUrl"));
            }
        }

        private ImageSource _agentImageSource;
        public ImageSource AgentImageSource
        {
            get => _agentImageSource;
            set
            {
                this.RaiseAndSetIfChanged(ref _agentImageSource, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AgentImageSource"));
            }
        }

        private bool _showDebug;
        public bool ShowDebug
        {
            get => _showDebug;
            set => this.RaiseAndSetIfChanged(ref _showDebug, value);
        }

        private string _appVersion;
        public string AppVersion
        {
            get => _appVersion;
            set => this.RaiseAndSetIfChanged(ref _appVersion, value);
        }

        private string _buildVersion;
        public string BuildVersion
        {
            get => _buildVersion;
            set => this.RaiseAndSetIfChanged(ref _buildVersion, value);
        }

        private bool _authSwitchToggled;
        public bool AuthSwitchToggled
        {
            get => _authSwitchToggled;
            set //this.RaiseAndSetIfChanged(ref _authSwitchToggled, value); 
            {
                this.RaiseAndSetIfChanged(ref _authSwitchToggled, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AuthSwitchToggled"));
            }
        }

        #endregion
    }
}
