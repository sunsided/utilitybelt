using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace UtilityBelt.Collections
{
    /// <summary>
    /// Extension methods for <see cref="CachingEnumerable{T}"/>
    /// </summary>
    public static class CachingEnumerableExtensions
    {
        /// <summary>
        /// Creates a new <see cref="CachingEnumerable{T}"/> to wrap the original source
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <param name="source">The <see cref="IEnumerable{T}"/> to wrap</param>
        /// <returns>The <see cref="CachingEnumerable{T}"/> instance that wraps the source</returns>
        public static IEnumerable<T> GetCachingEnumerator<T>(this IEnumerable<T> source)
        {
            Contract.Requires(source != null);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
            return new CachingEnumerable<T>(source);
        }
    }
}
