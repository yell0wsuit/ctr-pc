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
            presentation.SetSurfaceSize(1280, 720);

            // 1280x720 is exactly the 2560x1440 game's 16:9 aspect ratio, so the aspect-preserving
            // height for a 1280-wide surface is 720 (ScaledGameHeight(w) = (int)(w * gameHeight /
            // gameWidth + 0.5) = (int)(1280 * 0.5625 + 0.5) = 720).
            Assert.Equal(720, presentation.ScaledGameHeight(1280));
        }

        [Fact]
        public void TransformViewToGameXYRoundTripsAPoint()
        {
            ScreenPresentation presentation = new(2560, 1440);
            presentation.SetSurfaceSize(1280, 720);

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
    }
}
