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
using Osma.Mobile.App.Protocols;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Plugin.Connectivity;
using Osma.Mobile.App.ViewModels.Credentials;
using Osma.Mobile.App.Services;
using Autofac;
using Osma.Mobile.App.Converters;
using System.Threading;

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
        private readonly ILifetimeScope _scope;
        private IAgentContext _agentContext;

        public ObservableCollection<RecordBase> Messages { get; set; }
        private string receivedMsgId = string.Empty;        


        public ConnectionViewModel(IUserDialogs userDialogs,
                                   INavigationService navigationService,
                                   IAgentProvider agentContextProvider,
                                   IMessageService messageService,
                                   IDiscoveryService discoveryService,
                                   IConnectionService connectionService,
                                   IEventAggregator eventAggregator,
                                   IWalletRecordService walletRecordService,
                                   ILifetimeScope scope,
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
            _scope = scope;

            _record = record;
            MyDid = _record.MyDid;
            TheirDid = _record.TheirDid;
            ConnectionName = _record.Alias?.Name ?? _record.Id;
            ConnectionSubtitle = $"{_record.State:G}";
            ConnectionImageUrl = _record.Alias?.ImageUrl;
            ConnectionImageSource = Base64StringToImageSource.Base64StringToImage(_record.Alias?.ImageUrl);

            Messages = new ObservableCollection<RecordBase>();
        }

        //public event PropertyChangedEventHandler PropertyChanged;    

        public override async Task InitializeAsync(object navigationData)
        {                        
            //await RefreshTransactions();
            await InitializeChat();            
            _eventAggregator.GetEventByType<ServiceMessageProcessingEvent>()
                            .Where(_ => _.MessageType == MessageTypes.BasicMessageType)
                            .Subscribe(async _ => await UpdateChatAsync(_.RecordId));
            

            await base.InitializeAsync(navigationData);
        }

        //TODO: Add just the message recieved rather than the whole list
        public async Task InitializeChat()
        {
            _agentContext = await _agentContextProvider.GetContextAsync();
            List<RecordBase> records = new List<RecordBase>();

            List<BasicMessageRecord> msgs = await _walletRecordSevice
                .SearchAsync<BasicMessageRecord>(_agentContext.Wallet, SearchQuery.Equal("ConnectionId", _record.Id), count: 100);
            //List<CredentialRecord> credentials = await _walletRecordSevice
            //    .SearchAsync<CredentialRecord>(_agentContext.Wallet, SearchQuery.Equal("ConnectionId", _record.Id));
            //List<ProofRecord> proofs = await _walletRecordSevice
            //    .SearchAsync<ProofRecord>(_agentContext.Wallet, SearchQuery.Equal("ConnectionId", _record.Id));       

            records.AddRange(msgs);
            //records.AddRange(credentials);
            //records.AddRange(proofs);

            records.Sort((a, b) => Nullable.Compare(a.CreatedAtUtc, b.CreatedAtUtc));
            records.Except(Messages, new RecordComparator()).ToList().ForEach(x => Messages.Insert(0, x));

        }

        public async Task UpdateChatAsync(string messageRecordId)
        {
            var messageRecord = await _walletRecordSevice.GetAsync<BasicMessageRecord>(_agentContext.Wallet, messageRecordId);
            if (!messageRecordId.Equals(receivedMsgId) && messageRecord.ConnectionId == _record.Id)
            {
                receivedMsgId = messageRecordId;                
                Messages.Insert(0, messageRecord);
            }        
        }

        public async Task RefreshTransactions()
        {
            RefreshingTransactions = true;

            _agentContext = await _agentContextProvider.GetContextAsync();
            var message = _discoveryService.CreateQuery(_agentContext, "*");
            DiscoveryDiscloseMessage protocols = null;

            if (!CrossConnectivity.Current.IsConnected)
            {
                UserDialogs.Instance.Toast("Looks like you're missing internet connection!", new TimeSpan(3));
                HasTransactions = false;
                RefreshingTransactions = false;
                return;
            }

            try
            {
                var response = await _messageService.SendReceiveAsync(_agentContext, message, _record) as UnpackedMessageContext;
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
                if (MessageTypes.TrustPingMessageType.Contains(protocol.ProtocolId))
                {
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
                }

                //switch (protocol.ProtocolId)
                //{
                //    case MessageTypes.TrustPingMessageType:
                //        transactions.Add(new TransactionItem()
                //        {
                //            Title = "Trust Ping",
                //            Subtitle = "Version 1.0",
                //            PrimaryActionTitle = "Ping",
                //            PrimaryActionCommand = new Command(async () =>
                //            {
                //                await PingConnectionAsync();
                //            }, () => true),
                //            Type = TransactionItemType.Action.ToString("G")
                //        });
                //        break;                    
                //}
            }

            Transactions.InsertRange(transactions);
            HasTransactions = transactions.Any();

            RefreshingTransactions = false;
        }

        public async Task PingConnectionAsync()
        {
            _agentContext = await _agentContextProvider.GetContextAsync();
            var dialog = UserDialogs.Instance.Loading("Pinging");

            var message = new TrustPingMessage
            {
                ResponseRequested = true
            };

            bool success = false;
            try
            {
                var response = await _messageService.SendReceiveAsync(_agentContext, message, _record) as UnpackedMessageContext;
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

        public async Task SelectCredential(CredentialViewModel credential) => await NavigationService.NavigateToAsync(credential, null, NavigationType.Modal);

        #region Bindable Command
        public ICommand NavigateBackCommand => new Command(async () =>
        {
            await NavigationService.NavigateBackAsync();
        });

        public ICommand RequestProofCommand => new Command(async () => await NavigationService.NavigateToAsync<RequestIdentityProofViewModel>(_record));

        public ICommand ConnectionDetailsCommand => new Command(async () => await NavigationService.NavigateToAsync<ConnectionDetailsViewModel>(_record.Id));

        public ICommand SendTrustPingCommand => new Command(async () => await PingConnectionAsync());

        public ICommand DeleteConnectionCommand => new Command(async () =>
        {
            var res = await UserDialogs.Instance.ConfirmAsync($"You are about to delete connection '{_record.Alias.Name ?? _record.Id}'. Are you sure?", "Delete connection", "Delete", "Cancel");
            if (res)
            {
                var dialog = DialogService.Loading("Deleting");
                _agentContext = await _agentContextProvider.GetContextAsync();

                await _connectionService.DeleteAsync(_agentContext, _record.Id);

                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.ConnectionsUpdated });

                if (dialog.IsShowing)
                {
                    dialog.Hide();
                    dialog.Dispose();
                }
            }               
        });

        public ICommand RefreshTransactionsCommand => new Command(async () => await RefreshTransactions());

        public ICommand SendMessageCommand => new Command(async () =>
        {
            if (!string.IsNullOrEmpty(TextToSend))
            {
                _agentContext = await _agentContextProvider.GetContextAsync();
                var sentTime = DateTime.UtcNow;
                var messageRecord = new BasicMessageRecord
                {
                    Id = Guid.NewGuid().ToString(),
                    Direction = MessageDirection.Outgoing,
                    Text = TextToSend,
                    SentTime = sentTime,
                    ConnectionId = _record.Id
                };
                var message = new BasicMessage
                {
                    Content = TextToSend,
                    SentTime = sentTime.ToString("s", CultureInfo.InvariantCulture),
                    Type = MessageTypes.BasicMessageType
                };

                await _walletRecordSevice.AddAsync(_agentContext.Wallet, messageRecord);
                await _messageService.SendAsync(_agentContext, message, _record);

                Messages.Insert(0, messageRecord);
                Device.BeginInvokeOnMainThread(() => TextToSend = string.Empty);
            }
        });

        public ICommand SelectCredentialCommand => new Command(async (selectedRecord) =>
        {
            if (selectedRecord != null) 
            {
                if (selectedRecord is CredentialRecord)
                {
                    CredentialViewModel credential = _scope.Resolve<CredentialViewModel>(new NamedParameter("credential", selectedRecord),
                                                                                     new NamedParameter("connection", _record));
                    await SelectCredential(credential);
                }                    
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

        private ImageSource _connectionImageSource;
        public ImageSource ConnectionImageSource
        {
            get => _connectionImageSource;
            set => this.RaiseAndSetIfChanged(ref _connectionImageSource, value);
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
