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
namespace Hypertable.Persistence.Scanner
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The entity query scan target.
    /// </summary>
    internal sealed class EntityQueryScanTarget : EntityScanTarget
    {
        #region Fields

        /// <summary>
        /// The target collection.
        /// </summary>
        private readonly ICollection<object> collection;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQueryScanTarget"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="entitySpec">
        /// The entity spec.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        /// <param name="collection">
        /// The target collection.
        /// </param>
        internal EntityQueryScanTarget(Type entityType, EntitySpec entitySpec, Key key, ICollection<object> collection)
            : base(entityType, entitySpec, key)
        {
            this.setter = this.Add;
            this.collection = collection;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a value to the target collection.
        /// </summary>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private void Add(object target, object value)
        {
            lock (this.collection)
            {
                this.collection.Add(value);
            }
        }

        #endregion
    }
}