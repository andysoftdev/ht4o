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
    /// Compares two table bindings.
    /// </summary>
    internal sealed class TableBindingComparer : IEqualityComparer<ITableBinding>
    {
        #region Public Methods and Operators

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">
        /// The first object of type <see cref="ITableBinding"/> to compare.
        /// </param>
        /// <param name="y">
        /// The second object of type <see cref="ITableBinding"/> to compare.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified objects are equal, otherwise <c>false</c>.
        /// </returns>
        public bool Equals(ITableBinding x, ITableBinding y)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            return string.Equals(x.Namespace, y.Namespace) && string.Equals(x.TableName, y.TableName);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="tableBinding">
        /// The <see cref="ITableBinding"/> for which a hash code is to be returned.
        /// </param>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// The type of <paramref name="tableBinding"/> is a reference type and <paramref name="tableBinding"/> is null.
        /// </exception>
        public int GetHashCode(ITableBinding tableBinding)
        {
            if (tableBinding == null)
            {
                throw new ArgumentNullException("tableBinding");
            }

            var hashCode = tableBinding.TableName.GetHashCode();
            if (tableBinding.Namespace != null)
            {
                hashCode ^= 17 * tableBinding.Namespace.GetHashCode();
            }

            return hashCode;
        }

        #endregion
    }
}