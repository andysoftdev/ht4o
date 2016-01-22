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
namespace Hypertable.Persistence
{
    /// <summary>
    /// The nullable extensions.
    /// </summary>
    internal static class NullableExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Gets a value indicating whether the value is false.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// <c>true</c> if value is <c>false</c>; otherwise <c>false</c>.
        /// </returns>
        public static bool IsFalse(this bool? value)
        {
            return value.HasValue && !value.Value;
        }

        /// <summary>
        /// Gets a value indicating whether the value is null or false.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// <c>true</c> if value is <c>null</c> or <c>false</c>; otherwise <c>false</c>.
        /// </returns>
        public static bool IsNullOrFalse(this bool? value)
        {
            return !value.HasValue || !value.Value;
        }

        /// <summary>
        /// Gets a value indicating whether the value is null or true.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// <c>true</c> if value is <c>null</c> or <c>true</c>; otherwise <c>false</c>.
        /// </returns>
        public static bool IsNullOrTrue(this bool? value)
        {
            return !value.HasValue || value.Value;
        }

        /// <summary>
        /// Gets a value indicating whether the value is true.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// <c>true</c> if value is <c>true</c>; otherwise <c>false</c>.
        /// </returns>
        public static bool IsTrue(this bool? value)
        {
            return value.HasValue && value.Value;
        }

        #endregion
    }
}