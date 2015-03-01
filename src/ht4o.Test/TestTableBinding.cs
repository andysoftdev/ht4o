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
namespace Hypertable.Persistence.Test
{
    using Hypertable;
    using Hypertable.Persistence.Test.TestTableBindingTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestTableBindingTypes
    {
        using System;

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

        internal class TableBindingEntityA : ITableBinding
        {
            #region Public Properties

            public string Namespace
            {
                get
                {
                    return null;
                }
            }

            public string TableName
            {
                get
                {
                    return "TestEntityManager";
                }
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

        internal class TableBindingEntityB : ITableBinding
        {
            #region Public Properties

            public string Namespace
            {
                get
                {
                    return null;
                }
            }

            public string TableName
            {
                get
                {
                    return "EntityB";
                }
            }

            #endregion
        }
    }

    /// <summary>
    /// The test table binding.
    /// </summary>
    [TestClass]
    public class TestTableBinding : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            var bindings = Emf.Configuration.Binding;
            Assert.IsFalse(bindings.StrictExplicitColumnBinding);
            Assert.IsFalse(bindings.StrictExplicitKeyBinding);
            Assert.IsFalse(bindings.StrictExplicitTableBinding);

            Assert.IsTrue(bindings.RegisterTableBinding(typeof(EntityA), new TableBindingEntityA()));
            Assert.IsFalse(bindings.RegisterTableBinding(typeof(EntityA), new TableBindingEntityA()));
            Assert.IsTrue(bindings.RegisterTableBinding(typeof(EntityB), new TableBindingEntityB()));

            var eb1 = new EntityB();
            TestBase.TestSerialization(eb1);

            var eb2 = new EntityB { A = new EntityA() };
            TestBase.TestSerialization(eb2);

            using (var em = Emf.CreateEntityManager())
            {
                Assert.IsTrue(em.IsTypeDeclared<EntityA>());
                Assert.IsTrue(em.IsTypeDeclared<EntityB>());

                em.Configuration.Binding.StrictExplicitTableBinding = true;

                em.Persist(eb1);
                Assert.IsNotNull(eb1.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Key.Row));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Key.ColumnFamily));
                Assert.IsTrue(string.IsNullOrEmpty(eb1.Key.ColumnQualifier));

                em.Persist(eb2);
                Assert.IsNotNull(eb2.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb2.Key.Row));
                Assert.IsFalse(string.IsNullOrEmpty(eb2.Key.ColumnFamily));
                Assert.IsTrue(string.IsNullOrEmpty(eb2.Key.ColumnQualifier));

                Assert.IsNotNull(eb2.A.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb2.A.Key.Row));
                Assert.IsFalse(string.IsNullOrEmpty(eb2.A.Key.ColumnFamily));
                Assert.IsTrue(string.IsNullOrEmpty(eb2.A.Key.ColumnQualifier));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityB>(eb1.Key);
                Assert.AreEqual(eb1, _eb1);

                var _ea2 = em.Find<EntityA>(eb2.A.Key);
                Assert.AreEqual(eb2.A, _ea2);

                var _eb2 = em.Find<EntityB>(eb2.Key);
                Assert.AreEqual(eb2, _eb2);
            }

            Assert.IsTrue(bindings.UnregisterTableBinding(typeof(EntityA)));
            Assert.IsFalse(bindings.UnregisterTableBinding(typeof(EntityA)));
            Assert.IsTrue(bindings.UnregisterTableBinding(typeof(EntityB)));
        }

        #endregion
    }
}