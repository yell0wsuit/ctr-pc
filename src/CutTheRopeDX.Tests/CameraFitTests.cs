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
        // Nothing to scroll, or the viewport already exposes everything there was: hold centered.
        [InlineData(0f, 0f, 0.5f)]
        [InlineData(1040f, 1040f, 0.5f)]
        [InlineData(1040f, 2000f, 0.5f)]
        // Travel left to do, so the anchor follows the tracked position through it.
        [InlineData(1040f, 0f, 0.25f)]
        [InlineData(1040f, 520f, 0.25f)]
        public void TheAnchorFollowsTheTrackedPositionOnlyWhileTheLevelStillHasTravel(
            float scrollable, float slack, float expected)
        {
            // Tracked a quarter of the way through a level whose near edge is at -520.
            Assert.Equal(expected, GameplayCamera.Anchor(-260f, -520f, scrollable, slack), 0.001);
        }

        [Theory]
        [InlineData(-2000f, 0f)]
        [InlineData(2000f, 1f)]
        public void TheAnchorStaysInsideTheLevelWhenTheTrackedPositionLeavesIt(
            float tracked, float expected)
        {
            Assert.Equal(expected, GameplayCamera.Anchor(tracked, -520f, 1040f, 0f), 0.001);
        }
    }
}
