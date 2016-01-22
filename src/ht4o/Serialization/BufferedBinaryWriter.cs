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

namespace Hypertable.Persistence.Serialization
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal sealed class BufferedBinaryWriter : BinaryWriter
    {
        #region Static Fields

        private static readonly MemoryPagePool Pool = new MemoryPagePool();

        #endregion

        #region Fields

        private readonly unsafe byte* basePtr;

        private readonly int charMaxByteCount;

        private readonly System.Text.Encoder encoder;

        private readonly Encoding encoding;

        private readonly unsafe byte* endPtr;

        private readonly MemoryPage memoryPage;

        private unsafe byte* ptr;

        #endregion

        #region Constructors and Destructors

        public BufferedBinaryWriter(Stream output)
            : this(output, new UTF8Encoding(false, true))
        {
        }

        public unsafe BufferedBinaryWriter(Stream output, Encoding encoding)
            : base(output, encoding)
        {
            this.memoryPage = Pool.GetPage();

            this.basePtr = this.memoryPage.BasePtr;
            this.endPtr = this.memoryPage.EndPtr;

            this.ptr = this.basePtr;
            this.encoding = encoding;
            this.encoder = encoding.GetEncoder();
            this.charMaxByteCount = encoding.IsSingleByte ? 1 : encoding.GetMaxByteCount(1);
        }

        #endregion

        #region Properties

        private unsafe int Count
        {
            get
            {
                return (int)(this.ptr - this.basePtr);
            }
        }

        #endregion

        #region Public Methods and Operators

        public override void Flush()
        {
            this.WriteBuffer();
            base.Flush();
        }

        public override unsafe void Write(bool value)
        {
            this.EnsureBuffer(1);

            var p = this.ptr;
            *p = (byte)(value ? 1 : 0);

            ++this.ptr;
        }

        public override unsafe void Write(byte value)
        {
            this.EnsureBuffer(1);

            var p = this.ptr;
            *p = value;

            ++this.ptr;
        }

        public override unsafe void Write(sbyte value)
        {
            this.EnsureBuffer(1);

            var p = this.ptr;
            *p = (byte)value;

            ++this.ptr;
        }

        public override unsafe void Write(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var l = value.Length;

            if (this.EnsureBuffer(l))
            {
                var p = this.ptr;
                memcpy(p, value, new UIntPtr((uint)l));

                this.ptr += l;
            }
            else
            {
                this.Write(value, 0, l);
            }
        }

        public override unsafe void Write(char ch)
        {
            this.EnsureBuffer(this.charMaxByteCount);

            var p = this.ptr;

            this.ptr += this.encoder.GetBytes(&ch, 1, p, this.charMaxByteCount, true);
        }

        public override unsafe void Write(char[] chars)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }

            var length = chars.Length;
            var byteCount = this.charMaxByteCount > 1 ? this.encoding.GetByteCount(chars, 0, length) : length;
            if (this.EnsureBuffer(byteCount))
            {
                fixed (char* ch = &chars[0])
                {
                    var p = this.ptr;
                    this.ptr += this.encoder.GetBytes(ch, length, p, byteCount, true);
                }
            }
            else
            {
                base.Write(chars, 0, length);
            }
        }

        public override unsafe void Write(char[] chars, int index, int count)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }

            var byteCount = this.charMaxByteCount > 1 ? this.encoding.GetByteCount(chars, index, count) : count;
            if (this.EnsureBuffer(byteCount))
            {
                fixed (char* ch = &chars[index])
                {
                    var p = this.ptr;
                    this.ptr += this.encoder.GetBytes(ch, count, p, byteCount, true);
                }
            }
            else
            {
                base.Write(chars, index, count);
            }
        }

        public override unsafe void Write(double value)
        {
            this.EnsureBuffer(8);

            var p = this.ptr;

            var v = (ulong*)&value;
            p[0] = (byte)*v;
            p[1] = (byte)(*v >> 8);
            p[2] = (byte)(*v >> 16);
            p[3] = (byte)(*v >> 24);
            p[4] = (byte)(*v >> 32);
            p[5] = (byte)(*v >> 40);
            p[6] = (byte)(*v >> 48);
            p[7] = (byte)(*v >> 56);

            this.ptr += 8;
        }

        public override unsafe void Write(decimal value)
        {
            this.EnsureBuffer(16);

            var p = this.ptr;

            fixed (int* bits = &Decimal.GetBits(value)[0])
            {
                var v = bits;

                p[0] = (byte)*v;
                p[1] = (byte)(*v >> 8);
                p[2] = (byte)(*v >> 16);
                p[3] = (byte)(*v >> 24);

                ++v;

                p[4] = (byte)*v;
                p[5] = (byte)(*v >> 8);
                p[6] = (byte)(*v >> 16);
                p[7] = (byte)(*v >> 24);

                ++v;

                p[8] = (byte)*v;
                p[9] = (byte)(*v >> 8);
                p[10] = (byte)(*v >> 16);
                p[11] = (byte)(*v >> 24);

                ++v;

                p[12] = (byte)*v;
                p[13] = (byte)(*v >> 8);
                p[14] = (byte)(*v >> 16);
                p[15] = (byte)(*v >> 24);
            }

            this.ptr += 16;
        }

        public override unsafe void Write(short value)
        {
            this.EnsureBuffer(2);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);

            this.ptr += 2;
        }

        public override unsafe void Write(ushort value)
        {
            this.EnsureBuffer(2);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);

            this.ptr += 2;
        }

        public override unsafe void Write(int value)
        {
            this.EnsureBuffer(4);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);
            p[2] = (byte)(value >> 16);
            p[3] = (byte)(value >> 24);

            this.ptr += 4;
        }

        public override unsafe void Write(uint value)
        {
            this.EnsureBuffer(4);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);
            p[2] = (byte)(value >> 16);
            p[3] = (byte)(value >> 24);

            this.ptr += 4;
        }

        public override unsafe void Write(long value)
        {
            this.EnsureBuffer(8);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);
            p[2] = (byte)(value >> 16);
            p[3] = (byte)(value >> 24);
            p[4] = (byte)(value >> 32);
            p[5] = (byte)(value >> 40);
            p[6] = (byte)(value >> 48);
            p[7] = (byte)(value >> 56);

            this.ptr += 8;
        }

        public override unsafe void Write(ulong value)
        {
            this.EnsureBuffer(8);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);
            p[2] = (byte)(value >> 16);
            p[3] = (byte)(value >> 24);
            p[4] = (byte)(value >> 32);
            p[5] = (byte)(value >> 40);
            p[6] = (byte)(value >> 48);
            p[7] = (byte)(value >> 56);

            this.ptr += 8;
        }

        public override unsafe void Write(float value)
        {
            this.EnsureBuffer(4);

            var p = this.ptr;

            var v = (uint*)&value;
            p[0] = (byte)*v;
            p[1] = (byte)(*v >> 8);
            p[2] = (byte)(*v >> 16);
            p[3] = (byte)(*v >> 24);

            this.ptr += 4;
        }

        public override unsafe void Write(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var chars = value.ToCharArray();
            var length = chars.Length;
            var byteCount = length > 0 && this.charMaxByteCount > 1 ? this.encoding.GetByteCount(chars, 0, length) : length;
            if (this.EnsureBuffer(byteCount + 5))
            {
                this.WriteStringLength(byteCount);
                if (byteCount > 0)
                {
                    fixed (char* ch = &chars[0])
                    {
                        var p = this.ptr;
                        this.ptr += this.encoder.GetBytes(ch, length, p, byteCount, true);
                    }
                }
            }
            else
            {
                this.WriteStringLengthDirect(byteCount);
                base.Write(chars, 0, length);
            }
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.WriteBuffer();
            Pool.ReturnPage(this.memoryPage);
            base.Dispose(disposing);
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern unsafe IntPtr memcpy(byte* dest, byte[] src, UIntPtr count);

        private unsafe bool EnsureBuffer(int requiredCapacity)
        {
            if (this.ptr + requiredCapacity > this.endPtr)
            {
                this.WriteBuffer();
            }

            return requiredCapacity <= MemoryPage.Size;
        }

        private unsafe void WriteBuffer()
        {
            if (this.ptr > this.basePtr)
            {
                this.Write(this.memoryPage.Buffer, 0, this.Count);
                this.ptr = this.basePtr;
            }
        }

        private unsafe void WriteStringLength(int value)
        {
            var v = (uint)value;
            while (v >= 0x80)
            {
                *this.ptr++ = (byte)(v | 0x80);
                v >>= 7;
            }

            *this.ptr++ = (byte)v;
        }

        private void WriteStringLengthDirect(int value)
        {
           var v = (uint)value;
            while (v >= 0x80)
            {
                base.Write((byte)(v | 0x80));
                v >>= 7;
            }
            base.Write((byte)v);
        }

        #endregion

        private sealed class MemoryPage : IDisposable
        {
            #region Constants

            public const int Size = 512;

            #endregion

            #region Fields

            public readonly unsafe byte* BasePtr;

            public readonly byte[] Buffer = new byte[Size];

            public readonly unsafe byte* EndPtr;

            private GCHandle bufferHandle;

            #endregion

            #region Constructors and Destructors

            public unsafe MemoryPage()
            {
                this.bufferHandle = GCHandle.Alloc(this.Buffer, GCHandleType.Pinned);
                this.BasePtr = (byte*)this.bufferHandle.AddrOfPinnedObject();
                this.EndPtr = this.BasePtr + Size;
            }

            #endregion

            #region Public Methods and Operators

            public void Dispose()
            {
                this.bufferHandle.Free();
            }

            #endregion
        }

        private sealed class MemoryPagePool : IDisposable
        {
            #region Constants

            private const int Size = 16;

            #endregion

            #region Fields

            private readonly MemoryPage[] pool = new MemoryPage[Size];

            #endregion

            #region Constructors and Destructors

            public MemoryPagePool()
            {
                this.pool[0] = new MemoryPage();
            }

            #endregion

            #region Public Methods and Operators

            public void Dispose()
            {
                foreach (var page in this.pool.Where(page => page != null))
                {
                    page.Dispose();
                }
            }

            public MemoryPage GetPage()
            {
                for (var i = 0; i < this.pool.Length; i++)
                {
                    MemoryPage page;
                    if ((page = Interlocked.Exchange(ref this.pool[i], null)) != null)
                    {
                        return page;
                    }
                }

                return new MemoryPage();
            }

            public void ReturnPage(MemoryPage page)
            {
                for (var i = 0; i < this.pool.Length; i++)
                {
                    if (Interlocked.CompareExchange(ref this.pool[i], page, null) == null)
                    {
                        return;
                    }
                }

                page.Dispose();
            }

            #endregion
        }
    }
}