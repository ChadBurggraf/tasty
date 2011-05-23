//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryEventArgs.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents event arguments for <see cref="QueuedDictionary{TKey, TValue}"/> events.
    /// </summary>
    [Serializable]
    public sealed class QueuedDictionaryEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryEventArgs class.
        /// </summary>
        /// <param name="items">The collection of items participating in the event.</param>
        public QueuedDictionaryEventArgs(IEnumerable<QueuedDictionaryEventArgsItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.Items = new List<QueuedDictionaryEventArgsItem>(items);
        }

        /// <summary>
        /// Gets the collection of items participating in the event.
        /// </summary>
        public IList<QueuedDictionaryEventArgsItem> Items { get; private set; }
    }
}
