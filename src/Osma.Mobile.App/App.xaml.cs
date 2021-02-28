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
                    services.AddAriesFramework(builder => builder.RegisterEdgeAgent(
                        options: options =>
                        {
                            options.EndpointUri = "https://mediatoragentwin.azurewebsites.net";

                            options.WalletConfiguration.StorageConfiguration =
                                new WalletConfiguration.WalletStorageConfiguration
                                {
                                    Path = Path.Combine(
                                        path1: FileSystem.AppDataDirectory,
                                        path2: ".indy_client",
                                        path3: "wallets")
                                };
                            options.WalletConfiguration.Id = "MobileWallet";
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
                            options.PoolName = "sovrin-staging";
                        },

                        delayProvisioning: true));

                    services.AddSingleton<IPoolConfigurator, PoolConfigurator>();
                    //services.AddSingleton<DefaultBasicMessageHandler>();
                    //services.AddSingleton<DefaultTrustPingMessageHandler>();
                    //services.AddSingleton<DefaultDiscoveryService>();

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
            if (Preferences.Get(AppConstant.LocalWalletProvisioned, false))
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var context = await Container.Resolve<IAgentProvider>().GetContextAsync();
                        var (processedCount, unprocessedItems) = await Container.Resolve<IEdgeClientService>().FetchInboxAsync(context);
                        //Container.Resolve<AgentBase>().Handlers.Add();

                        //var pr = context.SupportedMessages;
                        //Console.WriteLine(pr);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });
            }
        }

        private void checkTimeAndAuth()
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
            checkTimeAndAuth();
        }
    }
}
