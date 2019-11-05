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
    using System.Buffers;
    using System.IO;
    using System.Runtime.InteropServices;

    internal sealed class WritableMemoryStream : Stream
    {
        #region Static Fields and Constants

        public static readonly int DefaultCapacity = 4 * 1024;

        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

        #endregion

        #region Fields

        private byte[] buffer;

        private GCHandle bufferHandle;

        private int position;

        private unsafe byte* ptr;

        private unsafe byte* basePtr;

        #endregion

        #region Constructors and Destructors

        public unsafe WritableMemoryStream(int capacity) {
            this.buffer = this.Rent(Math.Max(capacity, DefaultCapacity));
            this.bufferHandle = GCHandle.Alloc(this.buffer, GCHandleType.Pinned);
            this.basePtr = this.ptr = (byte*)this.bufferHandle.AddrOfPinnedObject();
        }

        public WritableMemoryStream()
            : this(DefaultCapacity) {
        }

        #endregion

        #region Properties

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => this.position;

        public override long Position {
            get {
                return this.position;
            }

            set {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Public Methods and Operators

        public override void Flush() {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            this.EnsureBuffer((int)value);
        }

        public unsafe byte[] ToArray() {
            var result = new byte[this.position];
            if (this.position > 0) {
                memcpyArrayPtr(result, this.basePtr, new UIntPtr((uint)this.position));
            }
            return result;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (offset != 0) {
                throw new NotImplementedException();
            }

            this.Write(buffer, count);
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing) {
            this.Return();

            base.Dispose(disposing);
        }

        [DllImport(
            "msvcrt.dll",
            EntryPoint = "memcpy",
            CallingConvention = CallingConvention.Cdecl,
            SetLastError = false)]
        private static extern unsafe IntPtr memcpyPtrArray(byte* dest, byte[] src, UIntPtr count);

        [DllImport(
            "msvcrt.dll",
            EntryPoint = "memcpy",
            CallingConvention = CallingConvention.Cdecl,
            SetLastError = false)]
        private static extern unsafe IntPtr memcpyArrayPtr(byte[] dest, byte* src, UIntPtr count);

        private unsafe void EnsureBuffer(int requiredCapacity) {
            var newCapacity = this.position + requiredCapacity;
            if (newCapacity > this.buffer.Length) {
                var newLength = newCapacity + DefaultCapacity;
                var newBuffer = this.Rent(newLength);
                var newBufferHandle = GCHandle.Alloc(newBuffer, GCHandleType.Pinned);
                var newPtr = (byte*)newBufferHandle.AddrOfPinnedObject();

                memcpyPtrArray(newPtr, this.buffer, new UIntPtr((uint)this.position));

                this.Return();

                this.buffer = newBuffer;
                this.bufferHandle = newBufferHandle;
                this.basePtr = newPtr;
                this.ptr = newPtr + this.position;
            }
        }

        private byte[] Rent(int length) {
            return Pool.Rent(length);
        }

        private void Return() {
            if (this.buffer != null) {
                this.bufferHandle.Free();
                Pool.Return(this.buffer);
                this.buffer = null;
            }
        }

        private unsafe void Write(byte[] buffer, int count) {
            this.EnsureBuffer(count);
            memcpyPtrArray(this.ptr, buffer, new UIntPtr((uint)count));
            this.position += count;
            this.ptr += count;
        }

        #endregion
    }
}