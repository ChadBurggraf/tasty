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
        [TestMethod]
        public void Schema_CreateDatabase()
        {
            SchemaUpgradeService.DropDatabase(Bootstrapper.TestCreateDropDatabaseConnectionString, Bootstrapper.TestDatabaseName, Bootstrapper.TestDatabaseUserName);
            SchemaUpgradeService.CreateDatabase(Bootstrapper.TestCreateDropDatabaseConnectionString, Bootstrapper.TestDatabaseName, Bootstrapper.TestDatabaseFilesPath, Bootstrapper.TestDatabaseUserName, Bootstrapper.TestDatabaseUserPassword);

        }

        [TestMethod]
        public void Schema_DropDatabase()
        {
            SchemaUpgradeService.CreateDatabase(Bootstrapper.TestCreateDropDatabaseConnectionString, Bootstrapper.TestDatabaseName, Bootstrapper.TestDatabaseFilesPath, Bootstrapper.TestDatabaseUserName, Bootstrapper.TestDatabaseUserPassword);
            SchemaUpgradeService.DropDatabase(Bootstrapper.TestCreateDropDatabaseConnectionString, Bootstrapper.TestDatabaseName, Bootstrapper.TestDatabaseUserName);
        }
    }
}
