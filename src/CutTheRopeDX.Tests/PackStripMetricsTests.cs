using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers how the pack picker's strip of boxes divides a viewport. The strip cannot hang from
    /// a fitted group, so it derives its own scale and its own box count; these pin that the two
    /// agree - a box is drawn at the scale the rest of the menu is, and the strip is only ever as
    /// many boxes wide as the viewport has room for at that scale.
    /// </summary>
    public sealed class PackStripMetricsTests
    {
        /// <summary>Authored width of one box, including its quad offset padding.</summary>
        private const float BoxWidth = 660f;

        [Theory]
        [InlineData(2560, 1440, 3)]
        [InlineData(1280, 720, 3)]
        [InlineData(2560, 1080, 3)]
        [InlineData(1024, 768, 2)]
        [InlineData(1000, 1000, 1)]
        [InlineData(720, 1280, 1)]
        [InlineData(400, 1280, 1)]
        public void TheStripIsAsManyBoxesWideAsTheViewportHasRoomFor(int width, int height, int expected)
        {
            Assert.Equal(expected, LayoutFor(width, height).VisibleBoxes);
        }

        [Fact]
        public void ABoxIsDrawnAtTheScaleTheRestOfTheMenuIs()
        {
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                PackStripLayout strip = LayoutFor(surface.Width, surface.Height);
                Assert.Equal(BoxWidth * ScaleFor(surface), strip.BoxWidth, 0.0001);
            }
        }

        [Fact]
        public void TheBoxesTheStripShowsFitInsideIt()
        {
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                CTRRectangle visible = VisibleFor(surface);
                PackStripLayout strip = LayoutFor(surface.Width, surface.Height);

                Assert.True(
                    strip.StripWidth <= visible.w,
                    $"{surface.Name}: a {strip.StripWidth} strip on a {visible.w} viewport");
            }
        }

        [Fact]
        public void TheSelectedBoxSitsInTheMiddleOfTheStrip()
        {
            // The strip is led by a spacer a whole box wide while its boxes overlap, so the offset
            // that centers the selected box has to give that overlap back. Without it the box sat
            // one overlap left of the middle at every width.
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                PackStripLayout strip = LayoutFor(surface.Width, surface.Height);

                Assert.Equal((strip.StripWidth - strip.BoxWidth) / 2f, strip.SelectedBoxLeft, 0.0001);
                Assert.True(strip.PackOffset >= 0f, $"{surface.Name}: the strip scrolls before its own start");
            }
        }

        [Fact]
        public void TheArtworkOfTheSelectedBoxIsDrawnWholeAtEveryWidth()
        {
            // The scroll points are laid out for a three-box strip, and a narrower one gives back
            // half a box of scroll per slot it dropped. Get that wrong on the strip that is one
            // box wide - the one with no slack at all - and the selected box is drawn with an edge
            // outside the strip, where it is clipped away.
            _ = HeadlessGame.Boot();

            PackDefinition pack = PackConfig.Packs[0];
            float artLeftInBox = Image.GetQuadOffset(pack.PackSpritesheet, pack.PackQuadIndex).X;
            float artWidth = Image.GetQuadSize(pack.PackSpritesheet, pack.PackQuadIndex).X;

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    PackStripLayout strip = MenuController.PackStrip();

                    float boxLeft = strip.SelectedBoxLeft;

                    Assert.True(
                        boxLeft + (artLeftInBox * strip.Scale) >= 0f,
                        $"{surface.Name}: the selected box is drawn off the left of the strip");
                    Assert.True(
                        boxLeft + ((artLeftInBox + artWidth) * strip.Scale) <= strip.StripWidth,
                        $"{surface.Name}: the selected box is drawn off the right of the strip");
                });
            }
        }

        [Fact]
        public void TheHoleTheSelectedBoxRevealsOmNomThroughIsWhollyInsideTheStrip()
        {
            // Om Nom is drawn where the selected box comes to rest, and what shows of him is
            // whatever the hole in that box is over. Lose part of the hole off the edge of the
            // strip and he is cut off before the scroll ever moves.
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    PackStripLayout strip = MenuController.PackStrip();
                    CTRRectangle window = FrameworkTypes.MakeRectangle(
                        0f,
                        0f,
                        strip.StripWidth,
                        ScreenPresentation.Instance.Snapshot.VisibleBounds.h);
                    float boxLeftAtRest = strip.SelectedBoxLeft;

                    CTRRectangle hole = MenuController.MonsterSlot.RevealWindow(
                        boxLeftAtRest,
                        strip.Scale,
                        window);
                    CTRRectangle unclipped = MenuController.MonsterSlot.RevealWindow(
                        boxLeftAtRest,
                        strip.Scale,
                        FrameworkTypes.MakeRectangle(-window.w, 0f, window.w * 3f, window.h));

                    Assert.Equal(unclipped.w, hole.w, 0.0001);
                    Assert.True(unclipped.w > 0f, $"{surface.Name}: the hole has no width");
                });
            }
        }

        [Fact]
        public void TheArtworkIsTheWidthTheseCasesAssume()
        {
            // The cases above measure a box from a constant rather than the texture, so that they
            // need no content loaded. This is the one that ties the constant to the artwork.
            _ = HeadlessGame.Boot();

            Assert.Equal(BoxWidth, MenuController.GetBoxWidth(), 0.0001);
        }

        /// <summary>Builds the layout for a surface size.</summary>
        /// <param name="width">Surface width in pixels.</param>
        /// <param name="height">Surface height in pixels.</param>
        /// <returns>The layout.</returns>
        private static PackStripLayout LayoutFor(int width, int height)
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(width, height);
            return PackStripLayout.For(
                snapshot.VisibleBounds,
                ContentFit.ScaleForAspect(snapshot.Aspect),
                BoxWidth);
        }

        /// <summary>The region a surface exposes.</summary>
        /// <param name="surface">Surface to measure.</param>
        /// <returns>The visible bounds.</returns>
        private static CTRRectangle VisibleFor(LayoutSurface surface)
        {
            return ViewportLayout.Compute(surface.Width, surface.Height).VisibleBounds;
        }

        /// <summary>The content scale a surface is drawn at.</summary>
        /// <param name="surface">Surface to measure.</param>
        /// <returns>The scale.</returns>
        private static float ScaleFor(LayoutSurface surface)
        {
            return ContentFit.ScaleForAspect(ViewportLayout.Compute(surface.Width, surface.Height).Aspect);
        }
    }
}
