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
    using Hypertable;
    using Hypertable.Persistence.Test.TestCustomEncoderDecoderTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace TestCustomEncoderDecoderTypes
    {
        using Hypertable.Persistence.Attributes;
        using Hypertable.Persistence.Serialization;

        [Entity("TestEntityManager", ColumnFamily = "a")]
        internal class EntityA
        {
            #region Fields

            public double X;

            public double Y;

            public double Z;

            #endregion

            #region Constructors and Destructors

            static EntityA()
            {
                Encoder.Register(
                    100, 
                    typeof(EntityA), 
                    (serializer, any) =>
                        {
                            var binaryWriter = serializer.BinaryWriter;
                            var e = (EntityA)any;
                            Encoder.WriteDouble(binaryWriter, e.X, false);
                            Encoder.WriteDouble(binaryWriter, e.Y, false);
                            Encoder.WriteDouble(binaryWriter, e.Z, false);
                        }, 
                    (deserializer) =>
                        {
                            var binaryReader = deserializer.BinaryReader;
                            return new EntityA { X = Decoder.ReadDouble(binaryReader), Y = Decoder.ReadDouble(binaryReader), Z = Decoder.ReadDouble(binaryReader) };
                        });
            }

            #endregion

            #region Public Properties

            public string Id { get; private set; }

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

                var other = (EntityA)o;
                return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
            }

            public override int GetHashCode()
            {
                return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
            }

            #endregion
        }

        [Entity("TestEntityManager", ColumnFamily = "b")]
        internal class EntityB
        {
            #region Fields

            public EntityA A;

            public EntityA B;

            public EntityA C;

            #endregion

            #region Constructors and Destructors

            static EntityB()
            {
                Encoder.Register(
                    101, 
                    typeof(EntityB), 
                    (serializer, any) =>
                        {
                            var e = (EntityB)any;
                            serializer.WriteObject(e.A);
                            serializer.WriteObject(e.B);
                            serializer.WriteObject(e.C);
                        }, 
                    (deserializer) =>
                        {
                            var eb = new EntityB();
                            deserializer.ReadObject<EntityA>(value => { eb.A = value; });
                            deserializer.ReadObject<EntityA>(value => { eb.B = value; });
                            deserializer.ReadObject<EntityA>(value => { eb.C = value; });
                            return eb;
                        });
            }

            #endregion

            #region Public Properties

            public string Id { get; private set; }

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

                var other = (EntityB)o;
                return (ReferenceEquals(this.A, other.A) || (this.A != null && this.A.Equals(other.A)))
                       && (ReferenceEquals(this.B, other.B) || (this.B != null && this.B.Equals(other.B)))
                       && (ReferenceEquals(this.C, other.C) || (this.C != null && this.C.Equals(other.C)));
            }

            public override int GetHashCode()
            {
                return (this.A != null ? this.A.GetHashCode() : 0) * (this.B != null ? this.B.GetHashCode() : 0) * (this.C != null ? this.C.GetHashCode() : 0);
            }

            #endregion
        }
    }

    /// <summary>
    /// The test custom encoder decoder.
    /// </summary>
    [TestClass]
    public class TestCustomEncoderDecoder : TestBase
    {
        #region Public Methods and Operators

        /// <summary>
        /// The persist and find.
        /// </summary>
        [TestMethod]
        public void PersistAndFind()
        {
            var ea1 = new EntityA { X = 1, Y = 2, Z = 3 };
            TestBase.TestSerialization(ea1);

            var ea2 = new EntityA { X = 4, Y = 5, Z = 6 };
            TestBase.TestSerialization(ea2);

            var eb1 = new EntityB { A = ea1, B = ea2 };

            TestBase.TestSerialization(eb1);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.A.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.B.Id));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityB>(eb1.Id);
                Assert.AreEqual(eb1, _eb1);
            }

            eb1 = new EntityB { A = ea1, B = ea2, C = ea1 };

            TestBase.TestSerialization(eb1);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.A.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.B.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.C.Id));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityB>(eb1.Id);
                Assert.AreEqual(eb1, _eb1);
            }

            eb1 = new EntityB { A = ea1, B = ea1, C = ea2 };

            TestBase.TestSerialization(eb1);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.A.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.B.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.C.Id));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityB>(eb1.Id);
                Assert.AreEqual(eb1, _eb1);
            }

            eb1 = new EntityB { A = ea1, B = ea1, C = ea1 };

            TestBase.TestSerialization(eb1);

            using (var em = Emf.CreateEntityManager())
            {
                em.Persist(eb1);
                Assert.IsFalse(string.IsNullOrEmpty(eb1.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.A.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.B.Id));
                Assert.IsFalse(string.IsNullOrEmpty(eb1.C.Id));
            }

            using (var em = Emf.CreateEntityManager())
            {
                var _eb1 = em.Find<EntityB>(eb1.Id);
                Assert.AreEqual(eb1, _eb1);
            }
        }

        #endregion
    }
}