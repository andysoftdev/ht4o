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

    internal sealed class FakeBinaryWriter : BinaryWriter
    {
        public FakeBinaryWriter()
            :base(new MemoryStream())
        {
        }

        public override Stream BaseStream { get; } = null;

        public override void Close() { }

        public override void Flush() { }

        public override long Seek(int offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void Write(char[] chars) { }

        public override void Write(long value) { }

        [CLSCompliant(false)]
        public override void Write(uint value) { }

        public override void Write(int value) { }

        [CLSCompliant(false)]
        public override void Write(ushort value) { }

        public override void Write(short value) { }

        public override void Write(decimal value) { }

        public override void Write(double value) { }

        public override void Write(float value) { }

        public override void Write(char ch) { }

        public override void Write(string value) { }

        public override void Write(byte[] buffer) { }

        [CLSCompliant(false)]
        public override void Write(sbyte value) { }

        public override void Write(byte value) { }

        public override void Write(bool value) { }

        [CLSCompliant(false)]
        public override void Write(ulong value) { }

        public override void Write(char[] chars, int index, int count) { }

        public override void Write(byte[] buffer, int index, int count) { }

        protected override void Dispose(bool disposing) { }
    }
}