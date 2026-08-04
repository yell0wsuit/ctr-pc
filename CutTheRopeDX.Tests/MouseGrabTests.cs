using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class MouseGrabTests
    {
        [Fact]
        public void ShouldGrabTrueForEmptyMouseAndCandyInRange()
        {
            Assert.True(MouseGrab.ShouldGrab(mouseHasCandy: false, candyPresent: true, inRange: true));
        }

        [Fact]
        public void ShouldGrabFalseWhenMouseAlreadyCarrying()
        {
            // single-occupancy: a mouse already carrying ignores other candies.
            Assert.False(MouseGrab.ShouldGrab(mouseHasCandy: true, candyPresent: true, inRange: true));
        }

        [Fact]
        public void ShouldGrabFalseWhenMissingOrOutOfRange()
        {
            Assert.False(MouseGrab.ShouldGrab(false, candyPresent: false, true));
            Assert.False(MouseGrab.ShouldGrab(false, true, inRange: false));
        }
    }
}
