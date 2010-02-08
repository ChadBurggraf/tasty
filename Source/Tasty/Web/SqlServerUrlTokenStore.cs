//-----------------------------------------------------------------------
// <copyright file="SqlServerUrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IUrlTokenStore"/> to persist <see cref="IUrlToken"/>s to the SQL Server.
    /// </summary>
    public class SqlServerUrlTokenStore : IUrlTokenStore
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the SqlServerUrlTokenStore class.
        /// </summary>
        public SqlServerUrlTokenStore()
        {
            this.ConnectionString = TastySettings.GetConnectionStringFromMetadata(TastySettings.Section.UrlTokens.Metadata);
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets or sets the connection string to use when connecting to the database.
        /// </summary>
        public string ConnectionString { get; set; }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Cleans all expired token records from the store.
        /// </summary>
        public void CleanExpiredUrlTokens()
        {
            this.EnsureConnectionString();

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                CleanExpiredUrlTokens(connection);
            }
        }

        /// <summary>
        /// Creates a new URL token record.
        /// </summary>
        /// <param name="record">The URL token record to create.</param>
        public void CreateUrlToken(UrlTokenRecord record)
        {
            this.EnsureConnectionString();

            const string Sql = "INSERT INTO [TastyUrlToken]([Key],[Type],[Data],[Created],[Expires]) VALUES(@Key,@Type,@Data,@Created,@Expires)";

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;

                    UrlTokenStore.ParameterizeRecord<SqlCommand, SqlParameter>(record, command);

                    command.ExecuteNonQuery();
                }

                CleanExpiredUrlTokens(connection);
            }
        }

        /// <summary>
        /// Gets a URL token record.
        /// </summary>
        /// <param name="key">The key of the record to get.</param>
        /// <returns>The URL token record identified by the given key.</returns>
        public UrlTokenRecord GetUrlToken(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must have a value.");
            }

            this.EnsureConnectionString();

            const string Sql = "SELECT * FROM [TastyUrlToken] WHERE [Key] = @Key";

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;
                    command.Parameters.Add(new SqlParameter("@Key", key));

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable results = new DataTable() { Locale = CultureInfo.InvariantCulture };
                        adapter.Fill(results);

                        return UrlTokenStore.CreateRecordCollection(results).FirstOrDefault();
                    }
                }
            }
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Cleans all expired token records from the store.
        /// </summary>
        /// <param name="connection">The <see cref="SqlConnection"/> to use when connecting to the database.</param>
        private static void CleanExpiredUrlTokens(SqlConnection connection)
        {
            const string Sql = "DELETE FROM [TastyUrlToken] WHERE [Expires] < @Now";

            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = Sql;
                command.Parameters.Add(new SqlParameter("@Now", DateTime.UtcNow));

                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Private Instance Methods

        /// <summary>
        /// Ensures that a connection string is configured.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"></exception>
        private void EnsureConnectionString()
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
                    message = "Please configure the name of the connection string to use for the Tasty.Web.SqlServerUrlTokenStore under /configuration/tasty/urlTokens/metadata/add[key=\"ConnectionStringName\"].";
                }

                throw new InvalidOperationException(message);
            }
        }

        #endregion
    }
}