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

    using Hypertable.Persistence.Serialization.Delegates;

    /// <summary>
    /// The decoder info.
    /// </summary>
    internal sealed class DecoderInfo
    {
        #region Fields

        /// <summary>
        /// The deserialize deleagte.
        /// </summary>
        internal readonly Deserialize Deserialize;

        /// <summary>
        /// The type to deserialize.
        /// </summary>
        internal readonly Type Type;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoderInfo"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="deserialize">
        /// The deserialize.
        /// </param>
        internal DecoderInfo(Type type, Deserialize deserialize)
        {
            this.Type = type;
            this.Deserialize = deserialize;
        }

        #endregion
    }
}