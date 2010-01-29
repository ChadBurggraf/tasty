using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Build;

namespace Tasty.Test
{
    [TestClass]
    public class SchemaTests
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["LocalSqlServer"].ConnectionString;
        private static string testDatabaseName = "TastyBuildTest";
        private static string testDatabaseFilesPath = ConfigurationManager.AppSettings["TestDatabaseFilesPath"];
        private static string testDatabaseUserName = "TastyBuildTestUser";
        private static string testDatabaseUserPassword = "tastypassword1234";

        [TestMethod]
        public void Schema_CreateDatabase()
        {
            SchemaUpgradeService.DropDatabase(connectionString, testDatabaseName, testDatabaseUserName);
            SchemaUpgradeService.CreateDatabase(connectionString, testDatabaseName, testDatabaseFilesPath, testDatabaseUserName, testDatabaseUserPassword);
        }

        [TestMethod]
        public void Schema_DropDatabase()
        {
            SchemaUpgradeService.CreateDatabase(connectionString, testDatabaseName, testDatabaseFilesPath, testDatabaseUserName, testDatabaseUserPassword);
            SchemaUpgradeService.DropDatabase(connectionString, testDatabaseName, testDatabaseUserName);
        }
    }
}
