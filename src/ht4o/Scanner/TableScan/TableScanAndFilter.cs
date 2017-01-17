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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Hypertable;

    //// TODO scan&filter depends on table row count vs query row count
    //// scan hints, track recent row count? Check if switch ScanAndFilter off....

    /// <summary>
    /// The table scan and filter.
    /// </summary>
    /// <typeparam name="T">
    /// The dictionary type.
    /// </typeparam>
    internal abstract class TableScanAndFilter<T> : ITableScan
        where T : IDictionary<Key, EntityScanTarget>
    {
        #region Public Properties

        /// <summary>
        /// Gets the entity scan targets.
        /// </summary>
        /// <value>
        /// The entity scan targets.
        /// </value>
        public abstract IEnumerable<EntityScanTarget> EntityScanTargets { get; }

        /// <summary>
        /// Gets a value indicating whether is empty.
        /// </summary>
        /// <value>
        /// The is empty.
        /// </value>
        public abstract bool? IsEmpty { get; }

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
            var entityScanTarget = entitySpec as EntityScanTarget;
            if (entityScanTarget == null)
            {
                throw new ArgumentException(@"EntitySpec of type EntityScanTarget expected", "entitySpec");
            }

            EntityScanTarget entityScanTargetExisting;
            if (!this.GetOrAdd(entityScanTarget, out entityScanTargetExisting))
            {
                entityScanTargetExisting.AddScanTargetRef(entityScanTarget);
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
            var scanSpec = new ScanSpec { MaxVersions = 1, ScanAndFilter = true };
            foreach (var entityScanTarget in this.EntityScanTargets)
            {
                var key = entityScanTarget.Key;
                if (!string.IsNullOrEmpty(key.ColumnFamily))
                {
                    if (key.ColumnQualifier == null)
                    {
                        scanSpec.AddColumn(key.ColumnFamily);
                    }
                    else
                    {
                        var sb = new StringBuilder();
                        sb.Append(key.ColumnFamily);
                        sb.Append(":");
                        sb.Append(key.ColumnQualifier);
                        scanSpec.AddColumn(sb.ToString());
                    }
                }

                scanSpec.AddRow(key.Row);
            }

            return scanSpec;
        }

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
        public abstract bool TryRemoveScanTarget(Key key, out EntityScanTarget entityScanTarget);

        #endregion

        #region Methods

        /// <summary>
        /// Gets or adds an entity scan target.
        /// </summary>
        /// <param name="entityScanTarget">
        /// The entity scan target.
        /// </param>
        /// <param name="entityScanTargetExisting">
        /// The existing entity scan target.
        /// </param>
        /// <returns>
        /// <c>true</c> if a scan target has been added, otherwise <c>false</c>.
        /// </returns>
        protected abstract bool GetOrAdd(EntityScanTarget entityScanTarget, out EntityScanTarget entityScanTargetExisting);

        #endregion
    }

    /// <summary>
    /// The table scan and filter.
    /// </summary>
    internal sealed class TableScanAndFilter : TableScanAndFilter<Dictionary<Key, EntityScanTarget>>
    {
        #region Fields

        /// <summary>
        /// The entity keys.
        /// </summary>
        private readonly IDictionary<Key, EntityScanTarget> keys = new Dictionary<Key, EntityScanTarget>(new KeyComparer());
        
        /// <summary>
        /// The unqualified entity keys.
        /// </summary>
        private readonly IDictionary<Key, EntityScanTarget> unqualifiedKeys = new Dictionary<Key, EntityScanTarget>(new RowComparer());

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the entity scan targets.
        /// </summary>
        /// <value>
        /// The entity scan targets.
        /// </value>
        public override IEnumerable<EntityScanTarget> EntityScanTargets
        {
            get
            {
                return this.keys.Values.Concat(this.unqualifiedKeys.Values);
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is something to scan or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if there is something to scan, otherwise <c>false</c>.
        /// </value>
        public override bool? IsEmpty
        {
            get
            {
                if (this.unqualifiedKeys.Count == 0)
                {
                    return this.keys.Count == 0;
                }

                return null;
            }
        }

        #endregion

        #region Public Methods and Operators

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
        public override bool TryRemoveScanTarget(Key key, out EntityScanTarget entityScanTarget)
        {
            if (this.keys.TryGetValue(key, out entityScanTarget))
            {
                this.keys.Remove(key);
                return true;
            }

            return this.unqualifiedKeys.TryGetValue(key, out entityScanTarget);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets or adds an entity scan target.
        /// </summary>
        /// <param name="entityScanTarget">
        /// The entity scan target.
        /// </param>
        /// <param name="entityScanTargetExisting">
        /// The existing entity scan target.
        /// </param>
        /// <returns>
        /// <c>true</c> if a scan target has been added, otherwise <c>false</c>.
        /// </returns>
        protected override bool GetOrAdd(EntityScanTarget entityScanTarget, out EntityScanTarget entityScanTargetExisting)
        {
            if (entityScanTarget == null)
            {
                throw new ArgumentNullException("entityScanTarget");
            }

            var d = string.IsNullOrEmpty(entityScanTarget.Key.ColumnFamily) ? this.unqualifiedKeys : this.keys;
            if (d.TryGetValue(entityScanTarget.Key, out entityScanTargetExisting))
            {
                return false;
            }

            d.Add(entityScanTarget.Key, entityScanTarget);
            return true;
        }

        #endregion
    }

    /// <summary>
    /// The concurrent table scan and filter.
    /// </summary>
    internal sealed class ConcurrentTableScanAndFilter : TableScanAndFilter<ConcurrentDictionary<Key, EntityScanTarget>>
    {
        #region Fields

        /// <summary>
        /// The entity keys.
        /// </summary>
        private readonly ConcurrentDictionary<Key, EntityScanTarget> keys = new ConcurrentDictionary<Key, EntityScanTarget>(new KeyComparer());

        /// <summary>
        /// The unqualified entity keys.
        /// </summary>
        private readonly ConcurrentDictionary<Key, EntityScanTarget> unqualifiedKeys = new ConcurrentDictionary<Key, EntityScanTarget>(new RowComparer());

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the entity scan targets.
        /// </summary>
        /// <value>
        /// The entity scan targets.
        /// </value>
        public override IEnumerable<EntityScanTarget> EntityScanTargets
        {
            get
            {
                return this.keys.Values.Concat(this.unqualifiedKeys.Values);
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is something to scan or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if there is something to scan, otherwise <c>false</c>.
        /// </value>
        public override bool? IsEmpty
        {
            get
            {
                if (this.unqualifiedKeys.Count == 0)
                {
                    return this.keys.Count == 0;
                }

                return null;
            }
        }

        #endregion

        #region Public Methods and Operators

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
        public override bool TryRemoveScanTarget(Key key, out EntityScanTarget entityScanTarget)
        {
            if (this.keys.TryRemove(key, out entityScanTarget))
            {
                return true;
            }

            return this.unqualifiedKeys.TryGetValue(key, out entityScanTarget);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets or adds an entity scan target.
        /// </summary>
        /// <param name="entityScanTarget">
        /// The entity scan target.
        /// </param>
        /// <param name="entityScanTargetExisting">
        /// The existing entity scan target.
        /// </param>
        /// <returns>
        /// <c>true</c> if a scan target has been added, otherwise <c>false</c>.
        /// </returns>
        protected override bool GetOrAdd(EntityScanTarget entityScanTarget, out EntityScanTarget entityScanTargetExisting)
        {
            if (entityScanTarget == null)
            {
                throw new ArgumentNullException("entityScanTarget");
            }

            var d = string.IsNullOrEmpty(entityScanTarget.Key.ColumnFamily) ? this.unqualifiedKeys : this.keys;

            var added = false;
            entityScanTargetExisting = d.GetOrAdd(
                entityScanTarget.Key, 
                key =>
                    {
                        added = true;
                        return entityScanTarget;
                    });

            return added;
        }

        #endregion
    }

    /// <summary>
    /// The async table scan and filter.
    /// </summary>
    internal sealed class LockedTableScanAndFilter : TableScanAndFilter<Dictionary<Key, EntityScanTarget>>
    {
        #region Fields

        /// <summary>
        /// The entity keys.
        /// </summary>
        private readonly IDictionary<Key, EntityScanTarget> keys = new Dictionary<Key, EntityScanTarget>(new KeyComparer());

        /// <summary>
        /// The unqualified entity keys.
        /// </summary>
        private readonly IDictionary<Key, EntityScanTarget> unqualifiedKeys = new Dictionary<Key, EntityScanTarget>(new RowComparer());

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the entity scan targets.
        /// </summary>
        /// <value>
        /// The entity scan targets.
        /// </value>
        public override IEnumerable<EntityScanTarget> EntityScanTargets
        {
            get
            {
                lock (this.keys)
                {
                    return this.keys.Values.Concat(this.unqualifiedKeys.Values).ToList();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is something to scan or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if there is something to scan, otherwise <c>false</c>.
        /// </value>
        public override bool? IsEmpty
        {
            get
            {
                lock (this.keys)
                {
                    if (this.unqualifiedKeys.Count == 0)
                    {
                        return this.keys.Count == 0;
                    }

                    return null;
                }
            }
        }

        #endregion

        #region Public Methods and Operators

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
        public override bool TryRemoveScanTarget(Key key, out EntityScanTarget entityScanTarget)
        {
            lock (this.keys)
            {
                if (this.keys.TryGetValue(key, out entityScanTarget))
                {
                    this.keys.Remove(key);
                    return true;
                }

                return this.unqualifiedKeys.TryGetValue(key, out entityScanTarget);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets or adds an entity scan target.
        /// </summary>
        /// <param name="entityScanTarget">
        /// The entity scan target.
        /// </param>
        /// <param name="entityScanTargetExisting">
        /// The existing entity scan target.
        /// </param>
        /// <returns>
        /// <c>true</c> if a scan target has been added, otherwise <c>false</c>.
        /// </returns>
        protected override bool GetOrAdd(EntityScanTarget entityScanTarget, out EntityScanTarget entityScanTargetExisting)
        {
            if (entityScanTarget == null)
            {
                throw new ArgumentNullException("entityScanTarget");
            }

            var d = string.IsNullOrEmpty(entityScanTarget.Key.ColumnFamily) ? this.unqualifiedKeys : this.keys;

            lock (this.keys)
            {
                if (d.TryGetValue(entityScanTarget.Key, out entityScanTargetExisting))
                {
                    return false;
                }

                d.Add(entityScanTarget.Key, entityScanTarget);
                return true;
            }
        }

        #endregion
    }
}