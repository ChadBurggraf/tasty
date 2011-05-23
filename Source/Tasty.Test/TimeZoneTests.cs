//-----------------------------------------------------------------------
// <copyright file="TimeZoneTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Web;

    /// <summary>
    /// Time zone tests.
    /// </summary>
    [TestClass]
    public class TimeZoneTests
    {
        /// <summary>
        /// Faile time zone from 0, 0 lat/lon tests.
        /// </summary>
        [TestMethod]
        public void TimeZoneFailFromZero()
        {
            TimeZoneCallResult result = TimeZoneRequest.Make(0, 0);
            Assert.AreNotEqual(TimeZoneCallStatus.Success, result.Status);
        }

        /// <summary>
        /// From coordinates tests.
        /// </summary>
        [TestMethod]
        public void TimeZoneFromCoordinates()
        {
            TimeZoneCallResult result = TimeZoneRequest.Make(33.41m, -111.94m);
            Assert.AreEqual(TimeZoneCallStatus.Success, result.Status);
            Assert.IsTrue(result.TimeZone == "America/Phoenix");
        }

        /// <summary>
        /// Parse failed request tests.
        /// </summary>
        [TestMethod]
        public void TimeZoneParseFailedRequest()
        {
            TimeZoneCallResult result = TimeZoneRequest.Make(-999999m, -99999m);
            Assert.AreEqual(TimeZoneCallStatus.Unknown, result.Status);

            QueryString query = new QueryString();
            query.Add("lat", "not a latitude");
            query.Add("lng", "not a longitude");

            TimeZoneRequest request = new TimeZoneRequest(query);
            TimeZoneResponse response = request.GetResponse();

            Assert.IsTrue(response.Status == TimeZoneCallStatus.InvalidParameter || response.Status == TimeZoneCallStatus.NotFound);
        }
    }
}
