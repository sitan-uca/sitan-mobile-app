using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.BasicMessage;
using Hyperledger.Aries.Features.IssueCredential;
using Hyperledger.Aries.Features.PresentProof;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Utils;
using Osma.Mobile.App.Views.Connections;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using static Hyperledger.Aries.Agents.MessageTypes;

namespace Osma.Mobile.App.Utilities
{
    public class ChatTemplateSelector : DataTemplateSelector
    {
        DataTemplate incomingDataTemplate;
        DataTemplate outgoingDataTemplate;
        DataTemplate outProofRecordDataTemplate;
        DataTemplate credentialRecordDataTemplate;
        DataTemplate incomingRecordDataTemplate;

        //public class Message
        //{
        //    public string Text { get; set; }
        //    public MessageDirection Direction { get; set; }
        //}

        public ChatTemplateSelector()
        {
            this.incomingDataTemplate = new DataTemplate(typeof(IncommingMsgViewCell));
            this.outgoingDataTemplate = new DataTemplate(typeof(OutgoingMsgViewCell));
            this.outProofRecordDataTemplate = new DataTemplate(typeof(OutProofRecordViewCell));
            this.incomingRecordDataTemplate = new DataTemplate(typeof(IncomingRecordViewCell));
            this.credentialRecordDataTemplate = new DataTemplate(typeof(CredentialRecordViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {            
            if (item is BasicMessageRecord) 
            {
                var messageVm = item as BasicMessageRecord;
                if (messageVm == null)
                    return null;

                return (messageVm.Direction == MessageDirection.Outgoing) ? outgoingDataTemplate : incomingDataTemplate;
            }
            else if (item is CredentialRecord)
            {
                return credentialRecordDataTemplate;
            }
            else if (item is ProofRecord)
            {
                var proofVm = item as ProofRecord;

                return (
                    proofVm.GetTag(TagConstants.Role).Equals(TagConstants.Requestor)
                    ) ? outProofRecordDataTemplate : incomingRecordDataTemplate;
            }

            return null;
        }
    }
}
