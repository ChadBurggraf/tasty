using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tasty.Configuration;
using Tasty.Web;

namespace Tasty.Test
{
    [TestClass]
    public class UrlTokenTests
    {
        [TestMethod]
        public void UrlTokens_CurrentTokenStore()
        {
            Assert.IsNotNull(UrlTokenStore.Current);
        }

        internal static void Store_CreateUrlToken(IUrlTokenStore store)
        {
            IUrlToken token = new TestIdUrlToken();
            string key = token.GenerateKey();

            store.CreateUrlToken(new UrlTokenRecord()
            {
                Created = DateTime.UtcNow,
                Data = token.Serialize(),
                Expires = DateTime.UtcNow.AddHours(1),
                Key = key,
                TokenType = token.GetType()
            });

            Assert.IsNotNull(store.GetUrlToken(key));
        }

        internal static void Store_ExpireUrlToken(IUrlTokenStore store)
        {
            IUrlToken token = new TestIdUrlToken();
            string key = token.GenerateKey();

            store.CreateUrlToken(new UrlTokenRecord()
            {
                Created = DateTime.UtcNow,
                Data = token.Serialize(),
                Expires = DateTime.UtcNow.AddHours(-1),
                Key = key,
                TokenType = token.GetType()
            });

            store.CleanExpiredUrlTokens();

            Assert.IsNull(store.GetUrlToken(key));
        }

        internal static void Store_GetUrlToken(IUrlTokenStore store)
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

            store.CreateUrlToken(record1);
            store.CreateUrlToken(record2);

            SqlDateTimeAssert.AreEqual(record1.Created, store.GetUrlToken(record1.Key).Created);
            Assert.AreEqual(record1.Data, store.GetUrlToken(record1.Key).Data);
            SqlDateTimeAssert.AreEqual(record1.Expires, store.GetUrlToken(record1.Key).Expires);

            SqlDateTimeAssert.AreEqual(record2.Created, store.GetUrlToken(record2.Key).Created);
            Assert.AreEqual(record2.Data, store.GetUrlToken(record2.Key).Data);
            SqlDateTimeAssert.AreEqual(record2.Expires, store.GetUrlToken(record2.Key).Expires);
        }
    }
}
