using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace UtilityBelt.Comparer
{
    /// <summary>
    /// Compares two sequences.
    /// </summary>
    /// <typeparam name="T">Type of item in the sequences.</typeparam>
    /// <remarks>
    /// Compares elements from the two input sequences in turn. If we
    /// run out of list before finding unequal elements, then the shorter
    /// list is deemed to be the lesser list.
    /// </remarks>
    /// <seealso cref="http://www.interact-sw.co.uk/iangblog/2007/12/13/natural-sorting"/>
    public class EnumerableComparer<T> : IComparer<IEnumerable<T>>
    {
        /// <summary>
        /// Object used for comparing each element.
        /// </summary>
        private readonly IComparer<T> _baseComparer;

        /// <summary>
        /// Create a sequence comparer using the default comparer for T.
        /// </summary>
        public EnumerableComparer()
            : this(null)
        {
        }

        /// <summary>
        /// Create a sequence comparer, using the specified item comparer
        /// for T.
        /// </summary>
        /// <param name="comparer">Comparer for comparing each pair of  items from the sequences.</param>
        public EnumerableComparer(IComparer<T> comparer)
        {
            _baseComparer = comparer ?? Comparer<T>.Default;
        }
        
        /// <summary>
        /// Compare two sequences of T.
        /// </summary>
        /// <param name="x">First sequence.</param>
        /// <param name="y">Second sequence.</param>
        public int Compare(IEnumerable<T> x, IEnumerable<T> y)
        {
            Contract.Requires(x != null);
            Contract.Requires(y != null);

            using (var leftIt = x.GetEnumerator())
            using (var rightIt = y.GetEnumerator())
            {
                while (true)
                {
                    bool left = leftIt.MoveNext();
                    bool right = rightIt.MoveNext();

                    if (!(left || right)) return 0;

                    if (!left) return -1;
                    if (!right) return 1;

                    int itemResult = _baseComparer.Compare(leftIt.Current, rightIt.Current);
                    if (itemResult != 0) return itemResult;
                }
            }
        }
    }
}
