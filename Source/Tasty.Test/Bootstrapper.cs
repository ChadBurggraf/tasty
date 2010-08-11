using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Build;
using Tasty.Web.UrlTokens;

namespace Tasty.Test
{
    [TestClass]
    public static class Bootstrapper
    {
        internal static string ConnectionString, CreateDropConnectionString, DatabaseName, DatabaseFilesPath, DatabaseUserName, DatabaseUserPassword;

        static Bootstrapper()
        {
            var cs = ConfigurationManager.ConnectionStrings["SqlServer"].ConnectionString.SplitConnectionString();

            ConnectionString = cs.ToConnectionString();
            CreateDropConnectionString = cs.Without("initial catalog").ToConnectionString();
            DatabaseName = cs["initial catalog"];
            DatabaseFilesPath = ConfigurationManager.AppSettings["DatabaseFilesPath"];
            DatabaseUserName = "TastyTestUser";
            DatabaseUserPassword = "tastypassword1234";
        }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext textContext)
        {
            //CreateTestDatabase();
        }

        private static void CreateTestDatabase()
        {
            SchemaUpgradeService.DropDatabase(CreateDropConnectionString, DatabaseName, DatabaseUserName);
            SchemaUpgradeService.CreateDatabase(CreateDropConnectionString, DatabaseName, DatabaseFilesPath, DatabaseUserName, DatabaseUserPassword);

            //RunEmbeddedSql(Assembly.GetAssembly(typeof(IJobStore)), "Tasty.Jobs.Sql.TastyJobs-SqlServer.sql");
            RunEmbeddedSql(Assembly.GetAssembly(typeof(IUrlTokenStore)), "Tasty.Web.UrlTokens.Sql.TastyUrlTokens-SqlServer.sql");
        }

        private static void DropTestDatabase()
        {
            SchemaUpgradeService.CreateDatabase(CreateDropConnectionString, DatabaseName, DatabaseFilesPath, DatabaseUserName, DatabaseUserPassword);
            SchemaUpgradeService.DropDatabase(CreateDropConnectionString, DatabaseName, DatabaseUserName);
        }

        public static void EnsureTestDatabase()
        {
            if (!SchemaUpgradeService.DatabaseExists(CreateDropConnectionString, DatabaseName))
            {
                CreateTestDatabase();
            }
        }

        private static void RunEmbeddedSql(Assembly assembly, string name)
        {
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    var commands = new SchemaUpgradeCommandSet(sr.ReadToEnd(), assembly.GetName().Version, true);

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
