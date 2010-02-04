using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Build;
using Tasty.Jobs;

namespace Tasty.Test
{
    internal static class Bootstrapper
    {
        internal static string ConnectionString, CreateDropConnectionString, DatabaseName, DatabaseFilesPath, DatabaseUserName, DatabaseUserPassword;

        static Bootstrapper()
        {
            var cs = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString.SplitConnectionString();

            ConnectionString = cs.ToConnectionString();
            CreateDropConnectionString = cs.Without("initial catalog").ToConnectionString();
            DatabaseName = cs["initial catalog"];
            DatabaseFilesPath = ConfigurationManager.AppSettings["DatabaseFilesPath"];
            DatabaseUserName = "TastyTestUser";
            DatabaseUserPassword = "tastypassword1234";
        }

        public static void CreateTestDatabase()
        {
            SchemaUpgradeService.DropDatabase(CreateDropConnectionString, DatabaseName, DatabaseUserName);
            SchemaUpgradeService.CreateDatabase(CreateDropConnectionString, DatabaseName, DatabaseFilesPath, DatabaseUserName, DatabaseUserPassword);

            var jobStoreAssembly = Assembly.GetAssembly(typeof(IJobStore));

            using (Stream stream = jobStoreAssembly.GetManifestResourceStream("Tasty.Jobs.Sql.TastyJobs-SqlServersql.sql"))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    var commands = new SchemaUpgradeCommandSet(sr.ReadToEnd(), jobStoreAssembly.GetName().Version, true);

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        SchemaUpgradeService.ExecuteCommandSet(commands, connection);
                    }
                }
            }
        }
    }
}
