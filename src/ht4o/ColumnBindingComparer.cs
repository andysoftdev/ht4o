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
namespace Hypertable.Persistence
{
    using System;
    using System.Collections.Generic;

    using Hypertable;

    /// <summary>
    /// The column binding comparer.
    /// </summary>
    public sealed class ColumnBindingComparer : IEqualityComparer<IColumnBinding>
    {
        #region Public Methods and Operators

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">
        /// The first object of type <see cref="IColumnBinding"/> to compare.
        /// </param>
        /// <param name="y">
        /// The second object of type <see cref="IColumnBinding"/> to compare.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified objects are equal, otherwise <c>false</c>.
        /// </returns>
        public bool Equals(IColumnBinding x, IColumnBinding y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return string.Equals(x.ColumnFamily, y.ColumnFamily) && string.Equals(x.ColumnQualifier, y.ColumnQualifier);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="IColumnBinding"/> for which a hash code is to be returned.
        /// </param>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.
        /// </exception>
        public int GetHashCode(IColumnBinding obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var hashCode = 17 + obj.ColumnFamily.GetHashCode();
            if (obj.ColumnQualifier != null)
            {
                hashCode = (29 * hashCode) + obj.ColumnQualifier.GetHashCode();
            }

            return hashCode;
        }

        #endregion
    }
}