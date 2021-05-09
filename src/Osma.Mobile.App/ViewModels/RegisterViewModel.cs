using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Agents.Edge;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Storage;
using Microsoft.Extensions.Options;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
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
        private readonly ILifetimeScope _scope;
        private readonly AgentOptions _agentOptions;
        private string[] Passphrase;

        public RegisterViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            IAgentProvider agentProvider,
            IPoolConfigurator poolConfigurator,
            ILifetimeScope scope,
            IOptions<AgentOptions> agentOptions,
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
            _agentOptions = agentOptions.Value;
            _scope = scope;
        }

        private string GenerateStrongPassphrase()
        {
            string filePath = "Osma.Mobile.App.Resources.google_english_medium_long.txt";
            string text = "";
            using (var reader = new StreamReader(Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(filePath)))
            {
                text = reader.ReadToEnd();
            }
            string[] wordArray = text.Split(Environment.NewLine);
            var len = wordArray.Length;
            var random = new Random();

            string[] generatedPassphrase = new string[10];
            for (int i = 0; i < 10; i++)
            {
                generatedPassphrase[i] = wordArray[random.Next(0, 7701)];
            }
            Passphrase = generatedPassphrase;
            //using (RNGCryptoServiceProvider rg = new RNGCryptoServiceProvider())
            //{
            //    byte[] rno = new byte[4];
            //    for (int i = 0; i < 4; i++)
            //    {
            //        rg.GetBytes(rno);
            //        int randomvalue = BitConverter.ToInt32(rno, 0);
            //    }                
            //}
            return string.Join(string.Empty, generatedPassphrase);
        }

        #region Bindable Commands
        public ICommand CreateWalletCommand => new Command(async () =>
        {
            var dialog = UserDialogs.Instance.Loading("Creating wallet");

            try
            {
                var pwd = GenerateStrongPassphrase();
                _agentOptions.AgentName = SitanAgentName;
                _agentOptions.WalletCredentials.Key = pwd;                               

                await _poolConfigurator.ConfigurePoolsAsync();
                await _edgeProvisioningService.ProvisionAsync();

                var context = await _agentContextProvider.GetContextAsync();
                var provisioningRecord = await _provisioningService.GetProvisioningAsync(context.Wallet);
                Preferences.Set(AppConstant.MediatorConnectionIdTagName, provisioningRecord.GetTag(AppConstant.MediatorConnectionIdTagName));
                Preferences.Set(AppConstant.WalletKey, pwd);
                Preferences.Set(AppConstant.LocalWalletProvisioned, true);

                //await NavigationService.NavigateToAsync<MainViewModel>(); 
                var verifyPasswordVm = _scope.Resolve<VerifyPasswordViewModel>(new NamedParameter("passwordArray", Passphrase));
                await NavigationService.NavigateToAsync(verifyPasswordVm);
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

        #region Bindable Properties
        private string _sitanAgentName;
        public string SitanAgentName
        {
            get => _sitanAgentName;
            set => this.RaiseAndSetIfChanged(ref _sitanAgentName, value);
        }
        #endregion
    }
}
