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

namespace Hypertable.Persistence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Hypertable.Persistence.Scanner;
    using EntitySpecSet =
        Hypertable.Persistence.Collections.Concurrent.ConcurrentSet<Hypertable.Persistence.Scanner.EntitySpec>;
    using TableMutatorDictionary =
        Hypertable.Persistence.Collections.Concurrent.ConcurrentStringDictionary<ITableMutator>;

    /// <summary>
    ///     The entity context.
    /// </summary>
    internal sealed class EntityContext : EntityBindingContext, IDisposable
    {
        #region Fields

        /// <summary>
        ///     The persistence configuration.
        /// </summary>
        private readonly PersistenceConfiguration configuration;

        /// <summary>
        ///     The entity manager factory context.
        /// </summary>
        private readonly FactoryContext factoryContext;

        /// <summary>
        ///     The table mutators.
        /// </summary>
        private readonly TableMutatorDictionary tableMutators = new TableMutatorDictionary();

        /// <summary>
        ///     Indicating whether this factory context has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        ///     The entity specs fetched.
        /// </summary>
        private EntitySpecSet entitySpecsFetched = new EntitySpecSet();

        /// <summary>
        ///     The entity specs written.
        /// </summary>
        private EntitySpecSet entitySpecsWritten = new EntitySpecSet();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityContext" /> class.
        /// </summary>
        /// <param name="factoryContext">
        ///     The factory context.
        /// </param>
        /// <param name="bindingContext">
        ///     The binding context.
        /// </param>
        internal EntityContext(FactoryContext factoryContext, BindingContext bindingContext)
            : base(bindingContext)
        {
            this.factoryContext = factoryContext;
            this.configuration = new PersistenceConfiguration(factoryContext.Configuration, this);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="EntityContext" /> class.
        /// </summary>
        ~EntityContext()
        {
            this.Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the database client.
        /// </summary>
        /// <value>
        ///     The database client.
        /// </value>
        /// <remarks>
        ///     Do not dispose the returned database client, it's owned by the entity context.
        /// </remarks>
        internal IClient Client
        {
            get
            {
                this.ThrowIfDisposed();
                return this.factoryContext.Client;
            }
        }

        /// <summary>
        ///     Gets the persistence configuration.
        /// </summary>
        /// <value>
        ///     The configuration.
        /// </value>
        internal PersistenceConfiguration Configuration
        {
            get
            {
                this.ThrowIfDisposed();
                return this.configuration;
            }
        }

        /// <summary>
        ///     Gets the entity specs fetched set.
        /// </summary>
        /// <value>
        ///     The entity specs fetched set.
        /// </value>
        internal EntitySpecSet EntitySpecsFetched => this.entitySpecsFetched;

        /// <summary>
        ///     Gets the entity specs written set.
        /// </summary>
        /// <value>
        ///     The entity specs written set.
        /// </value>
        internal EntitySpecSet EntitySpecsWritten => this.entitySpecsWritten;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the column families for type specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <returns>
        ///     The column families.
        /// </returns>
        public ISet<string> ColumnFamiliesForType(Type entityType)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            entityReference.EstablishColumnSets(() => this.ColumnNames(entityReference));
            return entityReference.ColumnFamilySet;
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Fetches all entities of the entity type specified from the database.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable Fetch(Type entityType, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            var scanSpec = this.ScanSpecForType(entityType);
            return EntityReader.Read(this, entityReference, scanSpec, behaviors);
        }

        /// <summary>
        ///     Fetches all entities of the entity type specified from the database.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        public void Fetch(Type entityType, Action<object> entitySink, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            var scanSpec = this.ScanSpecForType(entityType);
            EntityReader.Read(this, entityReference, scanSpec, entitySink, behaviors);
        }

        /// <summary>
        ///     Fetches all entities of the types specified from the database.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="queryTypes">
        ///     The query types.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable Fetch(Type entityType, IEnumerable<Type> queryTypes, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            return EntityReader.Read(this, entityReference, this.ScanSpecForType(queryTypes), behaviors);
        }

        /// <summary>
        ///     Fetches all entities of the types specified from the database.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="queryTypes">
        ///     The query types.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        public void Fetch(Type entityType, IEnumerable<Type> queryTypes, Action<object> entitySink, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            EntityReader.Read(this, entityReference, this.ScanSpecForType(queryTypes), entitySink, behaviors);
        }

        /// <summary>
        ///     Fetches all entities using the given scan specification from the database.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable Fetch(Type entityType, ScanSpec scanSpec, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (scanSpec == null)
            {
                throw new ArgumentNullException(nameof(scanSpec));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            return EntityReader.Read(this, entityReference, scanSpec, behaviors);
        }

        /// <summary>
        ///     Fetches all entities using the given scan specification from the database.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        public void Fetch(Type entityType, ScanSpec scanSpec, Action<object> entitySink, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (scanSpec == null)
            {
                throw new ArgumentNullException(nameof(scanSpec));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            EntityReader.Read(this, entityReference, scanSpec, entitySink, behaviors);
        }

        /// <summary>
        ///     Find an entity in the database using the key provider specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="keyProvider">
        ///     The key provider.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <returns>
        ///     The found entity instance or null if the entity does not exist.
        /// </returns>
        public object Find(Type entityType, object keyProvider, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (keyProvider == null)
            {
                throw new ArgumentNullException(nameof(keyProvider));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            return EntityReader.Read(this, entityReference, keyProvider, behaviors);
        }

        /// <summary>
        ///     Find the entities in the database using the key providers specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="keyProviders">
        ///     The key providers.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <returns>
        ///     The entities found.
        /// </returns>
        public IEnumerable FindMany(Type entityType, IEnumerable keyProviders, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (keyProviders == null)
            {
                throw new ArgumentNullException(nameof(keyProviders));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            return EntityReader.Read(this, entityReference, keyProviders, behaviors);
        }

        /// <summary>
        ///     Find the entities in the database using the key providers specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="keyProviders">
        ///     The key providers.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        public void FindMany(Type entityType, IEnumerable keyProviders, Action<object> entitySink, Behaviors behaviors)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (keyProviders == null)
            {
                throw new ArgumentNullException(nameof(keyProviders));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            EntityReader.Read(this, entityReference, keyProviders, entitySink, behaviors);
        }

        /// <summary>
        ///     Gets the database namespace instance for the entity type specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <returns>
        ///     The database namespace instance.
        /// </returns>
        /// <remarks>
        ///     Do not dispose the returned namespace instance, it's owned by the entity context.
        /// </remarks>
        public INamespace GetNamespace(Type entityType)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            return this.factoryContext.GetNamespace(entityReference.Namespace);
        }

        /// <summary>
        ///     Gets the database table instance for the entity type specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <returns>
        ///     The database table instance.
        /// </returns>
        /// <remarks>
        ///     Do not dispose the returned table instance, it's owned by the entity context.
        /// </remarks>
        public ITable GetTable(Type entityType)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            return this.GetTable(entityReference.Namespace, entityReference.TableName);
        }

        /// <summary>
        ///     Merge the state of the given entity into the database.
        /// </summary>
        /// <param name="entity">
        ///     The entity to merge.
        /// </param>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        public void Merge<T>(T entity) where T : class
        {
            ////TODO implement real merge
            this.Persist(entity, null, Behaviors.CreateNew);
        }

        /// <summary>
        ///     Stores an entity instance to the database.
        /// </summary>
        /// <param name="entity">
        ///     The entity to store.
        /// </param>
        /// <param name="ignoreKeys">
        ///     The keys to ignore.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        public void Persist<T>(T entity, ISet<Key> ignoreKeys, Behaviors behaviors) where T : class
        {
            EntityWriter.Persist(this, entity, ignoreKeys, behaviors);
        }

        /// <summary>
        ///     Remove an entity instance from the database using the key provider specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="keyProvider">
        ///     The key provider.
        /// </param>
        public void Remove(Type entityType, object keyProvider)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (keyProvider == null)
            {
                throw new ArgumentNullException(nameof(keyProvider));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            this.GetTableMutator(entityReference.Namespace, entityReference.TableName)
                .Delete(entityReference.GetKeyFromObject(keyProvider, true));

            //// TODO remove from any cache????
        }

        /// <summary>
        ///     Removes all entities using the given scan specification from the database.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        public void Remove(Type entityType, ScanSpec scanSpec)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            if (scanSpec == null)
            {
                throw new ArgumentNullException(nameof(scanSpec));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            if (!scanSpec.KeysOnly)
            {
                ////scanSpec = new ScanSpec(scanSpec); //// TODO copy/clone scan spec
                scanSpec.KeysOnly = true;
            }

            var table = this.GetTable(entityReference.Namespace, entityReference.TableName);
            var mutator = this.GetTableMutator(entityReference.Namespace, entityReference.TableName);
            using (var scanner = table.CreateScanner(scanSpec))
            {
                var cell = new Cell();
                while (scanner.Move(cell))
                {
                    //// TODO remove from any cache????
                    mutator.Delete(cell.Key);
                }
            }
        }

        /// <summary>
        ///     Gets the scan spec for the type specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <returns>
        ///     The scan spec.
        /// </returns>
        public ScanSpec ScanSpecForType(Type entityType)
        {
            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            var entityReference = this.EntityReferenceForType(entityType);
            if (entityReference == null)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            entityReference.EstablishColumnSets(() => this.ColumnNames(entityReference));
            var scanSpec = new ScanSpec {MaxVersions = 1};
            scanSpec.AddColumn(entityReference.ColumnSet);
            if (scanSpec.ColumnCount == 0)
            {
                throw new PersistenceException(string.Format(CultureInfo.InvariantCulture, @"{0} is not a valid entity",
                    entityType));
            }

            return scanSpec;
        }

        /// <summary>
        ///     Gets the merged scan spec for the types specified.
        /// </summary>
        /// <param name="entityTypes">
        ///     The query types.
        /// </param>
        /// <returns>
        ///     The merged scan spec.
        /// </returns>
        public ScanSpec ScanSpecForType(IEnumerable<Type> entityTypes)
        {
            if (entityTypes == null)
            {
                throw new ArgumentNullException(nameof(entityTypes));
            }

            var columnNames = new List<string>();
            foreach (var queryType in entityTypes)
            {
                var entityReference = this.EntityReferenceForType(queryType);
                if (entityReference == null)
                {
                    throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                        @"{0} is not a valid query type", queryType));
                }

                entityReference.EstablishColumnSets(() => this.ColumnNames(entityReference));
                columnNames.AddRange(entityReference.ColumnSet);
            }

            if (columnNames.Count > 5)
            {
                var registeredColumnNames = this.RegisteredColumnNames();

                foreach (var columnName in columnNames.ToList())
                {
                    var split = columnName.Split(':');
                    if (split.Length > 1)
                    {
                        ISet<string> columnQualifiers;
                        if (registeredColumnNames.TryGetValue(split[0], out columnQualifiers))
                        {
                            if (columnQualifiers.Remove(split[1]) && columnQualifiers.Count == 0)
                            {
                                columnNames.Add(split[0]);
                            }
                        }
                    }
                }
            }

            var scanSpec = new ScanSpec {MaxVersions = 1};
            scanSpec.AddColumn(ScanSpec.DistictColumn(columnNames));
            if (scanSpec.ColumnCount == 0)
            {
                throw new PersistenceException("Missing or invalid entity types");
            }

            return scanSpec;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Flushes all table mutators.
        /// </summary>
        internal void Flush()
        {
            foreach (var tableMutator in this.tableMutators.Values)
            {
                tableMutator.Flush();
            }
        }

        /// <summary>
        ///     Gets the database table instance for the namespace/table name specified.
        /// </summary>
        /// <param name="ns">
        ///     The namespace name.
        /// </param>
        /// <param name="tableName">
        ///     The table Name.
        /// </param>
        /// <returns>
        ///     The database table instance for the namespace/table name specified.
        /// </returns>
        /// <remarks>
        ///     Do not dispose the returned table instance, it's owned by the entity context.
        /// </remarks>
        internal ITable GetTable(string ns, string tableName)
        {
            return this.factoryContext.GetTable(ns, tableName);
        }

        /// <summary>
        ///     Gets the table mutator for the namespace/table name specified.
        /// </summary>
        /// <param name="ns">
        ///     The namespace name.
        /// </param>
        /// <param name="tableName">
        ///     The table Name.
        /// </param>
        /// <returns>
        ///     The table mutator.
        /// </returns>
        internal ITableMutator GetTableMutator(string ns, string tableName)
        {
            return this.tableMutators.GetOrAdd(this.GetFullyQualifiedTableName(ns, tableName),
                s => this.GetTable(ns, tableName).CreateMutator(this.configuration.MutatorSpec));
        }

        /// <summary>
        ///     Checks if the database context supports the feature specified.
        /// </summary>
        /// <param name="contextFeature">
        ///     The context feature.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the database context supports the feature specified, otherwise <c>false</c>.
        /// </returns>
        internal bool HasFeature(ContextFeature contextFeature)
        {
            return this.factoryContext.HasFeature(contextFeature);
        }

        /// <summary>
        ///     The dispose.
        /// </summary>
        /// <param name="disposing">
        ///     The disposing.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    foreach (var tableMutator in this.tableMutators.Values)
                    {
                        tableMutator.Dispose();
                    }
                }

                this.entitySpecsFetched.Clear();
                this.entitySpecsFetched = null;

                this.entitySpecsWritten.Clear();
                this.entitySpecsWritten = null;

                this.disposed = true;
            }
        }

        /// <summary>
        ///     Returns the fully qualified table name.
        /// </summary>
        /// <param name="ns">
        ///     The namespace name.
        /// </param>
        /// <param name="tableName">
        ///     The table name.
        /// </param>
        /// <returns>
        ///     The fully qualified table name.
        /// </returns>
        private string GetFullyQualifiedTableName(string ns, string tableName)
        {
            return this.factoryContext.GetFullyQualifiedTableName(ns, tableName);
        }

        /// <summary>
        ///     The throw if the object has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">
        ///     If the object has been already disposed.
        /// </exception>
        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("Entity context has been already disposed");
            }
        }

        #endregion
    }
}