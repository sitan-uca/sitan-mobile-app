using Hyperledger.Aries.Features.BasicMessage;
using Hyperledger.Aries.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Osma.Mobile.App.Extensions
{
    class RecordComparator : IEqualityComparer<RecordBase>
    {
        public bool Equals(RecordBase x, RecordBase y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(RecordBase obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
