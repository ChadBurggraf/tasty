﻿//-----------------------------------------------------------------------
// <copyright file="QueuedDictionaryAccessOfT.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;

    /// <summary>
    /// Represents access statistics for a key in a <see cref="QueuedDictionary"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    [Serializable]
    public sealed class QueuedDictionaryAccess<TKey> : QueuedDictionaryAccess
    {
        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryAccess class.
        /// </summary>
        /// <param name="key">The dictionary key being tracked.</param>
        public QueuedDictionaryAccess(TKey key)
            : base(key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the QueuedDictionaryAccess class.
        /// </summary>
        /// <param name="other">The object to create this instance from.</param>
        public QueuedDictionaryAccess(QueuedDictionaryAccess<TKey> other)
            : base(other)
        {
        }

        /// <summary>
        /// Gets or sets the key being referenced.
        /// </summary>
        public new TKey Key { get; set; }
    }
}