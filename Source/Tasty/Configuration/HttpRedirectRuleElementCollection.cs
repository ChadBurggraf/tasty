//-----------------------------------------------------------------------
// <copyright file="HttpRedirectRuleElementCollection.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
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
    public class HttpRedirectRuleElementCollection : ConfigurationElementCollection<HttpRedirectRuleElement>
    {
        /// <summary>
        /// Gets a value indicating whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public override bool Contains(HttpRedirectRuleElement item)
        {
            return 0 < (from e in this
                        where item.Pattern.Equals(e.Pattern, StringComparison.OrdinalIgnoreCase)
                        select e).Count();
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
    }
}
