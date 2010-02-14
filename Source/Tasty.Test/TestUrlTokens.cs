using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Tasty.Web.UrlTokens;

namespace Tasty.Test
{
    [DataContract(Namespace = UrlToken.XmlNamespace)]
    internal class TestIdUrlToken : UrlToken
    {
        public TestIdUrlToken()
        {
            Id = Guid.NewGuid();
        }

        public override int ExpiryHours
        {
            get { return 1; }
        }

        [DataMember]
        public Guid Id { get; set; }
    }
}
