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
    /// <summary>
    /// The serialization base.
    /// </summary>
    public class SerializationBase
    {
        #region Static Fields

        /// <summary>
        /// The default capacity for the memory stream.
        /// </summary>
        private static int defaultCapacity = 1024;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationBase"/> class.
        /// </summary>
        protected SerializationBase()
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the memory stream default capacity.
        /// </summary>
        /// <value>
        /// The default capacity.
        /// </value>
        public static int DefaultCapacity
        {
            get
            {
                return defaultCapacity;
            }

            set
            {
                if (value > 0)
                {
                    defaultCapacity = value;
                }
            }
        }

        #endregion
    }
}