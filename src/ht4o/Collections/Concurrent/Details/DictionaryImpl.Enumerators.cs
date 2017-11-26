// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

namespace Hypertable.Persistence.Collections.Concurrent.Details
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal abstract partial class DictionaryImpl<TKey, TKeyStore, TValue, TComparer>
        : DictionaryImpl<TKey, TValue, TComparer>
    {
        #region Methods

        internal override IDictionaryEnumerator GetDictionaryEnumerator()
        {
            return new SnapshotIDict(this);
        }

        internal override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new SnapshotKV(this);
        }

        #endregion

        #region Nested Types

        private class Snapshot : IDisposable
        {
            #region Fields

            protected TKey _curKey, _nextK;
            protected object _curValue, _nextV;
            private readonly DictionaryImpl<TKey, TKeyStore, TValue, TComparer> _table;
            private int _idx;

            #endregion

            #region Constructors and Destructors

            public Snapshot(DictionaryImpl<TKey, TKeyStore, TValue, TComparer> dict)
            {
                _table = dict;

                // linearization point.
                // if table is quiescent and has no copy in progress,
                // we can simply iterate over its table.
                while (true)
                {
                    if (_table._newTable == null)
                        break;

                    // there is a copy in progress, finish it and try again
                    _table.HelpCopyImpl(true);
                    _table = (DictionaryImpl<TKey, TKeyStore, TValue, TComparer>) _table._topDict._table;
                }

                // Warm-up the iterator
                MoveNext();
            }

            #endregion

            #region Public Methods and Operators

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_nextV == NULLVALUE)
                    return false;

                _curKey = _nextK;
                _curValue = _nextV;
                _nextV = NULLVALUE;

                var entries = _table._entries;
                while (_idx < entries.Length)
                {
                    // Scan array
                    var nextEntry = entries[_idx++];

                    if (nextEntry.value != null)
                    {
                        var nextK = _table.keyFromEntry(nextEntry.key);

                        var nextV = _table.TryGetValue(nextK);
                        if (nextV != null)
                        {
                            _nextK = nextK;

                            // PERF: this would be nice to have as a helper, 
                            // but it does not get inlined
                            if (default(TValue) == null && nextV == NULLVALUE)
                                _nextV = default(TValue);
                            else
                                _nextV = (TValue) nextV;


                            break;
                        }
                    }
                }

                return _curValue != NULLVALUE;
            }

            public void Reset()
            {
                _idx = 0;
            }

            #endregion
        }

        private sealed class SnapshotIDict : Snapshot, IDictionaryEnumerator
        {
            #region Constructors and Destructors

            public SnapshotIDict(DictionaryImpl<TKey, TKeyStore, TValue, TComparer> dict)
                : base(dict)
            {
            }

            #endregion

            #region Public Properties

            public DictionaryEntry Entry
            {
                get
                {
                    var curValue = _curValue;
                    if (curValue == NULLVALUE)
                        throw new InvalidOperationException();

                    return new DictionaryEntry(_curKey, (TValue) curValue);
                }
            }

            public object Key => Entry.Key;

            public object Value => Entry.Value;

            #endregion

            #region Properties

            object IEnumerator.Current => Entry;

            #endregion
        }

        private sealed class SnapshotKV : Snapshot, IEnumerator<KeyValuePair<TKey, TValue>>
        {
            #region Constructors and Destructors

            public SnapshotKV(DictionaryImpl<TKey, TKeyStore, TValue, TComparer> dict)
                : base(dict)
            {
            }

            #endregion

            #region Public Properties

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    var curValue = _curValue;
                    if (curValue == NULLVALUE)
                        throw new InvalidOperationException();

                    return new KeyValuePair<TKey, TValue>(_curKey, (TValue) curValue);
                }
            }

            #endregion

            #region Properties

            object IEnumerator.Current => Current;

            #endregion
        }

        #endregion
    }
}