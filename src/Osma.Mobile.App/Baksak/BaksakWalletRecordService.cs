﻿using Hyperledger.Aries.Contracts;
using Hyperledger.Aries.Features.BasicMessage;
using Hyperledger.Aries.Storage;
using Hyperledger.Indy.WalletApi;
using Osma.Mobile.App.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Osma.Mobile.App.Baksak
{
    public class BaksakWalletRecordService : DefaultWalletRecordService
    {
        private readonly IEventAggregator _eventAggregator;

        public BaksakWalletRecordService(IEventAggregator eventAgregator) : base()
        {
            _eventAggregator = eventAgregator;
        }

        public override Task AddAsync<T>(Wallet wallet, T record)
        {
            Task task = base.AddAsync(wallet, record);
            if (record is BasicMessageRecord)
            {
                _eventAggregator.Publish(new ApplicationEvent() { Type = ApplicationEventType.WalletRecordsMessageUpdated });
            }

            return task;
        }
    }
}
