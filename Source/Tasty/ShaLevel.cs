using System;
using System.Diagnostics.CodeAnalysis;

namespace Tasty
{
    /// <summary>
    /// Defines the possible SHA hashing levels.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
    public enum ShaLevel
    {
        /// <summary>
        /// Identifies SHA1.
        /// </summary>
        One,

        /// <summary>
        /// Identifies SHA256.
        /// </summary>
        TwoFiftySix,

        /// <summary>
        /// Identifies SHA512.
        /// </summary>
        FiveTwelve
    }
}
