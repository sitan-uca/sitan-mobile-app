using Acr.UserDialogs;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.ViewModels.Account;
using Osma.Mobile.App.Views;
using Osma.Mobile.App.Views.PinAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.PinAuth
{
    public class PinAuthViewModel : ABaseViewModel
    {

        private string _whoIsCalling { get; set; }
        public PinAuthViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService
            ) : base(nameof(PinAuthViewModel), userDialogs, navigationService)
        {
            ValidatePinFunc = (arg) =>
            {
                return string.Join("",arg).Equals(Preferences.Get(AppConstant.AppPin, null));
            };

            OnSuccessCommand = new Command(async () =>
            {
                if (_whoIsCalling != null) {
                    //When the user disables the pin
                    if (_whoIsCalling.Equals(nameof(AccountViewModel)))
                    {
                        Preferences.Set(AppConstant.PinAuthEnabled, false);
                        Preferences.Set(AppConstant.AppPin, null);
                        await NavigationService.NavigateBackAsync();

                    }
                    //When user wants to change the pin                       
                    else if (_whoIsCalling.Equals(nameof(CreatePinAuthViewModel)))
                    {
                        await NavigationService.NavigateBackAsync();
                        await NavigationService.NavigateToAsync<CreatePinAuthViewModel>();
                    }
                }
                else
                {
                    await NavigationService.NavigateBackAsync();
                }
            });
        }

        public override async Task InitializeAsync(object navigationData)
        {
            if (navigationData != null)
                _whoIsCalling = (string) navigationData;
            
            await base.InitializeAsync(navigationData);
        }

        private async Task HandleBack()
        {
            if (_whoIsCalling != null)
            {
                await NavigationService.NavigateBackAsync();
            }
            else
            {
                //await NavigationService.ExitAppAsync();
            }
            
        }

        #region Bindable command
        public ICommand OnSuccessCommand { get; }

        public ICommand OnErrorCommand { get; }

        public ICommand MyBackPressCommand => new Command(async () => await HandleBack());

        #endregion

        #region Bindable properety

        public Func<IList<char>, bool> ValidatePinFunc { get;  }

        #endregion
    }
}