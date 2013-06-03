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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using Hypertable;
    using Hypertable.Persistence.Test.Common;
    using Hypertable.Persistence.Test.TestEntityAttributeInheritTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using EntityBase = Hypertable.Persistence.Test.Common.EntityBase;

    /// <summary>
    /// The test query.
    /// </summary>
    [TestClass]
    public class TestQuery : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// Tests the entity manager Any method.
        /// </summary>
        [TestMethod]
        public void Any()
        {
            var ec1 = new EntityC { A = new EntityA(), B = new EntityB(), X = new EntityX(), Y = new EntityY() };
            TestBase.TestSerialization(ec1);

            using (var em = Emf.CreateEntityManager())
            {
                Assert.IsFalse(em.Any<EntityC>());
                Assert.IsFalse(em.Any<EntityA>());
                Assert.IsFalse(em.Any<EntityB>());
                Assert.IsFalse(em.Any<EntityXBase>());
                Assert.IsFalse(em.Any<EntityX>());
                Assert.IsFalse(em.Any<EntityX2>());
                Assert.IsFalse(em.Any<EntityY>());

                Assert.IsFalse(em.Any(new[] { typeof(EntityX), typeof(EntityX2) }));
            }

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(ec1);
            }

            using (var em = Emf.CreateEntityManager())
            {
                Assert.IsTrue(em.Any<EntityC>());
                Assert.IsTrue(em.Any<EntityA>());
                Assert.IsTrue(em.Any<EntityB>());
                Assert.IsTrue(em.Any<EntityXBase>());
                Assert.IsTrue(em.Any<EntityX>());
                Assert.IsFalse(em.Any<EntityX2>());
                Assert.IsTrue(em.Any<EntityY>());

                Assert.IsTrue(em.Any(new[] { typeof(EntityX), typeof(EntityX2) }));
            }

            var ex21 = new EntityX2();
            TestBase.TestSerialization(ex21);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(ex21);
                em.Flush();

                Assert.IsTrue(em.Any<EntityXBase>());
                Assert.IsTrue(em.Any<EntityX>());
                Assert.IsTrue(em.Any<EntityX2>());
                Assert.IsTrue(em.Any(new[] { typeof(EntityX), typeof(EntityX2) }));

                em.Remove(ec1.X);
                em.Flush();

                Assert.IsTrue(em.Any<EntityXBase>());
                Assert.IsFalse(em.Any<EntityX>());
                Assert.IsTrue(em.Any<EntityX2>());
                Assert.IsTrue(em.Any(new[] { typeof(EntityX), typeof(EntityX2) }));

                em.Remove(ex21);
                em.Flush();

                Assert.IsFalse(em.Any<EntityXBase>());
                Assert.IsFalse(em.Any<EntityX>());
                Assert.IsFalse(em.Any<EntityX2>());
                Assert.IsFalse(em.Any(new[] { typeof(EntityX), typeof(EntityX2) }));
            }
        }

        /// <summary>
        /// Tests the entity manager Fetch method.
        /// </summary>
        [TestMethod]
        public void Fetch()
        {
            var ec1 = new EntityC { A = new EntityA(), B = new EntityB(), X = new EntityX(), Y = new EntityY() };
            TestBase.TestSerialization(ec1);

            var ec2 = new EntityC { A = new EntityA(), B = new EntityB(), X = new EntityX(), Y = new EntityY() };
            TestBase.TestSerialization(ec2);

            var ex1 = new EntityX();
            TestBase.TestSerialization(ex1);

            var ex2 = new EntityX();
            TestBase.TestSerialization(ex2);

            var ex21 = new EntityX2();
            TestBase.TestSerialization(ex21);

            var ex22 = new EntityX2();
            TestBase.TestSerialization(ex22);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(ec1);
                em.Persist(ec2);

                em.Persist(ex1);
                em.Persist(ex2);
                em.Persist(ex21);
                em.Persist(ex22);
            }

            using (var em = Emf.CreateEntityManager())
            {
                {
                    var _ec = em.Fetch<EntityC>();
                    Assert.IsNotNull(_ec);
                    ISet<EntityC> s = new HashSet<EntityC>(_ec.ToList());
                    Assert.AreEqual(2, s.Count);
                    Assert.IsTrue(s.Contains(ec1));
                    Assert.IsTrue(s.Contains(ec2));
                }

                {
                    var _ec = em.Fetch<EntityBase>(new[] { typeof(EntityC) });
                    Assert.IsNotNull(_ec);
                    ISet<EntityBase> s = new HashSet<EntityBase>(_ec.ToList());
                    Assert.AreEqual(2, s.Count);
                    Assert.IsTrue(s.Contains(ec1));
                    Assert.IsTrue(s.Contains(ec2));
                }

                Assert.AreEqual(2, em.Fetch<EntityA>().Count());
                Assert.AreEqual(2, em.Fetch<EntityB>().Count());
                Assert.AreEqual(2, em.Fetch<EntityC>().Count());
                Assert.AreEqual(4, em.Fetch<EntityX>().Count());
                Assert.AreEqual(2, em.Fetch<EntityX2>().Count());
                Assert.AreEqual(6, em.Fetch<EntityXBase>().Count());
                Assert.AreEqual(2, em.Fetch<EntityY>().Count());
                Assert.AreEqual(10, em.Fetch<EntityBase>().Count());
                Assert.AreEqual(6, em.Fetch<EntityBase>(new[] { typeof(EntityX), typeof(EntityX2) }).Count());
                Assert.AreEqual(2, em.Fetch<EntityBase>(new[] { typeof(EntityY) }).Count());

                // pre-fetch
                // TODOAssert.AreEqual(2, em.Fetch<EntityC>(new[] { typeof(EntityC), typeof(EntityX), typeof(EntityY) }).Count());
            }
        }

        /// <summary>
        /// Tests the entity manager Fetch method.
        /// </summary>
        [TestMethod]
        public void FetchByScanSpec()
        {
            var ec1 = new EntityC { A = new EntityA(), B = new EntityB(), X = new EntityX(), Y = new EntityY() };
            TestBase.TestSerialization(ec1);

            var ex21 = new EntityX2();
            TestBase.TestSerialization(ex21);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(ec1);
                em.Persist(ex21);
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _ec = em.Fetch<EntityC>(new ScanSpec(ec1.Id));
                Assert.IsNotNull(_ec);
                Assert.AreEqual(1, _ec.Count());
                Assert.AreEqual(ec1, _ec.First());

                var ss = new ScanSpec();
                ss.AddColumn("a");
                var _ex = em.Fetch<EntityXBase>(ss);
                Assert.IsNotNull(_ex);
                Assert.AreEqual(2, _ex.Count());
                Assert.AreEqual(ec1.X, _ex.OfType<EntityX>().First());
                Assert.AreEqual(ex21, _ex.OfType<EntityX2>().First());
            }
        }

        /// <summary>
        /// Tests the entity manager Find method.
        /// </summary>
        [TestMethod]
        public void Find()
        {
            var ec1 = new EntityC();
            TestBase.TestSerialization(ec1);

            var ec2 = new EntityC();
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
            }
        }

        /// <summary>
        /// Tests the entity manager FindMany method.
        /// </summary>
        [TestMethod]
        public void FindMany()
        {
            var ec1 = new EntityC();
            TestBase.TestSerialization(ec1);

            var ec2 = new EntityC();
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
                {
                    var _ec = em.FindMany<EntityC>(new[] { ec1.Id, ec2.Id });
                    Assert.IsNotNull(_ec);
                    var l = _ec.ToList();
                    Assert.AreEqual(ec1, l[0]);
                    Assert.AreEqual(ec2, l[1]);
                }

                {
                    var _ec = em.FindMany<EntityC>(new[] { ec1.Id, ec2.Id, ec2.Id, ec1.Id });
                    Assert.IsNotNull(_ec);
                    var l = _ec.ToList();
                    Assert.AreEqual(ec1, l[0]);
                    Assert.AreEqual(ec2, l[1]);
                    Assert.AreEqual(ec2, l[2]);
                    Assert.AreEqual(ec1, l[3]);
                }

                {
                    var _ec = em.FindMany<EntityBase>(new[] { ec1.Id, ec2.Id, ec2.Id, ec1.Id });
                    Assert.IsNotNull(_ec);
                    var l = _ec.ToList();
                    Assert.AreEqual(ec1, l[0]);
                    Assert.AreEqual(ec2, l[1]);
                    Assert.AreEqual(ec2, l[2]);
                    Assert.AreEqual(ec1, l[3]);
                }
            }
        }

        #endregion
    }
}