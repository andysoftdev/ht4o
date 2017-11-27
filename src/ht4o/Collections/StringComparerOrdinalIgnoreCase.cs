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
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///     The string comparer.
    /// </summary>
    internal struct StringComparerOrdinalIgnoreCase : IEqualityComparer<string>
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Determines whether the specified strings are equal.
        /// </summary>
        /// <param name="x">
        ///     The first string of type <see cref="string" /> to compare.
        /// </param>
        /// <param name="y">
        ///     The second string of type <see cref="string" /> to compare.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the specified strings are reference equal, otherwise <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Returns the runtime hash code for the specified string.
        /// </summary>
        /// <param name="obj">
        ///     The <see cref="string" /> for which a hash code is to be returned.
        /// </param>
        /// <returns>
        ///     A runtime hash code for the specified string.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}