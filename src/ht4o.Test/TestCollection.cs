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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Hypertable;
    using Hypertable.Persistence.Test.TestCollectionTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestCollectionTypes
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;

        using Hypertable.Persistence.Attributes;
        using Hypertable.Persistence.Test.Common;

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

            public string Id { get; private set; }

            public string Name { get; set; }

            public EntityX[] X { get; set; }

            public EntityY[] Y { get; set; }

            public EntityY Y2 { get; set; }

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
                if (!string.Equals(this.Name, c.Name))
                {
                    return false;
                }

                return Equatable.AreEqual(this.X, c.X) && Equatable.AreEqual(this.Y, c.Y) && (this.Y2 == null ? c.Y2 == null : this.Y2.Equals(c.Y2));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager")]
        internal class EntityD
        {
            #region Constructors and Destructors

            public EntityD()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public string Id { get; private set; }

            public string Name { get; set; }

            public ISet<EntityX> X { get; set; }

            public HashSet<EntityY> Y { get; set; }

            public EntityY Y2 { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityD))
                {
                    return false;
                }

                var c = o as EntityD;
                if (!string.Equals(this.Name, c.Name))
                {
                    return false;
                }

                return Equatable.AreEqual(this.X, c.X) && Equatable.AreEqual(this.Y, c.Y) && (this.Y2 == null ? c.Y2 == null : this.Y2.Equals(c.Y2));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager")]
        internal class EntityE
        {
            #region Constructors and Destructors

            public EntityE()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public string Id { get; private set; }

            public string Name { get; set; }

            public IList<EntityX> X { get; set; }

            public List<EntityY> Y { get; set; }

            public EntityY Y2 { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityE))
                {
                    return false;
                }

                var e = o as EntityE;
                if (!string.Equals(this.Name, e.Name))
                {
                    return false;
                }

                return Equatable.AreEqual(this.X, e.X) && Equatable.AreEqual(this.Y, e.Y) && (this.Y2 == null ? e.Y2 == null : this.Y2.Equals(e.Y2));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager")]
        internal class EntityF
        {
            #region Constructors and Destructors

            public EntityF()
            {
                this.Name = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public string Id { get; private set; }

            public string Name { get; set; }

            public IList X { get; set; }

            public ArrayList Y { get; set; }

            public EntityY Y2 { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object o)
            {
                if (ReferenceEquals(this, o))
                {
                    return true;
                }

                if (!(o is EntityF))
                {
                    return false;
                }

                var f = o as EntityF;
                if (!string.Equals(this.Name, f.Name))
                {
                    return false;
                }

                return ListEquals(this.X, f.X) && ListEquals(this.Y, f.Y) && (this.Y2 == null ? f.Y2 == null : this.Y2.Equals(f.Y2));
            }

            public override int GetHashCode()
            {
                return this.Name.GetHashCode();
            }

            #endregion

            #region Methods

            private static bool ListEquals(IList a, IList b)
            {
                if (ReferenceEquals(a, b))
                {
                    return true;
                }

                if (a == null)
                {
                    return false;
                }

                if (a.Count != b.Count)
                {
                    return false;
                }

                for (var i = 0; i < a.Count; ++i)
                {
                    if ((a[i] == null) != (b[i] == null))
                    {
                        return false;
                    }

                    if (a[i] != null && !a[i].Equals(b[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            #endregion
        }
    }

    /// <summary>
    /// The test collection.
    /// </summary>
    [TestClass]
    public class TestCollection : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            {
                var ec1 = new EntityC();
                TestBase.TestSerialization(ec1);

                var ey = new EntityY();
                var ec2 = new EntityC { X = new[] { new EntityX(), new EntityX() }, Y = new[] { ey, ey }, Y2 = ey };
                TestBase.TestSerialization(ec2);

                using (var em = Emf.CreateEntityManager())
                {
                    em.Persist(ec1);
                    Assert.IsFalse(string.IsNullOrEmpty(ec1.Id));

                    em.Persist(ec2);
                    Assert.IsFalse(string.IsNullOrEmpty(ec2.Id));
                }

                using (var em = Emf.CreateEntityManager())
                {
                    var _ec1 = em.Find<EntityC>(ec1.Id);
                    Assert.AreEqual(ec1, _ec1);

                    var _ec2 = em.Find<EntityC>(ec2.Id);
                    Assert.AreEqual(ec2, _ec2);
                    Assert.AreSame(_ec2.Y[0], _ec2.Y[1]);
                    Assert.AreSame(_ec2.Y2, _ec2.Y[0]);
                }
            }

            {
                var ed1 = new EntityD();
                TestBase.TestSerialization(ed1);

                var ey = new EntityY();
                var ed2 = new EntityD { X = new HashSet<EntityX>(new[] { new EntityX(), new EntityX() }), Y = new HashSet<EntityY>(new[] { ey, new EntityY() }), Y2 = ey };
                TestBase.TestSerialization(ed2);

                using (var em = Emf.CreateEntityManager())
                {
                    em.Persist(ed1);
                    Assert.IsFalse(string.IsNullOrEmpty(ed1.Id));

                    em.Persist(ed2);
                    Assert.IsFalse(string.IsNullOrEmpty(ed2.Id));
                }

                using (var em = Emf.CreateEntityManager())
                {
                    var _ed1 = em.Find<EntityD>(ed1.Id);
                    Assert.AreEqual(ed1, _ed1);

                    var _ed2 = em.Find<EntityD>(ed2.Id);
                    Assert.AreEqual(ed2, _ed2);
                    Assert.IsNotNull(_ed2.Y.FirstOrDefault(item => object.ReferenceEquals(item, _ed2.Y2)));
                }
            }

            {
                var ee1 = new EntityE();
                TestBase.TestSerialization(ee1);

                var ey = new EntityY();
                var ee2 = new EntityE { X = new List<EntityX>(new[] { new EntityX(), new EntityX() }), Y = new List<EntityY>(new[] { ey, ey }), Y2 = ey };
                TestBase.TestSerialization(ee2);

                using (var em = Emf.CreateEntityManager())
                {
                    em.Persist(ee1);
                    Assert.IsFalse(string.IsNullOrEmpty(ee1.Id));

                    em.Persist(ee2);
                    Assert.IsFalse(string.IsNullOrEmpty(ee2.Id));
                }

                using (var em = Emf.CreateEntityManager())
                {
                    var _ee1 = em.Find<EntityE>(ee1.Id);
                    Assert.AreEqual(ee1, _ee1);

                    var _ee2 = em.Find<EntityE>(ee2.Id);
                    Assert.AreEqual(ee2, _ee2);
                    Assert.AreSame(_ee2.Y[0], _ee2.Y[1]);
                    Assert.AreSame(_ee2.Y2, _ee2.Y[0]);
                }
            }

            {
                var ef1 = new EntityF();
                TestBase.TestSerialization(ef1);

                var ey = new EntityY();
                var ef2 = new EntityF { X = new ArrayList(new[] { new EntityX(), new EntityX() }), Y = new ArrayList(new[] { ey, ey }), Y2 = ey };
                TestBase.TestSerialization(ef2);

                using (var em = Emf.CreateEntityManager())
                {
                    em.Persist(ef1);
                    Assert.IsFalse(string.IsNullOrEmpty(ef1.Id));

                    em.Persist(ef2);
                    Assert.IsFalse(string.IsNullOrEmpty(ef2.Id));
                }

                using (var em = Emf.CreateEntityManager())
                {
                    var _ef1 = em.Find<EntityF>(ef1.Id);
                    Assert.AreEqual(ef1, _ef1);

                    var _ef2 = em.Find<EntityF>(ef2.Id);
                    Assert.AreEqual(ef2, _ef2);
                    Assert.AreSame(_ef2.Y[0], _ef2.Y[1]);
                    Assert.AreSame(_ef2.Y2, _ef2.Y[0]);
                }
            }
        }

        #endregion
    }
}