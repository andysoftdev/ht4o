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

namespace Hypertable.Persistence.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using Hypertable.Persistence.Extensions;
    using Hypertable.Persistence.Reflection;
    using Hypertable.Persistence.Scanner;
    using Hypertable.Persistence.Serialization.Delegates;

    /// <summary>
    ///     The entity deserializer.
    /// </summary>
    internal sealed class EntityDeserializer : Deserializer
    {
        #region Fields

        /// <summary>
        ///     The deserializing entity delegate.
        /// </summary>
        private readonly DeserializingEntity deserializingEntity;

        /// <summary>
        ///     The entity binding context.
        /// </summary>
        private readonly EntityBindingContext entityBindingContext;

        /// <summary>
        ///     The entity reader.
        /// </summary>
        private readonly EntityReader entityReader;

        /// <summary>
        ///     The entity scanner.
        /// </summary>
        private readonly EntityScanner entityScanner;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityDeserializer" /> class.
        /// </summary>
        /// <param name="entityReader">
        ///     The entity reader.
        /// </param>
        /// <param name="binaryReader">
        ///     The binary reader.
        /// </param>
        /// <param name="deserializingEntity">
        ///     The deserializing entity delegate.
        /// </param>
        private EntityDeserializer(EntityReader entityReader, BinaryReader binaryReader,
            DeserializingEntity deserializingEntity)
            : base(binaryReader)
        {
            this.entityReader = entityReader;
            this.entityScanner = entityReader.EntityScanner;
            this.entityBindingContext = this.entityScanner.EntityContext;
            this.deserializingEntity = deserializingEntity;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Deserialize an object.
        /// </summary>
        /// <param name="entityReader">
        ///     The entity reader.
        /// </param>
        /// <param name="destinationType">
        ///     The destination type.
        /// </param>
        /// <param name="serialized">
        ///     The serialized object.
        /// </param>
        /// <param name="deserializingEntity">
        ///     The deserializing entity delegate.
        /// </param>
        /// <returns>
        ///     The deserialized object.
        /// </returns>
        internal static object Deserialize(EntityReader entityReader, Type destinationType, byte[] serialized,
            DeserializingEntity deserializingEntity)
        {
            using (var binaryReader = new BinaryArrayReader(serialized))
            {
                return new EntityDeserializer(entityReader, binaryReader, deserializingEntity).Deserialize(
                    destinationType, Decoder.ReadTag(binaryReader));
            }
        }

        /// <summary>
        ///     Deserialize an object.
        /// </summary>
        /// <param name="entityReader">
        ///     The entity reader.
        /// </param>
        /// <param name="destinationType">
        ///     The destination type.
        /// </param>
        /// <param name="serialized">
        ///     The serialized object.
        /// </param>
        /// <param name="count">
        ///     The size of the serialized object.
        /// </param>
        /// <param name="deserializingEntity">
        ///     The deserializing entity delegate.
        /// </param>
        /// <returns>
        ///     The deserialized object.
        /// </returns>
        internal static object Deserialize(EntityReader entityReader, Type destinationType, byte[] serialized, int count,
            DeserializingEntity deserializingEntity) {
            using (var binaryReader = new BinaryArrayReader(serialized, 0, count)) {
                return new EntityDeserializer(entityReader, binaryReader, deserializingEntity).Deserialize(
                    destinationType, Decoder.ReadTag(binaryReader));
            }
        }

        /// <summary>
        ///     Before deserialize object properties notification.
        /// </summary>
        /// <param name="destinationType">
        ///     The destination type.
        /// </param>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <param name="target">
        ///     The target object.
        /// </param>
        internal override void BeforeDeserializeObjectProperties(Type destinationType, Inspector inspector,
            object target)
        {
            var entityReference =
                inspector != null ? this.entityBindingContext.EntityReferenceForInspector(inspector) : null;
            if (entityReference != null)
            {
                this.deserializingEntity(entityReference, destinationType, target);
            }
        }

        /// <summary>
        ///     Reads a dictionary item.
        /// </summary>
        /// <param name="flags">
        ///     The flags.
        /// </param>
        /// <param name="keyType">
        ///     The key type.
        /// </param>
        /// <param name="keyTag">
        ///     The key tag.
        /// </param>
        /// <param name="valueType">
        ///     The value type.
        /// </param>
        /// <param name="valueTag">
        ///     The value tag.
        /// </param>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <param name="dictionary">
        ///     The dictionary.
        /// </param>
        internal override void ReadDictionaryItem(
            DictionaryFlags flags,
            Type keyType,
            Tags keyTag,
            Type valueType,
            Tags valueTag,
            Inspector inspector,
            IDictionary dictionary)
        {
            if (!flags.HasFlag(DictionaryFlags.KeyTypeTagged) || flags.HasFlag(DictionaryFlags.KeyValueTagged))
            {
                keyTag = Decoder.ReadTag(this.BinaryReader);
            }

            var key = this.Deserialize(keyType, keyTag);
            var entitySpec = key as EntitySpec;
            if (entitySpec != null)
            {
                ////TODO implement
                throw new NotImplementedException();
            }

            if (!flags.HasFlag(DictionaryFlags.ValueTypeTagged) || flags.HasFlag(DictionaryFlags.ValueTagged))
            {
                valueTag = Decoder.ReadTag(this.BinaryReader);
            }

            var value = this.Deserialize(valueType, valueTag);
            entitySpec = value as EntitySpec;
            if (entitySpec != null)
            {
                ////TODO implement
                throw new NotImplementedException();
            }

            dictionary.Add(key, value);
        }

        /// <summary>
        ///     Reads enumerable item.
        /// </summary>
        /// <param name="elementType">
        ///     The element type.
        /// </param>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <param name="collection">
        ///     The collection.
        /// </param>
        /// <param name="pos">
        ///     The pos.
        /// </param>
        /// <param name="count">
        ///     The count.
        /// </param>
        internal override void ReadEnumerableItem(Type elementType, Tags tag, Inspector inspector, object collection,
            int pos, int count)
        {
            var any = this.Deserialize(elementType, tag);
            var entitySpec = any as EntitySpec;
            if (entitySpec != null)
            {
                EntityScanTarget entityScanTarget;
                if (inspector.Enumerable.HasIndexer)
                {
                    inspector.Enumerable.Add(collection, null);
                    entityScanTarget = new EntityIndexerScanTarget(elementType, entitySpec, inspector.Enumerable,
                        collection, pos);
                }
                else
                {
                    entityScanTarget =
                        new EntityCollectionScanTarget(elementType, entitySpec, inspector.Enumerable, collection);
                }

                if (IsEntityReference(tag))
                {
                    this.entityScanner.Add(entityScanTarget);
                    this.ObjectRefs.Add(entityScanTarget);
                }
                else if (tag == Tags.ObjectRef)
                {
                    ((EntityScanTarget) entitySpec).AddScanTargetRef(entityScanTarget);
                }
            }
            else
            {
                inspector.Enumerable.Add(collection, any);
            }
        }

        /// <summary>
        ///     Sets an object property.
        /// </summary>
        /// <param name="inspectedProperty">
        ///     The inspected property.
        /// </param>
        /// <param name="target">
        ///     The target.
        /// </param>
        /// <param name="value">
        ///     The property value.
        /// </param>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        internal override void SetObjectProperty(InspectedProperty inspectedProperty, object target, object value,
            Tags tag)
        {
            var entitySpec = value as EntitySpec;
            if (entitySpec != null)
            {
                var entityScanTarget = new EntityScanTarget(inspectedProperty, entitySpec, target);
                if (IsEntityReference(tag))
                {
                    this.entityScanner.Add(entityScanTarget);
                    this.ObjectRefs.Add(entityScanTarget);
                }
                else if (tag == Tags.ObjectRef)
                {
                    ((EntityScanTarget) entitySpec).AddScanTargetRef(entityScanTarget);
                }
            }
            else
            {
                base.SetObjectProperty(inspectedProperty, target, value, tag);
            }
        }

        /// <summary>
        ///     The deferred read object.
        /// </summary>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        /// <param name="destinationType">
        ///     The destination type.
        /// </param>
        /// <param name="any">
        ///     The any.
        /// </param>
        /// <param name="action">
        ///     The action.
        /// </param>
        /// <typeparam name="T">
        ///     Type of object to read.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="action" /> in null.
        /// </exception>
        protected override void DeferredReadObject<T>(Tags tag, Type destinationType, object any, Action<T> action)
        {
            var entitySpec = any as EntitySpec;
            if (entitySpec != null)
            {
                var entityScanTarget =
                    new EntityScanTarget(destinationType, entitySpec, (target, value) => action((T) value));
                if (IsEntityReference(tag))
                {
                    this.entityScanner.Add(entityScanTarget);
                    this.ObjectRefs.Add(entityScanTarget);
                }
                else if (tag == Tags.ObjectRef)
                {
                    ((EntityScanTarget) entitySpec).AddScanTargetRef(entityScanTarget);
                }
            }
            else
            {
                base.DeferredReadObject(tag, destinationType, any, action);
            }
        }

        /// <summary>
        ///     Reads an object.
        /// </summary>
        /// <param name="destinationType">
        ///     The destination type.
        /// </param>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        /// <returns>
        ///     The object read.
        /// </returns>
        protected override object Read(Type destinationType, Tags tag)
        {
            EntitySpec entitySpec = null;
            switch (tag)
            {
                case Tags.EntityRef:
                    entitySpec = new EntitySpec(
                        this.ReadType(), this.ReadString(), this.ReadString(),
                        new Key(this.BinaryReader.ReadString(), this.ReadString(), this.ReadString()));
                    break;

                case Tags.EntityKey:
                {
                    var type = this.ReadType();
                    var entityReference = this.entityBindingContext.EntityReferenceForType(type);
                    if (entityReference == null)
                    {
                        throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                            @"{0} is not a valid entity", type));
                    }

                    var tableBinding = entityReference.TableBinding;
                    if (tableBinding == null)
                    {
                        throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                            @"Undefined table binding for type {0}", type));
                    }

                    entitySpec = new EntitySpec(
                        type, tableBinding.Namespace, tableBinding.TableName,
                        new Key(this.BinaryReader.ReadString(), this.ReadString(), this.ReadString()));
                    break;
                }

                case Tags.EntityRow:
                {
                    var type = this.ReadType();
                    var entityReference = this.entityBindingContext.EntityReferenceForType(type);
                    if (entityReference == null)
                    {
                        throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                            @"{0} is not a valid entity", type));
                    }

                    var tableBinding = entityReference.TableBinding;
                    if (tableBinding == null)
                    {
                        throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                            @"Undefined table binding for type {0}", type));
                    }

                    var columnBinding = entityReference.ColumnBinding;
                    if (columnBinding == null)
                    {
                        throw new PersistenceException(string.Format(CultureInfo.InvariantCulture,
                            @"Undefined column binding for type {0}", type));
                    }

                    entitySpec = new EntitySpec(
                        type, tableBinding.Namespace, tableBinding.TableName,
                        new Key(this.BinaryReader.ReadString(), columnBinding.ColumnFamily,
                            columnBinding.ColumnQualifier));
                    break;
                }
            }

            if (entitySpec != null)
            {
                object entity;
                if (this.entityReader.TryGetFetchedEntity(entitySpec, out entity))
                {
                    this.ObjectRefs.Add(entity);
                    return destinationType.Convert(entity);
                }

                return entitySpec;
            }

            return base.Read(destinationType, tag);
        }

        /// <summary>
        ///     Reads an array element.
        /// </summary>
        /// <param name="elementType">
        ///     The element type.
        /// </param>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        /// <param name="array">
        ///     The array.
        /// </param>
        /// <param name="indexes">
        ///     The indexes.
        /// </param>
        protected override void ReadArrayElement(Type elementType, Tags tag, Array array, int[] indexes)
        {
            var any = this.Deserialize(elementType, tag);
            var entitySpec = any as EntitySpec;
            if (entitySpec != null)
            {
                EntityScanTarget entityScanTarget = new EntityArrayScanTarget(elementType, entitySpec, array, indexes);
                if (IsEntityReference(tag))
                {
                    this.entityScanner.Add(entityScanTarget);
                    this.ObjectRefs.Add(entityScanTarget);
                }
                else if (tag == Tags.ObjectRef)
                {
                    ((EntityScanTarget) entitySpec).AddScanTargetRef(entityScanTarget);
                }
            }
            else
            {
                if (array == null)
                {
                    throw new ArgumentNullException(nameof(array));
                }

                array.SetValue(any, indexes);
            }
        }

        /// <summary>
        ///     Determines if the type tag is an entity reference.
        /// </summary>
        /// <param name="tag">
        ///     The type tag.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the type tag is an entity reference.
        /// </returns>
        private static bool IsEntityReference(Tags tag)
        {
            return tag == Tags.EntityRef || tag == Tags.EntityKey || tag == Tags.EntityRow;
        }

        #endregion
    }
}