using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the device-free presentation math ported verbatim from
    /// <c>ScreenSizeManager</c>'s scaled-view-rect and coordinate-transform arithmetic.
    /// </summary>
    public sealed class ScreenPresentationTests
    {
        [Fact]
        public void SetSurfaceSizeScaledGameHeightMatchesSurfaceForMatchingAspectRatio()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1280, 720, true);

            // 1280x720 is exactly the 2560x1440 game's 16:9 aspect ratio, so the aspect-preserving
            // height for a 1280-wide surface is 720 (ScaledGameHeight(w) = (int)(w * gameHeight /
            // gameWidth + 0.5) = (int)(1280 * 0.5625 + 0.5) = 720).
            Assert.Equal(720, presentation.ScaledGameHeight(1280));
        }

        [Fact]
        public void TransformViewToGameXYRoundTripsAPoint()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1280, 720, true);

            // The surface matches the game's aspect ratio exactly, so the scaled view rect fills
            // the whole surface with no letterbox/pillarbox, at a uniform 2x scale
            // (ScaledViewWidth/Height == SurfaceWidth/Height == 1280x720).
            Assert.Equal(1280, presentation.ScaledViewWidth);
            Assert.Equal(720, presentation.ScaledViewHeight);

            // TransformViewToGameX/Y(v) = v * GameSize / ScaledViewSize (ported verbatim from
            // ScreenSizeManager.TransformViewToGameX/Y). At 2x scale, the view-space center
            // (640, 360) maps to the game-space center (1280, 720).
            float gameX = presentation.TransformViewToGameX(640f);
            float gameY = presentation.TransformViewToGameY(360f);
            Assert.Equal(1280f, gameX);
            Assert.Equal(720f, gameY);

            // Round-trip back to view space using the algebraic inverse of that same formula
            // (v = g * ScaledViewSize / GameSize) and confirm we land back on the original point.
            float viewX = gameX * presentation.ScaledViewWidth / presentation.GameWidth;
            float viewY = gameY * presentation.ScaledViewHeight / presentation.GameHeight;
            Assert.Equal(640f, viewX);
            Assert.Equal(360f, viewY);
        }

        [Fact]
        public void ScaledViewProjectsTheSnapshotLegacyContentBounds()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(2560, 1080, true);

            Assert.Equal((int)presentation.Snapshot.LegacyContentBounds.x, presentation.ScaledViewX);
            Assert.Equal((int)presentation.Snapshot.LegacyContentBounds.y, presentation.ScaledViewY);
            Assert.Equal((int)presentation.Snapshot.LegacyContentBounds.w, presentation.ScaledViewWidth);
            Assert.Equal((int)presentation.Snapshot.LegacyContentBounds.h, presentation.ScaledViewHeight);
        }

        [Fact]
        public void RepublishingTheSameSurfaceReportsNoChange()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1280, 720, true);

            Assert.False(presentation.SetSurfaceSize(1280, 720, true));
        }

        [Fact]
        public void PublishingADifferentSurfaceReportsAChange()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1280, 720, true);

            Assert.True(presentation.SetSurfaceSize(1600, 900, true));
        }

        [Fact]
        public void ChangingOnlyTheCropInputReportsAChange()
        {
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(720, 1280, true);

            Assert.True(presentation.SetSurfaceSize(720, 1280, false));
        }

        [Fact]
        public void TheSnapshotIsTheOnlyStateAPublishMutates()
        {
            // Nothing shadows the snapshot: reading any projection after a publish gives values
            // derived from that same snapshot, so there is no second copy to fall out of step.
            ScreenPresentation presentation = new(2560, 1440);
            _ = presentation.SetSurfaceSize(1600, 900, true);
            ViewportLayoutSnapshot published = presentation.Snapshot;

            Assert.Equal(published.SurfaceWidth, presentation.SurfaceWidth);
            Assert.Equal(published.SurfaceHeight, presentation.SurfaceHeight);
            Assert.Equal((int)published.LegacyContentBounds.w, presentation.ScaledViewWidth);
        }
    }
}
