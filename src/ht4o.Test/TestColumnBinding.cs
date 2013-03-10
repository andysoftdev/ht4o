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
    using Hypertable.Persistence.Bindings;
    using Hypertable.Persistence.Test.TestColumnBindingTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestColumnBindingTypes
    {
        using System;

        using Hypertable.Persistence.Attributes;

        [Entity("TestEntityManager")]
        internal class EntityA
        {
            #region Constructors and Destructors

            public EntityA()
            {
                this.Name = Guid.NewGuid().ToString();
                this.Key = new Key();
            }

            #endregion

            #region Public Properties

            public Key Key { get; private set; }

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

        internal class ColumnBindingEntityA : IColumnBinding
        {
            #region Public Properties

            public string ColumnFamily
            {
                get
                {
                    return "a";
                }
            }

            public string ColumnQualifier
            {
                get
                {
                    return "qa";
                }
            }

            #endregion
        }

        [Entity("TestEntityManager")]
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

            public Key Key { get; set; }

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
    }

    /// <summary>
    /// The test column binding.
    /// </summary>
    [TestClass]
    public class TestColumnBinding : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            var bindingContext = new BindingContext();
            Assert.IsFalse(bindingContext.StrictExplicitColumnBinding);
            Assert.IsFalse(bindingContext.StrictExplicitKeyBinding);
            Assert.IsFalse(bindingContext.StrictExplicitTableBinding);

            Assert.IsTrue(bindingContext.RegisterColumnBinding(typeof(EntityA), new ColumnBindingEntityA()));
            Assert.IsFalse(bindingContext.RegisterColumnBinding(typeof(EntityA), new ColumnBindingEntityA()));
            Assert.IsTrue(bindingContext.RegisterColumnBinding(typeof(EntityB), new ColumnBinding("b", "qb")));

            var eb1 = new EntityB();
            TestBase.TestSerialization(eb1);

            var eb2 = new EntityB { A = new EntityA() };
            TestBase.TestSerialization(eb2);

            using (var em = Emf.CreateEntityManager(bindingContext))
            {
                em.Configuration.Binding.StrictExplicitColumnBinding = true;

                em.Persist(eb1);
                Assert.IsNotNull(eb1.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Key.Row));
                Assert.AreEqual("b", eb1.Key.ColumnFamily);
                Assert.AreEqual("qb", eb1.Key.ColumnQualifier);

                em.Persist(eb2);
                Assert.IsNotNull(eb2.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb2.Key.Row));
                Assert.AreEqual("b", eb2.Key.ColumnFamily);
                Assert.AreEqual("qb", eb2.Key.ColumnQualifier);

                Assert.IsNotNull(eb2.A.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb2.A.Key.Row));
                Assert.AreEqual("a", eb2.A.Key.ColumnFamily);
                Assert.AreEqual("qa", eb2.A.Key.ColumnQualifier);
            }

            using (var em = Emf.CreateEntityManager(bindingContext))
            {
                var _eb1 = em.Find<EntityB>(eb1.Key);
                Assert.AreEqual(eb1, _eb1);

                var _ea2 = em.Find<EntityA>(eb2.A.Key);
                Assert.AreEqual(eb2.A, _ea2);

                var _eb2 = em.Find<EntityB>(eb2.Key);
                Assert.AreEqual(eb2, _eb2);
            }

            Assert.IsTrue(bindingContext.UnregisterColumnBinding(typeof(EntityA)));
            Assert.IsFalse(bindingContext.UnregisterColumnBinding(typeof(EntityA)));
            Assert.IsTrue(bindingContext.UnregisterColumnBinding(typeof(EntityB)));
        }

        #endregion
    }
}