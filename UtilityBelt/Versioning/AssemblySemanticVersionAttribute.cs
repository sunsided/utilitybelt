using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.InteropServices;

namespace UtilityBelt.Versioning
{
    /// <summary>
    /// Specifies the semantic version of the assembly being attributed.
    /// </summary>
    /// <seealso cref="AssemblyVersionAttribute"/>
    [AttributeUsageAttribute(AttributeTargets.Assembly, Inherited = false)]
    [ComVisible(true)]
    public class AssemblySemanticVersionAttribute : Attribute
    {
        /// <summary>
        /// A string containing the assembly version number.
        /// </summary>
        public string Version
        {
            [Pure] get
            {
                Contract.Ensures(Contract.Result<string>() != null);
                return SemanticVersion.ToString();
            }
        }

        /// <summary>
        /// The semantic version
        /// </summary>
        public SemanticVersion SemanticVersion { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblySemanticVersionAttribute" /> class.
        /// </summary>
        /// <param name="version">The version.</param>
        public AssemblySemanticVersionAttribute(SemanticVersion version)
        {
            SemanticVersion = version;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="AssemblySemanticVersionAttribute" /> to <see cref="AssemblyVersionAttribute" />.
        /// </summary>
        /// <param name="versionAttribute">The version attribute.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator AssemblyVersionAttribute(AssemblySemanticVersionAttribute versionAttribute)
        {
            Contract.Requires(versionAttribute != null);
            Contract.Ensures(Contract.Result<AssemblyVersionAttribute>() != null);
            
            Version version = versionAttribute.SemanticVersion.ConvertToVersion();
            return new AssemblyVersionAttribute(version.ToString());
        }
    }
}
