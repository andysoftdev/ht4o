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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;

    using Hypertable.Persistence.Collections;
    using Hypertable.Persistence.Extensions;
    using Hypertable.Persistence.Reflection;
#if !HT4O_SERIALIZATION
    using Hypertable.Persistence.Scanner;

#endif

    /// <summary>
    /// The deserializer.
    /// </summary>
    public class Deserializer : SerializationBase
    {
        #region Constants

        /// <summary>
        /// The default references capacity.
        /// </summary>
        private const int DefaultRefsCapacity = 256;

        #endregion

        #region Fields

        /// <summary>
        /// The binary reader.
        /// </summary>
        private readonly BinaryReader binaryReader;

        /// <summary>
        /// The decoder infos.
        /// </summary>
        private readonly IDictionary<Tags, DecoderInfo> decoderInfos = new Dictionary<Tags, DecoderInfo>(256);

        /// <summary>
        /// The object references.
        /// </summary>
        private readonly ChunkedCollection<object> objectRefs = new ChunkedCollection<object>(DefaultRefsCapacity);

        /// <summary>
        /// The string references.
        /// </summary>
        private readonly ChunkedCollection<string> stringRefs = new ChunkedCollection<string>(DefaultRefsCapacity);

        /// <summary>
        /// The type references.
        /// </summary>
        private readonly ChunkedCollection<Type> typeRefs = new ChunkedCollection<Type>(DefaultRefsCapacity);

        /// <summary>
        /// The type schema references.
        /// </summary>
        private readonly List<TypeSchema> typeSchemaRefs = new List<TypeSchema>(32);

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Deserializer"/> class.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        protected Deserializer(BinaryReader binaryReader)
        {
            this.binaryReader = binaryReader;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the binary reader.
        /// </summary>
        /// <value>
        /// The binary reader.
        /// </value>
        public BinaryReader BinaryReader
        {
            get
            {
                return this.binaryReader;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the object references.
        /// </summary>
        /// <value>
        /// The object references.
        /// </value>
        internal ChunkedCollection<object> ObjectRefs
        {
            get
            {
                return this.objectRefs;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Deserialize from the stream specified.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <typeparam name="T">
        /// Type of the object to deserialize.
        /// </typeparam>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public static T Deserialize<T>(Stream stream)
        {
            using (var binaryReader = new BinaryReader(stream))
            {
                return FromByteArray<T>(binaryReader);
            }
        }

        /// <summary>
        /// Deserialize from the byte array specified.
        /// </summary>
        /// <param name="serialized">
        /// The serialized object.
        /// </param>
        /// <typeparam name="T">
        /// Type of the object to deserialize.
        /// </typeparam>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public static T FromByteArray<T>(byte[] serialized)
        {
            using (var binaryReader = new BinaryArrayReader(serialized))
            {
                return FromByteArray<T>(binaryReader);
            }
        }

        /// <summary>
        /// Deserialize from the byte array specified.
        /// </summary>
        /// <param name="serialized">
        /// The serialized object.
        /// </param>
        /// <param name="index">
        /// The index in the data at which deserialization begins
        /// </param>
        /// <typeparam name="T">
        /// Type of the object to deserialize.
        /// </typeparam>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public static T FromByteArray<T>(byte[] serialized, int index)
        {
            using (var binaryReader = new BinaryArrayReader(serialized, index))
            {
                return FromByteArray<T>(binaryReader);
            }
        }

        /// <summary>
        /// Deserialize from the byte array specified.
        /// </summary>
        /// <param name="serialized">
        /// The serialized object.
        /// </param>
        /// <param name="index">
        /// The index in the data at which deserialization begins
        /// </param>
        /// <param name="count">
        /// The number of bytes to deserialize.
        /// </param>
        /// <typeparam name="T">
        /// Type of the object to deserialize.
        /// </typeparam>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public static T FromByteArray<T>(byte[] serialized, int index, int count)
        {
            using (var binaryReader = new BinaryArrayReader(serialized, index, count))
            {
                return FromByteArray<T>(binaryReader);
            }
        }

        /// <summary>
        /// Deserialize from the byte array specified.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="serialized">
        /// The serialized object.
        /// </param>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public static object FromByteArray(Type destinationType, byte[] serialized)
        {
            using (var binaryReader = new BinaryArrayReader(serialized))
            {
                return new Deserializer(binaryReader).Deserialize(destinationType, Decoder.ReadTag(binaryReader));
            }
        }

        /// <summary>
        /// Deserialize from the byte array specified.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="serialized">
        /// The serialized object.
        /// </param>
        /// <param name="index">
        /// The index in the data at which deserialization begins
        /// </param>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public static object FromByteArray(Type destinationType, byte[] serialized, int index)
        {
            using (var binaryReader = new BinaryArrayReader(serialized, index))
            {
                return new Deserializer(binaryReader).Deserialize(destinationType, Decoder.ReadTag(binaryReader));
            }
        }

        /// <summary>
        /// Deserialize from the byte array specified.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="serialized">
        /// The serialized object.
        /// </param>
        /// <param name="index">
        /// The index in the data at which deserialization begins
        /// </param>
        /// <param name="count">
        /// The number of bytes to deserialize.
        /// </param>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        public static object FromByteArray(Type destinationType, byte[] serialized, int index, int count)
        {
            using (var binaryReader = new BinaryArrayReader(serialized, index, count))
            {
                return new Deserializer(binaryReader).Deserialize(destinationType, Decoder.ReadTag(binaryReader));
            }
        }

        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the object to read.
        /// </typeparam>
        /// <returns>
        /// The object read.
        /// </returns>
        public T ReadObject<T>()
        {
            var value = this.ReadObject(typeof(T));
            try
            {
                return (T)value;
            }
            catch (InvalidCastException e)
            {
#if!HT4O_SERIALIZATION

                if (value is EntitySpec)
                {
                    throw new SerializationException(
                        string.Format(CultureInfo.InvariantCulture, @"Unable to resolve entity reference for {0} , consider deferred reading", typeof(T)),
                        e);
                }

#endif

                throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid cast {0} to {1}", value.GetType(), typeof(T)), e);
            }
        }

        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The object read.
        /// </returns>
        public object ReadObject(Type type)
        {
            return this.Deserialize(type, Decoder.ReadTag(this.binaryReader));
        }

        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <typeparam name="T">
        /// Type of the object to read.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="action"/> is null.
        /// </exception>
        public void ReadObject<T>(Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var tag = Decoder.ReadTag(this.binaryReader);
            var value = this.Deserialize(typeof(T), tag);
            this.DeferredReadObject(tag, typeof(T), value, action);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determine if the type tag requires an collection element tag.
        /// </summary>
        /// <param name="tag">
        /// The type tag.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type requires an collection element tag, otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Required for reading old data, before v0.9.8.7
        /// </remarks>
        private static bool LegacyHasElementTag(Tags tag, Type type)
        {
            tag &= ~Tags.Array;
            return tag == Tags.Object || tag == Tags.Int || tag == Tags.UInt || tag == Tags.Long || tag == Tags.ULong || tag == Tags.Float || tag == Tags.Double
                   || (tag >= Tags.FirstCustomType && !type.IsSealed);
        }

        /// <summary>
        /// Before deserialize object properties notification.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="inspector">
        /// The inspector.
        /// </param>
        /// <param name="target">
        /// The target object.
        /// </param>
        internal virtual void BeforeDeserializeObjectProperties(Type destinationType, Inspector inspector, object target)
        {
        }

        /// <summary>
        /// Deserialize a property value.
        /// </summary>
        /// <param name="inspectedProperty">
        /// The inspected property.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The deserialized property value.
        /// </returns>
        internal object Deserialize(InspectedProperty inspectedProperty, Tags tag)
        {
            return this.Deserialize(inspectedProperty.PropertyType, tag);
        }

        /// <summary>
        /// Reads a dictionary item.
        /// </summary>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="keyType">
        /// The key type.
        /// </param>
        /// <param name="keyTag">
        /// The key tag.
        /// </param>
        /// <param name="valueType">
        /// The value type.
        /// </param>
        /// <param name="valueTag">
        /// The value tag.
        /// </param>
        /// <param name="inspector">
        /// The inspector.
        /// </param>
        /// <param name="dictionary">
        /// The dictionary.
        /// </param>
        internal virtual void ReadDictionaryItem(DictionaryFlags flags, Type keyType, Tags keyTag, Type valueType, Tags valueTag, Inspector inspector, IDictionary dictionary)
        {
            if (!flags.HasFlag(DictionaryFlags.KeyTypeTagged) || flags.HasFlag(DictionaryFlags.KeyValueTagged))
            {
                keyTag = Decoder.ReadTag(this.binaryReader);
            }

            var key = this.Deserialize(keyType, keyTag);
            if (!flags.HasFlag(DictionaryFlags.ValueTypeTagged) || flags.HasFlag(DictionaryFlags.ValueTagged))
            {
                valueTag = Decoder.ReadTag(this.binaryReader);
            }

            var value = this.Deserialize(valueType, valueTag);
            dictionary.Add(key, value);
        }

        /// <summary>
        /// Reads enumerable item.
        /// </summary>
        /// <param name="elementType">
        /// The element type.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="inspector">
        /// The inspector.
        /// </param>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="pos">
        /// The pos.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        internal virtual void ReadEnumerableItem(Type elementType, Tags tag, Inspector inspector, object collection, int pos, int count)
        {
            inspector.Enumerable.Add(collection, this.Deserialize(elementType, tag));
        }

        /// <summary>
        /// Sets an object property.
        /// </summary>
        /// <param name="inspectedProperty">
        /// The inspected property.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="value">
        /// The property value.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        internal virtual void SetObjectProperty(InspectedProperty inspectedProperty, object target, object value, Tags tag)
        {
            try
            {
                if (value == null && inspectedProperty.IsNotNullableValueType)
                {
                    value = Activator.CreateInstance(inspectedProperty.PropertyType, true);
                }

                inspectedProperty.Setter(target, value);
            }
            catch (Exception e)
            {
                throw new SerializationException(
                    string.Format(CultureInfo.InvariantCulture, "Set property value failed ({0}, {1}, {2})", inspectedProperty.PropertyType, inspectedProperty.InspectedType, tag),
                    e);
            }
        }

        /// <summary>
        /// Attempts to get the decoder info associated with the tag.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="decoderInfo">
        /// The decoder info.
        /// </param>
        /// <returns>
        /// <c>true</c> if the decoder info exists for the tag specified, otherwise <c>false</c>.
        /// </returns>
        internal bool TryGetDecoder(Tags tag, out DecoderInfo decoderInfo)
        {
            if (this.decoderInfos.TryGetValue(tag, out decoderInfo))
            {
                return true;
            }

            if (Decoder.TryGetDecoder(tag, out decoderInfo))
            {
                this.decoderInfos.Add(tag, decoderInfo);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deserializes an object from the binary reader specified.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <typeparam name="T">
        /// The type to deserialize.
        /// </typeparam>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        protected static T FromByteArray<T>(BinaryReader binaryReader)
        {
            return new Deserializer(binaryReader).Deserialize<T>();
        }

        /// <summary>
        /// The deferred read object.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <typeparam name="T">
        /// Type of object to read.
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="action"/> in null.
        /// </exception>
        protected virtual void DeferredReadObject<T>(Tags tag, Type destinationType, object value, Action<T> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            action((T)value);
        }

        /// <summary>
        /// Deserialize an object.
        /// </summary>
        /// <typeparam name="T">
        /// Type of object to deserialize.
        /// </typeparam>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        protected T Deserialize<T>()
        {
            return (T)this.Deserialize(typeof(T), Decoder.ReadTag(this.binaryReader));
        }

        /// <summary>
        /// Deserialize an object.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The deserialized object.
        /// </returns>
        protected object Deserialize(Type destinationType, Tags tag)
        {
            if ((tag & Tags.Array) > 0)
            {
                return this.ReadArray(destinationType, tag & ~Tags.Array);
            }

            return this.Read(destinationType, tag);
        }

        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The object read.
        /// </returns>
        protected virtual object Read(Type destinationType, Tags tag)
        {
            object value;
            if (tag >= Tags.FirstDecoderTag && tag <= Tags.LastDecoderTag)
            {
                if (!Decoder.TryReadValue(this.binaryReader, tag, out value))
                {
                    throw new SerializationException(@"Unknown tag");
                }
            }
            else
            {
                if (destinationType == typeof(object))
                {
                    destinationType = Serializer.TypeFromTag(tag);
                }

                switch (tag)
                {
                    case Tags.Null:
                        return null;

                    case Tags.Object:
                        value = this.ReadObj(destinationType);
                        break;

                    case Tags.ObjectRef:
                    {
                        var objref = Decoder.ReadCount(this.binaryReader);
                        if (objref < this.objectRefs.Count)
                        {
                            return this.objectRefs[objref];
                        }

                        throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid object reference {0}", objref));
                    }

                    case Tags.SerializationInfo:
                        return this.ReadSerializationInfo(destinationType);

                    case Tags.TypeRef:
                    {
                        var typeref = Decoder.ReadCount(this.binaryReader);
                        if (typeref < this.typeRefs.Count)
                        {
                            return this.typeRefs[typeref];
                        }

                        throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid type reference {0}", typeref));
                    }

                    case Tags.StringRef:
                    {
                        var stringref = Decoder.ReadCount(this.binaryReader);
                        if (stringref < this.stringRefs.Count)
                        {
                            return this.stringRefs[stringref];
                        }

                        throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid string reference {0}", stringref));
                    }

                    case Tags.TypeSchema:
                    case Tags.TypeSchema2:
                    case Tags.TypeSchemaRef:
                        value = this.ReadTypeSchema(tag);
                        break;

                    case Tags.KeyValuePair:
                        value = this.ReadKeyValuePair(destinationType);
                        break;

                    case Tags.Tuple:
                        value = this.ReadTuple(destinationType);
                        break;

                    case Tags.Collection:
                    {
                        var flags = (CollectionFlags)Decoder.ReadByte(this.binaryReader);
                        value = flags.HasFlag(CollectionFlags.Typed)
                            ? (flags.HasFlag(CollectionFlags.TypeTagged) 
                                ? this.ReadCollectionTypedTagged(destinationType, flags)
                                : this.ReadCollectionTyped(destinationType, flags))
                            : (flags.HasFlag(CollectionFlags.TypeTagged) 
                                ? this.ReadCollectionTagged(destinationType, flags)
                                : this.ReadCollection(destinationType, flags));
                        break;
                    }

                    case Tags.Enumerable:
                        value = this.ReadEnumerable(destinationType);
                        break;

                    case Tags.Dictionary:
                    {
                        var flags = (DictionaryFlags)Decoder.ReadByte(this.binaryReader);
                        value = flags.HasFlag(DictionaryFlags.Typed) 
                            ? this.ReadDictionaryTyped(destinationType, flags)
                            : this.ReadDictionary(destinationType, flags);
                        break;
                    }

                    default:
                    {
                        DecoderInfo decoderInfo;
                        if (!this.TryGetDecoder(tag, out decoderInfo))
                        {
                            throw new SerializationException(@"Invalid tag");
                        }

                        var objref = this.objectRefs.Count;
                        this.objectRefs.Add(null); // TODO DOES NOT WORK in all cases
                        value = decoderInfo.Deserialize(this);
                        this.objectRefs[objref] = value;

                        if (value != null)
                        {
                            this.BeforeDeserializeObjectProperties(destinationType, Inspector.InspectorForType(value.GetType()), value);
                        }

                        break;
                    }
                }
            }

            return destinationType.Convert(value);
        }

        /// <summary>
        /// Reads an array.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The array read.
        /// </returns>
        protected object ReadArray(Type destinationType, Tags tag)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            var rankAndFlags = Decoder.ReadCount(this.binaryReader);
            // an array can have a maximum of 32 dimensions
            var rank = rankAndFlags & (byte)ArrayFlags.RankMask;
            var lengths = new int[rank];
            for (var dimension = 0; dimension < rank; ++dimension)
            {
                lengths[dimension] = Decoder.ReadCount(this.binaryReader);
            }

            if (destinationType.IsArray || destinationType == typeof(object))
            {
                var elementType = destinationType.IsArray ? destinationType.GetElementType() : Serializer.TypeFromTag(tag);
                var hasElementTag = (rankAndFlags & (byte)ArrayFlags.ValueTagged) > 0
                                    || ((rankAndFlags & (byte)ArrayFlags.ValueNotTagged) == 0 && LegacyHasElementTag(tag, elementType));

                if (rank == 1 && typeof(byte) == elementType && tag == Tags.Byte && !hasElementTag)
                {
                    var bytes = this.binaryReader.ReadBytes(lengths[0]);
                    this.objectRefs.Add(bytes);
                    return bytes;
                }

                var array = Array.CreateInstance(elementType, lengths);
                this.objectRefs.Add(array);
                this.ReadArrayElementsInRank(array, 0, new int[rank], elementType, tag, hasElementTag);
                return array;
            }

            var inspector = InspectorForEnumerable(destinationType);
            if (inspector.Enumerable.HasAdd)
            {
                var obj = inspector.CreateInstance();
                if (obj != null)
                {
                    this.objectRefs.Add(obj);
                    var length = lengths.Aggregate(1, (current, rankLength) => current * rankLength);
                    if (inspector.Enumerable.HasCapacity)
                    {
                        inspector.Enumerable.Capacity(obj, length);
                    }

                    var elementType = inspector.Enumerable.ElementType;
                    var hasElementTag = (rankAndFlags & (byte)ArrayFlags.ValueTagged) > 0
                                        || ((rankAndFlags & (byte)ArrayFlags.ValueNotTagged) == 0 && LegacyHasElementTag(tag, elementType));

                    for (var n = 0; n < length; n++)
                    {
                        if (hasElementTag)
                        {
                            tag = Decoder.ReadTag(this.binaryReader);
                        }

                        this.ReadEnumerableItem(elementType, tag, inspector, obj, n, length);
                    }

                    return obj;
                }
            }

            throw new SerializationException(
                string.Format(CultureInfo.InvariantCulture, @"Invalid destination type {0}, expecting IList, IList<T>, ISet<T> or Array", destinationType));
        }

        /// <summary>
        /// Reads an array element.
        /// </summary>
        /// <param name="elementType">
        /// The element type.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="indexes">
        /// The indexes.
        /// </param>
        protected virtual void ReadArrayElement(Type elementType, Tags tag, Array array, int[] indexes)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            array.SetValue(this.Deserialize(elementType, tag), indexes);
        }

        /// <summary>
        /// Reads an array element in the array rank specified.
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="rank">
        /// The rank.
        /// </param>
        /// <param name="indexes">
        /// The indexes.
        /// </param>
        /// <param name="elementType">
        /// The element type.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="hasArrayElementTag">
        /// The has Array Element Tag.
        /// </param>
        protected void ReadArrayElementsInRank(Array array, int rank, int[] indexes, Type elementType, Tags tag, bool hasArrayElementTag)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }

            for (var i = 0; i < array.GetLength(rank); i++)
            {
                indexes[rank] = i;
                if (rank < array.Rank - 1)
                {
                    this.ReadArrayElementsInRank(array, rank + 1, indexes, elementType, tag, hasArrayElementTag);
                }
                else
                {
                    if (hasArrayElementTag)
                    {
                        tag = Decoder.ReadTag(this.binaryReader);
                    }

                    this.ReadArrayElement(elementType, tag, array, indexes);
                }
            }
        }

        /// <summary>
        /// Reads a collection.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="flags">
        /// The collection flags.
        /// </param>
        /// <returns>
        /// The collection read.
        /// </returns>
        protected object ReadCollection(Type destinationType, CollectionFlags flags)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            var count = Decoder.ReadCount(this.binaryReader);
            if (destinationType.IsArray)
            {
                var elementType = destinationType.GetElementType();
                var array = Array.CreateInstance(destinationType.GetElementType(), count);
                this.objectRefs.Add(array);
                for (var index = 0; index < count; ++index)
                {
                    var tag = Decoder.ReadTag(this.binaryReader);
                    array.SetValue(this.Deserialize(elementType, tag), index);
                }

                return array;
            }

            if (destinationType == typeof(object))
            {
                destinationType = flags.HasFlag(CollectionFlags.Set) ? typeof(HashSet<>).MakeGenericType(typeof(object)) : typeof(List<>).MakeGenericType(typeof(object));
            }

            var inspector = InspectorForEnumerable(destinationType);
            if (inspector.Enumerable.HasAdd)
            {
                var obj = inspector.CreateInstance();
                if (obj != null)
                {
                    this.objectRefs.Add(obj);
                    if (inspector.Enumerable.HasCapacity)
                    {
                        inspector.Enumerable.Capacity(obj, count);
                    }

                    var elementType = inspector.Enumerable.ElementType;
                    for (var n = 0; n < count; ++n)
                    {
                        var tag = Decoder.ReadTag(this.binaryReader);
                        this.ReadEnumerableItem(elementType, tag, inspector, obj, n, count);
                    }

                    return obj;
                }
            }

            throw new SerializationException(
                string.Format(CultureInfo.InvariantCulture, @"Invalid destination type {0}, expecting IList, IList<T>, ISet<T> or Array", destinationType));
        }

        /// <summary>
        /// Reads a tagged collection.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="flags">
        /// The collection flags.
        /// </param>
        /// <returns>
        /// The tagged collection read.
        /// </returns>
        protected object ReadCollectionTagged(Type destinationType, CollectionFlags flags)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            var tag = Decoder.ReadTag(this.binaryReader);
            var count = Decoder.ReadCount(this.binaryReader);
            if (destinationType.IsArray)
            {
                var elementType = destinationType.GetElementType();
                var array = Array.CreateInstance(destinationType.GetElementType(), count);
                this.objectRefs.Add(array);
                for (var index = 0; index < count; ++index)
                {
                    if (flags.HasFlag(CollectionFlags.ValueTagged))
                    {
                        tag = Decoder.ReadTag(this.binaryReader);
                    }

                    array.SetValue(this.Deserialize(elementType, tag), index);
                }

                return array;
            }

            if (destinationType == typeof(object))
            {
                destinationType = flags.HasFlag(CollectionFlags.Set)
                    ? typeof(HashSet<>).MakeGenericType(Serializer.TypeFromTag(tag))
                    : typeof(List<>).MakeGenericType(Serializer.TypeFromTag(tag));
            }

            var inspector = InspectorForEnumerable(destinationType);
            if (inspector.Enumerable.HasAdd)
            {
                var obj = inspector.CreateInstance();
                if (obj != null)
                {
                    this.objectRefs.Add(obj);
                    if (inspector.Enumerable.HasCapacity)
                    {
                        inspector.Enumerable.Capacity(obj, count);
                    }

                    var elementType = inspector.Enumerable.ElementType;
                    for (var n = 0; n < count; ++n)
                    {
                        if (flags.HasFlag(CollectionFlags.ValueTagged))
                        {
                            tag = Decoder.ReadTag(this.binaryReader);
                        }

                        this.ReadEnumerableItem(elementType, tag, inspector, obj, n, count);
                    }

                    return obj;
                }
            }

            throw new SerializationException(
                string.Format(CultureInfo.InvariantCulture, @"Invalid destination type {0}, expecting  IList<>, IList, ISet<> or Array", destinationType));
        }

        /// <summary>
        /// Reads a typed collection.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="flags">
        /// The collection flags.
        /// </param>
        /// <returns>
        /// The typed collection read.
        /// </returns>
        protected object ReadCollectionTyped(Type destinationType, CollectionFlags flags)
        {
            var type = this.ReadType();
            return
                this.ReadCollection(
                    destinationType == typeof(object) || destinationType.IsInterface || destinationType.IsAbstract || !typeof(IEnumerable).IsAssignableFrom(destinationType)
                        ? type
                        : destinationType,
                    flags);
        }

        /// <summary>
        /// Reads a typed and tagged collection.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="flags">
        /// The collection flags.
        /// </param>
        /// <returns>
        /// The typed and tagged collection read.
        /// </returns>
        protected object ReadCollectionTypedTagged(Type destinationType, CollectionFlags flags)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            var type = this.ReadType();
            return
                this.ReadCollectionTagged(
                    destinationType == typeof(object) || destinationType.IsInterface || destinationType.IsAbstract || !typeof(IEnumerable).IsAssignableFrom(destinationType)
                        ? type
                        : destinationType,
                    flags);
        }

        /// <summary>
        /// Reads a dictionary.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="flags">
        /// The dictionary flags.
        /// </param>
        /// <returns>
        /// The dictionary read.
        /// </returns>
        protected object ReadDictionary(Type destinationType, DictionaryFlags flags)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            var tagKey = flags.HasFlag(DictionaryFlags.KeyTypeTagged) ? Decoder.ReadTag(this.binaryReader) : Tags.Null;
            var keyType = Serializer.TypeFromTag(tagKey);

            var tagValue = flags.HasFlag(DictionaryFlags.ValueTypeTagged) ? Decoder.ReadTag(this.binaryReader) : Tags.Null;
            var valueType = Serializer.TypeFromTag(tagValue);

            if (destinationType == typeof(object))
            {
                destinationType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            }

            var inspector = InspectorForDictionary(destinationType);
            var obj = inspector.CreateInstance() as IDictionary;
            if (obj != null)
            {
                this.objectRefs.Add(obj);

                if (destinationType.IsGenericType)
                {
                    var genericArguments = destinationType.GetGenericArguments();
                    keyType = genericArguments[0];
                    valueType = genericArguments[1];
                }

                var count = Decoder.ReadCount(this.binaryReader);
                for (var n = 0; n < count; ++n)
                {
                    this.ReadDictionaryItem(flags, keyType, tagKey, valueType, tagValue, inspector, obj);
                }

                return obj;
            }

            throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid destination type {0}, expecting IDictionary or IDictionary<,>", destinationType));
        }

        /// <summary>
        /// Reads a typed dictionary.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <param name="flags">
        /// The dictionary flags.
        /// </param>
        /// <returns>
        /// The typed dictionary read.
        /// </returns>
        protected object ReadDictionaryTyped(Type destinationType, DictionaryFlags flags)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            var type = this.ReadType();
            return
                this.ReadDictionary(
                    destinationType == typeof(object) || destinationType.IsInterface || destinationType.IsAbstract || !typeof(IDictionary).IsAssignableFrom(destinationType)
                        ? type
                        : destinationType,
                    flags);
        }

        /// <summary>
        /// Reads an enumerable.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <returns>
        /// The enumerable read.
        /// </returns>
        protected object ReadEnumerable(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(object))
            {
                destinationType = typeof(object[]);
            }

            if (destinationType.IsArray)
            {
                var elementType = destinationType.GetElementType();
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), true);
                for (var tag = Decoder.ReadTag(this.binaryReader); tag != Tags.End; tag = Decoder.ReadTag(this.binaryReader))
                {
                    list.Add(this.Deserialize(elementType, tag));
                }

                var array = Array.CreateInstance(destinationType.GetElementType(), list.Count);
                this.objectRefs.Add(array);
                list.CopyTo(array, 0);
                return array;
            }

            var inspector = InspectorForEnumerable(destinationType);
            if (inspector.Enumerable.HasAdd)
            {
                var obj = inspector.CreateInstance();
                if (obj != null)
                {
                    this.objectRefs.Add(obj);
                    var elementType = inspector.Enumerable.ElementType;
                    var n = 0;
                    for (var tag = Decoder.ReadTag(this.binaryReader); tag != Tags.End; tag = Decoder.ReadTag(this.binaryReader))
                    {
                        this.ReadEnumerableItem(elementType, tag, inspector, obj, n++, -1);
                    }

                    return obj;
                }
            }

            throw new SerializationException(
                string.Format(CultureInfo.InvariantCulture, @"Invalid destination type {0}, expecting IList, IList<T>, ISet<T> or Array", destinationType));
        }

        /// <summary>
        /// Reads a key/value pair.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <returns>
        /// The key/value pair read.
        /// </returns>
        protected object ReadKeyValuePair(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            var genericArguments = new[] { this.ReadType(), this.ReadType() };
            var type = typeof(KeyValuePair<,>).MakeGenericType(genericArguments[0], genericArguments[1]);

            if (destinationType.IsGenericTypeDefinition(typeof(KeyValuePair<,>)))
            {
                if (!destinationType.IsAssignableFrom(type))
                {
                    type = destinationType;
                }
            }

            var ci = type.GetConstructor(genericArguments);
            return
                ci.Invoke(
                    new[] { this.Deserialize(genericArguments[0], Decoder.ReadTag(this.binaryReader)), this.Deserialize(genericArguments[1], Decoder.ReadTag(this.binaryReader)) });
        }

        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <returns>
        /// The object read.
        /// </returns>
        protected object ReadObj(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            Inspector inspector = null;
            object value = null;

            //// TODO call getter if real property

            // type
            var type = this.ReadType();
            if (destinationType.IsAssignableFrom(type) || destinationType.IsArray || typeof(IList).IsAssignableFrom(destinationType))
            {
                inspector = Inspector.InspectorForType(type);
                value = inspector.CreateInstance();
            }

            if (value == null)
            {
                if (destinationType != typeof(object))
                {
                    inspector = Inspector.InspectorForType(destinationType);
                    value = inspector.CreateInstance();
                }

                if (value == null)
                {
                    value = Resolver.InstanceResolver(type, destinationType);

                    if (value == null)
                    {
                        throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Unable to create instance for type {0}/{1}", type, destinationType));
                    }
                }
            }

            if (inspector == null)
            {
                throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Unable to create inspector for type {0}/{1}", type, destinationType));
            }

            var streamingContext = inspector.HasSerializationHandlers ? new StreamingContext() : default(StreamingContext);
            if (inspector.OnDeserializing != null)
            {
                inspector.OnDeserializing(value, streamingContext);
            }

            // type schema
            var typeSchema = this.ReadTypeSchema();
            this.objectRefs.Add(value);
            this.BeforeDeserializeObjectProperties(destinationType, inspector, value);

            var typeSchemaProperties = typeSchema.Properties;
            var length = typeSchemaProperties.Length;
            for (var i = 0; i < length; ++i)
            {
                // type schema will not be shared accross types, therefore we can assign the inspected property
                var inspectedProperty = typeSchemaProperties[i].InspectedProperty
                                        ?? inspector.GetProperty(typeSchemaProperties[i].PropertyName, i);

                var tag = Decoder.ReadTag(this.binaryReader);
                if (inspectedProperty != null)
                {
                    this.SetObjectProperty(inspectedProperty, value, this.Deserialize(inspectedProperty, tag), tag);
                }
                else
                {
                    // Alternate property name
                    if (Resolver.PropertyNameResolver != null)
                    {
                        var resolvedName = Resolver.PropertyNameResolver(type, typeSchemaProperties[i].PropertyName);
                        inspectedProperty = inspector.GetProperty(resolvedName, typeSchemaProperties[i].PropertyName);
                        if (inspectedProperty != null)
                        {
                            this.SetObjectProperty(
                                inspectedProperty,
                                value,
                                this.Deserialize(inspectedProperty, tag),
                                tag);
                        }

                        continue;
                    }

                    // Read unassignable values
                    Resolver.ObsoletePropertyResolver(value, this.Deserialize(typeof(object), tag));
                }
            }

            if (inspector.OnDeserialized != null)
            {
                inspector.OnDeserialized(value, streamingContext);
            }

            return value;
        }

        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <returns>
        /// The object read.
        /// </returns>
        protected object ReadSerializationInfo(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            Inspector inspector;
            var type = this.ReadType();
            if (destinationType.IsAssignableFrom(type))
            {
                inspector = Inspector.InspectorForType(type);
            }
            else if (destinationType != typeof(object))
            {
                inspector = Inspector.InspectorForType(destinationType);
            }
            else
            {
                throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Unable to create instance for type {0}", destinationType));
            }

            var serializable = inspector.Serializable;
            if (serializable == null)
            {
                throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Unable to create instance serialization info for type {0}", inspector.InspectedType));
            }

            var objref = this.objectRefs.Count;
            this.objectRefs.Add(null);

            var info = new SerializationInfo(serializable.InspectedType, new FormatterConverter());
            var count = Decoder.ReadCount(this.binaryReader);
            for (var m = 0; m < count; ++m)
            {
                var k = this.ReadString();
                var v = this.ReadObject(this.ReadType());

                info.AddValue(k, v);
            }

            var streamingContext = new StreamingContext();
            var value = serializable.CreateInstance(info, streamingContext);
            this.objectRefs[objref] = value;
            if (value != null)
            {
                this.BeforeDeserializeObjectProperties(destinationType, inspector, value);
            }

            if (inspector.OnDeserializing != null)
            {
                inspector.OnDeserializing(value, streamingContext);
            }

            var deserializationCallback = value as IDeserializationCallback;
            if (deserializationCallback != null)
            {
                deserializationCallback.OnDeserialization(this);
            }

            if (inspector.OnDeserialized != null)
            {
                inspector.OnDeserialized(value, streamingContext);
            }

            return value;
        }

        /// <summary>
        /// Reads a string value.
        /// </summary>
        /// <returns>
        /// The string read.
        /// </returns>
        protected string ReadString()
        {
            var tag = Decoder.ReadTag(this.binaryReader);
            switch (tag)
            {
                case Tags.Null:
                    return null;
                case Tags.StringEmpty:
                    return string.Empty;
                case Tags.String:
                    var value = Decoder.ReadString(this.binaryReader);
                    this.stringRefs.Add(value);
                    return value;
                case Tags.StringRef:
                    var stringref = Decoder.ReadCount(this.binaryReader);
                    if (stringref < this.stringRefs.Count)
                    {
                        return this.stringRefs[stringref];
                    }

                    throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid string reference {0}", stringref));
            }

            throw new SerializationException(@"Deserialize string failed");
        }

        /// <summary>
        /// Reads a tuple.
        /// </summary>
        /// <param name="destinationType">
        /// The destination type.
        /// </param>
        /// <returns>
        /// The tuple to read.
        /// </returns>
        protected object ReadTuple(Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            var size = Decoder.ReadCount(this.binaryReader);
            var items = new KeyValuePair<Type, object>[size];
            for (var i = 0; i < size; ++i)
            {
                var type = this.ReadType();
                items[i] = new KeyValuePair<Type, object>(type, this.Deserialize(type, Decoder.ReadTag(this.binaryReader)));
            }

            if (destinationType.IsTuple())
            {
                var typeArguments = items.Select(kv => kv.Key).ToArray();
                Type type;
                switch (size)
                {
                    case 1:
                        type = typeof(Tuple<>).MakeGenericType(typeArguments);
                        break;
                    case 2:
                        type = typeof(Tuple<,>).MakeGenericType(typeArguments);
                        break;
                    case 3:
                        type = typeof(Tuple<,,>).MakeGenericType(typeArguments);
                        break;
                    case 4:
                        type = typeof(Tuple<,,,>).MakeGenericType(typeArguments);
                        break;
                    case 5:
                        type = typeof(Tuple<,,,,>).MakeGenericType(typeArguments);
                        break;
                    case 6:
                        type = typeof(Tuple<,,,,,>).MakeGenericType(typeArguments);
                        break;
                    case 7:
                        type = typeof(Tuple<,,,,,,>).MakeGenericType(typeArguments);
                        break;
                    case 8:
                        type = typeof(Tuple<,,,,,,,>).MakeGenericType(typeArguments);
                        break;
                    default:
                        throw new SerializationException(@"Invalid item count for Tuple");
                }

                if (!destinationType.IsAssignableFrom(type))
                {
                    type = destinationType;
                }

                var ci = type.GetConstructor(typeArguments);
                return ci.Invoke(items.Select(kv => kv.Value).ToArray());
            }

            if (destinationType.IsGenericTypeDefinition(typeof(KeyValuePair<,>)) && size >= 2)
            {
                var type = typeof(KeyValuePair<,>).MakeGenericType(items[0].Key, items[1].Key);
                if (!destinationType.IsAssignableFrom(type))
                {
                    type = destinationType;
                }

                var ci = type.GetConstructor(new[] { items[0].Key, items[1].Key });
                return ci.Invoke(new[] { items[0].Value, items[1].Value });
            }

            throw new SerializationException(
                string.Format(CultureInfo.InvariantCulture, @"Invalid destination type {0}, expecting Tuple<,[,]>, or KeyValuePair<,>", destinationType));
        }

        /// <summary>
        /// Reads a type value.
        /// </summary>
        /// <returns>
        /// The type read.
        /// </returns>
        protected Type ReadType()
        {
            var tag = Decoder.ReadTag(this.binaryReader);
            switch (tag)
            {
                case Tags.Null:
                    return null;
                case Tags.Type:
                    var type = Decoder.TypeReader(this.binaryReader);
                    this.typeRefs.Add(type);
                    return type;
                case Tags.TypeCode:
                    return Decoder.ReadTypeCode(this.binaryReader);
                case Tags.TypeRef:
                    var typeref = Decoder.ReadCount(this.binaryReader);
                    if (typeref < this.typeRefs.Count)
                    {
                        return this.typeRefs[typeref];
                    }

                    throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid type reference {0}", typeref));
            }

            throw new SerializationException(@"Deserialize type failed");
        }

        /// <summary>
        /// Gets an inspector for a dictionary type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The inspector.
        /// </returns>
        private static Inspector InspectorForDictionary(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsInterface || type.IsAbstract)
            {
                if (type.IsGenericType)
                {
                    if (type.HasInterface(typeof(IDictionary<,>)) || typeof(IDictionary<,>).HasInterface(type))
                    {
                        type = typeof(Dictionary<,>).MakeGenericType(type.GetGenericArguments()[0], type.GetGenericArguments()[1]);
                    }
                }
                else if (type.HasInterface(typeof(IDictionary)) || typeof(IDictionary).HasInterface(type))
                {
                    type = typeof(Dictionary<object, object>);
                }
            }

            return Inspector.InspectorForType(type);
        }

        /// <summary>
        /// Gets an inspector for a enumerable type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The inspector.
        /// </returns>
        private static Inspector InspectorForEnumerable(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsInterface || type.IsAbstract)
            {
                if (type.IsGenericType)
                {
                    if (type.HasInterface(typeof(IList<>)))
                    {
                        type = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
                    }
                    else if (type.HasInterface(typeof(ISet<>)))
                    {
                        type = typeof(HashSet<>).MakeGenericType(type.GetGenericArguments()[0]);
                    }
                    else if (typeof(IList<>).HasInterface(type))
                    {
                        type = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
                    }
                    else if (typeof(ISet<>).HasInterface(type))
                    {
                        type = typeof(HashSet<>).MakeGenericType(type.GetGenericArguments()[0]);
                    }
                }
                else if (type.HasInterface(typeof(IList)) || typeof(IList).HasInterface(type))
                {
                    type = typeof(ArrayList);
                }
            }

            return Inspector.InspectorForType(type);
        }

        /// <summary>
        /// Reads the type schema.
        /// </summary>
        /// <returns>
        /// The type schema read.
        /// </returns>
        private TypeSchema ReadTypeSchema()
        {
            return this.ReadTypeSchema(Decoder.ReadTag(this.binaryReader));
        }

        /// <summary>
        /// Reads the type schema for the tag specified.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <returns>
        /// The type schema read.
        /// </returns>
        private TypeSchema ReadTypeSchema(Tags tag)
        {
            switch (tag)
            {
                case Tags.Null:
                    return null;
                case Tags.TypeSchema:
                {
                    var typeSchemaPropertyCount = Decoder.ReadCount(this.binaryReader);
                    var typeSchemaProperties = new TypeSchemaProperty[typeSchemaPropertyCount];
                    for (var i = 0; i < typeSchemaPropertyCount; ++i)
                    {
                        typeSchemaProperties[i] = new TypeSchemaProperty { PropertyName = this.ReadString() };
                    }

                    var typeSchema = new TypeSchema(typeSchemaProperties);
                    this.typeSchemaRefs.Add(typeSchema);
                    return typeSchema;
                }

                case Tags.TypeSchema2:
                {
                    switch (Decoder.ReadByte(this.binaryReader))
                    {
                        case TypeSchema.Version:
                            var typeSchemaPropertyCount = Decoder.ReadCount(this.binaryReader);
                            var typeSchemaProperties = new TypeSchemaProperty[typeSchemaPropertyCount];
                            for (var i = 0; i < typeSchemaPropertyCount; ++i)
                            {
                                typeSchemaProperties[i] = new TypeSchemaProperty { PropertyName = Decoder.ReadString(this.binaryReader) };
                            }

                            var typeSchema = new TypeSchema(typeSchemaProperties);
                            this.typeSchemaRefs.Add(typeSchema);
                            return typeSchema;
                    }

                    throw new NotSupportedException("Unsupported type schema");
                }

                case Tags.TypeSchemaRef:
                    var typeschemaref = Decoder.ReadCount(this.binaryReader);
                    if (typeschemaref < this.typeSchemaRefs.Count)
                    {
                        return this.typeSchemaRefs[typeschemaref];
                    }

                    throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid type schema reference {0}", typeschemaref));
            }

            throw new SerializationException(@"Deserialize type schema failed");
        }

        #endregion
    }
}