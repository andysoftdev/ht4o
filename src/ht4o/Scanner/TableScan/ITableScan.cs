/** -*- C# -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Collections.Generic;

    /// <summary>
    /// The table scan interface.
    /// </summary>
    internal interface ITableScan
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether there is something to scan or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if there is something to scan, otherwise <c>false</c>.
        /// </value>
        bool? IsEmpty { get; }

        /// <summary>
        /// Gets the entity scan targets.
        /// </summary>
        /// <value>
        /// The entity scan targets.
        /// </value>
        IEnumerable<EntityScanTarget> EntityScanTargets { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds an entity specification to the scan.
        /// </summary>
        /// <param name="entitySpec">
        /// The entity specification to add.
        /// </param>
        void Add(EntitySpec entitySpec);

        /// <summary>
        /// Creates the scan specification.
        /// </summary>
        /// <returns>
        /// The scan spec.
        /// </returns>
        ScanSpec CreateScanSpec();

        /// <summary>
        /// Try remove a scan target for the entity key specified.
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
        bool TryRemoveScanTarget(Key key, out EntityScanTarget entityScanTarget);

        #endregion
    }
}