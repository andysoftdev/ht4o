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

namespace Hypertable.Persistence.Bindings
{
    using System;
    using Hypertable.Persistence.Reflection;

    /// <summary>
    ///     The key property key binding.
    /// </summary>
    internal sealed class KeyPropertyKeyBinding : InspectedPropertyKeyBinding
    {
        #region Fields

        /// <summary>
        ///     The getter function.
        /// </summary>
        private readonly Func<object, object> get;

        /// <summary>
        ///     The setter method.
        /// </summary>
        private readonly Action<object, object> set;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyPropertyKeyBinding" /> class.
        /// </summary>
        /// <param name="inspectedProperty">
        ///     The inspected property.
        /// </param>
        /// <param name="columnBinding">
        ///     The column binding.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="inspectedProperty" /> does not have an entity key.
        /// </exception>
        internal KeyPropertyKeyBinding(InspectedProperty inspectedProperty, IColumnBinding columnBinding)
            : base(inspectedProperty, columnBinding)
        {
            if (!inspectedProperty.HasKey)
            {
                throw new ArgumentException("Invalid property, missing entity key", nameof(inspectedProperty));
            }

            this.get = inspectedProperty.Getter;
            this.set = inspectedProperty.Setter;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Creates a database key for the entity specified.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The database key.
        /// </returns>
        public override Key CreateKey(object entity)
        {
            Key key;
            var obj = this.get(entity);
            if (obj == null)
            {
                key = new Key();
                this.set(entity, key);
            }
            else
            {
                key = (Key) obj;
            }

            return this.GenerateKey(key, entity.GetType());
        }

        /// <summary>
        ///     Gets the database key from the entity specified.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The database key.
        /// </returns>
        public override Key KeyFromEntity(object entity)
        {
            return this.get(entity) as Key;
        }

        /// <summary>
        ///     Updates the entity using the database key specified.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="key">
        ///     The database key.
        /// </param>
        public override void SetKey(object entity, Key key)
        {
            this.set(entity, key);
        }

        #endregion
    }
}