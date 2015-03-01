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

    /// <summary>
    /// Declares a pair structure of the same type.
    /// </summary>
    /// <typeparam name="T">
    /// The pair type.
    /// </typeparam>
    internal struct Pair<T> : IEquatable<Pair<T>>
        where T : class, IEquatable<T>
    {
        #region Fields

        /// <summary>
        /// The first item.
        /// </summary>
        public readonly T First;

        /// <summary>
        /// The second item.
        /// </summary>
        public readonly T Second;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Pair{T}"/> struct.
        /// </summary>
        /// <param name="first">
        /// The first item.
        /// </param>
        /// <param name="second">
        /// The second item.
        /// </param>
        public Pair(T first, T second)
        {
            this.First = first;
            this.Second = second;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter, otherwise <c>false</c>.
        /// </returns>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        public bool Equals(Pair<T> other)
        {
            return (ReferenceEquals(this.First, other.First) || (this.First != null && this.First.Equals(other.First)))
                   && (ReferenceEquals(this.Second, other.Second) || (this.Second != null && this.Second.Equals(other.Second)));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter, otherwise <c>false</c>.
        /// </returns>
        public override bool Equals(object other)
        {
            if (other is Pair<T>)
            {
                return this.Equals((Pair<T>)other);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.First != null ? 29 * this.First.GetHashCode() : 17) + 29 * (this.Second != null ? this.Second.GetHashCode() : 17);
            }
        }

        #endregion
    }
}