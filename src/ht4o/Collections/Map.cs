/** -*- C# -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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
        /// The dictionary.
        /// </summary>
        private readonly IDictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

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
        internal TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;
            if (key.Equals(this.recentKey))
            {
                return this.recentValue;
            }

            if (!this.dictionary.TryGetValue(key, out value))
            {
                this.dictionary.Add(key, value = valueFactory(key));
            }

            this.recentKey = key;
            this.recentValue = value;
            return value;
        }

        #endregion
    }
}