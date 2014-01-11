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
namespace Hypertable.Persistence.Scanner
{
    using System;

    /// <summary>
    /// The entity array scan target.
    /// </summary>
    internal sealed class EntityArrayScanTarget : EntityScanTarget
    {
        #region Fields

        /// <summary>
        /// The array.
        /// </summary>
        private readonly Array array;

        /// <summary>
        /// The indexes.
        /// </summary>
        private readonly int[] indexes;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityArrayScanTarget"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="entitySpec">
        /// The entity spec.
        /// </param>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="indexes">
        /// The indexes.
        /// </param>
        internal EntityArrayScanTarget(Type entityType, EntitySpec entitySpec, Array array, int[] indexes)
            : base(entityType, entitySpec)
        {
            this.array = array;
            this.indexes = new int[indexes.Length];
            Array.Copy(indexes, this.indexes, indexes.Length);
            this.setter = this.Set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets an array element for the target specified.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private void Set(object target, object value)
        {
            lock (this.array)
            {
                this.array.SetValue(value, this.indexes);
            }
        }

        #endregion
    }
}