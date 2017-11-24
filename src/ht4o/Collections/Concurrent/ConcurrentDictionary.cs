/** -*- C# -*-
 * Copyright (C) 2010-2017 Thalmann Software & Consulting, http://www.softdev.ch
 *
 * This file is part of ht4o.
 *
 * ht4o is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or any later version.
 *
 * Hypertable is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */

namespace Hypertable.Persistence.Collections.Concurrent
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class ConcurrentDictionary<TKey, TValue, TComparer> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue> where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Constructors and Destructors

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that is empty, has the default concurrency level, has the default initial capacity, and uses the default comparer
        ///     for the key type.
        /// </summary>
        public ConcurrentDictionary()
            : base(new TComparer()) {
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <param name="capacity">
        ///     The initial number of elements that the
        ///     <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.
        /// </param>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that is empty, has the default concurrency level and uses the default comparer for the key type.
        /// </summary>
        public ConcurrentDictionary(int capacity)
            : base(1, capacity, new TComparer()) {
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that is empty, has the specified concurrency level and capacity, and uses the default comparer for the key type.
        /// </summary>
        /// <param name="concurrencyLevel">
        ///     The estimated number of threads that will update the
        ///     <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> concurrently.
        /// </param>
        /// <param name="capacity">
        ///     The initial number of elements that the
        ///     <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="concurrencyLevel" /> is less than 1.-or-<paramref name="capacity" /> is less than 0.
        /// </exception>
        public ConcurrentDictionary(int concurrencyLevel, int capacity)
            : base(concurrencyLevel, capacity, new TComparer()) {
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that contains elements copied from the specified
        ///     <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" />, has the default concurrency level, has
        ///     the default initial capacity, and uses the default comparer for the key type.
        /// </summary>
        /// <param name="collection">
        ///     The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements
        ///     are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="collection" /> or any of its keys is a null reference (Nothing in Visual Basic)
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="collection" /> contains one or more duplicate keys.
        /// </exception>
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : base(collection, new TComparer()) {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Removes the key from the dictionary.
        /// </summary>
        /// <param name="key">
        ///     The key to remove.
        /// </param>
        /// <returns>
        ///     <c>true</c> if an object was removed successfully, otherwise <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Remove(TKey key) {
            TValue value;
            return TryRemove(key, out value);
        }

        #endregion
    }

    internal class ConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue, Collections.EqualityComparer<TKey>> {
        #region Constructors and Destructors

        public ConcurrentDictionary() {
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <param name="capacity">
        ///     The initial number of elements that the
        ///     <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.
        /// </param>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that is empty, has the default concurrency level and uses the default comparer for the key type.
        /// </summary>
        public ConcurrentDictionary(int capacity)
            : base(capacity) {
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity)
            : base(concurrencyLevel, capacity) {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : base(collection) {
        }

        #endregion
    }
}