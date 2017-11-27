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
    using System.Globalization;
    using Hypertable.Persistence.Reflection;

    /// <summary>
    ///     The entity indexer scan target.
    /// </summary>
    internal sealed class EntityIndexerScanTarget : EntityScanTarget
    {
        #region Fields

        /// <summary>
        ///     The target collection.
        /// </summary>
        private readonly object collection;

        /// <summary>
        ///     The inspected enumerable.
        /// </summary>
        private readonly InspectedEnumerable inspectedEnumerable;

        /// <summary>
        ///     The position in the indexed collection.
        /// </summary>
        private readonly int pos;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityIndexerScanTarget" /> class.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="entitySpec">
        ///     The entity spec.
        /// </param>
        /// <param name="inspectedEnumerable">
        ///     The inspected enumerable.
        /// </param>
        /// <param name="collection">
        ///     The collection.
        /// </param>
        /// <param name="pos">
        ///     The position in the indexed collection.
        /// </param>
        /// <exception cref="PersistenceException">
        ///     If the <paramref name="inspectedEnumerable" /> does not have an indexer.
        /// </exception>
        internal EntityIndexerScanTarget(Type entityType, EntitySpec entitySpec,
            InspectedEnumerable inspectedEnumerable, object collection, int pos)
            : base(entityType, entitySpec)
        {
            if (!inspectedEnumerable.HasIndexer)
            {
                throw new PersistenceException(
                    string.Format(CultureInfo.InvariantCulture, @"Enumerable {0} has no index for type {1}.",
                        inspectedEnumerable.InspectedType, inspectedEnumerable.ElementType));
            }

            this.setter = this.Set;
            this.inspectedEnumerable = inspectedEnumerable;
            this.collection = collection;
            this.pos = pos;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sets the value at the target position.
        /// </summary>
        /// <param name="target">
        ///     The target.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        private void Set(object target, object value)
        {
            lock (this.inspectedEnumerable)
            {
                this.inspectedEnumerable.Indexer(this.collection, this.pos, value);
            }
        }

        #endregion
    }
}