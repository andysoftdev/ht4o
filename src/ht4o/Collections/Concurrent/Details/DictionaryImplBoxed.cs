// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

namespace Hypertable.Persistence.Collections.Concurrent.Details
{
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class DictionaryImplBoxed<TKey, TValue, TComparer>
        : DictionaryImpl<TKey, Boxed<TKey>, TValue, TComparer> where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Constructors and Destructors

        internal DictionaryImplBoxed(int capacity, ConcurrentDictionary<TKey, TValue, TComparer> topDict)
            : base(capacity, topDict)
        {
        }

        internal DictionaryImplBoxed(int capacity, DictionaryImplBoxed<TKey, TValue, TComparer> other)
            : base(capacity, other)
        {
        }

        #endregion

        #region Methods

        protected override DictionaryImpl<TKey, Boxed<TKey>, TValue, TComparer> CreateNew(int capacity)
        {
            return new DictionaryImplBoxed<TKey, TValue, TComparer>(capacity, this);
        }

        protected override bool TryClaimSlotForCopy(ref Boxed<TKey> entryKey, Boxed<TKey> key)
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

            return _keyComparer.Equals(key.Value, entryKey.Value);
        }

        protected override bool TryClaimSlotForPut(ref Boxed<TKey> entryKey, TKey key)
        {
            var entryKeyValue = entryKey;
            if (entryKeyValue == null)
            {
                entryKeyValue = Interlocked.CompareExchange(ref entryKey, new Boxed<TKey>(key), null);
                if (entryKeyValue == null)
                {
                    // claimed a new slot
                    allocatedSlotCount.Increment();
                    return true;
                }
            }

            return _keyComparer.Equals(key, entryKey.Value);
        }

        #endregion
    }

    internal class Boxed<T>
    {
        #region Fields

        public readonly T Value;

        #endregion

        #region Constructors and Destructors

        public Boxed(T key)
        {
            Value = key;
        }

        #endregion
    }
}