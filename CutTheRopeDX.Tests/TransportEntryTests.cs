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
                candyPresent: true, alreadyInTransit: false,
                inLantern: false, splitActive: false, inRange: true));
        }

        [Fact]
        public void ShouldEnter_FalseWhenAlreadyInTransit()
        {
            // Sock and bamboo transit are one lifecycle state, so one flag closes the gate for both.
            Assert.False(TransportEntry.ShouldEnter(true, alreadyInTransit: true, false, false, true));
        }

        [Fact]
        public void ShouldEnter_FalseWhenInLantern()
        {
            Assert.False(TransportEntry.ShouldEnter(true, false, inLantern: true, splitActive: false, inRange: true));
        }

        [Fact]
        public void ShouldEnter_FalseWhenSplitActive()
        {
            // split-candy (twoParts) is handled by the singleton halves, not per-candy transit.
            Assert.False(TransportEntry.ShouldEnter(true, false, false, splitActive: true, inRange: true));
        }

        [Fact]
        public void ShouldEnter_FalseWhenMissingOrOutOfRange()
        {
            Assert.False(TransportEntry.ShouldEnter(candyPresent: false, false, false, false, true));
            Assert.False(TransportEntry.ShouldEnter(true, false, false, false, inRange: false));
        }
    }
}
