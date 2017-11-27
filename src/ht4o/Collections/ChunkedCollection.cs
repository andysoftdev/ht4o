namespace Hypertable.Persistence.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    ///     Represents a strongly typed collection of objects that can be accessed by index. Provides methods to manipulate
    ///     collections.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of elements in the collection.
    /// </typeparam>
    internal sealed class ChunkedCollection<T> : ICollection<T>, ICollection
    {
        #region Fields

        /// <summary>
        ///     The chunks.
        /// </summary>
        private readonly List<T[]> chunks = new List<T[]>(32);

        /// <summary>
        ///     The chunk size.
        /// </summary>
        private readonly int chunkSize = 4 * 1024;

        /// <summary>
        ///     The element count.
        /// </summary>
        private int count;

        /// <summary>
        ///     The current chunk.
        /// </summary>
        private T[] current;

        /// <summary>
        ///     The current chunk's element count.
        /// </summary>
        private int currentCount;

        /// <summary>
        ///     The synchronization object.
        /// </summary>
        private object syncRoot;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChunkedCollection{T}" /> class.
        /// </summary>
        public ChunkedCollection()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChunkedCollection{T}" /> class.
        /// </summary>
        /// <param name="chunkSize">
        ///     The chunk size.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="chunkSize" /> is less than two.
        /// </exception>
        public ChunkedCollection(int chunkSize)
        {
            if (chunkSize < 2)
            {
                throw new ArgumentException(@"Invalid chunk size", nameof(chunkSize));
            }

            this.chunkSize = chunkSize;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the number of elements actually contained in the <see cref="ChunkedCollection{T}" />.
        /// </summary>
        public int Count => this.count;

        /// <summary>
        ///     Gets a value indicating whether the <see cref="ChunkedCollection{T}" /> is read-only.
        /// </summary>
        /// <returns>
        ///     True if the <see cref="ChunkedCollection{T}" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly => false;

        /// <summary>
        ///     Gets a value indicating whether access to the <see cref="ChunkedCollection{T}" /> is synchronized (thread safe).
        /// </summary>
        /// <returns>
        ///     True if access to the <see cref="ChunkedCollection{T}" /> is synchronized (thread safe); otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool IsSynchronized => false;

        /// <summary>
        ///     Gets an object that can be used to synchronize access to the <see cref="ChunkedCollection{T}" />.
        /// </summary>
        /// <returns>
        ///     An object that can be used to synchronize access to the <see cref="ChunkedCollection{T}" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object SyncRoot
        {
            get
            {
                if (this.syncRoot == null)
                {
                    Interlocked.CompareExchange<object>(ref this.syncRoot, new object(), null);
                }

                return this.syncRoot;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="ChunkedCollection{T}" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="ChunkedCollection{T}" />.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        int ICollection.Count => this.Count;

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="ChunkedCollection{T}" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="ChunkedCollection{T}" />.
        /// </returns>
        int ICollection<T>.Count => this.Count;

        /// <summary>
        ///     Gets all the element chunks.
        /// </summary>
        internal IEnumerable<T[]> Chunks => this.chunks;

        /// <summary>
        ///     Gets the chunk size of the <see cref="ChunkedCollection{T}" />.
        /// </summary>
        internal int ChunkSize => this.chunkSize;

        #endregion

        #region Indexers

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">
        ///     The index.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Index is not a valid index in the collection.
        /// </exception>
        /// <returns>
        ///     The element at the specified index.
        /// </returns>
        internal T this[int index]
        {
            get
            {
                if (index < 0 || index >= this.count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return this.chunks[index / this.chunkSize][index % this.chunkSize];
            }

            set
            {
                if (index < 0 || index >= this.count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                this.chunks[index / this.chunkSize][index % this.chunkSize] = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Adds an item to the <see cref="ChunkedCollection{T}" />.
        /// </summary>
        /// <param name="item">
        ///     The object to add to the <see cref="ChunkedCollection{T}" />.
        /// </param>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="ChunkedCollection{T}" /> is read-only.
        /// </exception>
        public void Add(T item)
        {
            if (this.current == null || this.currentCount == this.chunkSize)
            {
                this.AddChunk();
            }

            this.current[this.currentCount++] = item;
            ++this.count;
        }

        /// <summary>
        ///     Removes all items from the <see cref="ChunkedCollection{T}" />.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        ///     The <see cref="ChunkedCollection{T}" /> is read-only.
        /// </exception>
        public void Clear()
        {
            this.chunks.Clear();
            this.current = null;
            this.currentCount = 0;
            this.count = 0;
        }

        /// <summary>
        ///     Determines whether the <see cref="ChunkedCollection{T}" /> contains a specific value.
        /// </summary>
        /// <returns>
        ///     True if <paramref name="item" /> is found in the <see cref="ChunkedCollection{T}" />; otherwise, false.
        /// </returns>
        /// <param name="item">
        ///     The object to locate in the <see cref="ChunkedCollection{T}" />.
        /// </param>
        public bool Contains(T item)
        {
            return this.chunks.Any(c => c.Contains(item));
        }

        /// <summary>
        ///     Copies the elements of the <see cref="ChunkedCollection{T}" /> to an <see cref="T:System.Array" />, starting at a
        ///     particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from
        ///     <see cref="ChunkedCollection{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        ///     The zero-based index in <paramref name="array" /> at which copying begins.
        /// </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            this.CopyTo((Array) array, arrayIndex);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="ChunkedCollection{T}" /> to an <see cref="T:System.Array" />, starting at a
        ///     particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from
        ///     <see cref="ChunkedCollection{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="index">
        ///     The zero-based index in <paramref name="array" /> at which copying begins.
        /// </param>
        public void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException(@"Array is multidimensional", nameof(array));
            }

            try
            {
                this.CopyToUnchecked(array, index);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(@"Invalid array element type", nameof(array));
            }
        }

        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        ///     Removes the first occurrence of a specific object from the <see cref="ChunkedCollection{T}" />.
        /// </summary>
        /// <returns>
        ///     True if <paramref name="item" /> was successfully removed from the <see cref="ChunkedCollection{T}" />; otherwise,
        ///     false. This method also returns false if <paramref name="item" /> is not found in the original
        ///     <see cref="ChunkedCollection{T}" />.
        /// </returns>
        /// <param name="item">
        ///     The object to remove from the <see cref="ChunkedCollection{T}" />.
        /// </param>
        /// <exception cref="T:System.NotSupportedException">
        ///     Remove is not implemented yet.
        /// </exception>
        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Copies the elements of the <see cref="ChunkedCollection{T}" /> to a new array.
        /// </summary>
        /// <returns>
        ///     An array containing copies of the elements of the <see cref="ChunkedCollection{T}" />.
        /// </returns>
        public T[] ToArray()
        {
            var array = new T[this.count];
            this.CopyToUnchecked(array, 0);
            return array;
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The add chunk.
        /// </summary>
        private void AddChunk()
        {
            var chunk = new T[this.chunkSize];
            this.chunks.Add(chunk);
            this.current = chunk;
            this.currentCount = 0;
        }

        /// <summary>
        ///     The copy to unchecked.
        /// </summary>
        /// <param name="array">
        ///     The array.
        /// </param>
        /// <param name="index">
        ///     The index.
        /// </param>
        private void CopyToUnchecked(Array array, int index)
        {
            if (this.count > 0)
            {
                var chunk = 0;
                var remaining = this.count % this.chunkSize;
                var chunkCount = remaining > 0 ? this.chunks.Count - 1 : this.chunks.Count;
                while (chunk < chunkCount)
                {
                    Array.Copy(this.chunks[chunk++], 0, array, index, this.chunkSize);
                    index += this.chunkSize;
                }

                if (remaining > 0)
                {
                    Array.Copy(this.chunks[chunk], 0, array, index, remaining);
                }
            }
        }

        #endregion

        #region Nested Types

        /// <summary>
        ///     The enumerator.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            #region Fields

            /// <summary>
            ///     The chunked collection.
            /// </summary>
            private readonly ChunkedCollection<T> chunkedCollection;

            /// <summary>
            ///     The count.
            /// </summary>
            private readonly int count;

            /// <summary>
            ///     The chunk index.
            /// </summary>
            private int chunkIndex;

            /// <summary>
            ///     The current.
            /// </summary>
            private T current;

            /// <summary>
            ///     The current chunk.
            /// </summary>
            private T[] currentChunk;

            /// <summary>
            ///     The index.
            /// </summary>
            private int index;

            /// <summary>
            ///     The local chunk index.
            /// </summary>
            private int localChunkIndex;

            #endregion

            #region Constructors and Destructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="Enumerator" /> struct.
            /// </summary>
            /// <param name="chunkedCollection">
            ///     The chunked collection.
            /// </param>
            internal Enumerator(ChunkedCollection<T> chunkedCollection)
            {
                this.chunkedCollection = chunkedCollection;
                this.count = chunkedCollection.count;
                this.index = 0;
                this.localChunkIndex = 0;
                this.chunkIndex = 0;
                this.currentChunk = null;
                this.current = default(T);
            }

            #endregion

            #region Public Properties

            /// <summary>
            ///     Gets Current.
            /// </summary>
            public T Current => this.current;

            #endregion

            #region Explicit Interface Properties

            /// <summary>
            ///     Gets the current item.
            /// </summary>
            /// <value>
            ///     The current item.
            /// </value>
            object IEnumerator.Current => this.Current;

            #endregion

            #region Public Methods and Operators

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
            }

            /// <summary>
            ///     Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            ///     True if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of
            ///     the collection.
            /// </returns>
            public bool MoveNext()
            {
                if (this.index < this.count)
                {
                    if (this.currentChunk == null || this.localChunkIndex == this.currentChunk.Length)
                    {
                        this.currentChunk = this.chunkedCollection.chunks[this.chunkIndex++];
                        this.localChunkIndex = 0;
                    }

                    this.current = this.currentChunk[this.localChunkIndex++];
                    ++this.index;
                    return true;
                }

                this.current = default(T);
                return false;
            }

            #endregion

            #region Explicit Interface Methods

            /// <summary>
            ///     The reset.
            /// </summary>
            void IEnumerator.Reset()
            {
                this.index = 0;
                this.localChunkIndex = 0;
                this.chunkIndex = 0;
                this.currentChunk = null;
                this.current = default(T);
            }

            #endregion
        }

        #endregion
    }
}