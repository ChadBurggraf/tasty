//-----------------------------------------------------------------------
// <copyright file="GeocodeResponse.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;

    /// <summary>
    /// Represents a response to a geocode request.
    /// </summary>
    [DataContract]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeResponse
    {
        private static readonly Type[] KnownTypes = new Type[] 
        {
            typeof(GeocodeResponse),
            typeof(GeocodePlacemark),
            typeof(GeocodePoint),
            typeof(GeocodeLatLonBox),
            typeof(GeocodeExtendedData),
            typeof(GeocodeAddressDetails),
            typeof(GeocodeCountry),
            typeof(GeocodeAdministrativeArea),
            typeof(GeocodeLocality),
            typeof(GeocodeThoroughfare),
            typeof(GeocodePostalCode),
            typeof(GeocodeResponseStatus),
            typeof(GeocodeResposeStatusCode)
        };

        /// <summary>
        /// Gets or sets the name of the response.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the placemarks returned by the response.
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Serialization property.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The casing is consistent with the API.")]
        public GeocodePlacemark[] Placemark { get; set; }

        /// <summary>
        /// Gets or sets the status of the response.
        /// </summary>
        [DataMember]
        public GeocodeResponseStatus Status { get; set; }

        /// <summary>
        /// Deserializes a GeocodeResponse from a string of JSON.
        /// </summary>
        /// <param name="json">The JSON to deserialize the response from.</param>
        /// <returns>A GeocodeResponse instance.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Acronym.")]
        public static GeocodeResponse FromJson(string json)
        {
            using (Stream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Position = 0;

                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GeocodeResponse), KnownTypes);
                    return (GeocodeResponse)serializer.ReadObject(stream);
                }
            }
        }
    }
}
