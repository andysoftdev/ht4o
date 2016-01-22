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
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// The concurrent set.
    /// </summary>
    /// <typeparam name="T">
    /// The element type.
    /// </typeparam>
    internal sealed class ConcurrentSet<T> : ISet<T>
        where T : IEquatable<T>
    {
        #region Fields

        /// <summary>
        /// The dictionary.
        /// </summary>
        private readonly ConcurrentDictionary<T, object> dictionary;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class.
        /// </summary>
        internal ConcurrentSet()
        {
            this.dictionary = new ConcurrentDictionary<T, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentSet{T}"/> class.
        /// </summary>
        /// <param name="comparer">
        /// The comparer.
        /// </param>
        internal ConcurrentSet(IEqualityComparer<T> comparer)
        {
            this.dictionary = new ConcurrentDictionary<T, object>(comparer);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the number of elements contained in the set.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the set.
        /// </returns>
        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the set is read-only.
        /// </summary>
        /// <returns>
        /// true if the set is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds an item to the set.
        /// </summary>
        /// <param name="item">
        /// The element to add.
        /// </param>
        /// <returns>
        /// <c>true</c> if the element was added to the set successfully. If the element already exists, this method returns <c>false</c>.
        /// </returns>
        public bool Add(T item)
        {
            return this.dictionary.TryAdd(item, null);
        }

        /// <summary>
        /// Removes all items from the set>.
        /// </summary>
        public void Clear()
        {
            this.dictionary.Clear();
        }

        /// <summary>
        /// Determines whether an element is in the set.
        /// </summary>
        /// <param name="item">
        /// The element.
        /// </param>
        /// <returns>
        /// <c>true</c> if item is found in the set, otherwise <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            return this.dictionary.ContainsKey(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return this.dictionary.Keys.GetEnumerator();
        }

        /// <summary>
        /// Removes the specified element from the set.
        /// </summary>
        /// <param name="item">
        /// The element to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> if the element is successfully found and removed, otherwise <c>false</c>.
        /// </returns>
        public bool Remove(T item)
        {
            object value;
            return this.dictionary.TryRemove(item, out value);
        }

        #endregion

        #region Explicit Interface Methods

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            this.dictionary.Keys.CopyTo(array, arrayIndex);
        }

        void ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<T>.IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<T>.IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<T>.IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<T>.IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<T>.Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        bool ISet<T>.SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        void ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}