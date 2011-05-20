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

    /// <summary>
    /// Represents a dictionary with a size cap, which automatically evicts items
    /// based on the desired access pattern.
    /// </summary>
    /// <typeparam name="TKey">The type of the dictionary's keys.</typeparam>
    /// <typeparam name="TValue">The type of the dictionary's values.</typeparam>
    public class QueuedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        #region Private Fields

        private int maximumSize;
        private Dictionary<TKey, TValue> innerDictionary;
        private List<QueuedDictionaryAccess<TKey>> statistics;
        private QueuedDictionaryAccessComparer<TKey> statisticsComparer;
        private Dictionary<TKey, QueuedDictionaryAccess<TKey>> statisticsLookup;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedDictionary"/> class.
        /// </summary>
        public QueuedDictionary()
            : this(1000, QueuedDictionaryAccessCompareMode.LastAccessDateAscending)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedDictionary"/> class.
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
            this.innerDictionary = new Dictionary<TKey, TValue>(maximumSize);
            this.statistics = new List<QueuedDictionaryAccess<TKey>>(maximumSize);
            this.statisticsComparer = new QueuedDictionaryAccessComparer<TKey>(evictionMode);
            this.statisticsLookup = new Dictionary<TKey, QueuedDictionaryAccess<TKey>>();
        }

        #endregion

        #region Events

        /// <summary>
        /// Event fired when a batch of items in the dictionary are evicted.
        /// </summary>
        public event EventHandler<QueuedDictionaryEventArgs> Evicted;

        #endregion

        #region Public Instance Properties

        public virtual int Count
        {
            get { return this.innerDictionary.Count; }
        }

        public QueuedDictionaryAccessCompareMode EvictionMode
        {
            get
            {
                return this.statisticsComparer.Mode;
            }

            set
            {
                bool changed = this.statisticsComparer.Mode != value;
                this.statisticsComparer.Mode = value;

                if (changed)
                {
                    this.SortStatistics();
                }
            }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsSynchronized
        {
            get { return ((IDictionary)this.innerDictionary).IsSynchronized; }
        }

        ICollection IDictionary.Keys
        {
            get { return this.innerDictionary.Keys; }
        }

        public ICollection<TKey> Keys
        {
            get { return this.innerDictionary.Keys; }
        }

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

        public object SyncRoot
        {
            get { return ((IDictionary)this.innerDictionary).SyncRoot; }
        }

        ICollection IDictionary.Values
        {
            get { return this.innerDictionary.Values; }
        }

        public ICollection<TValue> Values
        {
            get { return this.innerDictionary.Values; }
        }

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

        public virtual TValue this[TKey key]
        {
            get
            {
                QueuedDictionaryAccess<TKey> access = this.StatisticsLookup[key];
                access.AccessCount++;
                access.LastAccessDate = DateTime.Now;
                this.SortStatistics();
                return this.InnerDictionary[key];
            }

            set
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
                this.SortStatistics();
                this.Evict();
            }
        }

        #endregion

        #region Protected Instance Properties

        protected Dictionary<TKey, TValue> InnerDictionary
        {
            get { return this.innerDictionary; }
        }

        protected List<QueuedDictionaryAccess<TKey>> Statistics
        {
            get { return this.statistics; }
        }

        protected QueuedDictionaryAccessComparer<TKey> StatisticsComparer
        {
            get { return this.statisticsComparer; }
        }

        protected Dictionary<TKey, QueuedDictionaryAccess<TKey>> StatisticsLookup
        {
            get { return this.statisticsLookup; }
        }

        #endregion

        #region Public Instance Methods

        public virtual void Add(object key, object value)
        {
            this.Add((TKey)key, (TValue)value);
        }

        public virtual void Add(TKey key, TValue value)
        {
            this[key] = value;
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            this[item.Key] = item.Value;
        }

        public virtual void Clear()
        {
            this.InnerDictionary.Clear();
            this.Statistics.Clear();
            this.StatisticsLookup.Clear();
        }

        public virtual bool Contains(object key)
        {
            return this.ContainsKey((TKey)key);
        }

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

        public virtual bool ContainsKey(TKey key)
        {
            return this.InnerDictionary.ContainsKey(key);
        }

        public virtual void CopyTo(Array array, int index)
        {
            ((IDictionary)this.InnerDictionary).CopyTo(array, index);
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary)this.InnerDictionary).CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.InnerDictionary.GetEnumerator();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)this.InnerDictionary).GetEnumerator();
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.InnerDictionary.GetEnumerator();
        }

        public virtual void Remove(object key)
        {
            throw new NotImplementedException();
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

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

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            if (this.InnerDictionary.TryGetValue(key, out value))
            {
                QueuedDictionaryAccess<TKey> access = this.StatisticsLookup[key];
                access.AccessCount++;
                access.LastAccessDate = DateTime.Now;
                this.SortStatistics();

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
            int index = 0, count = this.Count, max = this.MaximumSize;

            while (count > max)
            {
                QueuedDictionaryAccess<TKey> access = this.Statistics[index];
                this.Statistics.RemoveAt(index);
                evicted.Add(new QueuedDictionaryEventArgsItem(access.Key, this.InnerDictionary[access.Key], new QueuedDictionaryAccess<TKey>(access)));
                this.InnerDictionary.Remove(access.Key);

                count--;
                index++;
            }

            if (this.Evicted != null)
            {
                this.Evicted(this, new QueuedDictionaryEventArgs(evicted));
            }

            return index;
        }

        /// <summary>
        /// Sorts this instance's <see cref="Statistics"/> using the current <see cref="StatisticsComparer"/>.
        /// </summary>
        protected virtual void SortStatistics()
        {
            this.Statistics.Sort(this.StatisticsComparer);
        }

        #endregion
    }
}