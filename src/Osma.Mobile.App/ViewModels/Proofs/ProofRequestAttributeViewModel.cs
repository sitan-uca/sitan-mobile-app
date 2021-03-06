using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Osma.Mobile.App.Converters;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Proofs
{
    public class ProofRequestAttributeViewModel : ABaseViewModel
    {        
        private readonly IUserDialogs userDialogs;
        private readonly INavigationService navigationService;
        private readonly IAgentProvider agentContextProvider;
        private readonly ICredentialService credentialService;
        private readonly IConnectionService connectionService;
        private readonly ILifetimeScope scope;        

        public ProofRequestAttributeViewModel(IUserDialogs userDialogs,
                                              INavigationService navigationService,
                                              IAgentProvider agentContextProvider,
                                              ICredentialService credentialService,
                                              IConnectionService connectionService,
                                              ILifetimeScope scope,
                                              string name,
                                              bool isPredicate,
                                              IEnumerable<Credential> attributeCredentials,
                                              string referent
                                              ) : base("Credentials for Attribute", userDialogs, navigationService)
        {
            this.userDialogs = userDialogs;
            this.navigationService = navigationService;
            this.agentContextProvider = agentContextProvider;
            this.credentialService = credentialService;
            this.connectionService = connectionService;
            this.scope = scope;
            AttributeName = name;
            IsPredicate = isPredicate;
            AttributeCredentials = new ObservableCollection<AttributeCredentialsViewModel>();
            AttributeReferent = referent;

            if (attributeCredentials != null)
                foreach (Credential credential in attributeCredentials)
                {
                    var attributeCred = scope.Resolve<AttributeCredentialsViewModel>(
                        new NamedParameter("credential", credential),
                        new NamedParameter("attributeName", name),
                        new NamedParameter("referent", referent)
                    );

                    AttributeCredentials.Add(attributeCred);
                }
        }

        public override async Task InitializeAsync(object navigationData)
        {
            await GetConnectionForCredentials();
            await base.InitializeAsync(navigationData);
        }

        private async Task GetConnectionForCredentials()
        {
            if (AttributeCredentials == null)
                return;

            var agentContext = await agentContextProvider.GetContextAsync();
            foreach (var cred in AttributeCredentials)
            {
                try
                {
                    var credential = await credentialService.GetAsync(agentContext, cred.Credential.CredentialInfo.Referent);
                    if (credential == null || string.IsNullOrWhiteSpace(credential?.ConnectionId))
                        continue;

                    var connection = await connectionService.GetAsync(agentContext, credential.ConnectionId);
                    if (connection == null)
                        continue;

                    cred.CredentialConnection = connection.Alias?.Name;
                    cred.CredentialConnectionImageUrl = connection.Alias?.ImageUrl;
                    cred.CredentialConnectionImageSource = Base64StringToImageSource.Base64StringToImage(connection.Alias?.ImageUrl);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
        

        void SelectCredential(AttributeCredentialsViewModel attributeCredential)
        {
            if (attributeCredential == null)
                return;

            foreach (var cred in AttributeCredentials)
            {
                if (cred == attributeCredential)
                {
                    cred.Selected = !cred.Selected;
                }
                else
                {
                    cred.Selected = false;
                }
            }

            SelectedCredential = attributeCredential.Selected ? attributeCredential : null;
            if (SelectedCredential != null)
            {
                if (SelectedCredential.Credential.CredentialInfo.Attributes.TryGetValue(AttributeName, out var revealedValue))
                    RevealedAttributeValue = revealedValue;

                SelectedCredentialConnectionImageSource = SelectedCredential.CredentialConnectionImageSource;
                IsCredentialSelected = true;
            }
            else
            {
                RevealedAttributeValue = null;
                SelectedCredentialConnectionImageSource = null;
                IsCredentialSelected = false;
            }
        }

        #region Commands

        public ICommand SelectCredentialCommand => new Command<AttributeCredentialsViewModel>((credential) => SelectCredential(credential));

        #endregion

        #region Properties
        public string AttributeName { get; set; }        
        public bool IsPredicate { get; internal set; }
        public string AttributeReferent { get; set; }
        public ObservableCollection<AttributeCredentialsViewModel> AttributeCredentials { get; set; }
        AttributeCredentialsViewModel selectedCredential;

        public AttributeCredentialsViewModel SelectedCredential
        {
            get => selectedCredential;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedCredential, value);
            }
        }

        private string _revealedAttributeValue;
        public string RevealedAttributeValue
        {
            get => _revealedAttributeValue;
            set => this.RaiseAndSetIfChanged(ref _revealedAttributeValue, value);            
        }

        private bool _isCredentialSelected;
        public bool IsCredentialSelected
        {
            get => _isCredentialSelected;
            set => this.RaiseAndSetIfChanged(ref _isCredentialSelected, value);
        }

        private string _selectedCredentialConnectionImageUrl;
        public string SelectedCredentialConnectionImageUrl
        {
            get => _selectedCredentialConnectionImageUrl;
            set => this.RaiseAndSetIfChanged(ref _selectedCredentialConnectionImageUrl, value);
        }

        private ImageSource _selectedCredentialConnectionImageSource;
        public ImageSource SelectedCredentialConnectionImageSource
        {
            get => _selectedCredentialConnectionImageSource;
            set => this.RaiseAndSetIfChanged(ref _selectedCredentialConnectionImageSource, value);
        }
        #endregion
    }
}
