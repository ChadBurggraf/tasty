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
    using System.Reflection;
    
    /// <summary>
    /// Provides a simple SQL schema upgrade service.
    /// </summary>
    public class SchemaUpgradeService
    {
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
                throw new ArgumentException("connectionString must point to an initial catalog other than the database being created.", connectionString);
            }

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
                throw new ArgumentException("connectionString must point to an initial catalog other than the database being dropped.", connectionString);
            }

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
