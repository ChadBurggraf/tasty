//-----------------------------------------------------------------------
// <copyright file="GeocodeException.cs" company="Tasty Codes">
//     Copyright (c) 2010 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty.Geocode
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// Exception thrown when geocode requests fail.
    /// </summary>
    [Serializable]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "The spelling is correct.")]
    public class GeocodeException : Exception
    {
        #region Private Fields

        private const string DefaultMessage = "An unsuccessful, unrecoverable response was received from the geocode service.";
        private GeocodeRequestAddress address;
        private Uri requestUri;
        private string responseName;
        private GeocodeResposeStatusCode responseStatusCode;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the GeocodeException class.
        /// </summary>
        public GeocodeException() 
            : this(DefaultMessage) 
        { 
        }
        
        /// <summary>
        /// Initializes a new instance of the GeocodeException class.
        /// </summary>
        /// <param name="message">A message describing the exception.</param>
        public GeocodeException(string message) 
            : base(message) 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the GeocodeException class.
        /// </summary>
        /// <param name="message">A message describing the exception.</param>
        /// <param name="innerException">The inner exception that caused this exception to be thrown.</param>
        public GeocodeException(string message, Exception innerException)
            : base(message, innerException) 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the GeocodeException class.
        /// </summary>
        /// <param name="request">The request that caused the exception to be thrown.</param>
        /// <param name="response">The response that caused the exception to be thrown.</param>
        public GeocodeException(GeocodeRequest request, GeocodeResponse response)
            : this()
        {
            if (request != null)
            {
                this.address = request.Address;
                this.requestUri = request.RequestUri;
            }

            if (response != null)
            {
                this.responseName = response.Name;
                this.responseStatusCode = response.Status.Code;
            }
        }

        /// <summary>
        /// Initializes a new instance of the GeocodeException class.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds object data about the exception.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        protected GeocodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Instance Properties

        /// <summary>
        /// Gets the address that caused the exception to be thrown.
        /// </summary>
        public GeocodeRequestAddress Address 
        { 
            get { return this.address; } 
        }

        /// <summary>
        /// Gets the request URI that caused the exception to be thrown.
        /// </summary>
        public Uri RequestUri 
        { 
            get { return this.requestUri; } 
        }

        /// <summary>
        /// Gets the name of the response that caused the exception to be thrown.
        /// </summary>
        public string ResponseName 
        { 
            get { return this.responseName; } 
        }

        /// <summary>
        /// Gets the response status code that caused the exception to be thrown.
        /// </summary>
        public GeocodeResposeStatusCode ResponseStatusCode 
        { 
            get { return this.responseStatusCode; } 
        }

        #endregion

        #region Public Instance Methods

        /// <summary>
        /// Sets the given SerializationInfo with data about the exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds data about the exception.</param>
        /// <param name="context">The StreamingContext that holds contextual information about the source or destination.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("address", this.address);
            info.AddValue("requestUri", this.requestUri);
            info.AddValue("responseName", this.responseName);
            info.AddValue("responseStatusCode", this.responseStatusCode);
        }

        #endregion
    }
}