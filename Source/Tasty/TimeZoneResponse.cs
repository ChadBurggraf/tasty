//-----------------------------------------------------------------------
// <copyright file="TimeZoneResponse.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
//     Adapted from code by Jason Sukut, copyright (c) 2010 Jason Sukut.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Represents the response to a <see cref="TimeZoneRequest"/>.
    /// </summary>
    public class TimeZoneResponse
    {
        /// <summary>
        /// Initializes a new instance of the TimeZoneResponse class.
        /// </summary>
        public TimeZoneResponse()
        {
            this.Status = TimeZoneCallStatus.Unknown;
        }

        /// <summary>
        /// Gets the status of the request call.
        /// </summary>
        public TimeZoneCallStatus Status { get; private set; }

        /// <summary>
        /// Gets the response timezone.
        /// </summary>
        public string TimeZone { get; private set; }

        /// <summary>
        /// Creates a <see cref="TimeZoneResponse"/> from the given XML stream.
        /// </summary>
        /// <param name="stream">The stream to create the response from.</param>
        /// <returns>The created response.</returns>
        public static TimeZoneResponse FromXml(Stream stream)
        {
            TimeZoneResponse response = new TimeZoneResponse() { Status = TimeZoneCallStatus.Success };

            XPathDocument doc = new XPathDocument(stream);
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator iterator = nav.Select("//status");

            if (iterator.MoveNext())
            {
                string status = iterator.Current.GetAttribute("value", String.Empty);
                response.Status = (TimeZoneCallStatus)Enum.Parse(typeof(TimeZoneCallStatus), status);

                if (!Enum.IsDefined(typeof(TimeZoneCallStatus), response.Status))
                {
                    response.Status = TimeZoneCallStatus.Unknown;
                }
            }

            if (response.Status == TimeZoneCallStatus.Success)
            {
                iterator = nav.Select("//timezoneId");

                if (iterator.MoveNext())
                {
                    response.TimeZone = iterator.Current.Value.Trim();
                }
            }

            return response;
        }
    }
}
