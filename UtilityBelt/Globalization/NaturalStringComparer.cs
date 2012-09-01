using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using UtilityBelt.Comparer;

namespace UtilityBelt.Globalization
{
    /// <summary>
    /// Comparer for natural string comparison
    /// </summary>
    public class NaturalStringComparer : IComparer<string>
    {
        /// <summary>
        /// Determines if whitespace will be ignored in comparison
        /// </summary>
        private readonly bool _ignoreWhiteSpace;

        /// <summary>
        /// The part comparer
        /// </summary>
        private readonly EnumerableComparer<object> _partComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalStringComparer" /> class.
        /// </summary>
        public NaturalStringComparer()
            : this(true, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalStringComparer" /> class.
        /// </summary>
        /// <param name="ignoreWhiteSpace">if set to <see langword="true" /> whitespace will be ignored during comparison.</param>
        public NaturalStringComparer([DefaultValue(true)] bool ignoreWhiteSpace)
            : this(ignoreWhiteSpace, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalStringComparer" /> class.
        /// </summary>
        /// <param name="ignoreWhiteSpace">if set to <see langword="true" /> whitespace will be ignored during comparison.</param>
        /// <param name="baseComparer">Comparer for comparing each pair of items from the string.</param>
        public NaturalStringComparer([DefaultValue(true)] bool ignoreWhiteSpace, [DefaultValue(null)] IComparer<object> baseComparer)
        {
            _ignoreWhiteSpace = ignoreWhiteSpace;
            _partComparer = new EnumerableComparer<object>(baseComparer);
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <param name="x">The first object to compare.</param><param name="y">The second object to compare.</param>
        public int Compare(string x, string y)
        {
            Func<string, object> convert = str =>
                                               {
                                                   try
                                                   {
                                                       return Int32.Parse(str);
                                                   }
                                                   catch
                                                   {
                                                       return str;
                                                   }
                                               };

            var left = Regex.Split(_ignoreWhiteSpace ? x.Replace(" ", "") : x, "([0-9]+)").Select(convert);
            var right = Regex.Split(_ignoreWhiteSpace ? y.Replace(" ", "") : y, "([0-9]+)").Select(convert);

            Contract.Assume(left != null);
            Contract.Assume(right != null);
            return _partComparer.Compare(left, right);
        }
    }
}
