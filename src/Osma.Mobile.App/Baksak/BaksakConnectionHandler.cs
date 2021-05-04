﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hyperledger.Aries;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.DidExchange;
using Hyperledger.Aries.Utils;
using Osma.Mobile.App.Events;
using Osma.Mobile.App.Services;
using Osma.Mobile.App.Services.Interfaces;
using Osma.Mobile.App.ViewModels.Connections;
using Xamarin.Forms;

namespace Osma.Mobile.App.Baksak
{
    class BaksakConnectionHandler : IMessageHandler
    {

        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;

        private readonly WalletEventService _walletEventService = new WalletEventService();


        /// <summary>Initializes a new instance of the <see cref="BaksakConnectionHandler"/> class.</summary>
        /// <param name="connectionService">The connection service.</param>
        /// <param name="messageService">The message service.</param>
        public BaksakConnectionHandler(
            IEventAggregator eventAggregator,
            IConnectionService connectionService,
            INavigationService navigationService,
            IMessageService messageService)
        {
            _eventAggregator = eventAggregator;
            _connectionService = connectionService;
            _messageService = messageService;
            _navigationService = navigationService;
        }


        /// <inheritdoc />
        /// <summary>
        /// Gets the supported message types.
        /// </summary>
        /// <value>
        /// The supported message types.
        /// </value>
        public IEnumerable<MessageType> SupportedMessageTypes => new MessageType[]
        {
            MessageTypes.ConnectionInvitation,
            MessageTypes.ConnectionRequest,
            MessageTypes.ConnectionResponse,
            MessageTypesHttps.ConnectionInvitation,
            MessageTypesHttps.ConnectionRequest,
            MessageTypesHttps.ConnectionResponse
        };

        public async Task<AgentMessage> ProcessAsync(IAgentContext agentContext, UnpackedMessageContext messageContext)
        {
            switch (messageContext.GetMessageType())
            {
                case MessageTypesHttps.ConnectionInvitation:
                case MessageTypes.ConnectionInvitation:
                    {
                        var invitation = messageContext.GetMessage<ConnectionInvitationMessage>();
                        await _connectionService.CreateRequestAsync(agentContext, invitation);
                        return null;
                    }

                case MessageTypesHttps.ConnectionRequest:
                case MessageTypes.ConnectionRequest:
                    {
                        var request = messageContext.GetMessage<ConnectionRequestMessage>();
                        var connectionId = await _connectionService.ProcessRequestAsync(agentContext, request, messageContext.Connection);
                        messageContext.ContextRecord = messageContext.Connection;
                        
                        // Auto accept connection if set during invitation
                        if (messageContext.Connection.GetTag(TagConstants.AutoAcceptConnection) == "true")
                        {
                            var (message, record) = await _connectionService.CreateResponseAsync(agentContext, connectionId);
                            messageContext.ContextRecord = record;
                            return message;
                        }
                        else
                        {
                            //var (message, record) = await _connectionService.CreateResponseAsync(agentContext, connectionId);
                            //messageContext.ContextRecord = record;
                            _eventAggregator.Publish(new ApplicationEvent { Type = ApplicationEventType.ConnectionRequestReceived });                            
                            
                            List<object> data = new List<object>();
                            data.Add(request);
                            data.Add(connectionId);
                            //data.Add(record);
                            //new WalletEventService().ShowAcceptRequestDialog(data);
                            Device.BeginInvokeOnMainThread(async () => await _navigationService.NavigateToAsync<AcceptRequestViewModel>(data, NavigationType.Modal));                                                        
                            return null;
                        }
                        //return null;
                    }

                case MessageTypesHttps.ConnectionResponse:
                case MessageTypes.ConnectionResponse:
                    {
                        var response = messageContext.GetMessage<ConnectionResponseMessage>();
                        await _connectionService.ProcessResponseAsync(agentContext, response, messageContext.Connection);
                        messageContext.ContextRecord = messageContext.Connection;
                        return null;
                    }
                default:
                    throw new AriesFrameworkException(ErrorCode.InvalidMessage,
                        $"Unsupported message type {messageContext.GetMessageType()}");
            }
        }

            
    }
}
