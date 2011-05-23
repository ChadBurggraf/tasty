//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryAccess.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents access statistics for a key in a <see cref="QueuedDictionary{TKey, TValue}"/>.
    /// </summary>
    [Serializable]
    public class QueuedDictionaryAccess :
        IEquatable<QueuedDictionaryAccess>,
        IComparable<QueuedDictionaryAccess>,
        IComparable
    {
        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryAccess class.
        /// </summary>
        /// <param name="key">The dictionary key being tracked.</param>
        public QueuedDictionaryAccess(object key)
        {
            this.Key = key;
            this.CreationDate = this.LastAccessDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryAccess class.
        /// </summary>
        /// <param name="other">The object to create this instance from.</param>
        public QueuedDictionaryAccess(QueuedDictionaryAccess other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other", "other cannot be null.");
            }

            this.AccessCount = other.AccessCount;
            this.CreationDate = other.CreationDate;
            this.Key = other.Key;
            this.LastAccessDate = other.LastAccessDate;
        }

        /// <summary>
        /// Gets or sets the key's total access count.
        /// </summary>
        public int AccessCount { get; set; }

        /// <summary>
        /// Gets the date the key was created.
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets or sets the key being referenced.
        /// </summary>
        public object Key { get; set; }

        /// <summary>
        /// Gets or sets the key's last access date.
        /// </summary>
        public DateTime LastAccessDate { get; set; }

        /// <summary>
        /// Gets a value indicating whether the given objects are equal.
        /// </summary>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>True if the objects are equal, false otherwise.</returns>
        public static bool operator ==(QueuedDictionaryAccess left, QueuedDictionaryAccess right)
        {
            if (left as object != null)
            {
                return left.Equals(right);
            }
            else if (right as object != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the given objects are not-equal.
        /// </summary>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>True if the objects are not equal, false otherwise.</returns>
        public static bool operator !=(QueuedDictionaryAccess left, QueuedDictionaryAccess right)
        {
            if (left as object != null)
            {
                return !left.Equals(right);
            }
            else if (right as object != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the left object precedes the right object.
        /// </summary>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>True if the left object precedes the right object, false otherwise.</returns>
        public static bool operator <(QueuedDictionaryAccess left, QueuedDictionaryAccess right)
        {
            if (left as object != null)
            {
                return 0 > left.CompareTo(right);
            }
            else if (right as object != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the left object follows the right object.
        /// </summary>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>True if the left object follows the right object, false otherwise.</returns>
        public static bool operator >(QueuedDictionaryAccess left, QueuedDictionaryAccess right)
        {
            if (left as object != null)
            {
                return 0 < left.CompareTo(right);
            }
            else if (right as object != null)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as QueuedDictionaryAccess);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(QueuedDictionaryAccess other)
        {
            if (other != null)
            {
                IComparable thisKey = this.Key as IComparable;
                IComparable otherKey = other.Key as IComparable;

                if (thisKey != null)
                {
                    return thisKey.CompareTo(otherKey);
                }
                else if (otherKey != null)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }

            return 1;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the given object, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as QueuedDictionaryAccess);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the given object, false otherwise.</returns>
        public bool Equals(QueuedDictionaryAccess other)
        {
            if (this.Key != null && other != null && other.Key != null)
            {
                return this.Key.Equals(other.Key);
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            return this.Key != null ? this.Key.GetHashCode() : 0;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return this.Key != null ? this.Key.ToString() : String.Empty;
        }
    }
}
