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
    using System.Linq;
    using Hypertable.Persistence.Reflection;
    using Hypertable.Xml;

    ////TODO see also https://nhibernate.svn.sourceforge.net/svnroot/nhibernate/trunk/nhibernate/src/NHibernate/ISession.cs
    ////TODO cascading

    /// <summary>
    ///     The entity manager.
    /// </summary>
    public sealed class EntityManager : IDisposable
    {
        #region Fields

        /// <summary>
        ///     Indicating whether this factory context has been disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        ///     The entity context.
        /// </summary>
        private EntityContext entityContext;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityManager" /> class.
        /// </summary>
        /// <param name="factoryContext">
        ///     The entity manager factory context.
        /// </param>
        internal EntityManager(FactoryContext factoryContext)
            : this(factoryContext, factoryContext.Configuration.Binding)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityManager" /> class.
        /// </summary>
        /// <param name="factoryContext">
        ///     The entity manager factory context.
        /// </param>
        /// <param name="bindingContext">
        ///     The binding context.
        /// </param>
        internal EntityManager(FactoryContext factoryContext, BindingContext bindingContext)
        {
            this.DefaultBehaviors = Behaviors.CreateLazy;
            this.entityContext = new EntityContext(factoryContext, bindingContext);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="EntityManager" /> class.
        /// </summary>
        ~EntityManager()
        {
            this.Dispose(false);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the database client.
        /// </summary>
        /// <value>
        ///     The database client.
        /// </value>
        /// <remarks>
        ///     Do not dispose the returned database client, it's owned by the factory context.
        /// </remarks>
        public IClient Client
        {
            get
            {
                this.ThrowIfDisposed();
                return this.entityContext.Client;
            }
        }

        /// <summary>
        ///     Gets the persistence configuration.
        /// </summary>
        /// <value>
        ///     The configuration.
        /// </value>
        public PersistenceConfiguration Configuration
        {
            get
            {
                this.ThrowIfDisposed();
                return this.entityContext.Configuration;
            }
        }

        /// <summary>
        ///     Gets or sets the default behaviors.
        /// </summary>
        /// <value>
        ///     The default behaviors.
        /// </value>
        public Behaviors DefaultBehaviors { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Determines whether the database contains any elements of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     <c>true</c> if the there are any entities; otherwise <c>false</c>.
        /// </returns>
        public bool Any<T>() where T : class
        {
            return this.Any(typeof(T));
        }

        /// <summary>
        ///     Determines whether the database contains any entities of the type specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the there are any entities; otherwise <c>false</c>.
        /// </returns>
        public bool Any(Type entityType)
        {
            this.ThrowIfDisposed();
            var scanSpec = this.entityContext.ScanSpecForType(entityType);
            scanSpec.MaxCells = 1;
            var entities = this.Fetch(entityType, scanSpec, Behaviors.DoNotCache);
            return entities != null && entities.OfType<object>().Any();
        }

        /// <summary>
        ///     Determines whether the database contains any entities of the types specified.
        /// </summary>
        /// <param name="entityTypes">
        ///     The entity types.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the there are any entities; otherwise <c>false</c>.
        /// </returns>
        public bool Any(IEnumerable<Type> entityTypes)
        {
            if (entityTypes == null)
            {
                throw new ArgumentNullException(nameof(entityTypes));
            }

            this.ThrowIfDisposed();
            var types = entityTypes.ToArray();
            var scanSpec = this.entityContext.ScanSpecForType(types);
            scanSpec.MaxCells = 1;
            var entities = this.Fetch(TypeFinder.GetCommonBaseType(types), scanSpec, Behaviors.DoNotCache);
            return entities != null && entities.OfType<object>().Any();
        }

        /// <summary>
        ///     Creates a scan spec for type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     Newly created scan spec.
        /// </returns>
        public ScanSpec CreateScanSpec<T>() where T : class
        {
            return this.CreateScanSpec(typeof(T));
        }

        /// <summary>
        ///     Creates a scan spec for the type specified.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <returns>
        ///     Newly created scan spec.
        /// </returns>
        public ScanSpec CreateScanSpec(Type entityType)
        {
            this.ThrowIfDisposed();
            return this.entityContext.ScanSpecForType(entityType);
        }

        /// <summary>
        ///     Creates a scan spec for the types specified.
        /// </summary>
        /// <param name="entityTypes">
        ///     The entity types.
        /// </param>
        /// <returns>
        ///     Newly created scan spec.
        /// </returns>
        public ScanSpec CreateScanSpec(IEnumerable<Type> entityTypes)
        {
            this.ThrowIfDisposed();
            return this.entityContext.ScanSpecForType(entityTypes);
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
        ///     Removes an entity instance from the entity manager context.
        /// </summary>
        /// <param name="entity">
        ///     The entity to evict.
        /// </param>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     The entity.
        /// </returns>
        public T Evict<T>(T entity) where T : class
        {
            ////TODO implement evict
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Fetches all entities of the entity type specified from the database.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable<T> Fetch<T>() where T : class
        {
            return this.Fetch<T>(Behaviors.Default);
        }

        /// <summary>
        ///     Fetches all entities of the entity type specified from the database.
        /// </summary>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable<T> Fetch<T>(Behaviors behaviors) where T : class
        {
            return OfType<T>(this.Fetch(typeof(T), this.CheckBehaviors(behaviors)));
        }

        /// <summary>
        ///     Fetches all entities of the types specified from the database.
        /// </summary>
        /// <param name="queryTypes">
        ///     The query types.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable<T> Fetch<T>(IEnumerable<Type> queryTypes) where T : class
        {
            return this.Fetch<T>(queryTypes, Behaviors.Default);
        }

        /// <summary>
        ///     Fetches all entities of the types specified from the database.
        /// </summary>
        /// <param name="queryTypes">
        ///     The query types.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable<T> Fetch<T>(IEnumerable<Type> queryTypes, Behaviors behaviors) where T : class
        {
            return OfType<T>(this.Fetch(typeof(T), queryTypes, this.CheckBehaviors(behaviors)));
        }

        /// <summary>
        ///     Fetches all entities using the given  scan specification from the database.
        /// </summary>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable<T> Fetch<T>(ScanSpec scanSpec) where T : class
        {
            return this.Fetch<T>(scanSpec, Behaviors.Default);
        }

        /// <summary>
        ///     Fetches all entities using the given  scan specification from the database.
        /// </summary>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable<T> Fetch<T>(ScanSpec scanSpec, Behaviors behaviors) where T : class
        {
            return OfType<T>(this.Fetch(typeof(T), scanSpec, this.CheckBehaviors(behaviors)));
        }

        /// <summary>
        ///     Fetches all entities of the entity type specified from the database.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        public void Fetch<T>(Action<T> entitySink) where T : class
        {
            this.Fetch<T>(entitySink, Behaviors.Default);
        }

        /// <summary>
        ///     Fetches all entities of the entity type specified from the database.
        /// </summary>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        public void Fetch<T>(Action<T> entitySink, Behaviors behaviors) where T : class
        {
            this.Fetch(typeof(T), OfType<T>(entitySink), this.CheckBehaviors(behaviors));
        }

        /// <summary>
        ///     Fetches all entities of the types specified from the database.
        /// </summary>
        /// <param name="queryTypes">
        ///     The query types.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        public void Fetch<T>(IEnumerable<Type> queryTypes, Action<T> entitySink) where T : class
        {
            this.Fetch<T>(queryTypes, OfType<T>(entitySink), Behaviors.Default);
        }

        /// <summary>
        ///     Fetches all entities of the types specified from the database.
        /// </summary>
        /// <param name="queryTypes">
        ///     The query types.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        public void Fetch<T>(IEnumerable<Type> queryTypes, Action<T> entitySink, Behaviors behaviors) where T : class
        {
            this.Fetch(typeof(T), queryTypes, OfType<T>(entitySink), this.CheckBehaviors(behaviors));
        }

        /// <summary>
        ///     Fetches all entities using the given  scan specification from the database.
        /// </summary>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        public void Fetch<T>(ScanSpec scanSpec, Action<T> entitySink) where T : class
        {
            this.Fetch<T>(scanSpec, OfType<T>(entitySink), Behaviors.Default);
        }

        /// <summary>
        ///     Fetches all entities using the given  scan specification from the database.
        /// </summary>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to fetch.
        /// </typeparam>
        public void Fetch<T>(ScanSpec scanSpec, Action<T> entitySink, Behaviors behaviors) where T : class
        {
            this.Fetch(typeof(T), scanSpec, OfType<T>(entitySink), this.CheckBehaviors(behaviors));
        }

        /// <summary>
        ///     Fetches all entities of the entity type specified from the database.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable Fetch(Type entityType)
        {
            return this.Fetch(entityType, Behaviors.Default);
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
            this.ThrowIfDisposed();
            return this.entityContext.Fetch(entityType, this.CheckBehaviors(behaviors));
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
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable Fetch(Type entityType, IEnumerable<Type> queryTypes)
        {
            return this.Fetch(entityType, queryTypes, Behaviors.Default);
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
            this.ThrowIfDisposed();
            return this.entityContext.Fetch(entityType, queryTypes, this.CheckBehaviors(behaviors));
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
        /// <returns>
        ///     The entities fetched.
        /// </returns>
        public IEnumerable Fetch(Type entityType, ScanSpec scanSpec)
        {
            return this.Fetch(entityType, scanSpec, Behaviors.Default);
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
            this.ThrowIfDisposed();
            return this.entityContext.Fetch(entityType, scanSpec, this.CheckBehaviors(behaviors));
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
        public void Fetch(Type entityType, Action<object> entitySink)
        {
            this.Fetch(entityType, entitySink, Behaviors.Default);
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
            this.ThrowIfDisposed();
            this.entityContext.Fetch(entityType, entitySink, this.CheckBehaviors(behaviors));
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
        public void Fetch(Type entityType, Action<object> entitySink, IEnumerable<Type> queryTypes)
        {
            this.Fetch(entityType, queryTypes, entitySink, Behaviors.Default);
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
            this.ThrowIfDisposed();
            this.entityContext.Fetch(entityType, queryTypes, entitySink, this.CheckBehaviors(behaviors));
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
        public void Fetch(Type entityType, ScanSpec scanSpec, Action<object> entitySink)
        {
            this.Fetch(entityType, scanSpec, entitySink, Behaviors.Default);
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
            this.ThrowIfDisposed();
            this.entityContext.Fetch(entityType, scanSpec, entitySink, this.CheckBehaviors(behaviors));
        }

        /// <summary>
        ///     Find an entity in the database using the key provider specified.
        /// </summary>
        /// <param name="keyProvider">
        ///     The key provider.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entity to find.
        /// </typeparam>
        /// <returns>
        ///     The found entity instance or null if the entity does not exist.
        /// </returns>
        public T Find<T>(object keyProvider) where T : class
        {
            return this.Find<T>(keyProvider, Behaviors.Default);
        }

        /// <summary>
        ///     Find an entity in the database using the key provider specified.
        /// </summary>
        /// <param name="keyProvider">
        ///     The key provider.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entity to find.
        /// </typeparam>
        /// <returns>
        ///     The found entity instance or null if the entity does not exist.
        /// </returns>
        public T Find<T>(object keyProvider, Behaviors behaviors) where T : class
        {
            return (T) this.Find(typeof(T), keyProvider, this.CheckBehaviors(behaviors));
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
        /// <returns>
        ///     The found entity instance or null if the entity does not exist.
        /// </returns>
        public object Find(Type entityType, object keyProvider)
        {
            return this.Find(entityType, keyProvider, Behaviors.Default);
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
            this.ThrowIfDisposed();
            return this.entityContext.Find(entityType, keyProvider, this.CheckBehaviors(behaviors));
        }

        /// <summary>
        ///     Find the entities in the database using the key providers specified.
        /// </summary>
        /// <param name="keyProviders">
        ///     The key providers.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to find.
        /// </typeparam>
        /// <returns>
        ///     The entities found.
        /// </returns>
        public IEnumerable<T> FindMany<T>(IEnumerable keyProviders) where T : class
        {
            return this.FindMany<T>(keyProviders, Behaviors.Default);
        }

        /// <summary>
        ///     Find the entities in the database using the key providers specified.
        /// </summary>
        /// <param name="keyProviders">
        ///     The key providers.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to find.
        /// </typeparam>
        /// <returns>
        ///     The entities found.
        /// </returns>
        public IEnumerable<T> FindMany<T>(IEnumerable keyProviders, Behaviors behaviors) where T : class
        {
            return OfType<T>(this.FindMany(typeof(T), keyProviders, this.CheckBehaviors(behaviors)));
        }

        /// <summary>
        ///     Find the entities in the database using the key providers specified.
        /// </summary>
        /// <param name="keyProviders">
        ///     The key providers.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to find.
        /// </typeparam>
        public void FindMany<T>(IEnumerable keyProviders, Action<T> entitySink) where T : class
        {
            this.FindMany<T>(keyProviders, entitySink, Behaviors.Default);
        }

        /// <summary>
        ///     Find the entities in the database using the key providers specified.
        /// </summary>
        /// <param name="keyProviders">
        ///     The key providers.
        /// </param>
        /// <param name="entitySink">
        ///     The entity sink, receives the entities fetched.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the entities to find.
        /// </typeparam>
        public void FindMany<T>(IEnumerable keyProviders, Action<T> entitySink, Behaviors behaviors) where T : class
        {
            this.FindMany(typeof(T), keyProviders, OfType<T>(entitySink), this.CheckBehaviors(behaviors));
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
        /// <returns>
        ///     The entities found.
        /// </returns>
        public IEnumerable FindMany(Type entityType, IEnumerable keyProviders)
        {
            return this.FindMany(entityType, keyProviders, Behaviors.Default);
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
            this.ThrowIfDisposed();
            return this.entityContext.FindMany(entityType, keyProviders, this.CheckBehaviors(behaviors));
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
        public void FindMany(Type entityType, IEnumerable keyProviders, Action<object> entitySink)
        {
            this.FindMany(entityType, keyProviders, entitySink, Behaviors.Default);
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
            this.ThrowIfDisposed();
            this.entityContext.FindMany(entityType, keyProviders, entitySink, this.CheckBehaviors(behaviors));
        }


        /// <summary>
        ///     Flush the entity context to the server.
        /// </summary>
        public void Flush()
        {
            this.ThrowIfDisposed();
            this.entityContext.Flush();
        }

        /// <summary>
        ///     Gets the database namespace instance for the entity type specified.
        /// </summary>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     The database namespace instance.
        /// </returns>
        /// <remarks>
        ///     Do not dispose the returned namespace instance, it's owned by the entity manager.
        /// </remarks>
        public INamespace GetNamespace<T>()
        {
            return this.GetNamespace(typeof(T));
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
        ///     Do not dispose the returned namespace instance, it's owned by the entity manager.
        /// </remarks>
        public INamespace GetNamespace(Type entityType)
        {
            this.ThrowIfDisposed();
            return this.entityContext.GetNamespace(entityType);
        }

        /// <summary>
        ///     Gets the database table instance for the entity type specified.
        /// </summary>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     The database table instance.
        /// </returns>
        /// <remarks>
        ///     Do not dispose the returned table instance, it's owned by the factory context.
        /// </remarks>
        public ITable GetTable<T>()
        {
            return this.GetTable(typeof(T));
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
        ///     Do not dispose the returned table instance, it's owned by the factory context.
        /// </remarks>
        public ITable GetTable(Type entityType)
        {
            this.ThrowIfDisposed();
            return this.entityContext.GetTable(entityType);
        }

        /// <summary>
        ///     Gets value that indicates whether the entity type specified is declared in the corresponding table schema.
        /// </summary>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     <c>true</c> if the entity type is declared in the corresponding table schema; otherwise <c>false</c>.
        /// </returns>
        public bool IsTypeDeclared<T>()
        {
            return this.IsTypeDeclared(typeof(T));
        }

        /// <summary>
        ///     Gets value that indicates whether the entity type specified is declared in the corresponding table schema.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the entity type is declared in the corresponding table schema; otherwise <c>false</c>.
        /// </returns>
        public bool IsTypeDeclared(Type entityType)
        {
            this.ThrowIfDisposed();

            var columnFamilies = this.entityContext.ColumnFamiliesForType(entityType);
            var table = this.GetTable(entityType);
            var tableSchema = TableSchema.Parse(table.Schema);
            foreach (var name in tableSchema.AccessGroups.SelectMany(ag => ag.ColumnFamilies).Select(cf => cf.Name))
            {
                columnFamilies.Remove(name);
            }

            return columnFamilies.Count == 0;
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
        /// <returns>
        ///     The entity.
        /// </returns>
        public T Merge<T>(T entity) where T : class
        {
            this.ThrowIfDisposed();
            this.entityContext.Merge(entity);
            return entity;
        }

        /// <summary>
        ///     Stores an entity instance to the database.
        /// </summary>
        /// <param name="entity">
        ///     The entity to store.
        /// </param>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     The entity.
        /// </returns>
        public T Persist<T>(T entity) where T : class
        {
            return this.Persist<T>(entity, Behaviors.Default);
        }

        /// <summary>
        ///     Stores an entity instance to the database.
        /// </summary>
        /// <param name="entity">
        ///     The entity to store.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     The entity.
        /// </returns>
        public T Persist<T>(T entity, Behaviors behaviors) where T : class
        {
            return this.Persist<T>(entity, null, behaviors);
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
        /// <returns>
        ///     The entity.
        /// </returns>
        public T Persist<T>(T entity, ISet<Key> ignoreKeys, Behaviors behaviors) where T : class {
            this.ThrowIfDisposed();
            this.entityContext.Persist(entity, ignoreKeys, this.CheckBehaviors(behaviors));
            return entity;
        }

        /// <summary>
        ///     Refresh the state of the instance from the database, overwriting changes made to the entity, if any.
        /// </summary>
        /// <param name="entity">
        ///     The entity to refresh.
        /// </param>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <returns>
        ///     The entity.
        /// </returns>
        public T Refresh<T>(T entity) where T : class
        {
            ////TODO implement refresh
            ////EntityReader.Read(this, entityReference, entityKey, Behaviors.Default); //// does not work, creates a new instance
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Remove the entity instance from the database.
        /// </summary>
        /// <param name="entity">
        ///     The entity to remove.
        /// </param>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        public void Remove<T>(T entity) where T : class
        {
            this.Remove(typeof(T), entity);
        }

        /// <summary>
        ///     Remove an entity instance from the database using the key provider specified.
        /// </summary>
        /// <param name="keyProvider">
        ///     The key provider.
        /// </param>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        public void Remove<T>(object keyProvider) where T : class
        {
            this.Remove(typeof(T), keyProvider);
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
            this.ThrowIfDisposed();
            this.entityContext.Remove(entityType, keyProvider);
        }

        /// <summary>
        ///     Removes all entities using the given scan specification from the database.
        /// </summary>
        /// <typeparam name="T">
        ///     The entity type.
        /// </typeparam>
        /// <param name="scanSpec">
        ///     The scan specification.
        /// </param>
        public void Remove<T>(ScanSpec scanSpec) where T : class
        {
            this.Remove(typeof(T), scanSpec);
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
            this.ThrowIfDisposed();
            this.entityContext.Remove(entityType, scanSpec);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Filters the elements of the enumerable based on a specified type.
        /// </summary>
        /// <param name="enumerable">
        ///     The enumerable.
        /// </param>
        /// <typeparam name="T">
        ///     The enumerable element type.
        /// </typeparam>
        /// <returns>
        ///     The typed enumerable.
        /// </returns>
        private static IEnumerable<T> OfType<T>(IEnumerable enumerable) where T : class
        {
            return enumerable?.OfType<T>();
        }

        /// <summary>
        ///     Forward an untyped action to type action.
        /// </summary>
        /// <param name="sink">
        ///     The sink.
        /// </param>
        /// <typeparam name="T">
        ///     The sink element type.
        /// </typeparam>
        /// <returns>
        ///     The untyped action.
        /// </returns>
        private static Action<object> OfType<T>(Action<T> sink) where T : class
        {
            return o =>
            {
                var e = o as T;
                if (e != null)
                {
                    sink(e);
                }
            };
        }

        /// <summary>
        ///     Checks the behaviors.
        /// </summary>
        /// <param name="behaviors">
        ///     The unchecked behaviors.
        /// </param>
        /// <returns>
        ///     The checked behaviors.
        /// </returns>
        private Behaviors CheckBehaviors(Behaviors behaviors)
        {
            return !behaviors.IsDefault() ? behaviors : this.DefaultBehaviors;
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
                    this.entityContext.Dispose();
                }

                this.entityContext = null;
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
                throw new ObjectDisposedException("Entity manager has been already disposed");
            }
        }

        #endregion
    }
}