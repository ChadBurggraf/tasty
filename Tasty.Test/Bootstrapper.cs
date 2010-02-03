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
        internal static string TestCreateDropDatabaseConnectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString.SplitConnectionString().Without("initial catalog").ToConnectionString();
        internal static string TestDatabaseConnectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
        internal static string TestDatabaseName = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString.SplitConnectionString()["initial catalog"];
        internal static string TestDatabaseFilesPath = ConfigurationManager.AppSettings["TestDatabaseFilesPath"];
        internal static string TestDatabaseUserName = "TastyTestUser";
        internal static string TestDatabaseUserPassword = "tastypassword1234";

        public static void CreateTestDatabase()
        {
            SchemaUpgradeService.DropDatabase(TestCreateDropDatabaseConnectionString, TestDatabaseName, TestDatabaseUserName);
            SchemaUpgradeService.CreateDatabase(TestCreateDropDatabaseConnectionString, TestDatabaseName, TestDatabaseFilesPath, TestDatabaseUserName, TestDatabaseUserPassword);

            var jobStoreAssembly = Assembly.GetAssembly(typeof(IJobStore));

            using (Stream stream = jobStoreAssembly.GetManifestResourceStream("Tasty.Jobs.Sql.TastyJobs-SqlServersql.sql"))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    var commands = new SchemaUpgradeCommandSet(sr.ReadToEnd(), jobStoreAssembly.GetName().Version, true);

                    using (SqlConnection connection = new SqlConnection(TestDatabaseConnectionString))
                    {
                        connection.Open();
                        SchemaUpgradeService.ExecuteCommandSet(commands, connection);
                    }
                }
            }
        }
    }
}
