// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

#define LOCKFREE
namespace Hypertable.Persistence.Collections.Concurrent
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

#if LOCKFREE

    internal class ConcurrentDictionary<TKey, TValue, TComparer> : Details.ConcurrentDictionary<TKey, TValue, TComparer>
        where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Constructors and Destructors

        public ConcurrentDictionary()
        {
        }

        public ConcurrentDictionary(int capacity)
            : base(capacity)
        {
        }

        public ConcurrentDictionary(ConcurrentDictionary<TKey, TValue, TComparer> other)
            : base(other)
        {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> src)
            : base(src)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <returns><c>true</c> if the value have been updated; otherwise <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddOrUpdate(TKey key, TValue value)
        {
            var added = true;
            base.AddOrUpdate(
                key,
                value,
                (t, v) =>
                {
                    added = false;
                    return value;
                });

            return added;
        }

        #endregion
    }

    internal class
        ConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue, Collections.EqualityComparer<TKey>>
    {
        #region Constructors and Destructors

        public ConcurrentDictionary()
        {
        }

        public ConcurrentDictionary(int capacity)
            : base(capacity)
        {
        }

        public ConcurrentDictionary(ConcurrentDictionary<TKey, TValue> other)
            : base(other)
        {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> src)
            : base(src)
        {
        }

        #endregion
    }

#else

    internal class ConcurrentDictionary<TKey, TValue, TComparer> : FastConcurrentDictionary<TKey, TValue, TComparer> where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Constructors and Destructors

        public ConcurrentDictionary() {
        }

        public ConcurrentDictionary(int capacity)
            : base(capacity) {
        }

        public ConcurrentDictionary(ConcurrentDictionary<TKey, TValue, TComparer> other)
            : base(other) {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> src)
            : base(src) {
        }

        #endregion
    }

    internal class ConcurrentDictionary<TKey, TValue> : FastConcurrentDictionary<TKey, TValue>
    {
        #region Constructors and Destructors

        public ConcurrentDictionary() {
        }

        public ConcurrentDictionary(int capacity)
            : base(capacity) {
        }

        public ConcurrentDictionary(ConcurrentDictionary<TKey, TValue> other)
            : base(other) {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> src)
            : base(src) {
        }

        #endregion
    }

#endif
}