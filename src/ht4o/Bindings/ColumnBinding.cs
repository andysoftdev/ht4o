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
namespace Hypertable.Persistence.Bindings
{
    using System;

    /// <summary>
    /// The column binding.
    /// </summary>
    public sealed class ColumnBinding : IColumnBinding
    {
        #region Fields

        /// <summary>
        /// The column family.
        /// </summary>
        private readonly string columnFamily;

        /// <summary>
        /// The column qualifier.
        /// </summary>
        private readonly string columnQualifier;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBinding"/> class.
        /// </summary>
        /// <param name="columnFamily">
        /// The column family.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="columnFamily"/> is null or empty.
        /// </exception>
        public ColumnBinding(string columnFamily)
            : this(columnFamily, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBinding"/> class.
        /// </summary>
        /// <param name="columnFamily">
        /// The column family.
        /// </param>
        /// <param name="columnQualifier">
        /// The column qualifier.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="columnFamily"/> is null or empty.
        /// </exception>
        public ColumnBinding(string columnFamily, string columnQualifier)
        {
            if (string.IsNullOrEmpty(columnFamily))
            {
                throw new ArgumentNullException("columnFamily");
            }

            this.columnFamily = columnFamily;
            this.columnQualifier = columnQualifier;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the column family.
        /// </summary>
        /// <value>
        /// The column family.
        /// </value>
        public string ColumnFamily
        {
            get
            {
                return this.columnFamily;
            }
        }

        /// <summary>
        /// Gets the column qualifier.
        /// </summary>
        /// <value>
        /// The column qualifier.
        /// </value>
        public string ColumnQualifier
        {
            get
            {
                return this.columnQualifier;
            }
        }

        #endregion
    }
}