using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers measuring what a fitted group actually paints. The group's own box is the design
    /// box and its layout children span it, so measuring boxes rather than paint would report the
    /// whole design width for a button column a third of it wide - and the scale capped against
    /// that measurement would shrink the menu to a third of its size for no reason.
    /// </summary>
    public sealed class DesignExtentTests
    {
        [Fact]
        public void AColumnIsMeasuredByWhatItPaintsNotByTheBoxItLaysOutIn()
        {
            // The main menu's shape: a full-design-width stack, centering buttons a third as wide.
            FittedGroup group = new() { width = 2560, height = 1440 };
            VBox stack = new VBox().InitWithOffsetAlignWidth(5f, 2, 2560f);
            stack.anchor = stack.parentAnchor = 34;
            _ = stack.AddChild(Painted(737, 176));
            _ = stack.AddChild(Painted(737, 176));
            _ = group.AddChild(stack);

            CTRRectangle content = DesignExtent.Measure(group);

            Assert.Equal(737f, content.w, 0.5);
            Assert.Equal(357f, content.h, 0.5);
        }

        [Fact]
        public void ContentIsMeasuredInTheGroupsOwnCoordinates()
        {
            // What the cap needs is where the content sits inside the design box, whatever the
            // group's own placement in logical space happens to be at the time.
            FittedGroup group = new() { width = 2560, height = 1440, x = 4000f, y = 3000f };
            Image image = Painted(822, 710);
            image.anchor = image.parentAnchor = 10;
            image.y = 55f;
            _ = group.AddChild(image);

            CTRRectangle content = DesignExtent.Measure(group);

            Assert.Equal(869f, content.x, 0.5);
            Assert.Equal(55f, content.y, 0.5);
        }

        [Fact]
        public void AButtonIsMeasuredByThePartOfItThatIsShowing()
        {
            // A button carries both of its states as children and hides the one it is not in. The
            // pressed art is drawn nowhere, so it must not be able to widen the measurement.
            FittedGroup group = new() { width = 2560, height = 1440 };
            Image up = Painted(737, 176);
            Image down = Painted(2000, 176);
            _ = new Button().InitWithUpElementDownElementandID(up, down, MenuButtonId.Play);
            _ = group.AddChild(up.parent);

            CTRRectangle content = DesignExtent.Measure(group);

            Assert.Equal(737f, content.w, 0.5);
        }

        [Fact]
        public void AnElementIsMeasuredAtTheSizeItIsDrawn()
        {
            // Elements scale about their own center, and a scene that shrinks one means it.
            FittedGroup group = new() { width = 2560, height = 1440 };
            Image image = Painted(800, 400);
            image.anchor = image.parentAnchor = 18;
            image.scaleX = image.scaleY = 0.5f;
            _ = group.AddChild(image);

            CTRRectangle content = DesignExtent.Measure(group);

            Assert.Equal(400f, content.w, 0.5);
            Assert.Equal(200f, content.h, 0.5);
        }

        [Fact]
        public void AGroupWithNothingToPaintMeasuresEmpty()
        {
            FittedGroup group = new() { width = 2560, height = 1440 };
            _ = group.AddChild(new VBox().InitWithOffsetAlignWidth(5f, 2, 2560f));

            CTRRectangle content = DesignExtent.Measure(group);

            Assert.Equal(0f, content.w, 0.0001);
            Assert.Equal(0f, content.h, 0.0001);
        }

        /// <summary>An element of the given size that paints.</summary>
        /// <param name="width">Width in design units.</param>
        /// <param name="height">Height in design units.</param>
        /// <returns>The element.</returns>
        private static Image Painted(int width, int height)
        {
            return new Image { width = width, height = height };
        }
    }
}
