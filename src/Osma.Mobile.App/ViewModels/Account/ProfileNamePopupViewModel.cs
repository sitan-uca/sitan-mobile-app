using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Storage;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Account
{
    public class ProfileNamePopupViewModel : ABaseViewModel
    {
        private readonly IWalletRecordService _walletRecordService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IEventAggregator _eventAggregator;
        private ProvisioningRecord _provisioningRecord;

        public ProfileNamePopupViewModel
            (
                IUserDialogs userDialogs,
                INavigationService navigationService,
                IAgentProvider agentContextProvider,
                IWalletRecordService walletRecordService,
                IEventAggregator eventAggregator,
                ProvisioningRecord record
            ) : base
            (
                nameof(ProfileNamePopupViewModel),
                userDialogs,
                navigationService
            )
        {
            _walletRecordService = walletRecordService;
            _agentContextProvider = agentContextProvider;
            _eventAggregator = eventAggregator;
            _provisioningRecord = record;
            _agentName = record.Owner.Name;

        }

        public override async Task InitializeAsync(object navigationData)
        {
            await base.InitializeAsync(navigationData);
        }

        private async Task UpdateAgentName()
        {
            var context = await _agentContextProvider.GetContextAsync();
            _provisioningRecord.Owner.Name = AgentName;
            await _walletRecordService.UpdateAsync(context.Wallet, _provisioningRecord);
            _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ProvisioningRecordUpdated });
            await PopupNavigation.Instance.PopAsync(true);
        }

        #region Bindable commands

        public ICommand SaveAgentName => new Command(async () => await UpdateAgentName());

        #endregion

        #region Bindable properties
        private string _agentName;
        public string AgentName
        {
            get => _agentName;
            set => this.RaiseAndSetIfChanged(ref _agentName, value);
        }
        #endregion
    }
}
