//-----------------------------------------------------------------------
// <copyright file="QueuedDictionary.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents a dictionary with a size cap, which automatically evicts items
    /// based on the desired access pattern.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    [Serializable]
    public class QueuedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, ISerializable
    {
        #region Private Fields

        private int maximumSize;
        private QueuedDictionaryAccessCompareMode evictionMode;
        private Dictionary<TKey, TValue> innerDictionary;
        private List<QueuedDictionaryAccess<TKey>> statistics;
        private QueuedDictionaryAccessComparer<TKey> statisticsComparer;
        private Dictionary<TKey, QueuedDictionaryAccess<TKey>> statisticsLookup;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedDictionary{TKey, TValue}"/> class.
        /// </summary>
        public QueuedDictionary()
            : this(1000, QueuedDictionaryAccessCompareMode.LastAccessDateAscending)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="maximumSize">The maximum number of elements to allow the dictionary to hold.</param>
        /// <param name="evictionMode">The mode to use when determining which items to evict during an eviction.</param>
        public QueuedDictionary(int maximumSize, QueuedDictionaryAccessCompareMode evictionMode)
        {
            if (maximumSize < 1)
            {
                throw new ArgumentException("maximumSize must be greater than 0.", "maximumSize");
            }

            this.maximumSize = maximumSize;
            this.evictionMode = evictionMode;
            this.innerDictionary = new Dictionary<TKey, TValue>(maximumSize);
            this.statistics = new List<QueuedDictionaryAccess<TKey>>(maximumSize);
            this.statisticsComparer = new QueuedDictionaryAccessComparer<TKey>(evictionMode);
            this.statisticsLookup = new Dictionary<TKey, QueuedDictionaryAccess<TKey>>();
        }

        /// <summary>
        /// Initializes a new instance of the QueuedDictionary class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to read.</param>
        /// <param name="context">The <see cref="StreamingContext"/> for this serialization.</param>
        protected QueuedDictionary(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info", "info cannot be null.");
            }

            this.maximumSize = info.GetInt32("maximumSize");
            this.innerDictionary = (Dictionary<TKey, TValue>)info.GetValue("innerDictionary", typeof(Dictionary<TKey, TValue>));
            this.statistics = (List<QueuedDictionaryAccess<TKey>>)info.GetValue("statistics", typeof(List<QueuedDictionaryAccess<TKey>>));
            this.evictionMode = (QueuedDictionaryAccessCompareMode)info.GetValue("evictionMode", typeof(QueuedDictionaryAccessCompareMode));
            this.statisticsComparer = new QueuedDictionaryAccessComparer<TKey>(this.evictionMode);
            this.statisticsLookup = new Dictionary<TKey, QueuedDictionaryAccess<TKey>>();

            foreach (QueuedDictionaryAccess<TKey> access in this.statistics)
            {
                this.statisticsLookup[access.Key] = access;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a batch of items in the dictionary are evicted.
        /// </summary>
        public event EventHandler<QueuedDictionaryEventArgs> Evicted;

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the number of elements contained in the dictionary.
        /// </summary>
        public virtual int Count
        {
            get { return this.innerDictionary.Count; }
        }

        /// <summary>
        /// Gets or sets the eviction mode to use when determining which items
        /// to evict when <see cref="MaximumSize"/> has been reached.
        /// </summary>
        public QueuedDictionaryAccessCompareMode EvictionMode
        {
            get
            {
                return this.evictionMode;
            }

            set
            {
                this.evictionMode = this.statisticsComparer.Mode = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the dictionary has a fixed-size.
        /// </summary>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the dictionary is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the collection is syncronized.
        /// </summary>
        public bool IsSynchronized
        {
            get { return ((IDictionary)this.innerDictionary).IsSynchronized; }
        }

        /// <summary>
        /// Gets a collection containing the keys in the dictionary.
        /// </summary>
        ICollection IDictionary.Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        /// <summary>
        /// Gets a collection containing the keys in the dictionary.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        /// <summary>
        /// Gets or sets the maximum number of elements to allow in the dictionary
        /// before invoking the eviction policy.
        /// </summary>
        public int MaximumSize
        {
            get
            {
                return this.maximumSize;
            }

            set
            {
                if (value < 1)
                {
                    throw new ArgumentException("value must be greater than 0.", "value");
                }

                bool changed = this.maximumSize != value;
                this.maximumSize = value;

                if (changed)
                {
                    this.Evict();
                }
            }
        }

        /// <summary>
        /// Gets an object that can be used to syncronize access to the collection.
        /// </summary>
        public object SyncRoot
        {
            get { return ((IDictionary)this.innerDictionary).SyncRoot; }
        }

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        ICollection IDictionary.Values
        {
            get { return this.innerDictionary.Values; }
        }

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return this.innerDictionary.Values; }
        }

        #endregion

        #region Protected Instance Properties

        /// <summary>
        /// Gets the concrete dictionary used to store the dictionary contents.
        /// </summary>
        protected IDictionary<TKey, TValue> InnerDictionary
        {
            get { return this.innerDictionary; }
        }

        /// <summary>
        /// Gets the sorted list of access statistics.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It's fine.")]
        protected IList<QueuedDictionaryAccess<TKey>> Statistics
        {
            get { return this.statistics; }
        }

        /// <summary>
        /// Gets the comparer to use when sorting access statistics.
        /// </summary>
        protected QueuedDictionaryAccessComparer<TKey> StatisticsComparer
        {
            get { return this.statisticsComparer; }
        }

        /// <summary>
        /// Gets a collection used for direct lookup of access statistics.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "It's fine.")]
        protected IDictionary<TKey, QueuedDictionaryAccess<TKey>> StatisticsLookup
        {
            get { return this.statisticsLookup; }
        }

        #endregion

        #region Public Instance Indexers

        /// <summary>
        /// Gets or sets the element with the given key.
        /// </summary>
        /// <param name="key">The key to get or set the element for.</param>
        /// <returns>The element identified by the given key.</returns>
        public virtual object this[object key]
        {
            get
            {
                return this[(TKey)key];
            }

            set
            {
                this[(TKey)key] = (TValue)value;
            }
        }

        /// <summary>
        /// Gets or sets the element with the given key.
        /// </summary>
        /// <param name="key">The key to get or set the element for.</param>
        /// <returns>The element identified by the given key.</returns>
        public virtual TValue this[TKey key]
        {
            get
            {
                QueuedDictionaryAccess<TKey> access = this.StatisticsLookup[key];
                access.AccessCount++;
                access.LastAccessDate = DateTime.Now;
                return this.InnerDictionary[key];
            }

            set
            {
                this.Add(key, value);
            }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Adds an element with the given key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        public virtual void Add(object key, object value)
        {
            this.Add((TKey)key, (TValue)value);
        }

        /// <summary>
        /// Adds the given item to the dictionary.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Adds an element with the given key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        public virtual void Add(TKey key, TValue value)
        {
            if (!this.InnerDictionary.ContainsKey(key))
            {
                QueuedDictionaryAccess<TKey> access = new QueuedDictionaryAccess<TKey>(key);
                this.Statistics.Add(access);
                this.StatisticsLookup[key] = access;
            }
            else
            {
                QueuedDictionaryAccess<TKey> access = this.StatisticsLookup[key];
                access.AccessCount++;
                access.LastAccessDate = DateTime.Now;
            }

            this.InnerDictionary[key] = value;
            this.Evict();
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public virtual void Clear()
        {
            this.InnerDictionary.Clear();
            this.Statistics.Clear();
            this.StatisticsLookup.Clear();
        }

        /// <summary>
        /// Gets a value indicating whether the given key exists in the dictionary.
        /// </summary>
        /// <param name="key">The key to check the existence of.</param>
        /// <returns>True if the key exists, false otherwise.</returns>
        public virtual bool Contains(object key)
        {
            return this.ContainsKey((TKey)key);
        }

        /// <summary>
        /// Gets a value indicating whether the given item exists in the dictionary.
        /// </summary>
        /// <param name="item">The item to check the existence of.</param>
        /// <returns>True if the item exists, false otherwise.</returns>
        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (this.InnerDictionary.ContainsKey(item.Key))
            {
                object itemValue = item.Value;
                object thisValue = this.InnerDictionary[item.Key];

                if (thisValue != null)
                {
                    return thisValue.Equals(itemValue);
                }
                else if (itemValue != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether the given key exists in the dictionary.
        /// </summary>
        /// <param name="key">The key to check the existence of.</param>
        /// <returns>True if the key exists, false otherwise.</returns>
        public virtual bool ContainsKey(TKey key)
        {
            return this.InnerDictionary.ContainsKey(key);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at the given array index.
        /// </summary>
        /// <param name="array">The array to copy elements to.</param>
        /// <param name="index">The index in the array to begin copying.</param>
        public virtual void CopyTo(Array array, int index)
        {
            ((IDictionary)this.InnerDictionary).CopyTo(array, index);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at the given array index.
        /// </summary>
        /// <param name="array">The array to copy elements to.</param>
        /// <param name="arrayIndex">The index in the array to begin copying.</param>
        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary)this.InnerDictionary).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that enumerates through the collection.
        /// </summary>
        /// <returns>An enumerator that enumerates through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerDictionary.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that enumerates through the collection.
        /// </summary>
        /// <returns>An enumerator that enumerates through the collection.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)this.InnerDictionary).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that enumerates through the collection.
        /// </summary>
        /// <returns>An enumerator that enumerates through the collection.</returns>
        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.InnerDictionary.GetEnumerator();
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info", "info cannot be null.");
            }

            info.AddValue("maximumSize", this.maximumSize);
            info.AddValue("evictionMode", this.evictionMode);
            info.AddValue("innerDictionary", this.innerDictionary);
            info.AddValue("statistics", this.statistics);
        }

        /// <summary>
        /// Removes the element with the specified key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public virtual void Remove(object key)
        {
            this.Remove((TKey)key);
        }

        /// <summary>
        /// Removes the first occurrence of the given item.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item.Key);
        }

        /// <summary>
        /// Removes the element with the specified key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True if the key was found, false otherwise.</returns>
        public virtual bool Remove(TKey key)
        {
            if (this.Contains(key))
            {
                if (this.InnerDictionary.Remove(key))
                {
                    QueuedDictionaryAccess<TKey> access = this.StatisticsLookup[key];
                    this.StatisticsLookup.Remove(key);
                    this.Statistics.Remove(access);
                    this.SortStatistics();

                    return true;
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key
        /// </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>True if the dictionary contains the specified key, false otherwise.</returns>
        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            if (this.InnerDictionary.TryGetValue(key, out value))
            {
                QueuedDictionaryAccess<TKey> access = this.StatisticsLookup[key];
                access.AccessCount++;
                access.LastAccessDate = DateTime.Now;
                return true;
            }

            return false;
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Executes an eviction batch and evicts items in the dictionary until it reaches <see cref="MaximumSize"/> or less.
        /// </summary>
        /// <returns>The number of items evicted.</returns>
        protected virtual int Evict()
        {
            List<QueuedDictionaryEventArgsItem> evicted = new List<QueuedDictionaryEventArgsItem>();
            int count = this.Count, max = this.MaximumSize, total = count - max;

            if (count > max)
            {
                this.SortStatistics();
            }

            while (count > max)
            {
                QueuedDictionaryAccess<TKey> access = this.Statistics[0];
                this.Statistics.RemoveAt(0);
                evicted.Add(new QueuedDictionaryEventArgsItem(access.Key, this.InnerDictionary[access.Key], new QueuedDictionaryAccess<TKey>(access)));
                this.InnerDictionary.Remove(access.Key);

                count--;
            }

            if (this.Evicted != null)
            {
                this.Evicted(this, new QueuedDictionaryEventArgs(evicted));
            }

            return total;
        }

        /// <summary>
        /// Sorts this instance's <see cref="Statistics"/> using the current <see cref="StatisticsComparer"/>.
        /// </summary>
        protected virtual void SortStatistics()
        {
            this.statistics.Sort(this.StatisticsComparer);
        }

        #endregion
    }
}