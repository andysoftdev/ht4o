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
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    internal sealed class WritableMemoryStream : Stream
    {
        #region Static Fields and Constants

        public static readonly int DefaultCapacity = 16 * 1024;

        private readonly List<byte[]> chunks = new List<byte[]>(32);

        private int length;

        private byte[] chunk;

        private int position;

        private static readonly ArrayPool<byte> Pool = ArrayPool<byte>.Shared;

        #endregion

        #region Constructors and Destructors

        public unsafe WritableMemoryStream(int capacity) {
            if (capacity / DefaultCapacity > this.chunks.Capacity) {
                this.chunks.Capacity = capacity / DefaultCapacity + 1;
            }

            this.AddChunk();
        }

        public WritableMemoryStream() {
            this.AddChunk();
        }

        #endregion

        #region Properties

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => this.length;

        public override long Position {
            get {
                return this.length;
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
            if (value / DefaultCapacity > this.chunks.Capacity) {
                this.chunks.Capacity = (int)(value / DefaultCapacity + 1);
            }
        }

        public byte[] ToArray() {
            var array = new byte[this.length];

            if (this.length > 0) {
                int index = 0;
                foreach (var c in chunks) {
                    var l = c.Length;
                    if (index + l > this.length) {
                        l = this.length - index;
                    }

                    Array.Copy(c, 0, array, index, l);
                    index += l;
                }
            }

            return array;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (offset != 0) {
                throw new NotImplementedException();
            }

            for (int tail = this.position + count; tail > this.chunk.Length; tail = this.position + count) {
                var fill = this.chunk.Length - this.position;

                Array.Copy(buffer, offset, this.chunk, this.position, fill);
                this.length += fill;

                this.AddChunk();

                offset += fill;
                count -= fill;
            }

            Array.Copy(buffer, offset, this.chunk, this.position, count);
            this.position += count;
            this.length += count;
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing) {
            this.chunk = null;

            foreach (var c in chunks) {
                Pool.Return(c);
            }

            base.Dispose(disposing);
        }

        private void AddChunk() {
            this.chunk = Pool.Rent(DefaultCapacity);
            this.chunks.Add(this.chunk);
            this.position = 0;
        }

        #endregion
    }
}