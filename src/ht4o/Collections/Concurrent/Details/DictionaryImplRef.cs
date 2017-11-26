// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

namespace Hypertable.Persistence.Collections.Concurrent.Details
{
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class DictionaryImplRef<TKey, TKeyStore, TValue, TComparer>
        : DictionaryImpl<TKey, TKey, TValue, TComparer>
        where TKey : class
        where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Constructors and Destructors

        internal DictionaryImplRef(int capacity, ConcurrentDictionary<TKey, TValue, TComparer> topDict)
            : base(capacity, topDict)
        {
        }

        private DictionaryImplRef(int capacity, DictionaryImplRef<TKey, TKeyStore, TValue, TComparer> other)
            : base(capacity, other)
        {
        }

        #endregion

        #region Methods

        protected override DictionaryImpl<TKey, TKey, TValue, TComparer> CreateNew(int capacity)
        {
            return new DictionaryImplRef<TKey, TKeyStore, TValue, TComparer>(capacity, this);
        }

        protected override bool TryClaimSlotForCopy(ref TKey entryKey, TKey key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        protected override bool TryClaimSlotForPut(ref TKey entryKey, TKey key)
        {
            return TryClaimSlot(ref entryKey, key);
        }

        private bool TryClaimSlot(ref TKey entryKey, TKey key)
        {
            var entryKeyValue = entryKey;
            if (entryKeyValue == null)
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, key, null);
                if (entryKeyValue == null)
                {
                    // claimed a new slot
                    allocatedSlotCount.Increment();
                    return true;
                }
            }

            return key == entryKeyValue || _keyComparer.Equals(key, entryKeyValue);
        }

        #endregion
    }
}