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
    using System.Collections.Generic;
    using Hypertable.Persistence.Collections;

    /// <summary>
    ///     The entity scan result.
    /// </summary>
    internal sealed class EntityScanResult : EntitySpec
    {
        #region Fields

        /// <summary>
        ///     The result collection.
        /// </summary>
        private readonly ChunkedCollection<object> collection = new ChunkedCollection<object>();

        /// <summary>
        ///     The entity sink, receives the entities fetched.
        /// </summary>
        private readonly Action<object> valueSink;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityScanResult" /> class.
        /// </summary>
        /// <param name="entityReference">
        ///     The entity reference.
        /// </param>
        internal EntityScanResult(EntityReference entityReference)
            : base(entityReference, null)
        {
            this.collection = new ChunkedCollection<object>();
            this.valueSink = v =>
            {
                lock (this.collection.SyncRoot)
                {
                    this.collection.Add(v);
                }
            };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityScanResult" /> class.
        /// </summary>
        /// <param name="entityReference">
        ///     The entity reference.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        internal EntityScanResult(EntityReference entityReference, Action<object> entitySink)
            : base(entityReference, null)
        {
            this.valueSink = entitySink;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the values.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        internal ICollection<object> Values => this.collection;

        /// <summary>
        ///     Gets the value sink.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        internal Action<object> ValueSink => this.valueSink;

        #endregion
    }
}