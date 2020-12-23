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
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class HeapBinaryWriter : BinaryWriter {
        #region Static Fields

        private const int PageSize = 4096;

        private static readonly UTF8Encoding UTF8Encoding = new UTF8Encoding(false, true);

        #endregion

        #region Fields

        private readonly int charMaxByteCount;

        private readonly System.Text.Encoder encoder;

        private readonly Encoding encoding;

        private IntPtr buffer;

        private int length;

        private unsafe byte* ptr;

        #endregion

        #region Constructors and Destructors

        public HeapBinaryWriter()
            : this(0, UTF8Encoding) {
        }

        public HeapBinaryWriter(int capacity)
            : this(capacity, UTF8Encoding) {
        }

        public HeapBinaryWriter(Encoding encoding)
            : this(0, encoding) {
        }

        public unsafe HeapBinaryWriter(int capacity, Encoding encoding) {
            this.length = (capacity / PageSize + 1) * PageSize;
#if !HT4O_SERIALIZATION
            this.buffer = Hypertable.Heap.Alloc(length);
#else
            this.buffer = Marshal.AllocHGlobal(length);
#endif
            this.ptr = (byte*)this.buffer;

            this.encoding = encoding;
            this.encoder = encoding.GetEncoder();
            this.charMaxByteCount = encoding.IsSingleByte ? 1 : encoding.GetMaxByteCount(1);
        }

        #endregion

        #region Properties

        private unsafe int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return (int)(this.ptr - (byte*)this.buffer);
            }
        }

        #endregion

        #region Public Methods and Operators

        public override void Flush() {
        }

        public unsafe byte[] ToArray() {
            var array = new byte[this.Count];

            if (array.Length > 0) {
                fixed (byte* target = &array[0]) {
                    memcpy(target, this.buffer, new UIntPtr((uint)array.Length));
                }
            }

            return array;
        }

        public override unsafe void Write(bool value) {
            this.EnsureBuffer(1);

            var p = this.ptr;
            *p = (byte)(value ? 1 : 0);

            ++this.ptr;
        }

        public override unsafe void Write(byte value) {
            this.EnsureBuffer(1);

            var p = this.ptr;
            *p = value;

            ++this.ptr;
        }

        public override unsafe void Write(sbyte value) {
            this.EnsureBuffer(1);

            var p = this.ptr;
            *p = (byte)value;

            ++this.ptr;
        }

        public override unsafe void Write(byte[] value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            var l = value.Length;

            this.EnsureBuffer(l);
            var p = this.ptr;
            memcpy(p, value, new UIntPtr((uint)l));

            this.ptr += l;
        }

        public override unsafe void Write(char ch) {
            this.EnsureBuffer(this.charMaxByteCount);

            var p = this.ptr;

            this.ptr += this.encoder.GetBytes(&ch, 1, p, this.charMaxByteCount, true);
        }

        public override unsafe void Write(char[] chars) {
            if (chars == null) {
                throw new ArgumentNullException(nameof(chars));
            }

            var length = chars.Length;
            var byteCount = this.charMaxByteCount > 1 ? this.encoding.GetByteCount(chars, 0, length) : length;
            this.EnsureBuffer(byteCount);
            fixed (char* ch = &chars[0]) {
                var p = this.ptr;
                this.ptr += this.encoder.GetBytes(ch, length, p, byteCount, true);
            }
        }

        public override unsafe void Write(char[] chars, int index, int count) {
            if (chars == null) {
                throw new ArgumentNullException(nameof(chars));
            }

            var byteCount = this.charMaxByteCount > 1 ? this.encoding.GetByteCount(chars, index, count) : count;
            this.EnsureBuffer(byteCount);
            fixed (char* ch = &chars[index]) {
                var p = this.ptr;
                this.ptr += this.encoder.GetBytes(ch, count, p, byteCount, true);
            }
        }

        public override unsafe void Write(double value) {
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

        public override unsafe void Write(decimal value) {
            this.EnsureBuffer(16);

            var p = this.ptr;

            fixed (int* bits = &Decimal.GetBits(value)[0]) {
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

        public override unsafe void Write(short value) {
            this.EnsureBuffer(2);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);

            this.ptr += 2;
        }

        public override unsafe void Write(ushort value) {
            this.EnsureBuffer(2);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);

            this.ptr += 2;
        }

        public override unsafe void Write(int value) {
            this.EnsureBuffer(4);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);
            p[2] = (byte)(value >> 16);
            p[3] = (byte)(value >> 24);

            this.ptr += 4;
        }

        public override unsafe void Write(uint value) {
            this.EnsureBuffer(4);

            var p = this.ptr;

            p[0] = (byte)value;
            p[1] = (byte)(value >> 8);
            p[2] = (byte)(value >> 16);
            p[3] = (byte)(value >> 24);

            this.ptr += 4;
        }

        public override unsafe void Write(long value) {
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

        public override unsafe void Write(ulong value) {
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

        public override unsafe void Write(float value) {
            this.EnsureBuffer(4);

            var p = this.ptr;

            var v = (uint*)&value;
            p[0] = (byte)*v;
            p[1] = (byte)(*v >> 8);
            p[2] = (byte)(*v >> 16);
            p[3] = (byte)(*v >> 24);

            this.ptr += 4;
        }

        public override unsafe void Write(string value) {
            if (value == null) {
                throw new ArgumentNullException(nameof(value));
            }

            var chars = value.ToCharArray();
            var length = chars.Length;
            var byteCount = length > 0 && this.charMaxByteCount > 1
                ? this.encoding.GetByteCount(chars, 0, length)
                : length;
            this.EnsureBuffer(byteCount + 5);
            this.WriteStringLength(byteCount);
            if (byteCount > 0) {
                fixed (char* ch = &chars[0]) {
                    var p = this.ptr;
                    this.ptr += this.encoder.GetBytes(ch, length, p, byteCount, true);
                }
            }
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing) {
            if (this.buffer != IntPtr.Zero) {
#if !HT4O_SERIALIZATION
                Hypertable.Heap.Free(this.buffer);
#else
                Marshal.FreeHGlobal(this.buffer);
#endif
                this.buffer = IntPtr.Zero;
            }

            base.Dispose(disposing);
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern unsafe IntPtr memcpy(byte* dest, byte[] src, UIntPtr count);

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern unsafe IntPtr memcpy(byte* dest, IntPtr src, UIntPtr count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void EnsureBuffer(int requiredCapacity) {
            var count = this.Count;
            if (count + requiredCapacity > this.length) {
                var pageCount = (count + requiredCapacity) / PageSize + 1;
                var size = (pageCount + pageCount / 10) * PageSize;
                this.length = pageCount < 1000 ? Math.Max(2 * this.length, size) : size;
#if !HT4O_SERIALIZATION
                this.buffer = Hypertable.Heap.ReAlloc(this.buffer, length);
#else
                this.buffer = Marshal.ReAllocHGlobal(this.buffer, new IntPtr(length));
#endif
                this.ptr = (byte*)this.buffer + count;
            }
        }

        private unsafe void WriteStringLength(int value) {
            var v = (uint)value;
            while (v >= 0x80) {
                *this.ptr++ = (byte)(v | 0x80);
                v >>= 7;
            }

            *this.ptr++ = (byte)v;
        }

        #endregion
    }
}