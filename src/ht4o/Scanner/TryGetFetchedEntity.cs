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
namespace Hypertable.Persistence.Scanner
{
    using Hypertable;

    /// <summary>
    /// The try get fetched entity delegate.
    /// </summary>
    /// <param name="entitySpec">
    /// The entity spec.
    /// </param>
    /// <param name="entity">
    /// The entity.
    /// </param>
    /// <returns>
    /// <c>true</c> if the entity has been found; otherwise <c>false</c>.
    /// </returns>
    internal delegate bool TryGetFetchedEntity(EntitySpec entitySpec, out object entity);
}