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
namespace Hypertable.Persistence.Serialization.Delegates
{
    using System;

    /// <summary>
    /// The serializing entity delegate.
    /// </summary>
    /// <param name="isRoot">
    /// Indicating whether this object is the root of the object graph.
    /// </param>
    /// <param name="entityReference">
    /// The entity reference.
    /// </param>
    /// <param name="serializeType">
    /// The serialize type.
    /// </param>
    /// <param name="value">
    /// The object.
    /// </param>
    /// <returns>
    /// The entity key.
    /// </returns>
    internal delegate Key SerializingEntity(bool isRoot, EntityReference entityReference, Type serializeType, object value);
}