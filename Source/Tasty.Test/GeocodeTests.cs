//-----------------------------------------------------------------------
// <copyright file="GeocodeTests.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Tasty.Configuration;
    using Tasty.Geocode;

    /// <summary>
    /// Geocode tests.
    /// </summary>
    [TestClass]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeTests
    {
        private static readonly GeocodeRequestAddress HomeAddress = new GeocodeRequestAddress()
        {
            Street = "6840 E 2nd St Apt 22",
            City = "Scottsdale",
            State = "AZ",
            PostalCode = "85251"
        };

        private static readonly GeocodePoint HomeCoordinates = new GeocodePoint()
        {
            Coordinates = new decimal[] { -111.93307m, 33.491697m }
        };

        /// <summary>
        /// City tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void GeocodeCity()
        {
            if (!String.IsNullOrEmpty(TastySettings.Section.Geocode.ApiKey))
            {
                GeocodeRequest request = new GeocodeRequest(new GeocodeRequestAddress()
                {
                    City = "Scottsdale",
                    State = "AZ"
                });

                GeocodeResponse response = request.GetResponse();
                Assert.AreEqual<GeocodeResposeStatusCode>(GeocodeResposeStatusCode.Success, response.Status.Code);

                var mark = (from p in response.Placemark
                            orderby p.AddressDetails.Accuracy descending
                            select p).First();

                Assert.IsTrue(mark.AddressDetails.Accuracy >= 4);
            }
        }

        /// <summary>
        /// Make request tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void GeocodeMakeRequest()
        {
            if (!String.IsNullOrEmpty(TastySettings.Section.Geocode.ApiKey))
            {
                GeocodeRequest request = new GeocodeRequest(HomeAddress);
                GeocodeResponse response = request.GetResponse();
                Assert.AreEqual<GeocodeResposeStatusCode>(GeocodeResposeStatusCode.Success, response.Status.Code);
            }
        }

        /// <summary>
        /// Administrative area relative to city tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void GeocodeAdministrativeAreaForCity()
        {
            if (!String.IsNullOrEmpty(TastySettings.Section.Geocode.ApiKey))
            {
                GeocodeRequest request = new GeocodeRequest(new GeocodeRequestAddress()
                {
                    City = "Scottsdale",
                    State = "AZ"
                });

                GeocodeResponse response = request.GetResponse();
                GeocodePlacemark mark = response.Placemark.First();

                Assert.AreEqual("AZ", mark.AddressDetails.Country.AdministrativeArea.AdministrativeAreaName);
                Assert.AreEqual("Scottsdale", mark.AddressDetails.Country.AdministrativeArea.SubAdministrativeArea.Locality.LocalityName);
            }
        }

        /// <summary>
        /// Administrative area relative to HomeAddress tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void GeocodeAdministrativeAreaForHome()
        {
            if (!String.IsNullOrEmpty(TastySettings.Section.Geocode.ApiKey))
            {
                GeocodeRequest request = new GeocodeRequest(HomeAddress);
                GeocodeResponse response = request.GetResponse();

                GeocodePlacemark mark = response.Placemark.First();

                Assert.AreEqual("AZ", mark.AddressDetails.Country.AdministrativeArea.AdministrativeAreaName);
                Assert.AreEqual("Scottsdale", mark.AddressDetails.Country.AdministrativeArea.Locality.LocalityName);
                Assert.AreEqual("6840 E 2nd St #22", mark.AddressDetails.Country.AdministrativeArea.Locality.Thoroughfare.ThoroughfareName);
                Assert.AreEqual("85251", mark.AddressDetails.Country.AdministrativeArea.Locality.PostalCode.PostalCodeNumber);
            }
        }

        /// <summary>
        /// Lat/lon relative to HomeAddress tests.
        /// </summary>
        [TestMethod]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
        public void GeocodetLatLonForHome()
        {
            if (!String.IsNullOrEmpty(TastySettings.Section.Geocode.ApiKey))
            {
                GeocodeRequest request = new GeocodeRequest(HomeAddress);
                GeocodeResponse response = request.GetResponse();

                decimal latDiff = Math.Abs(HomeCoordinates.Latitude.Value - response.Placemark[0].Point.Latitude.Value);
                decimal lonDIff = Math.Abs(HomeCoordinates.Longitude.Value - response.Placemark[0].Point.Longitude.Value);

                Assert.IsTrue(latDiff < 0.001m);
                Assert.IsTrue(lonDIff < 0.001m);
            }
        }
    }
}
