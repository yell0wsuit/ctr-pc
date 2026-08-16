using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the device-free presentation math ported verbatim from
    /// <c>ScreenSizeManager</c>'s coordinate-transform arithmetic.
    /// </summary>
    public sealed class ScreenPresentationTests
    {
        [Fact]
        public void SetSurfaceSizeScaledGameHeightMatchesSurfaceForMatchingAspectRatio()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1280, 720);

            // 1280x720 is exactly the 2560x1440 game's 16:9 aspect ratio, so the aspect-preserving
            // height for a 1280-wide surface is 720 (ScaledGameHeight(w) = (int)(w * gameHeight /
            // gameWidth + 0.5) = (int)(1280 * 0.5625 + 0.5) = 720).
            Assert.Equal(720, presentation.ScaledGameHeight(1280));
        }

        [Fact]
        public void TransformViewToGameXYRoundTripsAPoint()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1280, 720);

            // The surface matches the game's aspect ratio exactly, so the render viewport fills
            // the whole surface at a uniform 2x scale.
            float gameX = presentation.TransformViewToGameX(640f);
            float gameY = presentation.TransformViewToGameY(360f);
            Assert.Equal(1280f, gameX);
            Assert.Equal(720f, gameY);

            // Round-trip back to view space using the algebraic inverse of that same formula
            // (v = g * Scale) and confirm we land back on the original point.
            float viewX = gameX * presentation.Snapshot.Scale;
            float viewY = gameY * presentation.Snapshot.Scale;
            Assert.Equal(640f, viewX);
            Assert.Equal(360f, viewY);
        }

        [Fact]
        public void RepublishingTheSameSurfaceReportsNoChange()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1280, 720);

            Assert.False(presentation.SetSurfaceSize(1280, 720));
        }

        [Fact]
        public void PublishingADifferentSurfaceReportsAChange()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1280, 720);

            Assert.True(presentation.SetSurfaceSize(1600, 900));
        }

        [Fact]
        public void TheSnapshotIsTheOnlyStateAPublishMutates()
        {
            // Nothing shadows the snapshot: reading any projection after a publish gives values
            // derived from that same snapshot, so there is no second copy to fall out of step.
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1600, 900);
            ViewportLayoutSnapshot published = presentation.Snapshot;

            Assert.Equal(published.SurfaceWidth, presentation.SurfaceWidth);
            Assert.Equal(published.SurfaceHeight, presentation.SurfaceHeight);
        }
    }
}
