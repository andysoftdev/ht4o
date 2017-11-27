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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Hypertable.Persistence.Reflection;

    /// <summary>
    ///     The type schema.
    /// </summary>
    internal sealed class TypeSchema
    {
        #region Constants

        public const byte Version = 0;

        #endregion

        #region Fields

        /// <summary>
        ///     The serialized schema.
        /// </summary>
        private readonly Lazy<byte[]> serializedSchema;

        /// <summary>
        ///     The type schema properties.
        /// </summary>
        private readonly TypeSchemaProperty[] typeSchemaProperties;

        /// <summary>
        ///     The write object action.
        /// </summary>
        private readonly Action<Serializer, object> writeObject;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeSchema" /> class.
        /// </summary>
        /// <param name="typeSchemaProperties">
        ///     The type schema properties.
        /// </param>
        internal TypeSchema(TypeSchemaProperty[] typeSchemaProperties)
        {
            this.typeSchemaProperties = typeSchemaProperties;
            this.serializedSchema = new Lazy<byte[]>(this.GetSerializedSchema);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeSchema" /> class.
        /// </summary>
        /// <param name="inspector">
        ///     The inspector.
        /// </param>
        /// <param name="handlePropertyIgnore">
        ///     The handle property ignore.
        /// </param>
        internal TypeSchema(Inspector inspector, bool handlePropertyIgnore)
        {
            var properties = new List<TypeSchemaProperty>(inspector.Properties.Count);
            foreach (var property in inspector.Properties.Where(property => !handlePropertyIgnore || !property.Ignore))
            {
                var typeSchemaProperty = new TypeSchemaProperty();
                typeSchemaProperty.PropertyName = property.Name;
                typeSchemaProperty.InspectedProperty = property;
                Encoder.TryGetEncoder(property.PropertyType, out typeSchemaProperty.EncoderInfo);
                properties.Add(typeSchemaProperty);
            }

            this.typeSchemaProperties = properties.ToArray();
            this.writeObject = TypeSchema.CompileWriteObjectDelegate(this.typeSchemaProperties);
            this.serializedSchema = new Lazy<byte[]>(this.GetSerializedSchema);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the type schema properties.
        /// </summary>
        /// <value>
        ///     The type schema properties.
        /// </value>
        internal TypeSchemaProperty[] Properties => this.typeSchemaProperties;

        /// <summary>
        ///     Gets the serialized schema.
        /// </summary>
        /// <value>
        ///     The serialized schema.
        /// </value>
        internal byte[] SerializedSchema => this.serializedSchema.Value;

        /// <summary>
        ///     Gets the write object action.
        /// </summary>
        /// <value>
        ///     The write object action.
        /// </value>
        internal Action<Serializer, object> WriteObject => this.writeObject;

        #endregion

        #region Methods

        /// <summary>
        ///     Compiles the write object delegate.
        /// </summary>
        /// <param name="typeSchemaProperties">
        ///     The type schema properties.
        /// </param>
        /// <returns>
        ///     The write object action.
        /// </returns>
        private static Action<Serializer, object> CompileWriteObjectDelegate(
            ICollection<TypeSchemaProperty> typeSchemaProperties)
        {
            if (typeSchemaProperties.Count > 0)
            {
                var serializer = Expression.Parameter(typeof(Serializer), @"serializer");
                var any = Expression.Parameter(typeof(object), @"any");

                var writeProperty =
                    typeof(TypeSchema).GetMethod(@"WriteProperty", BindingFlags.Static | BindingFlags.NonPublic);
                var encodeProperty =
                    typeof(TypeSchema).GetMethod(@"EncodeProperty", BindingFlags.Static | BindingFlags.NonPublic);

                var expressions = new List<Expression>(typeSchemaProperties.Count);
                expressions.AddRange(
                    typeSchemaProperties.Select(
                        typeSchemaProperty =>
                            typeSchemaProperty.EncoderInfo == null
                                ? Expression.Call(null, writeProperty, serializer,
                                    Expression.Constant(typeSchemaProperty.InspectedProperty), any)
                                : Expression.Call(
                                    null,
                                    encodeProperty,
                                    serializer,
                                    Expression.Constant(typeSchemaProperty.InspectedProperty),
                                    Expression.Constant(typeSchemaProperty.EncoderInfo),
                                    any)));

                var block = Expression.Block(expressions);
                return Expression.Lambda<Action<Serializer, object>>(block, serializer, any).Compile();
            }

            return (serializer, o) => { };
        }

        /// <summary>
        ///     Encodes the specified property.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer.
        /// </param>
        /// <param name="inspectedProperty">
        ///     The inspected property.
        /// </param>
        /// <param name="encoderInfo">
        ///     The encoder info.
        /// </param>
        /// <param name="any">
        ///     The object to which the property belongs.
        /// </param>
        private static void EncodeProperty(Serializer serializer, InspectedProperty inspectedProperty,
            EncoderInfo encoderInfo, object any)
        {
            var value = inspectedProperty.Getter(any);
            if (value == null)
            {
                Encoder.WriteTag(serializer.BinaryWriter, Tags.Null);
            }
            else
            {
                serializer.Encode(encoderInfo, value, true);
            }
        }

        /// <summary>
        ///     Writes the specified property.
        /// </summary>
        /// <param name="serializer">
        ///     The serializer.
        /// </param>
        /// <param name="inspectedProperty">
        ///     The inspected property.
        /// </param>
        /// <param name="any">
        ///     The object to which the property belongs.
        /// </param>
        private static void WriteProperty(Serializer serializer, InspectedProperty inspectedProperty, object any)
        {
            var value = inspectedProperty.Getter(any);
            if (value == null)
            {
                Encoder.WriteTag(serializer.BinaryWriter, Tags.Null);
            }
            else
            {
                serializer.Write(inspectedProperty.PropertyType, value.GetType(), value);
            }
        }

        /// <summary>
        ///     Gets the serializeds schema.
        /// </summary>
        private byte[] GetSerializedSchema()
        {
            using (var ms = new MemoryStream())
            {
                using (var binaryWriter = new BufferedBinaryWriter(ms, System.Text.Encoding.ASCII))
                {
                    Encoder.WriteTag(binaryWriter, Tags.TypeSchema2);
                    Encoder.WriteByte(binaryWriter, TypeSchema.Version, false);
                    Encoder.WriteCount(binaryWriter, this.Properties.Length);
                    foreach (var typeSchemaProperty in this.Properties)
                    {
                        Encoder.WriteString(binaryWriter, typeSchemaProperty.PropertyName, false);
                    }
                }

                return ms.ToArray();
            }
        }

        #endregion
    }
}