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
    using System.Collections.Generic;
    using System.Linq;

    using Hypertable;
    using Hypertable.Persistence.Test.TestEntityAttributeInheritTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestEntityAttributeInheritTypes
    {
        using System;

        using Hypertable.Persistence.Attributes;

        internal abstract class EntityBase
        {
            #region Constructors and Destructors

            protected EntityBase()
            {
                this.Key = new Key();
            }

            #endregion

            #region Public Properties

            public Key Key { get; private set; }

            #endregion
        }

        [Entity("TestEntityManager")]
        internal abstract class EntityBaseX : EntityBase
        {
        }

        [Entity(ColumnFamily = "a")]
        internal class EntityAX : EntityBaseX
        {
            #region Constructors and Destructors

            public EntityAX()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (object.ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityAX))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityAX).Name);
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity(ColumnQualifier = "a2x")]
        internal class EntityA2X : EntityAX
        {
        }

        internal interface IEntityBx
        {
        }

        [Entity(ColumnFamily = "b")]
        internal class EntityBX : EntityBaseX, IEntityBx
        {
            #region Constructors and Destructors

            public EntityBX()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public EntityAX A { get; set; }

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (object.ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityBX))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityBX).Name) && (this.A == null ? (o as EntityBX).A == null : this.A.Equals((o as EntityBX).A));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode() ^ (this.A != null ? this.A.GetHashCode() : 0);
            }

            #endregion
        }

        [Entity(ColumnFamily = "b", ColumnQualifier = "bx")]
        internal class EntityB2X : EntityBX
        {
        }
    }

    /// <summary>
    /// The test entity attribute inherit.
    /// </summary>
    [TestClass]
    public class TestEntityAttributeInherit : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The fetch.
        /// </summary>
        [TestMethod]
        public void Fetch()
        {
            var eb1 = new EntityBX();
            TestBase.TestSerialization(eb1);

            var eb2 = new EntityBX { A = new EntityAX() };
            TestBase.TestSerialization(eb2);

            var eb21 = new EntityB2X();
            TestBase.TestSerialization(eb21);

            var eb22 = new EntityB2X { A = new EntityA2X() };
            TestBase.TestSerialization(eb22);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                em.Persist(eb2);
                em.Persist(eb21);
                em.Persist(eb22);
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _ea = em.Fetch<EntityAX>();
                Assert.IsNotNull(_ea);
                ISet<EntityAX> sax = new HashSet<EntityAX>(_ea.ToList());
                Assert.AreEqual(2, sax.Count);
                Assert.IsTrue(sax.Contains(eb2.A));
                Assert.IsTrue(sax.Contains(eb22.A));
                {
                    var _eb = em.Fetch<EntityBX>();
                    Assert.IsNotNull(_eb);
                    ISet<EntityBX> sbx = new HashSet<EntityBX>(_eb.ToList());
                    Assert.AreEqual(4, sbx.Count);
                    Assert.IsTrue(sbx.Contains(eb1));
                    Assert.IsTrue(sbx.Contains(eb2));
                    Assert.IsTrue(sbx.Contains(eb21));
                    Assert.IsTrue(sbx.Contains(eb22));
                }

                {
                    var _eb = em.Fetch<EntityB2X>();
                    Assert.IsNotNull(_eb);
                    ISet<EntityB2X> sbx = new HashSet<EntityB2X>(_eb.ToList());
                    Assert.AreEqual(2, sbx.Count);
                    Assert.IsTrue(sbx.Contains(eb21));
                    Assert.IsTrue(sbx.Contains(eb22));
                }
            }
        }

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            var eb1 = new EntityBX();
            TestBase.TestSerialization(eb1);

            var eb2 = new EntityBX { A = new EntityAX() };
            TestBase.TestSerialization(eb2);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                Assert.IsNotNull(eb1.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Key.Row));
                Assert.AreEqual("b", eb1.Key.ColumnFamily);
                Assert.IsTrue(string.IsNullOrEmpty(eb1.Key.ColumnQualifier));

                em.Persist(eb2);
                Assert.IsNotNull(eb2.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb2.Key.Row));
                Assert.AreEqual("b", eb2.Key.ColumnFamily);
                Assert.IsTrue(string.IsNullOrEmpty(eb2.Key.ColumnQualifier));

                Assert.IsNotNull(eb2.A.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb2.A.Key.Row));
                Assert.AreEqual("a", eb2.A.Key.ColumnFamily);
                Assert.IsTrue(string.IsNullOrEmpty(eb2.A.Key.ColumnQualifier));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityBX>(eb1.Key);
                Assert.AreEqual(eb1, _eb1);

                var _ea2 = em.Find<EntityAX>(eb2.A.Key);
                Assert.AreEqual(eb2.A, _ea2);

                var _eb2 = em.Find<EntityBX>(eb2.Key);
                Assert.AreEqual(eb2, _eb2);

                Assert.AreEqual(2, em.Fetch<EntityBX>().Count());
                Assert.AreEqual(1, em.Fetch<EntityAX>().Count());
                Assert.AreEqual(3, em.Fetch<EntityBaseX>().Count());
                Assert.AreEqual(2, em.Fetch<IEntityBx>().Count());
                Assert.AreEqual(3, em.Fetch<EntityBaseX>(new[] { typeof(EntityBX), typeof(EntityAX) }).Count());
            }

            var eb21 = new EntityB2X();
            TestBase.TestSerialization(eb21);

            var eb22 = new EntityB2X { A = new EntityA2X() };
            TestBase.TestSerialization(eb22);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb21);
                Assert.IsNotNull(eb21.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb21.Key.Row));
                Assert.AreEqual("b", eb21.Key.ColumnFamily);
                Assert.AreEqual("bx", eb21.Key.ColumnQualifier);

                em.Persist(eb22);
                Assert.IsNotNull(eb22.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb22.Key.Row));
                Assert.AreEqual("b", eb22.Key.ColumnFamily);
                Assert.AreEqual("bx", eb22.Key.ColumnQualifier);

                Assert.IsNotNull(eb22.A.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb22.A.Key.Row));
                Assert.AreEqual("a", eb22.A.Key.ColumnFamily);
                Assert.AreEqual("a2x", eb22.A.Key.ColumnQualifier);
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb21 = em.Find<EntityBX>(eb21.Key);
                Assert.IsInstanceOfType(_eb21, typeof(EntityB2X));
                Assert.AreEqual(eb21, _eb21);

                var _ea22 = em.Find<EntityAX>(eb22.A.Key);
                Assert.IsInstanceOfType(_ea22, typeof(EntityA2X));
                Assert.AreEqual(eb22.A, _ea22);

                var _eb22 = em.Find<EntityBX>(eb22.Key);
                Assert.IsInstanceOfType(_eb22, typeof(EntityB2X));
                Assert.AreEqual(eb22, _eb22);

                Assert.AreEqual(1, em.Fetch<EntityA2X>().Count());
                Assert.AreEqual(2, em.Fetch<EntityAX>().Count());
                Assert.AreEqual(2, em.Fetch<EntityB2X>().Count());
                Assert.AreEqual(4, em.Fetch<EntityBX>().Count());
                Assert.AreEqual(4, em.Fetch<IEntityBx>().Count());
                Assert.AreEqual(6, em.Fetch<EntityBaseX>().Count());
                Assert.AreEqual(6, em.Fetch<EntityBaseX>(new[] { typeof(EntityBX), typeof(EntityAX) }).Count());
            }
        }

        #endregion
    }
}