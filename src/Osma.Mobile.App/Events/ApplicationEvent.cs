using System;
using System.Collections.Generic;
using System.Text;

namespace Osma.Mobile.App.Events
{
    public enum ApplicationEventType
    {
        ConnectionsUpdated,
        CredentialsUpdated,
        RefreshProofRequests,        
        ProvisioningRecordUpdated        
    }

    public class ApplicationEvent
    {
        public ApplicationEventType Type { get; set; }
    }
}
