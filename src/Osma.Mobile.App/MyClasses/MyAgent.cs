using Hyperledger.Aries.Agents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Osma.Mobile.App.MyClasses
{
    public class MyAgent : AgentBase
    {

        public MyAgent(IServiceProvider provider) : base(provider)
        {
        }


        protected override void ConfigureHandlers()
        {
            AddConnectionHandler();
            AddCredentialHandler();
            AddProofHandler();
            AddDiscoveryHandler();
            AddBasicMessageHandler();
            AddForwardHandler();
            AddTrustPingHandler();
        }
    }
}
