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
namespace Hypertable.Persistence.Scanner
{
    using System;
    using System.Globalization;

    using Hypertable.Persistence.Reflection;

    /// <summary>
    /// The entity collection scan target.
    /// </summary>
    internal sealed class EntityCollectionScanTarget : EntityScanTarget
    {
        #region Fields

        /// <summary>
        /// The collection add action.
        /// </summary>
        private readonly Action<object, object> add;

        /// <summary>
        /// The target collection.
        /// </summary>
        private readonly object collection;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCollectionScanTarget"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="entitySpec">
        /// The entity specification.
        /// </param>
        /// <param name="inspectedEnumerable">
        /// The inspected enumerable.
        /// </param>
        /// <param name="collection">
        /// The target collection.
        /// </param>
        /// <exception cref="PersistenceException">
        /// If the <paramref name="inspectedEnumerable"/> does not have an add method.
        /// </exception>
        internal EntityCollectionScanTarget(Type entityType, EntitySpec entitySpec, InspectedEnumerable inspectedEnumerable, object collection)
            : base(entityType, entitySpec)
        {
            if (!inspectedEnumerable.HasAdd)
            {
                throw new PersistenceException(
                    string.Format(CultureInfo.InvariantCulture, @"Enumerable {0} has no void Add({1}) method.", inspectedEnumerable.InspectedType, inspectedEnumerable.ElementType));
            }

            this.setter = this.Add;
            this.add = inspectedEnumerable.Add;
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
                this.add(this.collection, value);
            }
        }

        #endregion
    }
}