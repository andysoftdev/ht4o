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
namespace Hypertable.Persistence.TestPersistLoggingTypes
{
    using System;

    using Hypertable.Persistence.Attributes;

    [Entity(ColumnFamily = "a")]
    internal class EntityA
    {
        #region Constructors and Destructors

        public EntityA()
        {
            this.Name = Guid.NewGuid().ToString();
            this.Value = new byte[16];
        }

        #endregion

        #region Public Properties

        public Guid Id { get; private set; }

        public string Name { get; set; }

        public byte[] Value { get; set; }

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

    [Entity(ColumnFamily = "b")]
    internal class EntityB
    {
        #region Constructors and Destructors

        public EntityB()
        {
            this.Name = Guid.NewGuid().ToString();
            this.Value = new byte[16];
        }

        #endregion

        #region Public Properties

        public Guid Id { get; private set; }

        public string Name { get; set; }

        public byte[] Value { get; set; }

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

            return string.Equals(this.Name, (o as EntityB).Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        #endregion
    }
}

namespace Hypertable.Persistence.Test
{
    using System;

    using Hypertable;
    using Hypertable.Persistence.Serialization;
    using Hypertable.Persistence.Test.Common;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using EntityA = Hypertable.Persistence.TestPersistLoggingTypes.EntityA;
    using EntityB = Hypertable.Persistence.TestPersistLoggingTypes.EntityB;

    /// <summary>
    /// The test persist logging.
    /// </summary>
    [TestClass]
    public class TestPersistLogging : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist logging.
        /// </summary>
        [TestMethod]
        public void PersistLogging()
        {
            if (IsOdbc)
            {
                return; //TODO temporary skipped
            }

            var ea = new EntityA();
            TestBase.TestSerialization(ea);

            var eb = new EntityB();
            TestBase.TestSerialization(eb);

            for (var j = 0; j < 5; ++j)
            {
                using (var em = Emf.CreateEntityManager())
                {
                    em.Configuration.MutatorSpec = new MutatorSpec(MutatorKind.Chunked)
                        {
                            Queued = true, 
                            //// MaxCellCount = 8 * 1024,
                            MaxChunkSize = 64 * 1024, 
                            FlushEachChunk = true, 
                            Capacity = 32768
                        };

                    for (var i = 0; i < 100000; ++i)
                    {
                        ea = new EntityA();
                        Rng.Instance.NextBytes(ea.Value);
                        em.Persist(ea, Behaviors.CreateNew | Behaviors.DoNotCache);

                        eb = new EntityB();
                        Rng.Instance.NextBytes(eb.Value);
                        em.Persist(eb, Behaviors.CreateNew | Behaviors.DoNotCache);
                    }
                }
            }
        }

        /// <summary>
        /// The persist logging max performance.
        /// </summary>
        [TestMethod]
        public void PersistLoggingMaxPerformance()
        {
            if (IsOdbc)
            {
                return; //TODO temporary skipped
            }

            var ea = new EntityA();
            TestBase.TestSerialization(ea);

            var eb = new EntityB();
            TestBase.TestSerialization(eb);

            var rng = new Random();
            for (var j = 0; j < 5; ++j)
            {
                using (var em = Emf.CreateEntityManager())
                {
                    var mutatorSpec = new MutatorSpec(MutatorKind.Chunked)
                        {
                            Queued = true, 
                            //// MaxCellCount = 8 * 1024,
                            MaxChunkSize = 64 * 1024, 
                            FlushEachChunk = true, 
                            Capacity = 32768
                        };

                    using (var ma = em.GetTable<EntityA>().CreateMutator(mutatorSpec))
                    using (var mb = em.GetTable<EntityB>().CreateMutator(mutatorSpec))
                    {
                        for (var i = 0; i < 100000; ++i)
                        {
                            ea = new EntityA();
                            rng.NextBytes(ea.Value);
                            ma.Set(new Key { ColumnFamily = "a" }, Serializer.ToByteArray(ea), true);

                            eb = new EntityB();
                            rng.NextBytes(eb.Value);
                            mb.Set(new Key { ColumnFamily = "b" }, Serializer.ToByteArray(ea), true);
                        }
                    }
                }
            }
        }

        #endregion
    }
}