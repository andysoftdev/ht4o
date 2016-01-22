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

    using Hypertable.Persistence.Attributes;
    using Hypertable.Persistence.Reflection;

    /// <summary>
    /// The default table binding.
    /// </summary>
    internal sealed class DefaultTableBinding : ITableBinding
    {
        #region Fields

        /// <summary>
        /// The initial type.
        /// </summary>
        private readonly Type initialType;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTableBinding"/> class.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="type"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="type"/> is typeof(object).
        /// </exception>
        internal DefaultTableBinding(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type == typeof(object))
            {
                throw new ArgumentException(@"typeof(object) is not a valid table binding type", "type");
            }

            this.initialType = type;
            this.Merge(type);
        }

        #endregion

        #region Public Properties

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
                return !string.IsNullOrEmpty(this.TableName) && !string.IsNullOrEmpty(this.Namespace);
            }
        }

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public string Namespace { get; private set; }

        /// <summary>
        /// Gets database table name.
        /// </summary>
        /// <value>
        /// The database table name.
        /// </value>
        public string TableName { get; private set; }

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
                    if (string.IsNullOrEmpty(this.Namespace))
                    {
                        this.Namespace = entityAttribute.Namespace;
                    }

                    if (string.IsNullOrEmpty(this.TableName))
                    {
                        this.TableName = entityAttribute.TableName;
                    }
                }
            }
            else if (string.IsNullOrEmpty(this.TableName))
            {
                this.TableName = this.initialType.Name;
            }
        }

        #endregion
    }
}