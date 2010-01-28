﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Tasty.Http
{
    /// <summary>
    /// Represents a redirect rule element in the configuration.
    /// </summary>
    public class HttpRedirectRuleElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the rule's regular expression pattern.
        /// </summary>
        [ConfigurationProperty("pattern", IsRequired = true, IsKey = true)]
        public string Pattern
        {
            get { return (string)this["pattern"]; }
            set { this["pattern"] = value; }
        }

        /// <summary>
        /// Gets or sets the URL the rule redirects to.
        /// </summary>
        [ConfigurationProperty("redirectsTo", IsRequired = true)]
        public string RedirectsTo
        {
            get { return (string)this["redirectsTo"]; }
            set { this["redirectsTo"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of redirect the rule uses.
        /// </summary>
        [ConfigurationProperty("redirectType", IsRequired = false)]
        public HttpRedirectType RedirectType
        {
            get { return (HttpRedirectType)(this["redirectType"] ?? HttpRedirectType.Temporary); }
            set { this["redirectType"] = value; }
        }
    }
}