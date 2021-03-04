using Acr.UserDialogs;
using Osma.Mobile.App.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.PinAuth
{
    public class CreatePinAuthViewModel : ABaseViewModel
    {
        private string PinValue { get; set; }

        public CreatePinAuthViewModel(
            IUserDialogs userDialogs, 
            INavigationService navigationService
            ) : base(nameof(CreatePinAuthViewModel), userDialogs, navigationService)
        {
            EnterAuthPin = (arg) =>
            {
                PinValue = string.Join("", arg);
                return true;
            };

            //ConfirmAuthPin = (arg) =>
            //{
            //    //Preferences.Set(AppConstant.AppPin, arg.ToString());
            //    if (PinValue == arg.ToString())
            //    {
            //        Preferences.Set(AppConstant.AppPin, arg.ToString());
            //        return true;
            //    }
            //    return false;
            //};
        }

        public override async Task InitializeAsync(object navigationData)
        {
            PinValue = string.Empty;
            await base.InitializeAsync(navigationData);
        }

        #region Bindable commands
        public ICommand OnSuccessCommand => new Command(async () => await NavigationService.NavigateToAsync<ConfirmPinAuthViewModel>(PinValue));
        #endregion

        #region Bindable properety
        public Func<IList<char>, bool> EnterAuthPin { get; }
        #endregion
    }
}
