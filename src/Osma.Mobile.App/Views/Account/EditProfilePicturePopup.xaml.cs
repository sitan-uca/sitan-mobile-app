using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Osma.Mobile.App.Views.Account
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditProfilePicturePopup : PopupPage
    {
        public EditProfilePicturePopup()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty FromGalleryCommandProperty = BindableProperty.Create(nameof(FromGalleryCommand), typeof(ICommand), typeof(EditProfilePicturePopup));

        public ICommand FromGalleryCommand
        {
            get => (ICommand)this.GetValue(FromGalleryCommandProperty);
            set => this.SetValue(FromGalleryCommandProperty, value);
        }

        public static readonly BindableProperty DeletePictureCommandProperty = BindableProperty.Create(nameof(DeletePictureCommand), typeof(ICommand), typeof(EditProfilePicturePopup));

        public ICommand DeletePictureCommand
        {
            get => (ICommand)this.GetValue(DeletePictureCommandProperty);
            set => this.SetValue(DeletePictureCommandProperty, value);
        }
    }
}