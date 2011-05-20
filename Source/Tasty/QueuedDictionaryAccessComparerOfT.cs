//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryAccessComparerOfT.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implements <see cref="IComparer{T}"/> to compare <see cref="QueuedDictionAccess"/> objects.
    /// </summary>
    /// <typeparam name="TKey">The key type of the <see cref="QueuedDictionaryAccess{TKey}"/> objects being compared.</typeparam>
    public sealed class QueuedDictionaryAccessComparer<TKey> : QueuedDictionaryAccessComparer, IComparer<QueuedDictionaryAccess<TKey>>
    {
        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryAccessComparer class.
        /// </summary>
        /// <param name="mode">The comparison mode to use.</param>
        public QueuedDictionaryAccessComparer(QueuedDictionaryAccessCompareMode mode)
            : base(mode)
        {
        }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>The comparison result.</returns>
        public int Compare(QueuedDictionaryAccess<TKey> x, QueuedDictionaryAccess<TKey> y)
        {
            return base.Compare(x, y);
        }
    }
}