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
    using System.Collections.Generic;

    using NamespaceDictionary =
        Hypertable.Persistence.Collections.Concurrent.ConcurrentStringDictionary<INamespace>;
    using TableDictionary =
        Hypertable.Persistence.Collections.Concurrent.ConcurrentStringDictionary<ITable>;

    /// <summary>
    ///     The entity manager factory context.
    /// </summary>
    internal sealed class FactoryContext : IDisposable
    {
        #region Fields

        /// <summary>
        ///     The database client.
        /// </summary>
        private readonly IClient client;

        /// <summary>
        ///     The persistence configuration.
        /// </summary>
        private readonly PersistenceConfiguration configuration;

        /// <summary>
        ///     The database context.
        /// </summary>
        private readonly IContext context;

        /// <summary>
        ///     Indicating whether this factory context owns the database context.
        /// </summary>
        private readonly bool disposeContext;

        /// <summary>
        ///     Maps namespace names to database namespace instances.
        /// </summary>
        private readonly NamespaceDictionary namespaces = new NamespaceDictionary();

        /// <summary>
        ///     Maps full table names to database table instances.
        /// </summary>
        private readonly TableDictionary tables = new TableDictionary();

        /// <summary>
        ///     Indicating whether this factory context has been disposed.
        /// </summary>
        private bool disposed;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FactoryContext" /> class.
        /// </summary>
        /// <param name="configuration">
        ///     The persistence configuration.
        /// </param>
        /// <param name="context">
        ///     The database context.
        /// </param>
        /// <param name="disposeContext">
        ///     Flag indicating whether this factory context owns the database context.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="configuration" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="context" /> is null.
        /// </exception>
        internal FactoryContext(PersistenceConfiguration configuration, IContext context, bool disposeContext)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this.configuration = configuration;
            this.context = context;
            this.disposeContext = disposeContext;
            this.client = context.CreateClient();
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="FactoryContext" /> class.
        /// </summary>
        ~FactoryContext()
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
        ///     Do not dispose the returned database client, it's owned by the factory context.
        /// </remarks>
        internal IClient Client
        {
            get
            {
                this.ThrowIfDisposed();
                return this.client;
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
        ///     Gets the database context properties.
        /// </summary>
        /// <value>
        ///     The database context properties.
        /// </value>
        internal IDictionary<string, object> Properties
        {
            get
            {
                this.ThrowIfDisposed();
                return this.context != null ? this.context.Properties : new Dictionary<string, object>();
            }
        }

        /// <summary>
        ///     Gets the database root namespace.
        /// </summary>
        /// <value>
        ///     The database root namespace.
        /// </value>
        internal INamespace RootNamespace
        {
            get
            {
                this.ThrowIfDisposed();
                return this.GetNamespace(this.configuration.RootNamespace);
            }
        }

        /// <summary>
        ///     Gets the synchronization object.
        /// </summary>
        /// <value>
        ///     The synchronization object.
        /// </value>
        internal object SyncRoot { get; } = new object();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Clears the entire factory context.
        /// </summary>
        internal void Clear()
        {
            this.ThrowIfDisposed();

            foreach (var table in this.tables.Values)
            {
                table.Dispose();
            }

            this.tables.Clear();

            foreach (var ns in this.namespaces.Values)
            {
                ns.Dispose();
            }

            this.namespaces.Clear();
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
        internal string GetFullyQualifiedTableName(string ns, string tableName)
        {
            if (!string.IsNullOrEmpty(ns))
            {
                if (ns[ns.Length - 1] != '/')
                {
                    ns += '/';
                }

                if (ns[0] != '/')
                {
                    ns = ns.Insert(0, this.configuration.RootNamespace);
                }

                return ns + tableName;
            }

            return this.configuration.RootNamespace + tableName;
        }

        /// <summary>
        ///     Gets the database namespace instance for the namespace name specified.
        /// </summary>
        /// <param name="ns">
        ///     The namespace name.
        /// </param>
        /// <returns>
        ///     The database namespace instance for the namespace name specified.
        /// </returns>
        /// <remarks>
        ///     Do not dispose the returned namespace instance, it's owned by the factory context.
        /// </remarks>
        internal INamespace GetNamespace(string ns)
        {
            this.ThrowIfDisposed();

            if (!string.IsNullOrEmpty(ns))
            {
                if (ns != "/")
                {
                    ns = ns.TrimEnd('/');
                    if (ns[0] != '/')
                    {
                        ns = ns.Insert(0, this.configuration.RootNamespace);
                    }
                }
            }
            else
            {
                ns = this.configuration.RootNamespace;
                ns = ns.TrimEnd('/');
            }

            return this.namespaces.GetOrAdd(ns,
                _ns => this.client.OpenNamespace(_ns,
                    OpenDispositions.OpenAlways | OpenDispositions.CreateIntermediate));
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
        ///     Do not dispose the returned table instance, it's owned by the factory context.
        /// </remarks>
        internal ITable GetTable(string ns, string tableName)
        {
            this.ThrowIfDisposed();

            lock (this.SyncRoot)
            {
                return this.tables.GetOrAdd(
                    this.GetFullyQualifiedTableName(ns, tableName),
                    fullyQualifiedTableName => this.GetNamespace(ns).OpenTable(tableName, OpenDispositions.Force));
            }
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
            this.ThrowIfDisposed();
            return this.context.HasFeature(contextFeature);
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
                    this.Clear();

                    this.client.Dispose();
                    if (this.disposeContext)
                    {
                        this.context.Dispose();
                    }
                }

                this.disposed = true;
            }
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
                throw new ObjectDisposedException("Factory context has been already disposed");
            }
        }

        #endregion
    }
}