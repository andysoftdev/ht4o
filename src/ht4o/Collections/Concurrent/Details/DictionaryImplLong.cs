// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.


namespace Hypertable.Persistence.Collections.Concurrent.Details
{
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class DictionaryImplLong<TValue, TComparer>
        : DictionaryImpl<long, long, TValue, TComparer> where TComparer : struct, IEqualityComparer<long>
    {
        #region Constructors and Destructors

        internal DictionaryImplLong(int capacity, ConcurrentDictionary<long, TValue, TComparer> topDict)
            : base(capacity, topDict)
        {
        }

        private DictionaryImplLong(int capacity, DictionaryImplLong<TValue, TComparer> other)
            : base(capacity, other)
        {
        }

        #endregion

        #region Methods

        protected override DictionaryImpl<long, long, TValue, TComparer> CreateNew(int capacity)
        {
            return new DictionaryImplLong<TValue, TComparer>(capacity, this);
        }

        protected override bool TryClaimSlotForCopy(ref long entryKey, long key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        protected override bool TryClaimSlotForPut(ref long entryKey, long key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref long entryKey, long key)
        {
            var entryKeyValue = entryKey;
            //zero keys are claimed via hash
            if ((entryKeyValue == 0) & (key != 0))
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, key, 0);
                if (entryKeyValue == 0)
                {
                    // claimed a new slot
                    allocatedSlotCount.Increment();
                    return true;
                }
            }

            return key == entryKeyValue || _keyComparer.Equals(key, entryKey);
        }

        #endregion
    }
}