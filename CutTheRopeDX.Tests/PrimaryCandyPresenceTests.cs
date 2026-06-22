using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class PrimaryCandyPresenceTests
    {
        [Theory]
        [InlineData(false)] // candy present
        [InlineData(true)]  // candy gone (eaten/lost)
        public void WholeCandy_FollowsNoCandyFlag(bool noCandy)
        {
            // Split halves are irrelevant when not split.
            bool present = PrimaryCandyPresence.AnyPresent(false, noCandy, true, true);
            Assert.Equal(!noCandy, present);
        }

        [Fact]
        public void SplitCandy_IgnoresAlwaysTrueSingletonNoCandy()
        {
            // During a split the singleton noCandy is always true; presence must come from the halves.
            Assert.True(PrimaryCandyPresence.AnyPresent(true, noCandy: true, noCandyLeft: false, noCandyRight: true));
            Assert.True(PrimaryCandyPresence.AnyPresent(true, noCandy: true, noCandyLeft: true, noCandyRight: false));
            Assert.True(PrimaryCandyPresence.AnyPresent(true, noCandy: true, noCandyLeft: false, noCandyRight: false));
        }

        [Fact]
        public void SplitCandy_BothHalvesGone_IsAbsent()
        {
            Assert.False(PrimaryCandyPresence.AnyPresent(true, noCandy: true, noCandyLeft: true, noCandyRight: true));
        }
    }
}
