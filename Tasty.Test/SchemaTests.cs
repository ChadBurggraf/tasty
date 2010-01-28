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
            SchemaUpgrade.DropDatabase(connectionString, testDatabaseName, testDatabaseUserName);
            SchemaUpgrade.CreateDatabase(connectionString, testDatabaseName, testDatabaseFilesPath, testDatabaseUserName, testDatabaseUserPassword);
        }

        [TestMethod]
        public void Schema_DropDatabase()
        {
            SchemaUpgrade.CreateDatabase(connectionString, testDatabaseName, testDatabaseFilesPath, testDatabaseUserName, testDatabaseUserPassword);
            SchemaUpgrade.DropDatabase(connectionString, testDatabaseName, testDatabaseUserName);
        }
    }
}
