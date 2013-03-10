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
namespace Hypertable.Persistence.Test
{
    using Hypertable;
    using Hypertable.Persistence.Test.TestCyclicReferencesTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestCyclicReferencesTypes
    {
        using System;

        using Hypertable.Persistence.Attributes;

        [Entity("TestEntityManager", ColumnFamily = "a")]
        internal class EntityA
        {
            #region Constructors and Destructors

            public EntityA(EntityB owner)
            {
                this.OwnerB = owner;
                this.Name = Guid.NewGuid().ToString();
            }

            public EntityA(EntityC owner)
            {
                this.OwnerC = owner;
                this.Name = Guid.NewGuid().ToString();
            }

            public EntityA(EntityD owner)
            {
                this.OwnerD = owner;
                this.Name = Guid.NewGuid().ToString();
            }

            private EntityA()
            {
            }

            #endregion

            #region Public Properties

            public string Id { get; private set; }

            public string Name { get; set; }

            public EntityB OwnerB { get; private set; }

            public EntityC OwnerC { get; private set; }

            public EntityD OwnerD { get; private set; }

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

        [Entity("TestEntityManager", ColumnFamily = "b")]
        internal class EntityB
        {
            #region Constructors and Destructors

            public EntityB()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public EntityA A { get; set; }

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

                if (!(o is EntityB))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityB).Name) && (this.A == null ? (o as EntityB).A == null : this.A.Equals((o as EntityB).A));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode() ^ (this.A != null ? this.A.GetHashCode() : 0);
            }

            #endregion
        }

        internal class Intermediate
        {
            #region Constructors and Destructors

            public Intermediate()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public EntityA A { get; set; }

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is Intermediate))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as Intermediate).Name) && (this.A == null ? (o as Intermediate).A == null : this.A.Equals((o as Intermediate).A));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode() ^ (this.A != null ? this.A.GetHashCode() : 0);
            }

            #endregion
        }

        [Entity("TestEntityManager", ColumnFamily = "b")]
        internal class EntityC
        {
            #region Constructors and Destructors

            public EntityC()
            {
                this.Name = Guid.NewGuid().ToString();
                this.I = new Intermediate();
            }

            #endregion

            #region Public Properties

            public Intermediate I { get; set; }

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

                if (!(o is EntityC))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityC).Name) && (this.I == null ? (o as EntityC).I == null : this.I.Equals((o as EntityC).I));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode() ^ (this.I != null ? this.I.GetHashCode() : 0);
            }

            #endregion
        }

        [Entity("TestEntityManager", ColumnFamily = "b")]
        internal class EntityD
        {
            #region Constructors and Destructors

            public EntityD()
            {
                this.Name = Guid.NewGuid().ToString();
                this.I1 = this.I2 = new Intermediate();
            }

            #endregion

            #region Public Properties

            public Intermediate I1 { get; set; }

            public Intermediate I2 { get; set; }

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
                else if (!(o is EntityD))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityD).Name) && (this.I1 == null ? (o as EntityD).I1 == null : this.I1.Equals((o as EntityD).I1))
                       && (this.I2 == null ? (o as EntityD).I2 == null : this.I2.Equals((o as EntityD).I2));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode() ^ (this.I1 != null ? this.I1.GetHashCode() : 0) ^ (this.I2 != null ? this.I2.GetHashCode() : 0);
            }

            #endregion
        }
    }

    /// <summary>
    /// The test cyclic references.
    /// </summary>
    [TestClass]
    public class TestCyclicReferences : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            {
                var eb1 = new EntityB();
                TestBase.TestSerialization(eb1);

                var eb2 = new EntityB();
                eb2.A = new EntityA(eb2);
                TestBase.TestSerialization(eb2);

                using (var em = Emf.CreateEntityManager())
                {
                    em.Persist(eb1);
                    Assert.IsFalse(string.IsNullOrEmpty(eb1.Id));

                    em.Persist(eb2);
                    Assert.IsFalse(string.IsNullOrEmpty(eb2.Id));
                }

                using (var em = Emf.CreateEntityManager())
                {
                    var _eb1 = em.Find<EntityB>(eb1.Id);
                    Assert.AreEqual(eb1, _eb1);

                    var _ea2 = em.Find<EntityA>(eb2.A.Id);
                    Assert.AreEqual(eb2.A, _ea2);
                    Assert.AreEqual(eb2, _ea2.OwnerB);
                    Assert.AreSame(_ea2, _ea2.OwnerB.A);

                    var _eb2 = em.Find<EntityB>(eb2.Id);
                    Assert.AreEqual(eb2, _eb2);
                    Assert.AreSame(_eb2, _eb2.A.OwnerB);
                }
            }

            {
                var ec1 = new EntityC();
                TestBase.TestSerialization(ec1);

                var ec2 = new EntityC();
                ec2.I.A = new EntityA(ec2);
                TestBase.TestSerialization(ec2);

                var ec3 = new EntityC { I = { A = new EntityA(ec1) } };
                TestBase.TestSerialization(ec3);

                using (var em = Emf.CreateEntityManager())
                {
                    em.Persist(ec1);
                    Assert.IsFalse(string.IsNullOrEmpty(ec1.Id));

                    em.Persist(ec2);
                    Assert.IsFalse(string.IsNullOrEmpty(ec2.Id));

                    em.Persist(ec3);
                    Assert.IsFalse(string.IsNullOrEmpty(ec3.Id));
                }

                using (var em = Emf.CreateEntityManager())
                {
                    var _ec1 = em.Find<EntityC>(ec1.Id);
                    Assert.AreEqual(ec1, _ec1);

                    var _ea2 = em.Find<EntityA>(ec2.I.A.Id);
                    Assert.AreEqual(ec2.I.A, _ea2);
                    Assert.AreEqual(ec2, _ea2.OwnerC);
                    Assert.AreSame(_ea2, _ea2.OwnerC.I.A);

                    var _ec2 = em.Find<EntityC>(ec2.Id);
                    Assert.AreEqual(ec2, _ec2);
                    Assert.AreSame(_ec2, _ec2.I.A.OwnerC);

                    var _ea3 = em.Find<EntityA>(ec3.I.A.Id);
                    Assert.AreEqual(ec3.I.A, _ea3);
                    Assert.AreEqual(ec1, _ea3.OwnerC);
                    Assert.IsNull(_ea3.OwnerC.I.A);

                    var _ec3 = em.Find<EntityC>(ec3.Id);
                    Assert.AreEqual(ec3, _ec3);
                    Assert.AreEqual(_ec1, _ec3.I.A.OwnerC);
                }
            }

            {
                var ed1 = new EntityD();
                TestBase.TestSerialization(ed1);

                var ed2 = new EntityD();
                ed2.I1.A = new EntityA(ed2);
                TestBase.TestSerialization(ed2);

                var ed3 = new EntityD { I2 = { A = new EntityA(ed1) } };
                TestBase.TestSerialization(ed3);

                using (var em = Emf.CreateEntityManager())
                {
                    em.Persist(ed1);
                    Assert.IsFalse(string.IsNullOrEmpty(ed1.Id));

                    em.Persist(ed2);
                    Assert.IsFalse(string.IsNullOrEmpty(ed2.Id));

                    em.Persist(ed3);
                    Assert.IsFalse(string.IsNullOrEmpty(ed3.Id));
                }

                using (var em = Emf.CreateEntityManager())
                {
                    var _ed1 = em.Find<EntityD>(ed1.Id);
                    Assert.AreEqual(ed1, _ed1);
                    Assert.AreSame(_ed1.I1, _ed1.I2);

                    var _ea2 = em.Find<EntityA>(ed2.I1.A.Id);
                    Assert.AreEqual(ed2.I1.A, _ea2);
                    Assert.AreEqual(ed2, _ea2.OwnerD);
                    Assert.AreSame(_ea2.OwnerD.I2, _ea2.OwnerD.I2);
                    Assert.AreSame(_ea2, _ea2.OwnerD.I1.A);

                    var _ed2 = em.Find<EntityD>(ed2.Id);
                    Assert.AreEqual(ed2, _ed2);
                    Assert.AreSame(_ed2, _ed2.I1.A.OwnerD);
                    Assert.AreSame(_ed2.I1, _ed2.I2);

                    var _ea3 = em.Find<EntityA>(ed3.I1.A.Id);
                    Assert.AreEqual(ed3.I1.A, _ea3);
                    Assert.AreEqual(ed1, _ea3.OwnerD);
                    Assert.IsNull(_ea3.OwnerD.I1.A);
                    Assert.AreSame(_ea3.OwnerD.I1, _ea3.OwnerD.I2);

                    var _ed3 = em.Find<EntityD>(ed3.Id);
                    Assert.AreEqual(ed3, _ed3);
                    Assert.AreEqual(_ed1, _ed3.I1.A.OwnerD);
                    Assert.AreSame(_ed1.I1, _ed1.I2);
                }
            }
        }

        #endregion
    }
}