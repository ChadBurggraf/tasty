//-----------------------------------------------------------------------
// <copyright file="ServiceModelElement.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents the service model configuration element.
    /// </summary>
    public class ServiceModelElement : ConfigurationElement
    {
        /// <summary>
        /// Gets the endpoint collection.
        /// </summary>
        [ConfigurationProperty("endpoints")]
        public EndpointElementCollection Endpoints
        {
            get { return (EndpointElementCollection)(this["endpoints"] ?? (this["endpoints"] = new EndpointElementCollection())); }
        }

        /// <summary>
        /// Gets the service collection.
        /// </summary>
        [ConfigurationProperty("services")]
        public ServiceElementCollection Services
        {
            get { return (ServiceElementCollection)(this["services"] ?? (this["services"] = new ServiceElementCollection())); }
        }
    }
}
