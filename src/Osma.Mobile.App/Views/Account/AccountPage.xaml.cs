using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.ViewModels.Account;
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Osma.Mobile.App.Views.Account
{
    public partial class AccountPage : ContentPage
    {
        public AccountPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            var vm = (AccountViewModel)BindingContext;
            vm.NotifySwitchToggle.Execute(null);
        }       
    }
}
