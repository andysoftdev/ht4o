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
    using System;

    using Hypertable;

    /// <summary>
    /// The entity specification.
    /// </summary>
    internal class EntitySpec : IEquatable<EntitySpec>
    {
        #region Static Fields

        /// <summary>
        /// The key comparer.
        /// </summary>
        private static readonly KeyComparer KeyComparer = new KeyComparer();

        #endregion

        #region Fields

        /// <summary>
        /// The entity type.
        /// </summary>
        /// <remarks>
        /// Not part of equatable and hash.
        /// </remarks>
        protected readonly Type entityType;

        /// <summary>
        /// The pre-calculated hash code.
        /// </summary>
        private readonly int hashCode;

        /// <summary>
        /// The entity key.
        /// </summary>
        private readonly Key key;

        /// <summary>
        /// The database namespace.
        /// </summary>
        private readonly string ns;

        /// <summary>
        /// The database table name.
        /// </summary>
        private readonly string tableName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySpec"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="ns">
        /// The database namespace.
        /// </param>
        /// <param name="tableName">
        /// The database table name.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        internal EntitySpec(Type entityType, string ns, string tableName, Key key)
        {
            this.entityType = entityType;
            this.ns = ns;
            this.tableName = tableName;
            this.key = key;

            unchecked
            {
                this.hashCode = 17;
                if (this.key != null)
                {
                    this.hashCode = (29 * this.hashCode) + KeyComparer.GetHashCode(this.key);
                }

                this.hashCode = (31 * this.hashCode) + this.tableName.GetHashCode();
                this.hashCode = (37 * this.hashCode) + (this.ns ?? string.Empty).GetHashCode();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySpec"/> class.
        /// </summary>
        /// <param name="entityReference">
        /// The entity reference.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        internal EntitySpec(EntityReference entityReference, Key key)
            : this(entityReference.EntityType, entityReference.Namespace, entityReference.TableName, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySpec"/> class.
        /// </summary>
        /// <param name="other">
        /// The other entity specification.
        /// </param>
        internal EntitySpec(EntitySpec other)
        {
            this.entityType = other.entityType;
            this.ns = other.ns;
            this.tableName = other.tableName;
            this.key = other.key;
            this.hashCode = other.hashCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySpec"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="other">
        /// The other entity specification.
        /// </param>
        protected EntitySpec(Type entityType, EntitySpec other)
            : this(entityType, other.ns, other.tableName, other.Key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySpec"/> class.
        /// </summary>
        /// <param name="entityType">
        /// The entity type.
        /// </param>
        /// <param name="other">
        /// The other entity specification.
        /// </param>
        /// <param name="key">
        /// The entity key.
        /// </param>
        protected EntitySpec(Type entityType, EntitySpec other, Key key)
            : this(entityType, other.ns, other.tableName, key)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        /// <value>
        /// The entity type.
        /// </value>
        internal Type EntityType
        {
            get
            {
                return this.entityType;
            }
        }

        /// <summary>
        /// Gets the entity key.
        /// </summary>
        /// <value>
        /// The entity  key.
        /// </value>
        internal Key Key
        {
            get
            {
                return this.key;
            }
        }

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        internal string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        /// <summary>
        /// Gets the database table name.
        /// </summary>
        /// <value>
        /// The database table name.
        /// </value>
        internal string TableName
        {
            get
            {
                return this.tableName;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter, otherwise <c>false</c>.
        /// </returns>
        public bool Equals(EntitySpec other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!KeyComparer.Equals(this.key, other.key))
            {
                return false;
            }

            return string.Compare(this.tableName, other.tableName, StringComparison.Ordinal) == 0
                   && string.Compare(this.ns ?? string.Empty, other.ns ?? string.Empty, StringComparison.Ordinal) == 0;
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
            return this.Equals(other as EntitySpec);
        }

        /// <summary>
        /// Serves as a hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="EntitySpec"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return this.hashCode;
        }

        #endregion
    }
}