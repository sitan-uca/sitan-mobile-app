using System;

using System.Linq;
using System.Windows.Input;
using Acr.UserDialogs;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using Xamarin.Forms;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.DidExchange;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using Osma.Mobile.App.Events;
using Hyperledger.Aries.Contracts;
using Osma.Mobile.App.Extensions;
using System.Collections.Generic;
using Osma.Mobile.App.Converters;

namespace Osma.Mobile.App.ViewModels.Credentials
{
    public class CredentialViewModel : ABaseViewModel
    {
        private readonly CredentialRecord _credential;
        private readonly IConnectionService _connectionService;
        private readonly IAgentProvider agentContextProvider;
        private readonly IWalletRecordService walletRecordService;
        private readonly IMessageService messageService;
        private readonly ICredentialService credentialService;
        private readonly IEventAggregator eventAggregator;
        private readonly ConnectionRecord _connection;

        public CredentialViewModel(IUserDialogs userDialogs,
                                   INavigationService navigationService,
                                   IConnectionService connectionService,
                                   IAgentProvider agentContextProvider,
                                   IWalletRecordService walletRecordService,
                                   IMessageService messageService,
                                   ICredentialService credentialService,
                                   IEventAggregator eventAggregator,
                                   CredentialRecord credential,
                                   ConnectionRecord connection) : base(nameof(CredentialViewModel), userDialogs, navigationService)
        {
            this._connectionService = connectionService;
            this.agentContextProvider = agentContextProvider;
            this.walletRecordService = walletRecordService;
            this.messageService = messageService;
            this.credentialService = credentialService;
            this.eventAggregator = eventAggregator;
            this._credential = credential;
            this._connection = connection;

#if SMP_DEBUG
            _credentialName = "Credential Name";
            _credentialImageUrl = "http://placekitten.com/g/200/200";
            _credentialSubtitle = "10/22/2017";
            _credentialType = "Bank Statement";
            _qRImageUrl = "http://placekitten.com/g/100/100";

            var attributes = new List<CredentialAttribute>(new CredentialAttribute[] {
                new CredentialAttribute
                {
                    Type="Text",
                    Name="First Name",
                    Value="Jamie"
                },
                new CredentialAttribute
                {
                    Type="Text",
                    Name="Last Name",
                    Value="Doe"
                },
                new CredentialAttribute
                {
                    Type = "Text",
                    Name = "Country of Residence",
                    Value = "New Zealand"
                },
                new CredentialAttribute
                {
                    Type="File",
                    Name="Statement",
                    Value="Statement.pdf",
                    FileExt="PDF",
                    Date="05 Aug 2018"
                }
            });
            //_attributes = attributes.OrderByDescending(o => o.Type).OrderBy(o => o.Date);
            _attributes = attributes;
#else

            CredentialName = _credential.SchemaId.ToCredentialName();
            CredentialImageUrl = _connection?.Alias.ImageUrl;
            CredentialImageSource = Base64StringToImageSource.Base64StringToImage(_connection?.Alias.ImageUrl);
            CredentialSubtitle = _connection?.Alias.Name;
            CreatedAt = _credential.CreatedAtUtc?.ToLocalTime();
            CredentialState = _credential.State;
            CredentialType = _credential.TypeName;
            _isNew = IsCredentialNew(_credential);

            if (_credential.CredentialAttributesValues != null)
            {
                if (Attributes == null)
                    Attributes = new List<CredentialAttribute>();

                Attributes.AddRange(_credential.CredentialAttributesValues.Select((CredentialPreviewAttribute x) => new CredentialAttribute
                {
                    Name = x.Name,
                    Value = x.Value != null ? x.Value.ToString() : string.Empty,
                    Type = x.MimeType
                }));
            }
#endif
        }


        private bool IsCredentialNew(CredentialRecord credential)
        {
            //// TODO OS-200, Currently a Placeholder for a mix of new and not new cells
            //Random random = new Random();
            //return random.Next(0, 2) == 1;
            return _credential.State == CredentialState.Offered;
        }

        async Task AcceptCredential()
        {
            var dialog = UserDialogs.Instance.Loading("Requesting...");
            try
            {
                IsBusy = true;
                var context = await agentContextProvider.GetContextAsync();

                var (request, _) = await credentialService.CreateRequestAsync(context, _credential.Id);
                await messageService.SendAsync(context, request, _connection);

                eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialsUpdated });
                await NavigationService.PopModalAsync();
            }
            catch (Exception xx)
            {
                DialogService.Alert(xx.Message);
            }
            finally
            {
                IsBusy = false;
                dialog?.Hide();
                dialog?.Dispose();
            }
        }

        async Task RejectCredential()
        {
            var isConfirmed = await Application.Current.MainPage.DisplayAlert("Confirm", "Are you sure you want to reject this credential offer?", "Yes", "No");
            if (!isConfirmed)
                return;

            try
            {
                IsBusy = true;

                var context = await agentContextProvider.GetContextAsync();
                await credentialService.RejectOfferAsync(context, _credential.Id);

                eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialsUpdated });
                await NavigationService.PopModalAsync();
            }
            catch (Exception xx)
            {
                DialogService.Alert(xx.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteCredential()
        {
            var res = await UserDialogs.Instance
                .ConfirmAsync($"You are about to delete '{CredentialName}'. Are you sure?", "Delete credential", "Delete", "Cancel");
            if (res)
            {
                var dialog = DialogService.Loading("Deleting");
                var context = await agentContextProvider.GetContextAsync();

                await credentialService.DeleteCredentialAsync(context, _credential.Id);

                eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.CredentialsUpdated });

                if (dialog.IsShowing)
                {
                    dialog.Hide();
                    dialog.Dispose();
                }
            }
        }

#region Bindable Command

        public ICommand NavigateBackCommand => new Command(async () =>
        {
            await NavigationService.PopModalAsync();
        });

        public ICommand AcceptCredentialCommand => new Command(async () => await AcceptCredential());
        public ICommand RejectCredentialCommand => new Command(async () => await RejectCredential());

        public ICommand DeleteCredentialCommand => new Command(async () => await DeleteCredential());

#endregion

#region Bindable Properties

        private string _credentialName;
        public string CredentialName
        {
            get => _credentialName;
            set => this.RaiseAndSetIfChanged(ref _credentialName, value);
        }

        private string _credentialType;
        public string CredentialType
        {
            get => _credentialType;
            set => this.RaiseAndSetIfChanged(ref _credentialType, value);
        }

        private string _credentialImageUrl;
        public string CredentialImageUrl
        {
            get => _credentialImageUrl;
            set => this.RaiseAndSetIfChanged(ref _credentialImageUrl, value);
        }

        private ImageSource _credentialImageSource;
        public ImageSource CredentialImageSource
        {
            get => _credentialImageSource;
            set => this.RaiseAndSetIfChanged(ref _credentialImageSource, value);
        }

        private string _credentialSubtitle;
        public string CredentialSubtitle
        {
            get => _credentialSubtitle;
            set => this.RaiseAndSetIfChanged(ref _credentialSubtitle, value);
        }

        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set => this.RaiseAndSetIfChanged(ref _isNew, value);
        }

        private string _qRImageUrl;
        public string QRImageUrl
        {
            get => _qRImageUrl;
            set => this.RaiseAndSetIfChanged(ref _qRImageUrl, value);
        }

        private DateTime? _createdAt;
        public DateTime? CreatedAt
        {
            get => _createdAt;
            set => this.RaiseAndSetIfChanged(ref _createdAt, value);
        }

        private List<CredentialAttribute> _attributes;
        public List<CredentialAttribute> Attributes
        {
            get => _attributes;
            set => this.RaiseAndSetIfChanged(ref _attributes, value);
        }

        CredentialState _credentialState;
        public CredentialState CredentialState
        {
            get => _credentialState;
            set => this.RaiseAndSetIfChanged(ref _credentialState, value);
        }

#endregion
    }
}
