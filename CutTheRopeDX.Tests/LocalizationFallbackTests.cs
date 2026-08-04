using CutTheRopeDX.Helpers;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LocalizationFallbackTests
    {
        [Fact]
        public void GetStringKnownKeyReturnsLocalizedText()
        {
            Assert.Equal("Level", LocalizationManager.GetString("LEVEL", "en"));
        }

        [Fact]
        public void GetStringUnknownKeyReturnsKeyVerbatim()
        {
            Assert.Equal("hi world", LocalizationManager.GetString("hi world", "en"));
        }

        [Fact]
        public void GetStringUnknownKeyInUnknownLanguageReturnsKeyVerbatim()
        {
            Assert.Equal("My Test Level", LocalizationManager.GetString("My Test Level", "de"));
        }

        [Fact]
        public void GetStringEmptyKeyReturnsEmpty()
        {
            Assert.Equal(string.Empty, LocalizationManager.GetString("", "en"));
            Assert.Equal(string.Empty, LocalizationManager.GetString(null, "en"));
        }

        [Fact]
        public void HasStringRemainsTheExistenceTest()
        {
            Assert.True(LocalizationManager.HasString("LEVEL"));
            Assert.False(LocalizationManager.HasString("hi world"));
        }
    }
}
