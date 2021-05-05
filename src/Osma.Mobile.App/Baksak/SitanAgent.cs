using Hyperledger.Aries.Agents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Osma.Mobile.App.Baksak
{
    public class SitanAgent : AgentBase
    {

        public SitanAgent(IServiceProvider provider) : base(provider)
        {
        }

        /// <summary>
        /// Configures the handlers.
        /// </summary>
        protected override void ConfigureHandlers()
        {
            //AddConnectionHandler();            
            AddCredentialHandler();
            AddProofHandler();
            AddDiscoveryHandler();
            AddBasicMessageHandler();
            AddForwardHandler();
            AddTrustPingHandler();
            AddHandler<BaksakConnectionHandler>();            
        }        
        
    }
}
