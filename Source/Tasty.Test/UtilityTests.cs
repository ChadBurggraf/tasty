using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tasty.Test
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void Utility_CopyProperties()
        {
            var source1 = new CopyPropertiesTest() { Name = Guid.NewGuid().ToString(), Number = 42, Date = DateTime.UtcNow };
            var dest1 = new CopyPropertiesTest() { Name = "some string", Number = 0, Date = DateTime.MinValue };

            source1.CopyProperties(dest1);

            Assert.AreEqual(source1.Name, dest1.Name);
            Assert.AreEqual(source1.Number, dest1.Number);
            Assert.AreEqual(source1.Date, dest1.Date);
        }
    }

    internal class CopyPropertiesTest
    {
        public DateTime Date { get; set; }

        public string Name { get; set; }

        public int Number { get; set; }
    }
}
