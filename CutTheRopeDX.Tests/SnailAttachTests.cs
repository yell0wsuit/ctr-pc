using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Gate for an inactive snail landing on a candy. Each candy is judged on its own state, so in a
    /// multi-candy level the snail skips a gone or non-draggable candy and rides the next one it hits.
    /// </summary>
    public class SnailAttachTests
    {
        [Fact]
        public void ShouldAttach_TrueForAPresentDraggableCandyItTouches()
        {
            Assert.True(SnailAttach.ShouldAttach(
                candyGone: false, canBeDraggedBySnail: true, snailIntersectsCandy: true));
        }

        [Fact]
        public void ShouldAttach_FalseForAGoneCandy()
        {
            // Multi-candy: candy A already eaten/off-screen is skipped so the snail can ride candy B instead.
            Assert.False(SnailAttach.ShouldAttach(
                candyGone: true, canBeDraggedBySnail: true, snailIntersectsCandy: true));
        }

        [Fact]
        public void ShouldAttach_FalseForABodyThatCannotBeDragged()
        {
            // A light bulb and other non-candy bodies opt out via CanBeDraggedBySnail.
            Assert.False(SnailAttach.ShouldAttach(
                candyGone: false, canBeDraggedBySnail: false, snailIntersectsCandy: true));
        }

        [Fact]
        public void ShouldAttach_FalseWhenNotTouching()
        {
            Assert.False(SnailAttach.ShouldAttach(
                candyGone: false, canBeDraggedBySnail: true, snailIntersectsCandy: false));
        }
    }
}
