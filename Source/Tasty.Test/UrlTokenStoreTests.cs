using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Web.UrlTokens;

namespace Tasty.Test
{
    public abstract class UrlTokenStoreTests
    {
        protected UrlTokenStoreTests(IUrlTokenStore store)
        {
            this.Store = store;
        }

        protected IUrlTokenStore Store { get; private set; }

        protected virtual void CurrentTokenStore()
        {
            Assert.IsNotNull(UrlTokenStore.Current);
        }

        protected virtual void CreateUrlToken()
        {
            if (this.Store != null)
            {
                TestIdUrlToken token = new TestIdUrlToken();
                token.Metadata["Alpha"] = "One";
                token.Metadata["Bravo"] = "Two";

                string key = token.GenerateKey();

                this.Store.CreateUrlToken(new UrlTokenRecord()
                {
                    Created = DateTime.UtcNow,
                    Data = token.Serialize(),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Key = key,
                    TokenType = token.GetType()
                });

                Assert.IsNotNull(this.Store.GetUrlToken(key));
            }
        }

        protected virtual void ExpireUrlToken()
        {
            if (this.Store != null)
            {
                IUrlToken token = new TestIdUrlToken();
                string key = token.GenerateKey();

                this.Store.CreateUrlToken(new UrlTokenRecord()
                {
                    Created = DateTime.UtcNow,
                    Data = token.Serialize(),
                    Expires = DateTime.UtcNow.AddHours(-1),
                    Key = key,
                    TokenType = token.GetType()
                });

                this.Store.CleanExpiredUrlTokens();

                Assert.IsNull(this.Store.GetUrlToken(key));
            }
        }

        protected virtual void GetUrlToken()
        {
            if (this.Store != null)
            {
                IUrlToken token1 = new TestIdUrlToken();
                IUrlToken token2 = new TestIdUrlToken();

                UrlTokenRecord record1 = new UrlTokenRecord()
                {
                    Created = DateTime.UtcNow,
                    Data = token1.Serialize(),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Key = token1.GenerateKey(),
                    TokenType = token1.GetType()
                };

                UrlTokenRecord record2 = new UrlTokenRecord()
                {
                    Created = DateTime.UtcNow,
                    Data = token2.Serialize(),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Key = token2.GenerateKey(),
                    TokenType = token2.GetType()
                };

                this.Store.CreateUrlToken(record1);
                this.Store.CreateUrlToken(record2);

                SqlDateTimeAssert.AreEqual(record1.Created, this.Store.GetUrlToken(record1.Key).Created);
                Assert.AreEqual(record1.Data, this.Store.GetUrlToken(record1.Key).Data);
                SqlDateTimeAssert.AreEqual(record1.Expires, this.Store.GetUrlToken(record1.Key).Expires);

                SqlDateTimeAssert.AreEqual(record2.Created, this.Store.GetUrlToken(record2.Key).Created);
                Assert.AreEqual(record2.Data, this.Store.GetUrlToken(record2.Key).Data);
                SqlDateTimeAssert.AreEqual(record2.Expires, this.Store.GetUrlToken(record2.Key).Expires);
            }
        }

        protected virtual void QueryStringUrlTokenUrlProvider()
        {
            TestIdUrlToken source = new TestIdUrlToken();
            var provider = new QueryStringUrlTokenUrlProvider<TestIdUrlToken>() { QueryStringKey = "tc", Url = new Uri("http://tastycodes.com/") };
            Uri url = provider.UrlFromToken(source);

            TestIdUrlToken result = provider.TokenFromUrl(url);

            Assert.IsNotNull(result);
            Assert.AreEqual(source.Id, result.Id);
        }
    }
}
