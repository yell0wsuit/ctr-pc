using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the pure surface-size to snapshot computation. The legacy content bounds
    /// must reproduce the scaled view rectangle the game shipped with, so these values are
    /// asserted against the same arithmetic <see cref="ScreenPresentation"/> already uses.
    /// </summary>
    public sealed class ViewportLayoutTests
    {
        [Fact]
        public void MatchingAspectRatioFillsTheWholeSurface()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1280, 720, true);

            // 1280x720 is exactly 16:9, so a 2560x1440 screen fits with no bars.
            Assert.Equal(0f, snapshot.LegacyContentBounds.x);
            Assert.Equal(0f, snapshot.LegacyContentBounds.y);
            Assert.Equal(1280f, snapshot.LegacyContentBounds.w);
            Assert.Equal(720f, snapshot.LegacyContentBounds.h);
            Assert.Equal(0.5f, snapshot.LegacyScale);
        }

        [Fact]
        public void UltrawideSurfacePillarboxesTheLegacyContent()
        {
            // 21:9. Height is the limiting axis, so the legacy rect keeps the full height
            // and is centered horizontally.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(2560, 1080, true);

            Assert.Equal(1080f, snapshot.LegacyContentBounds.h);
            Assert.Equal(1920f, snapshot.LegacyContentBounds.w);
            Assert.Equal(320f, snapshot.LegacyContentBounds.x);
            Assert.Equal(0f, snapshot.LegacyContentBounds.y);
        }

        [Fact]
        public void FourThreeSurfaceLetterboxesTheLegacyContent()
        {
            // 4:3. Width is the limiting axis, so the legacy rect keeps the full width
            // and is centered vertically.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1024, 768, false);

            Assert.Equal(1024f, snapshot.LegacyContentBounds.w);
            Assert.Equal(576f, snapshot.LegacyContentBounds.h);
            Assert.Equal(0f, snapshot.LegacyContentBounds.x);
            Assert.Equal(96f, snapshot.LegacyContentBounds.y);
        }

        [Fact]
        public void SquareSurfaceLetterboxesTheLegacyContent()
        {
            // Crop-width is off here deliberately. With it on, a square surface produces a
            // 1778x1000 rectangle that overflows the surface horizontally and sits at a negative
            // x -- that is what cropping width means, and it is existing shipped behavior.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1000, 1000, false);

            Assert.Equal(1000f, snapshot.LegacyContentBounds.w);
            Assert.Equal(563f, snapshot.LegacyContentBounds.h);
            Assert.Equal(0f, snapshot.LegacyContentBounds.x);
            Assert.Equal(218f, snapshot.LegacyContentBounds.y);
        }

        [Fact]
        public void SquareSurfaceWithCropWidthOverflowsHorizontally()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1000, 1000, true);

            Assert.Equal(1778f, snapshot.LegacyContentBounds.w);
            Assert.Equal(1000f, snapshot.LegacyContentBounds.h);
            Assert.Equal(-389f, snapshot.LegacyContentBounds.x);
        }

        [Fact]
        public void PortraitSurfaceLetterboxesTheLegacyContent()
        {
            // 9:16. The legacy 16:9 content becomes a centered horizontal strip.
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(720, 1280, false);

            Assert.Equal(720f, snapshot.LegacyContentBounds.w);
            Assert.Equal(405f, snapshot.LegacyContentBounds.h);
            Assert.Equal(0f, snapshot.LegacyContentBounds.x);
            Assert.Equal(437f, snapshot.LegacyContentBounds.y);
        }

        [Fact]
        public void SnapshotCarriesTheSurfaceSizeItWasComputedFrom()
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(1600, 900, true);

            Assert.Equal(1600, snapshot.SurfaceWidth);
            Assert.Equal(900, snapshot.SurfaceHeight);
        }

        [Fact]
        public void EqualSurfaceSizesProduceEqualSnapshots()
        {
            ViewportLayoutSnapshot a = ViewportLayout.Compute(1600, 900, true);
            ViewportLayoutSnapshot b = ViewportLayout.Compute(1600, 900, true);

            Assert.Equal(a, b);
        }
    }
}
