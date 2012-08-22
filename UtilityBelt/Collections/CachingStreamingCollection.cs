using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Threading;

namespace UtilityBelt.Collections
{
    /// <summary>
    /// A caching collection that can serve multiple enumerators 
    /// while new items are added.
    /// Enumeration of this collection will eventually block until
    /// the add operation is finished using the control interface.
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class CachingStreamingCollection<T> : IEnumerable<T>
    {
        // TODO: Allow more than one interface; The collection is considered "closed" when no reader is open and will block if so

        /// <summary>
        /// The data store
        /// </summary>
        private readonly LinkedList<T> _dataList = new LinkedList<T>();

        /// <summary>
        /// Lock for <see cref="_dataList"/>
        /// </summary>
        private readonly ReaderWriterLockSlim _dataLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Determines whether the input stream is closed
        /// </summary>
        private volatile bool _streamClosed;

        /// <summary>
        /// WaitHandle to block on the close event
        /// </summary>
        private readonly ManualResetEvent _closeEvent = new ManualResetEvent(false);

        /// <summary>
        /// WaitHandle to block on the close event
        /// </summary>
        /// <seealso cref="AddingDone"/>
        public WaitHandle AddingDoneWaitHandle { get { return _closeEvent; } }

        /// <summary>
        /// Determines whether the adding has finished
        /// </summary>
        /// <seealso cref="AddingDoneWaitHandle"/>
        public bool AddingDone { get { return _streamClosed; } }

        /// <summary>
        /// The collection of reader locks
        /// </summary>
        private readonly Collection<AutoResetEvent> _readerLocks = new Collection<AutoResetEvent>();

        /// <summary>
        /// Determines whether the collection contains any data
        /// </summary>
        public bool HasData
        {
            get { return ReadFirstLocked(_dataList, _dataLock) != null; }
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="additionalData">Additional data.</param>
        /// <exception cref="System.InvalidOperationException">The input was already closed</exception>
        private void Add(T data, params T[] additionalData)
        {
            if (_streamClosed) throw new InvalidOperationException("Input already closed");

            _dataLock.EnterWriteLock();
            try
            {

                _dataList.AddLast(data);
                if (additionalData != null)
                {
                    for (int i = 0; i < additionalData.Length; ++i)
                    {
                        _dataList.AddLast(additionalData[i]);
                    }
                }
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }

            // wake up the iterators
            PulseIterators();
        }

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="System.InvalidOperationException">The input was already closed</exception>
        private void Add(IEnumerable<T> data)
        {
            Contract.Requires(data != null);
            if (_streamClosed) throw new InvalidOperationException("Input already closed");

            _dataLock.EnterWriteLock();
            try
            {
                foreach (var item in data)
                {
                    _dataList.AddLast(item);
                }
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }

            // wake up the iterators
            PulseIterators();
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        private void CloseStream()
        {
            if (_streamClosed) return;

            _dataLock.EnterWriteLock();
            try
            {
                _streamClosed = true;
                _closeEvent.Set();
            }
            finally
            {
                _dataLock.ExitWriteLock();
            }

            PulseIterators();
        }

        /// <summary>
        /// Pulses the iterators so they continue to serve data
        /// </summary>
        private void PulseIterators()
        {
            lock (_readerLocks)
            {
                foreach (var readerLock in _readerLocks)
                {
                    readerLock.Set();
                }
            }
        }

        /// <summary>
        /// Retrieves an interface to add elements to this collection
        /// </summary>
        /// <returns>The adding interface</returns>
        public IStreamingDataAppender<T> BeginAdding()
        {
            Contract.Ensures(Contract.Result<IStreamingDataAppender<T>>() != null);
            return new DataAppender(this);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator<T> GetEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator<T>>() != null);

            // shortcuts
            var dataList = _dataList;
            
            // register the lock for insert notifications
            var readerLock = new AutoResetEvent(false);
            lock (_readerLocks) _readerLocks.Add(readerLock);
            try
            {
                // if the stream is already closed, just pump out the values
                // TODO: yield unter full read lock!
                if (_streamClosed) { foreach (var item in dataList) { yield return item; } yield break; } 

                LinkedListNode<T> nextToken;

                // Read the first element
                LinkedListNode<T> token = ReadFirstLocked(dataList, _dataLock);

                // If there is no data at all, wait for the first signal
                // Afterwards check for the close signal and return the values if needed
                if (token == null)
                {
                    readerLock.WaitOne();
                    // TODO: yield unter full read lock!
                    if (_streamClosed) { foreach (var item in dataList) { yield return item; } yield break; }

                    // At this point, the first element has data
                    token = ReadFirstLocked(dataList, _dataLock);
                    Contract.Assert(token != null);
                }

                // yield the first value
                yield return token.Value;

                // begin the main iterator loop
                do
                {
                    // read the follower
                    nextToken = ReadNextLocked(token, _dataLock);

                    // if there is data, iterate next round
                    if (nextToken != null)
                    {
                        token = nextToken;
                        yield return token.Value;
                        continue;
                    }

                    // if there is no new data, wait for a signal
                    readerLock.WaitOne();

                } while (!_streamClosed);

                // At this point, iteration may have been interrupted, 
                // but data can still be available, so we simply iterate to the end.
                while ((nextToken = ReadNextLocked(token, _dataLock)) != null)
                {
                    token = nextToken;
                    yield return token.Value;
                }
            }
            finally
            {
                lock(_readerLocks) _readerLocks.Remove(readerLock);
            }
        }

        /// <summary>
        /// Reads the first token read locked
        /// </summary>
        /// <param name="list">The list</param>
        /// <param name="readLock">The read lock</param>
        /// <returns>The first node or <c>null</c> if <paramref name="list"/> has no elements</returns>
        private static LinkedListNode<T> ReadFirstLocked(LinkedList<T> list, ReaderWriterLockSlim readLock)
        {
            Contract.Requires(list != null);
            Contract.Requires(readLock != null);
            Contract.Ensures(Contract.Result<LinkedListNode<T>>() == list.First);

            readLock.EnterReadLock();
            try { return list.First; }
            finally { readLock.ExitReadLock(); }
        }

        /// <summary>
        /// Reads the next token read locked
        /// </summary>
        /// <param name="token">The token</param>
        /// <param name="readLock">The read lock</param>
        /// <returns>The next node or <c>null</c> if <paramref name="token"/> is the last element</returns>
        private static LinkedListNode<T> ReadNextLocked(LinkedListNode<T> token, ReaderWriterLockSlim readLock)
        {
            Contract.Requires(token != null);
            Contract.Requires(readLock != null);
            Contract.Ensures(Contract.Result<LinkedListNode<T>>() == token.Next);

            readLock.EnterReadLock();
            try { return token.Next; }
            finally { readLock.ExitReadLock(); }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator>() != null);
            return GetEnumerator();
        }

        /// <summary>
        /// Concrete data appender instance
        /// </summary>
        private class DataAppender : IStreamingDataAppender<T>
        {
            /// <summary>
            /// The collection to control
            /// </summary>
            private readonly CachingStreamingCollection<T> _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="DataAppender" /> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public DataAppender(CachingStreamingCollection<T> parent)
            {
                Contract.Requires(parent != null);
                Contract.Ensures(_parent != null);

                _parent = parent;
            }

            /// <summary>
            /// Disposes this instance.
            /// </summary>
            public void Dispose()
            {
                Finish();
            }

            /// <summary>
            /// Adds the specified data.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="additionalData">The additional data.</param>
            /// <exception cref="System.InvalidOperationException">The input was already closed</exception>
            public void Add(T data, params T[] additionalData)
            {
                if (_parent._streamClosed) throw new InvalidOperationException("Input already closed.");
                _parent.Add(data, additionalData);
            }

            /// <summary>
            /// Adds the specified data.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <exception cref="System.InvalidOperationException">The input was already closed</exception>
            public void Add(IEnumerable<T> data)
            {
                if (_parent._streamClosed) throw new InvalidOperationException("Input already closed.");
                _parent.Add(data);
            }

            /// <summary>
            /// Finishes the adding operation
            /// </summary>
            public void Finish()
            {
                _parent.CloseStream();
            }
        }
    }
}
