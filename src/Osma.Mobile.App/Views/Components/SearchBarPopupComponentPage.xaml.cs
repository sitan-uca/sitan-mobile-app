using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Osma.Mobile.App.Views.Components
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchBarPopupComponentPage : PopupPage
    {
        public static readonly BindableProperty SearchTermProperty = BindableProperty.Create(nameof(SearchTermAttr), typeof(string), 
            typeof(SearchBarPopupComponentPage));
        public static readonly BindableProperty PrimaryCommandProperty = BindableProperty.Create(nameof(PrimaryCommand), 
            typeof(ICommand), typeof(SearchBarPopupComponentPage));
        public static readonly BindableProperty SecondaryCommandProperty = BindableProperty.Create(nameof(SecondaryCommand),
            typeof(ICommand), typeof(SearchBarPopupComponentPage));

        public static readonly BindableProperty TextChangedEventProperty = BindableProperty.Create(nameof(OnTextChangedEvent),
            typeof(EventHandler<TextChangedEventArgs>), typeof(SearchBarPopupComponentPage), propertyChanged: TextChangedEventPropertyChanged);

        public string SearchTermAttr
        {
            get => (string)this.GetValue(SearchTermProperty);
            set => this.SetValue(SearchTermProperty, value);
        }

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

        public EventHandler<TextChangedEventArgs> OnTextChangedEvent
        {
            get => (EventHandler<TextChangedEventArgs>)this.GetValue(TextChangedEventProperty);
            set => this.SetValue(TextChangedEventProperty, value);
        }

        static void TextChangedEventPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SearchBarPopupComponentPage page = (SearchBarPopupComponentPage)bindable;
            page.searchBar.TextChanged += (EventHandler<TextChangedEventArgs>)newValue;            
        }

        public SearchBarPopupComponentPage()
        {
            InitializeComponent();
            searchBar.Focus();
        }      
    }
}