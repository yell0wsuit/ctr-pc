using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the region the faded-text composite is blitted back through. Getting this too small clips
    /// text, which is the whole risk of bounding a blit that used to cover the entire target.
    /// </summary>
    public class TextCompositeBlitRectangleTests
    {
        /// <summary>Logical-to-target scale the game runs at: a 2560-wide layout on a 1280-wide target.</summary>
        private static Matrix HalfScale => Matrix.CreateScale(0.5f, 0.5f, 1f);

        private static Rectangle Target => new(0, 0, 1280, 720);

        [Fact]
        public void NothingDrawnProducesAnEmptyRectangle()
        {
            // Every line fell outside maxHeight, so the bounds were never widened past their seed values.
            Rectangle rect = Text.CompositeBlitRectangle(
                float.MaxValue, float.MaxValue, float.MinValue, float.MinValue,
                FontEffectSettings.None, 40, HalfScale, Target);

            Assert.Equal(Rectangle.Empty, rect);
        }

        [Fact]
        public void TextCoversFarLessThanTheWholeTarget()
        {
            // The point of the exercise: a line of text must not cost a full-target blit.
            Rectangle rect = Text.CompositeBlitRectangle(
                400f, 300f, 900f, 360f,
                FontEffectSettings.None, 40, HalfScale, Target);

            Assert.True(rect.Width * rect.Height < Target.Width * Target.Height / 4);
        }

        [Fact]
        public void RectangleContainsTheLineBoxItWasGiven()
        {
            Rectangle rect = Text.CompositeBlitRectangle(
                400f, 300f, 900f, 360f,
                FontEffectSettings.None, 40, HalfScale, Target);

            // The line box maps to 200,150 - 450,180 under the half scale, and must be inside with room
            // to spare for glyph overhang.
            Assert.True(rect.Left < 200);
            Assert.True(rect.Top < 150);
            Assert.True(rect.Right > 450);
            Assert.True(rect.Bottom > 180);
        }

        [Fact]
        public void StrokeWidensTheRectangle()
        {
            FontEffectSettings stroked = new() { HasStroke = true, StrokeAmount = 6 };

            Rectangle plain = Text.CompositeBlitRectangle(
                400f, 300f, 900f, 360f, FontEffectSettings.None, 40, HalfScale, Target);
            Rectangle rect = Text.CompositeBlitRectangle(
                400f, 300f, 900f, 360f, stroked, 40, HalfScale, Target);

            Assert.True(rect.Left < plain.Left);
            Assert.True(rect.Right > plain.Right);
            Assert.True(rect.Top < plain.Top);
            Assert.True(rect.Bottom > plain.Bottom);
        }

        [Fact]
        public void ShadowExtendsTheRectangleInTheDirectionItFalls()
        {
            FontEffectSettings shadowed = new() { HasShadow = true, ShadowOffsetX = 40, ShadowOffsetY = 40 };

            Rectangle plain = Text.CompositeBlitRectangle(
                400f, 300f, 900f, 360f, FontEffectSettings.None, 40, HalfScale, Target);
            Rectangle rect = Text.CompositeBlitRectangle(
                400f, 300f, 900f, 360f, shadowed, 40, HalfScale, Target);

            // A shadow is drawn as a spread of offset copies, so it widens every side a little, but the
            // offset it is cast at has to dominate: far more room down and right than up and left.
            Assert.True(rect.Right - plain.Right > plain.Left - rect.Left);
            Assert.True(rect.Bottom - plain.Bottom > plain.Top - rect.Top);
        }

        [Fact]
        public void RectangleIsClippedToTheTarget()
        {
            // Text scrolled off the left edge, as ping-pong does mid-cycle.
            Rectangle rect = Text.CompositeBlitRectangle(
                -4000f, -200f, 200f, 200f,
                FontEffectSettings.None, 40, HalfScale, Target);

            Assert.True(rect.Left >= 0);
            Assert.True(rect.Top >= 0);
            Assert.True(rect.Right <= Target.Width);
            Assert.True(rect.Bottom <= Target.Height);
        }

        [Theory]
        [InlineData(1, 128)]
        [InlineData(128, 128)]
        [InlineData(129, 256)]
        [InlineData(500, 512)]
        [InlineData(512, 512)]
        public void TargetExtentRoundsUpAndNeverUnderCovers(int required, int expected)
        {
            // Under-covering would sample outside the target; rounding up is what keeps one target usable
            // across labels whose width changes by a pixel between frames.
            int extent = Text.CompositeTargetExtent(required);

            Assert.Equal(expected, extent);
            Assert.True(extent >= required);
        }

        [Fact]
        public void RotationIsBoundedByTheCornersNotTheAxes()
        {
            // A rotated element's axis-aligned extent is wider than the box it was given, so mapping two
            // corners instead of four would cut the corners off.
            Matrix rotated = Matrix.CreateRotationZ(MathHelper.PiOver4) * HalfScale;

            Rectangle rect = Text.CompositeBlitRectangle(
                400f, 300f, 900f, 360f, FontEffectSettings.None, 40, rotated, Target);

            Assert.True(rect.Width > 0);
            Assert.True(rect.Height > 0);
            foreach (Vector2 corner in new[]
            {
                new Vector2(400f, 300f),
                new Vector2(900f, 300f),
                new Vector2(400f, 360f),
                new Vector2(900f, 360f),
            })
            {
                Vector2 mapped = Vector2.Transform(corner, rotated);
                Assert.True(rect.Contains((int)mapped.X, (int)mapped.Y));
            }
        }
    }
}
