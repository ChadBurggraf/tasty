//-----------------------------------------------------------------------
// <copyright file="X509CertificateWhiteListValidator.cs" company="Tasty Codes">
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
    /// Extends <see cref="X509CertificateValidator"/> to validate X509 certificates against a whiteList.
    /// </summary>
    public class X509CertificateWhiteListValidator : X509CertificateValidator
    {
        private IList<X509Certificate2> whiteList;

        /// <summary>
        /// Initializes a new instance of the X509CertificateWhiteListValidator class.
        /// </summary>
        public X509CertificateWhiteListValidator()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the X509CertificateWhiteListValidator class.
        /// </summary>
        /// <param name="whiteList">The certificate whiteList to validate against.</param>
        public X509CertificateWhiteListValidator(IEnumerable<X509Certificate2> whiteList)
            : this()
        {
            if (whiteList != null)
            {
                this.whiteList = new List<X509Certificate2>(whiteList);
            }
        }

        /// <summary>
        /// Gets the certificate whiteList to validate against.
        /// </summary>
        public IList<X509Certificate2> WhiteList
        {
            get { return this.whiteList ?? (this.whiteList = new List<X509Certificate2>()); }
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

            foreach (var cert in this.WhiteList)
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
