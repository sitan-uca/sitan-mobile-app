using Osma.Mobile.App.ViewModels.Connections;
using Osma.Mobile.App.ViewModels.ScanQrCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace Osma.Mobile.App.Views.ScanQrCode
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScanQrCodePage : ContentPage
    {
        ScanQrCodeViewModel _vm;
        ZXingScannerView Scanner;

        public ScanQrCodePage()
        {
            InitializeComponent();
            //_vm = BindingContext as ScanInvitationViewModel;
        }

        private void ContentPage_Disappearing(object sender, EventArgs e)
        {
            //ScannerView.IsScanning = false;
            //ScannerView.IsAnalyzing = false;
            //_vm = BindingContext as ScanInvitationViewModel;
            //_vm?.PageDisappearingCommand.Execute(null);

            if (Scanner != null)
            {
                Scanner.IsAnalyzing = false;
            }
            base.OnDisappearing();
        }

        private void ContentPage_Appearing(object sender, EventArgs e)
        {
            //ScannerView.IsScanning = true;
            //ScannerView.IsAnalyzing = true;
            _vm = BindingContext as ScanQrCodeViewModel;
            //_vm?.PageAppearingCommand.Execute(null);

            CreateScannerView("QrCodeScanner", new List<BarcodeFormat> { BarcodeFormat.QR_CODE }, ZXingScannerView_OnScanResult);
            if (Scanner != null)
            {
                Scanner.IsAnalyzing = true;
            }
            base.OnAppearing();
        }

        private void ZXingScannerView_OnScanResult(Result result)
        {
            Scanner.IsAnalyzing = false;
            _vm.Result = result;
            _vm.ScanInviteCommand.Execute(null);
        }

        private void CreateScannerView(string wrapperName, IEnumerable<BarcodeFormat> barcodeFormats, Action<Result> scanResultAction)
        {
            var wrapper = this.FindByName<Frame>(wrapperName);

            if (wrapper == null)
            {
                return;
            }

            if (wrapper.Content == null)
            {
                var scannerOptions = new MobileBarcodeScanningOptions()
                {
                    PossibleFormats = new List<BarcodeFormat>()
                };

                foreach (var format in barcodeFormats)
                {
                    scannerOptions.PossibleFormats.Add(format);
                }

                Scanner = new ZXingScannerView()
                {
                    WidthRequest = 200,
                    HeightRequest = 200,
                    Options = scannerOptions
                };                

                Scanner.OnScanResult += scanResultAction.Invoke;
                Scanner.IsScanning = true;

                ZXingDefaultOverlay overlay = new ZXingDefaultOverlay();

                Grid grid = new Grid() { VerticalOptions = LayoutOptions.FillAndExpand };
                grid.Children.Add(Scanner);
                grid.Children.Add(overlay);

                wrapper.Content = grid;
            }
        }
    }
}