using CutTheRopeDX.Helpers;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class ParsingHelpersTests
    {
        [Theory]
        [InlineData("12.9", 12)]
        [InlineData("-12.9", -12)]
        [InlineData("42", 42)]
        [InlineData("invalid", 0)]
        [InlineData(null, 0)]
        public void ParseCoordinateIntOrZeroTruncatesDecimalValues(string value, int expected)
        {
            Assert.Equal(expected, ParsingHelpers.ParseCoordinateIntOrZero(value));
        }

        [Theory]
        [InlineData("2147483648")]
        [InlineData("1e100")]
        public void ParseCoordinateIntOrZeroReturnsZeroWhenValueIsOutsideIntRange(string value)
        {
            Assert.Equal(0, ParsingHelpers.ParseCoordinateIntOrZero(value));
        }
    }
}
