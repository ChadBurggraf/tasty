//-----------------------------------------------------------------------
// <copyright file="SqlServerUrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IUrlTokenStore"/> to persist <see cref="IUrlToken"/>s to SQL Server.
    /// </summary>
    public class SqlServerUrlTokenStore : SqlUrlTokenStore, IUrlTokenStore
    {
        /// <summary>
        /// Initializes a new instance of the SqlServerUrlTokenStore class.
        /// </summary>
        public SqlServerUrlTokenStore()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SqlServerUrlTokenStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        public SqlServerUrlTokenStore(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Cleans all expired token records from the store.
        /// </summary>
        public void CleanExpiredUrlTokens()
        {
            this.EnsureConnectionString();

            const string Sql = "DELETE FROM [TastyUrlToken] WHERE [Expires] < @Now";

            using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;
                    command.Parameters.Add(new SqlParameter("@Now", DateTime.UtcNow));

                    command.ExecuteNonQuery();
                }
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

                    command.Parameters.Add(new SqlParameter("@Key", record.Key));
                    command.Parameters.Add(new SqlParameter("@Type", record.StorageTypeName));
                    command.Parameters.Add(new SqlParameter("@Data", record.Data));
                    command.Parameters.Add(new SqlParameter("@Created", record.Created));
                    command.Parameters.Add(new SqlParameter("@Expires", record.Expires));

                    command.ExecuteNonQuery();
                }
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
    }
}