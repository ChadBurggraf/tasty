using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tasty.Build
{
    /// <summary>
    /// Represents the results of an <see cref="ISchemaUpgradeDelegate"/> request for 
    /// a string of SQL representing a set of upgrade commands.
    /// </summary>
    public class SchemaUpgradeCommandSetResult
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public SchemaUpgradeCommandSetResult()
        {
            RunInTransaction = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to run the commands in a transaction.
        /// Set to false if one or more of the commands in the set are illegal inside of a transaction.
        /// </summary>
        public bool RunInTransaction { get; set; }

        /// <summary>
        /// Gets or sets the SQL script to run.
        /// </summary>
        public string Sql { get; set; }
    }
}
