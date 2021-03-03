using Hyperledger.Aries.Features.BasicMessage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Osma.Mobile.App.Extensions
{
    class MessageRecordComparator : IEqualityComparer<BasicMessageRecord>
    {
        public bool Equals(BasicMessageRecord x, BasicMessageRecord y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(BasicMessageRecord obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
