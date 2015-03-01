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
namespace Hypertable.Persistence.Test.Common
{
    using System;
    using System.Runtime.Serialization;

    using Hypertable;
    using Hypertable.Persistence.Attributes;

    /// <summary>
    /// The entity a.
    /// </summary>
    internal class EntityA
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityA"/> class.
        /// </summary>
        public EntityA()
        {
            this.Name = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [DataMember(Name = "PropName")]
        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <returns>
        /// </returns>
        public override bool Equals(object o)
        {
            if (object.ReferenceEquals(this, o))
            {
                return true;
            }

            if (!(o is EntityA))
            {
                return false;
            }

            return string.Equals(this.Name, (o as EntityA).Name);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// The entity b.
    /// </summary>
    internal class EntityB
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityB"/> class.
        /// </summary>
        public EntityB()
        {
            this.Name = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <returns>
        /// </returns>
        public override bool Equals(object o)
        {
            if (object.ReferenceEquals(this, o))
            {
                return true;
            }

            if (!(o is EntityB))
            {
                return false;
            }

            return string.Equals(this.Name, (o as EntityB).Name);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// The entity base.
    /// </summary>
    [Entity("TestEntityManager")]
    internal abstract class EntityBase
    {
        #region Public Properties

        /// <summary>
        /// Gets the id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the last modified.
        /// </summary>
        public DateTime LastModified { get; private set; }

        #endregion
    }

    /// <summary>
    /// The entity x base.
    /// </summary>
    [Entity(ColumnFamily = "a")]
    internal abstract class EntityXBase : EntityBase
    {
    }

    /// <summary>
    /// The entity x.
    /// </summary>
    [Entity(ColumnQualifier = "1")]
    internal class EntityX : EntityXBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityX"/> class.
        /// </summary>
        public EntityX()
        {
            this.Name = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <returns>
        /// </returns>
        public override bool Equals(object o)
        {
            if (object.ReferenceEquals(this, o))
            {
                return true;
            }

            if (!(o is EntityX))
            {
                return false;
            }

            return string.Equals(this.Name, (o as EntityX).Name);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// The entity x 2.
    /// </summary>
    [Entity(ColumnQualifier = "2")]
    internal class EntityX2 : EntityXBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityX2"/> class.
        /// </summary>
        public EntityX2()
        {
            this.Name = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <returns>
        /// </returns>
        public override bool Equals(object o)
        {
            if (object.ReferenceEquals(this, o))
            {
                return true;
            }

            if (!(o is EntityX2))
            {
                return false;
            }

            return string.Equals(this.Name, (o as EntityX2).Name);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// The entity y.
    /// </summary>
    [Entity(ColumnFamily = "b")]
    internal class EntityY : EntityBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityY"/> class.
        /// </summary>
        public EntityY()
        {
            this.Name = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <returns>
        /// </returns>
        public override bool Equals(object o)
        {
            if (object.ReferenceEquals(this, o))
            {
                return true;
            }

            if (!(o is EntityY))
            {
                return false;
            }

            return string.Equals(this.Name, (o as EntityY).Name);
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// The entity c.
    /// </summary>
    [Entity(ColumnFamily = "c")]
    internal class EntityC : EntityBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityC"/> class.
        /// </summary>
        public EntityC()
        {
            this.Name = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the a.
        /// </summary>
        public EntityA A { get; set; }

        /// <summary>
        /// Gets or sets the b.
        /// </summary>
        public EntityB B { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the x.
        /// </summary>
        public EntityX X { get; set; }

        /// <summary>
        /// Gets or sets the y.
        /// </summary>
        public EntityY Y { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <returns>
        /// </returns>
        public override bool Equals(object o)
        {
            if (object.ReferenceEquals(this, o))
            {
                return true;
            }

            if (!(o is EntityC))
            {
                return false;
            }

            var c = o as EntityC;
            return string.Equals(this.Name, c.Name) && (this.A == null ? c.A == null : this.A.Equals(c.A)) && (this.B == null ? c.B == null : this.B.Equals(c.B))
                   && (this.X == null ? c.X == null : this.X.Equals(c.X)) && (this.Y == null ? c.Y == null : this.Y.Equals(c.Y));
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }
}