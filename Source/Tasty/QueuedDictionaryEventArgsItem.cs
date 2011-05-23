//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryEventArgsItem.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;

    /// <summary>
    /// Represents an item that has participated in a <see cref="QueuedDictionary{TKey, TValue}"/> event.
    /// </summary>
    [Serializable]
    public sealed class QueuedDictionaryEventArgsItem
    {
        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryEventArgsItem class.
        /// </summary>
        /// <param name="key">The item's key.</param>
        /// <param name="value">The item's value.</param>
        /// <param name="access">The item's access statistics.</param>
        public QueuedDictionaryEventArgsItem(object key, object value, QueuedDictionaryAccess access)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "key cannot be null.");
            }

            if (access == null)
            {
                throw new ArgumentNullException("access", "access cannot be null.");
            }

            this.Key = key;
            this.Value = value;
            this.Access = access;
        }

        /// <summary>
        /// Gets the item's access statistics.
        /// </summary>
        public QueuedDictionaryAccess Access { get; private set; }

        /// <summary>
        /// Gets the item's key.
        /// </summary>
        public object Key { get; private set; }

        /// <summary>
        /// Gets the item's value.
        /// </summary>
        public object Value { get; private set; }
    }
}
