using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.Discovery;
using Hyperledger.Aries.Features.TrustPing;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Extensions;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.Views.Connections;
using Osma.Mobile.App.Utilities;
using System.Collections.ObjectModel;
using ReactiveUI;
using Xamarin.Forms;
using System.ComponentModel;
using Hyperledger.Aries.Features.BasicMessage;
using System.Globalization;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Routing;
using System.Reactive.Linq;
using Hyperledger.Aries.Models.Events;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class ConnectionViewModel : ABaseViewModel, INotifyPropertyChanged
    {
        private readonly ConnectionRecord _record;

        private readonly IAgentProvider _agentContextProvider;
        private readonly IMessageService _messageService;
        private readonly IDiscoveryService _discoveryService;
        private readonly IConnectionService _connectionService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWalletRecordService _walletRecordSevice;
        private IAgentContext _agentContext;

        public ObservableCollection<BasicMessageRecord> Messages { get; set; }
        private List<BasicMessageRecord> m;

        public ConnectionViewModel(IUserDialogs userDialogs,
                                   INavigationService navigationService,
                                   IAgentProvider agentContextProvider,
                                   IMessageService messageService,
                                   IDiscoveryService discoveryService,
                                   IConnectionService connectionService,
                                   IEventAggregator eventAggregator,
                                   IWalletRecordService walletRecordService,
                                   ConnectionRecord record) :
                                   base(nameof(ConnectionViewModel),
                                       userDialogs,
                                       navigationService)
        {
            _agentContextProvider = agentContextProvider;
            _messageService = messageService;
            _discoveryService = discoveryService;
            _connectionService = connectionService;
            _eventAggregator = eventAggregator;
            _walletRecordSevice = walletRecordService;

            _record = record;
            MyDid = _record.MyDid;
            TheirDid = _record.TheirDid;
            ConnectionName = _record.Alias?.Name;
            ConnectionSubtitle = $"{_record.State:G}";
            ConnectionImageUrl = _record.Alias?.ImageUrl;

            if (ConnectionImageUrl == null) 
            {
                ConnectionImageUrl = "https://iconsgalore.com/wp-content/uploads/2018/10/cell-phone-1-featured-2.png";
            }

            //Messages.Add(new ChatTemplateSelector.Message() { Text = "Hi" });
            //Messages.Add(new ChatTemplateSelector.Message() { Text = "How are you?" });
            Messages = new ObservableCollection<BasicMessageRecord>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override async Task InitializeAsync(object navigationData)
        {
            _agentContext = await _agentContextProvider.GetContextAsync();
            await RefreshTransactions();
            await InitializeChat();

            _eventAggregator.GetEventByType<ApplicationEvent>()
                            .Where(_ => _.Type == ApplicationEventType.WalletRecordsMessageUpdated)
                            .Subscribe(async _ => await InitializeChat());

            await base.InitializeAsync(navigationData);
        }

        //TODO: Real time recieve messages
        public async Task InitializeChat()
        {
            List<BasicMessageRecord> m = await _walletRecordSevice
                .SearchAsync<BasicMessageRecord>(_agentContext.Wallet, SearchQuery.Equal("ConnectionId",_record.Id));
            m.Sort((a, b) => Nullable.Compare(a.CreatedAtUtc, b.CreatedAtUtc));
            m.Except(Messages, new MessageRecordComparator()).ToList().ForEach(x => Messages.Insert(0, x));
        }

        public async Task RefreshTransactions()
        {
            RefreshingTransactions = true;          
            var message = _discoveryService.CreateQuery(_agentContext, "*");           
            DiscoveryDiscloseMessage protocols = null;

            try
            {
                var response = await _messageService.SendReceiveAsync(_agentContext.Wallet, message, _record) as UnpackedMessageContext;
                protocols = response.GetMessage<DiscoveryDiscloseMessage>();
            }
            catch (Exception e)
            {
                //Swallow exception
                //TODO more granular error protection
                Console.Error.WriteLine(e);
            }

            IList<TransactionItem> transactions = new List<TransactionItem>();

            Transactions.Clear();

            if (protocols == null)
            {
                HasTransactions = false;
                RefreshingTransactions = false;
                return;
            }

            foreach (var protocol in protocols.Protocols)
            {
                switch (protocol.ProtocolId)
                {
                    case MessageTypes.TrustPingMessageType:
                        transactions.Add(new TransactionItem()
                        {
                            Title = "Trust Ping",
                            Subtitle = "Version 1.0",
                            PrimaryActionTitle = "Ping",
                            PrimaryActionCommand = new Command(async () =>
                            {
                                await PingConnectionAsync();
                            }, () => true),
                            Type = TransactionItemType.Action.ToString("G")
                        });
                        break;
                }
            }

            Transactions.InsertRange(transactions);
            HasTransactions = transactions.Any();

            RefreshingTransactions = false;
        }

        public async Task PingConnectionAsync()
        {
            var dialog = UserDialogs.Instance.Loading("Pinging");
            
            var message = new TrustPingMessage
            {
                ResponseRequested = true
            };

            bool success = false;
            try
            {
                var response = await _messageService.SendReceiveAsync(_agentContext.Wallet, message, _record) as UnpackedMessageContext;
                var trustPingResponse = response.GetMessage<TrustPingResponseMessage>();
                success = true;
            }
            catch (Exception)
            {
                //Swallow exception
                //TODO more granular error protection
            }

            if (dialog.IsShowing)
            {
                dialog.Hide();
                dialog.Dispose();
            }

            DialogService.Alert(
                    success ?
                    "Ping Response Recieved" :
                    "No Ping Response Recieved"
                );
        }

        #region Bindable Command
        public ICommand NavigateBackCommand => new Command(async () =>
        {
            await NavigationService.NavigateBackAsync();
        });

        public ICommand DeleteConnectionCommand => new Command(async () =>
        {
            var dialog = DialogService.Loading("Deleting");

            
            await _connectionService.DeleteAsync(_agentContext, _record.Id);

            _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionsUpdated });

            if (dialog.IsShowing)
            {
                dialog.Hide();
                dialog.Dispose();
            }

            await NavigationService.NavigateBackAsync();
        });

        public ICommand RefreshTransactionsCommand => new Command(async () => await RefreshTransactions());

        public ICommand SendMessageCommand => new Command(async () =>
        {
            if (!string.IsNullOrEmpty(_textToSend))
            {
                
                var sentTime = DateTime.UtcNow;
                var messageRecord = new BasicMessageRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Direction = MessageDirection.Outgoing,
                    Text = _textToSend,
                    SentTime = sentTime,
                    ConnectionId = _record.Id
                };
                var message = new BasicMessage
                {
                    Content = _textToSend,
                    SentTime = sentTime.ToString("s", CultureInfo.InvariantCulture),
                    Type = MessageTypes.BasicMessageType
                };

                await _walletRecordSevice.AddAsync(_agentContext.Wallet, messageRecord);
                await _messageService.SendAsync(_agentContext.Wallet, message, _record);
                
                //Messages.Insert(0, messageRecord);
                _textToSend = string.Empty;
            }
        });
        #endregion

        #region Bindable Properties
        private string _connectionName;
        public string ConnectionName
        {
            get => _connectionName;
            set => this.RaiseAndSetIfChanged(ref _connectionName, value);
        }

        private string _myDid;
        public string MyDid
        {
            get => _myDid;
            set => this.RaiseAndSetIfChanged(ref _myDid, value);
        }

        private string _theirDid;
        public string TheirDid
        {
            get => _theirDid;
            set => this.RaiseAndSetIfChanged(ref _theirDid, value);
        }

        private string _connectionImageUrl;
        public string ConnectionImageUrl
        {
            get => _connectionImageUrl;
            set => this.RaiseAndSetIfChanged(ref _connectionImageUrl, value);
        }

        private string _connectionSubtitle = "Lorem ipsum dolor sit amet";
        public string ConnectionSubtitle
        {
            get => _connectionSubtitle;
            set => this.RaiseAndSetIfChanged(ref _connectionSubtitle, value);
        }

        private RangeEnabledObservableCollection<TransactionItem> _transactions = new RangeEnabledObservableCollection<TransactionItem>();
        public RangeEnabledObservableCollection<TransactionItem> Transactions
        {
            get => _transactions;
            set => this.RaiseAndSetIfChanged(ref _transactions, value);
        }

        private bool _refreshingTransactions;
        public bool RefreshingTransactions
        {
            get => _refreshingTransactions;
            set => this.RaiseAndSetIfChanged(ref _refreshingTransactions, value);
        }

        private bool _hasTransactions;
        public bool HasTransactions
        {
            get => _hasTransactions;
            set => this.RaiseAndSetIfChanged(ref _hasTransactions, value);
        }

        private string _textToSend;
        public string TextToSend
        {
            get => _textToSend;
            set => this.RaiseAndSetIfChanged(ref _textToSend, value);
        }
        #endregion
    }
}
