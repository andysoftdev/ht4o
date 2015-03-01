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
    using Hypertable.Persistence.Bindings;
    using Hypertable.Persistence.Test.TestKeyBindingTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestKeyBindingTypes
    {
        using System;

        using Hypertable.Persistence.Attributes;

        internal class EntityA
        {
            #region Public Properties

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

        internal class KeyBindingEntityA : IKeyBinding
        {
            #region Public Methods and Operators

            public Key CreateKey(object entity)
            {
                var ea = (EntityA)entity;
                ea.Name = ea.GetType().Name + Guid.NewGuid().ToString();
                return new Key(ea.Name, "a"); // no entity key update
            }

            public Key KeyFromEntity(object entity)
            {
                var ea = (EntityA)entity;
                return new Key(ea.Name, "a");
            }

            public Key KeyFromValue(object value)
            {
                if (value is Key)
                {
                    return (Key)value;
                }

                if (value is EntityA)
                {
                    return this.KeyFromEntity(value);
                }

                return new Key((string)Convert.ChangeType(value, typeof(string)), "a");
            }

            public void SetKey(object entity, Key key)
            {
                var ea = (EntityA)entity;
                ea.Name = key.Row;
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

        internal class KeyBindingEntityB : IKeyBinding
        {
            #region Public Methods and Operators

            public Key CreateKey(object entity)
            {
                var eb = (EntityB)entity;
                eb.Key = new Key(eb.GetType().Name + Guid.NewGuid().ToString(), "a");
                return eb.Key;
            }

            public Key KeyFromEntity(object entity)
            {
                return ((EntityB)entity).Key;
            }

            public Key KeyFromValue(object value)
            {
                if (value is Key)
                {
                    return (Key)value;
                }

                if (value is EntityA)
                {
                    return this.KeyFromEntity(value);
                }

                return new Key((string)Convert.ChangeType(value, typeof(string)), "a");
            }

            public void SetKey(object entity, Key key)
            {
                ((EntityB)entity).Key = key;
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

            public string Name
            {
                get
                {
                    return this.Id;
                }

                set
                {
                    this.Id = value;
                }
            }

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
                return string.Equals(this.Name, c.Name) && (this.A == null ? c.A == null : this.A.Equals(c.A)) && (this.B == null ? c.B == null : this.B.Equals(c.B));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }
    }

    /// <summary>
    /// The test key binding.
    /// </summary>
    [TestClass]
    public class TestKeyBinding : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            var bindings = Emf.Configuration.Binding;

            Assert.IsTrue(bindings.RegisterKeyBinding(typeof(EntityA), new KeyBindingEntityA()));
            Assert.IsFalse(bindings.RegisterKeyBinding(typeof(EntityA), new KeyBindingEntityA()));
            Assert.IsTrue(bindings.RegisterKeyBinding(typeof(EntityB), new KeyBindingEntityB()));
            {
                var cb = new ColumnBinding("e");
                Assert.IsTrue(bindings.RegisterColumnBinding(typeof(EntityC), cb));
                Assert.IsTrue(bindings.RegisterKeyBinding(typeof(EntityC), new KeyBinding<EntityC>(e => e.Name)));
            }

            var eb1 = new EntityB();
            TestBase.TestSerialization(eb1);

            var eb2 = new EntityB { A = new EntityA() };
            TestBase.TestSerialization(eb2);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                Assert.IsNotNull(eb1.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Key.Row));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Key.ColumnFamily));
                Assert.IsTrue(string.IsNullOrEmpty(eb1.Key.ColumnQualifier));
                Assert.IsTrue(eb1.Key.Row.StartsWith(typeof(EntityB).Name));

                em.Persist(eb2);
                Assert.IsNotNull(eb2.Key);
                Assert.IsFalse(string.IsNullOrEmpty(eb2.Key.Row));
                Assert.IsFalse(string.IsNullOrEmpty(eb2.Key.ColumnFamily));
                Assert.IsTrue(string.IsNullOrEmpty(eb2.Key.ColumnQualifier));
                Assert.IsTrue(eb2.Key.Row.StartsWith(typeof(EntityB).Name));
                Assert.IsTrue(eb2.A.Name.StartsWith(typeof(EntityA).Name));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityB>(eb1.Key);
                Assert.AreEqual(eb1, _eb1);
                Assert.IsTrue(_eb1.Key.Row.StartsWith(_eb1.GetType().Name));

                var _ea2 = em.Find<EntityA>(eb2.A.Name); // EntityA key binding uses the Name property as row key
                Assert.AreEqual(eb2.A, _ea2);

                var _eb2 = em.Find<EntityB>(eb2.Key);
                Assert.AreEqual(eb2, _eb2);
                Assert.IsTrue(_eb2.Key.Row.StartsWith(typeof(EntityB).Name));
                Assert.AreEqual(eb2.A.Name, _eb2.A.Name);
                Assert.IsTrue(eb2.A.Name.StartsWith(typeof(EntityA).Name));
            }

            var ec1 = new EntityC { Name = "11", A = new EntityA() };
            TestBase.TestSerialization(ec1);

            var ec2 = new EntityC { Name = "222", A = new EntityA(), B = new EntityB() };
            TestBase.TestSerialization(ec2);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(ec1);
                Assert.AreEqual(ec1.Id, ec1.Name);

                em.Persist(ec2);
                Assert.AreEqual(ec2.Id, ec2.Name);
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _ec1 = em.Find<EntityC>("11");
                Assert.AreEqual(ec1, _ec1);

                var _ec2 = em.Find<EntityC>(ec2);
                Assert.AreEqual(ec2, _ec2);
            }

            bindings.UnregisterBinding(typeof(EntityA));
            bindings.UnregisterBinding(typeof(EntityB));
            bindings.UnregisterBinding(typeof(EntityC));
        }

        #endregion
    }
}