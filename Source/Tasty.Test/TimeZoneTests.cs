using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Web;

namespace Tasty.Test
{
    [TestClass]
    public class TimeZoneTests
    {
        [TestMethod]
        public void TimeZone_GetTimeZoneFromCoordinates()
        {
            TimeZoneCallResult result = TimeZoneRequest.Make(33.41M, -111.94M);
            Assert.IsTrue(result.Status == TimeZoneCallStatus.Success);
            Assert.IsTrue(result.TimeZone == "America/Phoenix");
        }

        [TestMethod()]
        public void TimeZone_ParseFailedRequest()
        {
            TimeZoneCallResult result = TimeZoneRequest.Make(-999999M, -99999M);
            Assert.IsTrue(result.Status == TimeZoneCallStatus.NotFound);

            QueryString query = new QueryString();
            query.Add("lat", "not a latitude");
            query.Add("lng", "not a longitude");

            TimeZoneRequest request = new TimeZoneRequest(query);
            TimeZoneResponse response = request.GetResponse();

            Assert.IsTrue(response.Status == TimeZoneCallStatus.InvalidParameter);
        }
    }
}
