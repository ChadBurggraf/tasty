//-----------------------------------------------------------------------
// <copyright file="SchemaUpgradeService.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Build
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    
    /// <summary>
    /// Provides a simple SQL schema upgrade service.
    /// </summary>
    public class SchemaUpgradeService
    {
        #region Public Fields

        /// <summary>
        /// Gets a regular expression that can be used to mach a version number.
        /// </summary>
        public const string VersionNumberExpression = @"\d+?\.\d+?(\.\d+?|\.\d+?\.\d+?)?\.sql$";

        #endregion

        #region Private Fields

        private string connectionString;
        private int? commandTimeout;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the SchemaUpgradeService class.
        /// </summary>
        /// <param name="connectionString">A connection string to the database to upgrade.</param>
        /// <param name="upgradeDelegate">An <see cref="ISchemaUpgradeDelegate"/> that will provide meta-data information about the target database.</param>
        public SchemaUpgradeService(string connectionString, ISchemaUpgradeDelegate upgradeDelegate)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString", "connectionString must have a value.");
            }

            this.connectionString = connectionString;
            this.UpgradeDelegate = upgradeDelegate;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the timeout, in seconds, to set for each individual command
        /// during the upgrade process.
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return (int)(this.commandTimeout ?? (this.commandTimeout = 60)); // Default to 1 minute.
            }

            set
            {
                this.commandTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets an <see cref="ISchemaUpgradeDelegate"/> that provides
        /// meta-data information about the database being upgraded.
        /// </summary>
        public ISchemaUpgradeDelegate UpgradeDelegate { get; set; }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Creates a new database on the server defined by the given connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the server.</param>
        /// <param name="databaseName">The name of the database to create.</param>
        /// <param name="filesPath">The path to the directory where the database files will be located.</param>
        public static void CreateDatabase(string connectionString, string databaseName, string filesPath)
        {
            CreateDatabase(connectionString, databaseName, filesPath, null, null);
        }

        /// <summary>
        /// Creates a new database on the server defined by the given connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the server.</param>
        /// <param name="databaseName">The name of the database to create.</param>
        /// <param name="filesPath">The path to the directory where the database files will be located.</param>
        /// <param name="databaseUser">The name of the server login and database user to create for accessing the database, or null if not applicable.</param>
        /// <param name="databaseUserPassword">The password of the server login and database user to create for accessing the database, or null if not applicable.</param>
        public static void CreateDatabase(string connectionString, string databaseName, string filesPath, string databaseUser, string databaseUserPassword)
        {
            EnsureConnectionStringCatalog(connectionString, databaseName);

            filesPath = !String.IsNullOrEmpty(filesPath) ? filesPath : Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            if (!Path.IsPathRooted(filesPath))
            {
                filesPath = Path.Combine(Environment.CurrentDirectory, filesPath);
            }

            if (filesPath.EndsWith(@"\", StringComparison.Ordinal))
            {
                filesPath = filesPath.Substring(0, filesPath.Length - 1);
            }

            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            bool createUser = !String.IsNullOrEmpty(databaseUser) && !String.IsNullOrEmpty(databaseUserPassword);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    string createSql = GetResourceAsText("Tasty.Build.Sql.CreateDatabase.sql");

                    command.CommandText = String.Format(CultureInfo.InvariantCulture, createSql, databaseName, filesPath);
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }

                if (createUser)
                {
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        string loginSql = GetResourceAsText("Tasty.Build.Sql.CreateDatabaseLogin.sql");

                        command.CommandType = CommandType.Text;
                        command.CommandText = String.Format(CultureInfo.InvariantCulture, loginSql, databaseName, databaseUser, databaseUserPassword);
                        command.ExecuteNonQuery();
                    }
                }
            }

            if (createUser)
            {
                SqlConnection.ClearAllPools();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    connection.ChangeDatabase(databaseName);

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        string userSql = GetResourceAsText("Tasty.Build.Sql.CreateDatabaseUser.sql");

                        command.CommandType = CommandType.Text;
                        command.CommandText = String.Format(CultureInfo.InvariantCulture, userSql, databaseUser);
                        command.ExecuteNonQuery();
                    }
                }
            }

            SqlConnection.ClearAllPools();
        }

        /// <summary>
        /// Gets a value indicating whether the given database exists for the server
        /// at the given connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the server.</param>
        /// <param name="databaseName">The name of the database to check for.</param>
        /// <returns>True if the database exists, false otherwise.</returns>
        public static bool DatabaseExists(string connectionString, string databaseName)
        {
            EnsureConnectionStringCatalog(connectionString, databaseName);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = String.Format(CultureInfo.InvariantCulture, GetResourceAsText("Tasty.Build.Sql.DatabaseExists.sql"), databaseName);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable results = new DataTable() { Locale = CultureInfo.InvariantCulture };
                        adapter.Fill(results);

                        return 0 < (int)results.Rows[0]["Exists"];
                    }
                }
            }
        }

        /// <summary>
        /// Drops the given database.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database server.</param>
        /// <param name="databaseName">The name of the database to drop.</param>
        public static void DropDatabase(string connectionString, string databaseName)
        {
            DropDatabase(connectionString, databaseName, null);
        }

        /// <summary>
        /// Drops the given database, optionally dropping the givin database login as well.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database server.</param>
        /// <param name="databaseName">The name of the database to drop.</param>
        /// <param name="databaseUser">The name of the login to drop, if applicable.</param>
        public static void DropDatabase(string connectionString, string databaseName, string databaseUser)
        {
            EnsureConnectionStringCatalog(connectionString, databaseName);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    string dropSql = GetResourceAsText("Tasty.Build.Sql.DropDatabase.sql");

                    command.CommandType = CommandType.Text;
                    command.CommandText = String.Format(CultureInfo.InvariantCulture, dropSql, databaseName);
                    command.ExecuteNonQuery();
                }

                if (!String.IsNullOrEmpty(databaseUser))
                {
                    string dropUserSql = GetResourceAsText("Tasty.Build.Sql.DropLogin.sql");

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = String.Format(CultureInfo.InvariantCulture, dropUserSql, databaseUser);
                        command.ExecuteNonQuery();
                    }
                }
            }

            SqlConnection.ClearAllPools();
        }

        /// <summary>
        /// Generates a SQL installation script by searching the given directory for SQL script files with
        /// names that correspond to version numbers and concatenating them together into a file created
        /// at the given output path.
        /// </summary>
        /// <param name="fromVersion">The lower-bound version number to restrict the resulting script to (exclusive).</param>
        /// <param name="toVersion">The upper-bound version number to restrict the resulting script to (inclusive).</param>
        /// <param name="searchPath">The directory to search for scripts within (the search with be recursive).</param>
        /// <param name="outputPath">The path of the output file to create or overwrite.</param>
        public static void GenerateInstallScript(Version fromVersion, Version toVersion, string searchPath, string outputPath)
        {
            GenerateInstallScript(fromVersion, toVersion, searchPath, outputPath, null);
        }

        /// <summary>
        /// Generates a SQL installation script by searching the given directory for SQL script files with
        /// names that correspond to version numbers and concatenating them together into a file created
        /// at the given output path.
        /// </summary>
        /// <param name="fromVersion">The lower-bound version number to restrict the resulting script to (exclusive).</param>
        /// <param name="toVersion">The upper-bound version number to restrict the resulting script to (inclusive).</param>
        /// <param name="searchPath">The directory to search for scripts within (the search with be recursive).</param>
        /// <param name="outputPath">The path of the output file to create or overwrite.</param>
        /// <param name="onGenerating">A function that should be called for each script that is found and added to the output.</param>
        public static void GenerateInstallScript(Version fromVersion, Version toVersion, string searchPath, string outputPath, Action<Version, string> onGenerating)
        {
            if (String.IsNullOrEmpty(searchPath))
            {
                throw new ArgumentNullException("searchPath", "searchPath must have a value.");
            }

            if (String.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentNullException("outputPath", "outputPath must have a value.");
            }

            if (fromVersion != null && toVersion != null && toVersion <= fromVersion)
            {
                throw new ArgumentException("If both fromVersion and toVersion are supplied, toVersion must be greater than fromVersion.", "toVersion");
            }

            var allScripts = from s in Directory.GetFiles(searchPath, "*.sql", SearchOption.AllDirectories)
                             where Regex.IsMatch(s, SchemaUpgradeService.VersionNumberExpression)
                             select new
                             {
                                 FilePath = s,
                                 VersionNumber = new Version(Path.GetFileNameWithoutExtension(s))
                             };

            if (fromVersion != null)
            {
                allScripts = allScripts.Where(s => s.VersionNumber > fromVersion);
            }

            if (toVersion != null)
            {
                allScripts = allScripts.Where(s => s.VersionNumber <= toVersion);
            }

            using (FileStream ofs = File.Create(outputPath))
            {
                using (StreamWriter sw = new StreamWriter(ofs))
                {
                    foreach (var script in allScripts.OrderBy(s => s.VersionNumber))
                    {
                        using (FileStream ifs = File.OpenRead(script.FilePath))
                        {
                            using (StreamReader sr = new StreamReader(ifs))
                            {
                                sw.WriteLine(sr.ReadToEnd());
                            }
                        }

                        sw.WriteLine("GO");

                        if (onGenerating != null)
                        {
                            string relativePath = script.FilePath.Substring(searchPath.Length);

                            if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                            {
                                relativePath = relativePath.Substring(1);
                            }

                            onGenerating(script.VersionNumber, relativePath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes the given <see cref="SchemaUpgradeCommandSet"/> on the given <see cref="SqlConnection"/>.
        /// </summary>
        /// <param name="commandSet">The <see cref="SchemaUpgradeCommandSet"/> containing the SQL commands to execute.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to execute the command set on.</param>
        public static void ExecuteCommandSet(SchemaUpgradeCommandSet commandSet, SqlConnection connection)
        {
            ExecuteCommandSet(commandSet, connection, null);
        }

        /// <summary>
        /// Executes the given <see cref="SchemaUpgradeCommandSet"/> on the given <see cref="SqlConnection"/>.
        /// </summary>
        /// <param name="commandSet">The <see cref="SchemaUpgradeCommandSet"/> containing the SQL commands to execute.</param>
        /// <param name="connection">The <see cref="SqlConnection"/> to execute the command set on.</param>
        /// <param name="commandTimeout">The timeout to set for each command, or null to use the default value.</param>
        public static void ExecuteCommandSet(SchemaUpgradeCommandSet commandSet, SqlConnection connection, int? commandTimeout)
        {
            SqlTransaction transaction = null;

            if (commandSet.RunInTransaction)
            {
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted, "Tasty.Build.SchemaUpgradeService");
            }

            try
            {
                foreach (string command in commandSet.Commands)
                {
                    using (SqlCommand sqlCommand = connection.CreateCommand())
                    {
                        sqlCommand.Transaction = transaction;
                        sqlCommand.CommandType = CommandType.Text;
                        sqlCommand.CommandText = command;

                        if (commandTimeout != null)
                        {
                            sqlCommand.CommandTimeout = commandTimeout.Value;
                        }

                        sqlCommand.ExecuteNonQuery();
                    }
                }

                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                }

                throw;
            }
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Performs the upgraded process.
        /// </summary>
        public void Upgrade()
        {
            Version current = this.UpgradeDelegate.GetCurrentVersion();
            Version target = this.UpgradeDelegate.GetTargetVersion();

            if (current < target)
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    IEnumerable<Version> path = this.UpgradeDelegate.GetUpgradePath(current, target);

                    foreach (Version version in path)
                    {
                        SchemaUpgradeCommandSetResult commands = this.UpgradeDelegate.GetCommandSet(version);
                        SchemaUpgradeCommandSet commandSet = new SchemaUpgradeCommandSet(commands.Sql, version, commands.RunInTransaction);

                        ExecuteCommandSet(commandSet, connection, this.CommandTimeout);

                        this.UpgradeDelegate.MarkAsUpgraded(version);
                    }
                }
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Ensures that the given connection string and database name both contain values, and that
        /// the connection string does not point to the given database as its initial catalog.
        /// </summary>
        /// <param name="connectionString">The connection string to ensure.</param>
        /// <param name="databaseName">The database name to ensure.</param>
        private static void EnsureConnectionStringCatalog(string connectionString, string databaseName)
        {
            if (String.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString", "connectionString must have a value.");
            }

            if (String.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException("databaseName", "databaseName must have a value.");
            }

            var connectionStringCollection = connectionString.SplitConnectionString();

            if (databaseName.Equals(connectionStringCollection["initial catalog"], StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("connectionString must point to an initial catalog other than the specified database.", connectionString);
            }
        }

        /// <summary>
        /// Gets an embedded resource file's contents as a string.
        /// </summary>
        /// <param name="resourceName">The name of the resource to get.</param>
        /// <returns>An embedded resource's text contents.</returns>
        private static string GetResourceAsText(string resourceName)
        {
            using (StreamReader reader = new StreamReader(Assembly.GetAssembly(typeof(SchemaUpgradeService)).GetManifestResourceStream(resourceName)))
            {
                return reader.ReadToEnd();
            }
        }

        #endregion
    }
}
