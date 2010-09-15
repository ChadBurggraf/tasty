//-----------------------------------------------------------------------
// <copyright file="PostgresUrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Npgsql;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IUrlTokenStore"/> to persist <see cref="IUrlToken"/>s to PostgreSQL.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class PostgresUrlTokenStore : SqlUrlTokenStore, IUrlTokenStore
    {
        /// <summary>
        /// Initializes a new instance of the PostgresUrlTokenStore class.
        /// </summary>
        public PostgresUrlTokenStore()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PostgresUrlTokenStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        public PostgresUrlTokenStore(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Cleans all expired token records from the store.
        /// </summary>
        public void CleanExpiredUrlTokens()
        {
            this.EnsureConnectionString();

            const string Sql = "DELETE FROM \"tasty_url_token\" WHERE \"expires\" < :now";

            using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;
                    command.Parameters.Add(new NpgsqlParameter(":now", DateTime.UtcNow));

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

            const string Sql = "INSERT INTO \"tasty_url_token\"(\"key\",\"type\",\"data\",\"created\",\"expires\") VALUES(:key,:type,:data,:created,:expires)";

            using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;

                    command.Parameters.Add(new NpgsqlParameter(":key", record.Key));
                    command.Parameters.Add(new NpgsqlParameter(":type", record.StorageTypeName));
                    command.Parameters.Add(new NpgsqlParameter(":data", record.Data));
                    command.Parameters.Add(new NpgsqlParameter(":created", record.Created));
                    command.Parameters.Add(new NpgsqlParameter(":expires", record.Expires));

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

            const string Sql = "SELECT * FROM \"tasty_url_token\" WHERE \"key\" = :key";

            using (NpgsqlConnection connection = new NpgsqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;
                    command.Parameters.Add(new NpgsqlParameter(":key", key));

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
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
