using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the ceiling the content scale is held under so a fitted composition keeps a margin
    /// to the screen edge. The scale curve grows content as a window departs from the design
    /// shape, and nothing measured what it was growing against: on a very wide window the main
    /// menu's logo was drawn off the top of the screen, and on a near-square one it reached the
    /// edge with a few units to spare.
    /// </summary>
    public sealed class FittedContentFitTests
    {
        /// <summary>The main menu's content in design coordinates: logo, then the button column.</summary>
        private static readonly CTRRectangle MenuContent = new(869f, 55f, 822f, 1300f);

        /// <summary>The design box that content is authored in.</summary>
        private static readonly CTRRectangle DesignBox = new(0f, 0f, 2560f, 1440f);

        [Fact]
        public void ContentWithRoomToSpareIsDrawnAtTheScaleItAskedFor()
        {
            // The shape the game was drawn for, where every layout rule must reduce to the
            // constant it was authored with.
            CTRRectangle visible = new(0f, 0f, 2560f, 1440f);

            float scale = FittedContentFit.ScaleFor(visible, DesignBox, MenuContent, 1f, 48f);

            Assert.Equal(1f, scale, 0.0001);
        }

        [Fact]
        public void ContentGrownPastTheEdgeIsHeldBackToTheMargin()
        {
            // A window at the widest supported shape, where the scale curve asks for 1.15 and the
            // menu's 1300-unit-tall content, centered, would run 45 units off both ends.
            CTRRectangle visible = new(0f, 0f, 4176f, 1440f);

            float scale = FittedContentFit.ScaleFor(visible, DesignBox, MenuContent, 1.15f, 48f);

            // The content's far edge from the design box center is the button column's bottom,
            // 665 units down, so it may grow until that reaches the margin.
            Assert.Equal((720f - 48f) / 665f, scale, 0.0001);
        }

        [Fact]
        public void TheWidthIsMeasuredAsWellAsTheHeight()
        {
            // Content wider than it is tall, in a window that is short of room across rather than
            // down: the axis that binds is whichever runs out first, not always the vertical one.
            CTRRectangle content = new(780f, 620f, 1000f, 200f);
            CTRRectangle visible = new(0f, 0f, 1440f, 3600f);

            float scale = FittedContentFit.ScaleFor(visible, DesignBox, content, 1.55f, 48f);

            Assert.Equal((720f - 48f) / 500f, scale, 0.0001);
        }

        [Fact]
        public void ContentIsMeasuredFromTheCenterItGrowsAbout()
        {
            // A fitted group is centered on the viewport and scaled about its own center, so what
            // reaches an edge first is the content's far side from that center - not its height.
            // Measuring the extent alone would let this one grow half again as large.
            CTRRectangle lopsided = new(1180f, 1140f, 200f, 200f);
            CTRRectangle visible = new(0f, 0f, 4176f, 1440f);

            float scale = FittedContentFit.ScaleFor(visible, DesignBox, lopsided, 1.15f, 48f);

            Assert.Equal((720f - 48f) / 620f, scale, 0.0001);
        }

        [Fact]
        public void TheCapNeverShrinksContentBelowItsAuthoredSize()
        {
            // Content authored taller than the margin allows is drawn as it always was. Giving
            // back growth is one thing; shrinking the shipped composition on the shape it was
            // drawn for is another, and no window makes that the better picture.
            CTRRectangle tall = new(1180f, 0f, 200f, 1440f);
            CTRRectangle visible = new(0f, 0f, 2560f, 1440f);

            float scale = FittedContentFit.ScaleFor(visible, DesignBox, tall, 1f, 48f);

            Assert.Equal(1f, scale, 0.0001);
        }

        [Fact]
        public void AScaleAlreadyBelowTheAuthoredSizeIsNotRaisedToIt()
        {
            // Never shrinking below the authored size is a floor under this function's own cap,
            // not a floor under the caller's scale. A scene that has already held itself down -
            // the level grid, whose widest row would otherwise run off the sides - asked for that
            // scale for a reason, and raising it back to one puts the grid straight off screen.
            CTRRectangle visible = new(0f, 0f, 1440f, 1440f);

            float scale = FittedContentFit.ScaleFor(visible, DesignBox, MenuContent, 0.833f, 48f);

            Assert.Equal(0.833f, scale, 0.0001);
        }

        [Fact]
        public void ContentWithNoExtentIsLeftAlone()
        {
            // A scene whose group holds nothing that paints has nothing to measure, and must not
            // be scaled by a division by zero.
            CTRRectangle empty = new(0f, 0f, 0f, 0f);
            CTRRectangle visible = new(0f, 0f, 4176f, 1440f);

            float scale = FittedContentFit.ScaleFor(visible, DesignBox, empty, 1.15f, 48f);

            Assert.Equal(1.15f, scale, 0.0001);
        }
    }
}
