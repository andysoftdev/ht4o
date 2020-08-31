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
    using System.Globalization;
    using Hypertable.Persistence.Collections;
    using Hypertable.Persistence.Scanner;
    using Hypertable.Persistence.Serialization;
    using EntitySpecSet = Hypertable.Persistence.Collections.Concurrent.ConcurrentSet<Hypertable.Persistence.Scanner.EntitySpec>;

    /// <summary>
    ///     The entity writer.
    /// </summary>
    internal sealed class EntityWriter
    {
        #region Fields

        /// <summary>
        ///     The behaviors.
        /// </summary>
        private readonly Behaviors behaviors;

        /// <summary>
        ///     Keeps track of the entities written.
        /// </summary>
        private readonly IdentitySet entitiesWritten;

        /// <summary>
        ///     The entity context.
        /// </summary>
        private readonly EntityContext entityContext;

        /// <summary>
        ///     The entity reference.
        /// </summary>
        private EntityReference entityReference;

        /// <summary>
        ///     The entity key.
        /// </summary>
        private Key key;

        /// <summary>
        ///     Indicating whether entity is a new entity or not.
        /// </summary>
        private bool newEntity;

        /// <summary>
        ///     The table mutator.
        /// </summary>
        private ITableMutator tableMutator;

        /// <summary>
        ///     The table mutator type.
        /// </summary>
        private Type tableMutatorType;

        /// <summary>
        ///     The keys to ignore.
        /// </summary>
        private readonly ISet<Key> ignoreKeys;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityWriter" /> class.
        /// </summary>
        /// <param name="entityContext">
        ///     The entity context.
        /// </param>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="ignoreKeys">
        ///     The keys to ignore.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <param name="entitiesWritten">
        ///     The entities written.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If <see cref="Behaviors.DoNotCache" /> has been combined with <see cref="Behaviors.CreateLazy" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If <see cref="Behaviors.BypassWriteCache" /> has been combined with <see cref="Behaviors.CreateLazy" />.
        /// </exception>
        private EntityWriter(EntityContext entityContext, ISet<Key> ignoreKeys, Behaviors behaviors,
            IdentitySet entitiesWritten)
        {
            if (behaviors.IsCreateLazy())
            {
                if (behaviors.DoNotCache())
                {
                    throw new ArgumentException(@"DontCache cannot be combined with CreateLazy", nameof(behaviors));
                }

                if (behaviors.BypassWriteCache())
                {
                    throw new ArgumentException(@"BypassWriteCache cannot be combined with CreateLazy", nameof(behaviors));
                }
            }

            this.entityContext = entityContext;
            this.ignoreKeys = ignoreKeys;
            this.behaviors = behaviors;
            this.entitiesWritten = entitiesWritten;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Persist the given entity using the behavior specified.
        /// </summary>
        /// <param name="entityContext">
        ///     The entity context.
        /// </param>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <param name="entitiesWritten">
        ///     The entities written.
        /// </param>
        /// <returns>
        ///     The entity key.
        /// </returns>
        private static Key Persist(EntityContext entityContext, Type entityType, object entity, Behaviors behaviors,
            IdentitySet entitiesWritten)
        {
            var entityWriter = new EntityWriter(entityContext, null, behaviors, entitiesWritten);
            return entityWriter.Persist(entityType, entity);
        }

        /// <summary>
        ///     Persist the given entity using the behavior specified.
        /// </summary>
        /// <param name="entityContext">
        ///     The entity context.
        /// </param>
        /// <param name="entity">
        ///     The entity to persist.
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
        internal static void Persist<T>(EntityContext entityContext, T entity, ISet<Key> ignoreKeys, Behaviors behaviors) where T : class {
            Persist(entityContext, typeof(T), entity, ignoreKeys, behaviors);
        }

        /// <summary>
        ///     Persist the given entity using the behavior specified.
        /// </summary>
        /// <param name="entityContext">
        ///     The entity context.
        /// </param>
        /// <param name="entityType">
        ///     The entity Type.
        /// </param>
        /// <param name="entity">
        ///     The entity to persist.
        /// </param>
        /// <param name="ignoreKeys">
        ///     The keys to ignore.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        internal static void Persist(EntityContext entityContext, Type entityType, object entity, ISet<Key> ignoreKeys, Behaviors behaviors) {
            Persist(entityContext, entityType, entity, ignoreKeys, behaviors, new IdentitySet(), true, true);
        }

        /// <summary>
        ///     Persist the given entity using the behavior specified.
        /// </summary>
        /// <param name="entityContext">
        ///     The entity context.
        /// </param>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="ignoreKeys">
        ///     The keys to ignore.
        /// </param>
        /// <param name="behaviors">
        ///     The behaviors.
        /// </param>
        /// <param name="entitiesWritten">
        ///     The entities written.
        /// </param>
        /// <param name="write">
        ///    if <c>true</c> writes the entity, otherwise only traversing.
        /// </param>
        /// <param name="isRoot">
        ///    if <c>true</c> if entity is the root, otherwise <c>false</c>.
        /// </param>
        /// <returns>
        ///     The entity key.
        /// </returns>
        private static Key Persist(EntityContext entityContext, Type entityType, object entity, ISet<Key> ignoreKeys, Behaviors behaviors,
            IdentitySet entitiesWritten, bool write = true, bool isRoot = false) {
            var entityWriter = new EntityWriter(entityContext, ignoreKeys, behaviors, entitiesWritten);
            return entityWriter.Persist(entityType, entity, write, isRoot);
        }

        /// <summary>
        ///     Persist the entity.
        /// </summary>
        /// <param name="entityType">
        ///     The entity type.
        /// </param>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="write">
        ///    if <c>true</c> writes the entity, otherwise only traversing.
        /// </param>
        /// <param name="isRoot">
        ///    if <c>true</c> if entity is the root, otherwise <c>false</c>.
        /// </param>
        /// <returns>
        ///     The entity key.
        /// </returns>
        /// <exception cref="PersistenceException">
        ///     If entity is not recognized as a valid entity.
        /// </exception>
        private Key Persist(Type entityType, object entity, bool write = true, bool isRoot = false)
        {
            if (this.entitiesWritten.Add(entity))
            {
                write = write && (this.newEntity || !this.behaviors.WriteNewOnly());
                var value = EntitySerializer.Serialize(this.entityContext, entityType, entity,
                    SerializationBase.DefaultCapacity, write, this.SerializingEntity);
                if (this.entityReference == null)
                {
                    throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                        @"{0} is not a valid entity", entityType));
                }

                var dontCache = this.behaviors.DoNotCache();
                var bypassEntitySpecsFetched =
                    this.newEntity || this.behaviors.IsCreateNew() || this.behaviors.BypassReadCache();
                var entitySpec = !dontCache || !bypassEntitySpecsFetched
                    ? new EntitySpec(this.entityReference, new Key(this.key))
                    : null;

                if (dontCache || this.entityContext.EntitySpecsWritten.Add(entitySpec) || (isRoot && this.behaviors.IsCreateNew()) || this.behaviors.BypassWriteCache())
                {
                    if (write)
                    {
                        if (bypassEntitySpecsFetched || !this.entityContext.EntitySpecsFetched.Contains(entitySpec))
                        {
                            var type = this.entityReference.EntityType;
                            if (this.tableMutatorType != type) {
                                this.tableMutatorType = type;
                                this.tableMutator = this.entityContext.GetTableMutator(this.entityReference.Namespace,
                                    this.entityReference.TableName);
                            }

                            //// TODO verbosity?
                            //// Logging.TraceEvent(TraceEventType.Verbose, () => string.Format(CultureInfo.InvariantCulture, @"Set {0}@{1}", this.tableMutator.Key, this.key));
                            this.tableMutator.Set(this.key, value);
                        }
                    }
                }
            }

            return this.key;
        }

        /// <summary>
        ///     The serializing entity callback.
        /// </summary>
        /// <param name="isRoot">
        ///     Indicating whether the entity is the root entity.
        /// </param>
        /// <param name="er">
        ///     The entity reference.
        /// </param>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="e">
        ///     The entity.
        /// </param>
        /// <returns>
        ///     The entity key.
        /// </returns>
        private Key SerializingEntity(bool isRoot, EntityReference er, Type serializeType, object e)
        {
            var write = true;

            if (isRoot)
            {
                this.entityReference = er;

                switch (this.behaviors & Behaviors.CreateBehaviors)
                {
                    case Behaviors.CreateAlways:
                        this.key = er.GenerateKey(e);
                        this.newEntity = true;
                        break;

                    case Behaviors.CreateLazy:
                        this.key = er.GetKeyFromEntity(e, out this.newEntity);
                        if (!this.newEntity)
                        {
                            var entitySpec = new EntitySpec(this.entityReference, new Key(this.key));

                            ////TODO needs to check if the entity has been modified (write,write again)
                            if (!this.entityContext.EntitySpecsWritten.Contains(entitySpec))
                            {
                                ////TODO needs to check if the entity has been modified (read, write)
                                if (this.behaviors.BypassReadCache() || !this.entityContext.EntitySpecsFetched.Contains(entitySpec))
                                {
                                    this.key = er.GenerateKey(e);
                                }
                            }
                            else
                            {
                                return null; // cancel further serialization
                            }
                        }

                        break;

                    case Behaviors.CreateNew:
                        this.key = er.GetKeyFromEntity(e, out this.newEntity);
                        break;
                }

                return this.key;
            }
            else if (this.ignoreKeys != null && (this.behaviors & Behaviors.CreateBehaviors) != Behaviors.CreateAlways)
            {
                var k = er.KeyBinding.KeyFromEntity(e);
                if (k != null && this.ignoreKeys.Contains(k))
                {
                    write = false;
                }
            }

            return Persist(this.entityContext, serializeType, e, this.ignoreKeys, this.behaviors, this.entitiesWritten, write);
        }

        #endregion
    }
}