using CutTheRopeDX.Helpers;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LocalizationFallbackTests
    {
        [Fact]
        public void GetString_KnownKey_ReturnsLocalizedText()
        {
            Assert.Equal("Level", LocalizationManager.GetString("LEVEL", "en"));
        }

        [Fact]
        public void GetString_UnknownKey_ReturnsKeyVerbatim()
        {
            Assert.Equal("hi world", LocalizationManager.GetString("hi world", "en"));
        }

        [Fact]
        public void GetString_UnknownKeyInUnknownLanguage_ReturnsKeyVerbatim()
        {
            Assert.Equal("My Test Level", LocalizationManager.GetString("My Test Level", "de"));
        }

        [Fact]
        public void GetString_EmptyKey_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, LocalizationManager.GetString("", "en"));
            Assert.Equal(string.Empty, LocalizationManager.GetString(null, "en"));
        }

        [Fact]
        public void HasString_RemainsTheExistenceTest()
        {
            Assert.True(LocalizationManager.HasString("LEVEL"));
            Assert.False(LocalizationManager.HasString("hi world"));
        }
    }
}
