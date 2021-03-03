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

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class AcceptRequestViewModel : ABaseViewModel
    {
        private readonly IProvisioningService _provisioningService;
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly IAgentProvider _contextProvider;
        private readonly IEventAggregator _eventAggregator;

        private ConnectionResponseMessage _responseMessage;
        private ConnectionRecord _record;

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
                _responseMessage = (ConnectionResponseMessage)dataList[1];
                _record = (ConnectionRecord)dataList[2];
                
                RequestTitle = $"Trust {request.Label}?";
                RequesterUrl = request.ImageUrl;
                RequestContents = $"{request.Label} would like to establish a pairwise DID connection with you. This will allow secure communication between you and {request.Label}.";
            }   
            return base.InitializeAsync(navigationData);
        }

        #region Bindable Commands
        public ICommand AcceptRequestCommand => new Command(async () =>
        {
            var loadingDialog = DialogService.Loading("Processing");
            var context = await _contextProvider.GetContextAsync();

            try
            {
                await _messageService.SendAsync(context.Wallet, _responseMessage, _record);
                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionsUpdated });
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
        #endregion
    }
}
