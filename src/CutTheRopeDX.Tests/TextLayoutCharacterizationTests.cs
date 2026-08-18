using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins text measurement and wrapping. These are layout inputs: a change in string width or
    /// line breaking moves every element positioned relative to a label.
    /// </summary>
    public sealed class TextLayoutCharacterizationTests
    {
        [Fact]
        public void StringWidthIsPinnedForAKnownString()
        {
            HeadlessFont font = new();

            Assert.Equal(72f, font.StringWidth("Cut the Rope"));
        }

        [Fact]
        public void MeasuredWidthIsPinnedByCharacterCount()
        {
            HeadlessFont font = new();

            Assert.Equal(48f, font.StringWidth("aaaaaaaa"));
            Assert.Equal(24f, font.StringWidth("aaaa"));
            Assert.Equal(12f, font.StringWidth("aa"));
        }

        [Fact]
        public void UnwrappedTextMeasuresToASingleLine()
        {
            _ = HeadlessGame.Boot();
            Text text = new Text().InitWithFont(new HeadlessFont());
            text.SetString("Cut the Rope");

            Assert.Equal(72, text.width);
            Assert.Equal(10, text.height);
        }

        [Fact]
        public void WrappingAtANarrowWidthProducesTwoLines()
        {
            _ = HeadlessGame.Boot();
            Text wrapped = new Text().InitWithFont(new HeadlessFont());
            wrapped.SetStringandWidth(
                "Cut the Rope is a puzzle game about feeding candy to a monster",
                200f);

            Assert.Equal(200, wrapped.width);
            Assert.Equal(20, wrapped.height);
        }

        [Fact]
        public void ScalingATextElementDoesNotChangeItsMeasuredSize()
        {
            // Element scale is a render-time transform. Measured width and height stay in the
            // element's own units, which is why a scaled parent group and an unscaled hit test
            // disagree.
            _ = HeadlessGame.Boot();
            Text text = new Text().InitWithFont(new HeadlessFont());
            text.SetString("Cut the Rope");

            text.scaleX = 2f;
            text.scaleY = 2f;
            text.FormatText();
            text.UpdateDrawerValues();

            Assert.Equal(72, text.width);
            Assert.Equal(10, text.height);
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void TextGeometryIsIndependentOfSurfaceSize(string name, int width, int height)
        {
            _ = HeadlessGame.Boot();
            LayoutSurfaces.WithSurface(width, height, () =>
            {
                Text text = new Text().InitWithFont(new HeadlessFont());
                text.SetString("Cut the Rope");

                // Text is measured in design units today, so nothing about it tracks the surface.
                Assert.Equal(72, text.width);
                Assert.Equal(10, text.height);
                Assert.False(string.IsNullOrEmpty(name));
            });
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
