/** -*- C# -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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
    using System.Linq;
    using System.Text;

    using Hypertable;
    using Hypertable.Persistence.Bindings;
    using Hypertable.Persistence.Collections;
    using Hypertable.Persistence.Reflection;

    /// <summary>
    /// The entity binding context.
    /// </summary>
    internal class EntityBindingContext : BindingContext
    {
        #region Fields

        /// <summary>
        /// The entity references.
        /// </summary>
        private readonly ConcurrentTypeDictionary<EntityReference> entityReferences = new ConcurrentTypeDictionary<EntityReference>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBindingContext"/> class.
        /// </summary>
        internal EntityBindingContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBindingContext"/> class.
        /// </summary>
        /// <param name="bindingContext">
        /// The binding context.
        /// </param>
        internal EntityBindingContext(BindingContext bindingContext)
            : base(bindingContext)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the column names for the entity reference specified.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <returns>
        /// The column names.
        /// </returns>
        internal IEnumerable<string> ColumnNames(EntityReference entityReference)
        {
            var distinctColumnBindings = this.DistinctColumnBindingsForType(entityReference.EntityType).ToList();
            if (distinctColumnBindings.Count <= 1)
            {
                return distinctColumnBindings.Select(ColumnName);
            }

            var columnBindingComparer = new ColumnBindingComparer();
            var columnFamilyBindings = new Dictionary<string, HashSet<IColumnBinding>>();

            foreach (var binding in distinctColumnBindings)
            {
                HashSet<IColumnBinding> columnBindings;
                if (!columnFamilyBindings.TryGetValue(binding.ColumnFamily, out columnBindings))
                {
                    columnFamilyBindings.Add(binding.ColumnFamily, columnBindings = new HashSet<IColumnBinding>(columnBindingComparer));
                }

                columnBindings.Add(binding);
            }

            var registeredColumnNames = this.RegisteredColumnNames();

            foreach (var binding in distinctColumnBindings.Where(b => b.ColumnQualifier != null))
            {
                ISet<string> columnQualifiers;
                if (registeredColumnNames.TryGetValue(binding.ColumnFamily, out columnQualifiers))
                {
                    if (columnQualifiers.Remove(binding.ColumnQualifier) && columnQualifiers.Count == 0)
                    {
                        columnFamilyBindings[binding.ColumnFamily] = new HashSet<IColumnBinding> { new ColumnBinding(binding.ColumnFamily) };
                    }
                }
            }

            return columnFamilyBindings.Values.SelectMany(s => s).Select(ColumnName);
        }

        /// <summary>
        /// Gets the entity reference for the inspector specified.
        /// </summary>
        /// <param name="inspector">
        /// The inspector.
        /// </param>
        /// <returns>
        /// The entity reference.
        /// </returns>
        internal EntityReference EntityReferenceForInspector(Inspector inspector)
        {
            return this.entityReferences.GetOrAdd(inspector.InspectedType, type => this.CreateEntityReference(inspector));
        }

        /// <summary>
        /// Gets the entity reference for the type specified.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <returns>
        /// The entity reference.
        /// </returns>
        internal EntityReference EntityReferenceForType(Type type)
        {
            return this.entityReferences.GetOrAdd(type, this.NewEntityReference);
        }

        /// <summary>
        /// Gets the common base type candidates for a given type.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <returns>
        /// The common base type candidates.
        /// </returns>
        protected override IEnumerable<Type> CommonBaseTypeCandidatesForType(Type type)
        {
            var candidates = base.CommonBaseTypeCandidatesForType(type);
            if (this.StrictExplicitColumnBinding || this.StrictExplicitTableBinding)
            {
                return candidates;
            }

            var candidate = TypeFinder.GetCommonBaseType(this.entityReferences.Keys.Where(type.IsAssignableFrom));
            return candidates.Concat(new[] { candidate });
        }

        /// <summary>
        /// The distinct column bindings for the type specified.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <returns>
        /// Distinct column bindings.
        /// </returns>
        protected override IEnumerable<IColumnBinding> DistinctColumnBindingsForType(Type type)
        {
            var bindings = base.DistinctColumnBindingsForType(type);
            if (this.StrictExplicitColumnBinding)
            {
                return bindings;
            }

            var mergedBindings = new HashSet<IColumnBinding>(bindings, new ColumnBindingComparer());
            foreach (var binding in this.entityReferences.Where(kv => type.IsAssignableFrom(kv.Key) && kv.Value.ColumnBinding != null).Select(kv => kv.Value.ColumnBinding))
            {
                mergedBindings.Add(binding);
            }

            return mergedBindings;
        }

        /// <summary>
        /// The distinct table bindings for the type specified.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <returns>
        /// Distinct table bindings.
        /// </returns>
        protected override IEnumerable<ITableBinding> DistinctTableBindingsForType(Type type)
        {
            var bindings = base.DistinctTableBindingsForType(type);
            if (this.StrictExplicitTableBinding)
            {
                return bindings;
            }

            var mergedBindings = new HashSet<ITableBinding>(bindings, new TableBindingComparer());
            foreach (var binding in this.entityReferences.Where(kv => type.IsAssignableFrom(kv.Key)).Select(kv => kv.Value.TableBinding))
            {
                mergedBindings.Add(binding);
            }

            return mergedBindings;
        }

        /// <summary>
        /// Removes all entity references affected by the column binding specified.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <param name="columnBinding">
        /// The column binding.
        /// </param>
        protected override void RemovingColumnBinding(Type type, IColumnBinding columnBinding)
        {
            this.entityReferences.Remove(
                from kv in this.entityReferences where type.IsAssignableFrom(kv.Key) || (kv.Value != null && object.ReferenceEquals(kv.Value.ColumnBinding, columnBinding)) select kv.Key);
        }

        /// <summary>
        /// Removes all entity references affected by the key binding specified.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <param name="keyBinding">
        /// The key binding.
        /// </param>
        protected override void RemovingKeyBinding(Type type, IKeyBinding keyBinding)
        {
            this.entityReferences.Remove(
                from kv in this.entityReferences where type.IsAssignableFrom(kv.Key) || (kv.Value != null && object.ReferenceEquals(kv.Value.KeyBinding, keyBinding)) select kv.Key);
        }

        /// <summary>
        /// Removes all entity references affected by the table binding specified.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <param name="tableBinding">
        /// The table binding.
        /// </param>
        protected override void RemovingTableBinding(Type type, ITableBinding tableBinding)
        {
            this.entityReferences.Remove(
                from kv in this.entityReferences where type.IsAssignableFrom(kv.Key) || (kv.Value != null && object.ReferenceEquals(kv.Value.TableBinding, tableBinding)) select kv.Key);
        }

        /// <summary>
        /// Gets the fully qualified column name for the column binding specified.
        /// </summary>
        /// <param name="columnBinding">
        /// The column binding.
        /// </param>
        /// <returns>
        /// The fully qualified column name.
        /// </returns>
        private static string ColumnName(IColumnBinding columnBinding)
        {
            return columnBinding.ColumnQualifier == null ? columnBinding.ColumnFamily : columnBinding.ColumnFamily + ":" + columnBinding.ColumnQualifier;
        }

        /// <summary>
        /// The new entity reference factory.
        /// </summary>
        /// <param name="type">
        /// The entity type.
        /// </param>
        /// <returns>
        /// The newly created entity reference.
        /// </returns>
        private EntityReference NewEntityReference(Type type)
        {
            return this.CreateEntityReference(type);
        }

        #endregion
    }
}