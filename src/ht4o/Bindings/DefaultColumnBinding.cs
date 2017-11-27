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
    using Hypertable.Persistence.Attributes;
    using Hypertable.Persistence.Extensions;

    /// <summary>
    ///     The default column binding.
    /// </summary>
    internal sealed class DefaultColumnBinding : IColumnBinding
    {
        #region Fields

        /// <summary>
        ///     The default column family.
        /// </summary>
        private readonly string defaultColumnFamily;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultColumnBinding" /> class.
        /// </summary>
        /// <param name="type">
        ///     The entity type.
        /// </param>
        /// <param name="defaultColumnFamily">
        ///     The default column family.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="type" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="type" /> is typeof(object).
        /// </exception>
        private DefaultColumnBinding(Type type, string defaultColumnFamily)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type == typeof(object))
            {
                throw new ArgumentException(@"typeof(object) is not a valid column binding type", nameof(type));
            }

            if (type.IsInterface)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture,
                        @"interface type {0} is not a valid column binding type", type), nameof(type));
            }

            this.defaultColumnFamily = defaultColumnFamily;

            var entityAttribute = ReflectionExtensions.GetAttribute<EntityAttribute>(type);
            if (entityAttribute != null)
            {
                this.ColumnFamily = entityAttribute.ColumnFamily;
                this.ColumnQualifier = entityAttribute.ColumnQualifier;
            }

            if (type.IsAbstract)
            {
                if (string.IsNullOrEmpty(this.ColumnFamily))
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture,
                            @"abstract type {0} requires a column family binding", type), nameof(type));
                }

                if (!string.IsNullOrEmpty(this.ColumnQualifier))
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture,
                            @"abstract type {0} may not bind to a column qualifier", type), nameof(type));
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the column family.
        /// </summary>
        /// <value>
        ///     The column family.
        /// </value>
        public string ColumnFamily { get; private set; }

        /// <summary>
        ///     Gets the column qualifier.
        /// </summary>
        /// <value>
        ///     The column qualifier.
        /// </value>
        public string ColumnQualifier { get; }

        /// <summary>
        ///     Gets a value indicating whether the binding is complete.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the binding is complete, otherwise <c>false</c>.
        /// </value>
        public bool IsComplete => !string.IsNullOrEmpty(this.ColumnFamily);

        #endregion

        #region Methods

        /// <summary>
        ///     Creates a new instance of the <see cref="DefaultColumnBinding" /> class.
        /// </summary>
        /// <param name="type">
        ///     The entity type.
        /// </param>
        /// <param name="defaultColumnFamily">
        ///     The default column family.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If the <paramref name="type" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If the <paramref name="type" /> is typeof(object).
        /// </exception>
        /// <returns>
        ///     The <see cref="DefaultColumnBinding" />.
        /// </returns>
        internal static DefaultColumnBinding Create(Type type, string defaultColumnFamily)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type == typeof(object))
            {
                return null;
            }

            if (type.IsAbstract)
            {
                var entityAttribute = ReflectionExtensions.GetAttribute<EntityAttribute>(type);
                if (string.IsNullOrEmpty(entityAttribute?.ColumnFamily))
                {
                    return null;
                }
            }

            return new DefaultColumnBinding(type, defaultColumnFamily);
        }

        /// <summary>
        ///     Merge in binding elements from the type specified.
        /// </summary>
        /// <param name="type">
        ///     The entity type.
        /// </param>
        internal void Merge(Type type)
        {
            if (type != null && type != typeof(object))
            {
                var entityAttribute = ReflectionExtensions.GetAttribute<EntityAttribute>(type);
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