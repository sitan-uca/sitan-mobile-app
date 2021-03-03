using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.DidExchange;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.ViewModels.Connections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Osma.Mobile.App.Services
{
    class WalletEventService
    {
        protected static INavigationService _navigationService;
        
        public static void Init(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void ShowAcceptRequestDialog(List<object> data)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await _navigationService.NavigateToAsync<AcceptRequestViewModel>(data, NavigationType.Modal);
            });
        }
    }
}
