using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers gameplay camera policy: how much world a viewport reveals, and when the camera
    /// stops following the tracked point because the level already fits.
    /// </summary>
    public sealed class CameraFitTests
    {
        [Fact]
        public void AWiderViewportRevealsMoreWorldRatherThanAddingBars()
        {
            CameraFit sixteenNine = LayoutMath.FitCamera(
                new CTRRectangle(0f, 0f, 960f, 1440f),
                new CTRRectangle(0f, 0f, 2560f, 1440f), 0.5f, 0.5f);
            CameraFit ultrawide = LayoutMath.FitCamera(
                new CTRRectangle(0f, 0f, 960f, 1440f),
                new CTRRectangle(0f, 0f, 3413f, 1440f), 0.5f, 0.5f);

            Assert.Equal(sixteenNine.Scale, ultrawide.Scale, 0.001);
            Assert.True(ultrawide.VisibleWorld.w > sixteenNine.VisibleWorld.w);
        }

        [Fact]
        public void APortraitViewportScalesTheLevelDownToFit()
        {
            CameraFit fit = LayoutMath.FitCamera(
                new CTRRectangle(0f, 0f, 960f, 1440f),
                new CTRRectangle(0f, 0f, 1440f, 2560f), 0.5f, 0.5f);

            // Width is the limiting axis at 9:16, so the level fills the width and the extra
            // height becomes revealed world above and below.
            Assert.Equal(1440f / 960f, fit.Scale, 0.001);
            Assert.True(fit.VisibleWorld.h > 1440f);
        }

        [Theory]
        [InlineData(1920f, 1440f, 3413f, 1440f, true)]   // level 1.33 wide, viewport 2.37: fits
        [InlineData(1920f, 1440f, 1440f, 2560f, false)]  // portrait viewport: does not fit
        [InlineData(960f, 2880f, 2560f, 1440f, false)]   // tall level: does not fit
        public void ScrollLocksWhenTheViewportAlreadyContainsTheLevel(
            float levelW, float levelH, float viewW, float viewH, bool expected)
        {
            Assert.Equal(expected, GameplayCamera.ScrollIsLocked(levelW, levelH, viewW, viewH));
        }
    }
}
