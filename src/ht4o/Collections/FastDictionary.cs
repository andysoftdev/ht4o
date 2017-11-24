namespace Hypertable.Persistence.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// The fast dictionary. Using "interface devirtualization" technique to optimize: Pass a struct implementing IEqualityComparer generic argument,
    /// then in most cases, the compiler and the JIT are going to generate code that is able to eliminate all virtual calls. And if there is a
    /// trivial equality comparison, that means that you can eliminate all calls and inline the whole thing inside that generic dictionary implementation.
    /// </summary>
    /// <typeparam name="TKey">The key type/</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <typeparam name="TComparer">The comparer type.</typeparam>
    /// <remarks>Originally copied and inspired from https://github.com/redknightlois/fastdictionary/blob/master/src/FastDictionary.cs</remarks>

    internal class FastDictionary<TKey, TValue, TComparer> : IDictionary<TKey, TValue> where TComparer : struct, IEqualityComparer<TKey>
    {
        #region Static Fields

        private const int InvalidNodePosition = -1;

        private const uint UnusedHash = 0xFFFFFFFF;

        private const uint DeletedHash = 0xFFFFFFFE;

        // TLoadFactor4 - controls hash map load. 4 means 100% load, ie. hashmap will grow
        // when number of items == capacity. Default value of 6 means it grows when
        // number of items == capacity * 3/2 (6/4). Higher load == tighter maps, but bigger
        // risk of collisions.
        private static readonly int LoadFactor = 6;

        #endregion

        #region Fields

        private readonly TComparer comparer = default(TComparer);

        // This is the initial capacity of the dictionary, we will never shrink beyond this point.
        private readonly int initialCapacity;

        private int capacity;

        private Entry[] entries;

        private int nextGrowthThreshold;

        // how many occupied buckets are marked deleted
        private int numberOfDeleted;

        // How many used buckets
        private int numberOfUsed;

        // This is the real counter of how many items are in the hash-table (regardless of buckets)
        private int size;

        #endregion

        #region Constructors and Destructors

        public FastDictionary(Dictionary<TKey, TValue> src)
            : this(src.Count * 3 / 2, src) {
        }

        public FastDictionary(
            int initialBucketCount,
            IEnumerable<KeyValuePair<TKey, TValue>> src)
            : this(initialBucketCount) {
            Contract.Requires(src != null);
            Contract.Ensures(this.capacity >= initialBucketCount);

            foreach (var item in src) {
                this[item.Key] = item.Value;
            }
        }

        public FastDictionary(
            IEnumerable<KeyValuePair<TKey, TValue>> src)
            : this(DictionaryHelper.InitialCapacity) {
            Contract.Requires(src != null);

            foreach (var item in src) {
                this[item.Key] = item.Value;
            }
        }

        public FastDictionary(
            ICollection<KeyValuePair<TKey, TValue>> src)
            : this(src.Count * 3 / 2) {
            Contract.Requires(src != null);

            foreach (var item in src) {
                this[item.Key] = item.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastDictionary(FastDictionary<TKey, TValue, TComparer> src)
            : this(src.capacity, src) {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastDictionary(
            int initialBucketCount,
            FastDictionary<TKey, TValue, TComparer> src) {
            Contract.Requires(src != null);
            Contract.Ensures(this.capacity >= initialBucketCount);
            Contract.Ensures(this.capacity >= src.capacity);

            this.initialCapacity = DictionaryHelper.NextPowerOf2(initialBucketCount);
            this.capacity = Math.Max(src.capacity, initialBucketCount);
            this.size = src.size;
            this.numberOfUsed = src.numberOfUsed;
            this.numberOfDeleted = src.numberOfDeleted;
            this.nextGrowthThreshold = src.nextGrowthThreshold;

            var newCapacity = this.capacity;

            if (ReferenceEquals(comparer, src.comparer)) {
                // Initialization through copy (very efficient) because the comparer is the same.
                this.entries = new Entry[newCapacity];
                Array.Copy(src.entries, this.entries, newCapacity);
            }
            else {
                // Initialization through rehashing because the comparer is not the same.
                var e = new Entry[newCapacity];
                BlockCopyMemoryHelper.Memset(e, new Entry(UnusedHash, default(TKey), default(TValue)));

                // Creating a temporary alias to use for rehashing.
                this.entries = src.entries;

                // This call will rewrite the aliases
                this.Rehash(e);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastDictionary()
            : this(DictionaryHelper.InitialCapacity) {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastDictionary(int initialBucketCount) {
            Contract.Ensures(this.capacity >= initialBucketCount);

            // Calculate the next power of 2.
            var newCapacity = initialBucketCount >= DictionaryHelper.MinBuckets
                                  ? initialBucketCount
                                  : DictionaryHelper.MinBuckets;
            newCapacity = DictionaryHelper.NextPowerOf2(newCapacity);

            this.initialCapacity = newCapacity;

            // Initialization
            this.entries = new Entry[newCapacity];
            BlockCopyMemoryHelper.Memset(this.entries, new Entry(UnusedHash, default(TKey), default(TValue)));

            this.capacity = newCapacity;

            this.numberOfUsed = 0;
            this.numberOfDeleted = 0;
            this.size = 0;

            this.nextGrowthThreshold = this.capacity * 4 / LoadFactor;
        }

        #endregion

        #region Public Properties

        public TValue this[TKey key] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                Contract.Requires(key != null);
                Contract.Ensures(this.numberOfUsed <= this.capacity);

                var hash = this.GetInternalHashCode(key);
                var bucket = hash % this.capacity;

                uint nHash;
                var uhash = (uint)hash;
                var numProbes = 1;
                do {
                    nHash = this.entries[bucket].Hash;
                    if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                        return this.entries[bucket].Value;
                    }

                    bucket = (bucket + numProbes) % this.capacity;
                    numProbes++;

                    Debug.Assert(numProbes < 100);
                }
                while (nHash != UnusedHash);

                throw new KeyNotFoundException();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set {
                Contract.Requires(key != null);
                Contract.Ensures(this.numberOfUsed <= this.capacity);

                this.ResizeIfNeeded();

                var hash = this.GetInternalHashCode(key);
                var bucket = hash % this.capacity;

                var uhash = (uint)hash;
                var numProbes = 1;
                do {
                    var nHash = this.entries[bucket].Hash;
                    if (nHash == UnusedHash) {
                        this.numberOfUsed++;
                        this.size++;

                        break;
                    }

                    if (nHash == DeletedHash) {
                        this.numberOfDeleted--;
                        this.size++;

                        break;
                    }

                    if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                        break;
                    }

                    bucket = (bucket + numProbes) % this.capacity;
                    numProbes++;

                    Debug.Assert(numProbes < 100);
                }
                while (true);

                this.entries[bucket].Hash = uhash;
                this.entries[bucket].Key = key;
                this.entries[bucket].Value = value;
            }
        }

        public int Capacity => this.capacity;

        public TComparer Comparer => this.comparer;

        public int Count => this.size;

        public bool IsEmpty => this.Count == 0;

        public bool IsReadOnly { get; }

        public ICollection<TKey> Keys => new KeyCollection(this);

        public ICollection<TValue> Values => new ValueCollection(this);

        #endregion

        #region Public Methods and Operators

        public void Add(TKey key, TValue value) {
            Contract.Ensures(this.numberOfUsed <= this.capacity);
            Contract.EndContractBlock();

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.ResizeIfNeeded();

            int hash = this.GetInternalHashCode(key);
            int bucket = hash % this.capacity;

            uint uhash = (uint)hash;
            int numProbes = 1;
            do {
                uint nHash = this.entries[bucket].Hash;
                if (nHash == UnusedHash) {
                    this.numberOfUsed++;
                    this.size++;

                    break;
                }

                if (nHash == DeletedHash) {
                    this.numberOfDeleted--;
                    this.size++;

                    break;
                }

                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    throw new ArgumentException("Cannot add duplicated key.", nameof(key));
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;
            }
            while (true);

            this.entries[bucket].Hash = uhash;
            this.entries[bucket].Key = key;
            this.entries[bucket].Value = value;
        }

        public bool TryAdd(TKey key, TValue value) {
            Contract.Ensures(this.numberOfUsed <= this.capacity);
            Contract.EndContractBlock();

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.ResizeIfNeeded();

            int hash = this.GetInternalHashCode(key);
            int bucket = hash % this.capacity;

            uint uhash = (uint)hash;
            int numProbes = 1;
            do {
                uint nHash = this.entries[bucket].Hash;
                if (nHash == UnusedHash) {
                    this.numberOfUsed++;
                    this.size++;

                    break;
                }

                if (nHash == DeletedHash) {
                    this.numberOfDeleted--;
                    this.size++;

                    break;
                }

                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    return false;
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;
            }
            while (true);

            this.entries[bucket].Hash = uhash;
            this.entries[bucket].Key = key;
            this.entries[bucket].Value = value;

            return true;
        }

        public bool AddOrUpdate(TKey key, TValue value) {
            Contract.Ensures(this.numberOfUsed <= this.capacity);
            Contract.EndContractBlock();

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            bool updated = false;

            this.ResizeIfNeeded();

            int hash = this.GetInternalHashCode(key);
            int bucket = hash % this.capacity;

            uint uhash = (uint)hash;
            int numProbes = 1;
            do {
                uint nHash = this.entries[bucket].Hash;
                if (nHash == UnusedHash) {
                    this.numberOfUsed++;
                    this.size++;

                    break;
                }

                if (nHash == DeletedHash) {
                    this.numberOfDeleted--;
                    this.size++;

                    break;
                }

                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    updated = true;
                    break;
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;
            }
            while (true);

            this.entries[bucket].Hash = uhash;
            this.entries[bucket].Key = key;
            this.entries[bucket].Value = value;

            return updated;
        }

        public TValue GetOrAdd(TKey key, TValue value) {
            Contract.Ensures(this.numberOfUsed <= this.capacity);
            Contract.EndContractBlock();

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.ResizeIfNeeded();

            int hash = this.GetInternalHashCode(key);
            int bucket = hash % this.capacity;

            uint uhash = (uint)hash;
            int numProbes = 1;
            do {
                uint nHash = this.entries[bucket].Hash;
                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    return this.entries[bucket].Value;
                }

                if (nHash == UnusedHash) {
                    this.numberOfUsed++;
                    this.size++;

                    break;
                }

                if (nHash == DeletedHash) {
                    this.numberOfDeleted--;
                    this.size++;

                    break;
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;
            }
            while (true);

            this.entries[bucket].Hash = uhash;
            this.entries[bucket].Key = key;
            return this.entries[bucket].Value = value;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory) {
            Contract.Ensures(this.numberOfUsed <= this.capacity);
            Contract.EndContractBlock();

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.ResizeIfNeeded();

            int hash = this.GetInternalHashCode(key);
            int bucket = hash % this.capacity;

            uint uhash = (uint)hash;
            int numProbes = 1;
            do {
                uint nHash = this.entries[bucket].Hash;
                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    return this.entries[bucket].Value;
                }

                if (nHash == UnusedHash) {
                    this.numberOfUsed++;
                    this.size++;

                    break;
                }

                if (nHash == DeletedHash) {
                    this.numberOfDeleted--;
                    this.size++;

                    break;
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;
            }
            while (true);

            this.entries[bucket].Hash = uhash;
            this.entries[bucket].Key = key;
            return this.entries[bucket].Value = factory(key);
        }

        public bool TryGetOrAddValue(TKey key, out TValue value, TValue newValue) {
            Contract.Ensures(this.numberOfUsed <= this.capacity);
            Contract.EndContractBlock();

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.ResizeIfNeeded();

            int hash = this.GetInternalHashCode(key);
            int bucket = hash % this.capacity;

            uint uhash = (uint)hash;
            int numProbes = 1;
            do {
                uint nHash = this.entries[bucket].Hash;

                if (nHash == UnusedHash) {
                    this.numberOfUsed++;
                    this.size++;

                    break;
                }

                if (nHash == DeletedHash) {
                    this.numberOfDeleted--;
                    this.size++;

                    break;
                }

                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    value = this.entries[bucket].Value;
                    return true;
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;
            }
            while (true);

            this.entries[bucket].Hash = uhash;
            this.entries[bucket].Key = key;
            this.entries[bucket].Value = value = newValue;

            return false;
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            this.Add(item.Key, item.Value);
        }

        public void Clear() {
            this.entries = new Entry[this.capacity];
            BlockCopyMemoryHelper.Memset(this.entries, new Entry(UnusedHash, default(TKey), default(TValue)));

            this.numberOfUsed = 0;
            this.numberOfDeleted = 0;
            this.size = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            TValue value;
            if (this.TryGetValue(item.Key, out value)) {
                var c = default(EqualityComparer<TValue>);
                return c.Equals(item.Value, value);
            }

            return false;
        }

        public bool Contains(TKey key) {
            Contract.Ensures(this.numberOfUsed <= this.capacity);

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            return this.Lookup(key) != InvalidNodePosition;
        }

        public bool ContainsKey(TKey key) {
            return this.Contains(key);
        }

        public bool ContainsValue(TValue value) {
            var count = this.capacity;

            if (value == null) {
                for (var i = 0; i < count; i++) {
                    if (this.entries[i].Hash < DeletedHash && this.entries[i].Value == null) {
                        return true;
                    }
                }
            }
            else {
                var c = default(EqualityComparer<TValue>);
                for (var i = 0; i < count; i++) {
                    if (this.entries[i].Hash < DeletedHash && c.Equals(this.entries[i].Value, value)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1) {
                throw new ArgumentException("Multiple dimensions array are not supporter", nameof(array));
            }

            if (index < 0 || index > array.Length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (array.Length - index < this.Count) {
                throw new ArgumentException("The array plus the offset is too small.");
            }

            var count = this.capacity;

            for (var i = 0; i < count; i++) {
                if (this.entries[i].Hash < DeletedHash) {
                    array[index++] = new KeyValuePair<TKey, TValue>(this.entries[i].Key, this.entries[i].Value);
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return new Enumerator(this);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            return this.Remove(item.Key);
        }

        public bool Remove(TKey key) {
            Contract.Ensures(this.numberOfUsed < this.capacity);

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            var bucket = this.Lookup(key);
            if (bucket == InvalidNodePosition) {
                return false;
            }

            this.SetDeleted(bucket);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue value) {
            Contract.Requires(key != null);
            Contract.Ensures(this.numberOfUsed <= this.capacity);

            int hash = this.GetInternalHashCode(key);
            int bucket = hash % this.capacity;

            uint nHash;
            var uhash = (uint)hash;
            int numProbes = 1;
            do {
                nHash = this.entries[bucket].Hash;
                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    value = this.entries[bucket].Value;
                    return true;
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;

                Debug.Assert(numProbes < 100);
            }
            while (nHash != UnusedHash);

            value = default(TValue);
            return false;
        }

        public bool TryGetValue(TKey key, out int bucket, out Entry entry) {
            Contract.Ensures(this.numberOfUsed <= this.capacity);
            Contract.EndContractBlock();

            if (key == null) {
                throw new ArgumentNullException(nameof(key));
            }

            this.ResizeIfNeeded();

            int hash = this.GetInternalHashCode(key);
            bucket = hash % this.capacity;

            uint uhash = (uint)hash;
            int numProbes = 1;
            do {
                uint nHash = this.entries[bucket].Hash;

                if (nHash == UnusedHash) {
                    this.numberOfUsed++;
                    this.size++;

                    break;
                }

                if (nHash == DeletedHash) {
                    this.numberOfDeleted--;
                    this.size++;

                    break;
                }

                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    entry = this.entries[bucket];
                    return true;
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;
            }
            while (true);

            entry.Hash = uhash;
            entry.Key = key;
            entry.Value = default(TValue);

            return false;
        }

        public void Insert(int bucket, ref Entry entry) {
            this.entries[bucket].Hash = entry.Hash;
            this.entries[bucket].Key = entry.Key;
            this.entries[bucket].Value = entry.Value;
        }

        #endregion

        #region Explicit Interface Methods

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetInternalHashCode(TKey key) {
            return this.comparer.GetHashCode(key) & 0x7FFFFFFF;
        }

        private void Grow(int newCapacity) {
            Contract.Requires(newCapacity >= this.capacity);
            Contract.Ensures((this.capacity & (this.capacity - 1)) == 0);

            var e = new Entry[newCapacity];
            BlockCopyMemoryHelper.Memset(e, new Entry(UnusedHash, default(TKey), default(TValue)));

            this.Rehash(e);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Lookup(TKey key) {
            int hash = this.GetInternalHashCode(key);
            int bucket = hash % this.capacity;

            uint nHash;
            var uhash = (uint)hash;
            int numProbes = 1;
            do {
                nHash = this.entries[bucket].Hash;
                if (nHash == uhash && this.comparer.Equals(this.entries[bucket].Key, key)) {
                    return bucket;
                }

                bucket = (bucket + numProbes) % this.capacity;
                numProbes++;

                Debug.Assert(numProbes < 100);
            }
            while (nHash != UnusedHash);

            return InvalidNodePosition;
        }

        private void Rehash(Entry[] newEntries) {
            uint c = (uint)newEntries.Length;

            var s = 0;

            for (var it = 0; it < this.entries.Length; it++) {
                uint hash = this.entries[it].Hash;
                if (hash >= DeletedHash) {
                    // No interest for the process of rehashing, we are skipping it.
                    continue;
                }

                uint bucket = hash % c;

                uint numProbes = 0;
                while (newEntries[bucket].Hash != UnusedHash) {
                    numProbes++;
                    bucket = (bucket + numProbes) % c;
                }

                newEntries[bucket].Hash = hash;
                newEntries[bucket].Key = this.entries[it].Key;
                newEntries[bucket].Value = this.entries[it].Value;

                s++;
            }

            this.capacity = newEntries.Length;
            this.size = s;
            this.entries = newEntries;

            this.numberOfUsed = s;
            this.numberOfDeleted = 0;

            this.nextGrowthThreshold = this.capacity * 4 / LoadFactor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResizeIfNeeded() {
            if (this.size >= this.nextGrowthThreshold) {
                this.Grow(this.capacity * 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetDeleted(int node) {
            Contract.Ensures(this.size <= Contract.OldValue<int>(this.size));

            if (this.entries[node].Hash < DeletedHash) {
                this.entries[node].Hash = DeletedHash;
                this.entries[node].Key = default(TKey);
                this.entries[node].Value = default(TValue);

                this.numberOfDeleted++;
                this.size--;
            }

            Contract.Assert(this.numberOfDeleted >= Contract.OldValue<int>(this.numberOfDeleted));
            Contract.Assert(this.entries[node].Hash == DeletedHash);

            if (3 * this.numberOfDeleted / 2 > this.capacity - this.numberOfUsed) {
                // We will force a rehash with the growth factor based on the current size.
                this.Shrink(Math.Max(this.initialCapacity, this.size * 2));
            }
        }

        private void Shrink(int newCapacity) {
            Contract.Requires(newCapacity > this.size);
            Contract.Ensures(this.numberOfUsed < this.capacity);

            // Calculate the next power of 2.
            newCapacity = Math.Max(DictionaryHelper.NextPowerOf2(newCapacity), this.initialCapacity);

            var e = new Entry[newCapacity];
            BlockCopyMemoryHelper.Memset(e, new Entry(UnusedHash, default(TKey), default(TValue)));

            this.Rehash(e);
        }

        #endregion

        #region Classes

        [Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly FastDictionary<TKey, TValue, TComparer> dictionary;

            private int index;

            private KeyValuePair<TKey, TValue> current;

            internal const int DictEntry = 1;

            internal const int KeyValuePair = 2;

            internal Enumerator(FastDictionary<TKey, TValue, TComparer> dictionary) {
                this.dictionary = dictionary;
                this.index = 0;
                this.current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext() {
                var count = this.dictionary.capacity;
                var entries = this.dictionary.entries;

                // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
                // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
                while (this.index < count) {
                    if (entries[this.index].Hash < DeletedHash) {
                        this.current = new KeyValuePair<TKey, TValue>(
                            entries[this.index].Key,
                            entries[this.index].Value);
                        this.index++;
                        return true;
                    }
                    this.index++;
                }

                this.index = count + 1;
                this.current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public KeyValuePair<TKey, TValue> Current => this.current;

            public void Dispose() {
            }

            object IEnumerator.Current => this.current;

            void IEnumerator.Reset() {
                this.index = 0;
                this.current = new KeyValuePair<TKey, TValue>();
            }
        }

        public sealed class KeyCollection : ICollection<TKey>
        {
            #region Fields

            private readonly FastDictionary<TKey, TValue, TComparer> dictionary;

            #endregion

            #region Constructors and Destructors

            public KeyCollection(FastDictionary<TKey, TValue, TComparer> dictionary) {
                Contract.Requires(dictionary != null);

                this.dictionary = dictionary;
            }

            #endregion

            #region Public Properties

            public int Count => this.dictionary.Count;

            #endregion

            #region Public Methods and Operators

            public void CopyTo(TKey[] array, int index) {
                if (array == null) {
                    throw new ArgumentNullException(nameof(array));
                }

                if (index < 0 || index > array.Length) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (array.Length - index < this.dictionary.Count) {
                    throw new ArgumentException("The array plus the offset is too small.", nameof(array));
                }

                int count = this.dictionary.capacity;
                var entries = this.dictionary.entries;

                for (int i = 0; i < count; i++) {
                    if (entries[i].Hash < DeletedHash) array[index++] = entries[i].Key;
                }
            }

            public KeyEnumerator GetEnumerator() {
                return new KeyEnumerator(this.dictionary);
            }

            #endregion

            #region Explicit Interface Methods

            void ICollection<TKey>.Add(TKey item) {
                throw new NotImplementedException();
            }

            void ICollection<TKey>.Clear() {
                throw new NotImplementedException();
            }

            bool ICollection<TKey>.Contains(TKey item) {
                throw new NotImplementedException();
            }

            bool ICollection<TKey>.IsReadOnly => true;

            bool ICollection<TKey>.Remove(TKey item) {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new KeyEnumerator(this.dictionary);
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() {
                return new KeyEnumerator(this.dictionary);
            }

            #endregion

            #region Classes

            [Serializable]
            public struct KeyEnumerator : IEnumerator<TKey>
            {
                private readonly FastDictionary<TKey, TValue, TComparer> dictionary;

                private int index;

                private TKey currentKey;

                internal KeyEnumerator(FastDictionary<TKey, TValue, TComparer> dictionary) {
                    this.dictionary = dictionary;
                    this.index = 0;
                    this.currentKey = default(TKey);
                }

                public void Dispose() {
                }

                public bool MoveNext() {
                    var count = this.dictionary.capacity;

                    var entries = this.dictionary.entries;
                    while (this.index < count) {
                        if (entries[this.index].Hash < DeletedHash) {
                            this.currentKey = entries[this.index].Key;
                            this.index++;
                            return true;
                        }
                        this.index++;
                    }

                    this.index = count + 1;
                    this.currentKey = default(TKey);
                    return false;
                }

                public TKey Current => this.currentKey;

                Object IEnumerator.Current => this.currentKey;

                void IEnumerator.Reset() {
                    this.index = 0;
                    this.currentKey = default(TKey);
                }
            }

            #endregion
        }

        public sealed class ValueCollection : ICollection<TValue>
        {
            #region Fields

            private readonly FastDictionary<TKey, TValue, TComparer> dictionary;

            #endregion

            #region Constructors and Destructors

            public ValueCollection(FastDictionary<TKey, TValue, TComparer> dictionary) {
                Contract.Requires(dictionary != null);

                this.dictionary = dictionary;
            }

            #endregion

            #region Public Properties

            public int Count => this.dictionary.Count;

            #endregion

            #region Public Methods and Operators

            public void CopyTo(TValue[] array, int index) {
                if (array == null) {
                    throw new ArgumentNullException(nameof(array));
                }

                if (index < 0 || index > array.Length) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (array.Length - index < this.dictionary.Count) {
                    throw new ArgumentException("The array plus the offset is too small.");
                }

                int count = this.dictionary.capacity;

                var entries = this.dictionary.entries;
                for (int i = 0; i < count; i++) {
                    if (entries[i].Hash < DeletedHash) {
                        array[index++] = entries[i].Value;
                    }
                }
            }

            public ValueEnumerator GetEnumerator() {
                return new ValueEnumerator(this.dictionary);
            }

            #endregion

            #region Explicit Interface Methods

            void ICollection<TValue>.Add(TValue item) {
                throw new NotImplementedException();
            }

            void ICollection<TValue>.Clear() {
                throw new NotImplementedException();
            }

            bool ICollection<TValue>.Contains(TValue item) {
                throw new NotImplementedException();
            }

            bool ICollection<TValue>.IsReadOnly => true;

            bool ICollection<TValue>.Remove(TValue item) {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new ValueEnumerator(this.dictionary);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() {
                return new ValueEnumerator(this.dictionary);
            }

            #endregion

            #region Classes

            [Serializable]
            public struct ValueEnumerator : IEnumerator<TValue>
            {
                private readonly FastDictionary<TKey, TValue, TComparer> dictionary;

                private int index;

                private TValue currentValue;

                internal ValueEnumerator(FastDictionary<TKey, TValue, TComparer> dictionary) {
                    this.dictionary = dictionary;
                    this.index = 0;
                    this.currentValue = default(TValue);
                }

                public void Dispose() {
                }

                public bool MoveNext() {
                    var count = this.dictionary.capacity;

                    var entries = this.dictionary.entries;
                    while (this.index < count) {
                        if (entries[this.index].Hash < DeletedHash) {
                            this.currentValue = entries[this.index].Value;
                            this.index++;
                            return true;
                        }
                        this.index++;
                    }

                    this.index = count + 1;
                    this.currentValue = default(TValue);
                    return false;
                }

                public TValue Current => this.currentValue;

                Object IEnumerator.Current => this.currentValue;

                void IEnumerator.Reset() {
                    this.index = 0;
                    this.currentValue = default(TValue);
                }
            }

            #endregion
        }

        public struct Entry
        {
            public uint Hash;

            public TKey Key;

            public TValue Value;

            public Entry(uint hash, TKey key, TValue value) {
                this.Hash = hash;
                this.Key = key;
                this.Value = value;
            }
        }

        private static class BlockCopyMemoryHelper
        {
            #region Public Methods and Operators

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Memset(Entry[] array, Entry value) {
                int block = 64, index = 0;
                var length = Math.Min(block, array.Length);

                //Fill the initial array
                while (index < length) {
                    array[index++] = value;
                }

                length = array.Length;
                while (index < length) {
                    Array.Copy(array, 0, array, index, Math.Min(block, (length - index)));
                    index += block;

                    block *= 2;
                }
            }

            #endregion
        }

        private static class DictionaryHelper
        {
            #region Static Fields

            /// <summary>
            /// Minimum size we're willing to let hashtables be.
            /// Must be a power of two, and at least 4.
            /// Note, however, that for a given hashtable, the initial size is a function of the first constructor arg, and may be > kMinBuckets.
            /// </summary>
            internal const int MinBuckets = 4;

            /// <summary>
            /// By default, if you don't specify a hashtable size at construction-time, we use this size.  Must be a power of two, and at least MinBuckets.
            /// </summary>
            internal const int InitialCapacity = 32;

            internal const int PowerOfTableSize = 2048;

            private static readonly int[] NextPowerOf2Table = new int[PowerOfTableSize];

            #endregion

            #region Constructors and Destructors

            static DictionaryHelper() {
                for (var i = 0; i <= MinBuckets; i++) {
                    NextPowerOf2Table[i] = MinBuckets;
                }

                for (var i = MinBuckets + 1; i < PowerOfTableSize; i++) {
                    NextPowerOf2Table[i] = NextPowerOf2Internal(i);
                }
            }

            #endregion

            #region Methods

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int NextPowerOf2(int v) {
                return v < PowerOfTableSize ? NextPowerOf2Table[v] : NextPowerOf2Internal(v);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int NextPowerOf2Internal(int v) {
                v--;
                v |= v >> 1;
                v |= v >> 2;
                v |= v >> 4;
                v |= v >> 8;
                v |= v >> 16;
                v++;

                return v;
            }

            #endregion
        }

        #endregion
    }

    internal sealed class FastDictionary<TKey, TValue> : FastDictionary<TKey, TValue, EqualityComparer<TKey>>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastDictionary(int initialBucketCount) :
            base(initialBucketCount) {
        }
    }
}