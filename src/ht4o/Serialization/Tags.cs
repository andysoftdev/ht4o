/** -*- C# -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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

    //// TODO any other type required? XmlDocument, XmlNode, Tuple, etc.
    //// TODO organize better?

    /// <summary>
    /// The tags.
    /// </summary>
    [Flags]
    public enum Tags
    {
        /// <summary>
        /// The null.
        /// </summary>
        Null = 0, 

        /// <summary>
        /// The array.
        /// </summary>
        Array = 1, 

        /// <summary>
        /// The first decoder tag.
        /// </summary>
        FirstDecoderTag = 1 << 1, 

        /// <summary>
        /// The true.
        /// </summary>
        True = 1 << 1, 

        /// <summary>
        /// The false.
        /// </summary>
        False = 2 << 1, 

        /// <summary>
        /// The bool.
        /// </summary>
        Bool = 3 << 1, 

        /// <summary>
        /// The s byte.
        /// </summary>
        SByte = 4 << 1, 

        /// <summary>
        /// The s byte 0.
        /// </summary>
        SByte0 = 5 << 1, 

        /// <summary>
        /// The byte.
        /// </summary>
        Byte = 6 << 1, 

        /// <summary>
        /// The byte 0.
        /// </summary>
        Byte0 = 7 << 1, 

        /// <summary>
        /// The short.
        /// </summary>
        Short = 8 << 1, 

        /// <summary>
        /// The short 0.
        /// </summary>
        Short0 = 9 << 1, 

        /// <summary>
        /// The u short.
        /// </summary>
        UShort = 10 << 1, 

        /// <summary>
        /// The u short 0.
        /// </summary>
        UShort0 = 11 << 1, 

        /// <summary>
        /// The int.
        /// </summary>
        Int = 12 << 1, 

        /// <summary>
        /// The int 0.
        /// </summary>
        Int0 = 13 << 1, 

        /// <summary>
        /// The u int.
        /// </summary>
        UInt = 14 << 1, 

        /// <summary>
        /// The u int 0.
        /// </summary>
        UInt0 = 15 << 1, 

        /// <summary>
        /// The long.
        /// </summary>
        Long = 16 << 1, 

        /// <summary>
        /// The long 0.
        /// </summary>
        Long0 = 17 << 1, 

        /// <summary>
        /// The u long.
        /// </summary>
        ULong = 18 << 1, 

        /// <summary>
        /// The u long 0.
        /// </summary>
        ULong0 = 19 << 1, 

        /// <summary>
        /// The char.
        /// </summary>
        Char = 20 << 1, 

        /// <summary>
        /// The float.
        /// </summary>
        Float = 21 << 1, 

        /// <summary>
        /// The float 0.
        /// </summary>
        Float0 = 22 << 1, 

        /// <summary>
        /// The float na n.
        /// </summary>
        FloatNaN = 23 << 1, 

        /// <summary>
        /// The double.
        /// </summary>
        Double = 24 << 1, 

        /// <summary>
        /// The double 0.
        /// </summary>
        Double0 = 25 << 1, 

        /// <summary>
        /// The double na n.
        /// </summary>
        DoubleNaN = 26 << 1, 

        /// <summary>
        /// The decimal.
        /// </summary>
        Decimal = 27 << 1, 

        /// <summary>
        /// The date time 0.
        /// </summary>
        DateTime0 = 28 << 1, 

        /// <summary>
        /// The date time.
        /// </summary>
        DateTime = 29 << 1, 

        /// <summary>
        /// The date time offset 0.
        /// </summary>
        DateTimeOffset0 = 30 << 1, 

        /// <summary>
        /// The date time offset.
        /// </summary>
        DateTimeOffset = 31 << 1, 

        /// <summary>
        /// The time span.
        /// </summary>
        TimeSpan = 32 << 1, 

        /// <summary>
        /// The time span 0.
        /// </summary>
        TimeSpan0 = 33 << 1, 

        /// <summary>
        /// The string.
        /// </summary>
        String = 34 << 1, 

        /// <summary>
        /// The string empty.
        /// </summary>
        StringEmpty = 35 << 1, 

        /// <summary>
        /// The guid.
        /// </summary>
        Guid = 36 << 1, 

        /// <summary>
        /// The type.
        /// </summary>
        Type = 37 << 1, 

        /// <summary>
        /// The type code.
        /// </summary>
        TypeCode = 38 << 1, 

        /// <summary>
        /// The uri.
        /// </summary>
        Uri = 39 << 1, 

        /// <summary>
        /// The last decoder tag.
        /// </summary>
        LastDecoderTag = Uri, 

        /// <summary>
        /// The tuple.
        /// </summary>
        Tuple = 48 << 1, 

        /// <summary>
        /// The key value pair.
        /// </summary>
        KeyValuePair = 49 << 1, 

        /// <summary>
        /// The collection.
        /// </summary>
        Collection = 50 << 1, 

        /// <summary>
        /// The enumerable.
        /// </summary>
        Enumerable = 51 << 1, 

        /// <summary>
        /// The dictionary.
        /// </summary>
        Dictionary = 52 << 1,

        /// <summary>
        /// The serialization info.
        /// </summary>
        SerializationInfo = 53 << 1,

        /// <summary>
        /// The object.
        /// </summary>
        Object = 54 << 1, 

        /// <summary>
        /// The object ref.
        /// </summary>
        ObjectRef = 55 << 1,

        /// <summary>
        /// The type schema.
        /// </summary>
        TypeSchema = 56 << 1,

        /// <summary>
        /// The type schema ref.
        /// </summary>
        TypeSchemaRef = 57 << 1, 

        /// <summary>
        /// The type ref.
        /// </summary>
        TypeRef = 58 << 1, 

        /// <summary>
        /// The string ref.
        /// </summary>
        StringRef = 59 << 1, 

        /// <summary>
        /// The entity ref.
        /// </summary>
        EntityRef = 60 << 1, 

        /// <summary>
        /// The entity key.
        /// </summary>
        EntityKey = 61 << 1, 

        /// <summary>
        /// The entity row.
        /// </summary>
        EntityRow = 62 << 1, 

        /// <summary>
        /// End of list.
        /// </summary>
        End = 63 << 1, 

        /// <summary>
        /// First custom type.
        /// </summary>
        FirstCustomType = 128
    }
}