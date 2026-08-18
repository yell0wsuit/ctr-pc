using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the sheet the pause menu is drawn on. It is one screen of the shape the game was
    /// composed for, so a window wider than that shape left it short of both edges with gameplay
    /// showing past its ends.
    /// </summary>
    public sealed class PausePlateLayoutTests
    {
        [Fact]
        public void TheDesignShapeDrawsThePlateExactlyAsItWasComposed()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                Image plate = Plate();

                Assert.Equal(1.25f, plate.scaleX, 0.001);
                Assert.Equal(1.25f, plate.scaleY, 0.001);
            });
        }

        [Theory]
        [InlineData(3840, 1080)]
        [InlineData(2560, 1080)]
        [InlineData(2572, 916)]
        public void AWiderViewportPullsThePlateOutToItsEdges(int width, int height)
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                Image plate = Plate();

                Assert.True(
                    plate.scaleX * plate.width >= visible.w,
                    $"a {plate.scaleX * plate.width} plate on a {visible.w} viewport");
            });
        }

        [Fact]
        public void ThePlateKeepsTheDepthItWasDrawnAt()
        {
            // Covering would scale the sheet whole and hang its torn edge far enough down a wide
            // window to swallow the first button of the pause column.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(3840, 1080, () =>
            {
                Image plate = Plate();

                Assert.Equal(1.25f, plate.scaleY, 0.001);
                Assert.True(plate.scaleX > plate.scaleY, "the plate should be wider, not deeper");
            });
        }

        [Fact]
        public void ANarrowViewportLeavesThePlateAsItWas()
        {
            // The sheet is already wider than a phone viewport; what hangs off the sides is what
            // always did.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                Image plate = Plate();

                Assert.Equal(1.25f, plate.scaleX, 0.001);
            });
        }

        [Theory]
        [InlineData(2560, 1440)]
        [InlineData(1280, 720)]
        [InlineData(3840, 1080)]
        [InlineData(2572, 916)]
        [InlineData(720, 1280)]
        public void TheBestScoreLabelStaysInTheCornerOfTheScreen(int width, int height)
        {
            // Its authored offset hangs it off the right edge of the plate, which is the corner of
            // the screen only where the plate is exactly one screen wide. On a wider window the
            // label was left stranded in the middle of the top edge.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
            {
                CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                Image plate = Plate();
                Text label = (Text)plate.GetChildWithName("mapNameLabel");
                Assert.NotNull(label);

                // Where the label's right edge lands: right-anchored against the plate's own
                // rectangle, then scaled about its center.
                float plateEdge = (visible.w + plate.width) / 2f;
                float rightEdge = plateEdge + label.x - (label.width * (1f - label.scaleX) / 2f);

                Assert.Equal(visible.w - AuthoredInsetFromRight, rightEdge, 0.01);
            });
        }

        [Fact]
        public void TheDesignShapeKeepsTheAuthoredBestScoreOffset()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                Text label = (Text)Plate().GetChildWithName("mapNameLabel");

                Assert.Equal(246f, label.x, 0.01);
            });
        }

        /// <summary>
        /// Distance the label's right edge keeps from the right edge of the screen, which is what
        /// the authored offset produces on the design shape.
        /// </summary>
        private const float AuthoredInsetFromRight = 10f;

        /// <summary>Loads a level and returns the pause plate, laid out for the current surface.</summary>
        /// <returns>The pause plate image.</returns>
        private static Image Plate()
        {
            GameController controller = HeadlessGame.LoadLevelWithController(0, 0);
            controller.RelayoutTree(ScreenPresentation.Instance.Snapshot);
            Image plate = (Image)controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU);
            Assert.NotNull(plate);
            return plate;
        }
    }
}
