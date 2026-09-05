using System;
using System.Text;

using CutTheRopeDX.Browser;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class ContentCacheTests
    {
        [Fact]
        public void WritingAfterFreezeIsRejected()
        {
            ContentCache cache = new();
            cache.Set("maps/pack1.xml", Encoding.UTF8.GetBytes("<map/>"));
            cache.Freeze();

            _ = Assert.Throws<InvalidOperationException>(
                () => cache.Set("maps/pack2.xml", Encoding.UTF8.GetBytes("<map/>")));
        }

        [Fact]
        public void ReadsReturnStoredBytesAfterFreeze()
        {
            ContentCache cache = new();
            cache.Set("maps/pack1.xml", Encoding.UTF8.GetBytes("<map/>"));
            cache.Freeze();

            Assert.Equal("<map/>", Encoding.UTF8.GetString(cache.Read("maps/pack1.xml")));
        }

        [Fact]
        public void ContainsKeyDistinguishesLoadedFromMissing()
        {
            ContentCache cache = new();
            cache.Set("maps/pack1.xml", Encoding.UTF8.GetBytes("<map/>"));

            Assert.True(cache.ContainsKey("maps/pack1.xml"));
            Assert.False(cache.ContainsKey("maps/pack2.xml"));
        }

        [Fact]
        public void ReadingAMissingKeyNamesTheContentAndTheFix()
        {
            ContentCache cache = new();
            cache.Freeze();

            InvalidOperationException failure =
                Assert.Throws<InvalidOperationException>(() => cache.Read("maps/absent.xml"));

            Assert.Contains("maps/absent.xml", failure.Message, StringComparison.Ordinal);
            Assert.Contains("build_web_content.py", failure.Message, StringComparison.Ordinal);
        }
    }
}
