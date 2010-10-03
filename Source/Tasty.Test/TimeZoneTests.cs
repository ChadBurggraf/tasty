using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Web;

namespace Tasty.Test
{
    [TestClass]
    public class TimeZoneTests
    {
        [TestMethod]
        public void TimeZone_FailGetTimeZoneFromZero()
        {
            TimeZoneCallResult result = TimeZoneRequest.Make(0, 0);
            Assert.AreNotEqual(TimeZoneCallStatus.Success, result.Status);
        }

        [TestMethod]
        public void TimeZone_GetTimeZoneFromCoordinates()
        {
            TimeZoneCallResult result = TimeZoneRequest.Make(33.41m, -111.94m);
            Assert.AreEqual(TimeZoneCallStatus.Success, result.Status);
            Assert.IsTrue(result.TimeZone == "America/Phoenix");
        }

        [TestMethod()]
        public void TimeZone_ParseFailedRequest()
        {
            TimeZoneCallResult result = TimeZoneRequest.Make(-999999m, -99999m);
            Assert.AreEqual(TimeZoneCallStatus.NotFound, result.Status);

            QueryString query = new QueryString();
            query.Add("lat", "not a latitude");
            query.Add("lng", "not a longitude");

            TimeZoneRequest request = new TimeZoneRequest(query);
            TimeZoneResponse response = request.GetResponse();

            Assert.IsTrue(result.Status == TimeZoneCallStatus.InvalidParameter || result.Status == TimeZoneCallStatus.NotFound);
        }
    }
}
