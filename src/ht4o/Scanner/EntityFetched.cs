﻿/** -*- C# -*-
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

namespace Hypertable.Persistence.Scanner
{
    using System;
    using Hypertable;

    /// <summary>
    ///     The entity fetched delegate.
    /// </summary>
    /// <param name="cell">
    ///     The fetched cell, if null then key, buffer, length must be provided.
    /// </param>
    /// <param name="key">
    ///     The cell key.
    /// </param>
    /// <param name="buffer">
    ///     The cell buffer.
    /// </param>
    /// <param name="length">
    ///     The cell buffer length.
    /// </param>
    /// <param name="entityScanTarget">
    ///     The entity scan target.
    /// </param>
    internal delegate void EntityFetched(ICell cell, Key key, IntPtr buffer, int length, EntityScanTarget entityScanTarget);
}