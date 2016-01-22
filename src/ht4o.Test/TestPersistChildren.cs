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
namespace Hypertable.Persistence.Test
{
    using System;

    using Hypertable;
    using Hypertable.Persistence.Test.TestPersistChildrenTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestPersistChildrenTypes
    {
        using System;

        using Hypertable.Persistence.Attributes;

        internal class EntityA
        {
            #region Constructors and Destructors

            public EntityA()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public Guid Id { get; private set; }

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityA))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityA).Name);
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        internal class EntityB
        {
            #region Constructors and Destructors

            public EntityB()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public Guid Id { get; private set; }

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityB))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityB).Name);
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager", ColumnFamily = "a")]
        internal class EntityX
        {
            #region Constructors and Destructors

            public EntityX()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public string Id { get; private set; }

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityX))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityX).Name);
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager", ColumnFamily = "b")]
        internal class EntityY
        {
            #region Constructors and Destructors

            public EntityY()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public string Id { get; private set; }

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityY))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityY).Name);
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager")]
        internal class EntityC
        {
            #region Constructors and Destructors

            public EntityC()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public EntityA A { get; set; }

            public EntityB B { get; set; }

            public string Id { get; private set; }

            public string Name { get; set; }

            public EntityX X { get; set; }

            public EntityY Y { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
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

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }
    }

    /// <summary>
    /// The test persist children.
    /// </summary>
    [TestClass]
    public class TestPersistChildren : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist children.
        /// </summary>
        [TestMethod]
        public void PersistChildren()
        {
            var ec1 = new EntityC();
            TestBase.TestSerialization(ec1);

            var ec2 = new EntityC { A = new EntityA(), B = new EntityB() };
            TestBase.TestSerialization(ec2);

            var ec3 = new EntityC { X = new EntityX(), Y = new EntityY() };
            TestBase.TestSerialization(ec3);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(ec1);
                Assert.IsFalse(string.IsNullOrEmpty(ec1.Id));

                em.Persist(ec2);
                Assert.IsFalse(string.IsNullOrEmpty(ec2.Id));
                Assert.IsFalse(ec2.A.Id.Equals(Guid.Empty));
                Assert.IsFalse(ec2.B.Id.Equals(Guid.Empty));

                em.Persist(ec3);
                Assert.IsFalse(string.IsNullOrEmpty(ec3.Id));
                Assert.IsFalse(string.IsNullOrEmpty(ec3.X.Id));
                Assert.IsFalse(string.IsNullOrEmpty(ec3.Y.Id));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _ec1 = em.Find<EntityC>(ec1.Id);
                Assert.AreEqual(ec1, _ec1);

                var _ea2 = em.Find<EntityA>(ec2.A.Id);
                Assert.AreEqual(ec2.A, _ea2);
                var _eb2 = em.Find<EntityB>(ec2.B.Id);
                Assert.AreEqual(ec2.B, _eb2);

                var _ec2 = em.Find<EntityC>(ec2.Id);
                Assert.AreEqual(ec2, _ec2);

                var _ex3 = em.Find<EntityX>(ec3.X.Id);
                Assert.AreEqual(ec3.X, _ex3);
                var _ey3 = em.Find<EntityY>(ec3.Y.Id);
                Assert.AreEqual(ec3.Y, _ey3);

                var _ec3 = em.Find<EntityC>(ec3.Id);
                Assert.AreEqual(ec3, _ec3);
            }
        }

        #endregion
    }
}