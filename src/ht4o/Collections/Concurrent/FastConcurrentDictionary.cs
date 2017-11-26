namespace Hypertable.Persistence.Collections.Concurrent
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using Hypertable.Persistence;

    /// <summary>
    /// The fast dictionary. Using "interface devirtualization" technique to optimize: Pass a struct implementing IEqualityComparer generic argument,
    /// then in most cases, the compiler and the JIT are going to generate code that is able to eliminate all virtual calls. And if there is a
    /// trivial equality comparison, that means that you can eliminate all calls and inline the whole thing inside that generic dictionary implementation.
    /// </summary>
    /// <typeparam name="TKey">The key type/</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <typeparam name="TComparer">The comparer type.</typeparam>
    /// <remarks>Originally copied and inspired from https://github.com/redknightlois/fastdictionary/blob/master/src/FastConcurrentDictionary.cs</remarks>

    internal class FastConcurrentDictionary<TKey, TValue, TComparer> : IDictionary<TKey, TValue> where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Static Fields

        private readonly FastDictionary<TKey, TValue, TComparer> inner;

        #endregion

        #region Constructors and Destructors

        public FastConcurrentDictionary(Dictionary<TKey, TValue> src) {
            this.inner = new FastDictionary<TKey, TValue, TComparer>(src);
        }

        public FastConcurrentDictionary(
            int initialBucketCount,
            IEnumerable<KeyValuePair<TKey, TValue>> src)
        {
            this.inner = new FastDictionary<TKey, TValue, TComparer>(initialBucketCount, src);
        }

        public FastConcurrentDictionary(
            IEnumerable<KeyValuePair<TKey, TValue>> src)
        {
            this.inner = new FastDictionary<TKey, TValue, TComparer>(src);
        }

        public FastConcurrentDictionary(
            ICollection<KeyValuePair<TKey, TValue>> src)
        {
            this.inner = new FastDictionary<TKey, TValue, TComparer>(src);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastConcurrentDictionary(FastConcurrentDictionary<TKey, TValue, TComparer> src)
        {
            this.inner = new FastDictionary<TKey, TValue, TComparer>(src);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastConcurrentDictionary(
            int initialBucketCount,
            FastConcurrentDictionary<TKey, TValue, TComparer> src) {
            this.inner = new FastDictionary<TKey, TValue, TComparer>(initialBucketCount, src);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastConcurrentDictionary()
        {
            this.inner = new FastDictionary<TKey, TValue, TComparer>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastConcurrentDictionary(int initialBucketCount) {
            this.inner = new FastDictionary<TKey, TValue, TComparer>(initialBucketCount);
        }

        #endregion

        #region Public Properties

        public TValue this[TKey key] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                lock (this.inner) {
                    return this.inner[key];
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                lock (this.inner) {
                    this.inner[key] = value;
                }
            }
        }

        public int Capacity {
            get {
                lock (this.inner) {
                    return this.inner.Capacity;
                }
            }
        }

        public TComparer Comparer {
            get {
                lock (this.inner) {
                    return this.inner.Comparer;
                }
            }
        }

        public int Count {
            get {
                lock (this.inner) {
                    return this.inner.Count;
                }
            }
        }

        public bool IsEmpty {
            get {
                lock (this.inner) {
                    return this.inner.IsEmpty;
                }
            }
        }

        public bool IsReadOnly {
            get {
                lock (this.inner) {
                    return this.inner.IsReadOnly;
                }
            }
        }

        public ICollection<TKey> Keys {
            get {
                lock (this.inner) {
                    return this.inner.Keys.ToList();
                }
            }
        }

        public ICollection<TValue> Values {
            get {
                lock (this.inner) {
                    return this.inner.Values.ToList();
                }
            }
        }

        #endregion

        #region Public Methods and Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(TKey key, TValue value) {
            lock (this.inner) {
                this.inner.Add(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(TKey key, TValue value) {
            lock (this.inner) {
                return this.inner.TryAdd(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddOrUpdate(TKey key, TValue value) {
            lock (this.inner) {
                return this.inner.AddOrUpdate(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory) {
            lock (this.inner) {
                return this.inner.AddOrUpdate(key, addValue, updateValueFactory);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, TValue value) {
            lock (this.inner) {
                return this.inner.GetOrAdd(key, value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory) {
            lock (this.inner) {
                return this.inner.GetOrAdd(key, factory);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetOrAddValue(TKey key, out TValue value, TValue newValue) {
            lock (this.inner) {
                return this.inner.TryGetOrAddValue(key, out value, newValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(KeyValuePair<TKey, TValue> item) {
            this.Add(item.Key, item.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            lock (this.inner) {
                this.inner.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(KeyValuePair<TKey, TValue> item) {
            lock (this.inner) {
                return this.inner.Contains(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(TKey key) {
            lock (this.inner) {
                return this.inner.Contains(key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key) {
            lock (this.inner) {
                return this.inner.ContainsKey(key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsValue(TValue value) {
            lock (this.inner) {
                return this.inner.ContainsValue(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index) {
            lock (this.inner) {
                this.inner.CopyTo(array, index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            lock (this.inner) {
                return this.inner.ToList().GetEnumerator();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(KeyValuePair<TKey, TValue> item) {
            lock (this.inner) {
                return this.inner.Remove(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(TKey key) {
            lock (this.inner) {
                return this.inner.Remove(key);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRemove(TKey key, out TValue value) {
            lock (this.inner) {
                return this.inner.TryRemove(key, out value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value) {
            lock (this.inner) {
                return this.inner.TryGetValue(key, out value);
            }
        }

        #endregion
        
        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion
    }

    internal class FastConcurrentDictionary<TKey, TValue> : FastConcurrentDictionary<TKey, TValue, Collections.EqualityComparer<TKey>>
    {
        public FastConcurrentDictionary() {
        }

        public FastConcurrentDictionary(int initialBucketCount) :
            base(initialBucketCount) {
        }

        public FastConcurrentDictionary(ConcurrentDictionary<TKey, TValue> other)
            : base(other) {
        }

        public FastConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> src)
            : base(src) {
        }
    }
}