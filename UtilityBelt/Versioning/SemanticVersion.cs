﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using UtilityBelt.Globalization;

namespace UtilityBelt.Versioning
{
    /// <summary>
    /// A semantic version as defined in <a href="http://semver.org/">Semantic Versioning 2.0.0-rc.1</a>
    /// </summary>
    public struct SemanticVersion : IComparable<SemanticVersion>, IComparable<Version>
    {
        /// <summary>
        /// The major version number
        /// </summary>
        public readonly int Major;

        /// <summary>
        /// The minor version number
        /// </summary>
        public readonly int Minor;

        /// <summary>
        /// The patch number
        /// </summary>
        public readonly int Patch;

        /// <summary>
        /// The pre-release version
        /// </summary>
        public readonly IList<string> PreReleaseParts;

        /// <summary>
        /// The pre-release version
        /// </summary>
        public string PreRelease
        {
            [Pure]
            get { return String.Join(".", PreReleaseParts); }
        }

        /// <summary>
        /// The build version
        /// </summary>
        public readonly IList<string> BuildParts;

        /// <summary>
        /// The build version
        /// </summary>
        public string Build
        {
            [Pure]
            get { return String.Join(".", BuildParts); }
        }

        /// <summary>
        /// Determines if this version is a prerelease version
        /// </summary>
        public bool IsPrerelease
        {
            [Pure]
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (PreReleaseParts.Count > 0));
                return PreReleaseParts.Count > 0;
            }
        }

        /// <summary>
        /// Determines if this version has a build version part
        /// </summary>
        public bool HasBuildVersion
        {
            [Pure]
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (BuildParts.Count > 0));
                return BuildParts.Count > 0;
            }
        }

        /// <summary>
        /// Determines if this is an initial development version
        /// </summary>
        public bool IsInitialDevelopmentVersion
        {
            [Pure]
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (Major == 0));
                return Major == 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticVersion" /> struct.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        public SemanticVersion(int major, int minor, int patch)
            : this(major, minor, patch, new string[0], new string[0])
        {
            Contract.Requires(major >= 0);
            Contract.Requires(minor >= 0);
            Contract.Requires(patch >= 0);

            Contract.Ensures(Contract.ValueAtReturn(out Major) == major);
            Contract.Ensures(Contract.ValueAtReturn(out Minor) == minor);
            Contract.Ensures(Contract.ValueAtReturn(out Patch) == patch);
            Contract.Ensures(Contract.ValueAtReturn(out PreReleaseParts).Count == 0);
            Contract.Ensures(Contract.ValueAtReturn(out BuildParts).Count == 0);

            Contract.Assume(PreReleaseParts.Count == 0);
            Contract.Assume(BuildParts.Count == 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticVersion" /> struct.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="preRelease">The pre release version. Must be a series of alphanumerical strings combined by dots, e.g. <code>alpha.2b.123</code>.</param>
        /// <param name="build">The build version. Must be a series of alphanumerical strings combined by dots, e.g. <code>alpha.2b.123</code>.</param>
        /// <exception cref="ArgumentException">Either <paramref name="preRelease"/> or <paramref name="build"/> versions contain invalid strings</exception>
        public SemanticVersion(int major, int minor, int patch, string preRelease, string build)
            : this(major, minor, patch, preRelease == null ? new string[0] : preRelease.Split('.'), build == null ? new string[0] : build.Split('.'))
        {
            Contract.Requires(major >= 0);
            Contract.Requires(minor >= 0);
            Contract.Requires(patch >= 0);

            Contract.Ensures(Contract.ValueAtReturn(out Major) == major);
            Contract.Ensures(Contract.ValueAtReturn(out Minor) == minor);
            Contract.Ensures(Contract.ValueAtReturn(out Patch) == patch);
            Contract.Ensures(Contract.ValueAtReturn(out PreReleaseParts) != null && Contract.ValueAtReturn(out PreReleaseParts).IsReadOnly);
            Contract.Ensures(Contract.ValueAtReturn(out BuildParts) != null && Contract.ValueAtReturn(out BuildParts).IsReadOnly);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemanticVersion" /> struct.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        /// <param name="preRelease">The pre release version. Must be a collection of alphanumerical strings, e.g. <code>{"alpha", "2b", "123"}</code>.</param>
        /// <param name="build">The build version. Must be a collection of alphanumerical strings, e.g. <code>{"alpha", "2b", "123"}</code>.</param>
        /// <exception cref="ArgumentException">Either <paramref name="preRelease"/> or <paramref name="build"/> versions contain invalid strings</exception>
        public SemanticVersion(int major, int minor, int patch, ICollection<string> preRelease, ICollection<string> build)
        {
            Contract.Requires(major >= 0);
            Contract.Requires(minor >= 0);
            Contract.Requires(patch >= 0);

            Contract.Ensures(Contract.ValueAtReturn(out Major) == major);
            Contract.Ensures(Contract.ValueAtReturn(out Minor) == minor);
            Contract.Ensures(Contract.ValueAtReturn(out Patch) == patch);
            Contract.Ensures(Contract.ValueAtReturn(out PreReleaseParts) != null && Contract.ValueAtReturn(out PreReleaseParts).IsReadOnly);
            Contract.Ensures(Contract.ValueAtReturn(out BuildParts) != null && Contract.ValueAtReturn(out BuildParts).IsReadOnly);

            // check pre-release and build version
            if (!IsValidVersionString(preRelease)) throw new ArgumentException("Pre-release version contains an invalid part.");
            if (!IsValidVersionString(build)) throw new ArgumentException("Build version contains an invalid part.");

            Major = major;
            Minor = minor;
            Patch = patch;
            PreReleaseParts = preRelease == null ? new string[0] : preRelease.ToArray();
            BuildParts = build == null ? new string[0] : build.ToArray();

            Contract.Assume(preRelease != null && PreReleaseParts.Count == preRelease.Count || PreReleaseParts.Count == 0);
            Contract.Assume(build != null && BuildParts.Count == build.Count || BuildParts.Count == 0);
            Contract.Assume(PreReleaseParts.IsReadOnly);
            Contract.Assume(BuildParts.IsReadOnly);
        }

        /// <summary>
        /// Determines whether the given version string collection is valid.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns><see langword="true" /> if the version is valid; otherwise, <see langword="false" />.</returns>
        [Pure]
        private static bool IsValidVersionString(ICollection<string> version)
        {
            if (version != null && version.Count >= 0)
            {
                if (version.Any(part => String.IsNullOrWhiteSpace(part) || !part.All(c => Char.IsLetterOrDigit(c) || c == '-')))
                {
                    return false;
                }

                Contract.Assume(Contract.ForAll(version,
                                                str =>
                                                !String.IsNullOrWhiteSpace(str) &&
                                                Contract.ForAll(str, c => Char.IsLetterOrDigit(c) || c == '-')));
            }
            return true;
        }

                                                           /// <summary>
        /// Returns a new <see cref="SemanticVersion"/> with the next highest major version number
        /// </summary>
        /// <returns>The <see cref="SemanticVersion"/>.</returns>
        [Pure]
        public SemanticVersion IncrementMajorVersion()
        {
            Contract.Ensures(Contract.Result<SemanticVersion>().Major == Major+1);
            Contract.Ensures(Contract.Result<SemanticVersion>().Minor == 0);
            Contract.Ensures(Contract.Result<SemanticVersion>().Patch == 0);
            Contract.Ensures(Contract.Result<SemanticVersion>().PreReleaseParts.Count == 0);
            Contract.Ensures(Contract.Result<SemanticVersion>().BuildParts.Count == 0);

            return new SemanticVersion(Major + 1, 0, 0);
        }

        /// <summary>
        /// Returns a new <see cref="SemanticVersion"/> with the next highest minor version number
        /// </summary>
        /// <returns>The <see cref="SemanticVersion"/>.</returns>
        [Pure]
        public SemanticVersion IncrementMinorVersion()
        {
            Contract.Ensures(Contract.Result<SemanticVersion>().Major == Major);
            Contract.Ensures(Contract.Result<SemanticVersion>().Minor == Minor + 1);
            Contract.Ensures(Contract.Result<SemanticVersion>().Patch == 0);
            Contract.Ensures(Contract.Result<SemanticVersion>().PreReleaseParts.Count == 0);
            Contract.Ensures(Contract.Result<SemanticVersion>().BuildParts.Count == 0);

            return new SemanticVersion(Major, Minor + 1, 0);
        }

        /// <summary>
        /// Converts this semantic version to a <see cref="Version"/>
        /// </summary>
        /// <returns>The <see cref="Version"/></returns>
        [Pure]
        public Version ConvertToVersion()
        {
            Contract.Ensures(Contract.Result<Version>().Major == Major);
            Contract.Ensures(Contract.Result<Version>().Minor == Minor);
            Contract.Ensures(Contract.Result<Version>().Build == Patch);

            var version = new Version(Major, Minor, Patch);
            Contract.Assume(version.Major == Major);
            Contract.Assume(version.Minor == Minor);
            Contract.Assume(version.Build == Patch);

            return version;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(SemanticVersion other)
        {
            int result = Major.CompareTo(other.Major);
            if (result == 0) result = Minor.CompareTo(other.Minor);
            if (result == 0) result = Patch.CompareTo(other.Patch);
            if (result == 0 && IsPrerelease) result = -1;

            var comparer = new NaturalStringComparer();
            if (result == 0) result = comparer.Compare(PreRelease, other.PreRelease);
            if (result == 0) result = comparer.Compare(Build, other.Build);
            return result;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(Version other)
        {
            int result = Major.CompareTo(other.Major);
            if (result == 0) result = Minor.CompareTo(other.Minor);
            if (result == 0) result = Patch.CompareTo(other.Build);
            if (result == 0) result = Build.CompareTo(other.Revision);
            return result;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            Contract.Ensures(Contract.Result<string>() != null);

            var builder = new StringBuilder();
            builder.AppendFormat("{0}.{1}.{2}", Major, Minor, Patch);
            if (IsPrerelease)
            {
                builder.Append("-" + String.Join(".", PreReleaseParts));
            }
            if (HasBuildVersion)
            {
                builder.Append("+" + String.Join(".", BuildParts));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj is SemanticVersion) return CompareTo((SemanticVersion) obj) == 0;
            if (obj is Version) return CompareTo((Version)obj) == 0;
            return base.Equals(obj);
        }

        /// <summary>
        /// Contract invariants
        /// </summary>
        [ContractInvariantMethod]
        private void ContractInvariant()
        {
            Contract.Invariant(Major >= 0);
            Contract.Invariant(Minor >= 0);
            Contract.Invariant(Patch >= 0);
            Contract.Invariant(PreReleaseParts != null);
            Contract.Invariant(Build != null);
        }
    }
}
