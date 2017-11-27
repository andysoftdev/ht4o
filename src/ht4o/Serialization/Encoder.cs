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
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using Hypertable.Persistence.Collections.Concurrent;
    using Hypertable.Persistence.Serialization.Delegates;

    /// <summary>
    ///     The encoder.
    /// </summary>
    public static class Encoder
    {
        #region Constants

        /// <summary>
        ///     DateTime ticks stored as local time.
        /// </summary>
        internal const byte LocalTimeTicks = 0x80;

        #endregion

        #region Static Fields

        /// <summary>
        ///     The encoder configuration.
        /// </summary>
        private static readonly EncoderConfiguration EncoderConfiguration = new EncoderConfiguration();

        /// <summary>
        ///     The encoder info.
        /// </summary>
        private static readonly ConcurrentTypeDictionary<EncoderInfo> EncoderInfos;

        /// <summary>
        ///     The type codes.
        /// </summary>
        private static readonly ConcurrentTypeDictionary<int> TypeCodes = new ConcurrentTypeDictionary<int>();

        /// <summary>
        ///     The type name cache.
        /// </summary>
        private static readonly ConcurrentTypeDictionary<Tuple<string, string>> TypeNameCache =
            new ConcurrentTypeDictionary<Tuple<string, string>>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="Encoder" /> class.
        /// </summary>
        static Encoder()
        {
            EncoderConfiguration.Binder = new Binder();
            EncoderConfiguration.TypeWriter = WriteType;

            var encoderInfos = new Dictionary<Type, EncoderInfo>
            {
                {typeof(sbyte), new EncoderInfo(Tags.SByte, WriteSByte)},
                {typeof(byte), new EncoderInfo(Tags.Byte, WriteByte)},
                {typeof(short), new EncoderInfo(Tags.Short, WriteShort)},
                {typeof(ushort), new EncoderInfo(Tags.UShort, WriteUShort)},
                {typeof(int), new EncoderInfo(Tags.Int, WriteInt)},
                {typeof(uint), new EncoderInfo(Tags.UInt, WriteUInt)},
                {typeof(long), new EncoderInfo(Tags.Long, WriteLong)},
                {typeof(ulong), new EncoderInfo(Tags.ULong, WriteULong)},
                {typeof(bool), new EncoderInfo(Tags.Bool, WriteBool)},
                {typeof(char), new EncoderInfo(Tags.Char, WriteChar)},
                {typeof(float), new EncoderInfo(Tags.Float, WriteFloat)},
                {typeof(double), new EncoderInfo(Tags.Double, WriteDouble)},
                {typeof(decimal), new EncoderInfo(Tags.Decimal, WriteDecimal)},
                {typeof(DateTime), new EncoderInfo(Tags.DateTime, WriteDateTime)},
                {typeof(DateTimeOffset), new EncoderInfo(Tags.DateTimeOffset, WriteDateTimeOffset)},
                {typeof(TimeSpan), new EncoderInfo(Tags.TimeSpan, WriteTimeSpan)},
                {typeof(string), new EncoderInfo(Tags.String, WriteString)},
                {typeof(StringBuilder), new EncoderInfo(Tags.String, WriteStringBuilder)},
                {typeof(Guid), new EncoderInfo(Tags.Guid, WriteGuid)},
                {typeof(Type).GetType(), new EncoderInfo(Tags.Type, WriteType)},
                {typeof(Uri), new EncoderInfo(Tags.Uri, WriteUri)}
            };

            EncoderInfos = new ConcurrentTypeDictionary<EncoderInfo>(encoderInfos);

            RegisterInternalTypeCode((int) Tags.Object, typeof(object));
            foreach (var kv in encoderInfos)
            {
                RegisterInternalTypeCode((int) kv.Value.Tag, kv.Key);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the configuration.
        /// </summary>
        /// <value>
        ///     The encoder configuration.
        /// </value>
        public static EncoderConfiguration Configuration => EncoderConfiguration;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Register a type by the type code specified.
        /// </summary>
        /// <param name="typeCode">
        ///     The type code.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the type code has been registered successfully, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="type" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If a different type code already registered for type.
        /// </exception>
        public static bool Register(int typeCode, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var internalTypeCode = ToInternalTypeCode(typeCode);

            int existingTypeCode;
            if (TypeCodes.TryGetValue(type, out existingTypeCode) && existingTypeCode != internalTypeCode)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    "Different type code already registered for type {0}", type));
            }

            return RegisterInternalTypeCode(internalTypeCode, type);
        }

        /// <summary>
        ///     Register a custom serializer by the type and type code specified.
        /// </summary>
        /// <param name="typeCode">
        ///     The type code.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="serialize">
        ///     The serialize delegate.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the deserialize delegate has been registered successfully, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="type" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="serialize" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="typeCode" /> is not in the valid range.
        /// </exception>
        public static bool Register(int typeCode, Type type, Serialize serialize)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (serialize == null)
            {
                throw new ArgumentNullException(nameof(serialize));
            }

            Register(typeCode, type);
            var internalTypeCode = ToInternalTypeCode(typeCode);
            return EncoderInfos.TryAdd(type, new EncoderInfo((Tags) internalTypeCode, serialize));
        }

        /// <summary>
        ///     Register a custom serializer/deserializer by the type and type code specified.
        /// </summary>
        /// <param name="typeCode">
        ///     The type code.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="serialize">
        ///     The serialize delegate.
        /// </param>
        /// <param name="deserialize">
        ///     The deserialize delegate.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the serializer and deserialize delegate has been registered successfully, otherwise <c>false</c>.
        /// </returns>
        public static bool Register(int typeCode, Type type, Serialize serialize, Deserialize deserialize)
        {
            return Register(typeCode, type, serialize) && Decoder.Register(typeCode, type, deserialize);
        }

        /// <summary>
        ///     Attempts to write a type code for the type specified.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the type code has been successfully written, otherwise <c>false</c>.
        /// </returns>
        public static bool TryWriteTypeCode(BinaryWriter binaryWriter, Type type, bool writeTag)
        {
            int typeCode;
            if (!TypeCodes.TryGetValue(type, out typeCode))
            {
                return false;
            }

            if (writeTag)
            {
                WriteTag(binaryWriter, Tags.TypeCode);
            }

            WriteIntVariant(binaryWriter, typeCode);
            return true;
        }

        /// <summary>
        ///     Write a bool value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        public static void WriteBool(BinaryWriter binaryWriter, bool value)
        {
            WriteTag(binaryWriter, value ? Tags.True : Tags.False);
        }

        /// <summary>
        ///     Write a byte value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteByte(BinaryWriter binaryWriter, byte value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0)
                {
                    WriteTag(binaryWriter, Tags.Byte0);
                    return;
                }

                WriteTag(binaryWriter, Tags.Byte);
            }

            binaryWriter.Write(value);
        }

        /// <summary>
        ///     Write a char value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteChar(BinaryWriter binaryWriter, char value, bool writeTag)
        {
            if (writeTag)
            {
                WriteTag(binaryWriter, Tags.Char);
            }

            binaryWriter.Write(value);
        }

        /// <summary>
        ///     Write a collection count value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        public static void WriteCount(BinaryWriter binaryWriter, int value)
        {
            WriteUInt(binaryWriter, unchecked((uint) value), false);
        }

        /// <summary>
        ///     Write a date time value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="configuration">
        ///     The encoder configuration.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteDateTime(BinaryWriter binaryWriter, DateTime value, EncoderConfiguration configuration,
            bool writeTag)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (writeTag)
            {
                if (value.Ticks == 0)
                {
                    WriteTag(binaryWriter, Tags.DateTime0);
                    return;
                }

                WriteTag(binaryWriter, Tags.DateTime);
            }

            if (configuration.DateTimeBehavior == DateTimeBehavior.Utc)
            {
                WriteByte(binaryWriter, (byte) value.Kind, false);
                WriteLongVariant(binaryWriter,
                    value.Kind == DateTimeKind.Local ? value.ToUniversalTime().Ticks : value.Ticks);
            }
            else
            {
                var kindAndFlag = (byte) value.Kind;
                if (value.Kind == DateTimeKind.Local)
                {
                    kindAndFlag |= LocalTimeTicks;
                }

                WriteByte(binaryWriter, kindAndFlag, false);
                WriteLongVariant(binaryWriter, value.Ticks);
            }
        }

        /// <summary>
        ///     Write a date time offset value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteDateTimeOffset(BinaryWriter binaryWriter, DateTimeOffset value, bool writeTag)
        {
            if (writeTag)
            {
                if (value.Ticks == 0 && value.Offset.Ticks == 0)
                {
                    WriteTag(binaryWriter, Tags.DateTimeOffset0);
                    return;
                }

                WriteTag(binaryWriter, Tags.DateTimeOffset);
            }

            WriteLongVariant(binaryWriter, value.Ticks);
            WriteLongVariant(binaryWriter, value.Offset.Ticks);
        }

        /// <summary>
        ///     Write a decimal value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteDecimal(BinaryWriter binaryWriter, decimal value, bool writeTag)
        {
            if (writeTag)
            {
                WriteTag(binaryWriter, Tags.Decimal);
            }

            binaryWriter.Write(value);
        }

        /// <summary>
        ///     Write a double value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteDouble(BinaryWriter binaryWriter, double value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0.0)
                {
                    WriteTag(binaryWriter, Tags.Double0);
                    return;
                }

                if (double.IsNaN(value))
                {
                    WriteTag(binaryWriter, Tags.DoubleNaN);
                    return;
                }

                WriteTag(binaryWriter, Tags.Double);
            }

            binaryWriter.Write(value);
        }

        /// <summary>
        ///     Write a float value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteFloat(BinaryWriter binaryWriter, float value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0.0f)
                {
                    WriteTag(binaryWriter, Tags.Float0);
                    return;
                }

                if (float.IsNaN(value))
                {
                    WriteTag(binaryWriter, Tags.FloatNaN);
                    return;
                }

                WriteTag(binaryWriter, Tags.Float);
            }

            binaryWriter.Write(value);
        }

        /// <summary>
        ///     Write a guid value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteGuid(BinaryWriter binaryWriter, Guid value, bool writeTag)
        {
            if (writeTag)
            {
                WriteTag(binaryWriter, Tags.Guid);
            }

            binaryWriter.Write(value.ToByteArray());
        }

        /// <summary>
        ///     Write a int value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteInt(BinaryWriter binaryWriter, int value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0)
                {
                    WriteTag(binaryWriter, Tags.Int0);
                    return;
                }

                WriteTag(binaryWriter, Tags.Int);
            }

            WriteIntVariant(binaryWriter, value);
        }

        /// <summary>
        ///     Write a long value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteLong(BinaryWriter binaryWriter, long value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0)
                {
                    WriteTag(binaryWriter, Tags.Long0);
                    return;
                }

                WriteTag(binaryWriter, Tags.Long);
            }

            WriteLongVariant(binaryWriter, value);
        }

        /// <summary>
        ///     Write a signed byte value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        [CLSCompliant(false)]
        public static void WriteSByte(BinaryWriter binaryWriter, sbyte value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0)
                {
                    WriteTag(binaryWriter, Tags.SByte0);
                    return;
                }

                WriteTag(binaryWriter, Tags.SByte);
            }

            binaryWriter.Write(value);
        }

        /// <summary>
        ///     Write a short value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteShort(BinaryWriter binaryWriter, short value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0)
                {
                    WriteTag(binaryWriter, Tags.Short0);
                    return;
                }

                WriteTag(binaryWriter, Tags.Short);
            }

            WriteShortVariant(binaryWriter, value);
        }

        /// <summary>
        ///     Write a string value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteString(BinaryWriter binaryWriter, string value, bool writeTag)
        {
            if (writeTag)
            {
                if (string.IsNullOrEmpty(value))
                {
                    WriteTag(binaryWriter, value == null ? Tags.Null : Tags.StringEmpty);
                    return;
                }

                WriteTag(binaryWriter, Tags.String);
            }

            binaryWriter.Write(value);
        }

        /// <summary>
        ///     Write a time span value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteTimeSpan(BinaryWriter binaryWriter, TimeSpan value, bool writeTag)
        {
            if (writeTag)
            {
                if (value.Ticks == 0)
                {
                    WriteTag(binaryWriter, Tags.TimeSpan0);
                    return;
                }

                WriteTag(binaryWriter, Tags.TimeSpan);
            }

            WriteLongVariant(binaryWriter, value.Ticks);
        }

        /// <summary>
        ///     Write a type value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteType(BinaryWriter binaryWriter, Type value, bool writeTag)
        {
            WriteType(binaryWriter, value, EncoderConfiguration, writeTag);
        }

        /// <summary>
        ///     Write a unsigned int value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        [CLSCompliant(false)]
        public static void WriteUInt(BinaryWriter binaryWriter, uint value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0)
                {
                    WriteTag(binaryWriter, Tags.UInt0);
                    return;
                }

                WriteTag(binaryWriter, Tags.UInt);
            }

            WriteUIntVariant(binaryWriter, value);
        }

        /// <summary>
        ///     Write a unsigned long value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        [CLSCompliant(false)]
        public static void WriteULong(BinaryWriter binaryWriter, ulong value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0)
                {
                    WriteTag(binaryWriter, Tags.ULong0);
                    return;
                }

                WriteTag(binaryWriter, Tags.ULong);
            }

            WriteULongVariant(binaryWriter, value);
        }

        /// <summary>
        ///     Write a uri value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        public static void WriteUri(BinaryWriter binaryWriter, Uri value, bool writeTag)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (writeTag)
            {
                WriteTag(binaryWriter, Tags.Uri);
            }

            binaryWriter.Write(value.ToString());
        }

        /// <summary>
        ///     Write a unsigned short value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        [CLSCompliant(false)]
        public static void WriteUShort(BinaryWriter binaryWriter, ushort value, bool writeTag)
        {
            if (writeTag)
            {
                if (value == 0)
                {
                    WriteTag(binaryWriter, Tags.UShort0);
                    return;
                }

                WriteTag(binaryWriter, Tags.UShort);
            }

            WriteUShortVariant(binaryWriter, value);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Register a custom serializer by the type and tag specified.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        /// <param name="serialize">
        ///     The serialize delegate.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the deserialize delegate has been registered successfully, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="type" /> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     If <paramref name="serialize" /> is null.
        /// </exception>
        internal static bool Register(Type type, Tags tag, Serialize serialize)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (serialize == null)
            {
                throw new ArgumentNullException(nameof(serialize));
            }

            return EncoderInfos.TryAdd(type, new EncoderInfo(tag, serialize));
        }

        /// <summary>
        ///     Converts a type code to it's internal value.
        /// </summary>
        /// <param name="typeCode">
        ///     The type code.
        /// </param>
        /// <returns>
        ///     The <see cref="int" />.
        ///     The internal type code value.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="typeCode" /> is out of the valid range.
        /// </exception>
        internal static int ToInternalTypeCode(int typeCode)
        {
            if (typeCode < 0 || 2 * typeCode + (long) Tags.FirstCustomType > int.MaxValue)
            {
                throw new ArgumentException("Invalid type code", nameof(typeCode));
            }

            return 2 * typeCode + (int) Tags.FirstCustomType;
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
        internal static bool TryGetEncoder(Type type, out EncoderInfo encoderInfo)
        {
            return EncoderInfos.TryGetValue(type, out encoderInfo);
        }

        /// <summary>
        ///     Write a tag to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="tag">
        ///     The tag.
        /// </param>
        internal static void WriteTag(BinaryWriter binaryWriter, Tags tag)
        {
            WriteUIntVariant(binaryWriter, unchecked((uint) tag));
        }

        /// <summary>
        ///     Write a type value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="configuration">
        ///     The encoder configuration.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        internal static void WriteType(BinaryWriter binaryWriter, Type value, EncoderConfiguration configuration,
            bool writeTag)
        {
            if (configuration.StrictExplicitTypeCodes)
            {
                throw new SerializationException(string.Format(CultureInfo.InvariantCulture,
                    @"Missing type code for type {0}", value));
            }

            if (writeTag)
            {
                WriteTag(binaryWriter, Tags.Type);
            }

            configuration.TypeWriter(binaryWriter, value, configuration);
        }

        /// <summary>
        ///     The register internal type code.
        /// </summary>
        /// <param name="internalTypeCode">
        ///     The internal type code.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the type code has been registered successfully, otherwise <c>false</c>.
        /// </returns>
        private static bool RegisterInternalTypeCode(int internalTypeCode, Type type)
        {
            if (!Decoder.RegisterInternalTypeCode(internalTypeCode, type))
            {
                return false;
            }

            return TypeCodes.TryAdd(type, internalTypeCode);
        }

        /// <summary>
        ///     Write a bool value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteBool(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteBool(binaryWriter, (bool) value);
        }

        /// <summary>
        ///     Write a byte value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteByte(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteByte(binaryWriter, (byte) value, writeTag);
        }

        /// <summary>
        ///     Write a char value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteChar(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteChar(binaryWriter, (char) value, writeTag);
        }

        /// <summary>
        ///     Write a date time value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteDateTime(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteDateTime(binaryWriter, (DateTime) value, EncoderConfiguration, writeTag);
        }

        /// <summary>
        ///     Write a date time offset value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteDateTimeOffset(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteDateTimeOffset(binaryWriter, (DateTimeOffset) value, writeTag);
        }

        /// <summary>
        ///     Write a decimal value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteDecimal(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteDecimal(binaryWriter, (decimal) value, writeTag);
        }

        /// <summary>
        ///     Write a double value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteDouble(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteDouble(binaryWriter, (double) value, writeTag);
        }

        /// <summary>
        ///     Write a float value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteFloat(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteFloat(binaryWriter, (float) value, writeTag);
        }

        /// <summary>
        ///     Write a guid value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteGuid(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteGuid(binaryWriter, (Guid) value, writeTag);
        }

        /// <summary>
        ///     Write a int value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteInt(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteInt(binaryWriter, (int) value, writeTag);
        }

        /// <summary>
        ///     Write a int value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        private static void WriteIntVariant(BinaryWriter binaryWriter, int value)
        {
            WriteUIntVariant(binaryWriter, Zig(value));
        }

        /// <summary>
        ///     Write a long value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteLong(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteLong(binaryWriter, (long) value, writeTag);
        }

        /// <summary>
        ///     Write a long value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        private static void WriteLongVariant(BinaryWriter binaryWriter, long value)
        {
            WriteULongVariant(binaryWriter, Zig(value));
        }

        /// <summary>
        ///     Write a signed byte value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteSByte(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteSByte(binaryWriter, (sbyte) value, writeTag);
        }

        /// <summary>
        ///     Write a short value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteShort(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteShort(binaryWriter, (short) value, writeTag);
        }

        /// <summary>
        ///     Write a short value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        private static void WriteShortVariant(BinaryWriter binaryWriter, short value)
        {
            WriteUShortVariant(binaryWriter, Zig(value));
        }

        /// <summary>
        ///     Write a string value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteString(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteString(binaryWriter, (string) value, writeTag);
        }

        /// <summary>
        ///     Write a string builder value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteStringBuilder(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            var stringBuilder = (StringBuilder) value;
            WriteString(binaryWriter, stringBuilder.ToString(), writeTag);
        }

        /// <summary>
        ///     Write a time span value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteTimeSpan(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteTimeSpan(binaryWriter, (TimeSpan) value, writeTag);
        }

        /// <summary>
        ///     Write a type value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteType(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteType(binaryWriter, (Type) value, writeTag);
        }

        /// <summary>
        ///     Write a type value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="configuration">
        ///     The encoder configuration.
        /// </param>
        private static void WriteType(BinaryWriter binaryWriter, Type type, EncoderConfiguration configuration)
        {
            WriteType(binaryWriter, type, configuration.Binder);
        }

        /// <summary>
        ///     Write a type value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="binder">
        ///     The serialization binder.
        /// </param>
        private static void WriteType(BinaryWriter binaryWriter, Type type, SerializationBinder binder)
        {
            var tuple = TypeNameCache.GetOrAdd(
                type,
                _ =>
                {
                    string assemblyName;
                    string typeName;
                    binder.BindToName(type, out assemblyName, out typeName);
                    return new Tuple<string, string>(assemblyName, typeName);
                });

            binaryWriter.Write(tuple.Item1);
            binaryWriter.Write(tuple.Item2);
        }

        /// <summary>
        ///     Write a unsigned int value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteUInt(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteUInt(binaryWriter, (uint) value, writeTag);
        }

        /// <summary>
        ///     Write a unsigned int value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        private static void WriteUIntVariant(BinaryWriter binaryWriter, uint value)
        {
            if ((value & ~0x7fU) != 0)
            {
                binaryWriter.Write((byte) ((value & 0x7f) | 0x80));
                value >>= 7;
                if ((value & ~0x7fU) != 0)
                {
                    binaryWriter.Write((byte) ((value & 0x7f) | 0x80));
                    value >>= 7;
                    if ((value & ~0x7fU) != 0)
                    {
                        binaryWriter.Write((byte) ((value & 0x7f) | 0x80));
                        value >>= 7;
                        if ((value & ~0x7fU) != 0)
                        {
                            binaryWriter.Write((byte) ((value & 0x7f) | 0x80));
                            value >>= 7;
                        }
                    }
                }
            }

            binaryWriter.Write((byte) value);
        }

        /// <summary>
        ///     Write a unsigned long value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteULong(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteULong(binaryWriter, (ulong) value, writeTag);
        }

        /// <summary>
        ///     Write a unsigned long value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        private static void WriteULongVariant(BinaryWriter binaryWriter, ulong value)
        {
            while ((value & ~0x7fUL) != 0)
            {
                binaryWriter.Write((byte) ((value & 0x7f) | 0x80));
                value >>= 7;
            }

            binaryWriter.Write((byte) value);
        }

        /// <summary>
        ///     Write a uri value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteUri(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteUri(binaryWriter, (Uri) value, writeTag);
        }

        /// <summary>
        ///     Write a unsigned short value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <param name="writeTag">
        ///     If <c>true</c> the encoder writes the leading type tag, otherwise <c>false</c>.
        /// </param>
        private static void WriteUShort(BinaryWriter binaryWriter, object value, bool writeTag)
        {
            WriteUShort(binaryWriter, (ushort) value, writeTag);
        }

        /// <summary>
        ///     Write a unsigned short value to the binary writer.
        /// </summary>
        /// <param name="binaryWriter">
        ///     The binary writer.
        /// </param>
        /// <param name="value">
        ///     The value.
        /// </param>
        private static void WriteUShortVariant(BinaryWriter binaryWriter, ushort value)
        {
            if ((value & ~0x7fU) != 0)
            {
                binaryWriter.Write((byte) ((value & 0x7f) | 0x80));
                value >>= 7;
                if ((value & ~0x7fU) != 0)
                {
                    binaryWriter.Write((byte) ((value & 0x7f) | 0x80));
                    value >>= 7;
                }
            }

            binaryWriter.Write((byte) value);
        }

        /// <summary>
        ///     The zig.
        /// </summary>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <returns>
        ///     The zigged value.
        /// </returns>
        private static ushort Zig(short value)
        {
            return (ushort) ((value << 1) ^ (value >> 15));
        }

        /// <summary>
        ///     The zig.
        /// </summary>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <returns>
        ///     The zigged value.
        /// </returns>
        private static uint Zig(int value)
        {
            return (uint) ((value << 1) ^ (value >> 31));
        }

        /// <summary>
        ///     The zig.
        /// </summary>
        /// <param name="value">
        ///     The value.
        /// </param>
        /// <returns>
        ///     The zigged value.
        /// </returns>
        private static ulong Zig(long value)
        {
            return (ulong) ((value << 1) ^ (value >> 63));
        }

        #endregion
    }
}