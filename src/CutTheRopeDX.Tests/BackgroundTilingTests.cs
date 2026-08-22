using CutTheRopeDX.GameMain;
using CutTheRopeDX.Framework;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class BackgroundTilingTests
    {
        [Theory]
        [InlineData(1440f, 0)]
        [InlineData(2880f, 1)]
        [InlineData(4320f, 2)]
        [InlineData(5760f, 3)]
        public void EveryP1SeamGetsAP2Overlay(float mapHeight, int expectedCount)
        {
            Assert.Equal(
                expectedCount,
                BackgroundTiling.GetP2Count(mapHeight, mapSectionHeight: 1440f));
        }

        [Fact]
        public void ThreeP1SectionsPlaceP2AtBothSeams()
        {
            Assert.Equal(1120f, BackgroundTiling.ResolveP2Y(1120f, 1440f, seamIndex: 0));
            Assert.Equal(2560f, BackgroundTiling.ResolveP2Y(1120f, 1440f, seamIndex: 1));
        }

        [Theory]
        [InlineData(2560, 1440)]
        [InlineData(2560, 1600)]
        [InlineData(2560, 2160)]
        public void ThreeP1SectionsKeepBothOverlaysAtTallSurfaceHeights(int width, int height)
        {
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                float sectionHeight = FrameworkTypes.SCREEN_HEIGHT;

                Assert.Equal(
                    2,
                    BackgroundTiling.GetP2Count(
                        mapHeight: sectionHeight * 3f,
                        mapSectionHeight: sectionHeight));
                Assert.Equal(1120f, BackgroundTiling.ResolveP2Y(1120f, 1440f, seamIndex: 0));
                Assert.Equal(2560f, BackgroundTiling.ResolveP2Y(1120f, 1440f, seamIndex: 1));
            });
        }
    }
}
