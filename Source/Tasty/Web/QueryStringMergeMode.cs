//-----------------------------------------------------------------------
// <copyright file="QueryStringMergeMode.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;

    /// <summary>
    /// Defines the possible modes a querystring merge operation can be performed in.
    /// </summary>
    public enum QueryStringMergeMode
    {
        /// <summary>
        /// Identifies that any existing keys in the destination query string are left untouched.
        /// </summary>
        SkipExisting = 0,

        /// <summary>
        /// Identifies that all keys are added to the desintation query string,
        /// possibly resulting in duplicate keys and even duplicate values.
        /// </summary>
        AddToExisting,

        /// <summary>
        /// Identifies that any existing keys in the destination are overwritten 
        /// by the values found in the source.
        /// </summary>
        OverwriteExisting
    }
}
