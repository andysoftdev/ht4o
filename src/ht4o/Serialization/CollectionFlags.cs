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

    /// <summary>
    /// The collection flags.
    /// </summary>
    [Flags]
    public enum CollectionFlags
    {
        /// <summary>
        /// Nothing have been set.
        /// </summary>
        None = 0, 

        /// <summary>
        /// Set if a typed collection.
        /// </summary>
        Typed = 1, 

        /// <summary>
        /// Set if a tag for the declared element type have been written.
        /// </summary>
        TypeTagged = 1 << 1,

        /// <summary>
        /// Set for a set collection.
        /// </summary>
        Set = 1 << 2,

        /// <summary>
        /// Set if the element values have been tagged.
        /// </summary>
        ValueTagged = 1 << 3
    }
}