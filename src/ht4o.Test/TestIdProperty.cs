/** -*- C# -*-
 * Copyright (C) 2010-2014 Thalmann Software & Consulting, http://www.softdev.ch
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
    using Hypertable.Persistence.Test.TestIdPropertyTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestIdPropertyTypes
    {
        using System;

        using Hypertable.Persistence.Attributes;

        [Entity("TestEntityManager", ColumnFamily = "a")]
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

            public DateTime LastModified { get; private set; }

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

            public DateTime LastModified { get; private set; }

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

        internal abstract class EntityBase
        {
            #region Public Properties

            public string Id { get; private set; }

            public DateTime LastModified { get; private set; }

            #endregion
        }

        [Entity("TestEntityManager", ColumnFamily = "c")]
        internal class EntityC : EntityBase
        {
            #region Constructors and Destructors

            public EntityC()
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
                if (object.ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityC))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as EntityC).Name) && (this.A == null ? (o as EntityC).A == null : this.A.Equals((o as EntityC).A));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode() ^ (this.A != null ? this.A.GetHashCode() : 0);
            }

            #endregion
        }
    }

    /// <summary>
    /// The test id property.
    /// </summary>
    [TestClass]
    public class TestIdProperty : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            var eb1 = new EntityB();
            TestBase.TestSerialization(eb1);

            var eb2 = new EntityB { A = new EntityA() };
            TestBase.TestSerialization(eb2);

            var ec1 = new EntityC { A = new EntityA() };
            TestBase.TestSerialization(ec1);

            var dateTime = DateTime.UtcNow - TimeSpan.FromMilliseconds(250);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Id));
                Assert.AreEqual(0, eb1.LastModified.Ticks);

                em.Persist(eb2);
                Assert.IsFalse(string.IsNullOrEmpty(eb2.Id));
                Assert.IsFalse(eb2.A.Id.Equals(Guid.Empty));
                Assert.AreEqual(0, eb2.LastModified.Ticks);
                Assert.AreEqual(0, eb2.A.LastModified.Ticks);

                em.Persist(ec1);
                Assert.IsFalse(string.IsNullOrEmpty(ec1.Id));
                Assert.IsFalse(ec1.A.Id.Equals(Guid.Empty));
                Assert.AreEqual(0, ec1.LastModified.Ticks);
                Assert.AreEqual(0, ec1.A.LastModified.Ticks);
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityB>(eb1.Id);
                Assert.AreEqual(eb1, _eb1);
                Assert.IsTrue(_eb1.LastModified > dateTime);

                var _ea2 = em.Find<EntityA>(eb2.A.Id);
                Assert.AreEqual(eb2.A, _ea2);
                Assert.IsTrue(_ea2.LastModified > dateTime);

                var _eb2 = em.Find<EntityB>(eb2.Id);
                Assert.AreEqual(eb2, _eb2);
                Assert.IsTrue(_eb2.LastModified > dateTime);
                Assert.IsTrue(_eb2.A.LastModified > dateTime);
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _ec1 = em.Find<EntityC>(ec1.Id);
                Assert.AreEqual(ec1, _ec1);
                Assert.IsTrue(_ec1.LastModified > dateTime);
                Assert.IsTrue(_ec1.A.LastModified > dateTime);

                var _ea2 = em.Find<EntityA>(ec1.A.Id);
                Assert.AreEqual(ec1.A, _ea2);
                Assert.IsTrue(_ec1.A.LastModified > dateTime);
            }
        }

        #endregion
    }
}