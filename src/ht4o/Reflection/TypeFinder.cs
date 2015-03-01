/** -*- C# -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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
namespace Hypertable.Persistence.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The type finder.
    /// </summary>
    internal static class TypeFinder
    {
        #region Methods

        /// <summary>
        /// Gets the common base type for the types specified.
        /// </summary>
        /// <param name="types">
        /// The types.
        /// </param>
        /// <returns>
        /// The common base type.
        /// </returns>
        internal static Type GetCommonBaseType(IEnumerable<Type> types)
        {
            return GetCommonBaseType(types.ToArray());
        }

        /// <summary>
        /// Gets the common base type for the types specified.
        /// </summary>
        /// <param name="types">
        /// The types.
        /// </param>
        /// <returns>
        /// The common base type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="types"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="types"/> contains a null element.
        /// </exception>
        internal static Type GetCommonBaseType(Type[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }

            if (types.Length == 0)
            {
                return typeof(object);
            }

            var commonBaseClass = types[0];
            if (commonBaseClass == null)
            {
                throw new ArgumentException("types contains a null reference", "types");
            }

            for (var i = 1; i < types.Length; ++i)
            {
                var type = types[i];
                if (type == null)
                {
                    throw new ArgumentException("types contains a null reference", "types");
                }

                if (type.IsAssignableFrom(commonBaseClass))
                {
                    commonBaseClass = type;
                }
                else
                {
                    while (commonBaseClass != null && !commonBaseClass.IsAssignableFrom(type))
                    {
                        commonBaseClass = commonBaseClass.BaseType;
                    }
                }
            }

            return commonBaseClass;
        }

        #endregion
    }
}