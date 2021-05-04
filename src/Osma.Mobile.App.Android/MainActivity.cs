using System;
using System.Collections.Generic;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using FFImageLoading.Forms.Platform;
using FormsPinView.Droid;
using Java.Lang;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Osma.Mobile.App.Services.Interfaces;
using Xamarin.Forms;

namespace Osma.Mobile.App.Droid
{
    [Activity(Label = "Sitan", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public partial/*GORILLA*/class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            Forms.SetFlags("IndicatorView_Experimental");
            Forms.Init(this, bundle);
            
            PinItemViewRenderer.Init();

            Acr.UserDialogs.UserDialogs.Init(this);
            // Initializing FFImageLoading
            CachedImageRenderer.Init(false);
            Rg.Plugins.Popup.Popup.Init(this, bundle);
            // Initializing Xamarin Essentials
            Xamarin.Essentials.Platform.Init(this, bundle);

            

            // Initializing QR Code Scanning support
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);

#if GORILLA
            LoadApplication(UXDivers.Gorilla.Droid.Player.CreateApplication(
                this,
                new UXDivers.Gorilla.Config("Good Gorilla")
                    .RegisterAssemblyFromType<Converters.InverseBooleanConverter>()
                    .RegisterAssemblyFromType<FFImageLoading.Transformations.CircleTransformation>()
                    .RegisterAssemblyFromType<FFImageLoading.Forms.CachedImage>()
                ));
#else

            // Initializing User Dialogs
            // Android requires that we set content root.
            var host = App.BuildHost(typeof(PlatformModule).Assembly)
                .UseContentRoot(System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal)).Build();     

            LoadApplication(host.Services.GetRequiredService<App>());
            Window.SetStatusBarColor(Android.Graphics.Color.Argb(255, 0, 0, 0));
#endif

            //Loading dependent libindy
            JavaSystem.LoadLibrary("c++_shared");
            JavaSystem.LoadLibrary("indy");
            CreateNotificationFromIntent(Intent);
            CheckAndRequestRequiredPermissions();
        }

        readonly string[] _permissionsRequired =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        private int _requestCode = -1;
        private List<string> _permissionsToBeGranted = new List<string>();

        private void CheckAndRequestRequiredPermissions()
        {
            for (int i = 0; i < _permissionsRequired.Length; i++)
                if (CheckSelfPermission(_permissionsRequired[i]) != (int)Permission.Granted)
                    _permissionsToBeGranted.Add(_permissionsRequired[i]);

            if (_permissionsToBeGranted.Any())
            {
                _requestCode = 10;
                RequestPermissions(_permissionsRequired.ToArray(), _requestCode);
            }
            else
                System.Diagnostics.Debug.WriteLine("Device already has all the required permissions");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            if (grantResults.Length == _permissionsToBeGranted.Count)
                System.Diagnostics.Debug.WriteLine("All permissions required that werent granted, have now been granted");
            else
                System.Diagnostics.Debug.WriteLine("Some permissions requested were denied by the user");
           
           Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
           base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override void OnBackPressed()
        {
            Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }

        protected override void OnNewIntent(Intent intent)
        {
            CreateNotificationFromIntent(intent);
        }

        void CreateNotificationFromIntent(Intent intent)
        {
            if (intent?.Extras != null)
            {
                string title = intent.GetStringExtra(AndroidNotificationManager.TitleKey);
                string message = intent.GetStringExtra(AndroidNotificationManager.MessageKey);
                DependencyService.Get<INotificationManager>().ReceiveNotification(title, message);
            }
        }
    }
}

