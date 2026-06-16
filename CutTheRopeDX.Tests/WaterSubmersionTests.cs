using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class WaterSubmersionTests
    {
        [Fact]
        public void IsSubmerged_TrueWhenBelowSurfaceInsideWaterColumn()
        {
            Assert.True(WaterSubmersion.IsSubmerged(100f, 130f, 50f, 120f, 100f, 20f));
        }

        [Fact]
        public void IsSubmerged_FalseWhenAboveOrAtSurface()
        {
            Assert.False(WaterSubmersion.IsSubmerged(100f, 119.9f, 50f, 120f, 100f, 20f));
            Assert.False(WaterSubmersion.IsSubmerged(100f, 120f, 50f, 120f, 100f, 20f));
        }

        [Fact]
        public void IsSubmerged_FalseWhenOutsideWaterColumn()
        {
            Assert.False(WaterSubmersion.IsSubmerged(29.9f, 130f, 50f, 120f, 100f, 20f));
            Assert.False(WaterSubmersion.IsSubmerged(170.1f, 130f, 50f, 120f, 100f, 20f));
        }

        [Fact]
        public void IsSubmerged_TrueOnInclusiveHorizontalBoundaries()
        {
            Assert.True(WaterSubmersion.IsSubmerged(30f, 130f, 50f, 120f, 100f, 20f));
            Assert.True(WaterSubmersion.IsSubmerged(170f, 130f, 50f, 120f, 100f, 20f));
        }
    }
}
