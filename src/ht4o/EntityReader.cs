/** -*- C# -*-
 * Copyright (C) 2010-2013 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Linq;

    using Hypertable;
    using Hypertable.Persistence.Collections;
    using Hypertable.Persistence.Scanner;
    using Hypertable.Persistence.Serialization;

    using EntitySpecDictionary = System.Collections.Concurrent.ConcurrentDictionary<Hypertable.Persistence.Scanner.EntitySpec, object>;
    using EntitySpecSet = Hypertable.Persistence.Collections.ConcurrentSet<Hypertable.Persistence.Scanner.EntitySpec>;

    /// <summary>
    /// The entity reader.
    /// </summary>
    internal sealed class EntityReader
    {
        #region Fields

        /// <summary>
        /// The behaviors.
        /// </summary>
        private readonly Behaviors behaviors;

        /// <summary>
        /// The entities fetched.
        /// </summary>
        private readonly EntitySpecDictionary entitiesFetched = new EntitySpecDictionary();

        /// <summary>
        /// The entity scanner.
        /// </summary>
        private readonly EntityScanner entityScanner;

        /// <summary>
        /// The entity specs fetched.
        /// </summary>
        private readonly EntitySpecSet entitySpecsFetched;

        /// <summary>
        /// The fetched cell.
        /// </summary>
        private FetchedCell fetchedCell;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityReader"/> class.
        /// </summary>
        /// <param name="entityScanner">
        /// The entity scanner.
        /// </param>
        /// <param name="behaviors">
        /// The behaviors.
        /// </param>
        private EntityReader(EntityScanner entityScanner, Behaviors behaviors)
        {
            this.entityScanner = entityScanner;
            this.behaviors = behaviors;
            this.entitySpecsFetched = entityScanner.EntityContext.EntitySpecsFetched;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity scanner.
        /// </summary>
        /// <value>
        /// The entity scanner.
        /// </value>
        internal EntityScanner EntityScanner
        {
            get
            {
                return this.entityScanner;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads an entity which belongs to the given database key from the database.
        /// </summary>
        /// <param name="entityContext">
        /// The entity context.
        /// </param>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="key">
        /// The database key.
        /// </param>
        /// <param name="behaviors">
        /// The behaviors.
        /// </param>
        /// <returns>
        /// The entity.
        /// </returns>
        internal static object Read(EntityContext entityContext, EntityReference entityReference, object key, Behaviors behaviors)
        {
            var entityScanTarget = new EntityScanTarget(entityReference, key);
            var entityScanner = new EntityScanner(entityContext);
            entityScanner.Add(entityScanTarget);
            new EntityReader(entityScanner, behaviors).Read();
            return entityScanTarget.Value;
        }

        /// <summary>
        /// Reads all entities which belongs to the given database keys from the database.
        /// </summary>
        /// <param name="entityContext">
        /// The entity context.
        /// </param>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="keys">
        /// The entity keys.
        /// </param>
        /// <param name="behaviors">
        /// The behaviors.
        /// </param>
        /// <returns>
        /// The entities.
        /// </returns>
        internal static IEnumerable Read(EntityContext entityContext, EntityReference entityReference, IEnumerable keys, Behaviors behaviors)
        {
            var entityScanner = new EntityScanner(entityContext);
            var entityScanTargets = new ChunkedCollection<EntityScanTarget>();
            foreach (var key in keys)
            {
                if (key != null)
                {
                    var entityScanTarget = new EntityScanTarget(entityReference, entityReference.GetKeyFromObject(key, false));
                    entityScanTargets.Add(entityScanTarget);
                    entityScanner.Add(entityScanTarget);
                }
            }

            new EntityReader(entityScanner, behaviors).Read();
            return entityScanTargets.Select(entityScanTarget => entityScanTarget.Value);
        }

        /// <summary>
        /// Reads all entities which belongs to the given scan specification from the database.
        /// </summary>
        /// <param name="entityContext">
        /// The entity context.
        /// </param>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="scanSpec">
        /// The scan spec.
        /// </param>
        /// <param name="behaviors">
        /// The behaviors.
        /// </param>
        /// <returns>
        /// The entities.
        /// </returns>
        internal static IEnumerable Read(EntityContext entityContext, EntityReference entityReference, ScanSpec scanSpec, Behaviors behaviors)
        {
            var entityScanResult = new EntityScanResult(entityReference);
            var entityScanner = new EntityScanner(entityContext);
            entityScanner.Add(entityScanResult, scanSpec);

            new EntityReader(entityScanner, behaviors).Read();
            return entityScanResult.Values;
        }

        /// <summary>
        /// Attempts to get an already fetched entity from the cache.
        /// </summary>
        /// <param name="entitySpec">
        /// The entity spec.
        /// </param>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// <c>true</c> if the cache contains an element with the specified entity spec, otherwise <c>false</c>.
        /// </returns>
        internal bool TryGetFetchedEntity(EntitySpec entitySpec, out object entity)
        {
            return this.entitiesFetched.TryGetValue(entitySpec, out entity);
        }

        /// <summary>
        /// The deserializing entity callback.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="entity">
        /// The entity.
        /// </param>
        private void DeserializingEntity(EntityReference entityReference, Type destinationType, object entity)
        {
            this.entitiesFetched.TryAdd(this.fetchedCell.EntityScanTarget, entity);
            entityReference.SetKey(entity, this.fetchedCell.Cell.Key);
        }

        /// <summary>
        /// The entity fetched callback.
        /// </summary>
        /// <param name="fc">
        /// The fetched cell.
        /// </param>
        private void EntityFetched(ref FetchedCell fc)
        {
            this.fetchedCell = fc;
            var entityScanTarget = fc.EntityScanTarget;
            var entity = EntityDeserializer.Deserialize(this, typeof(object)/*TODO REMOVE ?? entityScanTarget.EntityType*/, fc.Cell.Value, this.DeserializingEntity);

            if (!this.behaviors.DoNotCache())
            {
                this.entitySpecsFetched.Add(new EntitySpec(entityScanTarget));
            }

            if (entity == null || entityScanTarget.EntityType.IsAssignableFrom(entity.GetType()))
            {
                entityScanTarget.SetValue(entity);
            }
        }

        /// <summary>
        /// Reads all entities from the entity scanner.
        /// </summary>
        private void Read()
        {
           while (!this.entityScanner.IsEmpty)
            {
                this.entityScanner.Fetch(this.TryGetFetchedEntity, this.EntityFetched);
            }
        }

        #endregion
    }
}