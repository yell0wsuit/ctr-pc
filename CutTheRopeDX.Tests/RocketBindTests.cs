using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class RocketBindTests
    {
        [Fact]
        public void ShouldBind_TrueForIdleRocketAndFreeCandyIntersecting()
        {
            Assert.True(RocketBind.ShouldBind(
                rocketIdle: true, candyPresent: true, candyInLantern: false,
                mouseHasCandy: false, intersects: true));
        }

        [Fact]
        public void ShouldBind_FalseWhenRocketNotIdle()
        {
            // one-time use: a rocket that has left idle (flying/exhausted) never binds again.
            Assert.False(RocketBind.ShouldBind(rocketIdle: false, true, false, false, true));
        }

        [Fact]
        public void ShouldBind_FalseWhenCandyInLantern()
        {
            Assert.False(RocketBind.ShouldBind(true, true, candyInLantern: true, false, true));
        }

        [Fact]
        public void ShouldBind_FalseWhenMouseHasCandy()
        {
            Assert.False(RocketBind.ShouldBind(true, true, false, mouseHasCandy: true, true));
        }

        [Fact]
        public void ShouldBind_FalseWhenMissingOrNoIntersection()
        {
            Assert.False(RocketBind.ShouldBind(true, candyPresent: false, false, false, true));
            Assert.False(RocketBind.ShouldBind(true, true, false, false, intersects: false));
        }
    }
}
