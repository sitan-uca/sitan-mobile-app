using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Decorators.Attachments;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Models.Events;
using Hyperledger.Aries.Storage;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Extensions;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Utilities;
using ReactiveUI;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Proofs
{
    public class ProofRequestsViewModel : ABaseViewModel
    {
        private readonly IConnectionService _connectionService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IProofService _proofService;
        private readonly IEventAggregator eventAggregator;
        private readonly IProvisioningService _provisioningService;
        private readonly ILifetimeScope _scope;        

        public ProofRequestsViewModel(IUserDialogs userDialogs,
                                   INavigationService navigationService,
                                   IAgentProvider agentContextProvider,
                                   IProofService proofService,
                                   IConnectionService connectionService,
                                   IProvisioningService provisioningService,
                                   IEventAggregator eventAggregator,
                                   ILifetimeScope scope) : base("Proofs", userDialogs, navigationService)
        {
            _connectionService = connectionService;
            _agentContextProvider = agentContextProvider;
            _proofService = proofService;
            _provisioningService = provisioningService;
            this.eventAggregator = eventAggregator;
            _scope = scope;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await RefreshProofRequests();

            eventAggregator.GetEventByType<ApplicationEvent>()
                          .Where(_ => _.Type == ApplicationEventType.RefreshProofRequests)
                          .Subscribe(async _ => await RefreshProofRequests());

            eventAggregator.GetEventByType<ServiceMessageProcessingEvent>()
               .Where
               (_ =>
                   _.MessageType == MessageTypes.PresentProofNames.RequestPresentation                   
               )
               .Subscribe(async _ => await NotifyAndRefresh(_));

            await base.InitializeAsync(navigationData);
        }

        private async Task NotifyAndRefresh(ServiceMessageProcessingEvent _event)
        {
            await RefreshProofRequests();
            switch (_event.MessageType)
            {
                case MessageTypes.PresentProofNames.RequestPresentation:
                    NotificationService.TriggerNotification("Presentation request", "New presentation request received");
                    break;
            }
        }

        public async Task RefreshProofRequests(string tab = null)
        {
            try
            {
                RefreshingProofRequests = true;
                ProofRequests.Clear();

                var agentContext = await _agentContextProvider.GetContextAsync();
                IEnumerable<ProofRecord> proofRequests = null;
                if (tab == null || tab.Equals(nameof(ProofState.Requested)))
                    proofRequests = await _proofService.ListRequestedAsync(agentContext);
                else if (tab.Equals(nameof(ProofState.Accepted)))
                    proofRequests = await _proofService.ListAcceptedAsync(agentContext);
                else if (tab.Equals(nameof(ProofState.Proposed)))
                    proofRequests = await _proofService.ListProposedAsync(agentContext);

                IList<ProofRequestViewModel> proofRequestVms = new List<ProofRequestViewModel>();
                foreach (var proofReq in proofRequests)
                {
                    var connection = (proofReq.ConnectionId == null) ? null : await _connectionService.GetAsync(agentContext, proofReq.ConnectionId);
                    var proofRequestViewModel = _scope.Resolve<ProofRequestViewModel>(new NamedParameter("proofRecord", proofReq), new NamedParameter("connection", connection));
                    proofRequestVms.Add(proofRequestViewModel);                                        
                }

                ProofRequests.InsertRange(proofRequestVms);
                HasRequests = ProofRequests.Any();
            }
            catch (Exception xx)
            {
                await UserDialogs.Instance.AlertAsync(xx.Message);
                Debug.WriteLine(xx.StackTrace);
            }
            finally
            {
                RefreshingProofRequests = false;
            }
        }

        public async Task ScanProofRequest()
        {
            var expectedFormat = ZXing.BarcodeFormat.QR_CODE;
            var opts = new ZXing.Mobile.MobileBarcodeScanningOptions { PossibleFormats = new List<ZXing.BarcodeFormat> { expectedFormat } };

            var scanner = new ZXing.Mobile.MobileBarcodeScanner();

            var result = await scanner.Scan(opts);
            if (result == null)
                return;

            RequestPresentationMessage presentationMessage;

            try
            {
                presentationMessage = await MessageDecoder.ParseMessageAsync(result.Text) as RequestPresentationMessage
                    ?? throw new Exception("Unknown message type");
            }
            catch (Exception)
            {
                DialogService.Alert("Invalid Proof Request!");
                return;
            }

            if (presentationMessage == null)
                return;

            try
            {
                var request = presentationMessage.Requests?.FirstOrDefault((Attachment x) => x.Id == "libindy-request-presentation-0");
                if (request == null)
                {
                    DialogService.Alert("scanned qr code does not look like a proof request", "Error");
                    return;
                }
                var proofRequest = request.Data.Base64.GetBytesFromBase64().GetUTF8String().ToObject<ProofRequest>();
                if (proofRequest == null)
                    return;

                var proofRequestViewModel = _scope.Resolve<ProofRequestViewModel>(new NamedParameter("proofRequest", proofRequest),
                                                                                  new NamedParameter("requestPresentationMessage", presentationMessage));

                await NavigationService.NavigateToAsync(proofRequestViewModel);
            }
            catch (Exception xx)
            {
                DialogService.Alert(xx.Message);
            }
        }

        public async Task CreateRequestPresentationBarcode()
        {
            var context = await _agentContextProvider.GetContextAsync();
            var provisionedRecord = await _provisioningService.GetProvisioningAsync(context.Wallet);
            //var (requestMsg, _) = await _proofService.CreateRequestAsync(context, new ProofRequest { });

            //string barcodeValue = provisionedRecord.Endpoint.Uri + "?d_m=" + Uri.EscapeDataString(requestMsg.ToByteArray().ToBase64String());
        }

        #region Bindable Command

        public ICommand ScanProofRequestCommand => new Command(async () => await ScanProofRequest());

        public ICommand CreateRequestBarcodeCommand => new Command(async () => await CreateRequestPresentationBarcode());

        public ICommand SelectProofRequestCommand => new Command<ProofRequestViewModel>(async (proofRequest) =>
        {
            if (proofRequest == null)
                return;

            await NavigationService.NavigateToAsync(proofRequest);
        });

        public ICommand RefreshCommand => new Command(async () => await RefreshProofRequests());

        public ICommand ProofsSubTabChange => new Command<object>(async (tab) =>
        {
            await RefreshProofRequests(tab.ToString());
        });

        #endregion

        #region Bindable Properties

        private RangeEnabledObservableCollection<ProofRequestViewModel> _proofRequests = new RangeEnabledObservableCollection<ProofRequestViewModel>();
        public RangeEnabledObservableCollection<ProofRequestViewModel> ProofRequests
        {
            get => _proofRequests;
            set => this.RaiseAndSetIfChanged(ref _proofRequests, value);
        }

        private bool _hasRequests;
        public bool HasRequests
        {
            get => _hasRequests;
            set => this.RaiseAndSetIfChanged(ref _hasRequests, value);
        }

        private bool _refreshingProofRequests;
        public bool RefreshingProofRequests
        {
            get => _refreshingProofRequests;
            set => this.RaiseAndSetIfChanged(ref _refreshingProofRequests, value);
        }
        #endregion

    }
}
