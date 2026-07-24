using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class RocketBindTests
    {
        [Fact]
        public void ShouldBind_TrueForIdleRocketOnPresentFreeCandy()
        {
            Assert.True(RocketBind.ShouldBind(
                rocketIdle: true, candyPresent: true, candyInLantern: false,
                intersects: true));
        }

        [Fact]
        public void ShouldBind_FalseWhenRocketNotIdle()
        {
            // one-time use: a rocket that has left idle (flying/exhausted) never binds again.
            Assert.False(RocketBind.ShouldBind(rocketIdle: false, true, false, true));
        }

        [Fact]
        public void ShouldBind_FalseWhenCandyInLantern()
        {
            Assert.False(RocketBind.ShouldBind(true, true, candyInLantern: true, true));
        }

        [Fact]
        public void ShouldBind_FalseWhenMissingOrNoIntersection()
        {
            Assert.False(RocketBind.ShouldBind(true, candyPresent: false, false, true));
            Assert.False(RocketBind.ShouldBind(true, true, false, intersects: false));
        }
    }
}
