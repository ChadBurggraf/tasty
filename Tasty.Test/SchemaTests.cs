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
            Bootstrapper.CreateTestDatabase();
        }

        [TestMethod]
        public void Schema_DropDatabase()
        {
            Bootstrapper.DropTestDatabase();
        }
    }
}
