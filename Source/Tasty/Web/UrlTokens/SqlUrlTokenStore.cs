//-----------------------------------------------------------------------
// <copyright file="SqlUrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using Tasty.Configuration;

    /// <summary>
    /// Base class for <see cref="IUrlTokenStore"/> implementors that use a connection string to connect to a database.
    /// </summary>
    public abstract class SqlUrlTokenStore
    {
        /// <summary>
        /// Initializes a new instance of the SqlUrlTokenStore class.
        /// </summary>
        protected SqlUrlTokenStore()
            : this(TastySettings.GetConnectionStringFromMetadata(TastySettings.Section.UrlTokens.Metadata))
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlUrlTokenStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        protected SqlUrlTokenStore(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets the connection string to use when connecting to the database.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Ensures that a connection string is configured.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected void EnsureConnectionString()
        {
            if (String.IsNullOrEmpty(this.ConnectionString))
            {
                string message = null;
                string connectionStringName = null;
                var keyValueElement = TastySettings.Section.UrlTokens.Metadata["ConnectionStringName"];

                if (keyValueElement != null)
                {
                    connectionStringName = keyValueElement.Value;
                }

                if (!String.IsNullOrEmpty(connectionStringName))
                {
                    message = String.Format(CultureInfo.InvariantCulture, "You've specified that the current URL token store should use the connection string named \"{0}\", but there is either no connection string configured with that name or it is empty.", connectionStringName);
                }
                else
                {
                    message = String.Format(
                        CultureInfo.InvariantCulture,
                        "Please configure the name of the connection string to use for the {0} under /configuration/tasty/urlTokens/metadata/add[key=\"ConnectionStringName\"].",
                        GetType().FullName);
                }

                throw new InvalidOperationException(message);
            }
        }
    }
}
