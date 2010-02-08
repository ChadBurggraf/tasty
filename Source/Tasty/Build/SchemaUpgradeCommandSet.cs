//-----------------------------------------------------------------------
// <copyright file="SchemaUpgradeCommandSet.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents a set of SQL commands corresponding to a specific version number.
    /// </summary>
    public sealed class SchemaUpgradeCommandSet
    {
        /// <summary>
        /// Initializes a new instance of the SchemaUpgradeCommandSet class.
        /// </summary>
        /// <param name="sql">A string of SQL script to create the command set for.</param>
        /// <param name="versionNumber">The version number to create the command set for.</param>
        /// <param name="runInTransaction">A value indicating whether to run this command set in a trasaction.</param>
        public SchemaUpgradeCommandSet(string sql, Version versionNumber, bool runInTransaction)
        {
            if (versionNumber == null)
            {
                throw new ArgumentNullException("versionNumber", "versionNumber must contain a value.");
            }

            this.Commands = new ReadOnlyCollection<string>((sql ?? String.Empty).SplitSqlCommands());
            this.RunInTransaction = runInTransaction;
            this.VersionNumber = versionNumber;
        }

        /// <summary>
        /// Gets a collection of the individual commands in this command set.
        /// </summary>
        public ReadOnlyCollection<string> Commands { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to run this command set in a trasaction.
        /// Set to false if one or more of the commands in the set are illegal inside of a transaction.
        /// </summary>
        public bool RunInTransaction { get; set; }

        /// <summary>
        /// Gets this command set's version number.
        /// </summary>
        public Version VersionNumber { get; private set; }
    }
}
