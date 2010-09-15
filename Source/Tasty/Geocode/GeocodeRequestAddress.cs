//-----------------------------------------------------------------------
// <copyright file="GeocodeRequestAddress.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents an address to use in a <see cref="Tasty.Geocode.GeocodeRequest"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeRequestAddress
    {
        /// <summary>
        /// Gets or sets the street number of the address.
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets the value of this instance as a string.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString()
        {
            List<string> parts = new List<string>();

            if (!String.IsNullOrEmpty(this.Street))
            {
                parts.Add(this.Street);
            }

            if (!String.IsNullOrEmpty(this.City))
            {
                parts.Add(this.City);
            }

            if (!String.IsNullOrEmpty(this.State))
            {
                parts.Add(this.State);
            }

            if (!String.IsNullOrEmpty(this.PostalCode))
            {
                parts.Add(this.PostalCode);
            }

            return String.Join(", ", parts.ToArray());
        }
    }
}
