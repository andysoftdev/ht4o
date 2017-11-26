/** -*- C# -*-
 * Copyright (C) 2010-2016 Thalmann Software & Consulting, http://www.softdev.ch
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
namespace Hypertable.Persistence.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a collection of key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">
    /// The key type.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    internal sealed class Map<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : IEquatable<TKey>
    {
        #region Fields

        /// <summary>
        /// The inner dictionary.
        /// </summary>
        private readonly FastDictionary<TKey, TValue> dictionary = new FastDictionary<TKey, TValue>(256);

        /// <summary>
        /// The recent key.
        /// </summary>
        private TKey recentKey;

        /// <summary>
        /// The recent value.
        /// </summary>
        private TValue recentValue;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns an enumerator that iterates through the map.
        /// </summary>
        /// <returns>
        /// An enumerator for the map.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// Returns an enumerator that iterates through the map.
        /// </summary>
        /// <returns>
        /// An enumerator for the map.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a key/value pair to the map if the key does not already exist.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="valueFactory">
        /// The function used to generate a value for the key.
        /// </param>
        /// <returns>
        /// The value for the key.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key.Equals(this.recentKey))
            {
                return this.recentValue;
            }

            this.recentKey = key;
            return this.recentValue = this.dictionary.GetOrAdd(key, valueFactory);
        }

        #endregion
    }
}