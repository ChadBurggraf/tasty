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
        private Dictionary<string, string> metadata;

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

        [DataMember]
        public Dictionary<string, string> Metadata
        {
            get { return this.metadata ?? (this.metadata = new Dictionary<string, string>()); }
            set { this.metadata = value; }
        }
    }
}
