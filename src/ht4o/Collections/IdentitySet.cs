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
namespace Hypertable.Persistence.Collections
{
    using System.Collections.Generic;

    /// <summary>
    /// The identity set.
    /// </summary>
    internal sealed class IdentitySet : HashSet<object>
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentitySet"/> class.
        /// </summary>
        internal IdentitySet()
            : base(new IdentityComparer())
        {
        }

        #endregion
    }
}