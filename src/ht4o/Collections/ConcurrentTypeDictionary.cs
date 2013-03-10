/** -*- C# -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The concurrent type dictionary.
    /// </summary>
    /// <typeparam name="TValue">
    /// The value type.
    /// </typeparam>
    internal sealed class ConcurrentTypeDictionary<TValue> : ConcurrentDictionary<Type, TValue>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentTypeDictionary{TValue}"/> class.
        /// </summary>
        internal ConcurrentTypeDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentTypeDictionary{TValue}"/> class.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        internal ConcurrentTypeDictionary(ConcurrentTypeDictionary<TValue> other)
            : base(other)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds or updates a type/value pair.
        /// </summary>
        /// <param name="type">
        /// The type to be added.
        /// </param>
        /// <param name="value">
        /// The value to be added.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type/value pair was added to the dictionary successfully, <c>false</c> if the value has been updated.
        /// </returns>
        internal bool AddOrUpdate(Type type, TValue value)
        {
            var added = true;
            base.AddOrUpdate(
                type, 
                value, 
                (t, v) =>
                    {
                        added = false;
                        return value;
                    });

            return added;
        }

        /// <summary>
        /// Get the value for the type specified.
        /// </summary>
        /// <param name="type">
        /// The type to lookup.
        /// </param>
        /// <returns>
        /// The value found or default(TValue).
        /// </returns>
        internal TValue GetValue(Type type)
        {
            TValue value;
            if (!this.TryGetValue(type, out value))
            {
                return default(TValue);
            }

            return value;
        }

        /// <summary>
        /// Removes the type from the dictionary.
        /// </summary>
        /// <param name="type">
        /// The type to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> if an object was removed successfully, otherwise <c>false</c>.
        /// </returns>
        internal bool Remove(Type type)
        {
            TValue value;
            return this.TryRemove(type, out value);
        }

        /// <summary>
        /// Removes all types specified from the dictionary.
        /// </summary>
        /// <param name="types">
        /// The types to remove.
        /// </param>
        /// <returns>
        /// The number of types removed successfully.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="types"/> is null.
        /// </exception>
        internal int Remove(IEnumerable<Type> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

            TValue value;
            return types.ToList().Count(key => this.TryRemove(key, out value)); // ToList is required
        }

        #endregion
    }
}