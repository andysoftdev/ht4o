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
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hypertable.Persistence.Collections;
    using Hypertable.Persistence.Scanner.TableScan;

    /// <summary>
    ///     The entity scanner.
    /// </summary>
    internal sealed class EntityScanner
    {
        #region Fields

        /// <summary>
        ///     The processor count.
        /// </summary>
        private static int FetchTaskCount = Math.Max(Environment.ProcessorCount - 1, 7);

        /// <summary>
        ///     The entity context.
        /// </summary>
        private readonly EntityContext entityContext;

        /// <summary>
        ///     The synchronization object.
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        ///     Indicating whether to use an async table scanner or not.
        /// </summary>
        private readonly bool useAsyncTableScanner;

        /// <summary>
        ///     Indicating whether to use the internal cell buffer or not.
        /// </summary>
        private readonly bool useCellBuffer = true;

        /// <summary>
        ///     Indicating whether to use parallel deserialization or not.
        /// </summary>
        private readonly bool useParallelDeserialization;

        /// <summary>
        ///     The table scans.
        /// </summary>
        private Map<Pair<string>, ITableScan> tables;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityScanner" /> class.
        /// </summary>
        /// <param name="entityContext">
        ///     The entity context.
        /// </param>
        internal EntityScanner(EntityContext entityContext)
        {
            this.entityContext = entityContext;
            this.useAsyncTableScanner = entityContext.Configuration.UseAsyncTableScanner &&
                                        entityContext.HasFeature(ContextFeature.AsyncTableScanner);
            this.useParallelDeserialization = entityContext.Configuration.UseParallelDeserialization;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the entity context.
        /// </summary>
        /// <value>
        ///     The entity context.
        /// </value>
        internal EntityContext EntityContext => this.entityContext;

        /// <summary>
        ///     Gets a value indicating whether there is something to scan or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if there is something to scan, otherwise <c>false</c>.
        /// </value>
        internal bool IsEmpty
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.tables == null;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds an entity scan target to the entity scanner.
        /// </summary>
        /// <param name="entityScanTarget">
        ///     The entity scan target to add.
        /// </param>
        internal void Add(EntityScanTarget entityScanTarget)
        {
            lock (this.syncRoot)
            {
                if (this.tables == null)
                {
                    this.tables = new Map<Pair<string>, ITableScan>();
                }

                var tableScanSpec =
                    this.tables.GetOrAdd(
                        new Pair<string>(entityScanTarget.Namespace, entityScanTarget.TableName),
                        kvp => this.CreateTableScanAndFilter());
                tableScanSpec.Add(entityScanTarget);
            }
        }

        /// <summary>
        ///     Adds an entity specification to the scan.
        /// </summary>
        /// <param name="entitySpec">
        ///     The entity specification to add.
        /// </param>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        internal void Add(EntitySpec entitySpec, ScanSpec scanSpec)
        {
            lock (this.syncRoot)
            {
                if (this.tables == null)
                {
                    this.tables = new Map<Pair<string>, ITableScan>();
                }

                this.tables = new Map<Pair<string>, ITableScan>();

                var tableScanSpec = this.tables.GetOrAdd(
                    new Pair<string>(entitySpec.Namespace, entitySpec.TableName),
                    kvp => this.CreateTableScan(scanSpec));
                tableScanSpec.Add(entitySpec);
            }
        }

        /// <summary>
        ///     Fetches all the scan targets.
        /// </summary>
        /// <param name="tryGetFetchedEntity">
        ///     The try Get Fetched Entity.
        /// </param>
        /// <param name="entityFetched">
        ///     The entity fetched delegate.
        /// </param>
        internal void Fetch(TryGetFetchedEntity tryGetFetchedEntity, EntityFetched entityFetched) {
            IEnumerable<KeyValuePair<Pair<string>, ITableScan>> tablesToFetch;
            lock (this.syncRoot) {
                tablesToFetch = this.tables;
                this.tables = null;
            }

            foreach (var tableItem in tablesToFetch) {
                var tableScan = tableItem.Value;
                foreach (var entityScanTarget in tableScan.EntityScanTargets.ToList()) {
                    object entity;
                    if (tryGetFetchedEntity(entityScanTarget, out entity)) {
                        if (entity == null || entityScanTarget.EntityType.IsAssignableFrom(entity.GetType())) {
                            entityScanTarget.SetValue(entity);
                        }

                        EntityScanTarget removedEntityScanTarget;
                        tableScan.TryRemoveScanTarget(entityScanTarget.Key, out removedEntityScanTarget);
                    }
                }
            }

            tablesToFetch = tablesToFetch.Where(kv => kv.Value.IsEmpty.IsNullOrFalse()).ToList();

            var reviewScanSpec = this.entityContext.Configuration.ReviewScanSpec;

            ////TODO compare async vs sync on multi-core
            if (this.useAsyncTableScanner) {
                using (var asynResult = new AsyncResult(
                    (ctx, cells) => {
                        try {
                            var tableItem = (KeyValuePair<Pair<string>, ITableScan>)ctx.Param;

                            ////TODO check what's the fasted way to process the fetched cells on multi-core
                            if (cells.Count > 256) {
                                ParallelProcessFetchedCells(tableItem, cells, entityFetched);
                            }
                            else {
                                SequentialProcessFetchedCells(tableItem, cells, entityFetched);
                            }
                        }
                        catch (AggregateException aggregateException) {
                            foreach (var exception in aggregateException.Flatten().InnerExceptions) {
                                Logging.TraceException(exception);
                            }

                            throw;
                        }
                        catch (Exception exception) {
                            Logging.TraceException(exception);
                            throw;
                        }

                        return AsyncCallbackResult.Continue;
                    })) {
                    foreach (var tableItem in tablesToFetch) {
                        var table = this.entityContext.GetTable(tableItem.Key.First, tableItem.Key.Second);
                        if (table == null) {
                            throw new PersistenceException(
                                string.Format(CultureInfo.InvariantCulture, @"Table {0}/{1} does not exists",
                                    tableItem.Key.First.TrimEnd('/'), tableItem.Key.Second));
                        }

                        var scanSpec = tableItem.Value.CreateScanSpec();

                        if (reviewScanSpec != null) {
                            reviewScanSpec(table, scanSpec);
                        }

                        //// TODO add more infos + time, other places???
                        Logging.TraceEvent(
                            TraceEventType.Verbose,
                            () => string.Format(CultureInfo.InvariantCulture, @"Begin scan {0} on table {1}", scanSpec,
                                table.Name));

                        table.BeginScan(asynResult, scanSpec, tableItem);
                    }

                    asynResult.Join();
                    if (asynResult.Error != null) {
                        throw asynResult.Error;
                    }

                    foreach (var tableItem in tablesToFetch) {
                        if (tableItem.Value.IsEmpty.IsFalse()) {
                            //// TODO remaining cells should be deleted
                        }
                    }
                }
            }
            else if (this.useParallelDeserialization) {
                foreach (var tableItem in tablesToFetch) {
                    var table = this.entityContext.GetTable(tableItem.Key.First, tableItem.Key.Second);
                    if (table == null) {
                        throw new PersistenceException(
                            string.Format(CultureInfo.InvariantCulture, @"Table {0}/{1} does not exists",
                                tableItem.Key.First.TrimEnd('/'), tableItem.Key.Second));
                    }

                    var scanSpec = tableItem.Value.CreateScanSpec();

                    if (reviewScanSpec != null) {
                        reviewScanSpec(table, scanSpec);
                    }

                    //// TODO add/remove more infos + time, other places???
                    Logging.TraceEvent(
                        TraceEventType.Verbose,
                        () => string.Format(CultureInfo.InvariantCulture, @"Scan {0} on table {1}", scanSpec,
                            table.Name));

                    if (scanSpec.RowCount == 0 || scanSpec.RowCount > 32) {
                        var fetchedCells = new BlockingQueue<FetchedCell>();

                        Action ProcessFetchedCells = () => {
                            while (true) {
                                var fetchedCell = fetchedCells.Dequeue();
                                if (fetchedCell == null) {
                                    break;
                                }

                                try {
                                    entityFetched(fetchedCell, null, IntPtr.Zero, 0, fetchedCell.EntityScanTarget);
                                }
                                finally {
                                    PooledCell.Return(fetchedCell.Value);
                                }
                            }
                        };

                        var tasks = new Task[FetchTaskCount];
                        for (var i = 0; i < tasks.Length; ++i) {
                            tasks[i] = Task.Run(ProcessFetchedCells);
                        }

                        using (var scanner = table.CreateScanner(scanSpec)) {
                            var fetchedCell = new FetchedCell();
                            while (scanner.Move(fetchedCell)) {
                                if (tableItem.Value.TryRemoveScanTarget(fetchedCell.Key, out fetchedCell.EntityScanTarget)) {
                                    fetchedCells.Enqueue(fetchedCell);
                                    fetchedCell = new FetchedCell();
                                }
                                else {
                                    PooledCell.Return(fetchedCell.Value);
                                }
                            }

                            if (tableItem.Value.IsEmpty.IsFalse()) {
                                //// TODO remaining cells should be deleted
                            }
                        }

                        for (var i = 0; i < tasks.Length; ++i) {
                            fetchedCells.Enqueue(null);
                        }

                        Task.WaitAll(tasks);
                    }
                    else if (this.useCellBuffer) {
                        Func<Key, IntPtr, int, bool> callback = (key, buffer, length) => {
                            EntityScanTarget entityScanTarget;
                            if (tableItem.Value.TryRemoveScanTarget(key, out entityScanTarget)) {
                                entityFetched(null, key, buffer, length, entityScanTarget);
                            }

                            return true;
                        };

                        using (var scanner = table.CreateScanner(scanSpec)) {
                            while (scanner.Next(callback));

                            if (tableItem.Value.IsEmpty.IsFalse()) {
                                //// TODO remaining cells should be deleted
                            }
                        }
                    }
                    else {
                        var bufferedCell = new BufferedCell(0);

                        using (var scanner = table.CreateScanner(scanSpec)) {
                            while (scanner.Move(bufferedCell)) {
                                EntityScanTarget entityScanTarget;
                                if (tableItem.Value.TryRemoveScanTarget(bufferedCell.Key, out entityScanTarget)) {
                                    entityFetched(bufferedCell, null, IntPtr.Zero, 0, entityScanTarget);
                                }
                            }

                            if (tableItem.Value.IsEmpty.IsFalse()) {
                                //// TODO remaining cells should be deleted
                            }
                        }
                    }
                }
            }
            else if (this.useCellBuffer) {
                foreach (var tableItem in tablesToFetch) {
                    var table = this.entityContext.GetTable(tableItem.Key.First, tableItem.Key.Second);
                    if (table == null) {
                        throw new PersistenceException(
                            string.Format(CultureInfo.InvariantCulture, @"Table {0}/{1} does not exists",
                                tableItem.Key.First.TrimEnd('/'), tableItem.Key.Second));
                    }

                    var scanSpec = tableItem.Value.CreateScanSpec();

                    if (reviewScanSpec != null) {
                        reviewScanSpec(table, scanSpec);
                    }

                    //// TODO add/remove more infos + time, other places???
                    Logging.TraceEvent(
                        TraceEventType.Verbose,
                        () => string.Format(CultureInfo.InvariantCulture, @"Scan {0} on table {1}", scanSpec,
                            table.Name));

                    Func<Key, IntPtr, int, bool> callback = (key, buffer, length) => {
                        EntityScanTarget entityScanTarget;
                        if (tableItem.Value.TryRemoveScanTarget(key, out entityScanTarget)) {
                            entityFetched(null, key, buffer, length, entityScanTarget);
                        }

                        return true;
                    };

                    using (var scanner = table.CreateScanner(scanSpec)) {
                        while (scanner.Next(callback));

                        if (tableItem.Value.IsEmpty.IsFalse()) {
                            //// TODO remaining cells should be deleted
                        }
                    }
                }
            }
            else {
                var bufferedCell = new BufferedCell(0);

                foreach (var tableItem in tablesToFetch) {
                    var table = this.entityContext.GetTable(tableItem.Key.First, tableItem.Key.Second);
                    if (table == null) {
                        throw new PersistenceException(
                            string.Format(CultureInfo.InvariantCulture, @"Table {0}/{1} does not exists",
                                tableItem.Key.First.TrimEnd('/'), tableItem.Key.Second));
                    }

                    var scanSpec = tableItem.Value.CreateScanSpec();

                    if (reviewScanSpec != null) {
                        reviewScanSpec(table, scanSpec);
                    }

                    //// TODO add/remove more infos + time, other places???
                    Logging.TraceEvent(
                        TraceEventType.Verbose,
                        () => string.Format(CultureInfo.InvariantCulture, @"Scan {0} on table {1}", scanSpec,
                            table.Name));

                    using (var scanner = table.CreateScanner(scanSpec)) {
                        while (scanner.Move(bufferedCell)) {
                            EntityScanTarget entityScanTarget;
                            if (tableItem.Value.TryRemoveScanTarget(bufferedCell.Key, out entityScanTarget)) {
                                entityFetched(bufferedCell, null, IntPtr.Zero, 0, entityScanTarget);
                            }
                        }

                        if (tableItem.Value.IsEmpty.IsFalse()) {
                            //// TODO remaining cells should be deleted
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Process the fetched cells in parallel.
        /// </summary>
        /// <param name="tableItem">
        ///     The table item.
        /// </param>
        /// <param name="cells">
        ///     The cells.
        /// </param>
        /// <param name="entityFetched">
        ///     The entity fetched delegate.
        /// </param>
        private static void ParallelProcessFetchedCells(KeyValuePair<Pair<string>, ITableScan> tableItem,
            IEnumerable<Cell> cells, EntityFetched entityFetched)
        {
            Parallel.ForEach(
                cells,
                cell =>
                {
                    EntityScanTarget entityScanTarget;
                    if (tableItem.Value.TryRemoveScanTarget(cell.Key, out entityScanTarget))
                    {
                        entityFetched(cell, null, IntPtr.Zero, 0, entityScanTarget);
                    }
                });
        }

        /// <summary>
        ///     Process the fetched cells sequential.
        /// </summary>
        /// <param name="tableItem">
        ///     The table item.
        /// </param>
        /// <param name="cells">
        ///     The cells.
        /// </param>
        /// <param name="entityFetched">
        ///     The entity fetched delegate.
        /// </param>
        private static void SequentialProcessFetchedCells(KeyValuePair<Pair<string>, ITableScan> tableItem,
            IEnumerable<Cell> cells, EntityFetched entityFetched)
        {
            foreach (var cell in cells)
            {
                EntityScanTarget entityScanTarget;
                if (tableItem.Value.TryRemoveScanTarget(cell.Key, out entityScanTarget))
                {
                    entityFetched(cell, null, IntPtr.Zero, 0, entityScanTarget);
                }
            }
        }

        /// <summary>
        ///     Creates a table scan instance.
        /// </summary>
        /// <param name="scanSpec">
        ///     The scan spec.
        /// </param>
        /// <returns>
        ///     The newly created table scan instance.
        /// </returns>
        private ITableScan CreateTableScan(ScanSpec scanSpec)
        {
            return new TableScan.TableScan(scanSpec);
        }

        /// <summary>
        ///     Creates a table scan and filter instance.
        /// </summary>
        /// <returns>
        ///     The newly created table scan and filter instance.
        /// </returns>
        private ITableScan CreateTableScanAndFilter()
        {
            return this.useAsyncTableScanner || this.useParallelDeserialization
                ? (ITableScan) new ConcurrentTableScanAndFilter()
                : new TableScanAndFilter();
        }

        #endregion

        #region Nested Types

        private sealed class BlockingQueue<T>
        {
            private int count;

            private readonly Queue<T> queue = new Queue<T>();

            public T Dequeue()
            {
                lock (this.queue)
                {
                    while (this.count <= 0)
                    {
                        Monitor.Wait(this.queue);
                    }

                    this.count--;

                    return this.queue.Dequeue();
                }
            }

            public void Enqueue(T data)
            {
                lock (this.queue)
                {
                    this.queue.Enqueue(data);

                    this.count++;

                    Monitor.Pulse(this.queue);
                }
            }
        }

        #endregion
    }
}
