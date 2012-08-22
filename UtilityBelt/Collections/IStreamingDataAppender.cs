using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace UtilityBelt.Collections
{

    /// <summary>
    /// Interface to the adding operations of a <see cref="CachingStreamingCollection{T}"/>
    /// </summary>
    /// <typeparam name="T">The data type</typeparam>
    [ContractClass(typeof(StreamingDataAppenderContracts<>))]
    public interface IStreamingDataAppender<in T> : IDisposable
    {
        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="additionalData">The additional data.</param>
        /// <exception cref="System.InvalidOperationException">The input was already closed</exception>
        void Add(T data, params T[] additionalData);

        /// <summary>
        /// Adds the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="System.InvalidOperationException">The input was already closed</exception>
        void Add(IEnumerable<T> data);

        /// <summary>
        /// Finishes the adding operation
        /// </summary>
        void Finish();
    }

    #region Contracts

    [ContractClassFor(typeof(IStreamingDataAppender<>))]
    internal abstract class StreamingDataAppenderContracts<T> : IStreamingDataAppender<T>
    {
        public abstract void Dispose();

        public abstract void Add(T data, params T[] additionalData);

        public void Add(IEnumerable<T> data)
        {
            Contract.Requires(data != null);
        }

        public abstract void Finish();
    }

    #endregion Contracts
}