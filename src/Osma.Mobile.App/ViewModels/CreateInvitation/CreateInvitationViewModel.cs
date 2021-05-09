using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.DidExchange;
using Osma.Mobile.App.Services.Interfaces;
using Plugin.Clipboard;
using ReactiveUI;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Osma.Mobile.App.ViewModels.CreateInvitation
{
    public class CreateInvitationViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentContextProvider;
        private readonly IConnectionService _connectionService;

        public CreateInvitationViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            IAgentProvider agentContextProvider,
            IConnectionService defaultConnectionService
            ) : base(
                "CreateInvitation",
                userDialogs,
                navigationService
           )
        {
            _agentContextProvider = agentContextProvider;
            _connectionService = defaultConnectionService;
            HasQrCodeValue = !string.IsNullOrEmpty(QrCodeValue);
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await base.InitializeAsync(navigationData);
        }

        private async Task CreateInvitation()
        {
            IsBusy = true;
            try
            {
                var context = await _agentContextProvider.GetContextAsync();
                var (invitation, _) = await _connectionService.CreateInvitationAsync(context);
                // null because the base64 string of the image is too large to convert to qrcode, 
                // so we don't send the image in this step
                invitation.ImageUrl = null;
                string barcodeValue = invitation.ServiceEndpoint + "?c_i=" + Uri.EscapeDataString(invitation.ToByteArray().ToBase64String());
                string linkValue = invitation.ServiceEndpoint + "?c_i=" + invitation.ToJson().ToBase64();
                QrCodeValue = barcodeValue;
                LinkValue = linkValue;
                HasQrCodeValue = !string.IsNullOrEmpty(QrCodeValue);
            }
            catch (Exception ex)
            {
                DialogService.Alert(ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private ZXingBarcodeImageView QRCodeGenerator(string barcodeValue)
        {
            var barcode = new ZXingBarcodeImageView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                AutomationId = "zxingBarcodeImageView",
            };

            barcode.BarcodeFormat = ZXing.BarcodeFormat.QR_CODE;
            barcode.BarcodeOptions.Width = 300;
            barcode.BarcodeOptions.Height = 300;
            barcode.BarcodeOptions.Margin = 10;
            barcode.BarcodeValue = barcodeValue;

            return barcode;

        }

        #region Bindable Command

        public ICommand CreateInvitationCommand => new Command(async () => await CreateInvitation());

        public ICommand CopyInvitation => new Command(() => {
            if (!string.IsNullOrEmpty(LinkValue))
            {
                CrossClipboard.Current.SetText(LinkValue);
                UserDialogs.Instance.Toast($"Copied to Clipboard {LinkValue}", new TimeSpan(3));
            }            
        });

        public ICommand NavigateBackCommand => new Command(async () => await NavigationService.PopModalAsync());

        #endregion

        #region Bindable Properties

        private string _qrCodeValue;
        public string QrCodeValue
        {
            get => _qrCodeValue;
            set => this.RaiseAndSetIfChanged(ref _qrCodeValue, value);
        }

        private string _linkValue;
        public string LinkValue
        {
            get => _linkValue;
            set => this.RaiseAndSetIfChanged(ref _linkValue, value);
        }

        private bool _hasQrCodeValue;
        public bool HasQrCodeValue
        {
            get => _hasQrCodeValue;
            set => this.RaiseAndSetIfChanged(ref _hasQrCodeValue, value);
        }

        #endregion
    }
}
