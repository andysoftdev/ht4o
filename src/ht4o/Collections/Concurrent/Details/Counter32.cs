// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

namespace Hypertable.Persistence.Collections.Concurrent.Details
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class Counter32
    {
        #region Constants

        private const int MAX_DRIFT = 1;

        #endregion

        #region Static Fields

        private static readonly int MAX_CELL_COUNT = Environment.ProcessorCount * 2;

        #endregion

        #region Fields

        // how many cells we have
        private int cellCount;

        // spaced out counters
        private Cell[] cells;

        // default counter
        private int cnt;

        private int lastCnt;

        // delayed estimated count
        private int lastCntTicks;

        #endregion

        #region Public Properties

        public int EstimatedValue
        {
            get
            {
                if (cells == null)
                    return cnt;

                var curTicks = Environment.TickCount;
                // more than a millisecond passed?
                if (curTicks != lastCntTicks)
                {
                    lastCnt = Value;
                    lastCntTicks = curTicks;
                }

                return lastCnt;
            }
        }

        public int Value
        {
            get
            {
                var count = cnt;
                var cells = this.cells;

                if (cells != null)
                    for (var i = 0; i < cells.Length; i++)
                    {
                        var cell = cells[i];
                        if (cell != null)
                            count += cell.counter.cnt;
                        else
                            break;
                    }

                return count;
            }
        }

        #endregion

        #region Public Methods and Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int c)
        {
            Cell cell = null;

            var curCellCount = cellCount;
            if ((curCellCount > 1) & (cells != null))
                cell = cells[GetIndex(curCellCount)];

            var drift = cell == null ? increment(ref cnt, c) : increment(ref cell.counter.cnt, c);

            if (drift > MAX_DRIFT)
                TryAddCell(curCellCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Decrement()
        {
            Cell cell = null;

            var curCellCount = cellCount;
            if ((curCellCount > 1) & (cells != null))
                cell = cells[GetIndex(curCellCount)];

            var drift = cell == null ? decrement(ref cnt) : decrement(ref cell.counter.cnt);

            if (drift > MAX_DRIFT)
                TryAddCell(curCellCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Increment()
        {
            Cell cell = null;

            var curCellCount = cellCount;
            if ((curCellCount > 1) & (cells != null))
                cell = cells[GetIndex(curCellCount)];

            var drift = cell == null ? increment(ref cnt) : increment(ref cell.counter.cnt);

            if (drift > MAX_DRIFT)
                TryAddCell(curCellCount);
        }

        #endregion

        #region Methods

        private static int decrement(ref int val)
        {
            return val - Interlocked.Decrement(ref val) - 1;
        }

        private static int GetIndex(int cellCount)
        {
            return Environment.CurrentManagedThreadId % cellCount;
        }

        private static int increment(ref int val)
        {
            return -val + Interlocked.Increment(ref val) - 1;
        }

        private static int increment(ref int val, int inc)
        {
            return -val + Interlocked.Add(ref val, inc) - inc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryAddCell(int curCellCount)
        {
            if (curCellCount < MAX_CELL_COUNT)
            {
                var cells = this.cells;
                if (cells == null)
                {
                    var newCells = new Cell[MAX_CELL_COUNT];
                    cells = Interlocked.CompareExchange(ref this.cells, newCells, null) ?? newCells;
                }

                if (cells[curCellCount] == null)
                    Interlocked.CompareExchange(ref cells[curCellCount], new Cell(), null);

                if (cellCount == curCellCount)
                    Interlocked.CompareExchange(ref cellCount, curCellCount + 1, curCellCount);
            }
        }

        #endregion

        #region Nested Types

        private class Cell
        {
            #region Fields

            public SpacedCounter counter;

            #endregion

            #region Nested Types

            [StructLayout(LayoutKind.Explicit)]
            public struct SpacedCounter
            {
                // 64 bytes - sizeof(int) - sizeof(objecHeader64)
                [FieldOffset(44)]
                public int cnt;
            }

            #endregion
        }

        #endregion
    }
}