using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class TransportEntryTests
    {
        [Fact]
        public void ShouldEnterTrueForFreeCandyInRange()
        {
            Assert.True(TransportEntry.ShouldEnter(
                candyPresent: true, alreadyInTransit: false,
                inLantern: false, splitActive: false, inRange: true));
        }

        [Fact]
        public void ShouldEnterFalseWhenAlreadyInTransit()
        {
            // Sock and bamboo transit are one lifecycle state, so one flag closes the gate for both.
            Assert.False(TransportEntry.ShouldEnter(true, alreadyInTransit: true, false, false, true));
        }

        [Fact]
        public void ShouldEnterFalseWhenInLantern()
        {
            Assert.False(TransportEntry.ShouldEnter(true, false, inLantern: true, splitActive: false, inRange: true));
        }

        [Fact]
        public void ShouldEnterFalseWhenSplitActive()
        {
            // A split candy has no whole body to swallow; its halves have to merge before a
            // transporter can take it, which the body-role table enforces on the scene side.
            Assert.False(TransportEntry.ShouldEnter(true, false, false, splitActive: true, inRange: true));
        }

        [Fact]
        public void ShouldEnterFalseWhenMissingOrOutOfRange()
        {
            Assert.False(TransportEntry.ShouldEnter(candyPresent: false, false, false, false, true));
            Assert.False(TransportEntry.ShouldEnter(true, false, false, false, inRange: false));
        }
    }
}
