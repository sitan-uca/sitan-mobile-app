using Acr.UserDialogs;
using Osma.Mobile.App.Services.Interfaces;
using Plugin.Clipboard;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels
{
    public class VerifyPasswordViewModel : ABaseViewModel
    {
        
        public VerifyPasswordViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService, 
            string[] passwordArray) : 
            base("Confirm Password", userDialogs, navigationService)
        {
            GeneratedPassword = string.Join(string.Empty, passwordArray);
        }

        #region Bindable commands
        public ICommand CopyToClipboard => new Command(() => 
        {
            CrossClipboard.Current.SetText(GeneratedPassword);
            UserDialogs.Instance.Toast($"Copied to Clipboard {GeneratedPassword}", new TimeSpan(3));
        });

        public ICommand ContinueToMainPage => new Command(async () => await NavigationService.NavigateToAsync<MainViewModel>());
        #endregion

        #region Bindable properties
        private string _generatedPassword;
        public string GeneratedPassword
        {
            get => _generatedPassword;
            set => this.RaiseAndSetIfChanged(ref _generatedPassword, value);
        }
        #endregion
    }
}
