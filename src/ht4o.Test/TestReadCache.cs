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
    using System.Linq;

    using Hypertable;
    using Hypertable.Persistence.Test.TestReadCacheTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestReadCacheTypes
    {
        using System;
        using System.Collections.Generic;

        using Hypertable.Persistence.Attributes;
        using Hypertable.Persistence.Test.Common;

        [Entity("TestEntityManager")]
        internal abstract class Base
        {
            #region Constructors and Destructors

            protected Base(string id)
            {
                this.Id = id;
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

                if (!(o is Base))
                {
                    return false;
                }

                return string.Equals(this.Name, (o as Base).Name);
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager", ColumnFamily = "a")]
        [Serializable]
        internal class Parent : Base
        {
            #region Constructors and Destructors

            public Parent(string id)
                : base(id)
            {
                this.Children = new List<Child>();
            }

            #endregion

            #region Public Properties

            public List<Child> Children { get; private set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (object.ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is Parent))
                {
                    return false;
                }

                return base.Equals(o) && Equatable.AreEqual(this.Children, (o as Parent).Children);
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager", ColumnFamily = "b")]
        [Serializable]
        internal class Child : Base
        {
            #region Constructors and Destructors

            public Child(string id)
                : base(id)
            {
            }

            #endregion
        }
    }

    /// <summary>
    /// The read cache.
    /// </summary>
    [TestClass]
    public class TestReadCache : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            var p = new Parent("0");
            p.Children.Add(new Child("X"));
            p.Children.Add(new Child("Y"));
            p.Children.Add(new Child("Z"));
            TestBase.TestSerialization(p);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(p, Behaviors.CreateNew);
                Assert.AreEqual("0", p.Id);
                Assert.AreEqual("X", p.Children[0].Id);
                Assert.AreEqual("Y", p.Children[1].Id);
                Assert.AreEqual("Z", p.Children[2].Id);
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _p = em.Find<Parent>(p.Id);
                Assert.AreEqual(p, _p);
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _b = em.Fetch<Base>(new[] { typeof(Parent), typeof(Child) });
                var _p = _b.OfType<Parent>().First();
                Assert.AreEqual(p, _p);

                var i = 0;
                foreach (var c in _b.OfType<Child>().OrderBy(c => c.Id))
                {
                    Assert.AreEqual(p.Children[i], c);
                    Assert.AreSame(_p.Children[i++], c);
                }
            }
        }

        #endregion
    }
}