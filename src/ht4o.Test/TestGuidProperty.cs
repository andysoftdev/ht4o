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
    using Hypertable.Persistence.Test.TestGuidPropertyTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestGuidPropertyTypes
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

            public string Name { get; set; }

            [Id]
            public Guid RowKey { get; private set; }

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

            public string Name { get; set; }

            [Id]
            public Guid RowKey { get; private set; }

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
    }

    /// <summary>
    /// The test guid property.
    /// </summary>
    [TestClass]
    public class TestGuidProperty : TestBase
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

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                Assert.IsFalse(eb1.RowKey.Equals(Guid.Empty));

                em.Persist(eb2);
                Assert.IsFalse(eb2.RowKey.Equals(Guid.Empty));
                Assert.IsFalse(eb2.A.RowKey.Equals(Guid.Empty));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityB>(eb1.RowKey);
                Assert.AreEqual(eb1, _eb1);

                var _ea2 = em.Find<EntityA>(eb2.A.RowKey);
                Assert.AreEqual(eb2.A, _ea2);

                var _eb2 = em.Find<EntityB>(eb2.RowKey);
                Assert.AreEqual(eb2, _eb2);
            }
        }

        #endregion
    }
}