using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TransportEntryTests
    {
        [Fact]
        public void ShouldEnter_TrueForFreeCandyInRange()
        {
            Assert.True(TransportEntry.ShouldEnter(
                candyPresent: true, alreadyInSock: false, alreadyInBamboo: false,
                inLantern: false, splitActive: false, inRange: true));
        }

        [Fact]
        public void ShouldEnter_FalseWhenAlreadyInTransit()
        {
            Assert.False(TransportEntry.ShouldEnter(true, alreadyInSock: true, false, false, false, true));
            Assert.False(TransportEntry.ShouldEnter(true, false, alreadyInBamboo: true, false, false, true));
        }

        [Fact]
        public void ShouldEnter_FalseWhenInLantern()
        {
            Assert.False(TransportEntry.ShouldEnter(true, false, false, inLantern: true, false, true));
        }

        [Fact]
        public void ShouldEnter_FalseWhenSplitActive()
        {
            // split-candy (twoParts) is handled by the singleton halves, not per-candy transit.
            Assert.False(TransportEntry.ShouldEnter(true, false, false, false, splitActive: true, true));
        }

        [Fact]
        public void ShouldEnter_FalseWhenMissingOrOutOfRange()
        {
            Assert.False(TransportEntry.ShouldEnter(candyPresent: false, false, false, false, false, true));
            Assert.False(TransportEntry.ShouldEnter(true, false, false, false, false, inRange: false));
        }
    }
}
