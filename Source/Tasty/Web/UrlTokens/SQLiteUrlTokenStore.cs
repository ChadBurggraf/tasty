//-----------------------------------------------------------------------
// <copyright file="SQLiteUrlTokenStore.cs" company="Tasty Codes">
//     Copyright (c) 2012 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Web.UrlTokens
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using System.Text.RegularExpressions;
    using Tasty.Configuration;

    /// <summary>
    /// Implements <see cref="IUrlTokenStore"/> to persist <see cref="IUrlToken"/>s to SQLite.
    /// </summary>
    public class SQLiteUrlTokenStore : SqlUrlTokenStore, IUrlTokenStore
    {
        /// <summary>
        /// Initializes a new instance of the SQLiteUrlTokenStore class.
        /// </summary>
        public SQLiteUrlTokenStore()
            : this(TastySettings.GetConnectionStringFromMetadata(TastySettings.Section.UrlTokens.Metadata))
        {
        }

        /// <summary>
        /// Initializes a new instance of the SQLiteUrlTokenStore class.
        /// </summary>
        /// <param name="connectionString">The connection string to use when connecting to the database.</param>
        public SQLiteUrlTokenStore(string connectionString)
        {
            this.ConnectionString = connectionString;
            this.EnsureConnectionString();

            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(this.ConnectionString);
            builder.DataSource = ResolveDatabasePath(builder.DataSource);
            this.ConnectionString = builder.ToString();

            EnsureDatabase(builder.DataSource);
        }

        /// <summary>
        /// Cleans all expired token records from the store.
        /// </summary>
        public void CleanExpiredUrlTokens()
        {
            const string Sql = "DELETE FROM [TastyUrlToken] WHERE [Expires] < @Now;";

            using (SQLiteConnection connection = new SQLiteConnection(this.ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;
                    command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

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
            const string Sql = "INSERT INTO [TastyUrlToken]([Key],[Type],[Data],[Created],[Expires]) VALUES(@Key,@Type,@Data,@Created,@Expires);";

            using (SQLiteConnection connection = new SQLiteConnection(this.ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;

                    command.Parameters.AddWithValue("@Key", record.Key);
                    command.Parameters.AddWithValue("@Type", record.StorageTypeName);
                    command.Parameters.AddWithValue("@Data", record.Data);
                    command.Parameters.AddWithValue("@Created", record.Created);
                    command.Parameters.AddWithValue("@Expires", record.Expires);

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
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "key must have a value.");
            }

            const string Sql = "SELECT * FROM [TastyUrlToken] WHERE [Key] = @Key;";

            using (SQLiteConnection connection = new SQLiteConnection(this.ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = Sql;
                    command.Parameters.AddWithValue("@Key", key);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UrlTokenRecord()
                            {
                                Key = (string)reader["Key"],
                                TokenType = Type.GetType((string)reader["Type"]),
                                Data = (string)reader["Data"],
                                Created = ((DateTime)reader["Created"]).ToUniversalTime(),
                                Expires = ((DateTime)reader["Expires"]).ToUniversalTime()
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Ensures the SQLite database exists, creating it if not.
        /// </summary>
        /// <param name="path">The path of the SQLite database to ensure.</param>
        private static void EnsureDatabase(string path)
        {
            if (!File.Exists(path))
            {
                string sql;
                Stream stream = null;

                try
                {
                    stream = typeof(SQLiteUrlTokenStore).Assembly.GetManifestResourceStream("Tasty.Web.UrlTokens.Sql.TastyUrlTokens-SQLite.sql");

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        stream = null;
                        sql = reader.ReadToEnd();
                    }
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }

                using (SQLiteConnection connection = new SQLiteConnection(string.Concat("data source=", path, ";journal mode=Off;synchronous=Off;version=3")))
                {
                    connection.Open();

                    using (SQLiteCommand command = connection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the given database path pulled from a Web.config file.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>The resolved path.</returns>
        private static string ResolveDatabasePath(string path)
        {
            const string DataDirectory = "|DataDirectory|";
            path = (path ?? string.Empty).Trim();

            if (!string.IsNullOrEmpty(path))
            {
                int dataDirectoryIndex = path.IndexOf(DataDirectory, StringComparison.OrdinalIgnoreCase);

                if (dataDirectoryIndex > -1)
                {
                    path = path.Substring(dataDirectoryIndex + DataDirectory.Length);
                    path = Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data"), path);
                }
                else
                {
                    Regex exp = new Regex(string.Concat("[", Regex.Escape(new string(Path.GetInvalidPathChars())), "]"));

                    if (!exp.IsMatch(path) && !Path.IsPathRooted(path))
                    {
                        string dir = TastySettings.Section.ElementInformation.Source;
                        dir = !string.IsNullOrEmpty(dir) ? Path.GetDirectoryName(dir) : null;

                        if (!string.IsNullOrEmpty(dir))
                        {
                            path = Path.Combine(dir, path);
                        }
                        else
                        {
                            path = Path.GetFullPath(path);
                        }
                    }
                }
            }
            else
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
            }

            return path;
        }
    }
}
