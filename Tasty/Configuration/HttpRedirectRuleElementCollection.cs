//-----------------------------------------------------------------------
// <copyright file="HttpRedirectRuleElementCollection.cs" company="Chad Burggraf">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    
    /// <summary>
    /// Represents a collection of <see cref="HttpRedirectRuleElement"/>s in the configuration.
    /// </summary>
    public class HttpRedirectRuleElementCollection : ConfigurationElementCollection, ICollection<HttpRedirectRuleElement>
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether the collection is read only.
        /// </summary>
        public new bool IsReadOnly
        {
            get { return base.IsReadOnly(); }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Adds a configuation element to the collection.
        /// </summary>
        /// <param name="item">The element to add.</param>
        public void Add(HttpRedirectRuleElement item)
        {
            BaseAdd(item);
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        /// <summary>
        /// Gets a value indicating whether the given configuration element is
        /// present in the collection. Determines presence based on the equality
        /// of the element keys.
        /// </summary>
        /// <param name="item">The element to check the existence of.</param>
        /// <returns>True if the element exists in the collection, false otherwise.</returns>
        public bool Contains(HttpRedirectRuleElement item)
        {
            return 0 < (from e in this
                        where item.Pattern.Equals(e.Pattern, StringComparison.OrdinalIgnoreCase)
                        select e).Count();
        }

        /// <summary>
        /// Copies the collection to the given array, starting at the given index.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The index in the target array to begin copying at.</param>
        public void CopyTo(HttpRedirectRuleElement[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the given element from the collection.
        /// </summary>
        /// <param name="item">The element to remove.</param>
        /// <returns>True if the element was found and removed, false otherwise.</returns>
        public bool Remove(HttpRedirectRuleElement item)
        {
            bool removed = this.Contains(item);

            if (removed)
            {
                BaseRemove(this.GetElementKey(item));
            }

            return removed;
        }

        /// <summary>
        /// Gets the enumerator used to enumerate over the collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public new IEnumerator<HttpRedirectRuleElement> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return (HttpRedirectRuleElement)BaseGet(i);
            }
        }

        #endregion

        #region Protected Instance Methods

        /// <summary>
        /// Creates a new element.
        /// </summary>
        /// <returns>The created element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new HttpRedirectRuleElement();
        }

        /// <summary>
        /// Gets the unique key of the given element.
        /// </summary>
        /// <param name="element">The element to get the key of.</param>
        /// <returns>The given element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((HttpRedirectRuleElement)element).Pattern;
        }

        #endregion
    }
}
