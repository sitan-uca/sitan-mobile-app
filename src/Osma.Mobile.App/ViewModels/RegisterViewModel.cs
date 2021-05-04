using System;
using System.Diagnostics;
using System.Windows.Input;
using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Agents.Edge;
using Hyperledger.Aries.Configuration;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels
{
    public class RegisterViewModel : ABaseViewModel
    {
        private readonly IAgentProvider _agentContextProvider;
        private readonly IPoolConfigurator _poolConfigurator;
        private readonly IProvisioningService _provisioningService;
        private readonly IEdgeProvisioningService _edgeProvisioningService;

        public RegisterViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            IAgentProvider agentProvider,
            IPoolConfigurator poolConfigurator,
            IProvisioningService provisioningService,
            IEdgeProvisioningService edgeProvisioningService) : base(
                nameof(RegisterViewModel),
                userDialogs,
                navigationService)
        {
            _agentContextProvider = agentProvider;
            _poolConfigurator = poolConfigurator;
            _provisioningService = provisioningService;
            _edgeProvisioningService = edgeProvisioningService;
        }

        #region Bindable Commands
        public ICommand CreateWalletCommand => new Command(async () =>
        {
            var dialog = UserDialogs.Instance.Loading("Creating wallet");

            try
            {
                await _poolConfigurator.ConfigurePoolsAsync();
                await _edgeProvisioningService.ProvisionAsync();

                var context = await _agentContextProvider.GetContextAsync();
                var provisioningRecord = await _provisioningService.GetProvisioningAsync(context.Wallet);
                Preferences.Set(AppConstant.MediatorConnectionIdTagName, provisioningRecord.GetTag(AppConstant.MediatorConnectionIdTagName));
                Preferences.Set(AppConstant.LocalWalletProvisioned, true);

                await NavigationService.NavigateToAsync<MainViewModel>();                
            }
            catch(Exception e)
            {
                UserDialogs.Instance.Alert($"Failed to create wallet: {e.Message}");
                Debug.WriteLine(e);
            }
            finally
            {
                dialog?.Hide();
                dialog?.Dispose();
            }
        });
        #endregion
    }
}
