using Osma.Mobile.App.ViewModels.Connections;
using System;
using Xamarin.Forms;

namespace Osma.Mobile.App.Views.Connections
{
    public partial class ConnectionPage : ContentPage
    {

        public ConnectionPage()
        {            
            InitializeComponent();
        }

        private void ToggleModalTapped(object sender, EventArgs e)
        {
            //moreModal.IsVisible = !moreModal.IsVisible;
        }

        public void OnListTapped(object sender, ItemTappedEventArgs e)
        {
            chatInput.UnFocusEntry();
        }
       
    }
}
