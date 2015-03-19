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

    using Hypertable.Persistence.Extensions;
    using Hypertable.Persistence.Serialization.Delegates;

    /// <summary>
    /// The encoder info.
    /// </summary>
    internal sealed class EncoderInfo
    {
        #region Fields

        /// <summary>
        /// The tag.
        /// </summary>
        internal readonly Tags Tag;

        /// <summary>
        /// The encode delegate.
        /// </summary>
        private readonly Encode encode;

        /// <summary>
        /// The serialize delegate.
        /// </summary>
        private readonly Serialize serialize;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderInfo"/> class.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="encode">
        /// The encode delegate.
        /// </param>
        internal EncoderInfo(Tags tag, Encode encode)
        {
            this.Tag = tag;
            this.encode = encode;
            this.serialize = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncoderInfo"/> class.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        /// <param name="serialize">
        /// The serialize delegate.
        /// </param>
        internal EncoderInfo(Tags tag, Serialize serialize)
        {
            this.Tag = tag;
            this.encode = null;
            this.serialize = serialize;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Encodes an object specified.
        /// </summary>
        /// <param name="serializer">
        /// The serializer.
        /// </param>
        /// <param name="any">
        /// The object to encode.
        /// </param>
        /// <param name="writeTag">
        /// If <c>true</c> the encoder must writes the leading tag, otherwise <c>false</c>.
        /// </param>
        internal void Encode(Serializer serializer, object any, bool writeTag)
        {
            if (this.encode != null)
            {
                this.encode(serializer.BinaryWriter, any, writeTag);
            }
            else
            {
                if (writeTag)
                {
                    Encoder.WriteTag(serializer.BinaryWriter, this.Tag);
                }

                this.serialize(serializer, any);
            }
        }

        /// <summary>
        /// Check if the value type requires object ref handling. 
        /// </summary>
        /// <param name="type">
        /// The value type.
        /// </param>
        /// <returns>
        /// <c>true</c> if the type requires ObjectRef handling; otherwise <c>false</c>.
        /// </returns>
        internal bool HandleObjectRef(Type type)
        {
            ////TODO review, correct?
            return this.Tag >= Tags.FirstCustomType || (this.Tag >= Tags.TypeCode && type.IsComplex());
        }

        #endregion
    }
}