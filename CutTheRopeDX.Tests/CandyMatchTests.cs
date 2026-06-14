using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyMatchTests
    {
        [Theory]
        [InlineData("first", "first", true)]
        [InlineData("First", "first", true)]
        [InlineData(" second ", "second", true)]
        [InlineData("first", "second", false)]
        public void Matches_ComparesKeysCaseInsensitively(string a, string b, bool expected)
        {
            Assert.Equal(expected, CandyMatch.Matches(a, b));
        }

        [Theory]
        [InlineData(null, "first")]
        [InlineData("first", null)]
        [InlineData(null, null)]
        public void Matches_NullKeyNeverMatches(string a, string b)
        {
            Assert.False(CandyMatch.Matches(a, b));
        }
    }
}
