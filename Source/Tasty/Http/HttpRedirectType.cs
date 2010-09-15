//-----------------------------------------------------------------------
// <copyright file="HttpRedirectType.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Http
{
    /// <summary>
    /// Defines the possible HTTP redirect types.
    /// </summary>
    public enum HttpRedirectType
    {
        /// <summary>
        /// Identifies a temporary redirect (302).
        /// </summary>
        Temporary = 0,

        /// <summary>
        /// Identifies a permanent redirect (301).
        /// </summary>
        Permanent
    }
}