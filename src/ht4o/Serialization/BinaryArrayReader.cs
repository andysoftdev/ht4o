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

    internal sealed class BinaryArrayReader : BinaryReader
    {
        #region Fields

        public readonly unsafe byte* endPtr;

        public unsafe byte* ptr;

        private readonly unsafe byte* basePtr;

        private readonly byte[] buffer;

        private readonly System.Text.Decoder decoder;

        private readonly Encoding encoding;

        private readonly int offset;

        private readonly int readCharByteCount;

        private GCHandle bufferHandle;

        #endregion

        #region Constructors and Destructors

        public BinaryArrayReader(byte[] buffer)
            : this(buffer, 0)
        {
        }

        public BinaryArrayReader(byte[] buffer, int index)
            : this(buffer, index, buffer.Length - index)
        {
        }

        public BinaryArrayReader(byte[] buffer, int index, int count)
            : this(buffer, index, count, new UTF8Encoding())
        {
        }

        public unsafe BinaryArrayReader(byte[] buffer, int index, int count, Encoding encoding)
            : base(new MemoryStream(), encoding)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (buffer.Length - index < count)
            {
                throw new ArgumentException("The buffer length minus index is less than count.");
            }

            this.buffer = buffer;
            this.offset = index;

            this.encoding = encoding;
            this.decoder = encoding.GetDecoder();
            this.readCharByteCount = encoding is UnicodeEncoding ? 2 : 1;

            this.bufferHandle = GCHandle.Alloc(this.buffer, GCHandleType.Pinned);
            this.basePtr = (byte*) this.bufferHandle.AddrOfPinnedObject() + index;
            this.endPtr = this.basePtr + count;

            this.ptr = this.basePtr;
        }

        #endregion

        #region Public Properties

        public override Stream BaseStream => null;

        #endregion

        #region Public Methods and Operators

        public override unsafe int PeekChar()
        {
            var p = this.ptr;
            var ch = this.Read();
            this.ptr = p;
            return ch;
        }

        public override unsafe int Read()
        {
            var ch = default(char);

            var charsRead = 0;
            while (charsRead == 0)
            {
                var byteCount = this.Truncate(this.readCharByteCount);
                if (byteCount == 0)
                {
                    return -1;
                }

                charsRead = this.decoder.GetChars(this.ptr, byteCount, &ch, 1, false);

                this.ptr += byteCount;
            }

            return ch;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override unsafe int Read(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (buffer.Length - index < count)
            {
                throw new ArgumentException("The buffer length minus index is less than count.", nameof(buffer));
            }

            var byteCount = this.Truncate(count);
            if (byteCount > 0)
            {
                fixed (byte* p = &buffer[index])
                {
                    memcpy(p, this.ptr, new UIntPtr((uint) byteCount));
                }

                this.ptr += byteCount;
            }

            return byteCount;
        }

        public override unsafe bool ReadBoolean()
        {
            this.ThrowIfEndOfStream(1);
            return *this.ptr++ != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override unsafe byte ReadByte()
        {
            this.ThrowIfEndOfStream(1);
            return *this.ptr++;
        }

        public override unsafe byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var byteCount = this.Truncate(count);
            var result = new byte[byteCount];
            if (byteCount > 0)
            {
                fixed (byte* p = &result[0])
                {
                    memcpy(p, this.ptr, new UIntPtr((uint) byteCount));
                }

                this.ptr += byteCount;
            }

            return result;
        }

        public override char ReadChar()
        {
            var value = this.Read();
            if (value == -1)
            {
                throw new EndOfStreamException();
            }

            return (char) value;
        }

        public override char[] ReadChars(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var chars = new char[count];
            var charsRead = this.Read(chars, 0, count);
            if (charsRead < count)
            {
                Array.Resize(ref chars, charsRead);
            }

            return chars;
        }

        public override unsafe decimal ReadDecimal()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(16);

                var lo = ((int) this.ptr[0]) | ((int) this.ptr[1] << 8) | ((int) this.ptr[2] << 16)
                         | ((int) this.ptr[3] << 24);
                var mid = ((int) this.ptr[4]) | ((int) this.ptr[5] << 8) | ((int) this.ptr[6] << 16)
                          | ((int) this.ptr[7] << 24);
                var hi = ((int) this.ptr[8]) | ((int) this.ptr[9] << 8) | ((int) this.ptr[10] << 16)
                         | ((int) this.ptr[11] << 24);
                var flags = ((int) this.ptr[12]) | ((int) this.ptr[13] << 8) | ((int) this.ptr[14] << 16)
                            | ((int) this.ptr[15] << 24);

                this.ptr += 16;

                try
                {
                    const int SignMask = unchecked((int) 0x80000000);
                    const int ScaleShift = 16;
                    return new Decimal(lo, mid, hi, (flags & SignMask) > 0, (byte) (flags >> ScaleShift));
                }
                catch (ArgumentException)
                {
                    // ReadDecimal cannot leak out ArgumentException
                    throw new IOException("Read decimal failed");
                }
            }
        }

        public override unsafe double ReadDouble()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(8);

                var lo = (uint) (this.ptr[0] | this.ptr[1] << 8 | this.ptr[2] << 16 | this.ptr[3] << 24);
                var hi = (uint) (this.ptr[4] | this.ptr[5] << 8 | this.ptr[6] << 16 | this.ptr[7] << 24);

                this.ptr += 8;

                var v = ((ulong) hi) << 32 | lo;
                return *((double*) &v);
            }
        }

        public override unsafe short ReadInt16()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(2);

                this.ptr += 2;
                return (short) (this.ptr[0] | this.ptr[1] << 8);
            }
        }

        public override unsafe int ReadInt32()
        {
            this.ThrowIfEndOfStream(4);

            this.ptr += 4;
            return (int) (this.ptr[0] | this.ptr[1] << 8 | this.ptr[2] << 16 | this.ptr[3] << 24);
        }

        public override unsafe long ReadInt64()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(8);

                var lo = (uint) (this.ptr[0] | this.ptr[1] << 8 | this.ptr[2] << 16 | this.ptr[3] << 24);
                var hi = (uint) (this.ptr[4] | this.ptr[5] << 8 | this.ptr[6] << 16 | this.ptr[7] << 24);

                this.ptr += 8;

                return (long) ((ulong) hi) << 32 | lo;
            }
        }

        public override unsafe sbyte ReadSByte()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(1);
                return (sbyte) *this.ptr++;
            }
        }

        public override unsafe float ReadSingle()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(4);

                var v = (uint) (this.ptr[0] | this.ptr[1] << 8 | this.ptr[2] << 16 | this.ptr[3] << 24);

                this.ptr += 4;

                return *((float*) &v);
            }
        }

        public override unsafe string ReadString()
        {
            unchecked
            {
                var byteCount = this.ReadStringLength();
                this.ThrowIfEndOfStream(byteCount);
                var index = (int) (this.ptr - this.basePtr) + this.offset;
                this.ptr += byteCount;
                return byteCount > 0 ? this.encoding.GetString(this.buffer, index, byteCount) : string.Empty;
            }
        }

        public override unsafe ushort ReadUInt16()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(2);

                this.ptr += 2;
                return (ushort) (this.ptr[0] | this.ptr[1] << 8);
            }
        }

        public override unsafe uint ReadUInt32()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(4);

                this.ptr += 4;
                return (uint) (this.ptr[0] | this.ptr[1] << 8 | this.ptr[2] << 16 | this.ptr[3] << 24);
            }
        }

        public override unsafe ulong ReadUInt64()
        {
            unchecked
            {
                this.ThrowIfEndOfStream(8);

                var lo = (uint) (this.ptr[0] | this.ptr[1] << 8 | this.ptr[2] << 16 | this.ptr[3] << 24);
                var hi = (uint) (this.ptr[4] | this.ptr[5] << 8 | this.ptr[6] << 16 | this.ptr[7] << 24);

                this.ptr += 8;

                return ((ulong) hi) << 32 | lo;
            }
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing)
        {
            this.bufferHandle.Free();
            base.Dispose(disposing);
        }

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl,
            SetLastError = false)]
        private static extern unsafe IntPtr memcpy(byte* dest, byte* src, UIntPtr count);

        private unsafe int ReadStringLength()
        {
            unchecked
            {
                var count = 0;
                var shift = 0;
                byte b;
                do
                {
                    this.ThrowIfEndOfStream(1);
                    b = *this.ptr++;
                    count |= (b & 0x7F) << shift;
                    shift += 7;
                } while ((b & 0x80) != 0);
                return count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void ThrowIfEndOfStream(int requiredBytes)
        {
            if (this.ptr + requiredBytes > this.endPtr)
            {
                throw new EndOfStreamException();
            }
        }

        private unsafe int Truncate(int requiredBytes)
        {
            unchecked
            {
                var remainigBytes = (int) (this.endPtr - this.ptr);
                if (remainigBytes < requiredBytes)
                {
                    requiredBytes = remainigBytes;
                }

                return requiredBytes;
            }
        }

        #endregion
    }
}