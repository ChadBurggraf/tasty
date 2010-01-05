using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tasty.Http
{
    /// <summary>
    /// Defines the possible HTTP redirect types.
    /// </summary>
    public enum HttpRedirectType
    {
        /// <summary>
        /// Identifies a permanent redirect (301).
        /// </summary>
        Permanent,

        /// <summary>
        /// Identifies a temporary redirect (302).
        /// </summary>
        Temporary = 0
    }
}
