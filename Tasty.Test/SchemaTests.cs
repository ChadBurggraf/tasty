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
            SchemaUpgradeService.DropDatabase(Bootstrapper.CreateDropConnectionString, Bootstrapper.DatabaseName, Bootstrapper.DatabaseUserName);
            SchemaUpgradeService.CreateDatabase(Bootstrapper.CreateDropConnectionString, Bootstrapper.DatabaseName, Bootstrapper.DatabaseFilesPath, Bootstrapper.DatabaseUserName, Bootstrapper.DatabaseUserPassword);
        }

        [TestMethod]
        public void Schema_DropDatabase()
        {
            SchemaUpgradeService.CreateDatabase(Bootstrapper.CreateDropConnectionString, Bootstrapper.DatabaseName, Bootstrapper.DatabaseFilesPath, Bootstrapper.DatabaseUserName, Bootstrapper.DatabaseUserPassword);
            SchemaUpgradeService.DropDatabase(Bootstrapper.CreateDropConnectionString, Bootstrapper.DatabaseName, Bootstrapper.DatabaseUserName);
        }
    }
}
