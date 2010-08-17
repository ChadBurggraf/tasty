//-----------------------------------------------------------------------
// <copyright file="X509CertificateWhitelistValidator.cs" company="Tasty Codes">
//     Copyright (c) 2010 Tasty Codes.
//     Adapted from code by Davide Icardi, Copyright (c) Davide Icardi 2007.
//     The original can be found at http://www.codeproject.com/KB/WCF/wcfcertificates.aspx
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Extends <see cref="X509CertificateValidator"/> to validate X509 certificates against a whitelist.
    /// </summary>
    public class X509CertificateWhitelistValidator : X509CertificateValidator
    {
        private IList<X509Certificate2> whitelist;

        /// <summary>
        /// Initializes a new instance of the X509CertificateWhitelistValidator class.
        /// </summary>
        public X509CertificateWhitelistValidator()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509CertificateWhitelistValidator class.
        /// </summary>
        /// <param name="whitelist">The certificate whitelist to validate against.</param>
        public X509CertificateWhitelistValidator(IEnumerable<X509Certificate2> whitelist)
            : this()
        {
            if (whitelist != null)
            {
                this.whitelist = new List<X509Certificate2>(whitelist);
            }
        }

        /// <summary>
        /// Gets the certificate whitelist to validate against.
        /// </summary>
        public IList<X509Certificate2> Whitelist
        {
            get { return this.whitelist ?? (this.whitelist = new List<X509Certificate2>()); }
        }

        /// <summary>
        /// Validates the X509 certificate.
        /// </summary>
        /// <param name="certificate">The X509 certificate to validate.</param>
        public override void Validate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate", "certificate cannot be null.");
            }

            foreach (var cert in this.Whitelist)
            {
                if (cert.Equals(certificate))
                {
                    return;
                }
            }

            X509CertificateValidator.PeerOrChainTrust.Validate(certificate);
        }
    }
}
