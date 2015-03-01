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
namespace Hypertable.Persistence
{
    using System;

    ////TODO review behaviors naming

    /// <summary>
    /// The entity manager behaviors.
    /// </summary>
    [Flags]
    public enum Behaviors
    {
        /// <summary>
        /// Use the entity manager default behaviors.
        /// </summary>
        Default = 0, 

        /// <summary>
        /// Creates always a new entity key for each entity in the object graph.
        /// </summary>
        CreateAlways = 1, 

        /// <summary>
        /// Creates new entity keys for new entities and those which are not part of the current EntityManager context.
        /// </summary>
        CreateLazy = 2, 

        /// <summary>
        /// Creates new entity keys only for new entities.
        /// </summary>
        CreateNew = 3, 

        /// <summary>
        /// Masks for all create behaviors.
        /// </summary>
        CreateBehaviors = CreateAlways | CreateLazy | CreateNew, 

        /// <summary>
        /// Don't cache entities written nor entities read.
        /// </summary>
        DoNotCache = 1 << 8, 

        /// <summary>
        /// Bypasses the entity keys written cache, cannot be combined with Behaviors.CreateLazy.
        /// </summary>
        BypassWriteCache = 1 << 9, 

        /// <summary>
        /// Bypasses the entity read cache. If set fetched entities won't be insert into the read cache.
        /// </summary>
        BypassReadCache = 1 << 10
    }
}