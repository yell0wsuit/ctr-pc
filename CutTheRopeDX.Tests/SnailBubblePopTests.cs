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
        public void ShouldPop_TrueWhileAnActiveSnailRidesABubbledCandy()
        {
            Assert.True(SnailBubblePop.ShouldPop(snailActive: true, ridesACandy: true, candyHasBubble: true));
        }

        [Fact]
        public void ShouldPop_FalseWhenTheCandyHasNoBubble()
        {
            Assert.False(SnailBubblePop.ShouldPop(snailActive: true, ridesACandy: true, candyHasBubble: false));
        }

        [Fact]
        public void ShouldPop_FalseForAnInactiveSnail()
        {
            Assert.False(SnailBubblePop.ShouldPop(snailActive: false, ridesACandy: true, candyHasBubble: true));
        }

        [Fact]
        public void ShouldPop_FalseWhenRidingNothing()
        {
            Assert.False(SnailBubblePop.ShouldPop(snailActive: true, ridesACandy: false, candyHasBubble: true));
        }
    }
}
