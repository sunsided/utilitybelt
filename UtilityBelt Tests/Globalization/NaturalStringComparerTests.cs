using NUnit.Framework;
using UtilityBelt.Globalization;

namespace UtilityBelt.Tests.Globalization
{
    [TestFixture]
    class NaturalStringComparerTests
    {
        [Test]
        [TestCase("a", "a", true, Result = 0)]
        [TestCase("a", "b", true, Result = -1)]
        [TestCase("b", "a", true, Result = 1)]
        [TestCase("ab", "aa", true, Result = 1)]
        [TestCase("a1", "a2", true, Result = -1)]
        [TestCase("a100", "a001", true, Result = 1)]
        [TestCase("a1", "a100", true, Result = -1)]
        [TestCase("a10", "a100", true, Result = -1)]
        [TestCase("a99", "a100", true, Result = -1)]
        [TestCase("a999", "a100", true, Result = 1)]
        [TestCase("value 1", "value1", true, Result = 0)]
        [TestCase("value 1", "value 100", true, Result = -1)]
        public int CompareStrings(string a, string b, bool ignoreWhiteSpace)
        {
            var comparer = new NaturalStringComparer(ignoreWhiteSpace);
            return comparer.Compare(a, b);
        }
    }
}
