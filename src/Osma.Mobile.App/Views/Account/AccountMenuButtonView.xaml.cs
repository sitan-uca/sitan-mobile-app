using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.Views.Account
{
    public partial class AccountMenuButtonView : ViewCell
    {
        public AccountMenuButtonView()
        {
            InitializeComponent();
        }


        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create("Title", typeof(string), typeof(AccountMenuButtonView), "", propertyChanged: TitlePropertyChanged);

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }

        }

        static void TitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AccountMenuButtonView view = (AccountMenuButtonView)bindable;
            view.TitleLabel.Text = newValue.ToString();
        }

        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create("Subtitle", typeof(string), typeof(AccountMenuButtonView), "", propertyChanged: SubtitlePropertyChanged);

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        static void SubtitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AccountMenuButtonView view = (AccountMenuButtonView)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                view.SubtitleLabel.Text = newValue.ToString();
            });
        }

        public static readonly BindableProperty ImageProperty =
            BindableProperty.Create("Image", typeof(string), typeof(AccountMenuButtonView), "", propertyChanged: ImagePropertyChanged);

        public string Image
        {
            get { return (string)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        static void ImagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AccountMenuButtonView view = (AccountMenuButtonView)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                view.IconImage.Source = newValue.ToString();
            });
        }


        public static readonly BindableProperty TappedCommandProperty =
            BindableProperty.Create("TappedCommand", typeof(ICommand), typeof(AccountMenuButtonView), null, propertyChanged: TappedCommandPropertyChanged);

        public ICommand TappedCommand
        {
            get { return (ICommand)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        static void TappedCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AccountMenuButtonView view = (AccountMenuButtonView)bindable;
            view.TapGestureRecognizer.Command = (ICommand)newValue;
        }


        public static readonly BindableProperty NumberOfTapsRequiredProperty =
            BindableProperty.Create("NumberOfTapsRequired", typeof(Int32), typeof(AccountMenuButtonView), 1, propertyChanged: NumberOfTapsRequiredPropertyChanged);

        public Int32 NumberOfTapsRequired
        {
            get { return (Int32)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        static void NumberOfTapsRequiredPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AccountMenuButtonView view = (AccountMenuButtonView)bindable;
            view.TapGestureRecognizer.NumberOfTapsRequired = (Int32)newValue;
        }

        public static readonly BindableProperty SwitchToggledEventProperty =
            BindableProperty.Create("SwitchToggledEvent", typeof(EventHandler<ToggledEventArgs>), typeof(AccountMenuButtonView), null, propertyChanged: SwitchToggledEventPropertyChanged);

        public EventHandler<ToggledEventArgs> SwitchToggledEvent
        {
            get { return (EventHandler<ToggledEventArgs>)GetValue(SwitchToggledEventProperty); }
            set { SetValue(SwitchToggledEventProperty, value); }
        }
        
        static void SwitchToggledEventPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AccountMenuButtonView view = (AccountMenuButtonView)bindable;
            view.SwitchToggle.Toggled += (EventHandler<ToggledEventArgs>)newValue;
        }

        public static readonly BindableProperty SwitchToggledProperty =
            BindableProperty.Create("SwitchToggled", typeof(bool), typeof(AccountMenuButtonView), false, propertyChanged: SwitchToggledPropertyChanged);

        public bool SwitchToggled
        {
            get { return (bool)GetValue(SwitchToggledProperty); }
            set { SetValue(SwitchToggledProperty, value); }
        }

        static void SwitchToggledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AccountMenuButtonView view = (AccountMenuButtonView)bindable;
            view.SwitchToggle.IsToggled = (bool)newValue;
        }

        public static readonly BindableProperty SwitchToggleVisibleProperty =
            BindableProperty.Create("SwitchToggleVisible", typeof(bool), typeof(AccountMenuButtonView), false, propertyChanged: SwitchToggleVisiblePropertyChanged);

        public bool SwitchToggleVisible
        {
            get { return (bool)GetValue(SwitchToggleVisibleProperty); }
            set { SetValue(SwitchToggleVisibleProperty, value); }
        }

        static void SwitchToggleVisiblePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            AccountMenuButtonView view = (AccountMenuButtonView)bindable;
            view.SwitchToggle.IsVisible = (bool)newValue;
        }
    }
}
