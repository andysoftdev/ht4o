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
namespace Hypertable.Persistence.Serialization
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.IO;

    using Hypertable;
    using Hypertable.Persistence.Extensions;
    using Hypertable.Persistence.Reflection;
    using Hypertable.Persistence.Serialization.Delegates;

    /// <summary>
    /// The entity serializer.
    /// </summary>
    internal sealed class EntitySerializer : Serializer
    {
        #region Static Fields

        /// <summary>
        /// The type schema dictionary.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, TypeSchema> TypeSchemaDictionary = new ConcurrentDictionary<Type, TypeSchema>();

        #endregion

        #region Fields

        /// <summary>
        /// The entity context.
        /// </summary>
        private readonly EntityContext entityContext;

        /// <summary>
        /// The root object.
        /// </summary>
        private readonly object root;

        /// <summary>
        /// The serializing entity delegate.
        /// </summary>
        private readonly SerializingEntity serializingEntity;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySerializer"/> class.
        /// </summary>
        /// <param name="entityContext">
        /// The entity context.
        /// </param>
        /// <param name="binaryWriter">
        /// The binary writer.
        /// </param>
        /// <param name="root">
        /// The root object.
        /// </param>
        /// <param name="serializingEntity">
        /// The serializing entity delegate.
        /// </param>
        private EntitySerializer(EntityContext entityContext, BinaryWriter binaryWriter, object root, SerializingEntity serializingEntity)
            : base(binaryWriter)
        {
            this.entityContext = entityContext;
            this.root = root;
            this.serializingEntity = serializingEntity;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serialize an object.
        /// </summary>
        /// <param name="entityContext">
        /// The entity context.
        /// </param>
        /// <param name="serializeType">
        /// The serialize type.
        /// </param>
        /// <param name="value">
        /// The object to serialize.
        /// </param>
        /// <param name="capacity">
        /// The internal memory stream intial capacity.
        /// </param>
        /// <param name="serializingEntity">
        /// The serializing entity delegate.
        /// </param>
        /// <returns>
        /// The serialized object.
        /// </returns>
        internal static byte[] Serialize(EntityContext entityContext, Type serializeType, object value, int capacity, SerializingEntity serializingEntity)
        {
            using (var memoryStream = new MemoryStream(capacity))
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    new EntitySerializer(entityContext, binaryWriter, value, serializingEntity).Write(serializeType, value);
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Encodes the object specified.
        /// </summary>
        /// <param name="encoderInfo">
        /// The encoder info.
        /// </param>
        /// <param name="value">
        /// The object to encode.
        /// </param>
        /// <param name="writeTag">
        /// If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        internal override void Encode(EncoderInfo encoderInfo, object value, bool writeTag)
        {
            var anyType = value.GetType();

            ////TODO review, correct?
            if (encoderInfo.Tag >= Tags.FirstCustomType || anyType.IsComplex())
            {
                if (this.WriteOrAddObjectRef(value))
                {
                    return;
                }

                var inspector = Inspector.InspectorForType(anyType);
                var entityReference = this.entityContext.EntityReferenceForInspector(inspector);

                // Entity?
                if (entityReference != null && this.SerializeEntityReference(entityReference, anyType, value))
                {
                    return;
                }
            }

            encoderInfo.Encode(this, value, writeTag);
        }

        /// <summary>
        /// Gets the type schema.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="inspector">
        /// The inspector.
        /// </param>
        /// <returns>
        /// The type schema.
        /// </returns>
        internal override TypeSchema GetTypeSchema(Type type, Inspector inspector)
        {
            return TypeSchemaDictionary.GetOrAdd(type, arg => new TypeSchema(inspector, true));
        }

        /// <summary>
        /// Writes an object.
        /// </summary>
        /// <param name="inspector">
        /// The inspector.
        /// </param>
        /// <param name="serializeType">
        /// The serialize type.
        /// </param>
        /// <param name="type">
        /// The object type.
        /// </param>
        /// <param name="value">
        /// The object to write.
        /// </param>
        internal override void WriteObject(Inspector inspector, Type serializeType, Type type, object value)
        {
            if (this.WriteOrAddObjectRef(value))
            {
                return;
            }

            var entityReference = this.entityContext.EntityReferenceForInspector(inspector);

            // Entity?
            if (entityReference != null && this.SerializeEntityReference(entityReference, serializeType, value))
            {
                return;
            }

            this.WriteObjectTrailer(type, inspector).WriteObject(this, value);
        }

        /// <summary>
        /// Serializes an entity reference.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// <c>true</c> if no further encoding is required, otherwise <c>false</c>.
        /// </returns>
        private bool SerializeEntityReference(EntityReference entityReference, Type entityType, object entity)
        {
            var isRoot = ReferenceEquals(this.root, entity);
            var key = this.serializingEntity(isRoot, entityReference, entityType, entity);
            if (!isRoot)
            {
                if (key == null)
                {
                    bool generated;
                    key = entityReference.GetKeyFromEntity(entity, out generated);
                    if (generated)
                    {
                        throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Entity key has not been set for type {0}", entityType));
                    }
                }

                if (this.entityContext.StrictStaticTableBinding)
                {
                    if (this.entityContext.StrictStaticColumnBinding)
                    {
                        this.WriteEntityRow(entityReference, key);
                    }
                    else
                    {
                        this.WriteEntityKey(entityReference, key);
                    }
                }
                else
                {
                    this.WriteEntityReference(entityReference, key);
                }

                return true;
            }

            return key == null; // cancel further serialization
        }

        /// <summary>
        /// Writes an entity key.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        private void WriteEntityKey(EntityReference entityReference, Key key)
        {
            var binaryWriter = this.BinaryWriter;
            Encoder.WriteTag(binaryWriter, Tags.EntityKey);
            this.WriteType(entityReference.EntityType);
            this.BinaryWriter.Write(key.Row);
            this.WriteString(key.ColumnFamily);
            this.WriteString(key.ColumnQualifier);
        }

        /// <summary>
        /// Writes an entity reference.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        private void WriteEntityReference(EntityReference entityReference, Key key)
        {
            var binaryWriter = this.BinaryWriter;
            Encoder.WriteTag(binaryWriter, Tags.EntityRef);
            this.WriteType(entityReference.EntityType);
            this.WriteString(entityReference.Namespace);
            this.WriteString(entityReference.TableName);
            this.BinaryWriter.Write(key.Row);
            this.WriteString(key.ColumnFamily);
            this.WriteString(key.ColumnQualifier);
        }

        /// <summary>
        /// Writes an entity row.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        private void WriteEntityRow(EntityReference entityReference, Key key)
        {
            var binaryWriter = this.BinaryWriter;
            Encoder.WriteTag(binaryWriter, Tags.EntityRow);
            this.WriteType(entityReference.EntityType);
            this.BinaryWriter.Write(key.Row);
        }

        #endregion
    }
}