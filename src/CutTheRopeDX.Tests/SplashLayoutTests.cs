using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers how the startup splash divides a viewport. The stage is contained in the screen and
    /// the legal disclaimer hangs under it, so the two have to grow together: the disclaimer used
    /// to be held at its authored size while the stage above it grew by half on a phone.
    /// </summary>
    public sealed class SplashLayoutTests
    {
        /// <summary>Width of the splash animation's own stage.</summary>
        private const float StageWidth = 640f;

        /// <summary>Height of the splash animation's own stage.</summary>
        private const float StageHeight = 960f;

        /// <summary>Scale the disclaimer is authored at, which the design shape keeps.</summary>
        private const float AuthoredScale = 0.65f;

        [Fact]
        public void TheDesignShapeIsDrawnExactlyAsItWasAuthored()
        {
            SplashLayout layout = LayoutFor(2560, 1440);
            CTRRectangle visible = VisibleFor(2560, 1440);

            Assert.Equal(AuthoredScale, layout.DisclaimerScale, 0.0001);
            Assert.Equal(visible.w * 0.9f, layout.DisclaimerWrapWidth, 0.01);
            Assert.Equal(layout.Stage.y + layout.Stage.h - 35f, layout.DisclaimerBottom, 0.01);
        }

        [Fact]
        public void TheDisclaimerGrowsWithTheStageItHangsUnder()
        {
            SplashLayout design = LayoutFor(2560, 1440);
            SplashLayout phone = LayoutFor(720, 1280);

            float stageGrowth = phone.Stage.h / design.Stage.h;
            Assert.True(stageGrowth > 1f, "the fixture viewport should draw the stage larger");
            Assert.Equal(design.DisclaimerScale * stageGrowth, phone.DisclaimerScale, 0.0001);
        }

        [Fact]
        public void TheColumnTheDisclaimerIsDrawnInKeepsItsShareOfTheScreen()
        {
            // Only the letters grow: a wrap width left alone while the scale grew would push the
            // text out past both edges of the screen.
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                CTRRectangle visible = VisibleFor(surface.Width, surface.Height);
                SplashLayout layout = LayoutFor(surface.Width, surface.Height);
                float drawnColumn = layout.DisclaimerWrapWidth * layout.DisclaimerScale;

                Assert.Equal(visible.w * 0.9f * AuthoredScale, drawnColumn, 0.01);
                Assert.True(
                    drawnColumn < visible.w,
                    $"{surface.Name}: a {drawnColumn} column on a {visible.w} viewport");
            }
        }

        [Fact]
        public void TheDisclaimerStaysUnderTheStageAndOnTheScreen()
        {
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                CTRRectangle visible = VisibleFor(surface.Width, surface.Height);
                SplashLayout layout = LayoutFor(surface.Width, surface.Height);

                Assert.InRange(layout.DisclaimerBottom, 0f, visible.h);
                Assert.True(
                    layout.DisclaimerBottom <= layout.Stage.y + layout.Stage.h,
                    $"{surface.Name}: the disclaimer sits below the stage it hangs under");
            }
        }

        [Fact]
        public void TheStageIsContainedInEveryViewport()
        {
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                CTRRectangle visible = VisibleFor(surface.Width, surface.Height);
                SplashLayout layout = LayoutFor(surface.Width, surface.Height);

                Assert.True(
                    layout.Stage.w <= visible.w + 0.01f && layout.Stage.h <= visible.h + 0.01f,
                    $"{surface.Name}: a {layout.Stage.w}x{layout.Stage.h} stage on a "
                    + $"{visible.w}x{visible.h} viewport");
                Assert.Equal(
                    StageWidth / StageHeight,
                    layout.Stage.w / layout.Stage.h,
                    0.0001);
            }
        }

        /// <summary>Builds the layout for a surface size.</summary>
        /// <param name="width">Surface width in pixels.</param>
        /// <param name="height">Surface height in pixels.</param>
        /// <returns>The layout.</returns>
        private static SplashLayout LayoutFor(int width, int height)
        {
            return SplashLayout.For(VisibleFor(width, height), StageWidth, StageHeight);
        }

        /// <summary>The region a surface exposes.</summary>
        /// <param name="width">Surface width in pixels.</param>
        /// <param name="height">Surface height in pixels.</param>
        /// <returns>The visible bounds.</returns>
        private static CTRRectangle VisibleFor(int width, int height)
        {
            return ViewportLayout.Compute(width, height).VisibleBounds;
        }
    }
}
