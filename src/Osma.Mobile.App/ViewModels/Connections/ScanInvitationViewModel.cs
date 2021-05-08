using Acr.UserDialogs;
using Hyperledger.Aries.Features.DidExchange;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Utilities;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class ScanInvitationViewModel : ABaseViewModel
    {
        public ScanInvitationViewModel(IUserDialogs userDialogs,
                                    INavigationService navigationService) :
                                    base("Scan Invite", userDialogs, navigationService)
        {
            ScannerOptions = new ZXing.Mobile.MobileBarcodeScanningOptions{ PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.QR_CODE } };
        }

        public override async Task InitializeAsync(object navigationData)
        {            
            await base.InitializeAsync(navigationData);
        }

        public async Task ScanInvite()
        {
            //var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
            //var opts = new ZXing.Mobile.MobileBarcodeScanningOptions { PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat } };

            //var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            //var result = await scanner.Scan(opts);
            if (Result == null) return;

            ConnectionInvitationMessage invitation;
            //IsAnalyzing = false;
            //IsScanning = false;

            try
            {
                invitation = await MessageDecoder.ParseMessageAsync(Result.Text) as ConnectionInvitationMessage
                    ?? throw new Exception("Unknown message type");
            }
            catch (Exception)
            {
                DialogService.Alert("Invalid invitation!");
                return;
            }   

            Device.BeginInvokeOnMainThread(async () =>
            {
                await NavigationService.NavigateToAsync<AcceptInviteViewModel>(invitation, NavigationType.Modal);                
            });
        }

        #region Bindable commands
        public ICommand ScanInviteCommand => new Command(async () => await ScanInvite());

        //public ICommand PageAppearingCommand => new Command(() => { IsScanning = true; IsAnalyzing = true; });

        //public ICommand PageDisappearingCommand => new Command(() => { IsScanning = false; IsAnalyzing = false; });
        #endregion

        #region Bindable Properties
        private bool _isScanning;
        public bool IsScanning
        {
            get => _isScanning;
            set => this.RaiseAndSetIfChanged(ref _isScanning, value);
        }

        private bool _isAnalyzing;
        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set => this.RaiseAndSetIfChanged(ref _isAnalyzing, value);
        }

        private ZXing.Result _result;
        public ZXing.Result Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }        

        private ZXing.Mobile.MobileBarcodeScanningOptions _scannerOptions;
        public ZXing.Mobile.MobileBarcodeScanningOptions ScannerOptions
        {
            get => _scannerOptions;
            set => this.RaiseAndSetIfChanged(ref _scannerOptions, value);
        }
        #endregion
    }
}
