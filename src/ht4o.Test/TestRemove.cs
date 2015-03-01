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
    using Hypertable.Persistence.Test.Common;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The test remove.
    /// </summary>
    [TestClass]
    public class TestRemove : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The remove entity.
        /// </summary>
        [TestMethod]
        public void RemoveEntity()
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

                em.Remove(ec1);
                em.Flush();

                _ec1 = em.Find<EntityC>(ec1.Id);
                Assert.IsNull(_ec1);

                var _ec2 = em.Find<EntityC>(ec2.Id);
                Assert.AreEqual(ec2, _ec2);

                em.Remove(ec2);
                em.Flush();

                _ec2 = em.Find<EntityC>(ec2.Id);
                Assert.IsNull(_ec2);
            }
        }

        /// <summary>
        /// The remove key.
        /// </summary>
        [TestMethod]
        public void RemoveKey()
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

                em.Remove<EntityC>(ec1.Id);
                em.Flush();

                _ec1 = em.Find<EntityC>(ec1.Id);
                Assert.IsNull(_ec1);

                var _ec2 = em.Find<EntityC>(ec2.Id);
                Assert.AreEqual(ec2, _ec2);

                em.Remove<EntityC>(ec2.Id);
                em.Flush();

                _ec2 = em.Find<EntityC>(ec2.Id);
                Assert.IsNull(_ec2);
            }
        }

        #endregion
    }
}