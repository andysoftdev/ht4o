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
namespace Hypertable.Persistence.Scanner.TableScan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Hypertable;

    /// <summary>
    /// The simple table scan.
    /// </summary>
    internal sealed class TableScan : ITableScan
    {
        #region Fields

        /// <summary>
        /// The scan spec.
        /// </summary>
        private readonly ScanSpec scanSpec;

        /// <summary>
        /// The synchronization object.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// The entity scan result.
        /// </summary>
        private EntityScanResult entityScanResult;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TableScan"/> class.
        /// </summary>
        /// <param name="scanSpec">
        /// The scan spec.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Id the <paramref name="scanSpec"/> is null.
        /// </exception>
        internal TableScan(ScanSpec scanSpec)
        {
            if (scanSpec == null)
            {
                throw new ArgumentNullException("scanSpec");
            }

            this.scanSpec = scanSpec;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the entity scan targets.
        /// </summary>
        /// <value>
        /// The entity scan targets.
        /// </value>
        public IEnumerable<EntityScanTarget> EntityScanTargets
        {
            get
            {
                return Enumerable.Empty<EntityScanTarget>();
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is something to scan or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if there is something to scan, otherwise <c>false</c>.
        /// </value>
        public bool? IsEmpty
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds an entity specification to the scan.
        /// </summary>
        /// <param name="entitySpec">
        /// The entity specification to add.
        /// </param>
        public void Add(EntitySpec entitySpec)
        {
            var esr = entitySpec as EntityScanResult;
            if (esr == null)
            {
                throw new ArgumentException(@"EntitySpec of type EntityScanResult expected", "entitySpec");
            }

            lock (this.syncRoot)
            {
                this.entityScanResult = esr;
            }
        }

        /// <summary>
        /// Creates the scan specification.
        /// </summary>
        /// <returns>
        /// The scan spec.
        /// </returns>
        public ScanSpec CreateScanSpec()
        {
            return this.scanSpec;
        }

        /// <summary>
        /// Try get a scan target for the entity key specified.
        /// </summary>
        /// <param name="key">
        /// The entity key.
        /// </param>
        /// <param name="entityScanTarget">
        /// The entity scan target.
        /// </param>
        /// <returns>
        /// <c>true</c> if a scan target as been found for the entity key specified, otherwise <c>false</c>.
        /// </returns>
        public bool TryRemoveScanTarget(Key key, out EntityScanTarget entityScanTarget)
        {
            lock (this.syncRoot)
            {
                entityScanTarget = new EntityQueryScanTarget(this.entityScanResult.EntityType, this.entityScanResult, key, this.entityScanResult.Values);
            }

            return true;
        }

        #endregion
    }
}