using System;
using System.Diagnostics.Contracts;
using System.Linq;
using NUnit.Framework;
using UtilityBelt.Versioning;

namespace UtilityBelt.Tests.Versioning
{
    /// <summary>
    /// Tests for the <see cref="SemanticVersion"/> type
    /// </summary>
    [TestFixture]
    public class SemanticVersionTests
    {
        /// <summary>
        /// Tests string building from simple version numbers
        /// </summary>
        [Test]
        public void SimpleVersionDecompositionResultEqualsInput()
        {
            var version = new SemanticVersion(1, 2, 3);
            Assert.That(version.ToString(), Is.EqualTo("1.2.3"));
        }

        /// <summary>
        /// Tests string building from complex version numbers where pre-release and build versions are given as collections
        /// </summary>
        [Test]
        public void ComplexVersionDecompositionResultEqualsInput()
        {
            var preRelease = new [] {"alpha", "2", "abc"};
            var build = new[] { "svn", "12342" };

            Contract.Assert(Contract.ForAll(preRelease, part => !String.IsNullOrEmpty(part) && part.All(Char.IsLetterOrDigit)));
            Contract.Assert(Contract.ForAll(build, part => !String.IsNullOrEmpty(part) && part.All(Char.IsLetterOrDigit)));

            var version = new SemanticVersion(1, 2, 3, preRelease, build);
            Assert.That(version.ToString(), Is.EqualTo("1.2.3-alpha.2.abc+svn.12342"));

            version = new SemanticVersion(1, 2, 3, preRelease, null);
            Assert.That(version.ToString(), Is.EqualTo("1.2.3-alpha.2.abc"));

            version = new SemanticVersion(1, 2, 3, null, build);
            Assert.That(version.ToString(), Is.EqualTo("1.2.3+svn.12342"));
        }

        /// <summary>
        /// Tests string building from complex version numbers where pre-release and build versions are given as strings
        /// </summary>
        [Test]
        public void ComplexVersionByStringDecompositionResultEqualsInput()
        {
            const string preRelease = "alpha.2.abc.f-d";
            const string build = "svn.a-4.12342";

            var version = new SemanticVersion(1, 2, 3, preRelease, build);
            Assert.That(version.ToString(), Is.EqualTo("1.2.3-alpha.2.abc.f-d+svn.a-4.12342"));

            version = new SemanticVersion(1, 2, 3, preRelease, null);
            Assert.That(version.ToString(), Is.EqualTo("1.2.3-alpha.2.abc.f-d"));

            version = new SemanticVersion(1, 2, 3, null, build);
            Assert.That(version.ToString(), Is.EqualTo("1.2.3+svn.a-4.12342"));
        }

        /// <summary>
        /// Ensures that passing an invalid pre-release version throws an exception
        /// </summary>
        /// <param name="preRelease">The pre-release version</param>
        [Test, ExpectedException(typeof(ArgumentException))]
        public void InvalidPreReleaseVersionThrows([Values("a b c", "ab+23", "ab???c", "1.b..2")] string preRelease)
        {
            new SemanticVersion(1, 2, 3, preRelease, null);
        }

        /// <summary>
        /// Ensures that passing an invalid build version throws an exception
        /// </summary>
        /// <param name="build">The build version</param>
        [Test, ExpectedException(typeof(ArgumentException))]
        public void InvalidBuildVersionThrows([Values("a b c", "ab+23", "ab???c", "1.b..2")] string build)
        {
            new SemanticVersion(1, 2, 3, null, build);
        }
    }
}
