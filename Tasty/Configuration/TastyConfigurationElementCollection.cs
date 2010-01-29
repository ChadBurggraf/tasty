//-----------------------------------------------------------------------
// <copyright file="TastyConfigurationElementCollection.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// Represents the base class for <see cref="ConfigurationElementCollection"/> implementors.
    /// </summary>
    /// <typeparam name="T">The type of the configuration elements contained in the collection.</typeparam>
    public abstract class TastyConfigurationElementCollection<T> : ConfigurationElementCollection, ICollection<T>
        where T : ConfigurationElement
    {
        #region Public Instance Properties

        /// <summary>
        /// Gets a value indicating whether the colleciton is read only.
        /// </summary>
        public virtual new bool IsReadOnly
        {
            get { return true; }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Adds a new item to the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public virtual void Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public virtual void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public abstract bool Contains(T item);

        /// <summary>
        /// Gets an enumerator that can be used to enumerate over the collection.
        /// </summary>
        /// <returns>The collection's enumerator.</returns>
        public virtual new IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return BaseGet(i) as T;
            }
        }

        /// <summary>
        /// Copies the collection to the given array, starting at the given index in the array.
        /// </summary>
        /// <param name="array">The array to copy elements to.</param>
        /// <param name="arrayIndex">The index in the array to start copying at.</param>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the given item from the collection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was found and removed, false otherwise.</returns>
        public virtual bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Creates a new instance of the collection's contained <see cref="ConfigurationElement"/> type.
        /// </summary>
        /// <returns>A new <see cref="ConfigurationElement"/> instance.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return (ConfigurationElement)Activator.CreateInstance(typeof(T));
        }

        #endregion
    }
}