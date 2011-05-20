//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryAccessCompareMode.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;

    /// <summary>
    /// Defines the compare modes for evicting items in a <see cref="QueuedDictionary"/>.
    /// </summary>
    public enum QueuedDictionaryAccessCompareMode
    {
        /// <summary>
        /// Identifies comparison by access count, in ascending order.
        /// Equivalent to evicting least-accessed items first.
        /// </summary>
        AccessCountAscending,

        /// <summary>
        /// Identifies comparison by access count, in descending order.
        /// Equivalent to evicting most-accessed items first.
        /// </summary>
        AccessCountDescending,

        /// <summary>
        /// Identifies comparison by creation date, in ascending order.
        /// Equivalent to LRU by creation date.
        /// </summary>
        CreationDateAscending,

        /// <summary>
        /// Identifies comparison by creation date, in descending order.
        /// Equivalent to MRU by creation date.
        /// </summary>
        CreationDateDescending,

        /// <summary>
        /// Identifies comparison by last access date, in ascending order.
        /// Equivalent to LRU by last access date.
        /// </summary>
        LastAccessDateAscending,

        /// <summary>
        /// Identifies comparison by last access date, in decending order.
        /// Equivalent to MRU by last access date.
        /// </summary>
        LastAccessDateDescending
    }
}