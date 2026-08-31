using CutTheRopeDX.Framework;
using CutTheRopeDX.GameMain;

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

        [Theory]
        // A window no wider than one section, aligned with it, needs that section alone.
        [InlineData(0f, 2559f, 0f, 2559f, 0, 0)]
        // The design window overhangs the one-pixel-narrow art, so its neighbour shows too.
        [InlineData(0f, 2559f, 0f, 2560f, 0, 1)]
        // A camera parked on the second section needs only that one.
        [InlineData(0f, 2559f, 2559f, 2559f, 1, 1)]
        // A camera straddling the seam needs the sections on both sides of it.
        [InlineData(0f, 2559f, 1280f, 2560f, 0, 1)]
        // Sections repeat in both directions, so a camera left of the origin sees negative ones.
        [InlineData(0f, 2559f, -100f, 2560f, -1, 0)]
        public void SectionRangeCoversEverySectionTheWindowTouches(
            float sectionOrigin,
            float sectionSize,
            float windowStart,
            float windowSize,
            int expectedFirst,
            int expectedLast)
        {
            (int first, int last) = BackgroundTiling.GetSectionRange(
                sectionOrigin,
                sectionSize,
                windowStart,
                windowSize);

            Assert.Equal(expectedFirst, first);
            Assert.Equal(expectedLast, last);
        }

        [Fact]
        public void SectionRangeIsTheAuthoredSectionAloneWhenTheSizeIsUnusable()
        {
            Assert.Equal((0, 0), BackgroundTiling.GetSectionRange(0f, 0f, 0f, 2560f));
        }
    }
}
