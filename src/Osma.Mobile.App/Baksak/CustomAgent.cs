using Hyperledger.Aries.Agents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Osma.Mobile.App.Baksak
{
    public class CustomAgent : AgentBase
    {

        public CustomAgent(IServiceProvider provider) : base(provider)
        {
        }

        /// <summary>
        /// Configures the handlers.
        /// </summary>
        protected override void ConfigureHandlers()
        {
            //AddConnectionHandler();
            //AddBaksakConnectionHandler();
            AddCredentialHandler();
            AddProofHandler();
            AddDiscoveryHandler();
            AddBasicMessageHandler();
            AddForwardHandler();
            AddTrustPingHandler();
            AddHandler<BaksakConnectionHandler>();
        }

        //protected void AddBaksakConnectionHandler() => Handlers.Add(Provider.GetRequiredService<BaksakConnectionHandler>());
        
    }
}
