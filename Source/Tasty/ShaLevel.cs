//-----------------------------------------------------------------------
// <copyright file="ShaLevel.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Diagnostics.CodeAnalysis;

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
