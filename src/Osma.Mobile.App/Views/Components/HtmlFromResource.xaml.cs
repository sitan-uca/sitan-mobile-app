using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace Osma.Mobile.App.Views.Components
{
    public partial class HtmlFromResource : ContentView
    {
        public HtmlFromResource()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty FileNameProperty =
            BindableProperty.Create("FileName", typeof(string), typeof(DetailedCell), "", propertyChanged: FileNamePropertyChanged);


        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        static void FileNamePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            HtmlFromResource view = (HtmlFromResource)bindable;
            Device.BeginInvokeOnMainThread(() =>
            {
                var source = new HtmlWebViewSource();
                string url = DependencyService.Get<IBaseUrl>().Get();
                string TempUrl = string.Empty;
                if (Device.RuntimePlatform == Device.Android)
                    //TODO Fix the problem (Could not find a part of the path)
                    TempUrl = Path.Combine(url, "Resources", "legal.html");
                    
                else if (Device.RuntimePlatform == Device.iOS)
                    TempUrl = Path.Combine(url, "Resources", "legal");

                //var p = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                
                source.BaseUrl = url;
                string html;
                try
                {
                    //using (var sr = new StreamReader(new Uri(TempUrl).LocalPath))
                    using(var sr = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Osma.Mobile.App.Resources.legal.html")))
                    {
                        html = sr.ReadToEnd();
                        source.Html = html;
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
                view.webview.Source = source;
            });
        }
    }
}
