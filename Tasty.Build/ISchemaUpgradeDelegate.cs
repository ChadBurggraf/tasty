using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Tasty.Build
{
    /// <summary>
    /// Delegate for providing meta-data information about a database being upgraded
    /// with a <see cref="SchemaUpgrade"/> process.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Well, it is a delegate isn't it?")]
    public interface ISchemaUpgradeDelegate
    {
        /// <summary>
        /// Gets a string of SQL commands that represent a single step
        /// in the upgrade process. The SQL should represent the upgrade
        /// to the given version from the version immediately preceding it.
        /// </summary>
        /// <param name="forVersion">The version to get the upgrade commands for.</param>
        /// <returns>A string of SQL commands.</returns>
        SchemaUpgradeCommandSetResult GetCommandSet(Version forVersion);

        /// <summary>
        /// Gets the current version the target database is at.
        /// </summary>
        /// <returns>The target database's current version.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Could require significant non-performant operations.")]
        Version GetCurrentVersion();

        /// <summary>
        /// Gets the version to upgrade the target database to.
        /// </summary>
        /// <returns>The target database's destination version.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Could require significant non-performant operations.")]
        Version GetTargetVersion();

        /// <summary>
        /// Gets a collection of individual version steps that represent the path
        /// for upgrading the target database from one version to another.
        /// </summary>
        /// <param name="currentVersion">The version the database is being upgraded from.</param>
        /// <param name="targetVersion">The version the database is being upgraded to.</param>
        /// <returns>A collection of versions.</returns>
        IEnumerable<Version> GetUpgradePath(Version currentVersion, Version targetVersion);

        /// <summary>
        /// Marks the target database as successfully upgraded for the given version.
        /// This method is called after each discreet upgrade step is performed successfully.
        /// </summary>
        /// <param name="forVersion">The version to mark the database for.</param>
        void MarkAsUpgraded(Version forVersion);
    }
}
