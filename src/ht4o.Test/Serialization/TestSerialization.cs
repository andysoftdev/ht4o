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
namespace Hypertable.Persistence.Test.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml;

    using Hypertable.Persistence.Serialization;
    using Hypertable.Persistence.Test.Common;
    using Hypertable.Persistence.Test.Serialization.TestSerializationTypes;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Decoder = Hypertable.Persistence.Serialization.Decoder;
    using Encoder = Hypertable.Persistence.Serialization.Encoder;

    namespace TestSerializationTypes
    {
        using System;
        using System.Runtime.Serialization;
        using System.Text;

        using Hypertable.Persistence.Test.Common;

        using Microsoft.VisualStudio.TestTools.UnitTesting;

        #region Enums

        internal enum Numbers
        {
            One, 

            Two, 

            Three
        }

        #endregion

        #region Interfaces

        internal interface IfcA
        {
            #region Public Properties

            double Double { get; set; }

            #endregion

            #region Public Methods and Operators

            void AssertIsEqualIfcA(object o);

            IfcA Randomize();

            #endregion
        }

        internal interface IfcB : IfcA
        {
            #region Public Properties

            long Long { get; set; }

            #endregion

            #region Public Methods and Operators

            void AssertIsEqualIfcB(object o);

            new IfcB Randomize();

            #endregion
        }

        #endregion

        internal struct CustomTypeA
        {
            #region Fields

            public double X;

            public int Y;

            public long Z;

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualCustomTypeA(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(CustomTypeA));
                var cta = (CustomTypeA)o;
                Assert.AreEqual(this.X, cta.X);
                Assert.AreEqual(this.Y, cta.Y);
                Assert.AreEqual(this.Z, cta.Z);
            }

            #endregion
        }

        internal struct StructA
        {
            #region Public Properties

            public double X { get; set; }

            public double Y { get; set; }

            public double Z { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectA_Nullable(object o)
            {
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(StructA_Nullable));
                var san = (StructA_Nullable)o;
                Assert.IsTrue(Equatable.AreEqual(this.X, san.X));
                Assert.IsTrue(Equatable.AreEqual(this.Y, san.Y));
                Assert.IsTrue(Equatable.AreEqual(this.Z, san.Z));
            }

            public void AssertIsEqualStructA(object o)
            {
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(StructA));
                var sa = (StructA)o;
                Assert.AreEqual(this.X, sa.X);
                Assert.AreEqual(this.Y, sa.Y);
                Assert.AreEqual(this.Z, sa.Z);
            }

            #endregion
        }

        internal struct StructA_Nullable
        {
            #region Public Properties

            public double? X { get; set; }

            public double? Y { get; set; }

            public double? Z { get; set; }

            #endregion
        }

        [Serializable]
        internal struct StructB
        {
            #region Fields

            public double X;

            public double Y;

            public double Z;

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectB_Nullable(object o)
            {
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(StructB_Nullable));
                var sbn = (StructB_Nullable)o;
                Assert.IsTrue(Equatable.AreEqual(this.X, sbn.X));
                Assert.IsTrue(Equatable.AreEqual(this.Y, sbn.Y));
                Assert.IsTrue(Equatable.AreEqual(this.Z, sbn.Z));
            }

            public void AssertIsEqualStructB(object o)
            {
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(StructB));
                var sb = (StructB)o;
                Assert.AreEqual(this.X, sb.X);
                Assert.AreEqual(this.Y, sb.Y);
                Assert.AreEqual(this.Z, sb.Z);
            }

            #endregion
        }

        [Serializable]
        internal struct StructB_Nullable
        {
            #region Fields

#pragma warning disable 649 // Field is never assigned to, and will always have its default value

            public double? X;

            public double? Y;

            public double? Z;

#pragma warning restore 649

            #endregion
        }

        internal class IfcAImpl : IfcA
        {
            #region Public Properties

            public double Double { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualIfcA(object o)
            {
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(IfcA));
                var ia = (IfcA)o;
                Assert.IsTrue(this.Double.Equals(ia.Double));
            }

            public IfcA Randomize()
            {
                this.Double = Rng.Instance.NextDouble();
                return this;
            }

            #endregion
        }

        internal class IfcAImplOther : IfcA
        {
            #region Public Properties

            public double Double { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualIfcA(object o)
            {
                Assert.IsFalse(object.ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(IfcA));
                var ia = (IfcA)o;
                Assert.IsTrue(this.Double.Equals(ia.Double));
            }

            public IfcA Randomize()
            {
                this.Double = Rng.Instance.NextDouble();
                return this;
            }

            #endregion
        }

        internal class IfcBImpl : IfcB
        {
            #region Public Properties

            public double Double { get; set; }

            public long Long { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualIfcA(object o)
            {
                Assert.IsFalse(object.ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(IfcA));
                var ia = (IfcA)o;
                Assert.IsTrue(this.Double.Equals(ia.Double));
            }

            public void AssertIsEqualIfcB(object o)
            {
                Assert.IsFalse(object.ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(IfcB));
                var ia = (IfcB)o;
                Assert.IsTrue(this.Double.Equals(ia.Double));
                Assert.IsTrue(this.Long.Equals(ia.Long));
            }

            public IfcB Randomize()
            {
                this.Double = Rng.Instance.NextDouble();
                this.Long = (long)(Rng.Instance.Next(int.MaxValue) - int.MaxValue / 2) * Rng.Instance.Next(byte.MaxValue);
                return this;
            }

            #endregion

            #region Explicit Interface Methods

            IfcA IfcA.Randomize()
            {
                this.Double = Rng.Instance.NextDouble();
                return this;
            }

            #endregion
        }

        internal class IfcBImplOther : IfcB
        {
            #region Public Properties

            public double Double { get; set; }

            public long Long { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualIfcA(object o)
            {
                Assert.IsFalse(object.ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(IfcA));
                var ia = (IfcA)o;
                Assert.IsTrue(this.Double.Equals(ia.Double));
            }

            public void AssertIsEqualIfcB(object o)
            {
                Assert.IsFalse(object.ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(IfcB));
                var ia = (IfcB)o;
                Assert.IsTrue(this.Double.Equals(ia.Double));
                Assert.IsTrue(this.Long.Equals(ia.Long));
            }

            public IfcB Randomize()
            {
                this.Double = Rng.Instance.NextDouble();
                this.Long = (long)(Rng.Instance.Next(int.MaxValue) - int.MaxValue / 2) * Rng.Instance.Next(byte.MaxValue);
                return this;
            }

            #endregion

            #region Explicit Interface Methods

            IfcA IfcA.Randomize()
            {
                this.Double = Rng.Instance.NextDouble();
                return this;
            }

            #endregion
        }

        internal class ObjectA
        {
            #region Public Properties

            public bool Boolean { get; set; }

            public byte Byte { get; set; }

            public char Char { get; set; }

            public DateTime DateTime { get; set; }

            public decimal Decimal { get; set; }

            public double Double { get; set; }

            public float Float { get; set; }

            public int GetOnly
            {
                get
                {
                    return 0;
                }
            }

            public Guid Guid { get; set; }

            public int Int { get; set; }

            public long Long { get; set; }

            public sbyte SByte { get; set; }

            public int SetOnly
            {
                set
                {
                }
            }

            public short Short { get; set; }

            public string String { get; set; }

            public StringBuilder StringBuilder { get; set; }

            public TimeSpan TimeSpan { get; set; }

            public uint UInt { get; set; }

            public ulong ULong { get; set; }

            public ushort UShort { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectA(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectA));
                var oa = (ObjectA)o;
                Assert.IsTrue(this.Boolean.Equals(oa.Boolean));
                Assert.IsTrue(this.SByte.Equals(oa.SByte));
                Assert.IsTrue(this.Byte.Equals(oa.Byte));
                Assert.IsTrue(this.Short.Equals(oa.Short));
                Assert.IsTrue(this.UShort.Equals(oa.UShort));
                Assert.IsTrue(this.Int.Equals(oa.Int));
                Assert.IsTrue(this.UInt.Equals(oa.UInt));
                Assert.IsTrue(this.Long.Equals(oa.Long));
                Assert.IsTrue(this.ULong.Equals(oa.ULong));
                Assert.IsTrue(this.Char.Equals(oa.Char));
                Assert.IsTrue(this.Float.Equals(oa.Float));
                Assert.IsTrue(this.Double.Equals(oa.Double));
                Assert.IsTrue(this.Decimal.Equals(oa.Decimal));
                Assert.IsTrue(this.DateTime.Equals(oa.DateTime));
                Assert.IsTrue(this.TimeSpan.Equals(oa.TimeSpan));
                Assert.IsTrue(string.Equals(this.String, oa.String));
                Assert.IsTrue(string.Equals(this.StringBuilder != null ? this.StringBuilder.ToString() : null, oa.StringBuilder != null ? oa.StringBuilder.ToString() : null));
                Assert.IsTrue(this.Guid.Equals(oa.Guid));
            }

            public void AssertIsEqualObjectA_Nullable(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectA_Nullable));
                var oan = (ObjectA_Nullable)o;
                Assert.IsTrue(Equatable.AreEqual(this.Boolean, oan.Boolean));
                Assert.IsTrue(Equatable.AreEqual(this.SByte, oan.SByte));
                Assert.IsTrue(Equatable.AreEqual(this.Byte, oan.Byte));
                Assert.IsTrue(Equatable.AreEqual(this.Short, oan.Short));
                Assert.IsTrue(Equatable.AreEqual(this.UShort, oan.UShort));
                Assert.IsTrue(Equatable.AreEqual(this.Int, oan.Int));
                Assert.IsTrue(Equatable.AreEqual(this.UInt, oan.UInt));
                Assert.IsTrue(Equatable.AreEqual(this.Long, oan.Long));
                Assert.IsTrue(Equatable.AreEqual(this.ULong, oan.ULong));
                Assert.IsTrue(Equatable.AreEqual(this.Char, oan.Char));
                Assert.IsTrue(Equatable.AreEqual(this.Float, oan.Float));
                Assert.IsTrue(Equatable.AreEqual(this.Double, oan.Double));
                Assert.IsTrue(Equatable.AreEqual(this.Decimal, oan.Decimal));
                Assert.IsTrue(Equatable.AreEqual(this.DateTime, oan.DateTime));
                Assert.IsTrue(Equatable.AreEqual(this.TimeSpan, oan.TimeSpan));
                Assert.IsTrue(string.Equals(this.String, oan.String));
                Assert.IsTrue(string.Equals(this.StringBuilder != null ? this.StringBuilder.ToString() : null, oan.StringBuilder != null ? oan.StringBuilder.ToString() : null));
                Assert.IsTrue(Equatable.AreEqual(this.Guid, oan.Guid));
            }

            public ObjectA Randomize()
            {
                this.Boolean = (Rng.Instance.Next() % 2) == 0;
                this.SByte = (sbyte)(Rng.Instance.Next(byte.MaxValue) - sbyte.MinValue);
                this.Byte = (byte)Rng.Instance.Next(byte.MaxValue);
                this.Short = (short)(Rng.Instance.Next(ushort.MaxValue) - short.MinValue);
                this.UShort = (ushort)Rng.Instance.Next(ushort.MaxValue);
                this.Int = Rng.Instance.Next(int.MaxValue) - int.MaxValue / 2;
                this.UInt = (uint)Rng.Instance.Next(int.MaxValue);
                this.Long = (long)(Rng.Instance.Next(int.MaxValue) - int.MaxValue / 2) * Rng.Instance.Next(byte.MaxValue);
                this.ULong = (ulong)Rng.Instance.Next(int.MaxValue) * (ulong)Rng.Instance.Next(byte.MaxValue);
                this.Char = (char)Rng.Instance.Next(byte.MaxValue);
                this.Float = (float)Rng.Instance.NextDouble();
                this.Double = Rng.Instance.NextDouble();
                this.Decimal = Rng.Instance.Next(int.MaxValue);
                this.DateTime = DateTime.UtcNow;
                this.TimeSpan = TimeSpan.FromTicks(Rng.Instance.Next(int.MaxValue));
                this.String = Guid.NewGuid().ToString();
                this.StringBuilder = new StringBuilder(Guid.NewGuid().ToString());
                this.Guid = Guid.NewGuid();

                return this;
            }

            #endregion
        }

        internal class ObjectA_Nullable
        {
            #region Public Properties

            public bool? Boolean { get; set; }

            public byte? Byte { get; set; }

            public char? Char { get; set; }

            public DateTime? DateTime { get; set; }

            public decimal? Decimal { get; set; }

            public double? Double { get; set; }

            public float? Float { get; set; }

            public int GetOnly
            {
                get
                {
                    return 0;
                }
            }

            public Guid? Guid { get; set; }

            public int? Int { get; set; }

            public long? Long { get; set; }

            public sbyte? SByte { get; set; }

            public int SetOnly
            {
                set
                {
                }
            }

            public short? Short { get; set; }

            public string String { get; set; }

            public StringBuilder StringBuilder { get; set; }

            public TimeSpan? TimeSpan { get; set; }

            public uint? UInt { get; set; }

            public ulong? ULong { get; set; }

            public ushort? UShort { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectA(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectA));
                var oa = (ObjectA)o;
                Assert.IsTrue(Equatable.AreEqual(this.Boolean, oa.Boolean));
                Assert.IsTrue(Equatable.AreEqual(this.SByte, oa.SByte));
                Assert.IsTrue(Equatable.AreEqual(this.Byte, oa.Byte));
                Assert.IsTrue(Equatable.AreEqual(this.Short, oa.Short));
                Assert.IsTrue(Equatable.AreEqual(this.UShort, oa.UShort));
                Assert.IsTrue(Equatable.AreEqual(this.Int, oa.Int));
                Assert.IsTrue(Equatable.AreEqual(this.UInt, oa.UInt));
                Assert.IsTrue(Equatable.AreEqual(this.Long, oa.Long));
                Assert.IsTrue(Equatable.AreEqual(this.ULong, oa.ULong));
                Assert.IsTrue(Equatable.AreEqual(this.Char, oa.Char));
                Assert.IsTrue(Equatable.AreEqual(this.Float, oa.Float));
                Assert.IsTrue(Equatable.AreEqual(this.Double, oa.Double));
                Assert.IsTrue(Equatable.AreEqual(this.Decimal, oa.Decimal));
                Assert.IsTrue(Equatable.AreEqual(this.DateTime, oa.DateTime));
                Assert.IsTrue(Equatable.AreEqual(this.TimeSpan, oa.TimeSpan));
                Assert.IsTrue(string.Equals(this.String, oa.String));
                Assert.IsTrue(string.Equals(this.StringBuilder != null ? this.StringBuilder.ToString() : null, oa.StringBuilder != null ? oa.StringBuilder.ToString() : null));
                Assert.IsTrue(Equatable.AreEqual(this.Guid, oa.Guid));
            }

            public void AssertIsEqualObjectA_Nullable(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectA_Nullable));
                var oan = (ObjectA_Nullable)o;
                Assert.IsTrue(Equatable.AreEqual(this.Boolean, oan.Boolean));
                Assert.IsTrue(Equatable.AreEqual(this.SByte, oan.SByte));
                Assert.IsTrue(Equatable.AreEqual(this.Byte, oan.Byte));
                Assert.IsTrue(Equatable.AreEqual(this.Short, oan.Short));
                Assert.IsTrue(Equatable.AreEqual(this.UShort, oan.UShort));
                Assert.IsTrue(Equatable.AreEqual(this.Int, oan.Int));
                Assert.IsTrue(Equatable.AreEqual(this.UInt, oan.UInt));
                Assert.IsTrue(Equatable.AreEqual(this.Long, oan.Long));
                Assert.IsTrue(Equatable.AreEqual(this.ULong, oan.ULong));
                Assert.IsTrue(Equatable.AreEqual(this.Char, oan.Char));
                Assert.IsTrue(Equatable.AreEqual(this.Float, oan.Float));
                Assert.IsTrue(Equatable.AreEqual(this.Double, oan.Double));
                Assert.IsTrue(Equatable.AreEqual(this.Decimal, oan.Decimal));
                Assert.IsTrue(Equatable.AreEqual(this.DateTime, oan.DateTime));
                Assert.IsTrue(Equatable.AreEqual(this.TimeSpan, oan.TimeSpan));
                Assert.IsTrue(string.Equals(this.String, oan.String));
                Assert.IsTrue(string.Equals(this.StringBuilder != null ? this.StringBuilder.ToString() : null, oan.StringBuilder != null ? oan.StringBuilder.ToString() : null));
                Assert.IsTrue(Equatable.AreEqual(this.Guid, oan.Guid));
            }

            public ObjectA_Nullable Randomize()
            {
                this.Boolean = (Rng.Instance.Next() % 2) == 0;
                this.SByte = (sbyte)(Rng.Instance.Next(byte.MaxValue) - sbyte.MinValue);
                this.Byte = (byte)Rng.Instance.Next(byte.MaxValue);
                this.Short = (short)(Rng.Instance.Next(ushort.MaxValue) - short.MinValue);
                this.UShort = (ushort)Rng.Instance.Next(ushort.MaxValue);
                this.Int = Rng.Instance.Next(int.MaxValue) - int.MaxValue / 2;
                this.UInt = (uint)Rng.Instance.Next(int.MaxValue);
                this.Long = (long)(Rng.Instance.Next(int.MaxValue) - int.MaxValue / 2) * Rng.Instance.Next(byte.MaxValue);
                this.ULong = (ulong)Rng.Instance.Next(int.MaxValue) * (ulong)Rng.Instance.Next(byte.MaxValue);
                this.Char = (char)Rng.Instance.Next(byte.MaxValue);
                this.Float = (float)Rng.Instance.NextDouble();
                this.Double = Rng.Instance.NextDouble();
                this.Decimal = Rng.Instance.Next(int.MaxValue);
                this.DateTime = System.DateTime.UtcNow;
                this.TimeSpan = System.TimeSpan.FromTicks(Rng.Instance.Next(int.MaxValue));
                this.String = System.Guid.NewGuid().ToString();
                this.StringBuilder = new StringBuilder(System.Guid.NewGuid().ToString());
                this.Guid = System.Guid.NewGuid();

                return this;
            }

            #endregion
        }

        internal abstract class ObjectBase
        {
            #region Constructors and Destructors

            protected ObjectBase()
            {
                this.Id = Guid.NewGuid().ToString();
            }

            #endregion

            #region Public Properties

            public string Id { get; set; }

            #endregion
        }

        internal class ObjectBaseJ : ObjectBase
        {
            #region Public Properties

            public ObjectA A { get; set; }

            #endregion
        }

        internal class ObjectC
        {
            #region Public Properties

            public bool Boolean { get; set; }

            public double[] DoubleArray { get; set; }

            public int GetOnly
            {
                get
                {
                    return 0;
                }
            }

            public Guid Guid { get; set; }

            public int Int { get; set; }

            public ObjectA ObjectA { get; set; }

            public ObjectA[] ObjectAArray { get; set; }

            public ObjectA[,] ObjectAArray2 { get; set; }

            public ObjectA[][] ObjectAArray22 { get; set; }

            public ObjectA_Nullable ObjectAN { get; set; }

            public ObjectA_Nullable[] ObjectANArray { get; set; }

            public int SetOnly
            {
                set
                {
                }
            }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectC(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectC));
                var oc = (ObjectC)o;
                Assert.IsTrue(this.Boolean.Equals(oc.Boolean));
                Assert.IsTrue(this.Int.Equals(oc.Int));

                Assert.IsTrue((this.ObjectA == null) == (oc.ObjectA == null));
                if (this.ObjectA != null)
                {
                    this.ObjectA.AssertIsEqualObjectA(oc.ObjectA);
                }

                Assert.IsTrue((this.ObjectAN == null) == (oc.ObjectAN == null));
                if (this.ObjectAN != null)
                {
                    this.ObjectAN.AssertIsEqualObjectA_Nullable(oc.ObjectAN);
                }

                Assert.IsTrue((this.DoubleArray == null) == (oc.DoubleArray == null));
                if (this.DoubleArray != null)
                {
                    Assert.IsTrue(Equatable.AreEqual((object)this.DoubleArray, oc.DoubleArray));
                }

                Assert.IsTrue((this.ObjectAArray == null) == (oc.ObjectAArray == null));
                if (this.ObjectAArray != null)
                {
                    for (var n = 0; n < this.ObjectAArray.Length; ++n)
                    {
                        Assert.IsTrue((this.ObjectAArray[n] == null) == (oc.ObjectAArray[n] == null));
                        if (this.ObjectAArray[n] != null)
                        {
                            this.ObjectAArray[n].AssertIsEqualObjectA(oc.ObjectAArray[n]);
                        }
                    }
                }

                Assert.IsTrue((this.ObjectANArray == null) == (oc.ObjectANArray == null));
                if (this.ObjectAArray != null)
                {
                    for (var n = 0; n < this.ObjectANArray.Length; ++n)
                    {
                        Assert.IsTrue((this.ObjectANArray[n] == null) == (oc.ObjectANArray[n] == null));
                        if (this.ObjectANArray[n] != null)
                        {
                            this.ObjectANArray[n].AssertIsEqualObjectA_Nullable(oc.ObjectANArray[n]);
                        }
                    }
                }

                Assert.IsTrue((this.ObjectAArray2 == null) == (oc.ObjectAArray2 == null));
                if (this.ObjectAArray2 != null)
                {
                    for (var n = 0; n < 2; ++n)
                    {
                        for (var m = 0; m < 2; ++m)
                        {
                            Assert.IsTrue((this.ObjectAArray2[n, m] == null) == (oc.ObjectAArray2[n, m] == null));
                            if (this.ObjectAArray2[n, m] != null)
                            {
                                this.ObjectAArray2[n, m].AssertIsEqualObjectA(oc.ObjectAArray2[n, m]);
                            }
                        }
                    }
                }

                Assert.IsTrue((this.ObjectAArray22 == null) == (oc.ObjectAArray22 == null));
                if (this.ObjectAArray22 != null)
                {
                    for (var n = 0; n < 2; ++n)
                    {
                        for (var m = 0; m < 2; ++m)
                        {
                            Assert.IsTrue((this.ObjectAArray22[n][m] == null) == (oc.ObjectAArray22[n][m] == null));
                            if (this.ObjectAArray22[n][m] != null)
                            {
                                this.ObjectAArray22[n][m].AssertIsEqualObjectA(oc.ObjectAArray22[n][m]);
                            }
                        }
                    }
                }
            }

            public ObjectC Randomize()
            {
                this.Boolean = (Rng.Instance.Next() % 2) == 0;
                this.Int = Rng.Instance.Next(int.MaxValue) - int.MaxValue / 2;
                this.Guid = Guid.NewGuid();
                this.ObjectA = new ObjectA().Randomize();
                this.ObjectAN = new ObjectA_Nullable().Randomize();
                this.DoubleArray = new double[5];
                for (var n = 0; n < this.DoubleArray.Length; ++n)
                {
                    this.DoubleArray[n] = Rng.Instance.NextDouble();
                }

                this.ObjectAArray = new ObjectA[5];
                for (var n = 0; n < this.ObjectAArray.Length; ++n)
                {
                    this.ObjectAArray[n] = new ObjectA().Randomize();
                }

                this.ObjectANArray = new ObjectA_Nullable[5];
                for (var n = 0; n < this.ObjectANArray.Length; ++n)
                {
                    this.ObjectANArray[n] = new ObjectA_Nullable().Randomize();
                }

                this.ObjectAArray2 = new ObjectA[2, 2];
                for (var n = 0; n < 2; ++n)
                {
                    for (var m = 0; m < 2; ++m)
                    {
                        this.ObjectAArray2[n, m] = new ObjectA().Randomize();
                    }
                }

                this.ObjectAArray22 = new ObjectA[2][];
                for (var n = 0; n < 2; ++n)
                {
                    this.ObjectAArray22[n] = new ObjectA[2];
                    for (var m = 0; m < 2; ++m)
                    {
                        this.ObjectAArray22[n][m] = new ObjectA().Randomize();
                    }
                }

                return this;
            }

            #endregion
        }

        internal class ObjectE
        {
            #region Public Properties

            public IfcA IfcA { get; set; }

            public IfcA[] IfcAArray { get; set; }

            public IfcB IfcB { get; set; }

            public IfcB[] IfcBArray { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectE(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectE));
                var oe = (ObjectE)o;
                Assert.IsTrue((this.IfcA == null) == (oe.IfcA == null));
                if (this.IfcA != null)
                {
                    this.IfcA.AssertIsEqualIfcA(oe.IfcA);
                }

                Assert.IsTrue((this.IfcB == null) == (oe.IfcB == null));
                if (this.IfcB != null)
                {
                    this.IfcB.AssertIsEqualIfcB(oe.IfcB);
                }

                Assert.IsTrue((this.IfcAArray == null) == (oe.IfcAArray == null));
                if (this.IfcAArray != null)
                {
                    for (var n = 0; n < this.IfcAArray.Length; ++n)
                    {
                        Assert.IsTrue((this.IfcAArray[n] == null) == (oe.IfcAArray[n] == null));
                        if (this.IfcAArray[n] != null)
                        {
                            this.IfcAArray[n].AssertIsEqualIfcA(oe.IfcAArray[n]);
                        }
                    }
                }

                Assert.IsTrue((this.IfcBArray == null) == (oe.IfcBArray == null));
                if (this.IfcBArray != null)
                {
                    for (var n = 0; n < this.IfcBArray.Length; ++n)
                    {
                        Assert.IsTrue((this.IfcBArray[n] == null) == (oe.IfcBArray[n] == null));
                        if (this.IfcBArray[n] != null)
                        {
                            this.IfcBArray[n].AssertIsEqualIfcB(oe.IfcBArray[n]);
                        }
                    }
                }
            }

            public ObjectE Randomize()
            {
                this.IfcA = new IfcAImpl().Randomize();
                this.IfcB = new IfcBImpl().Randomize();
                this.IfcAArray = new IfcA[5];
                for (var n = 0; n < this.IfcAArray.Length; ++n)
                {
                    this.IfcAArray[n] = new IfcAImpl().Randomize();
                }

                this.IfcBArray = new IfcB[5];
                for (var n = 0; n < this.IfcBArray.Length; ++n)
                {
                    this.IfcBArray[n] = new IfcBImpl().Randomize();
                }

                return this;
            }

            #endregion
        }

        internal class ObjectF
        {
            #region Public Properties

            public byte[] ByteArray { get; set; }

            public long Long { get; set; }

            public ObjectG ObjectG { get; private set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectF(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectF));
                var of = (ObjectF)o;
                Assert.IsFalse(ReferenceEquals(this.ObjectG, of.ObjectG));
                Assert.IsTrue(this.Long.Equals(of.Long));
                Assert.IsFalse(ReferenceEquals(this.ByteArray, of.ByteArray));
                Assert.IsTrue(ReferenceEquals(of.ByteArray, of.ObjectG.ByteArray));
            }

            public ObjectF Randomize(ObjectG og)
            {
                this.ObjectG = og;
                this.Long = (long)(Rng.Instance.Next(int.MaxValue) - int.MaxValue / 2) * Rng.Instance.Next(byte.MaxValue);
                this.ByteArray = og.ByteArray;
                return this;
            }

            #endregion
        }

        internal class ObjectG
        {
            #region Public Properties

            public byte[] ByteArray { get; set; }

            public double Double { get; set; }

            public ObjectF ObjectF { get; private set; }

            public ObjectF[] ObjectFArray { get; private set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectG(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectG));
                var og = (ObjectG)o;
                Assert.IsTrue(this.Double.Equals(og.Double));
                Assert.IsTrue((this.ObjectF == null) == (og.ObjectF == null));
                if (this.ObjectF != null)
                {
                    this.ObjectF.AssertIsEqualObjectF(og.ObjectF);
                    Assert.IsTrue(ReferenceEquals(og, og.ObjectF.ObjectG));
                    Assert.IsTrue(ReferenceEquals(og.ByteArray, og.ObjectF.ByteArray));
                }

                Assert.IsTrue((this.ObjectFArray == null) == (og.ObjectFArray == null));
                if (this.ObjectFArray != null)
                {
                    for (var n = 0; n < this.ObjectFArray.Length; ++n)
                    {
                        Assert.IsTrue((this.ObjectFArray[n] == null) == (og.ObjectFArray[n] == null));
                        if (this.ObjectFArray[n] != null)
                        {
                            this.ObjectFArray[n].AssertIsEqualObjectF(og.ObjectFArray[n]);
                            Assert.IsTrue(ReferenceEquals(og, og.ObjectFArray[n].ObjectG));
                            Assert.IsTrue(ReferenceEquals(og.ByteArray, og.ObjectFArray[n].ByteArray));
                        }
                    }

                    Assert.IsTrue(ReferenceEquals(this.ObjectFArray[0], this.ObjectFArray[3]));
                    Assert.IsTrue(ReferenceEquals(this.ObjectFArray[1], this.ObjectFArray[2]));
                    Assert.IsTrue(ReferenceEquals(og.ObjectFArray[0], og.ObjectFArray[3]));
                    Assert.IsTrue(ReferenceEquals(og.ObjectFArray[1], og.ObjectFArray[2]));
                }
            }

            public ObjectG Randomize()
            {
                this.Double = Rng.Instance.NextDouble();
                this.ByteArray = new byte[5];
                for (var n = 0; n < this.ByteArray.Length; ++n)
                {
                    this.ByteArray[n] = (byte)Rng.Instance.Next(byte.MaxValue);
                }

                this.ObjectF = new ObjectF().Randomize(this);
                this.ObjectFArray = new ObjectF[4];
                this.ObjectFArray[0] = new ObjectF().Randomize(this);
                this.ObjectFArray[1] = new ObjectF().Randomize(this);
                this.ObjectFArray[2] = this.ObjectFArray[1];
                this.ObjectFArray[3] = this.ObjectFArray[0];
                return this;
            }

            #endregion
        }

        internal class ObjectH
        {
            #region Public Properties

            public string String { get; set; }

            #endregion
        }

        internal class ObjectH2
        {
        }

        internal class ObjectI
        {
            #region Public Properties

            public object[] Objects { get; set; }

            #endregion
        }

        internal class ObjectJ : ObjectBaseJ
        {
            #region Public Properties

            public ObjectC C { get; set; }

            #endregion
        }

        internal class ObjectN
        {
            #region Constructors and Destructors

            public ObjectN()
            {
            }

            public ObjectN(string name)
            {
                this.Name = name;
            }

            #endregion

            #region Public Properties

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                var objectN = obj as ObjectN;
                return objectN != null && string.Equals(this.Name, objectN.Name);
            }

            public override int GetHashCode()
            {
                return this.Name != null ? this.Name.GetHashCode() : 0;
            }

            #endregion
        }

        [Serializable]
        internal class ObjectSerializableA
        {
            #region Fields

            public double X;

            public double Y;

            public double Z;

            #endregion

            #region Public Properties

            public string Name { get; set; }

            #endregion

            #region Public Methods and Operators

            public void AssertIsEqualObjectA_Nullable(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(StructA_Nullable));
                var san = (StructA_Nullable)o;
                Assert.IsTrue(NullableEquals(this.X, san.X));
                Assert.IsTrue(NullableEquals(this.Y, san.Y));
                Assert.IsTrue(NullableEquals(this.Z, san.Z));
            }

            public void AssertIsEqualObjectSerializableA(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(ObjectSerializableA));
                var sa = (ObjectSerializableA)o;
                Assert.AreEqual(this.X, sa.X);
                Assert.AreEqual(this.Y, sa.Y);
                Assert.AreEqual(this.Z, sa.Z);
                Assert.AreEqual(this.Name, sa.Name);
            }

            public void AssertIsEqualStructA(object o)
            {
                Assert.IsFalse(ReferenceEquals(this, o));
                Assert.IsNotNull(o);
                Assert.IsInstanceOfType(o, typeof(StructA));
                var sa = (StructA)o;
                Assert.AreEqual(this.X, sa.X);
                Assert.AreEqual(this.Y, sa.Y);
                Assert.AreEqual(this.Z, sa.Z);
            }

            #endregion

            #region Methods

            private static bool NullableEquals<T>(T a, T? b) where T : struct
            {
                return (b.HasValue && a.Equals(b)) || (!b.HasValue && a.Equals(default(T)));
            }

            #endregion
        }

        [Serializable]
        internal class ObjectSerializableAttributes : ObjectSerializableAttributesBase
        {
            #region Fields

            [NonSerialized]
            public int OnDeserializing;

            [NonSerialized]
            public int OnSerializing;

            #endregion

            #region Methods

            [OnDeserialized]
            internal void OnDeserializedMethod2(StreamingContext context)
            {
                Assert.IsNotNull(context);
                ++this.OnDeserialized;
            }

            [OnDeserializing]
            internal void OnDeserializingMethod(StreamingContext context)
            {
                Assert.IsNotNull(context);
                ++this.OnDeserializing;
            }

            [OnSerialized]
            internal void OnSerializedMethod2(StreamingContext context)
            {
                Assert.IsNotNull(context);
                ++this.OnSerialized;
            }

            [OnSerializing]
            internal void OnSerializingMethod(StreamingContext context)
            {
                Assert.IsNotNull(context);
                ++this.OnSerializing;
            }

            #endregion
        }

        [Serializable]
        internal class ObjectSerializableAttributesBase
        {
            #region Fields

            [NonSerialized]
            public int OnDeserialized;

            [NonSerialized]
            public int OnSerialized;

            #endregion

            #region Methods

            [OnDeserialized]
            internal void OnDeserializedMethod(StreamingContext context)
            {
                Assert.IsNotNull(context);
                ++this.OnDeserialized;
            }

            [OnSerialized]
            internal void OnSerializedMethod(StreamingContext context)
            {
                Assert.IsNotNull(context);
                ++this.OnSerialized;
            }

            #endregion
        }
    }

    /// <summary>
    /// The test serialization.
    /// </summary>
    [TestClass]
    public class TestSerialization
    {
        #region Public Methods and Operators

        /// <summary>
        /// The test anonymous object array.
        /// </summary>
        [TestMethod]
        public void TestAnonymousObjectArray()
        {
            {
                var i = new ObjectI { Objects = new object[] { new[] { new ObjectH { String = "AAA" } } } };
                var b = Serializer.ToByteArray(i);
                Assert.IsNotNull(b);
                var d = Deserializer.FromByteArray<ObjectI>(b);
                Assert.IsNotNull(d);
                Assert.IsInstanceOfType(d, typeof(ObjectI));
                Assert.IsNotNull(d.Objects);
                Assert.IsTrue(d.Objects.Length == 1);
                Assert.IsNotNull(d.Objects[0]);
                Assert.IsInstanceOfType(d.Objects[0], typeof(object[]));
                Assert.IsTrue((d.Objects[0] as object[]).Length == 1);
                Assert.IsInstanceOfType((d.Objects[0] as object[])[0], typeof(ObjectH));
                Assert.AreEqual("AAA", ((d.Objects[0] as object[])[0] as ObjectH).String);
            }

            {
                var i = new ObjectI { Objects = new object[] { new[] { new ObjectH2() } } };
                var b = Serializer.ToByteArray(i);
                Assert.IsNotNull(b);
                var d = Deserializer.FromByteArray<ObjectI>(b);
                Assert.IsNotNull(d);
                Assert.IsInstanceOfType(d, typeof(ObjectI));
                Assert.IsNotNull(d.Objects);
                Assert.IsTrue(d.Objects.Length == 1);
                Assert.IsNotNull(d.Objects[0]);
                Assert.IsInstanceOfType(d.Objects[0], typeof(object[]));
                Assert.IsTrue((d.Objects[0] as object[]).Length == 1);
                Assert.IsInstanceOfType((d.Objects[0] as object[])[0], typeof(ObjectH2));
            }

            {
                var a = new[] { "a", "b", "c", "d" };
                var b = Serializer.ToByteArray(a);
                Assert.IsNotNull(b);
                var d = Deserializer.FromByteArray<object[]>(b);
                Assert.IsNotNull(d);
                Assert.IsInstanceOfType(d, typeof(object[]));
                Assert.IsTrue(d.Length == 4);
                Assert.AreEqual(d[0], "a");
                Assert.AreEqual(d[1], "b");
                Assert.AreEqual(d[2], "c");
                Assert.AreEqual(d[3], "d");
            }

            {
                var a = new[] { "a", "b", "c", "d" };
                var b = Serializer.ToByteArray(a);
                Assert.IsNotNull(b);
                var d = Deserializer.FromByteArray<string[]>(b);
                Assert.IsNotNull(d);
                Assert.IsInstanceOfType(d, typeof(string[]));
                Assert.IsTrue(d.Length == 4);
                Assert.AreEqual(d[0], "a");
                Assert.AreEqual(d[1], "b");
                Assert.AreEqual(d[2], "c");
                Assert.AreEqual(d[3], "d");
            }

            {
                var a = new object[] { "a", "b", 7, 77.77 };
                var b = Serializer.ToByteArray(a);
                Assert.IsNotNull(b);
                var d = Deserializer.FromByteArray<object[]>(b);
                Assert.IsNotNull(d);
                Assert.IsInstanceOfType(d, typeof(object[]));
                Assert.IsTrue(d.Length == 4);
                Assert.AreEqual(d[0], "a");
                Assert.AreEqual(d[1], "b");
                Assert.AreEqual(d[2], 7);
                Assert.AreEqual(d[3], 77.77);
            }

            {
                var a = new object[] { "a", "b", 7, 77.77 };
                var b = Serializer.ToByteArray(a);
                Assert.IsNotNull(b);
                var d = Deserializer.FromByteArray<string[]>(b);
                Assert.IsNotNull(d);
                Assert.IsInstanceOfType(d, typeof(string[]));
                Assert.IsTrue(d.Length == 4);
                Assert.AreEqual(d[0], "a");
                Assert.AreEqual(d[1], "b");
                Assert.AreEqual(d[2], "7");
                Assert.AreEqual(d[3], "77.77");
            }
        }

        /// <summary>
        /// The test custom types.
        /// </summary>
        [TestMethod]
        public void TestCustomTypes()
        {
            Encoder.Register(
                0, 
                typeof(CustomTypeA), 
                (serializer, any) =>
                    {
                        var binaryWriter = serializer.BinaryWriter;
                        var cta = (CustomTypeA)any;
                        Encoder.WriteDouble(binaryWriter, cta.X, false);
                        Encoder.WriteInt(binaryWriter, cta.Y, false);
                        Encoder.WriteLong(binaryWriter, cta.Z, false);
                    }, 
                (deserializer) =>
                    {
                        var binaryReader = deserializer.BinaryReader;
                        return new CustomTypeA { X = Decoder.ReadDouble(binaryReader), Y = Decoder.ReadInt(binaryReader), Z = Decoder.ReadLong(binaryReader) };
                    });
            {
                var sA = new CustomTypeA { X = 1.9, Y = 234, Z = 35354634635L };
                var b = Serializer.ToByteArray(sA);
                Assert.IsNotNull(b);
                sA.AssertIsEqualCustomTypeA(Deserializer.FromByteArray<CustomTypeA>(b));
            }

            {
                var av = new[] { new CustomTypeA { X = 1.9, Y = 234545, Z = 3545764535L }, new CustomTypeA { X = 2.9, Y = 5345, Z = 457535 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<CustomTypeA[]>(b);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }

            {
                var av = new List<CustomTypeA> { new CustomTypeA { X = 1.9, Y = 234545, Z = 3545764535L }, new CustomTypeA { X = 2.9, Y = 5345, Z = 457535 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<List<CustomTypeA>>(b);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }
        }

        /// <summary>
        /// The test date time.
        /// </summary>
        [TestMethod]
        public void TestDateTime()
        {
            var dt = new DateTime();
            var b = Serializer.ToByteArray(dt);
            Assert.IsNotNull(b);
            var dtr = Deserializer.FromByteArray<DateTime>(b);
            Assert.IsTrue(dtr == dt && dtr.Kind == dt.Kind);

            dt = DateTime.Now;
            b = Serializer.ToByteArray(dt);
            Assert.IsNotNull(b);
            dtr = Deserializer.FromByteArray<DateTime>(b);
            Assert.IsTrue(dtr == dt && dtr.Kind == dt.Kind);

            dt = DateTime.UtcNow;
            b = Serializer.ToByteArray(dt);
            Assert.IsNotNull(b);
            dtr = Deserializer.FromByteArray<DateTime>(b);
            Assert.IsTrue(dtr == dt && dtr.Kind == dt.Kind);

            dt = DateTime.MaxValue;
            b = Serializer.ToByteArray(dt);
            Assert.IsNotNull(b);
            dtr = Deserializer.FromByteArray<DateTime>(b);
            Assert.IsTrue(dtr == dt && dtr.Kind == dt.Kind);

            dt = DateTime.MinValue;
            b = Serializer.ToByteArray(dt);
            Assert.IsNotNull(b);
            dtr = Deserializer.FromByteArray<DateTime>(b);
            Assert.IsTrue(dtr == dt && dtr.Kind == dt.Kind);

            dt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
            b = Serializer.ToByteArray(dt);
            Assert.IsNotNull(b);
            dtr = Deserializer.FromByteArray<DateTime>(b);
            Assert.IsTrue(dtr == dt && dtr.Kind == dt.Kind);
        }

        /// <summary>
        /// The test date time offset.
        /// </summary>
        [TestMethod]
        public void TestDateTimeOffset()
        {
            var dto = new DateTimeOffset();
            var b = Serializer.ToByteArray(dto);
            Assert.IsNotNull(b);
            var dtor = Deserializer.FromByteArray<DateTimeOffset>(b);
            Assert.IsTrue(dtor == dto && dtor.Offset == dto.Offset);

            dto = DateTimeOffset.Now;
            b = Serializer.ToByteArray(dto);
            Assert.IsNotNull(b);
            dtor = Deserializer.FromByteArray<DateTimeOffset>(b);
            Assert.IsTrue(dtor == dto && dtor.Offset == dto.Offset);

            dto = DateTimeOffset.UtcNow;
            b = Serializer.ToByteArray(dto);
            Assert.IsNotNull(b);
            dtor = Deserializer.FromByteArray<DateTimeOffset>(b);
            Assert.IsTrue(dtor == dto && dtor.Offset == dto.Offset);

            dto = DateTimeOffset.MaxValue;
            b = Serializer.ToByteArray(dto);
            Assert.IsNotNull(b);
            dtor = Deserializer.FromByteArray<DateTimeOffset>(b);
            Assert.IsTrue(dtor == dto && dtor.Offset == dto.Offset);

            dto = DateTimeOffset.MinValue;
            b = Serializer.ToByteArray(dto);
            Assert.IsNotNull(b);
            dtor = Deserializer.FromByteArray<DateTimeOffset>(b);
            Assert.IsTrue(dtor == dto && dtor.Offset == dto.Offset);

            dto = new DateTimeOffset(DateTime.Now.Ticks, TimeSpan.FromHours(4));
            b = Serializer.ToByteArray(dto);
            Assert.IsNotNull(b);
            dtor = Deserializer.FromByteArray<DateTimeOffset>(b);
            Assert.IsTrue(dtor == dto && dtor.Offset == dto.Offset);
        }

        /// <summary>
        /// The test dictionary.
        /// </summary>
        [TestMethod]
        public void TestDictionary()
        {
            {
                var dv = new Dictionary<string, string>();
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<string, string>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
                var idr = Deserializer.FromByteArray<IDictionary<string, string>>(b);
                Assert.IsNotNull(idr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, idr));
            }

            {
                var dv = new Dictionary<string, string> { { "A", "Aa" }, { "B", "Bb" }, { "C", "Cc" } };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<string, string>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
                var idr = Deserializer.FromByteArray<IDictionary<string, string>>(b);
                Assert.IsNotNull(idr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, idr));
                var cdr = Deserializer.FromByteArray<ConcurrentDictionary<string, string>>(b);
                Assert.IsNotNull(cdr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, cdr));
            }

            {
                var dv = new Dictionary<string, string> { { "A", "11" }, { "B", "22" }, { "C", "33" } };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<string, int>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual(dv, dr));
            }

            {
                var dv = new Dictionary<string, byte> { { "A", 1 }, { "B", 2 }, { "C", 3 } };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<string, byte>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
            }

            {
                var dv = new Dictionary<byte, string> { { 1, "11" }, { 2, "22" }, { 3, "33" } };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<byte, string>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
            }

            {
                var dv = new Dictionary<byte, byte> { { 1, 11 }, { 2, 22 }, { 3, 33 } };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<byte, byte>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
            }

            {
                var dv = new Dictionary<string, string[]> { { "A", new[] { "11", "111" } }, { "B", new[] { "22", "222" } }, { "C", new[] { "33", "333" } } };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<string, string[]>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
            }

            {
                var dv = new Dictionary<ObjectN, byte> { { new ObjectN("A"), 11 }, { new ObjectN("B"), 22 }, { new ObjectN("C"), 33 } };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<ObjectN, byte>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
            }

            {
                var dv = new Dictionary<byte, ObjectN> { { 11, new ObjectN("A") }, { 22, new ObjectN("B") }, { 33, new ObjectN("C") } };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<byte, ObjectN>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
            }

            {
                var dv = new Dictionary<ObjectN, string[]>
                    {
                       { new ObjectN("A"), new[] { "11", "111" } }, { new ObjectN("B"), new[] { "22", "222" } }, { new ObjectN("C"), new[] { "33", "333" } } 
                    };
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<ObjectN, string[]>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
            }

            {
                var dv = new ConcurrentDictionary<uint, ObjectN>();
                dv.TryAdd(11, new ObjectN("A"));
                dv.TryAdd(22, new ObjectN("B"));
                dv.TryAdd(33, new ObjectN("C"));
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<Dictionary<uint, ObjectN>>(b);
                Assert.IsNotNull(dr);
                Assert.IsTrue(Equatable.AreEqual((object)dv, dr));
            }

            {
                var dv = new Dictionary<string, string>();
                var b = Serializer.ToByteArray(dv);
                Assert.IsNotNull(b);
                var dr = Deserializer.FromByteArray<object>(b);
                Assert.IsNotNull(dr);
                Assert.IsInstanceOfType(dr, typeof(Dictionary<string, string>));
                Assert.IsTrue(Equatable.AreEqual(dv, dr));
            }
        }

        /// <summary>
        /// The test enum.
        /// </summary>
        [TestMethod]
        public void TestEnum()
        {
            var b = Serializer.ToByteArray(Numbers.One);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Numbers>(b) == Numbers.One);

            b = Serializer.ToByteArray(Numbers.Two);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Numbers>(b) == Numbers.Two);

            b = Serializer.ToByteArray(Numbers.Three);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Numbers>(b) == Numbers.Three);
        }

        /// <summary>
        /// The test enumerable.
        /// </summary>
        [TestMethod]
        public void TestEnumerable()
        {
            {
                var lv = new List<byte>();
                var b = Serializer.ToByteArray<IEnumerable>(lv.Distinct());
                Assert.IsNotNull(b);
                var alr = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsNotNull(alr);
                Assert.IsTrue(Equatable.AreEqual(lv, alr));
                var ilvr = Deserializer.FromByteArray<IList>(b);
                Assert.IsNotNull(ilvr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var lvr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(lvr);
                Assert.IsTrue(Equatable.AreEqual((object)lv, lvr));
                var ilvgr = Deserializer.FromByteArray<IList<byte>>(b);
                Assert.IsNotNull(ilvgr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)avr, lv));

                var isvgr = Deserializer.FromByteArray<ISet<byte>>(b);
                Assert.IsNotNull(isvgr);
                var hsvgr = Deserializer.FromByteArray<HashSet<byte>>(b);
                Assert.IsNotNull(hsvgr);
                var ssvgr = Deserializer.FromByteArray<SortedSet<byte>>(b);
                Assert.IsNotNull(ssvgr);
            }

            {
                var lv = new List<byte>(new byte[] { 1, 2, 3 });
                var b = Serializer.ToByteArray(lv.Distinct());
                Assert.IsNotNull(b);
                var alr = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsNotNull(alr);
                Assert.IsTrue(Equatable.AreEqual(lv, alr));
                var ilvr = Deserializer.FromByteArray<IList>(b);
                Assert.IsNotNull(ilvr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var lvr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(lvr);
                Assert.IsTrue(Equatable.AreEqual((object)lv, lvr));
                var ilvgr = Deserializer.FromByteArray<IList<byte>>(b);
                Assert.IsNotNull(ilvgr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)avr, lv));

                var isvgr = Deserializer.FromByteArray<ISet<byte>>(b);
                Assert.IsNotNull(isvgr);
                var hsvgr = Deserializer.FromByteArray<HashSet<byte>>(b);
                Assert.IsNotNull(hsvgr);
                var ssvgr = Deserializer.FromByteArray<SortedSet<byte>>(b);
                Assert.IsNotNull(ssvgr);
            }
        }

        /// <summary>
        /// The test exception.
        /// </summary>
        /// <exception cref="Exception">
        /// </exception>
        [TestMethod]
        public void TestException()
        {
            var e = new Exception("Exception");
            try
            {
                throw e;
            }
            catch (Exception ec)
            {
                var b = Serializer.ToByteArray(ec);
                Assert.IsNotNull(b);
                var eb = Deserializer.FromByteArray<Exception>(b);

                Assert.AreEqual(e.Message, eb.Message);
                Assert.AreEqual(ec.StackTrace, eb.StackTrace);
            }
        }

        /// <summary>
        /// The test interfaces.
        /// </summary>
        [TestMethod]
        public void TestInterfaces()
        {
            IfcA iA = new IfcAImpl();
            var b = Serializer.ToByteArray(iA);
            Assert.IsNotNull(b);
            iA.AssertIsEqualIfcA(Deserializer.FromByteArray<IfcA>(b));

            iA.Randomize();
            b = Serializer.ToByteArray(iA);
            Assert.IsNotNull(b);
            iA.AssertIsEqualIfcA(Deserializer.FromByteArray<IfcA>(b));
            iA.AssertIsEqualIfcA(Deserializer.FromByteArray<IfcAImplOther>(b));

            IfcB iB = new IfcBImpl();
            b = Serializer.ToByteArray<IfcA>(iB);
            Assert.IsNotNull(b);
            iB.AssertIsEqualIfcB(Deserializer.FromByteArray<IfcB>(b));

            iB.Randomize();
            b = Serializer.ToByteArray(iB);
            Assert.IsNotNull(b);
            iB.AssertIsEqualIfcB(Deserializer.FromByteArray<IfcB>(b));
            iB.AssertIsEqualIfcA(Deserializer.FromByteArray<IfcA>(b));
            iB.AssertIsEqualIfcB(Deserializer.FromByteArray<IfcBImplOther>(b));

            var oC = new ObjectE();
            b = Serializer.ToByteArray(oC);
            Assert.IsNotNull(b);
            oC.AssertIsEqualObjectE(Deserializer.FromByteArray<ObjectE>(b));

            oC.Randomize();
            b = Serializer.ToByteArray(oC);
            Assert.IsNotNull(b);
            oC.AssertIsEqualObjectE(Deserializer.FromByteArray<ObjectE>(b));
        }

        /// <summary>
        /// The test key value pair.
        /// </summary>
        [TestMethod]
        public void TestKeyValuePair()
        {
            var kvp = new KeyValuePair<int, bool>();
            var b = Serializer.ToByteArray(kvp);
            Assert.IsNotNull(b);
            kvp = Deserializer.FromByteArray<KeyValuePair<int, bool>>(b);
            Assert.IsTrue(kvp.Key == 0);
            Assert.IsFalse(kvp.Value);

            kvp = new KeyValuePair<int, bool>(31, true);
            b = Serializer.ToByteArray(kvp);
            Assert.IsNotNull(b);
            kvp = Deserializer.FromByteArray<KeyValuePair<int, bool>>(b);
            Assert.IsTrue(kvp.Key == 31);
            Assert.IsTrue(kvp.Value);

            var oA = new ObjectA();
            var kvp2 = new KeyValuePair<int, ObjectA>(31, oA);
            b = Serializer.ToByteArray(kvp2);
            Assert.IsNotNull(b);
            kvp2 = Deserializer.FromByteArray<KeyValuePair<int, ObjectA>>(b);
            Assert.IsTrue(kvp2.Key == 31);
            oA.AssertIsEqualObjectA(kvp2.Value);

            IfcA iA = new IfcAImpl();
            IfcB iB = new IfcBImpl();
            var kvp3 = new KeyValuePair<IfcA, IfcB>(iA, iB);
            b = Serializer.ToByteArray(kvp3);
            Assert.IsNotNull(b);
            kvp3 = Deserializer.FromByteArray<KeyValuePair<IfcA, IfcB>>(b);
            iA.AssertIsEqualIfcA(kvp3.Key);
            iB.AssertIsEqualIfcB(kvp3.Value);

            var kvp4 = new KeyValuePair<object, IfcB>(iA, iB);
            b = Serializer.ToByteArray(kvp3);
            Assert.IsNotNull(b);
            kvp4 = Deserializer.FromByteArray<KeyValuePair<object, IfcB>>(b);
            iA.AssertIsEqualIfcA(kvp4.Key);
            iB.AssertIsEqualIfcB(kvp4.Value);
        }

        /// <summary>
        /// The test list.
        /// </summary>
        [TestMethod]
        public void TestList()
        {
            {
                var lv = new ArrayList();
                var b = Serializer.ToByteArray(lv);
                Assert.IsNotNull(b);
                var alr = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsNotNull(alr);
                Assert.IsTrue(Equatable.AreEqual(lv, alr));
                var ilvr = Deserializer.FromByteArray<IList>(b);
                Assert.IsNotNull(ilvr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var lvr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(lvr);
                Assert.IsTrue(Equatable.AreEqual(lv, lvr));
                var ilvgr = Deserializer.FromByteArray<IList<uint>>(b);
                Assert.IsNotNull(ilvgr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(avr, lv));
            }

            {
                var lv = new ArrayList(new byte[] { 1, 2, 3 });
                var b = Serializer.ToByteArray(lv);
                Assert.IsNotNull(b);
                var alr = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsNotNull(alr);
                Assert.IsTrue(Equatable.AreEqual(lv, alr));
                var ilvr = Deserializer.FromByteArray<IList>(b);
                Assert.IsNotNull(ilvr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var lvr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(lvr);
                Assert.IsTrue(Equatable.AreEqual(lv, lvr));
                var ilvgr = Deserializer.FromByteArray<IList<uint>>(b);
                Assert.IsNotNull(ilvgr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(avr, lv));
            }

            {
                IList lv = new ArrayList(new byte[] { 1, 2, 3 });
                var b = Serializer.ToByteArray(lv);
                Assert.IsNotNull(b);
                var alr = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsNotNull(alr);
                Assert.IsTrue(Equatable.AreEqual(lv, alr));
                var ilvr = Deserializer.FromByteArray<IList>(b);
                Assert.IsNotNull(ilvr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var lvr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(lvr);
                Assert.IsTrue(Equatable.AreEqual(lv, lvr));
                var ilvgr = Deserializer.FromByteArray<IList<uint>>(b);
                Assert.IsNotNull(ilvgr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(avr, lv));
            }

            {
                var lv = new List<byte>(new byte[] { 1, 2, 3 });
                var b = Serializer.ToByteArray(lv);
                Assert.IsNotNull(b);
                var alr = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsNotNull(alr);
                Assert.IsTrue(Equatable.AreEqual(lv, alr));
                var ilvr = Deserializer.FromByteArray<IList>(b);
                Assert.IsNotNull(ilvr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var lvr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(lvr);
                Assert.IsTrue(Equatable.AreEqual((object)lv, lvr));
                var ilvgr = Deserializer.FromByteArray<IList<uint>>(b);
                Assert.IsNotNull(ilvgr);
                Assert.IsTrue(Equatable.AreEqual(lv, ilvr));
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)avr, lv));
            }

            {
                IList<byte> lv = new List<byte>(new byte[] { 1, 2, 3 });
                var b = Serializer.ToByteArray(lv);
                Assert.IsNotNull(b);
                var alr = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsNotNull(alr);
                Assert.IsTrue(Equatable.AreEqual(alr, lv));
                var ilvr = Deserializer.FromByteArray<IList>(b);
                Assert.IsNotNull(ilvr);
                Assert.IsTrue(Equatable.AreEqual(ilvr, lv));
                var lvr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(lvr);
                Assert.IsTrue(Equatable.AreEqual((object)lv, lvr));
                var ilvgr = Deserializer.FromByteArray<IList<byte>>(b);
                Assert.IsNotNull(ilvgr);
                Assert.IsTrue(Equatable.AreEqual((object)lv, ilvgr));
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)lv, avr));

                var isvgr = Deserializer.FromByteArray<ISet<byte>>(b);
                Assert.IsNotNull(isvgr);
                var hsvgr = Deserializer.FromByteArray<HashSet<byte>>(b);
                Assert.IsNotNull(hsvgr);
                var ssvgr = Deserializer.FromByteArray<SortedSet<byte>>(b);
                Assert.IsNotNull(ssvgr);
            }

            {
                ISet<byte> lv = new SortedSet<byte>(new byte[] { 1, 2, 3 });
                var b = Serializer.ToByteArray(lv);
                Assert.IsNotNull(b);
                var isvgr = Deserializer.FromByteArray<ISet<byte>>(b);
                Assert.IsNotNull(isvgr);
                Assert.IsTrue(isvgr.GetType() == typeof(SortedSet<byte>));
            }

            {
                IList<byte> lv = new List<byte>(new byte[] { 1, 2, 3 });
                var b = Serializer.ToByteArray(lv);
                Assert.IsNotNull(b);
                var olv = Deserializer.FromByteArray<object>(b);
                Assert.IsNotNull(olv);
                Assert.IsInstanceOfType(olv, typeof(List<byte>));
                Assert.IsTrue(Equatable.AreEqual(lv, olv));
            }

            {
                ISet<byte> lv = new HashSet<byte>(new byte[] { 1, 2, 3 });
                var b = Serializer.ToByteArray(lv);
                Assert.IsNotNull(b);
                var olv = Deserializer.FromByteArray<object>(b);
                Assert.IsNotNull(olv);
                Assert.IsInstanceOfType(olv, typeof(HashSet<byte>));
                Assert.IsTrue(Equatable.AreEqual(lv, olv));
            }
        }

        /// <summary>
        /// The test objects.
        /// </summary>
        [TestMethod]
        public void TestObjects()
        {
            var oA = new ObjectA();
            var b = Serializer.ToByteArray(oA);
            Assert.IsNotNull(b);
            oA.AssertIsEqualObjectA(Deserializer.FromByteArray<ObjectA>(b));
            oA.AssertIsEqualObjectA_Nullable(Deserializer.FromByteArray<ObjectA_Nullable>(b));

            oA.Randomize();
            b = Serializer.ToByteArray(oA);
            Assert.IsNotNull(b);
            oA.AssertIsEqualObjectA(Deserializer.FromByteArray<ObjectA>(b));
            oA.AssertIsEqualObjectA_Nullable(Deserializer.FromByteArray<ObjectA_Nullable>(b));

            var oAn = new ObjectA_Nullable();
            b = Serializer.ToByteArray(oAn);
            Assert.IsNotNull(b);
            oAn.AssertIsEqualObjectA_Nullable(Deserializer.FromByteArray<ObjectA_Nullable>(b));
            oAn.AssertIsEqualObjectA(Deserializer.FromByteArray<ObjectA>(b));

            oAn.Randomize();
            b = Serializer.ToByteArray(oAn);
            Assert.IsNotNull(b);
            oAn.AssertIsEqualObjectA_Nullable(Deserializer.FromByteArray<ObjectA_Nullable>(b));
            oAn.AssertIsEqualObjectA(Deserializer.FromByteArray<ObjectA>(b));

            var oC = new ObjectC();
            b = Serializer.ToByteArray(oC);
            Assert.IsNotNull(b);
            oC.AssertIsEqualObjectC(Deserializer.FromByteArray<ObjectC>(b));

            oC.Randomize();
            b = Serializer.ToByteArray(oC);
            Assert.IsNotNull(b);
            oC.AssertIsEqualObjectC(Deserializer.FromByteArray<ObjectC>(b));

            IList<ObjectA> loA = new List<ObjectA>();
            loA.Add(new ObjectA().Randomize());
            loA.Add(new ObjectA().Randomize());
            loA.Add(new ObjectA().Randomize());
            loA.Add(new ObjectA().Randomize());
            loA.Add(new ObjectA().Randomize());

            b = Serializer.ToByteArray(loA);
            Assert.IsNotNull(b);

            var loAr = Deserializer.FromByteArray<IList<ObjectA>>(b);
            Assert.IsTrue(loA.Count == loAr.Count);
            for (var n = 0; n < loA.Count; ++n)
            {
                loA[n].AssertIsEqualObjectA(loAr[n]);
            }

            var loAr2 = Deserializer.FromByteArray<IList<ObjectA_Nullable>>(b);
            Assert.IsTrue(loA.Count == loAr2.Count);
            for (var n = 0; n < loA.Count; ++n)
            {
                loA[n].AssertIsEqualObjectA_Nullable(loAr2[n]);
            }

            var loAr3 = Deserializer.FromByteArray<ISet<ObjectA>>(b);
            Assert.IsTrue(loA.Count == loAr3.Count);

            var loAr4 = Deserializer.FromByteArray<ObjectA[]>(b);
            Assert.IsTrue(loA.Count == loAr4.Length);
            for (var n = 0; n < loA.Count; ++n)
            {
                loA[n].AssertIsEqualObjectA(loAr4[n]);
            }

            b = Serializer.ToByteArray(loA.Distinct());
            Assert.IsNotNull(b);

            var loAr5 = Deserializer.FromByteArray<List<ObjectA>>(b);
            Assert.IsTrue(loA.Count == loAr5.Count);
            for (var n = 0; n < loA.Count; ++n)
            {
                loA[n].AssertIsEqualObjectA(loAr5[n]);
            }

            var aoj = new[] { new ObjectBaseJ(), new ObjectJ() };
            b = Serializer.ToByteArray(aoj);
            Assert.IsNotNull(b);
            var a1 = Deserializer.FromByteArray<ObjectBaseJ[]>(b);
            Assert.AreEqual(2, a1.Length);
            Assert.IsInstanceOfType(a1[0], typeof(ObjectBaseJ));
            Assert.IsInstanceOfType(a1[1], typeof(ObjectJ));

            var a2 = Deserializer.FromByteArray<object[]>(b);
            Assert.AreEqual(2, a2.Length);
            Assert.IsInstanceOfType(a2[0], typeof(ObjectBaseJ));
            Assert.IsInstanceOfType(a2[1], typeof(ObjectJ));

            aoj = new[] { new ObjectBaseJ { A = new ObjectA().Randomize() }, new ObjectJ { A = new ObjectA().Randomize(), C = new ObjectC().Randomize() } };
            b = Serializer.ToByteArray(aoj);
            Assert.IsNotNull(b);
            a1 = Deserializer.FromByteArray<ObjectBaseJ[]>(b);
            Assert.AreEqual(2, a1.Length);
            Assert.IsInstanceOfType(a1[0], typeof(ObjectBaseJ));
            Assert.AreEqual((object)aoj[0].Id, a1[0].Id);
            a1[0].A.AssertIsEqualObjectA(aoj[0].A);
            Assert.IsInstanceOfType(a1[1], typeof(ObjectJ));
            Assert.AreEqual((object)aoj[1].Id, a1[1].Id);
            a1[1].A.AssertIsEqualObjectA(aoj[1].A);
            ((ObjectJ)a1[1]).C.AssertIsEqualObjectC(((ObjectJ)aoj[1]).C);

            a2 = Deserializer.FromByteArray<object[]>(b);
            Assert.AreEqual(2, a2.Length);
            Assert.IsInstanceOfType(a2[0], typeof(ObjectBaseJ));
            ((ObjectBaseJ)a2[0]).A.AssertIsEqualObjectA(aoj[0].A);
            Assert.IsInstanceOfType(a1[1], typeof(ObjectJ));
            ((ObjectJ)a2[1]).A.AssertIsEqualObjectA(aoj[1].A);
            ((ObjectJ)a2[1]).C.AssertIsEqualObjectC(((ObjectJ)aoj[1]).C);
        }

        /// <summary>
        /// The test objects cyclic references.
        /// </summary>
        [TestMethod]
        public void TestObjectsCyclicReferences()
        {
            var oG = new ObjectG();
            var b = Serializer.ToByteArray(oG);
            Assert.IsNotNull(b);
            oG.AssertIsEqualObjectG(Deserializer.FromByteArray<ObjectG>(b));

            oG.Randomize();
            b = Serializer.ToByteArray(oG);
            Assert.IsNotNull(b);
            oG.AssertIsEqualObjectG(Deserializer.FromByteArray<ObjectG>(b));
        }

        /// <summary>
        /// The test primitives.
        /// </summary>
        [TestMethod]
        public void TestPrimitives()
        {
            var b = Serializer.ToByteArray(true);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<bool>(b));

            b = Serializer.ToByteArray(false);
            Assert.IsNotNull(b);
            Assert.IsFalse(Deserializer.FromByteArray<bool>(b));

            b = Serializer.ToByteArray<byte>(0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<byte>(b) == 0);

            b = Serializer.ToByteArray<byte>(0x34);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<byte>(b) == 0x34);
            Assert.IsTrue(Deserializer.FromByteArray<sbyte>(b) == 0x34);

            b = Serializer.ToByteArray<sbyte>(0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<sbyte>(b) == 0);

            b = Serializer.ToByteArray<sbyte>(-4);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<sbyte>(b) == -4);

            b = Serializer.ToByteArray<short>(0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<short>(b) == 0);

            b = Serializer.ToByteArray<short>(-1024);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<short>(b) == -1024);

            b = Serializer.ToByteArray<ushort>(0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<ushort>(b) == 0);

            b = Serializer.ToByteArray<ushort>(1024);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<ushort>(b) == 1024);
            Assert.IsTrue(Deserializer.FromByteArray<short>(b) == 1024);

            b = Serializer.ToByteArray(0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<int>(b) == 0);

            b = Serializer.ToByteArray(-2048);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<int>(b) == -2048);

            b = Serializer.ToByteArray<uint>(0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<uint>(b) == 0);

            b = Serializer.ToByteArray<uint>(2048);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<uint>(b) == 2048);
            Assert.IsTrue(Deserializer.FromByteArray<int>(b) == 2048);

            b = Serializer.ToByteArray<long>(0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<long>(b) == 0);

            b = Serializer.ToByteArray<long>(-4096);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<long>(b) == -4096);

            b = Serializer.ToByteArray<ulong>(0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<ulong>(b) == 0);

            b = Serializer.ToByteArray<ulong>(4096);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<ulong>(b) == 4096);
            Assert.IsTrue(Deserializer.FromByteArray<long>(b) == 4096);

            //// TODO char
            b = Serializer.ToByteArray(0.0f);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<float>(b) == 0.0f);

            b = Serializer.ToByteArray(1.234234f);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<float>(b) == 1.234234f);

            b = Serializer.ToByteArray(5.0f);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<float>(b) == 5.0f);

            b = Serializer.ToByteArray(0.0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<double>(b) == 0.0);

            b = Serializer.ToByteArray(1.234234);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<double>(b) == 1.234234);

            b = Serializer.ToByteArray(5.0);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<double>(b) == 5.0);

            //// TODO decimal
            b = Serializer.ToByteArray(string.Empty);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<string>(b) == string.Empty);

            b = Serializer.ToByteArray("abcdefg");
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<string>(b) == "abcdefg");
            Assert.IsTrue(Deserializer.FromByteArray<StringBuilder>(b).ToString() == "abcdefg");

            b = Serializer.ToByteArray<string>(null);
            Assert.IsNotNull(b);
            Assert.IsNull(Deserializer.FromByteArray<string>(b));
            Assert.IsNull(Deserializer.FromByteArray<StringBuilder>(b));

            b = Serializer.ToByteArray(new StringBuilder("abcdefg"));
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<StringBuilder>(b).ToString() == "abcdefg");
            Assert.IsTrue(Deserializer.FromByteArray<string>(b) == "abcdefg");

            b = Serializer.ToByteArray<StringBuilder>(null);
            Assert.IsNotNull(b);
            Assert.IsNull(Deserializer.FromByteArray<StringBuilder>(b));
            Assert.IsNull(Deserializer.FromByteArray<string>(b));

            var guid = Guid.NewGuid();
            b = Serializer.ToByteArray(guid);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Guid>(b).Equals(guid));

            var type = typeof(string);
            b = Serializer.ToByteArray(type);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Type>(b) == type);

            type = typeof(Dictionary<int, Uri>);
            b = Serializer.ToByteArray(type);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Type>(b) == type);

            type = typeof(Assert);
            b = Serializer.ToByteArray(type);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Type>(b) == type);

            type = typeof(XmlText);
            b = Serializer.ToByteArray(type);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Type>(b) == type);

            var uri = new Uri("http://www.xyz.com/path/local?attr=50;val='abc'");
            b = Serializer.ToByteArray(uri);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<Uri>(b).Equals(uri));
        }

        /// <summary>
        /// The test primitives array.
        /// </summary>
        [TestMethod]
        public void TestPrimitivesArray()
        {
            {
                var av = new byte[0];
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }

            {
                var av = new byte[3] { 1, 2, 3 };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<byte[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }

            {
                var av = new byte[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<byte[,]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(av, avr));
            }

            {
                var av = new byte[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<short[,]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(av, avr));
            }

            {
                var av = new[] { new byte[3] { 1, 2, 3 }, new byte[3] { 4, 5, 6 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<byte[][]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
                var avr2 = Deserializer.FromByteArray<short[][]>(b);
                Assert.IsNotNull(avr2);
                Assert.IsTrue(Equatable.AreEqual(av, avr2));
            }

            {
                var av = new[,] { { 1.0f, 2000.0f, 50000.0f }, { 4.4f, 5.5f, 6.6f } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<float[,]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(av, avr));
            }

            {
                var av = new[,] { { 1.0, 2000.0, 50000.0 }, { 4.4, 5.5, 6.6 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<double[,]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(av, avr));
            }

            {
                var av = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<Guid[]>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }

            {
                var av = new byte[3] { 1, 2, 3 };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<object>(b);
                Assert.IsNotNull(avr);
                Assert.IsInstanceOfType(avr, typeof(byte[]));
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }
        }

        /// <summary>
        /// The test primitives array convert.
        /// </summary>
        [TestMethod]
        public void TestPrimitivesArrayConvert()
        {
            {
                var b = Serializer.ToByteArray(1024);
                Assert.IsNotNull(b);
                var av = Deserializer.FromByteArray<int[]>(b);
                Assert.IsTrue(av.Length == 1);
                Assert.IsTrue(av[0] == 1024);

                b = Serializer.ToByteArray<long>(122345);
                Assert.IsNotNull(b);
                av = Deserializer.FromByteArray<int[]>(b);
                Assert.IsTrue(av.Length == 1);
                Assert.IsTrue(av[0] == 122345);
            }

            {
                var b = Serializer.ToByteArray(true);
                Assert.IsNotNull(b);
                var av = Deserializer.FromByteArray<bool[]>(b);
                Assert.IsTrue(av.Length == 1);
                Assert.IsTrue(av[0]);

                b = Serializer.ToByteArray(false);
                Assert.IsNotNull(b);
                av = Deserializer.FromByteArray<bool[]>(b);
                Assert.IsTrue(av.Length == 1);
                Assert.IsFalse(av[0]);
            }
        }

        /// <summary>
        /// The test primitives array to list convert.
        /// </summary>
        [TestMethod]
        public void TestPrimitivesArrayToListConvert()
        {
            {
                var av = new byte[0];
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }

            {
                var av = new byte[3] { 1, 2, 3 };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }

            {
                var av = new byte[3] { 1, 2, 3 };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(av, avr));
            }

            {
                var av = new byte[2, 3] { { 1, 2, 3 }, { 4, 5, 6 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<List<byte>>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(av, avr));
            }

            {
                var av = new[] { new byte[3] { 1, 2, 3 }, new byte[3] { 4, 5, 6 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<List<List<byte>>>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual(av, avr));
            }

            {
                var av = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<List<Guid>>(b);
                Assert.IsNotNull(avr);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }
        }

        /// <summary>
        /// The test primitives list convert.
        /// </summary>
        [TestMethod]
        public void TestPrimitivesListConvert()
        {
            {
                var b = Serializer.ToByteArray(1024);
                Assert.IsNotNull(b);
                var av = Deserializer.FromByteArray<List<int>>(b);
                Assert.IsTrue(av.Count == 1);
                Assert.IsTrue(av[0] == 1024);

                b = Serializer.ToByteArray<long>(122345);
                Assert.IsNotNull(b);
                av = Deserializer.FromByteArray<List<int>>(b);
                Assert.IsTrue(av.Count == 1);
                Assert.IsTrue(av[0] == 122345);
            }

            {
                var b = Serializer.ToByteArray(true);
                Assert.IsNotNull(b);
                var av = Deserializer.FromByteArray<List<bool>>(b);
                Assert.IsTrue(av.Count == 1);
                Assert.IsTrue(av[0]);

                b = Serializer.ToByteArray(false);
                Assert.IsNotNull(b);
                av = Deserializer.FromByteArray<List<bool>>(b);
                Assert.IsTrue(av.Count == 1);
                Assert.IsFalse(av[0]);
            }

            {
                var b = Serializer.ToByteArray(1024);
                Assert.IsNotNull(b);
                var av = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsTrue(av.Count == 1);
                Assert.IsTrue((int)av[0] == 1024);

                b = Serializer.ToByteArray<long>(122345);
                Assert.IsNotNull(b);
                av = Deserializer.FromByteArray<ArrayList>(b);
                Assert.IsTrue(av.Count == 1);
                Assert.IsTrue((long)av[0] == 122345);
            }
        }

        /// <summary>
        /// The test primitives nullable.
        /// </summary>
        [TestMethod]
        public void TestPrimitivesNullable()
        {
            {
                bool? bv = null;
                var b = Serializer.ToByteArray(bv);
                Assert.IsNotNull(b);
                bv = Deserializer.FromByteArray<bool?>(b);
                Assert.IsFalse(bv.HasValue);

                bv = true;
                b = Serializer.ToByteArray(bv);
                Assert.IsNotNull(b);
                bv = Deserializer.FromByteArray<bool?>(b);
                Assert.IsTrue(bv.HasValue);
                Assert.IsTrue(bv.Value);

                bv = false;
                b = Serializer.ToByteArray(bv);
                Assert.IsNotNull(b);
                bv = Deserializer.FromByteArray<bool?>(b);
                Assert.IsTrue(bv.HasValue);
                Assert.IsFalse(bv.Value);
            }

            {
                byte? bv = null;
                var b = Serializer.ToByteArray(bv);
                Assert.IsNotNull(b);
                bv = Deserializer.FromByteArray<byte?>(b);
                Assert.IsFalse(bv.HasValue);

                bv = 0x34;
                b = Serializer.ToByteArray(bv);
                Assert.IsNotNull(b);
                bv = Deserializer.FromByteArray<byte?>(b);
                Assert.IsTrue(bv.HasValue);
                Assert.IsTrue(bv.Value == 0x34);
                Assert.IsTrue(Deserializer.FromByteArray<byte>(b) == 0x34);
                Assert.IsTrue(Deserializer.FromByteArray<sbyte>(b) == 0x34);
            }
        }

        /// <summary>
        /// The test serializables.
        /// </summary>
        [TestMethod]
        public void TestSerializables()
        {
            {
                var sA = new ObjectSerializableA { X = 1.9, Y = 23.4545, Z = 3535.35, Name = "abc" };
                var b = Serializer.ToByteArray(sA);
                Assert.IsNotNull(b);
                sA.AssertIsEqualObjectSerializableA(Deserializer.FromByteArray<ObjectSerializableA>(b));
                sA.AssertIsEqualObjectSerializableA(Deserializer.FromByteArray<object>(b));
                sA.AssertIsEqualStructA(Deserializer.FromByteArray<StructA>(b));
                sA.AssertIsEqualObjectA_Nullable(Deserializer.FromByteArray<StructA_Nullable>(b));
            }
        }

        /// <summary>
        /// The test serializable attributes.
        /// </summary>
        [TestMethod]
        public void TestSerializablesAttributes()
        {
            {
                var o = new ObjectSerializableAttributes();
                var b = Serializer.ToByteArray(o);
                Assert.IsNotNull(b);
                Assert.AreEqual(1, o.OnSerializing);
                Assert.AreEqual(2, o.OnSerialized);
                Assert.AreEqual(0, o.OnDeserializing);
                Assert.AreEqual(0, o.OnDeserialized);

                var r = Deserializer.FromByteArray<ObjectSerializableAttributes>(b);
                Assert.IsNotNull(r);
                Assert.AreEqual(0, r.OnSerializing);
                Assert.AreEqual(0, r.OnSerialized);
                Assert.AreEqual(1, r.OnDeserializing);
                Assert.AreEqual(2, r.OnDeserialized);
            }
        }

        /// <summary>
        /// The test structs.
        /// </summary>
        [TestMethod]
        public void TestStructs()
        {
            {
                var sA = new StructA { X = 1.9, Y = 23.4545, Z = 3535.35 };
                var b = Serializer.ToByteArray(sA);
                Assert.IsNotNull(b);
                sA.AssertIsEqualStructA(Deserializer.FromByteArray<StructA>(b));
                sA.AssertIsEqualObjectA_Nullable(Deserializer.FromByteArray<StructA_Nullable>(b));

                sA.AssertIsEqualStructA(Deserializer.FromByteArray<object>(b));
            }

            {
                var av = new[] { new StructA { X = 1.9, Y = 23.4545, Z = 3535.35 }, new StructA { X = 2.9, Y = 53.4545, Z = 7535.35 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<StructA[]>(b);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }

            {
                var av = new List<StructA> { new StructA { X = 1.9, Y = 23.4545, Z = 3535.35 }, new StructA { X = 2.9, Y = 53.4545, Z = 7535.35 } };
                var b = Serializer.ToByteArray(av);
                Assert.IsNotNull(b);
                var avr = Deserializer.FromByteArray<List<StructA>>(b);
                Assert.IsTrue(Equatable.AreEqual((object)av, avr));
            }

            {
                var sB = new StructB { X = 1.9, Y = 23.4545, Z = 3535.35 };
                var b = Serializer.ToByteArray(sB);
                Assert.IsNotNull(b);
                sB.AssertIsEqualStructB(Deserializer.FromByteArray<StructB>(b));
                sB.AssertIsEqualObjectB_Nullable(Deserializer.FromByteArray<StructB_Nullable>(b));

                sB.AssertIsEqualStructB(Deserializer.FromByteArray<object>(b));
            }

            {
                var bv = new[] { new StructB { X = 1.9, Y = 23.4545, Z = 3535.35 }, new StructB { X = 2.9, Y = 53.4545, Z = 7535.35 } };
                var b = Serializer.ToByteArray(bv);
                Assert.IsNotNull(b);
                var bvr = Deserializer.FromByteArray<StructB[]>(b);
                Assert.IsTrue(Equatable.AreEqual((object)bv, bvr));
            }

            {
                var bv = new List<StructB> { new StructB { X = 1.9, Y = 23.4545, Z = 3535.35 }, new StructB { X = 2.9, Y = 53.4545, Z = 7535.35 } };
                var b = Serializer.ToByteArray(bv);
                Assert.IsNotNull(b);
                var bvr = Deserializer.FromByteArray<List<StructB>>(b);
                Assert.IsTrue(Equatable.AreEqual((object)bv, bvr));

                var bvr2 = Deserializer.FromByteArray<IEnumerable<StructB>>(b);
                Assert.IsInstanceOfType(bvr2, typeof(List<StructB>));
                Assert.IsTrue(Equatable.AreEqual(bv, bvr2));
            }
        }

        /// <summary>
        /// The test time span.
        /// </summary>
        [TestMethod]
        public void TestTimeSpan()
        {
            var ts = new TimeSpan();
            var b = Serializer.ToByteArray(ts);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<TimeSpan>(b) == ts);

            ts = TimeSpan.FromMilliseconds(324235.2353);
            b = Serializer.ToByteArray(ts);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<TimeSpan>(b) == ts);

            ts = TimeSpan.MaxValue;
            b = Serializer.ToByteArray(ts);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<TimeSpan>(b) == ts);

            ts = TimeSpan.MinValue;
            b = Serializer.ToByteArray(ts);
            Assert.IsNotNull(b);
            Assert.IsTrue(Deserializer.FromByteArray<TimeSpan>(b) == ts);
        }

        /// <summary>
        /// The test tuple.
        /// </summary>
        [TestMethod]
        public void TestTuple()
        {
            var t1 = new Tuple<string>("Test");
            var b = Serializer.ToByteArray(t1);
            Assert.IsNotNull(b);
            t1 = Deserializer.FromByteArray<Tuple<string>>(b);
            Assert.AreEqual("Test", t1.Item1);

            var t2 = new Tuple<int, bool>(3, false);
            b = Serializer.ToByteArray(t2);
            Assert.IsNotNull(b);
            t2 = Deserializer.FromByteArray<Tuple<int, bool>>(b);
            Assert.IsTrue(t2.Item1 == 3);
            Assert.IsFalse(t2.Item2);

            t2 = new Tuple<int, bool>(31, true);
            b = Serializer.ToByteArray(t2);
            Assert.IsNotNull(b);
            t2 = Deserializer.FromByteArray<Tuple<int, bool>>(b);
            Assert.IsTrue(t2.Item1 == 31);
            Assert.IsTrue(t2.Item2);

            var kvp = Deserializer.FromByteArray<KeyValuePair<int, bool>>(b);
            Assert.IsTrue(kvp.Key == 31);
            Assert.IsTrue(kvp.Value);

            t2 = new Tuple<int, bool>(31, true);
            b = Serializer.ToByteArray(t2);
            Assert.IsNotNull(b);
            t2 = Deserializer.FromByteArray<Tuple<int, bool>>(b);
            Assert.IsTrue(t2.Item1 == 31);
            Assert.IsTrue(t2.Item2);

            var oA = new ObjectA();
            var t3 = new Tuple<int, bool, ObjectA>(31, true, oA);
            b = Serializer.ToByteArray(t3);
            Assert.IsNotNull(b);
            t3 = Deserializer.FromByteArray<Tuple<int, bool, ObjectA>>(b);
            Assert.IsTrue(t3.Item1 == 31);
            Assert.IsTrue(t3.Item2);
            oA.AssertIsEqualObjectA(t3.Item3);
        }

        #endregion
    }
}