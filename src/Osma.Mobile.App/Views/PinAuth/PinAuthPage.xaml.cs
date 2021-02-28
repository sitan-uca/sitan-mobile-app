using Osma.Mobile.App.ViewModels;
using Osma.Mobile.App.ViewModels.PinAuth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Osma.Mobile.App.Views.PinAuth
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PinAuthPage : ContentPage
    {
        public PinAuthPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override bool OnBackButtonPressed() 
        {
            var vm = (PinAuthViewModel)BindingContext;
            vm.MyBackPressCommand.Execute(null); // You can add parameters if any
            return false;
        }

        public void Handle_Success(object sender, EventArgs e)
        {
            //Navigation.PopAsync();
            Debug.WriteLine("Auth Success");
        }
    }
}