using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Tasty.Http
{
    /// <summary>
    /// Defines the tasty.http configuration section.
    /// </summary>
    public class HttpSettings : ConfigurationSection
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        static HttpSettings()
        {
            Section = (HttpSettings)(ConfigurationManager.GetSection("tasty.http") ?? new HttpSettings());
        }

        /// <summary>
        /// Gets the currently configured tasty.http settings section.
        /// </summary>
        public static HttpSettings Section { get; private set; }

        /// <summary>
        /// Gets the configured HTTP redirects collection.
        /// </summary>
        [ConfigurationProperty("redirects", IsRequired = false)]
        public HttpRedirectRuleElementCollection Redirects
        {
            get { return (HttpRedirectRuleElementCollection)(this["redirects"] ?? (this["redirects"] = new HttpRedirectRuleElementCollection())); }
        }
    }
}
