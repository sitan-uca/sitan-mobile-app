using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Decorators.Attachments;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.PresentProof;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Utilities;
using Osma.Mobile.App.ViewModels.Connections;
using Osma.Mobile.App.ViewModels.Proofs;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.ScanQrCode
{
    public class ScanQrCodeViewModel : ABaseViewModel
    {
        private readonly ILifetimeScope _scope;

        public ScanQrCodeViewModel(IUserDialogs userDialogs,
                                   INavigationService navigationService,
                                   ILifetimeScope scope) :
                                   base("Scan", 
                                       userDialogs, 
                                       navigationService)
        {
            _scope = scope;            
        }

        public override async Task InitializeAsync(object navigationData)
        {            
            await base.InitializeAsync(navigationData);
        }

        public async Task ScanInvite()
        {            
            if (Result == null) return;

            try
            {
                var msg = await MessageDecoder.ParseMessageAsync(Result.Text) ?? throw new Exception("Unknown message type");
                if (msg.Type == MessageTypes.ConnectionInvitation) 
                {
                    var invitation = msg as ConnectionInvitationMessage;
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await NavigationService.NavigateToAsync<AcceptInviteViewModel>(invitation, NavigationType.Modal);
                    });
                }
                else if(msg.Type == MessageTypes.PresentProofNames.RequestPresentation)
                {
                    var requestMessage = msg as RequestPresentationMessage;

                    var request = requestMessage.Requests?.FirstOrDefault((Attachment x) => x.Id == "libindy-request-presentation-0");
                    if (request == null)
                    {
                        DialogService.Alert("scanned qr code does not look like a proof request", "Error");
                        return;
                    }
                    var proofRequest = request.Data.Base64.GetBytesFromBase64().GetUTF8String().ToObject<ProofRequest>();
                    if (proofRequest == null)
                        return;

                    var proofRequestViewModel = _scope.Resolve<ProofRequestViewModel>(new NamedParameter("proofRequest", proofRequest),
                                                                                      new NamedParameter("requestPresentationMessage", requestMessage));

                    await NavigationService.NavigateToAsync(proofRequestViewModel);
                }
            }
            catch (Exception)
            {
                DialogService.Alert("Invalid invitation!");
                return;
            }
        }

        #region Bindable commands

        public ICommand ScanInviteCommand => new Command(async () => await ScanInvite());

        #endregion

        #region Bindable Properties        

        private ZXing.Result _result;
        public ZXing.Result Result
        {
            get => _result;
            set => this.RaiseAndSetIfChanged(ref _result, value);
        }        
       
        #endregion
    }
}
