//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryAccess.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Represents access statistics for a key in a <see cref="QueuedDictionary{TKey, TValue}"/>.
    /// </summary>
    [Serializable]
    public class QueuedDictionaryAccess : IEquatable<QueuedDictionaryAccess>
    {
        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryAccess class.
        /// </summary>
        /// <param name="key">The dictionary key being tracked.</param>
        public QueuedDictionaryAccess(object key)
            : this(key, DateTime.Now)
        {
        }

        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryAccess class.
        /// </summary>
        /// <param name="key">The dictionary key being tracked.</param>
        /// <param name="now">The current date, used to initialize the date-based access fields.</param>
        public QueuedDictionaryAccess(object key, DateTime now)
        {
            this.Key = key;
            this.CreationDate = this.LastAccessDate = now;
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
