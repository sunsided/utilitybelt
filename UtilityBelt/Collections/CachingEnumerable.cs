using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;

namespace UtilityBelt.Collections
{
    /// <summary>
    /// Enumerable that acts as a caching wrapper around another enumerable
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    public class CachingEnumerable<T> : IEnumerable<T>
    {
        /// <summary>
        /// The parent's enumerator
        /// </summary>
        private readonly Lazy<IEnumerator<T>> _parentEnumerator;

        /// <summary>
        /// Determines whether the parent's enumerator has reached the end
        /// </summary>
        private volatile bool _parentEnumeratorClosed;

        /// <summary>
        /// The data cache
        /// </summary>
        private readonly LinkedList<T> _cache = new LinkedList<T>();
        
        /// <summary>
        /// Lock to access the cache
        /// </summary>
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingEnumerable{T}" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public CachingEnumerable(IEnumerable<T> parent)
        {
            Contract.Requires(parent != null);
            _parentEnumerator = new Lazy<IEnumerator<T>>(parent.GetEnumerator, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingEnumerable{T}" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public CachingEnumerable(IEnumerator<T> parent)
        {
            Contract.Requires(parent != null);
            _parentEnumerator = new Lazy<IEnumerator<T>>(() => parent, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Requests a new value from the parent and adds it to the cache. Locking needs to be performed on the caller side.
        /// </summary>
        /// <returns><see langword="true"/> if the parent enumerator yielded a new value, or <see langword="false"/> if not</returns>
        private bool RequestFromParentAndAddToCache()
        {
            if (_parentEnumeratorClosed) return false;

            try
            {
                IEnumerator<T> enumerator = _parentEnumerator.Value;

                // request a value and exit if the parent enumerator stops
                bool hasValue = enumerator.MoveNext();
                if (!hasValue)
                {
                    _parentEnumeratorClosed = true;
                    return false;
                }

                // There are values, so pass them through and add them to the cache
                var element = enumerator.Current;
                _cache.AddLast(element);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(
                    "An error occurred while trying to request a new value from the enumeration source", e);
            }
        }
        
        /// <summary>
        /// Atomically requests a value from the cache and if there is none, requests it from the parent
        /// </summary>
        /// <param name="current">The current iterator token</param>
        /// <returns></returns>
        private LinkedListNode<T> RequestValue(LinkedListNode<T> current)
        {
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                // If this is the first run, current will be null, so we need to request a new value
                if (current == null || current.Next == null)
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        // Check if another thread already requested value
                        if (current == null)
                        {
                            // if the cache head is now diffferent from null, return that value
                            if (_cache.First != null) return _cache.First;
                        }
                        else if (current.Next != null)
                        {
                            // if the current token now has a follower, return that value
                            return current.Next;
                        }

                        // request a new value
                        if (RequestFromParentAndAddToCache())
                        {
                            // request succeeded, so there is one additional value in the cache
                            return current == null ? _cache.First : current.Next;
                        }

                        // request failed, so there is no next value
                        return null;
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
                }

                // At this point, there are follow-up values in the cache, so we just return the next one
                return current.Next;
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator<T>>() != null);
            LinkedListNode<T> current = null;
            do
            {
                current = RequestValue(current);
                if (current != null) yield return current.Value;

            } while (current != null);
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
    }
}
