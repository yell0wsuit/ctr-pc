using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// A snail and a bubble cannot coexist on one candy: while a snail is actively riding, any
    /// bubble on that candy pops (Experiments reference). Judged per candy so a bubble on
    /// another candy is untouched.
    /// </summary>
    public class SnailBubblePopTests
    {
        [Fact]
        public void ShouldPopTrueWhileAnActiveSnailRidesABubbledCandy()
        {
            Assert.True(SnailBubblePop.ShouldPop(snailActive: true, ridesACandy: true, candyHasBubble: true));
        }

        [Fact]
        public void ShouldPopFalseWhenTheCandyHasNoBubble()
        {
            Assert.False(SnailBubblePop.ShouldPop(snailActive: true, ridesACandy: true, candyHasBubble: false));
        }

        [Fact]
        public void ShouldPopFalseForAnInactiveSnail()
        {
            Assert.False(SnailBubblePop.ShouldPop(snailActive: false, ridesACandy: true, candyHasBubble: true));
        }

        [Fact]
        public void ShouldPopFalseWhenRidingNothing()
        {
            Assert.False(SnailBubblePop.ShouldPop(snailActive: true, ridesACandy: false, candyHasBubble: true));
        }
    }
}
