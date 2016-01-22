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
    using System.Globalization;

    using Hypertable;

    /// <summary>
    /// The partial key binding.
    /// </summary>
    public abstract class PartialKeyBinding : IKeyBinding
    {
        #region Fields

        /// <summary>
        /// The entity type.
        /// </summary>
        private readonly Type entityType;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialKeyBinding"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityType"/> is null.
        /// </exception>
        protected PartialKeyBinding(Type entityType)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }

            this.entityType = entityType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialKeyBinding"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="columnBinding">
        /// The column binding.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="entityType"/> is null.
        /// </exception>
        protected PartialKeyBinding(Type entityType, IColumnBinding columnBinding)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException("entityType");
            }

            this.entityType = entityType;
            this.ColumnBinding = columnBinding;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <value>
        /// The entity type.
        /// </value>
        public Type EntityType
        {
            get
            {
                return this.entityType;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the column binding.
        /// </summary>
        /// <value>
        /// The column binding.
        /// </value>
        internal IColumnBinding ColumnBinding { get; set; }

        /// <summary>
        /// Gets or sets the timestamp action.
        /// </summary>
        /// <value>
        /// The timestamp action.
        /// </value>
        internal Action<object, object> TimestampAction { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Creates a database key for the entity specified.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// The database key.
        /// </returns>
        public abstract Key CreateKey(object entity);

        /// <summary>
        /// Gets the database key from the entity specified.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// The database key.
        /// </returns>
        public abstract Key KeyFromEntity(object entity);

        /// <summary>
        /// Gets the database key from the value specified.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The database key.
        /// </returns>
        public virtual Key KeyFromValue(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var key = value as Key;
            if (key != null)
            {
                return key;
            }

            if (this.entityType.IsInstanceOfType(value))
            {
                return this.KeyFromEntity(value);
            }

            return this.Merge(new Key((string)Convert.ChangeType(value, typeof(string), CultureInfo.CurrentCulture)));
        }

        /// <summary>
        /// Updates the entity using the database key specified.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="key">
        /// The database key.
        /// </param>
        public abstract void SetKey(object entity, Key key);

        #endregion

        #region Methods

        /// <summary>
        /// Generates a new key.
        /// </summary>
        /// <param name="key">
        /// The key to update.
        /// </param>
        /// <returns>
        /// The updated key.
        /// </returns>
        protected Key GenerateKey(Key key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            key.Row = Key.Generate();
            return this.Merge(key);
        }

        /// <summary>
        /// Merge in the column details.
        /// </summary>
        /// <param name="key">
        /// The to merge in key the column details.
        /// </param>
        /// <returns>
        /// The merged key.
        /// </returns>
        protected Key Merge(Key key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            var columnBinding = this.ColumnBinding;
            if (columnBinding != null)
            {
                key.ColumnFamily = columnBinding.ColumnFamily;
                key.ColumnQualifier = columnBinding.ColumnQualifier;
            }

            return key;
        }

        /// <summary>
        /// Executes the timestamp action for the entity and database key specified.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="key">
        /// The database key.
        /// </param>
        protected void Timestamp(object entity, Key key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (this.TimestampAction != null)
            {
                this.TimestampAction(entity, key.DateTime);
            }
        }

        #endregion
    }
}