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

namespace Hypertable.Persistence.Scanner
{
    /// <summary>
    ///     Declares fetched cell.
    /// </summary>
    internal sealed class FetchedCell
    {
        #region Fields

        /// <summary>
        ///     The key.
        /// </summary>
        internal Key Key;

        /// <summary>
        ///     The value.
        /// </summary>
        internal byte[] Value;

        /// <summary>
        ///     The value length.
        /// </summary>
        internal int ValueLength;

        /// <summary>
        ///     The entity scan target.
        /// </summary>
        internal EntityScanTarget EntityScanTarget;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FetchedCell" /> class.
        /// </summary>
        internal FetchedCell() {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FetchedCell" /> class.
        /// </summary>
        /// <param name="cell">
        ///     The fetched cell.
        /// </param>
        /// <param name="entityScanTarget">
        ///     The entity scan target.
        /// </param>
        internal FetchedCell(Cell cell, EntityScanTarget entityScanTarget)
        {
            this.Key = cell.Key;
            this.Value = cell.Value;
            this.ValueLength = cell.Value != null ? cell.Value.Length : 0;
            this.EntityScanTarget = entityScanTarget;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FetchedCell" /> class.
        /// </summary>
        /// <param name="cell">
        ///     The fetched cell.
        /// </param>
        /// <param name="entityScanTarget">
        ///     The entity scan target.
        /// </param>
        internal FetchedCell(BufferedCell cell, EntityScanTarget entityScanTarget) {
            this.Key = cell.Key;
            this.Value = cell.Value;
            this.ValueLength = cell.ValueLength;
            this.EntityScanTarget = entityScanTarget;
        }

        #endregion
    }
}