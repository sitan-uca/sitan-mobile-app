using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Storage;
using Osma.Mobile.App.Converters;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Views.Account;
using Plugin.Media;
using Plugin.Media.Abstractions;
using ReactiveUI;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Account
{
    public class ProfileViewModel : ABaseViewModel
    {
        private readonly ILifetimeScope _scope;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IProvisioningService _provisioningService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWalletRecordService _walletRecordService;
        private ProvisioningRecord _provisioningRecord;

        public ProfileViewModel
            (
                IAgentProvider agentContextProvider,
                IProvisioningService provisioningService,
                IWalletRecordService walletRecordService,
                ILifetimeScope scope,
                IEventAggregator eventAggregator,
                IUserDialogs userDialogs,
                INavigationService navigationService                
            ) : base
            (
                nameof(ProfileViewModel),
                userDialogs,
                navigationService
            )
        {
            _scope = scope;
            _agentContextProvider = agentContextProvider;
            _provisioningService = provisioningService;
            _eventAggregator = eventAggregator;
            _walletRecordService = walletRecordService;
        }

        public override async Task InitializeAsync(object navigationData)
        {
            
            _eventAggregator.GetEventByType<ApplicationEvent>()
                            .Where(_ => _.Type == ApplicationEventType.ProvisioningRecordUpdated)
                            .Subscribe(async _ => await InitializeAgentInfo());
            
            await InitializeAgentInfo();

            await base.InitializeAsync(navigationData);
        }

        public async Task InitializeAgentInfo()
        {
            var context = await _agentContextProvider.GetContextAsync();
            _provisioningRecord = await _provisioningService.GetProvisioningAsync(context.Wallet);
            AgentName = _provisioningRecord.Owner.Name;
            AgentImageSource = Base64StringToImageSource.Base64StringToImage(_provisioningRecord.Owner.ImageUrl);
            MediatorEndpointUrl = _provisioningRecord.Endpoint.Uri;
            if (Preferences.ContainsKey(AppConstant.MediatorConnectionIdTagName))
            {
                var mediatorConnection = await _walletRecordService.GetAsync<ConnectionRecord>(context.Wallet, Preferences.Get(AppConstant.MediatorConnectionIdTagName, string.Empty));
                MediatorConnectionState = mediatorConnection.State.ToString();
            }            
        }

        public async Task SelectPictureFromGallery()
        {
            var context = await _agentContextProvider.GetContextAsync();

            await CrossMedia.Current.Initialize();
            if(!CrossMedia.Current.IsPickPhotoSupported)
            {
                await UserDialogs.Instance.AlertAsync("Not Supported", "Your device does not currently support this functionality", "Ok");
                return;
            }

            var mediaOptions = new PickMediaOptions { PhotoSize = PhotoSize.MaxWidthHeight, CompressionQuality=50, MaxWidthHeight=200 };
            var selectedImageFile = await CrossMedia.Current.PickPhotoAsync(mediaOptions);

            //AgentImageSource = ImageSource.FromStream(() => selectedImageFile.GetStream());            

            if (selectedImageFile != null)
            {
                _provisioningRecord.Owner.ImageUrl = ImageToBase64(selectedImageFile.Path);
                await _walletRecordService.UpdateAsync(context.Wallet, _provisioningRecord);
                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ProvisioningRecordUpdated });
            }
            PopupNavigation.Instance.PopAsync();
        }

        private string ImageToBase64(string imagePath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }

        private async Task DeleteProfilePicture()
        {
            var context = await _agentContextProvider.GetContextAsync();

            _provisioningRecord.Owner.ImageUrl = null;
            await _walletRecordService.UpdateAsync(context.Wallet, _provisioningRecord);
            _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ProvisioningRecordUpdated });
            PopupNavigation.Instance.PopAsync();
        }

        #region Bindable commands
        public ICommand EditNameCommand => 
            new Command(async () => await NavigationService
            .NavigateToPopupAsync(true, _scope.Resolve<ProfileNamePopupViewModel>(new NamedParameter("record", _provisioningRecord))));

        public ICommand EditProfileImage => new Command(async () =>
        {
            await PopupNavigation.Instance.PushAsync(new EditProfilePicturePopup
            {
                FromGalleryCommand = new Command(async () => await SelectPictureFromGallery()),
                DeletePictureCommand = new Command(async () => await DeleteProfilePicture())
            });                       
        });

        //public async void GetEditAgentNameTapped(object sender, EventArgs e)
        //{
        //    await NavigationService.NavigateToPopupAsync(true, _scope.Resolve<ProfileNamePopupViewModel>(new NamedParameter("record", _provisioningRecord)));
        //}
        #endregion

        #region Bindable Properties
        private string _agentName;
        public string AgentName
        {
            get => _agentName;
            set => this.RaiseAndSetIfChanged(ref _agentName, value);
        }

        private string _agentImageUrl;
        public string AgentImageUrl
        {
            get => _agentImageUrl;
            set => this.RaiseAndSetIfChanged(ref _agentImageUrl, value);
        }

        private string _mediatorEndpointUrl;
        public string MediatorEndpointUrl
        {
            get => _mediatorEndpointUrl;
            set => this.RaiseAndSetIfChanged(ref _mediatorEndpointUrl, value);
        }

        private string _mediatorConnectionState;
        public string MediatorConnectionState
        {
            get => _mediatorConnectionState;
            set => this.RaiseAndSetIfChanged(ref _mediatorConnectionState, value);
        }

        private ImageSource _agentImageSource;
        public ImageSource AgentImageSource
        {
            get => _agentImageSource;
            set => this.RaiseAndSetIfChanged(ref _agentImageSource, value);
        }
        #endregion
    }
}
