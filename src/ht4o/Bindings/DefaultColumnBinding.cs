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
namespace Hypertable.Persistence.Bindings
{
    using System;

    using Hypertable.Persistence.Attributes;
    using Hypertable.Persistence.Reflection;

    /// <summary>
    /// The default column binding.
    /// </summary>
    internal sealed class DefaultColumnBinding : IColumnBinding
    {
        #region Fields

        /// <summary>
        /// The default column family.
        /// </summary>
        private readonly string defaultColumnFamily;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultColumnBinding"/> class.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <param name="defaultColumnFamily">
        /// The default column family.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="type"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="type"/> is typeof(object).
        /// </exception>
        internal DefaultColumnBinding(Type type, string defaultColumnFamily)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type == typeof(object))
            {
                throw new ArgumentException(@"typeof(object) is not a valid column binding type", "type");
            }

            this.defaultColumnFamily = defaultColumnFamily;

            var entityAttribute = type.GetAttribute<EntityAttribute>();
            if (entityAttribute != null)
            {
                this.ColumnFamily = entityAttribute.ColumnFamily;
                this.ColumnQualifier = entityAttribute.ColumnQualifier;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the column family.
        /// </summary>
        /// <value>
        /// The column family.
        /// </value>
        public string ColumnFamily { get; private set; }

        /// <summary>
        /// Gets the column qualifier.
        /// </summary>
        /// <value>
        /// The column qualifier.
        /// </value>
        public string ColumnQualifier { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the binding is complete.
        /// </summary>
        /// <value>
        /// <c>true</c> if the binding is complete, otherwise <c>false</c>.
        /// </value>
        public bool IsComplete
        {
            get
            {
                return !string.IsNullOrEmpty(this.ColumnFamily);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Merge in binding elements from the type specified.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        internal void Merge(Type type)
        {
            if (type != null && type != typeof(object))
            {
                var entityAttribute = type.GetAttribute<EntityAttribute>();
                if (entityAttribute != null)
                {
                    if (string.IsNullOrEmpty(this.ColumnFamily))
                    {
                        this.ColumnFamily = entityAttribute.ColumnFamily;
                    }
                }
            }
            else if (string.IsNullOrEmpty(this.ColumnFamily))
            {
                if (string.IsNullOrEmpty(this.defaultColumnFamily))
                {
                    throw new InvalidOperationException("Undefined default column family");
                }

                this.ColumnFamily = this.defaultColumnFamily;
            }
        }

        #endregion
    }
}