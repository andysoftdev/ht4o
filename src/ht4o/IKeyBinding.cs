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
namespace Hypertable.Persistence
{
    using Hypertable;

    /// <summary>
    /// Defines a key binding.
    /// </summary>
    public interface IKeyBinding
    {
        #region Public Methods and Operators

        /// <summary>
        /// Creates a database key for the entity specified.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// The database key.
        /// </returns>
        Key CreateKey(object entity);

        /// <summary>
        /// Gets the database key from the entity specified.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// The database key.
        /// </returns>
        Key KeyFromEntity(object entity);

        /// <summary>
        /// Gets the database key from the value specified.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <returns>
        /// The database key.
        /// </returns>
        Key KeyFromValue(object value);

        /// <summary>
        /// Updates the entity using the database key specified.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="key">
        /// The database key.
        /// </param>
        void SetKey(object entity, Key key);

        #endregion
    }
}