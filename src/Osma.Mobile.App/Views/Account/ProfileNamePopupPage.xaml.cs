using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Osma.Mobile.App.Views.Account
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfileNamePopupPage : PopupPage
    {
        public ProfileNamePopupPage()
        {
            InitializeComponent();
        }

        public void CancelClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync(true);
        }
    }
}