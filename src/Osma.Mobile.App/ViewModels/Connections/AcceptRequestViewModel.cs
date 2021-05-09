using System;
using System.Collections.Generic;
using System.Text;
using Osma.Mobile.App.Services.Interfaces;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Acr.UserDialogs;
using System.Threading.Tasks;
using ReactiveUI;
using System.Windows.Input;
using Xamarin.Forms;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Converters;
using Hyperledger.Aries.Decorators.Attachments;
using Osma.Mobile.App.Protocols;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class AcceptRequestViewModel : ABaseViewModel
    {
        private readonly IProvisioningService _provisioningService;
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly IAgentProvider _contextProvider;
        private readonly IEventAggregator _eventAggregator;

        //private ConnectionResponseMessage _responseMessage;
        //private ConnectionRecord _record;
        private string _connectionId;

        public AcceptRequestViewModel(IUserDialogs userDialogs,
                                     INavigationService navigationService,
                                     IProvisioningService provisioningService,
                                     IConnectionService connectionService,
                                     IMessageService messageService,
                                     IAgentProvider contextProvider,
                                     IEventAggregator eventAggregator)
                                     : base(nameof(AcceptRequestViewModel), userDialogs, navigationService)
        {
            _provisioningService = provisioningService;
            _connectionService = connectionService;
            _contextProvider = contextProvider;
            _messageService = messageService;
            _contextProvider = contextProvider;
            _eventAggregator = eventAggregator;
        }

        public override Task InitializeAsync(object navigationData)
        {
            
            if (navigationData is List<object> dataList)
            {
                var request = (ConnectionRequestMessage)dataList[0];
                _connectionId = (string)dataList[1];
                //_responseMessage = (ConnectionResponseMessage)dataList[1];
                //_record = (ConnectionRecord)dataList[2];
                
                RequestTitle = $"Trust {request.Label}?";
                RequesterUrl = request.ImageUrl;
                RequesterImageSource = Base64StringToImageSource.Base64StringToImage(request.ImageUrl);
                RequestContents = $"{request.Label} would like to establish a pairwise DID connection with you. This will allow secure communication between you and {request.Label}.";
            }   
            return base.InitializeAsync(navigationData);
        }

        #region Bindable Commands
        public ICommand AcceptRequestCommand => new Command(async () =>
        {
            var loadingDialog = DialogService.Loading("Processing");
            var context = await _contextProvider.GetContextAsync();
            var provisioning = await _provisioningService.GetProvisioningAsync(context.Wallet);
            try
            {
                var (message, record) = await _connectionService.CreateResponseAsync(context, _connectionId);
                //messageContext.ContextRecord = record;            
                
                message.AddAttachment(
                    new Attachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        MimeType = ConnectionMimeTypes.TextMimeType,
                        Nickname = "agent-profile-pic",
                        Data = new AttachmentContent
                        {
                            Base64 = provisioning.Owner.ImageUrl
                        }                       
                    });

                await _messageService.SendAsync(context, message, record);          
                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionsUpdated });
            } 
            catch (Exception ex)
            {
                DialogService.Alert(ex.Message);
            }
            finally
            {
                loadingDialog.Hide();
                await NavigationService.PopModalAsync();
            }
        });

        public ICommand RejectRequestCommand => new Command(async () => await NavigationService.PopModalAsync());

        #endregion

        #region Bindable Properties
        private string _requestTitle;
        public string RequestTitle
        {
            get => _requestTitle;
            set => this.RaiseAndSetIfChanged(ref _requestTitle, value);
        }

        private string _requestContents = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua";
        public string RequestContents
        {
            get => _requestContents;
            set => this.RaiseAndSetIfChanged(ref _requestContents, value);
        }

        private string _requesterUrl;
        public string RequesterUrl
        {
            get => _requesterUrl;
            set => this.RaiseAndSetIfChanged(ref _requesterUrl, value);
        }

        private ImageSource _requesterImageSource;
        public ImageSource RequesterImageSource
        {
            get => _requesterImageSource;
            set => this.RaiseAndSetIfChanged(ref _requesterImageSource, value);
        }
        #endregion
    }
}
