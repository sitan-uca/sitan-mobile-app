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

namespace Osma.Mobile.App.ViewModels.Account
{
    //TODO the image resources in the android package are missing. Populate them 
    public class AccountViewModel : ABaseViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public AccountViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService
        ) : base(
            "Account",
            userDialogs,
            navigationService
        )
        {
            _appVersion = AppInfo.VersionString;
            _buildVersion = AppInfo.BuildString;
#if DEBUG
            _fullName = "Jamie Doe";
            _avatarUrl = "http://i.pravatar.cc/100";
#endif

            _authSwitchToggled = Preferences.Get(AppConstant.PinAuthEnabled, false);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            
            await base.InitializeAsync(navigationData);
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

        //TODO: this method crashes the app. Fix it
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
        #endregion

        #region Bindable Properties

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set => this.RaiseAndSetIfChanged(ref _fullName, value);
        }

        private string _avatarUrl;
        public string AvatarUrl
        {
            get => _avatarUrl;
            set => this.RaiseAndSetIfChanged(ref _avatarUrl, value);
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
