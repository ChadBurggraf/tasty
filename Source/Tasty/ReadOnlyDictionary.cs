//-----------------------------------------------------------------------
// <copyright file="ReadOnlyDictionary.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Wraps an <see cref="IDictionary{TKey, TValue}"/> instance with read-only behavior.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> dictionary;

        /// <summary>
        /// Initializes a new instance of the ReadOnlyDictionary class.
        /// </summary>
        /// <param name="dictionary">The dictionary to wrap with read only behavior.</param>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary", "dictionary cannot be null.");
            }

            this.dictionary = dictionary;
        }

        /// <summary>
        /// Gets the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return this.dictionary.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether this collection is read only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a collection of all of the keys in the dictionary.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return this.dictionary.Keys; }
        }

        /// <summary>
        /// Gets a collection containing all of the values in the dictionary.
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return this.dictionary.Values; }
        }

        /// <summary>
        /// Gets or sets the value for the given key. This indexer's setter is not supported.
        /// </summary>
        /// <param name="key">The key to get or set the value for.</param>
        /// <returns>The value for the given key.</returns>
        public TValue this[TKey key]
        {
            get
            {
                return this.dictionary[key];
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.dictionary.Contains(item);
        }

        /// <summary>
        /// Gets a value indicating whether the dictionary contains the given key.
        /// </summary>
        /// <param name="key">The key to check for.</param>
        /// <returns>True if the dictionary contains the key, false otherwise.</returns>
        public bool ContainsKey(TKey key)
        {
            return this.dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Copies the elements of the collection to the given array, starting at the given index in the array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index in the array to start copying at.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.dictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        public bool Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Tries to get the value for the given key.
        /// </summary>
        /// <param name="key">The key to try to get the value for.</param>
        /// <param name="value">Contains the value upon completion when successful.</param>
        /// <returns>True if the value was retrieved successfully, false otherwise.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }
    }
}
