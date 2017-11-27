// Copyright (c) Vladimir Sadov. All rights reserved.
//
// This file is distributed under the MIT License. See LICENSE.md for details.

namespace Hypertable.Persistence.Collections.Concurrent.Details
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class ConcurrentDictionary<TKey, TValue, TComparer> :
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IDictionary,
        ICollection
        where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Fields

        internal uint _lastResizeTickMillis;
        internal DictionaryImpl<TKey, TValue, TComparer> _table;

        #endregion

        #region Constructors and Destructors

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that is empty, has the default concurrency level, has the default initial capacity, and uses the default comparer
        ///     for the key type.
        /// </summary>
        public ConcurrentDictionary()
            : this(31)
        {
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <param name="capacity">
        ///     The initial number of elements that the
        ///     <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.
        /// </param>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that is empty, has the default concurrency level and uses the default comparer for the key type.
        /// </summary>
        public ConcurrentDictionary(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            if (default(TKey) == null)
            {
                if (typeof(TKey) == typeof(ValueType) ||
                    !(default(TKey) is ValueType))
                {
                    _table = DictionaryImpl<TKey, TValue, TComparer>.CreateRefUnsafe(this, capacity);
                    return;
                }
            }
            else
            {
                if (typeof(TKey) == typeof(uint) || typeof(TKey) == typeof(ulong))
                    throw new NotSupportedException(
                        "Unsupported until we have confirmation of how to by-pass the code-gen issue with the casting of Boxed<TKey>. Use int or long instead.");

                if (typeof(TKey) == typeof(int))
                {
                    _table = DictionaryImpl<TKey, TValue, TComparer>.CreateIntUnsafe(this, capacity);
                    return;
                }

                if (typeof(TKey) == typeof(long))
                {
                    _table = DictionaryImpl<TKey, TValue, TComparer>.CreateLongUnsafe(this, capacity);
                    return;
                }
            }

            _table = new DictionaryImplBoxed<TKey, TValue, TComparer>(capacity, this);
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that is empty, has the specified concurrency level and capacity, and uses the default comparer for the key type.
        /// </summary>
        /// <param name="concurrencyLevel">
        ///     The estimated number of threads that will update the
        ///     <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> concurrently.
        /// </param>
        /// <param name="capacity">
        ///     The initial number of elements that the
        ///     <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.
        /// </param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///     <paramref name="concurrencyLevel" /> is less than 1.-or-<paramref name="capacity" /> is less than 0.
        /// </exception>
        public ConcurrentDictionary(int concurrencyLevel, int capacity)
            : this(capacity)
        {
            if (concurrencyLevel < 1)
                throw new ArgumentOutOfRangeException(nameof(concurrencyLevel));
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that contains elements copied from the specified
        ///     <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" />, has the default concurrency level, has
        ///     the default initial capacity, and uses the default comparer for the key type.
        /// </summary>
        /// <param name="collection">
        ///     The <see cref="T:System.Collections.IEnumerable{KeyValuePair{TKey,TValue}}" /> whose elements
        ///     are copied to the new <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="collection" /> or any of its keys is a null reference (Nothing in Visual Basic)
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="collection" /> contains one or more duplicate keys.
        /// </exception>
        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this()
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            InitializeFromCollection(collection);
        }

        #endregion

        #region Public Properties

        public int Count => _table.Count;

        public bool IsEmpty => _table.Count == 0;

        public ReadOnlyCollection<TKey> Keys
        {
            get
            {
                var keys = new List<TKey>(Count);
                foreach (var kv in this)
                    keys.Add(kv.Key);

                return new ReadOnlyCollection<TKey>(keys);
            }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        public ReadOnlyCollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>(Count);
                foreach (var kv in this)
                    values.Add(kv.Value);

                return new ReadOnlyCollection<TValue>(values);
            }
        }

        #endregion

        #region Properties

        bool IDictionary.IsFixedSize => false;

        bool IDictionary.IsReadOnly => false;

        bool ICollection.IsSynchronized => false;

        ICollection IDictionary.Keys => Keys;

        ICollection IDictionary.Values => Values;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        #endregion

        #region Public Indexers

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var oldValObj = _table.TryGetValue(key);

                Debug.Assert(!(oldValObj is DictionaryImpl.Prime));

                if (oldValObj != null)
                {
                    // PERF: this would be nice to have as a helper, 
                    // but it does not get inlined
                    TValue value;
                    if (default(TValue) == null && oldValObj == DictionaryImpl.NULLVALUE)
                        value = default(TValue);
                    else
                        value = (TValue) oldValObj;

                    return value;
                }

                return ThrowKeyNotFound();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                object oldValObj = null;
                var newValObj = DictionaryImpl.ToObjectValue(value);
                _table.PutIfMatch(key, newValObj, ref oldValObj, DictionaryImpl.ValueMatch.Any);
            }
        }

        #endregion

        #region Indexers

        object IDictionary.this[object key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                TValue tValue;
                if (key is TKey && TryGetValue((TKey) key, out tValue))
                    return tValue;
                return null;
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));
                if (!(key is TKey))
                    throw new ArgumentException();
                if (!(value is TValue))
                    throw new ArgumentException();
                this[(TKey) key] = (TValue) value;
            }
        }

        #endregion

        #region Public Methods and Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value))
                throw new ArgumentException("AddingDuplicate");
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory,
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (addValueFactory == null)
                throw new ArgumentNullException(nameof(addValueFactory));
            if (updateValueFactory == null)
                throw new ArgumentNullException(nameof(updateValueFactory));
            TValue tValue2;
            while (true)
            {
                TValue tValue;
                if (TryGetValue(key, out tValue))
                {
                    tValue2 = updateValueFactory(key, tValue);
                    if (TryUpdate(key, tValue2, tValue))
                        break;
                }
                else
                {
                    tValue2 = addValueFactory(key);
                    if (TryAdd(key, tValue2))
                        break;
                }
            }
            return tValue2;
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (updateValueFactory == null)
                throw new ArgumentNullException(nameof(updateValueFactory));
            while (true)
            {
                TValue tValue;
                if (TryGetValue(key, out tValue))
                {
                    var tValue2 = updateValueFactory(key, tValue);
                    if (TryUpdate(key, tValue2, tValue))
                        return tValue2;
                }
                else if (TryAdd(key, addValue))
                {
                    return addValue;
                }
            }
        }

        public void Clear()
        {
            _table.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            TValue value;
            return TryGetValue(keyValuePair.Key, out value) &&
                   System.Collections.Generic.EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);
        }

        public bool ContainsKey(TKey key)
        {
            TValue value;
            return TryGetValue(key, out value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("index");

            foreach (var entry in this)
                array[arrayIndex++] = entry;
        }

        public void CopyTo(DictionaryEntry[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("array");

            foreach (var entry in this)
                array[arrayIndex++] = new DictionaryEntry(entry.Key, entry.Value);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("index");

            var length = array.Length;
            foreach (var entry in this)
                if ((uint) arrayIndex < (uint) length)
                    array[arrayIndex++] = entry;
                else
                    throw new ArgumentException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, TValue value)
        {
            object oldValObj = null;
            var newValObj = DictionaryImpl.ToObjectValue(value);
            if (_table.PutIfMatch(key, newValObj, ref oldValObj, DictionaryImpl.ValueMatch.NullOrDead))
                return value;

            // PERF: this would be nice to have as a helper, 
            // but it does not get inlined
            if (default(TValue) == null && oldValObj == DictionaryImpl.NULLVALUE)
                oldValObj = null;

            return (TValue) oldValObj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return _table.GetOrAdd(key, valueFactory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key)
        {
            object oldValObj = null;
            var found = _table.PutIfMatch(key, DictionaryImpl.TOMBSTONE, ref oldValObj,
                DictionaryImpl.ValueMatch.NotNullOrDead);
            Debug.Assert(!(oldValObj is DictionaryImpl.Prime));

            return found;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var oldValObj = DictionaryImpl.ToObjectValue(item.Value);
            return _table.PutIfMatch(item.Key, DictionaryImpl.TOMBSTONE, ref oldValObj,
                DictionaryImpl.ValueMatch.OldValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value)
        {
            object oldValObj = null;
            var newValObj = DictionaryImpl.ToObjectValue(value);
            return _table.PutIfMatch(key, newValObj, ref oldValObj, DictionaryImpl.ValueMatch.NullOrDead);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value)
        {
            var oldValObj = _table.TryGetValue(key);

            Debug.Assert(!(oldValObj is DictionaryImpl.Prime));

            if (oldValObj != null)
            {
                // PERF: this would be nice to have as a helper, 
                // but it does not get inlined
                if (default(TValue) == null && oldValObj == DictionaryImpl.NULLVALUE)
                    value = default(TValue);
                else
                    value = (TValue) oldValObj;
                return true;
            }

            value = default(TValue);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue value)
        {
            object oldValObj = null;
            var found = _table.PutIfMatch(key, DictionaryImpl.TOMBSTONE, ref oldValObj,
                DictionaryImpl.ValueMatch.NotNullOrDead);

            Debug.Assert(!(oldValObj is DictionaryImpl.Prime));
            Debug.Assert(found ^ (oldValObj == null));

            // PERF: this would be nice to have as a helper, 
            // but it does not get inlined
            if (default(TValue) == null && oldValObj == DictionaryImpl.NULLVALUE)
                oldValObj = null;

            value = found ? (TValue) oldValObj : default(TValue);

            return found;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryUpdate(TKey key, TValue value, TValue comparisonValue)
        {
            var oldValObj = DictionaryImpl.ToObjectValue(comparisonValue);
            var newValObj = DictionaryImpl.ToObjectValue(value);
            return _table.PutIfMatch(key, newValObj, ref oldValObj, DictionaryImpl.ValueMatch.OldValue);
        }

        #endregion

        #region Explicit Interface Methods

        void IDictionary.Add(object key, object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (!(key is TKey))
                throw new ArgumentException();
            TValue value2;
            try
            {
                value2 = (TValue) value;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException();
            }
            ((IDictionary<TKey, TValue>) this).Add((TKey) key, value2);
        }

        bool IDictionary.Contains(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return key is TKey && ContainsKey((TKey) key);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            var pairs = array as KeyValuePair<TKey, TValue>[];
            if (pairs != null)
            {
                CopyTo(pairs, index);
                return;
            }

            var entries = array as DictionaryEntry[];
            if (entries != null)
            {
                CopyTo(entries, index);
                return;
            }

            var objects = array as object[];
            if (objects != null)
            {
                CopyTo(objects, index);
                return;
            }

            throw new ArgumentNullException(nameof(array));
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return _table.GetDictionaryEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key is TKey)
            {
                TValue tValue;
                TryRemove((TKey) key, out tValue);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        #endregion

        #region Methods

        private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            foreach (var current in collection)
                if (!TryAdd(current.Key, current.Value))
                    throw new ArgumentException("Collection contains duplicate keys");
        }

        private TValue ThrowKeyNotFound()
        {
            throw new KeyNotFoundException();
        }

        #endregion
    }

    internal class
        ConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue, Collections.EqualityComparer<TKey>>
    {
        #region Constructors and Destructors

        public ConcurrentDictionary()
        {
        }

        // System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>
        /// <param name="capacity">
        ///     The initial number of elements that the
        ///     <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> can contain.
        /// </param>
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Collections.Concurrent.ConcurrentDictionary`2" /> class
        ///     that is empty, has the default concurrency level and uses the default comparer for the key type.
        /// </summary>
        public ConcurrentDictionary(int capacity)
            : base(capacity)
        {
        }

        public ConcurrentDictionary(int concurrencyLevel, int capacity)
            : base(concurrencyLevel, capacity)
        {
        }

        public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : base(collection)
        {
        }

        #endregion
    }
}