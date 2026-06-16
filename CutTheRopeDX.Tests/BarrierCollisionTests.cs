using CutTheRopeDX.Framework;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class BarrierCollisionTests
    {
        private const float T1X = 0, T1Y = 100, T2X = 200, T2Y = 100;
        private const float B1X = 0, B1Y = 110, B2X = 200, B2Y = 110;
        private static readonly float BouncerRadius = ActivePhysicsConstants.BouncerCollisionRadius;
        private const float SpikeRadius = 15f;

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

        // A whole candy whose swept path crosses the bouncer barrier registers a hit at the real
        // bouncer collision radius (mirrors the candies[0] inline bouncer formula).
        [Fact]
        public void Hits_TrueForWholeCandyCrossingBouncer()
        {
            Assert.True(BarrierCollision.Hits(
                T1X, T1Y, T2X, T2Y, B1X, B1Y, B2X, B2Y,
                px: 100, py: 120, prevX: 100, prevY: 80, radius: BouncerRadius));
        }

        // The same candy far below the barrier does not register a hit.
        [Fact]
        public void Hits_FalseForWholeCandyBelowBouncer()
        {
            Assert.False(BarrierCollision.Hits(
                T1X, T1Y, T2X, T2Y, B1X, B1Y, B2X, B2Y,
                px: 100, py: 400, prevX: 100, prevY: 400, radius: BouncerRadius));
        }

        // A whole candy whose swept path crosses the spike registers a break at the spike radius
        // (mirrors the candies[0] inline spike formula).
        [Fact]
        public void Hits_TrueForWholeCandyCrossingSpike()
        {
            Assert.True(BarrierCollision.Hits(
                T1X, T1Y, T2X, T2Y, B1X, B1Y, B2X, B2Y,
                px: 100, py: 120, prevX: 100, prevY: 80, radius: SpikeRadius));
        }

        // A whole candy resting clear of the spike does not break.
        [Fact]
        public void Hits_FalseForWholeCandyClearOfSpike()
        {
            Assert.False(BarrierCollision.Hits(
                T1X, T1Y, T2X, T2Y, B1X, B1Y, B2X, B2Y,
                px: 100, py: 300, prevX: 100, prevY: 300, radius: SpikeRadius));
        }
    }
}
