//-----------------------------------------------------------------------
// <copyright file="EndpointElementCollection.cs" company="Tasty Codes">
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
    /// Represents a collection of <see cref="EndpointElement"/>s in the configuration.
    /// </summary>
    public class EndpointElementCollection : TastyConfigurationElementCollection<EndpointElement>
    {
        /// <summary>
        /// Gets the element with the given contract name.
        /// </summary>
        /// <param name="contract">The contract name of the element to get.</param>
        /// <returns>The element with the given contract name.</returns>
        public new EndpointElement this[string contract]
        {
            get { return (EndpointElement)BaseGet(contract); }
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains the given item.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the collection contains the item, false otherwise.</returns>
        public override bool Contains(EndpointElement item)
        {
            return 0 < (from e in this
                        where item.Contract.Equals(e.Contract, StringComparison.OrdinalIgnoreCase)
                        select e).Count();
        }

        /// <summary>
        /// Gets the unique key of the given element.
        /// </summary>
        /// <param name="element">The element to get the key of.</param>
        /// <returns>The given element's key.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EndpointElement)element).Contract;
        }
    }
}
