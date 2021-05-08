using Acr.UserDialogs;
using Hyperledger.Aries.Features.IssueCredential;
using Osma.Mobile.App.Extensions;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Proofs
{
    public class AttributeCredentialsViewModel : ABaseViewModel
    {
        private readonly IUserDialogs userDialogs;
        private readonly INavigationService navigationService;                        

        public Credential Credential { get; }
        public string Referent { get; }
        public string AttributeName { get; set; }

        string _credentialName;
        public string CredentialName
        {
            get => _credentialName;
            set
            {
                this.RaiseAndSetIfChanged(ref _credentialName, value);
            }
        }

        string _credentialConnection = string.Empty;
        public string CredentialConnection
        {
            get => _credentialConnection;
            set
            {
                this.RaiseAndSetIfChanged(ref _credentialConnection, value);
            }
        }

        string _credentialConnectionImageUrl = string.Empty;
        public string CredentialConnectionImageUrl
        {
            get => _credentialConnectionImageUrl;
            set
            {
                this.RaiseAndSetIfChanged(ref _credentialConnectionImageUrl, value);
            }
        }

        ImageSource _credentialConnectionImageSource;
        public ImageSource CredentialConnectionImageSource
        {
            get => _credentialConnectionImageSource;
            set
            {
                this.RaiseAndSetIfChanged(ref _credentialConnectionImageSource, value);
            }
        }

        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                this.RaiseAndSetIfChanged(ref _selected, value);
            }
        }

        
        public AttributeCredentialsViewModel(IUserDialogs userDialogs,
                                             INavigationService navigationService,
                                             Credential credential,
                                             string attributeName,
                                             string referent
                                            ) : base(nameof(AttributeCredentialsViewModel), userDialogs, navigationService)
        {
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            Credential = credential;
            AttributeName = attributeName;
            Referent = referent;
            CredentialName = Credential.CredentialInfo.SchemaId.ToCredentialName();
        }
    }
}
