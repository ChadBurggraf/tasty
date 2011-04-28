//-----------------------------------------------------------------------
// <copyright file="EmptyMailModel.cs" company="Tasty Codes">
//     Copyright (c) 2011 Chad Burggraf.
// </copyright>
//-----------------------------------------------------------------------

namespace Tasty
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an empty implementation of <see cref="MailModel"/>.
    /// </summary>
    [DataContract(Namespace = MailModel.XmlNamespace)]
    public sealed class EmptyMailModel : MailModel
    {
        /// <summary>
        /// Initializes a new instance of the EmptyMailModel class.
        /// </summary>
        public EmptyMailModel()
            : base()
        {
        }
    }
}
