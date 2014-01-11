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
namespace Hypertable.Persistence.Attributes
{
    using System;

    /// <summary>
    /// The entity attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class EntityAttribute : Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAttribute"/> class.
        /// </summary>
        public EntityAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAttribute"/> class.
        /// </summary>
        /// <param name="tableName">
        /// The table name.
        /// </param>
        public EntityAttribute(string tableName)
        {
            this.TableName = tableName;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the column family.
        /// </summary>
        /// <value>
        /// The column family.
        /// </value>
        public string ColumnFamily { get; set; }

        /// <summary>
        /// Gets or sets the column qualifier.
        /// </summary>
        /// <value>
        /// The column qualifier.
        /// </value>
        public string ColumnQualifier { get; set; }

        /// <summary>
        /// Gets or sets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        /// <value>
        /// The table name.
        /// </value>
        public string TableName { get; private set; }

        #endregion
    }
}