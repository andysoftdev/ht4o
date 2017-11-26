// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

namespace Hypertable.Persistence.Collections.Concurrent.Details
{
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class DictionaryImplInt<TValue, TComparer> : DictionaryImpl<int, int, TValue, TComparer>
        where TComparer : struct, IEqualityComparer<int>
    {
        #region Constructors and Destructors

        internal DictionaryImplInt(int capacity, ConcurrentDictionary<int, TValue, TComparer> topDict)
            : base(capacity, topDict)
        {
        }

        private DictionaryImplInt(int capacity, DictionaryImplInt<TValue, TComparer> other)
            : base(capacity, other)
        {
        }

        #endregion

        #region Methods

        protected override DictionaryImpl<int, int, TValue, TComparer> CreateNew(int capacity)
        {
            return new DictionaryImplInt<TValue, TComparer>(capacity, this);
        }

        protected override bool TryClaimSlotForCopy(ref int entryKey, int key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        protected override bool TryClaimSlotForPut(ref int entryKey, int key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref int entryKey, int key)
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