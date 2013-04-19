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

    using Hypertable.Persistence.Reflection;
    using Hypertable.Persistence.Serialization.Delegates;

    //// TODO support for custom types via interface method

    /// <summary>
    /// The decoder.
    /// </summary>
    public static class Decoder
    {
        #region Constants

        /// <summary>
        /// The int16 msb.
        /// </summary>
        private const short Int16Msb = unchecked((short)(1 << 15));

        /// <summary>
        /// The int32 msb.
        /// </summary>
        private const int Int32Msb = 1 << 31;

        /// <summary>
        /// The int64 msb.
        /// </summary>
        private const long Int64Msb = 1L << 63;

        #endregion

        #region Static Fields

        /// <summary>
        /// The decoder infos.
        /// </summary>
        private static readonly ConcurrentDictionary<Tags, DecoderInfo> DecoderInfos = new ConcurrentDictionary<Tags, DecoderInfo>();

        /// <summary>
        /// The type codes.
        /// </summary>
        private static readonly ConcurrentDictionary<int, Type> TypeCodes = new ConcurrentDictionary<int, Type>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Reads a boolean value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The boolean value.
        /// </returns>
        public static bool ReadBool(BinaryReader binaryReader)
        {
            var value = ReadTag(binaryReader);
            if (value == Tags.True)
            {
                return true;
            }

            if (value == Tags.False)
            {
                return false;
            }

            throw new SerializationException(@"Invalid bool value");
        }

        /// <summary>
        /// Reads a byte value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The byte value.
        /// </returns>
        public static byte ReadByte(BinaryReader binaryReader)
        {
            return binaryReader.ReadByte();
        }

        /// <summary>
        /// Reads a char value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The char value.
        /// </returns>
        public static char ReadChar(BinaryReader binaryReader)
        {
            return binaryReader.ReadChar();
        }

        /// <summary>
        /// Reads a collection count value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The collection count value.
        /// </returns>
        public static int ReadCount(BinaryReader binaryReader)
        {
            return unchecked((int)ReadUIntVariant(binaryReader));
        }

        /// <summary>
        /// Reads a date time value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The date time value.
        /// </returns>
        public static DateTime ReadDateTime(BinaryReader binaryReader)
        {
            var kind = (DateTimeKind)ReadByte(binaryReader);
            var ticks = ReadLongVariant(binaryReader);
            if (ticks == long.MinValue)
            {
                return DateTime.MinValue;
            }

            if (ticks == long.MaxValue)
            {
                return DateTime.MaxValue;
            }

            return kind == DateTimeKind.Local ? new DateTime(ticks, DateTimeKind.Utc).ToLocalTime() : new DateTime(ticks, kind);
        }

        /// <summary>
        /// Reads a date time offset value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The date time offset value.
        /// </returns>
        public static DateTimeOffset ReadDateTimeOffset(BinaryReader binaryReader)
        {
            return new DateTimeOffset(ReadLongVariant(binaryReader), new TimeSpan(ReadLongVariant(binaryReader)));
        }

        /// <summary>
        /// Reads a decimal value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The decimal value.
        /// </returns>
        public static decimal ReadDecimal(BinaryReader binaryReader)
        {
            return binaryReader.ReadDecimal();
        }

        /// <summary>
        /// Reads a double value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The double value.
        /// </returns>
        public static double ReadDouble(BinaryReader binaryReader)
        {
            return binaryReader.ReadDouble();
        }

        /// <summary>
        /// Reads a float value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The float value.
        /// </returns>
        public static float ReadFloat(BinaryReader binaryReader)
        {
            return binaryReader.ReadSingle();
        }

        /// <summary>
        /// Reads a guid value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The guid value.
        /// </returns>
        public static Guid ReadGuid(BinaryReader binaryReader)
        {
            return new Guid(binaryReader.ReadBytes(16));
        }

        /// <summary>
        /// Reads a int value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The int value.
        /// </returns>
        public static int ReadInt(BinaryReader binaryReader)
        {
            return ReadIntVariant(binaryReader);
        }

        /// <summary>
        /// Reads a long value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The long value.
        /// </returns>
        public static long ReadLong(BinaryReader binaryReader)
        {
            return ReadLongVariant(binaryReader);
        }

        /// <summary>
        /// Reads a signed byte value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The signed byte value.
        /// </returns>
        [CLSCompliant(false)]
        public static sbyte ReadSByte(BinaryReader binaryReader)
        {
            return binaryReader.ReadSByte();
        }

        /// <summary>
        /// Reads a short value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The short value.
        /// </returns>
        public static short ReadShort(BinaryReader binaryReader)
        {
            return ReadShortVariant(binaryReader);
        }

        /// <summary>
        /// Reads a string value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The string value.
        /// </returns>
        public static string ReadString(BinaryReader binaryReader)
        {
            return binaryReader.ReadString();
        }

        /// <summary>
        /// Reads a string value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <param name="readTag">
        /// The read Tag.
        /// </param>
        /// <returns>
        /// The string value.
        /// </returns>
        public static string ReadString(BinaryReader binaryReader, bool readTag)
        {
            if (readTag)
            {
                switch (ReadTag(binaryReader))
                {
                    case Tags.Null:
                        return null;
                    case Tags.StringEmpty:
                        return string.Empty;
                    case Tags.String:
                        return ReadString(binaryReader);
                }

                throw new SerializationException(@"Deserialize string failed");
            }

            return ReadString(binaryReader);
        }

        /// <summary>
        /// Reads a tag value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The tag value.
        /// </returns>
        public static Tags ReadTag(BinaryReader binaryReader)
        {
            return unchecked((Tags)ReadUIntVariant(binaryReader));
        }

        /// <summary>
        /// Reads a time span value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The time span value.
        /// </returns>
        public static TimeSpan ReadTimeSpan(BinaryReader binaryReader)
        {
            var ticks = ReadLongVariant(binaryReader);
            if (ticks == long.MinValue)
            {
                return TimeSpan.MinValue;
            }

            if (ticks == long.MaxValue)
            {
                return TimeSpan.MaxValue;
            }

            return new TimeSpan(ticks);
        }

        /// <summary>
        /// Reads a type value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The type value.
        /// </returns>
        public static Type ReadType(BinaryReader binaryReader)
        {
            return TypeLoader.GetType(ReadString(binaryReader)); //// TODO better approach ????
        }

        /// <summary>
        /// Reads a unsigned int value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The unsigned int value.
        /// </returns>
        [CLSCompliant(false)]
        public static uint ReadUInt(BinaryReader binaryReader)
        {
            return ReadUIntVariant(binaryReader);
        }

        /// <summary>
        /// Reads a unsigned long value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The unsigned long value.
        /// </returns>
        [CLSCompliant(false)]
        public static ulong ReadULong(BinaryReader binaryReader)
        {
            return ReadULongVariant(binaryReader);
        }

        /// <summary>
        /// Reads a unsigned short value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The unsigned short value.
        /// </returns>
        [CLSCompliant(false)]
        public static ushort ReadUShort(BinaryReader binaryReader)
        {
            return ReadUShortVariant(binaryReader);
        }

        /// <summary>
        /// Reads a uri value from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The uri value.
        /// </returns>
        public static Uri ReadUri(BinaryReader binaryReader)
        {
            return new Uri(ReadString(binaryReader));
        }

        /// <summary>
        /// Register a custom deserializer by the type and type code specified.
        /// </summary>
        /// <param name="typeCode">
        /// The type code.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="deserialize">
        /// The deserialize delegate.
        /// </param>
        /// <returns>
        /// <c>true</c> if the deserialize delegate has been registered successfully, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="type"/> is null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="deserialize"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="typeCode"/> is not in the valid range.
        /// </exception>
        public static bool Register(int typeCode, Type type, Deserialize deserialize)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (deserialize == null)
            {
                throw new ArgumentNullException("deserialize");
            }

            Encoder.Register(typeCode, type);
            var internalTypeCode = Encoder.ToInternalTypeCode(typeCode);
            return DecoderInfos.TryAdd((Tags)internalTypeCode, new DecoderInfo(type, deserialize));
        }

        /// <summary>
        /// Register a custom serializer/deserializer by the type and type code specified.
        /// </summary>
        /// <param name="typeCode">
        /// The type code.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="serialize">
        /// The serialize delegate.
        /// </param>
        /// <param name="deserialize">
        /// The deserialize delegate.
        /// </param>
        /// <returns>
        /// <c>true</c> if the serializer and deserialize delegate has been registered successfully, otherwise <c>false</c>.
        /// </returns>
        public static bool Register(int typeCode, Type type, Serialize serialize, Deserialize deserialize)
        {
            return Register(typeCode, type, deserialize) && Encoder.Register(typeCode, type, serialize);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads a type from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The type.
        /// </returns>
        internal static Type ReadTypeCode(BinaryReader binaryReader)
        {
            Type type;
            var typecode = ReadInt(binaryReader);
            if (TypeCodes.TryGetValue(typecode, out type))
            {
                return type;
            }

            throw new SerializationException(string.Format(CultureInfo.InvariantCulture, @"Invalid type code {0}", typecode));
        }

        /// <summary>
        /// Register a type by the type code specified.
        /// </summary>
        /// <param name="internalTypeCode">
        /// The internal type code.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type code has been registered successfully, otherwise <c>false</c>.
        /// </returns>
        internal static bool RegisterInternalTypeCode(int internalTypeCode, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return TypeCodes.TryAdd(internalTypeCode, type);
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
        internal static bool TryGetDecoder(Tags tag, out DecoderInfo decoderInfo)
        {
            return DecoderInfos.TryGetValue(tag, out decoderInfo);
        }

        /// <summary>
        /// Attempts to read a value from the binary reader associated with the tag.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="any">
        /// The object read.
        /// </param>
        /// <returns>
        /// <c>true</c> if the object has been read from the binary reader successfully, otherwise <c>false</c>.
        /// </returns>
        internal static bool TryReadValue(BinaryReader binaryReader, Tags tag, out object any)
        {
            any = null;
            switch (tag)
            {
                case Tags.Null:
                    break;
                case Tags.True:
                    any = true;
                    break;
                case Tags.False:
                    any = false;
                    break;
                case Tags.Bool:
                    any = ReadBool(binaryReader);
                    break;
                case Tags.SByte:
                    any = ReadSByte(binaryReader);
                    break;
                case Tags.SByte0:
                    any = (sbyte)0;
                    break;
                case Tags.Byte:
                    any = ReadByte(binaryReader);
                    break;
                case Tags.Byte0:
                    any = (byte)0;
                    break;
                case Tags.Short:
                    any = ReadShort(binaryReader);
                    break;
                case Tags.Short0:
                    any = (short)0;
                    break;
                case Tags.UShort:
                    any = ReadUShort(binaryReader);
                    break;
                case Tags.UShort0:
                    any = (ushort)0;
                    break;
                case Tags.Int:
                    any = ReadInt(binaryReader);
                    break;
                case Tags.Int0:
                    any = 0;
                    break;
                case Tags.UInt:
                    any = ReadUInt(binaryReader);
                    break;
                case Tags.UInt0:
                    any = (uint)0;
                    break;
                case Tags.Long:
                    any = ReadLong(binaryReader);
                    break;
                case Tags.Long0:
                    any = (long)0;
                    break;
                case Tags.ULong:
                    any = ReadULong(binaryReader);
                    break;
                case Tags.ULong0:
                    any = (ulong)0;
                    break;
                case Tags.Char:
                    any = ReadChar(binaryReader);
                    break;
                case Tags.Float:
                    any = ReadFloat(binaryReader);
                    break;
                case Tags.Float0:
                    any = 0.0f;
                    break;
                case Tags.FloatNaN:
                    any = float.NaN;
                    break;
                case Tags.Double:
                    any = ReadDouble(binaryReader);
                    break;
                case Tags.Double0:
                    any = 0.0;
                    break;
                case Tags.DoubleNaN:
                    any = double.NaN;
                    break;
                case Tags.Decimal:
                    any = ReadDecimal(binaryReader);
                    break;
                case Tags.DateTime0:
                    any = new DateTime(0, DateTimeKind.Unspecified);
                    break;
                case Tags.DateTime:
                    any = ReadDateTime(binaryReader);
                    break;
                case Tags.DateTimeOffset0:
                    any = new DateTimeOffset(0, TimeSpan.Zero);
                    break;
                case Tags.DateTimeOffset:
                    any = ReadDateTimeOffset(binaryReader);
                    break;
                case Tags.TimeSpan:
                    any = ReadTimeSpan(binaryReader);
                    break;
                case Tags.TimeSpan0:
                    any = TimeSpan.Zero;
                    break;
                case Tags.String:
                    any = ReadString(binaryReader);
                    break;
                case Tags.StringEmpty:
                    any = string.Empty;
                    break;
                case Tags.Guid:
                    any = ReadGuid(binaryReader);
                    break;
                case Tags.Type:
                    any = ReadType(binaryReader);
                    break;
                case Tags.TypeCode:
                    any = ReadTypeCode(binaryReader);
                    break;
                case Tags.Uri:
                    any = ReadUri(binaryReader);
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reads a int from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The int value.
        /// </returns>
        private static int ReadIntVariant(BinaryReader binaryReader)
        {
            return Zag(ReadUIntVariant(binaryReader));
        }

        /// <summary>
        /// Reads a long from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The long value.
        /// </returns>
        private static long ReadLongVariant(BinaryReader binaryReader)
        {
            return Zag(ReadULongVariant(binaryReader));
        }

        /// <summary>
        /// Reads a short from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The short value.
        /// </returns>
        private static short ReadShortVariant(BinaryReader binaryReader)
        {
            return Zag(ReadUShortVariant(binaryReader));
        }

        /// <summary>
        /// Reads a unsigned int from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The unsigned int value.
        /// </returns>
        private static uint ReadUIntVariant(BinaryReader binaryReader)
        {
            var b = binaryReader.ReadByte();
            var value = b & 0x7FU;

            if ((b & 0x80) != 0)
            {
                b = binaryReader.ReadByte();
                value |= (b & 0x7FU) << 7;
                if ((b & 0x80) != 0)
                {
                    b = binaryReader.ReadByte();
                    value |= (b & 0x7FU) << 14;
                    if ((b & 0x80) != 0)
                    {
                        b = binaryReader.ReadByte();
                        value |= (b & 0x7FU) << 21;
                        if ((b & 0x80) != 0)
                        {
                            b = binaryReader.ReadByte();
                            value |= (b & 0x7FU) << 28;
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Reads a unsigned long from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The unsigned long value.
        /// </returns>
        private static ulong ReadULongVariant(BinaryReader binaryReader)
        {
            var b = binaryReader.ReadByte();
            var value = b & 0x7FUL;
            var shift = 7;
            while ((b & 0x80) != 0)
            {
                b = binaryReader.ReadByte();
                value |= (b & 0x7FUL) << shift;
                shift += 7;
            }

            return value;
        }

        /// <summary>
        /// Reads a unsigned short from the binary reader.
        /// </summary>
        /// <param name="binaryReader">
        /// The binary reader.
        /// </param>
        /// <returns>
        /// The unsigned short value.
        /// </returns>
        private static ushort ReadUShortVariant(BinaryReader binaryReader)
        {
            var b = binaryReader.ReadByte();
            var value = (ushort)(b & 0x7FU);

            if ((b & 0x80) != 0)
            {
                b = binaryReader.ReadByte();
                value |= (ushort)((b & 0x7FU) << 7);
                if ((b & 0x80) != 0)
                {
                    b = binaryReader.ReadByte();
                    value |= (ushort)((b & 0x7FU) << 14);
                }
            }

            return value;
        }

        /// <summary>
        /// The zag.
        /// </summary>
        /// <param name="ziggedValue">
        /// The zigged value.
        /// </param>
        /// <returns>
        /// The signed short value.
        /// </returns>
        private static short Zag(ushort ziggedValue)
        {
            var value = (short)ziggedValue;
            return (short)((-(value & 0x01)) ^ ((value >> 1) & ~Int16Msb));
        }

        /// <summary>
        /// The zag.
        /// </summary>
        /// <param name="ziggedValue">
        /// The zigged value.
        /// </param>
        /// <returns>
        /// The signed int value.
        /// </returns>
        private static int Zag(uint ziggedValue)
        {
            var value = (int)ziggedValue;
            return (-(value & 0x01)) ^ ((value >> 1) & ~Int32Msb);
        }

        /// <summary>
        /// The zag.
        /// </summary>
        /// <param name="ziggedValue">
        /// The zigged value.
        /// </param>
        /// <returns>
        /// The signed long value.
        /// </returns>
        private static long Zag(ulong ziggedValue)
        {
            var value = (long)ziggedValue;
            return (-(value & 0x01L)) ^ ((value >> 1) & ~Int64Msb);
        }

        #endregion
    }
}