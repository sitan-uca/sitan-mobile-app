using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Osma.Mobile.App.Views.Connections
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilterConnectionsPopupPage : PopupPage
    {
        public FilterConnectionsPopupPage()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty PrimaryCommandProperty = BindableProperty.Create(nameof(PrimaryCommand), typeof(ICommand), typeof(FilterConnectionsPopupPage));
        public static readonly BindableProperty SecondaryCommandProperty = BindableProperty.Create(nameof(SecondaryCommand), typeof(ICommand), typeof(FilterConnectionsPopupPage));
        public static readonly BindableProperty FilterValueProperty = BindableProperty.Create(nameof(FilterValue), typeof(string), typeof(FilterConnectionsPopupPage));


        public ICommand PrimaryCommand
        {
            get => (ICommand)this.GetValue(PrimaryCommandProperty);
            set => this.SetValue(PrimaryCommandProperty, value);
        }
        
        public ICommand SecondaryCommand
        {
            get => (ICommand)this.GetValue(SecondaryCommandProperty);
            set => this.SetValue(SecondaryCommandProperty, value);
        }

        public string FilterValue
        {
            get => (string)this.GetValue(FilterValueProperty);
            set => this.SetValue(FilterValueProperty, value);
        }

        //public void CancelClicked(object sender, EventArgs e)
        //{
        //    PopupNavigation.Instance.PopAsync(true);
        //}
    }
}