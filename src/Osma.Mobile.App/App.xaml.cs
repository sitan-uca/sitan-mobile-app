using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.BasicMessage;
using Hyperledger.Aries.Features.TrustPing;
using Hyperledger.Aries.Features.Discovery;
using Hyperledger.Aries.Routing;
using Hyperledger.Aries.Storage;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Utilities;
using Osma.Mobile.App.ViewModels;
using Osma.Mobile.App.ViewModels.Account;
using Osma.Mobile.App.ViewModels.Connections;
using Osma.Mobile.App.ViewModels.CreateInvitation;
using Osma.Mobile.App.ViewModels.Credentials;
using Osma.Mobile.App.ViewModels.Proofs;
using Osma.Mobile.App.Views;
using Osma.Mobile.App.Views.Account;
using Osma.Mobile.App.Views.Connections;
using Osma.Mobile.App.Views.CreateInvitation;
using Osma.Mobile.App.Views.Credentials;
using Osma.Mobile.App.Views.Proofs;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Osma.Mobile.App.ViewModels.PinAuth;
using Osma.Mobile.App.Views.PinAuth;
using Osma.Mobile.App.Baksak;
using Plugin.Connectivity;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Osma.Mobile.App
{
    public partial class App : Application
    {
        public new static App Current => Application.Current as App;
        public static IContainer Container { get; set; }

        // Timer to check new messages in the configured mediator agent every 10sec
        private readonly Timer timer;
        private static IHost Host { get; set; }

        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();
            timer = new Timer
            {
                Enabled = false,
                AutoReset = true,
                Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
            };
            timer.Elapsed += Timer_Elapsed;
        }

        public App(IHost host) : this() => Host = host;

        public static IHostBuilder BuildHost(Assembly platformSpecific = null) =>
            XamarinHost.CreateDefaultBuilder<App>()
                .ConfigureServices((_, services) =>
                {
                    services.AddAriesFramework(builder => builder.RegisterEdgeAgent<SitanAgent>(
                        options: options =>
                        {
                            options.EndpointUri = "https://mediatoragentwin.azurewebsites.net";
                            //options.EndpointUri = "https://fast-warthog-93.loca.lt";

                            options.WalletConfiguration.StorageConfiguration =
                                new WalletConfiguration.WalletStorageConfiguration
                                {
                                    Path = Path.Combine(
                                        path1: FileSystem.AppDataDirectory,
                                        path2: ".indy_client",
                                        path3: "wallets")
                                };
                            options.WalletConfiguration.Id = "BaksakMobileWallet";
                            //TODO Ask user for a wallet key during provisioning
                            options.WalletCredentials.Key = "SecretWalletKey";
                            options.AgentName = "Mobile Agent";
                            options.RevocationRegistryDirectory = Path.Combine(
                                path1: FileSystem.AppDataDirectory,
                                path2: ".indy_client",
                                path3: "tails");

                            // Available network configurations (see PoolConfigurator.cs):
                            //   sovrin-live
                            //   sovrin-staging
                            //   sovrin-builder
                            //   bcovrin-test
                            //   baksak-main
                            options.PoolName = "baksak-main";
                            options.ProtocolVersion = 2;
                        },

                        delayProvisioning: true));
                    
                    services.AddSingleton<IPoolConfigurator, PoolConfigurator>();
                    //services.AddSingleton<IWalletRecordService, BaksakWalletRecordService>();
                    services.AddExtendedWalletRecordService<BaksakWalletRecordService>();

                    var containerBuilder = new ContainerBuilder();
                    containerBuilder.RegisterAssemblyModules(typeof(CoreModule).Assembly);
                    if (platformSpecific != null)
                    {
                        containerBuilder.RegisterAssemblyModules(platformSpecific);
                    }

                    containerBuilder.Populate(services);
                    Container = containerBuilder.Build();
                });

        protected override async void OnStart()
        {
            await Host.StartAsync();

            //await Container.Resolve<IPoolConfigurator>().ConfigurePoolsAsync();

            // View models and pages mappings
            var _navigationService = Container.Resolve<INavigationService>();
            _navigationService.AddPageViewModelBinding<MainViewModel, MainPage>();
            _navigationService.AddPageViewModelBinding<ConnectionsViewModel, ConnectionsPage>();
            _navigationService.AddPageViewModelBinding<ConnectionViewModel, ConnectionPage>();
            _navigationService.AddPageViewModelBinding<RegisterViewModel, RegisterPage>();
            _navigationService.AddPageViewModelBinding<AcceptInviteViewModel, AcceptInvitePage>();
            _navigationService.AddPageViewModelBinding<CredentialsViewModel, CredentialsPage>();
            _navigationService.AddPageViewModelBinding<CredentialViewModel, CredentialPage>();
            _navigationService.AddPageViewModelBinding<AccountViewModel, AccountPage>();
            _navigationService.AddPageViewModelBinding<CreateInvitationViewModel, CreateInvitationPage>();

            _navigationService.AddPageViewModelBinding<ProofRequestsViewModel, ProofRequestsPage>();
            _navigationService.AddPageViewModelBinding<ProofRequestViewModel, ProofRequestPage>();
            _navigationService.AddPageViewModelBinding<ProofRequestAttributeViewModel, ProofRequestAttributePage>();

            _navigationService.AddPageViewModelBinding<PinAuthViewModel, PinAuthPage>();
            _navigationService.AddPageViewModelBinding<CreatePinAuthViewModel, CreatePinAuthPage>();
            _navigationService.AddPageViewModelBinding<ConfirmPinAuthViewModel, ConfirmPinAuthPage>();

            _navigationService.AddPageViewModelBinding<AcceptRequestViewModel, AcceptRequestPage>();
            _navigationService.AddPageViewModelBinding<RequestIdentityProofViewModel, RequestIdentityProof>();
            _navigationService.AddPageViewModelBinding<ConnectionDetailsViewModel, ConnectionDetailsPage>();
            _navigationService.AddPageViewModelBinding<ProfileViewModel, ProfilePage>();            
            _navigationService.AddPopupViewModelBinding<ProfileNamePopupViewModel, ProfileNamePopupPage>();
            //_navigationService.AddPageViewModelBinding<ProofsViewModel, ProofsPage>();

            if (Preferences.Get(AppConstant.LocalWalletProvisioned, false))
            {
                 await _navigationService.NavigateToAsync<MainViewModel>();
            }
            else
            {
                await _navigationService.NavigateToAsync<RegisterViewModel>();
            }
            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check for new messages with the mediator agent if successfully provisioned
            if (Preferences.Get(AppConstant.LocalWalletProvisioned, false) && CrossConnectivity.Current.IsConnected)
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var context = await Container.Resolve<IAgentProvider>().GetContextAsync();
                        var (processedCount, unprocessedItems) = await Container.Resolve<IEdgeClientService>().FetchInboxAsync(context);
                        
                        foreach(var item in unprocessedItems)
                        {
                            Debug.WriteLine("Failed to Process: " + item.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });
            }
            
        }

        private void pinAuthenticateIfEnabled()
        {
            if(Preferences.Get(AppConstant.PinAuthEnabled, false))
            {
                var _navigationService = Container.Resolve<INavigationService>();
                _navigationService.NavigateToAsync<PinAuthViewModel>();
            }
        }

        protected override void OnSleep() =>
            // Stop timer when application goes to background
            timer.Enabled = false;

        protected override void OnResume()
        // Resume timer when application comes in foreground
        {
            timer.Enabled = true;
            pinAuthenticateIfEnabled();
        }
    }
}
