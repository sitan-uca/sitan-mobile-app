using Acr.UserDialogs;
using Osma.Mobile.App.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class FilterConnectionsPopupViewModel : ABaseViewModel
    {
        public FilterConnectionsPopupViewModel(
            IUserDialogs userDialogs, 
            INavigationService navigationService) : base (
                nameof(FilterConnectionsPopupViewModel), 
                userDialogs, 
                navigationService)
        {

        }

        #region Bindable commands
        
        #endregion
    }
}
