//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryAccessComparer.cs" company="Tasty Codes">
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
    public class QueuedDictionaryAccessComparer : IComparer<QueuedDictionaryAccess>
    {
        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryAccessComparer class.
        /// </summary>
        /// <param name="mode">The comparison mode to use.</param>
        public QueuedDictionaryAccessComparer(QueuedDictionaryAccessCompareMode mode)
        {
            this.Mode = mode;
        }

        /// <summary>
        /// Gets or sets the comparison mode to use.
        /// </summary>
        public QueuedDictionaryAccessCompareMode Mode { get; set; }

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>The comparison result.</returns>
        public int Compare(QueuedDictionaryAccess x, QueuedDictionaryAccess y)
        {
            int result = 0;

            if (x != null)
            {
                switch (this.Mode)
                {
                    case QueuedDictionaryAccessCompareMode.AccessCountAscending:
                        result = x.AccessCount.CompareTo(y.AccessCount);
                        break;
                    case QueuedDictionaryAccessCompareMode.AccessCountDescending:
                        result = -1 * x.AccessCount.CompareTo(y.AccessCount);
                        break;
                    case QueuedDictionaryAccessCompareMode.CreationDateAscending:
                        result = x.CreationDate.CompareTo(y.CreationDate);
                        break;
                    case QueuedDictionaryAccessCompareMode.CreationDateDescending:
                        result = -1 * x.CreationDate.CompareTo(y.CreationDate);
                        break;
                    case QueuedDictionaryAccessCompareMode.LastAccessDateAscending:
                        result = x.LastAccessDate.CompareTo(y.LastAccessDate);
                        break;
                    case QueuedDictionaryAccessCompareMode.LastAccessDateDescending:
                        result = -1 * x.LastAccessDate.CompareTo(y.LastAccessDate);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (y != null)
            {
                result = -1;
            }

            return result;
        }
    }
}
