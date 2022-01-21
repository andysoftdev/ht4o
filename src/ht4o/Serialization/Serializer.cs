﻿/** -*- C# -*-
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Hypertable.Persistence.Collections;
    using Hypertable.Persistence.Collections.Concurrent;
    using Hypertable.Persistence.Extensions;
    using Hypertable.Persistence.Reflection;
    using Hypertable.Persistence.Serialization.Delegates;

    /// <summary>
    ///     The serializer.
    /// </summary>
    public class Serializer : SerializationBase
    {
        #region Static Fields

        /// <summary>
        ///     The type schema dictionary.
        /// </summary>
        private static readonly ConcurrentTypeDictionary<TypeSchema> TypeSchemaDictionary =
            new ConcurrentTypeDictionary<TypeSchema>();

        #endregion

        #region Fields

        /// <summary>
        ///     The encoder configuration.
        /// </summary>
        private readonly EncoderConfiguration configuration;

        /// <summary>
        ///     The encoder info.
        /// </summary>
        private readonly FastDictionary<Type, EncoderInfo> encoderInfos = new FastDictionary<Type, EncoderInfo>(256);

        /// <summary>
        ///     The identity dictionary.
        /// </summary>
        private readonly IdentityDictionary<int> identityDictionary = new IdentityDictionary<int>(256);

        /// <summary>
        ///     The string dictionary.
        /// </summary>
        private readonly StringDictionary<int> stringDictionary = new StringDictionary<int>(256);

        /// <summary>
        ///     The type dictionary.
        /// </summary>
        private readonly FastDictionary<Type, int> typeDictionary = new FastDictionary<Type, int>(256);

        /// <summary>
        ///     The type schema reference dictionary.
        /// </summary>
        private readonly FastDictionary<Type, TypeSchemaRef> typeSchemaRefDictionary =
            new FastDictionary<Type, TypeSchemaRef>(256);

        /// <summary>
        ///     The binary writer.
        /// </summary>
        private BinaryWriter binaryWriter;

        /// <summary>
        ///     The identity value.
        /// </summary>
        private int identityValue;

        /// <summary>
        ///     The default streaming context.
        /// </summary>
        private StreamingContext streamingContext;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Serializer" /> class.
        /// </summary>
        public Serializer()
            : this(null, null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Serializer" /> class.
        /// </summary>
        /// <param name="configuration">
        ///     The encoder configuration.
        /// </param>
        public Serializer(EncoderConfiguration configuration)
            : this(null, configuration, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Serializer" /> class.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        protected Serializer(BinaryWriter binaryWriter)
            : this(binaryWriter, null, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Serializer" /> class.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="configuration">
        ///     The encoder configuration.
        /// </param>
        /// <param name="streamingContext">
        ///     The streaming context object.
        /// </param>
        protected Serializer(BinaryWriter binaryWriter, EncoderConfiguration configuration, object streamingContext)
        {
            this.binaryWriter = binaryWriter;
            this.configuration = EncoderConfiguration.CreateFrom(configuration);
            this.streamingContext = new StreamingContext(StreamingContextStates.Persistence, streamingContext);

            this.encoderInfos.Add(typeof(Type).GetType(), new EncoderInfo(Tags.Type, this.WriteType));
            this.encoderInfos.Add(typeof(DateTime), new EncoderInfo(Tags.DateTime, this.WriteDateTime));
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the binary writer.
        /// </summary>
        /// <value>
        ///     The binary writer.
        /// </value>
        public BinaryWriter BinaryWriter => this.binaryWriter;

        /// <summary>
        ///     Gets the configuration.
        /// </summary>
        /// <value>
        ///     The encoder configuration.
        /// </value>
        public EncoderConfiguration Configuration => this.configuration;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Serializes the object specified.
        /// </summary>
        /// <param name="value">
        ///     The object to serialize.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the object to serialize.
        /// </typeparam>
        /// <returns>
        ///     The serialized object.
        /// </returns>
        public static byte[] ToByteArray<T>(T value)
        {
            return ToByteArray(value, SerializationBase.DefaultCapacity);
        }

        /// <summary>
        ///     Serializes the object specified.
        /// </summary>
        /// <param name="value">
        ///     The object to serialize.
        /// </param>
        /// <param name="capacity">
        ///     The internal memory stream initial capacity.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the object to serialize.
        /// </typeparam>
        /// <returns>
        ///     The serialized object.
        /// </returns>
        public static byte[] ToByteArray<T>(T value, int capacity)
        {
            return ToByteArray(typeof(T), value, capacity);
        }

        /// <summary>
        ///     Serializes the object specified.
        /// </summary>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="value">
        ///     The object to serialize.
        /// </param>
        /// <param name="capacity">
        ///     The internal memory stream initial capacity.
        /// </param>
        /// <returns>
        ///     The serialized object.
        /// </returns>
        public static byte[] ToByteArray(Type serializeType, object value, int capacity)
        {
#if HT4O_BUFFEREDBINARYWRITER
            using (var memoryStream = new WritableMemoryStream(capacity))
            {
                using (var binaryWriter = new BufferedBinaryWriter(memoryStream, true))
                {
                    new Serializer(binaryWriter).Write(serializeType, value);
                }

                return memoryStream.ToArray();
            }
#else
            using (var binaryWriter = new HeapBinaryWriter(capacity)) {
                new Serializer(binaryWriter).Write(serializeType, value);
                return binaryWriter.ToArray();
            }
#endif
        }

        /// <summary>
        ///     Deserialize from the stream specified.
        /// </summary>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <typeparam name="T">
        ///     Type of the object to deserialize.
        /// </typeparam>
        /// <returns>
        ///     The deserialized object.
        /// </returns>
        public T Deserialize<T>(Stream stream)
        {
            return Deserializer.Deserialize<T>(stream);
        }

        /// <summary>
        ///     Deserialize from the stream specified.
        /// </summary>
        /// <param name="destinationType">
        ///     The destination type.
        /// </param>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <returns>
        ///     The deserialized object.
        /// </returns>
        public object Deserialize(Type destinationType, Stream stream)
        {
            return Deserializer.Deserialize(destinationType, stream);
        }

        /// <summary>
        ///     Serializes the object specified.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the object to serialize.
        /// </typeparam>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <param name="value">
        ///     The object to serialize.
        /// </param>
        public void Serialize<T>(Stream stream, T value)
        {
            this.Serialize(stream, typeof(T), value);
        }

        /// <summary>
        ///     Serializes the object specified.
        /// </summary>
        /// <typeparam name="T">
        ///     Type of the object to serialize.
        /// </typeparam>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The object to serialize.
        /// </param>
        public void Serialize<T>(BinaryWriter binaryWriter, T value) {
            this.Serialize(binaryWriter, typeof(T), value);
        }

        /// <summary>
        ///     Serializes the object specified.
        /// </summary>
        /// <param name="stream">
        ///     The stream.
        /// </param>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="value">
        ///     The object to serialize.
        /// </param>
        public void Serialize(Stream stream, Type serializeType, object value)
        {
            using (this.binaryWriter = new BufferedBinaryWriter(stream, true))
            {
                this.Write(serializeType, value);
            }
        }

        /// <summary>
        ///     Serializes the object specified.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="value">
        ///     The object to serialize.
        /// </param>
        public void Serialize(BinaryWriter binaryWriter, Type serializeType, object value) {
            this.binaryWriter = binaryWriter;
            this.Write(serializeType, value);
        }

        /// <summary>
        ///     Writes an object.
        /// </summary>
        /// <param name="value">
        ///     The object to write.
        /// </param>
        public void WriteObject(object value)
            {
                this.Write(value?.GetType() ?? typeof(object), value);
            }

        #endregion

        #region Methods

        /// <summary>
        ///     Determine if the type tag requires an collection element tag.
        /// </summary>
        /// <param name="tag">
        ///     The type tag.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the type requires an collection element tag, otherwise <c>false</c>.
        /// </returns>
        internal static bool HasElementTag(Tags tag, Type type)
        {
            tag &= ~Tags.Array;
            return tag == Tags.Object || tag == Tags.String || tag == Tags.Tuple || tag == Tags.Uri
                   || (tag >= Tags.FirstCustomType && type.IsClass);
        }

        /// <summary>
        ///     Determine if the type from the tag specified.
        /// </summary>
        /// <param name="tag">
        ///     The type tag.
        /// </param>
        /// <returns>
        ///     The type or typeof(object).
        /// </returns>
        internal static Type TypeFromTag(Tags tag)
        {
            tag &= ~Tags.Array;
            switch (tag)
            {
                case Tags.SByte:
                    return typeof(sbyte);
                case Tags.Byte:
                    return typeof(byte);
                case Tags.Short:
                    return typeof(short);
                case Tags.UShort:
                    return typeof(ushort);
                case Tags.Int:
                    return typeof(int);
                case Tags.UInt:
                    return typeof(uint);
                case Tags.Long:
                    return typeof(long);
                case Tags.ULong:
                    return typeof(ulong);
                case Tags.Bool:
                    return typeof(bool);
                case Tags.Char:
                    return typeof(char);
                case Tags.Float:
                    return typeof(float);
                case Tags.Double:
                    return typeof(double);
                case Tags.Decimal:
                    return typeof(decimal);
                case Tags.DateTime:
                    return typeof(DateTime);
                case Tags.DateTimeOffset:
                    return typeof(DateTimeOffset);
                case Tags.String:
                    return typeof(string);
                case Tags.Guid:
                    return typeof(Guid);
                case Tags.Type:
                    return typeof(Type).GetType();
                case Tags.Uri:
                    return typeof(Uri);
            }

            return typeof(object);
        }

        /// <summary>
        ///     Encodes the object specified.
        /// </summary>
        /// <param name="encoderInfo">
        ///     The encoder info.
        /// </param>
        /// <param name="value">
        ///     The object to encode.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        internal virtual void Encode(EncoderInfo encoderInfo, object value, bool writeTag)
        {
            if (value != null && encoderInfo.HandleObjectRef(value.GetType()))
            {
                if (this.WriteOrAddObjectRef(value))
                {
                    return;
                }
            }

            encoderInfo.Encode(this, value, writeTag);
        }

        /// <summary>
        ///     Gets the type schema.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <returns>
        ///     The type schema.
        /// </returns>
        internal virtual TypeSchema GetTypeSchema(Type type, Inspector inspector)
        {
            return TypeSchemaDictionary.GetOrAdd(type, inspector, (t, i) => new TypeSchema(i, false));
        }

        /// <summary>
        ///     Write an object.
        /// </summary>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="type">
        ///     The object type.
        /// </param>
        /// <param name="value">
        ///     The object to write.
        /// </param>
        internal void Write(Type serializeType, Type type, object value)
        {
            var inspector = Inspector.InspectorForType(type);

            if (inspector.IsEnum)
            {
                ////TODO needs to be declared as enum - deserialize to object!!!!
                type = inspector.EnumType;
            }

            EncoderInfo encoderInfo;
            if (this.TryGetEncoder(type, out encoderInfo))
            {
                this.Encode(encoderInfo, value, true);
                return;
            }

            if (inspector.IsArray)
            {
                this.WriteArray(serializeType, type, (Array) value);
                return;
            }

            if (inspector.IsKeyValuePair)
            {
                this.WriteKeyValuePair(type, value);
                return;
            }

            if (inspector.IsTuple)
            {
                this.WriteTuple(type, value);
                return;
            }

            if (inspector.IsSerializable)
            {
                this.WriteSerializationInfo(inspector, serializeType, type, value);
                return;
            }

            var dictionary = value as IDictionary;
            if (dictionary != null)
            {
                this.WriteDictionary(serializeType, type, dictionary);
                return;
            }

            var enumerable = value as IEnumerable;

            ////TODO make better?
            if (enumerable != null && (inspector.IsCollection || !inspector.HasProperties))
            {
                this.WriteEnumerable(inspector, serializeType, type, enumerable);
                return;
            }

            this.WriteObject(inspector, serializeType, type, value);
        }

        /// <summary>
        ///     Writes an object.
        /// </summary>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="type">
        ///     The object type.
        /// </param>
        /// <param name="value">
        ///     The object to write.
        /// </param>
        internal virtual void WriteObject(Inspector inspector, Type serializeType, Type type, object value)
        {
            if (this.WriteOrAddObjectRef(value))
            {
                return;
            }

            inspector.OnSerializing?.Invoke(value, streamingContext);

            this.WriteObjectTrailer(type, value, inspector).WriteObject(this, value);

            inspector.OnSerialized?.Invoke(value, streamingContext);
        }

        /// <summary>
        ///     Writes the object trailer.
        /// </summary>
        /// <param name="type">
        ///     The object type.
        /// </param>
        /// <param name="value">
        ///     The object to write.
        /// </param>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <returns>
        ///     The type schema.
        /// </returns>
        internal TypeSchema WriteObjectTrailer(Type type, object value, Inspector inspector)
        {
            Encoder.WriteTag(this.binaryWriter, Tags.Object);
            this.WriteType(type, value);
            return this.WriteTypeSchema(type, inspector);
        }

        /// <summary>
        ///     Writes an object.
        /// </summary>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="type">
        ///     The object type.
        /// </param>
        /// <param name="value">
        ///     The object to write.
        /// </param>
        internal virtual void WriteSerializationInfo(Inspector inspector, Type serializeType, Type type, object value)
        {
            if (this.WriteOrAddObjectRef(value))
            {
                return;
            }

            var serializable = inspector.Serializable;
            var info = new SerializationInfo(serializable.InspectedType, new FormatterConverter());
            var streamingContext = new StreamingContext();
            ((ISerializable) value).GetObjectData(info, streamingContext);

            inspector.OnSerializing?.Invoke(value, streamingContext);

            Encoder.WriteTag(this.binaryWriter, Tags.SerializationInfo);
            this.WriteType(type, value);
            Encoder.WriteCount(this.binaryWriter, info.MemberCount);
            var enumerator = info.GetEnumerator();
            while (enumerator.MoveNext())
            {
                this.WriteString(enumerator.Name);
                value = enumerator.Value;
                this.WriteType(enumerator.ObjectType, value);
                this.WriteObject(value);
            }

            inspector.OnSerialized?.Invoke(value, streamingContext);
        }

        /// <summary>
        ///     Write an object.
        /// </summary>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        protected void Write(Type serializeType, object value)
        {
            if (value == null)
            {
                Encoder.WriteTag(this.binaryWriter, Tags.Null);
            }
            else
            {
                this.Write(serializeType, value.GetType(), value);
            }
        }

        /// <summary>
        ///     Writes or adds an object reference.
        /// </summary>
        /// <param name="value">
        ///     The object.
        /// </param>
        /// <returns>
        ///     <c>true</c> if an object reference has been written, otherwise <c>false</c>.
        /// </returns>
        protected bool WriteOrAddObjectRef(object value)
        {
            // Avoid extensive adding of value types to the identity dictionary
            if (!value.GetType().IsValueType())
            {
                if (this.WriteOrAddRef(this.identityDictionary, value, this.identityValue, Tags.ObjectRef))
                {
                    return true;
                }
            }

            ++this.identityValue;

            return false;
        }

        /// <summary>
        ///     Writes or adds a string reference.
        /// </summary>
        /// <param name="value">
        ///     The object.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a string reference has been written, otherwise <c>false</c>.
        /// </returns>
        protected bool WriteOrAddStringRef(string value)
        {
            return this.WriteOrAddRef(this.stringDictionary, value, this.stringDictionary.Count, Tags.StringRef);
        }

        /// <summary>
        ///     Writes or adds a type reference.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if a type reference has been written, otherwise <c>false</c>.
        /// </returns>
        protected bool WriteOrAddTypeRef(Type type)
        {
            return this.WriteOrAddRef(this.typeDictionary, type, this.typeDictionary.Count, Tags.TypeRef);
        }

        /// <summary>
        ///     Writes a string.
        /// </summary>
        /// <param name="value">
        ///     The value.
        /// </param>
        protected void WriteString(string value)
        {
            if (!string.IsNullOrEmpty(value) && this.WriteOrAddStringRef(value))
            {
                return;
            }

            Encoder.WriteString(this.binaryWriter, value, true);
        }

        /// <summary>
        ///     Writes a type.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        protected void WriteType(Type type, object value)
        {
            if (!Encoder.TryWriteTypeCode(this.binaryWriter, type, true))
            {
                if (this.WriteOrAddTypeRef(type))
                {
                    return;
                }

                Encoder.WriteType(this.binaryWriter, type, value, this.configuration, true);
            }
        }

        /// <summary>
        ///     Determine if the collection is typed collection or not.
        /// </summary>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the collection is typed collection, otherwise <c>false</c>.
        /// </returns>
        private static CollectionFlags GetCollectionFlags(Type serializeType, Type type)
        {
            var flags = CollectionFlags.None;

            if (serializeType == typeof(object) || serializeType.IsInterface || serializeType.IsAbstract)
            {
                if (type.IsGenericType)
                {
                    var baseType = type.GetGenericTypeDefinition();
                    if (baseType != typeof(List<>) && baseType != typeof(HashSet<>))
                    {
                        flags |= CollectionFlags.Typed;
                    }
                    else if (!typeof(IList).IsAssignableFrom(type))
                    {
                        var setType = typeof(ISet<>).MakeGenericType(type.GetGenericArguments());
                        if (setType.IsAssignableFrom(type))
                        {
                            flags |= CollectionFlags.Set;
                        }
                    }
                }
                else if (type != typeof(ArrayList))
                {
                    flags |= CollectionFlags.Typed;
                }
            }

            return flags;
        }

        /// <summary>
        ///     Determine if the dictionary is typed dictionary or not.
        /// </summary>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the dictionary is typed dictionary, otherwise <c>false</c>.
        /// </returns>
        private static DictionaryFlags GetDictionaryFlags(Type serializeType, Type type)
        {
            var flags = DictionaryFlags.None;

            if (serializeType == typeof(object) || serializeType.IsInterface || serializeType.IsAbstract)
            {
                if (type.IsGenericType)
                {
                    var baseType = type.GetGenericTypeDefinition();
                    if (baseType != typeof(Dictionary<,>))
                    {
                        flags |= DictionaryFlags.Typed;
                    }
                }
            }

            return flags;
        }

        /// <summary>
        ///     Write an object.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer.
        /// </param>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="getter">
        ///     The getter.
        /// </param>
        private static void Write(Serializer serializer, Type serializeType, object value, Func<object, object> getter)
        {
            serializer.Write(serializeType, getter(value));
        }

        /// <summary>
        ///     Writes a count.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        private static void WriteCount(Serializer serializer, int value)
        {
            Encoder.WriteCount(serializer.binaryWriter, value);
        }

        /// <summary>
        ///     Writes a type.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        private static void WriteType(Serializer serializer, Type type)
        {
            serializer.WriteType(type, null);
        }

        /// <summary>
        ///     Attempts to get the encoder info associated with the type.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="encoderInfo">
        ///     The encoder Info.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the decoder info exists for the tag specified, otherwise <c>false</c>.
        /// </returns>
        private bool TryGetEncoder(Type type, out EncoderInfo encoderInfo)
        {
            /*if (this.encoderInfos.TryGetValue(type, out encoderInfo)) {
                return true;
            }

            if (Encoder.TryGetEncoder(type, out encoderInfo)) {
                this.encoderInfos.Add(type, encoderInfo);
                return true;
            }

            return false;*/

            int bucket;
            FastDictionary<Type, EncoderInfo>.Entry entry;

            if (this.encoderInfos.TryGetValue(type, out bucket, out entry))
            {
                encoderInfo = entry.Value;
                return true;
            }

            if (Encoder.TryGetEncoder(type, out encoderInfo))
            {
                entry.Value = encoderInfo;
                this.encoderInfos.Insert(bucket, ref entry);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Writes an array.
        /// </summary>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="value">
        ///     The array to write.
        /// </param>
        private void WriteArray(Type serializeType, Type type, Array value)
        {
            if (this.WriteOrAddObjectRef(value))
            {
                return;
            }

            var elementType = type.GetElementType();

            // Dedicated handling of trivial byte arrays (performance)
            if (elementType == typeof(byte) && value.Rank == 1)
            {
                Encoder.WriteTag(this.binaryWriter, Tags.Byte | Tags.Array);
                // an array can have a maximum of 32 dimensions
                Encoder.WriteCount(this.binaryWriter, 1 | (byte) ArrayFlags.ValueNotTagged);
                Encoder.WriteCount(this.binaryWriter, value.Length);
                this.binaryWriter.Write((byte[]) value);
            }
            else
            {
                EncoderInfo encoderInfo;
                if (this.TryGetEncoder(elementType, out encoderInfo))
                {
                    var hasElementTag = HasElementTag(encoderInfo.Tag, elementType);

                    Encoder.WriteTag(this.binaryWriter, encoderInfo.Tag | Tags.Array);
                    // an array can have a maximum of 32 dimensions
                    Encoder.WriteCount(this.binaryWriter,
                        value.Rank | (byte) (hasElementTag ? ArrayFlags.ValueTagged : ArrayFlags.ValueNotTagged));
                    for (var dimension = 0; dimension < value.Rank; dimension++)
                    {
                        Encoder.WriteCount(this.binaryWriter, value.GetLength(dimension));
                    }

                    if (elementType.IsSealed || elementType.IsPrimitive)
                    {
                        foreach (var item in value)
                        {
                            this.Encode(encoderInfo, item, hasElementTag);
                        }
                    }
                    else
                    {
                        EncoderInfo itemEncoderInfo = null;
                        var itemType = typeof(object);

                        foreach (var item in value)
                        {
                            if (item != null && item.GetType() != elementType)
                            {
                                if (itemEncoderInfo == null || itemType != item.GetType())
                                {
                                    if (this.TryGetEncoder(item.GetType(), out itemEncoderInfo))
                                    {
                                        this.Encode(itemEncoderInfo, item, hasElementTag);
                                        itemType = item.GetType();
                                    }
                                    else
                                    {
                                        this.Write(
                                            serializeType != typeof(object[]) && serializeType != typeof(object)
                                                ? elementType
                                                : typeof(object), item);
                                    }
                                }
                                else
                                {
                                    this.Encode(itemEncoderInfo, item, hasElementTag);
                                }
                            }
                            else
                            {
                                this.Encode(encoderInfo, item, hasElementTag);
                            }
                        }
                    }
                }
                else
                {
                    Encoder.WriteTag(this.binaryWriter, Tags.Object | Tags.Array);
                    Encoder.WriteCount(this.binaryWriter, value.Rank);
                    for (var dimension = 0; dimension < value.Rank; dimension++)
                    {
                        Encoder.WriteCount(this.binaryWriter, value.GetLength(dimension));
                    }

                    var itemType = serializeType != typeof(object[]) && serializeType != typeof(object)
                        ? elementType
                        : typeof(object);
                    foreach (var item in value)
                    {
                        this.Write(itemType, item);
                    }
                }
            }
        }

        /// <summary>
        ///     Write a date time value to the binary writer.
        /// </summary>
        /// <param name="bw">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private void WriteDateTime(BinaryWriter bw, object value, bool writeTag)
        {
            Encoder.WriteDateTime(bw, (DateTime) value, this.configuration, writeTag);
        }

        /// <summary>
        ///     Writes a dictionary.
        /// </summary>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="value">
        ///     The dictionary to write.
        /// </param>
        private void WriteDictionary(Type serializeType, Type type, IDictionary value)
        {
            if (this.WriteOrAddObjectRef(value))
            {
                return;
            }

            var keyType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);
            var valueType = type.IsGenericType ? type.GetGenericArguments()[1] : typeof(object);

            Encoder.WriteTag(this.binaryWriter, Tags.Dictionary);

            var flags = GetDictionaryFlags(serializeType, type);

            EncoderInfo keyEncoderInfo;
            EncoderInfo valueEncoderInfo;
            if (this.TryGetEncoder(keyType, out keyEncoderInfo))
            {
                flags |= DictionaryFlags.KeyTypeTagged;
                var hasKeyElementTag = HasElementTag(keyEncoderInfo.Tag, keyType);
                flags |= hasKeyElementTag ? DictionaryFlags.KeyValueTagged : DictionaryFlags.None;

                if (this.TryGetEncoder(valueType, out valueEncoderInfo))
                {
                    flags |= DictionaryFlags.ValueTypeTagged;
                    var hasValueElementTag = HasElementTag(valueEncoderInfo.Tag, valueType);
                    flags |= hasValueElementTag ? DictionaryFlags.ValueTagged : DictionaryFlags.None;

                    Encoder.WriteByte(this.binaryWriter, (byte) flags, false);

                    if (flags.HasFlag(DictionaryFlags.Typed))
                    {
                        this.WriteType(type, null);
                    }

                    Encoder.WriteTag(this.binaryWriter, keyEncoderInfo.Tag);
                    Encoder.WriteTag(this.binaryWriter, valueEncoderInfo.Tag);

                    Encoder.WriteCount(this.binaryWriter, value.Count);
                    if ((keyType.IsSealed || keyType.IsPrimitive) && (valueType.IsSealed || valueType.IsPrimitive))
                    {
                        foreach (DictionaryEntry item in value)
                        {
                            this.Encode(keyEncoderInfo, item.Key, hasKeyElementTag);
                            this.Encode(valueEncoderInfo, item.Value, hasValueElementTag);
                        }
                    }
                    else
                    {
                        EncoderInfo keyItemEncoderInfo = null;
                        var keyItemType = typeof(object);

                        EncoderInfo valueItemEncoderInfo = null;
                        var valueItemType = typeof(object);

                        foreach (DictionaryEntry item in value)
                        {
                            if (item.Key.GetType() != valueType)
                            {
                                if (keyItemEncoderInfo == null || keyItemType != item.Key.GetType())
                                {
                                    if (this.TryGetEncoder(item.Key.GetType(), out keyItemEncoderInfo))
                                    {
                                        this.Encode(keyItemEncoderInfo, item.Key, hasKeyElementTag);
                                        keyItemType = item.Key.GetType();
                                    }
                                    else
                                    {
                                        this.Write(valueType, item.Key);
                                    }
                                }
                                else
                                {
                                    this.Encode(keyItemEncoderInfo, item.Key, hasKeyElementTag);
                                }
                            }
                            else
                            {
                                this.Encode(keyEncoderInfo, item.Key, hasKeyElementTag);
                            }

                            if (item.Value != null && item.Value.GetType() != valueType)
                            {
                                if (valueItemEncoderInfo == null || valueItemType != item.Value.GetType())
                                {
                                    if (this.TryGetEncoder(item.Value.GetType(), out valueItemEncoderInfo))
                                    {
                                        this.Encode(valueItemEncoderInfo, item.Value, hasValueElementTag);
                                        valueItemType = item.Value.GetType();
                                    }
                                    else
                                    {
                                        this.Write(valueType, item.Value);
                                    }
                                }
                                else
                                {
                                    this.Encode(valueItemEncoderInfo, item.Value, hasValueElementTag);
                                }
                            }
                            else
                            {
                                this.Encode(valueEncoderInfo, item.Value, hasValueElementTag);
                            }
                        }
                    }
                }
                else
                {
                    if (serializeType == typeof(object))
                    {
                        flags |= DictionaryFlags.Typed;
                    }

                    Encoder.WriteByte(this.binaryWriter, (byte) flags, false);
                    if (flags.HasFlag(DictionaryFlags.Typed))
                    {
                        this.WriteType(type, null);
                    }

                    Encoder.WriteTag(this.binaryWriter, keyEncoderInfo.Tag);
                    Encoder.WriteCount(this.binaryWriter, value.Count);
                    if (keyType.IsSealed || keyType.IsPrimitive)
                    {
                        foreach (DictionaryEntry item in value)
                        {
                            this.Encode(keyEncoderInfo, item.Key, hasKeyElementTag);
                            this.Write(valueType, item.Value);
                        }
                    }
                    else
                    {
                        EncoderInfo keyItemEncoderInfo = null;
                        var keyItemType = typeof(object);

                        foreach (DictionaryEntry item in value)
                        {
                            if (item.Key.GetType() != valueType)
                            {
                                if (keyItemEncoderInfo == null || keyItemType != item.Key.GetType())
                                {
                                    if (this.TryGetEncoder(item.Key.GetType(), out keyItemEncoderInfo))
                                    {
                                        this.Encode(keyItemEncoderInfo, item.Key, hasKeyElementTag);
                                        keyItemType = item.Key.GetType();
                                    }
                                    else
                                    {
                                        this.Write(valueType, item.Key);
                                    }
                                }
                                else
                                {
                                    this.Encode(keyItemEncoderInfo, item.Key, hasKeyElementTag);
                                }
                            }
                            else
                            {
                                this.Encode(keyEncoderInfo, item.Key, hasKeyElementTag);
                            }

                            this.Write(valueType, item.Value);
                        }
                    }
                }
            }
            else if (this.TryGetEncoder(valueType, out valueEncoderInfo))
            {
                flags |= DictionaryFlags.ValueTypeTagged;
                var hasValueElementTag = HasElementTag(valueEncoderInfo.Tag, valueType);
                flags |= hasValueElementTag ? DictionaryFlags.ValueTagged : DictionaryFlags.None;

                Encoder.WriteByte(this.binaryWriter, (byte) flags, false);
                if (flags.HasFlag(DictionaryFlags.Typed))
                {
                    this.WriteType(type, null);
                }

                Encoder.WriteTag(this.binaryWriter, valueEncoderInfo.Tag);

                Encoder.WriteCount(this.binaryWriter, value.Count);
                if (valueType.IsSealed || valueType.IsPrimitive)
                {
                    foreach (DictionaryEntry item in value)
                    {
                        this.Write(keyType, item.Key);
                        this.Encode(valueEncoderInfo, item.Value, hasValueElementTag);
                    }
                }
                else
                {
                    EncoderInfo valueItemEncoderInfo = null;
                    var valueItemType = typeof(object);

                    foreach (DictionaryEntry item in value)
                    {
                        this.Write(keyType, item.Key);
                        if (item.Value != null && item.Value.GetType() != valueType)
                        {
                            if (valueItemEncoderInfo == null || valueItemType != item.Value.GetType())
                            {
                                if (this.TryGetEncoder(item.Value.GetType(), out valueItemEncoderInfo))
                                {
                                    this.Encode(valueItemEncoderInfo, item.Value, hasValueElementTag);
                                    valueItemType = item.Value.GetType();
                                }
                                else
                                {
                                    this.Write(valueType, item.Value);
                                }
                            }
                            else
                            {
                                this.Encode(valueItemEncoderInfo, item.Value, hasValueElementTag);
                            }
                        }
                        else
                        {
                            this.Encode(valueEncoderInfo, item.Value, hasValueElementTag);
                        }
                    }
                }
            }
            else
            {
                if (serializeType == typeof(object))
                {
                    flags |= DictionaryFlags.Typed;
                }

                Encoder.WriteByte(this.binaryWriter, (byte) flags, false);
                if (flags.HasFlag(DictionaryFlags.Typed))
                {
                    this.WriteType(type, null);
                }

                Encoder.WriteCount(this.binaryWriter, value.Count);
                foreach (DictionaryEntry item in value)
                {
                    this.Write(keyType, item.Key);
                    this.Write(valueType, item.Value);
                }
            }
        }

        /// <summary>
        ///     Writes an enumerable.
        /// </summary>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <param name="serializeType">
        ///     The serialize type.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="value">
        ///     The enumerable to write.
        /// </param>
        private void WriteEnumerable(Inspector inspector, Type serializeType, Type type, IEnumerable value)
        {
            if (this.WriteOrAddObjectRef(value))
            {
                return;
            }

            var elementType = inspector.Enumerable.ElementType;
            var hasCount = inspector.Enumerable.HasCount;
            if (hasCount)
            {
                Encoder.WriteTag(this.binaryWriter, Tags.Collection);

                var flags = GetCollectionFlags(serializeType, type);
                EncoderInfo encoderInfo;
                if (this.TryGetEncoder(elementType, out encoderInfo))
                {
                    flags |= CollectionFlags.TypeTagged;
                    var hasElementTag = HasElementTag(encoderInfo.Tag, elementType);
                    flags |= hasElementTag ? CollectionFlags.ValueTagged : CollectionFlags.None;

                    Encoder.WriteByte(this.binaryWriter, (byte) flags, false);

                    if (flags.HasFlag(CollectionFlags.Typed))
                    {
                        this.WriteType(type, null);
                    }

                    Encoder.WriteTag(this.binaryWriter, encoderInfo.Tag);

                    Encoder.WriteCount(this.binaryWriter, (int) inspector.Enumerable.Count(value));

                    if (elementType.IsSealed || elementType.IsPrimitive)
                    {
                        foreach (var item in value)
                        {
                            this.Encode(encoderInfo, item, hasElementTag);
                        }
                    }
                    else
                    {
                        EncoderInfo itemEncoderInfo = null;
                        var itemType = typeof(object);

                        foreach (var item in value)
                        {
                            if (item != null && item.GetType() != elementType)
                            {
                                if (itemEncoderInfo == null || itemType != item.GetType())
                                {
                                    if (this.TryGetEncoder(item.GetType(), out itemEncoderInfo))
                                    {
                                        this.Encode(itemEncoderInfo, item, hasElementTag);
                                        itemType = item.GetType();
                                    }
                                    else
                                    {
                                        this.Write(elementType, item);
                                    }
                                }
                                else
                                {
                                    this.Encode(itemEncoderInfo, item, hasElementTag);
                                }
                            }
                            else
                            {
                                this.Encode(encoderInfo, item, hasElementTag);
                            }
                        }
                    }

                    return;
                }

                if (serializeType == typeof(object))
                {
                    flags |= CollectionFlags.Typed;
                }

                Encoder.WriteByte(this.binaryWriter, (byte) flags, false);
                if (flags.HasFlag(CollectionFlags.Typed))
                {
                    this.WriteType(type, null);
                }

                Encoder.WriteCount(this.binaryWriter, (int) inspector.Enumerable.Count(value));
            }
            else
            {
                Encoder.WriteTag(this.binaryWriter, Tags.Enumerable);
            }

            foreach (var item in value)
            {
                this.Write(elementType, item);
            }

            if (!hasCount)
            {
                Encoder.WriteTag(this.binaryWriter, Tags.End);
            }
        }

        /// <summary>
        ///     Writes a key/value pair.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="keyValuePair">
        ///     The key/value pair to write.
        /// </param>
        private void WriteKeyValuePair(Type type, object keyValuePair)
        {
            var typeargs = keyValuePair.GetType().GetGenericArguments();
            var propertyInfoKey = type.GetProperty(@"Key");
            var keyGetter = DelegateFactory.CreateGetter(propertyInfoKey);
            var propertyInfoValue = type.GetProperty(@"Value");
            var valueGetter = DelegateFactory.CreateGetter(propertyInfoValue);
            var write = typeof(Serializer).GetMethod(@"Write", BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] {typeof(Serializer), typeof(Type), typeof(object), typeof(Func<object, object>)}, null);
            var writeType = typeof(Serializer).GetMethod(@"WriteType", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] {typeof(Serializer), typeof(Type)}, null);

            var serializerParameter = Expression.Parameter(typeof(Serializer), @"serializer");
            var keyValuePairParameter = Expression.Parameter(typeof(object), @"keyValuePair");
            var expressions = new List<Expression>
            {
                Expression.Call(null, writeType, serializerParameter, Expression.Constant(typeargs[0])),
                Expression.Call(null, writeType, serializerParameter, Expression.Constant(typeargs[1])),
                Expression.Call(null, write, serializerParameter, Expression.Constant(typeargs[0]),
                    keyValuePairParameter, Expression.Constant(keyGetter)),
                Expression.Call(null, write, serializerParameter, Expression.Constant(typeargs[1]),
                    keyValuePairParameter, Expression.Constant(valueGetter))
            };

            var block = Expression.Block(expressions);
            var serializer = Expression.Lambda<Serialize>(block, serializerParameter, keyValuePairParameter).Compile();
            Encoder.Register(type, Tags.KeyValuePair, serializer);
            Encoder.WriteTag(this.binaryWriter, Tags.KeyValuePair);
            serializer(this, keyValuePair);
        }

        /// <summary>
        ///     Writes or adds an reference.
        /// </summary>
        /// <param name="dictionary">
        ///     The dictionary.
        /// </param>
        /// <param name="key">
        ///     The key.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        /// <returns>
        ///     <c>true</c> if an object reference has been written, otherwise <c>false</c>.
        /// </returns>
        /// <typeparam name="TKey">
        ///     The key type.
        /// </typeparam>
        /// <typeparam name="TComparer">
        ///     The comparer type.
        /// </typeparam>
        private bool WriteOrAddRef<TKey, TComparer>(FastDictionary<TKey, int, TComparer> dictionary, TKey key,
            int value, Tags tag) where TComparer : struct, IEqualityComparer<TKey>
        {
            /*int valueref;
            if (dictionary.TryGetValue(key, out valueref))
            {
                Encoder.WriteTag(this.binaryWriter, tag);
                Encoder.WriteCount(this.binaryWriter, valueref);
                return true;
            }

            dictionary.Add(key, value);
            return false;*/

            int valueref;
            if (dictionary.TryGetOrAddValue(key, out valueref, value))
            {
                Encoder.WriteTag(this.binaryWriter, tag);
                Encoder.WriteCount(this.binaryWriter, valueref);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Writes a tuple.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="tuple">
        ///     The tuple to write.
        /// </param>
        private void WriteTuple(Type type, object tuple)
        {
            var typeargs = tuple.GetType().GetGenericArguments();
            var write = typeof(Serializer).GetMethod(@"Write", BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] {typeof(Serializer), typeof(Type), typeof(object), typeof(Func<object, object>)}, null);
            var writeType = typeof(Serializer).GetMethod(@"WriteType", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] {typeof(Serializer), typeof(Type)}, null);
            var writeCount = typeof(Serializer).GetMethod(@"WriteCount", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] {typeof(Serializer), typeof(int)}, null);

            var serializerParameter = Expression.Parameter(typeof(Serializer), @"serializer");
            var tupleParameter = Expression.Parameter(typeof(object), @"serializerParameter");
            var expressions = new List<Expression>
            {
                Expression.Call(null, writeCount, serializerParameter, Expression.Constant(typeargs.Length))
            };

            for (var i = 0; i < typeargs.Length; ++i)
            {
                var propertyInfoItem = type.GetProperty(@"Item" + (i + 1).ToString(CultureInfo.InvariantCulture));
                var itemGetter = DelegateFactory.CreateGetter(propertyInfoItem);
                expressions.Add(Expression.Call(null, writeType, serializerParameter,
                    Expression.Constant(typeargs[i])));
                expressions.Add(Expression.Call(null, write, serializerParameter, Expression.Constant(typeargs[i]),
                    tupleParameter, Expression.Constant(itemGetter)));
            }

            var block = Expression.Block(expressions);
            var serializer = Expression.Lambda<Serialize>(block, serializerParameter, tupleParameter).Compile();
            Encoder.Register(type, Tags.Tuple, serializer);
            Encoder.WriteTag(this.binaryWriter, Tags.Tuple);
            serializer(this, tuple);
        }

        /// <summary>
        ///     Write a type value to the binary writer.
        /// </summary>
        /// <param name="bw">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private void WriteType(BinaryWriter bw, object value, bool writeTag)
        {
            Encoder.WriteType(bw, (Type) value, null, this.configuration, writeTag);
        }

        /// <summary>
        ///     Writes a type schema.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <returns>
        ///     The type schema written.
        /// </returns>
        private TypeSchema WriteTypeSchema(Type type, Inspector inspector)
        {
            /*TypeSchemaRef typeSchemaRef;
            if (this.typeSchemaRefDictionary.TryGetValue(type, out typeSchemaRef))
            {
                Encoder.WriteTag(this.binaryWriter, Tags.TypeSchemaRef);
                Encoder.WriteCount(this.binaryWriter, typeSchemaRef.Ref);
                return typeSchemaRef.TypeSchema;
            }

            var typeSchema = this.GetTypeSchema(type, inspector);
            this.binaryWriter.Write(typeSchema.SerializedSchema);

            typeSchemaRef.TypeSchema = typeSchema;
            typeSchemaRef.Ref = this.typeSchemaRefDictionary.Count;
            this.typeSchemaRefDictionary.Add(type, typeSchemaRef);
            return typeSchemaRef.TypeSchema;*/

            int bucket;
            FastDictionary<Type, TypeSchemaRef>.Entry entry;

            if (this.typeSchemaRefDictionary.TryGetValue(type, out bucket, out entry))
            {
                Encoder.WriteTag(this.binaryWriter, Tags.TypeSchemaRef);
                Encoder.WriteCount(this.binaryWriter, entry.Value.Ref);
                return entry.Value.TypeSchema;
            }

            var typeSchema = this.GetTypeSchema(type, inspector);
            this.binaryWriter.Write(typeSchema.SerializedSchema);

            entry.Value.TypeSchema = typeSchema;
            entry.Value.Ref = this.typeSchemaRefDictionary.Count;
            this.typeSchemaRefDictionary.Insert(bucket, ref entry);
            return entry.Value.TypeSchema;
        }

        #endregion

        #region Nested Types

        /// <summary>
        ///     The type schema reference.
        /// </summary>
        private struct TypeSchemaRef
        {
            #region Fields

            /// <summary>
            ///     The reference.
            /// </summary>
            public int Ref;

            /// <summary>
            ///     The type schema.
            /// </summary>
            public TypeSchema TypeSchema;

            #endregion
        }

        #endregion
    }
}