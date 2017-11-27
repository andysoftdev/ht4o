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

namespace Hypertable.Persistence.Collections.Concurrent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     The concurrent type dictionary.
    /// </summary>
    /// <typeparam name="TValue">
    ///     The value type.
    /// </typeparam>
    internal sealed class ConcurrentTypeDictionary<TValue> : ConcurrentDictionary<Type, TValue>
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentTypeDictionary{TValue}" /> class.
        /// </summary>
        internal ConcurrentTypeDictionary()
            : base(256)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentTypeDictionary{TValue}" /> class.
        /// </summary>
        /// <param name="other">
        ///     The other.
        /// </param>
        internal ConcurrentTypeDictionary(ConcurrentTypeDictionary<TValue> other)
            : base(other)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConcurrentTypeDictionary{TValue}" /> class.
        /// </summary>
        /// <param name="collection">
        ///     The collection.
        /// </param>
        internal ConcurrentTypeDictionary(IEnumerable<KeyValuePair<Type, TValue>> collection)
            : base(collection)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Get the value for the type specified.
        /// </summary>
        /// <param name="type">
        ///     The type to lookup.
        /// </param>
        /// <returns>
        ///     The value found or default(TValue).
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal TValue GetValue(Type type)
        {
            TValue value;
            return this.TryGetValue(type, out value) ? value : default(TValue);
        }

        /// <summary>
        ///     Removes all types specified from the dictionary.
        /// </summary>
        /// <param name="types">
        ///     The types to remove.
        /// </param>
        /// <returns>
        ///     The number of types removed successfully.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="types" /> is null.
        /// </exception>
        internal int Remove(IEnumerable<Type> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            TValue value;
            return types.ToList().Count(key => TryRemove(key, out value)); // ToList is required
        }

        #endregion
    }
}