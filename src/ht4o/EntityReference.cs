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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Hypertable.Persistence.Bindings;

    /// <summary>
    ///     The entity reference.
    /// </summary>
    internal sealed class EntityReference
    {
        #region Fields

        /// <summary>
        ///     The column binding.
        /// </summary>
        /// <remarks>
        ///     Might be null.
        /// </remarks>
        private readonly IColumnBinding columnBinding;

        /// <summary>
        ///     The entity type.
        /// </summary>
        private readonly Type entityType;

        /// <summary>
        ///     The key binding.
        /// </summary>
        private readonly IKeyBinding keyBinding;

        /// <summary>
        ///     The table binding.
        /// </summary>
        private readonly ITableBinding tableBinding;

        /// <summary>
        ///     The column family set.
        /// </summary>
        private ISet<string> columnFamilySet;

        /// <summary>
        ///     The column set.
        /// </summary>
        private ISet<string> columnSet;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityReference" /> class.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="tableBinding">
        ///     The table binding.
        /// </param>
        /// <param name="columnBinding">
        ///     The column binding.
        /// </param>
        /// <param name="keyBinding">
        ///     The key binding.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="entityType" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="tableBinding" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="keyBinding" /> is null.
        /// </exception>
        internal EntityReference(Type entityType, ITableBinding tableBinding, IColumnBinding columnBinding,
            IKeyBinding keyBinding)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (tableBinding == null)
            {
                throw new ArgumentNullException(nameof(tableBinding));
            }

            if (keyBinding == null)
            {
                throw new ArgumentNullException(nameof(keyBinding));
            }

            this.entityType = entityType;
            this.tableBinding = tableBinding;
            this.columnBinding = columnBinding;
            this.keyBinding = keyBinding;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityReference" /> class.
        /// </summary>
        /// <param name="entityReference">
        ///     The entity reference.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="entityReference" /> is null.
        /// </exception>
        internal EntityReference(EntityReference entityReference)
        {
            if (entityReference == null)
            {
                throw new ArgumentNullException(nameof(entityReference));
            }

            this.entityType = entityReference.entityType;
            this.tableBinding = entityReference.tableBinding;
            this.columnBinding = entityReference.columnBinding;
            this.keyBinding = entityReference.keyBinding;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the column binding.
        /// </summary>
        /// <value>
        ///     The column binding.
        /// </value>
        internal IColumnBinding ColumnBinding => this.columnBinding;

        /// <summary>
        ///     Gets column family set.
        /// </summary>
        /// <value>
        ///     The column family set.
        /// </value>
        internal ISet<string> ColumnFamilySet => this.columnFamilySet;

        /// <summary>
        ///     Gets the column set.
        /// </summary>
        /// <value>
        ///     The column set.
        /// </value>
        internal ISet<string> ColumnSet => this.columnSet;

        /// <summary>
        ///     Gets the entity type.
        /// </summary>
        /// <value>
        ///     The entity type.
        /// </value>
        internal Type EntityType => this.entityType;

        /// <summary>
        ///     Gets the key binding.
        /// </summary>
        /// <value>
        ///     The key binding.
        /// </value>
        internal IKeyBinding KeyBinding => this.keyBinding;

        /// <summary>
        ///     Gets the namespace.
        /// </summary>
        /// <value>
        ///     The namespace.
        /// </value>
        internal string Namespace => this.tableBinding.Namespace;

        /// <summary>
        ///     Gets the table binding.
        /// </summary>
        /// <value>
        ///     The table binding.
        /// </value>
        internal ITableBinding TableBinding => this.tableBinding;

        /// <summary>
        ///     Gets the table name.
        /// </summary>
        /// <value>
        ///     The table name.
        /// </value>
        internal string TableName => this.tableBinding.TableName;

        #endregion

        #region Methods

        /// <summary>
        ///     Establish the column set for this entity reference.
        /// </summary>
        /// <param name="establishColumns">
        ///     The establish columns function.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="establishColumns" /> is null.
        /// </exception>
        internal void EstablishColumnSets(Func<IEnumerable<string>> establishColumns)
        {
            if (establishColumns == null)
            {
                throw new ArgumentNullException(nameof(establishColumns));
            }

            if (this.columnSet == null)
            {
                this.columnSet = ScanSpec.DistictColumn(establishColumns(), out this.columnFamilySet);
            }
        }

        /// <summary>
        ///     Generates a valid database key for the entity specified.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     A valid database key.
        /// </returns>
        internal Key GenerateKey(object entity)
        {
            return this.VerifyKey(this.keyBinding.CreateKey(entity), true);
        }

        /// <summary>
        ///     Gets or creates a database key from the entity specified.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="generated">
        ///     <c>true</c> if the database key has been generated, otherwise <c>false</c>.
        /// </param>
        /// <returns>
        ///     A valid database key.
        /// </returns>
        internal Key GetKeyFromEntity(object entity, out bool generated)
        {
            var key = this.keyBinding.KeyFromEntity(entity);
            generated = key == null || string.IsNullOrEmpty(key.Row);
            return this.VerifyKey(generated ? this.keyBinding.CreateKey(entity) : key, true);
        }

        /// <summary>
        ///     Gets a database key from the object specified.
        /// </summary>
        /// <param name="entityOrKey">
        ///     The entity or key object.
        /// </param>
        /// <param name="verifyColumnFamily">
        ///     Verify the column family.
        /// </param>
        /// <returns>
        ///     A valid database key.
        /// </returns>
        /// <exception cref="PersistenceException">
        ///     If it's not possible to derive a valid key from <paramref name="entityOrKey" />.
        /// </exception>
        internal Key GetKeyFromObject(object entityOrKey, bool verifyColumnFamily)
        {
            Key entityKey;
            if (this.EntityType.IsInstanceOfType(entityOrKey))
            {
                entityKey = this.keyBinding.KeyFromEntity(entityOrKey);
                if (entityKey == null)
                {
                    throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                        @"Query key from entity {0} failed", entityOrKey));
                }
            }
            else
            {
                entityKey = this.keyBinding.KeyFromValue(entityOrKey);
                if (entityKey == null)
                {
                    throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                        @"Query key from value {0} failed", entityOrKey));
                }
            }

            return this.VerifyKey(entityKey, verifyColumnFamily);
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
        internal void SetKey(object entity, Key key)
        {
            this.keyBinding.SetKey(entity, this.VerifyKey(key, true));
        }

        /// <summary>
        ///     Validates the specified database key.
        /// </summary>
        /// <param name="key">
        ///     The database key to validate.
        /// </param>
        /// <param name="verifyColumnFamily">
        ///     Verify the column family.
        /// </param>
        /// <returns>
        ///     The validated database key.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="key" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If there is no row set for <paramref name="key" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If there is no column family set for <paramref name="key" />.
        /// </exception>
        private Key VerifyKey(Key key, bool verifyColumnFamily)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrEmpty(key.Row))
            {
                throw new ArgumentException(@"Row key has not been specified", nameof(key));
            }

            if (verifyColumnFamily && string.IsNullOrEmpty(key.ColumnFamily))
            {
                var partialKeyBinding = this.keyBinding as PartialKeyBinding;
                if (partialKeyBinding != null && partialKeyBinding.ColumnBinding == null)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                        @"Undefined column binding for type {0}", this.entityType));
                }

                throw new ArgumentException(@"Missing column family)", nameof(key));
            }

            return key;
        }

        #endregion
    }
}