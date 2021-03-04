using Acr.UserDialogs;
using Osma.Mobile.App.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.PinAuth
{
    public class ConfirmPinAuthViewModel : ABaseViewModel
    {
        private string PinValue { get; set; }

        public ConfirmPinAuthViewModel
            (
                IUserDialogs userDialogs,
                INavigationService navigationService
            ) : base(nameof(ConfirmPinAuthViewModel), userDialogs, navigationService)
        {
            ConfirmAuthPin = (arg) =>
            {
                if (PinValue.Equals(string.Join("", arg)))
                {
                    Preferences.Set(AppConstant.AppPin, PinValue);
                    Preferences.Set(AppConstant.PinAuthEnabled, true);
                    return true;
                }
                return false;
            };
        }

        public override async Task InitializeAsync(object navigationData)
        {
            PinValue = (string) navigationData;
            await base.InitializeAsync(navigationData);
        }

        #region Bindable commands
        public ICommand OnSuccessCommand => new Command(async () => {
            await NavigationService.RemoveLastFromBackStackAsync();
            await NavigationService.NavigateBackAsync();
            });
        #endregion

        #region Bindable properety

        public Func<IList<char>, bool> ConfirmAuthPin { get; }

        #endregion
    }
}
