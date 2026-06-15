using CutTheRopeDX.GameMain;
using Xunit;

namespace CutTheRopeDX.Tests
{
    public class BarrierCollisionTests
    {
        private const float T1X = 0, T1Y = 100, T2X = 200, T2Y = 100;
        private const float B1X = 0, B1Y = 110, B2X = 200, B2Y = 110;

        [Fact]
        public void Hits_TrueWhenCandyBoxOverlapsTopEdge()
        {
            Assert.True(BarrierCollision.Hits(
                T1X, T1Y, T2X, T2Y, B1X, B1Y, B2X, B2Y,
                px: 100, py: 100, prevX: 100, prevY: 100, radius: 15f));
        }

        [Fact]
        public void Hits_TrueWhenSweptSegmentCrossesBarrier()
        {
            Assert.True(BarrierCollision.Hits(
                T1X, T1Y, T2X, T2Y, B1X, B1Y, B2X, B2Y,
                px: 100, py: 130, prevX: 100, prevY: 60, radius: 1f));
        }

        [Fact]
        public void Hits_FalseWhenFarAway()
        {
            Assert.False(BarrierCollision.Hits(
                T1X, T1Y, T2X, T2Y, B1X, B1Y, B2X, B2Y,
                px: 500, py: 500, prevX: 500, prevY: 500, radius: 15f));
        }
    }
}
