using Acr.UserDialogs;
using Autofac;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Models.Records;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Osma.Mobile.App.Extensions;
using Osma.Mobile.App.Services.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Osma.Mobile.App.ViewModels.Connections
{
    public class RequestIdentityProofViewModel : ABaseViewModel
    {
        private readonly IConnectionService _connectionService;
        private readonly IAgentProvider _agentContextProvider;
        private readonly IProofService _proofService;
        private readonly IMessageService _messageService;
        private readonly ISchemaService _schemaService;
        private readonly ICredentialService _credentialService;
        private readonly ILedgerService _ledgerService;
        private ConnectionRecord _connectionRecord;

        public RequestIdentityProofViewModel(
            IUserDialogs userDialogs,
            INavigationService navigationService,
            IAgentProvider agentContextProvider,
            IMessageService messageService,
            IConnectionService defaultConnectionService,
            ISchemaService schemaService,
            ICredentialService credentialService,
            ILedgerService ledgerService,
            IProofService proofService
            ) : base(
                "Request Identity Proof",
                userDialogs,
                navigationService)
        {
            _agentContextProvider = agentContextProvider;
            _connectionService = defaultConnectionService;
            _proofService = proofService;
            _messageService = messageService;
            _schemaService = schemaService;
            _credentialService = credentialService;
            _ledgerService = ledgerService;
            Schemas = new RangeEnabledObservableCollection<SchemaRecord>();
            CredDefinitions = new RangeEnabledObservableCollection<DefinitionRecord>();
        }

        public override async Task InitializeAsync(object navigationData)
        {
            _connectionRecord = navigationData as ConnectionRecord;
            var context = await _agentContextProvider.GetContextAsync();
            var credentialsRecords = await _credentialService.ListAsync(context);
            List<SchemaRecord> schemasList = new List<SchemaRecord>();
            List<DefinitionRecord> definitionsList = new List<DefinitionRecord>();
            foreach (var credentialRecord in credentialsRecords)
            {
                if (credentialRecord.State == CredentialState.Rejected)
                    continue;
                var schemaResp = await _ledgerService.LookupSchemaAsync(context, credentialRecord.SchemaId);
                var schemaJobj = JObject.Parse(schemaResp?.ObjectJson ?? "");
                schemasList.Add(new SchemaRecord { Id = schemaResp.Id, Name = schemaJobj.GetValue("name").ToString() });
                var defResp = await _ledgerService.LookupDefinitionAsync(context, credentialRecord.CredentialDefinitionId);
                definitionsList.Add(new DefinitionRecord { Id = defResp.Id });
            }
            Schemas.InsertRange(schemasList);
            CredDefinitions.InsertRange(definitionsList);
            
            //CredDefinitions = await _schemaService.ListCredentialDefinitionsAsync(context.Wallet);
            await base.InitializeAsync(navigationData);
        }

        public async Task RequestIdentityProof()
        {
            var context = await _agentContextProvider.GetContextAsync();
            //var connection = await _connectionService.GetAsync(context, _connectionRecord.Id);

            if (SelectedDefinition == null || SelectedSchema == null)
                return;

            var identityAttributes = new ProofAttributeInfo
            {
                Names = new string[] { "fist_name", "last_name" },
                Restrictions = new List<AttributeFilter>
                {
                    new AttributeFilter
                    {
                        SchemaId = SelectedSchema.Id,
                        CredentialDefinitionId = SelectedDefinition.Id
                    }
                }
            };

            
            var proofRequestObject = new ProofRequest
            {
                Name = "Identity Proof Request",
                Version = "3.0",
                Nonce = new BigInteger(Guid.NewGuid().ToByteArray()).ToString(),
                RequestedAttributes = new Dictionary<string, ProofAttributeInfo> 
                {
                    {$"identity_attrs_requirement", identityAttributes}                    
                },
                RequestedPredicates = null
            };

            var (request, _) = await _proofService.CreateRequestAsync(context, proofRequestObject, _connectionRecord.Id);
            await _messageService.SendAsync(context, request, _connectionRecord);
        }

        #region Bindable Command
        public ICommand SendRequestCommand => new Command(async () => await RequestIdentityProof() );
        #endregion

        #region Bindable Properties
        private RangeEnabledObservableCollection<SchemaRecord> _schemas;
        public RangeEnabledObservableCollection<SchemaRecord> Schemas
        {
            get => _schemas;
            set => this.RaiseAndSetIfChanged(ref _schemas, value);
        }

        private RangeEnabledObservableCollection<DefinitionRecord> _credDefinitions;
        public RangeEnabledObservableCollection<DefinitionRecord> CredDefinitions
        {
            get => _credDefinitions;
            set => this.RaiseAndSetIfChanged(ref _credDefinitions, value);
        }

        private SchemaRecord _selectedSchema;
        public SchemaRecord SelectedSchema
        {
            get => _selectedSchema;
            set => this.RaiseAndSetIfChanged(ref _selectedSchema, value);
        }

        private DefinitionRecord _selectedDefinition;
        public DefinitionRecord SelectedDefinition
        {
            get => _selectedDefinition;
            set => this.RaiseAndSetIfChanged(ref _selectedDefinition, value);
        }
        #endregion
    }
}
