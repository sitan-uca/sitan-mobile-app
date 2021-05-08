using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.Views.Components
{
    public partial class DetailedCell : ViewCell
    {

        // It may be worth it in the future to write custom renderers for each platform as this will give performance benefits

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create("Title", typeof(string), typeof(DetailedCell), "", propertyChanged: TitlePropertyChanged);

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        static void TitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DetailedCell cell = (DetailedCell)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                cell.TitleLabel.Text = newValue?.ToString();
            });
        }

        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create("Subtitle", typeof(string), typeof(DetailedCell), "",
            propertyChanged: SubtitlePropertyChanged);

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        static void SubtitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DetailedCell cell = (DetailedCell)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                cell.SubtitleLabel.Text = newValue.ToString();
                if (string.IsNullOrWhiteSpace(newValue.ToString()))
                {
                    Grid.SetRowSpan(cell.TitleLabel, 2);
                }
                else
                {
                    Grid.SetRowSpan(cell.TitleLabel, 1);
                }
            });
        }


        public static readonly BindableProperty ImageURLProperty =
            BindableProperty.Create("ImageURL", typeof(string), typeof(DetailedCell), "", propertyChanged: ImageURLPropertyChanged);

        public string ImageURL
        {
            get { return (string)GetValue(ImageURLProperty); }
            set { SetValue(ImageURLProperty, value); }
        }

        static void ImageURLPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DetailedCell cell = (DetailedCell)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                cell.Image.Source = newValue?.ToString();
            });
        }

        public static readonly BindableProperty CellImageSourceProperty =
            BindableProperty.Create(nameof(CellImageSource), typeof(ImageSource), typeof(DetailedCell), null, propertyChanged: CellImageSourcePropertyChanged);

        public ImageSource CellImageSource
        {
            get { return (ImageSource)GetValue(CellImageSourceProperty); }
            set { SetValue(CellImageSourceProperty, value); }
        }

        static void CellImageSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DetailedCell cell = (DetailedCell)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                cell.Image.Source = (ImageSource)newValue;
            });
        }

        public static readonly BindableProperty IsNewProperty =
            BindableProperty.Create("IsNew", typeof(bool), typeof(DetailedCell), false,
                propertyChanged: IsNewPropertyChanged);

        public bool IsNew
        {
            get { return (bool)GetValue(IsNewProperty); }
            set { SetValue(IsNewProperty, value); }
        }

        static void IsNewPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DetailedCell cell = (DetailedCell)bindable;
            bool isNew = (bool)newValue;
            Device.BeginInvokeOnMainThread(() =>
            {
                cell.NewLabelContainer.IsVisible = isNew;
                cell.NewLabel.IsVisible = isNew;
                cell.TitleLabel.FontAttributes = isNew ? FontAttributes.Bold : FontAttributes.None;
                cell.View.BackgroundColor = isNew ? Color.FromHex("#f2f7ea") : Color.Transparent;
            });
        }
        
        public static readonly BindableProperty TappedCommandProperty =
           BindableProperty.Create("TappedCommand", typeof(EventHandler), typeof(DetailedCell), null, propertyChanged: TappedCommandPropertyChanged);

        public EventHandler TappedCommand
        {
            get { return (EventHandler)GetValue(TappedCommandProperty); }
            set { SetValue(TappedCommandProperty, value); }
        }

        static void TappedCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DetailedCell cell = (DetailedCell)bindable;
            cell.ParentViewCell.Tapped += (EventHandler)newValue;
            
        }


        public static readonly BindableProperty SwipeDeleteCommandProperty =
            BindableProperty.Create(nameof(SwipeDeleteCommand), typeof(ICommand), typeof(DetailedCell), null, propertyChanged: SwipeDeleteCommandPropertyChanged);

        public ICommand SwipeDeleteCommand
        {
            get { return (ICommand)GetValue(SwipeDeleteCommandProperty); }
            set { SetValue(SwipeDeleteCommandProperty, value); }
        }

        static void SwipeDeleteCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            DetailedCell cell = (DetailedCell)bindable;
            cell.SwipeDelete.Command = (ICommand)newValue;
        }

        public DetailedCell()
        {
            InitializeComponent();
        }       
    }
}
