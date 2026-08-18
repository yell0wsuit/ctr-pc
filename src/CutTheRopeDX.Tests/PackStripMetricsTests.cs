using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers how wide the pack picker's strip of boxes is. The strip cannot hang from a fitted
    /// group, so it derives its own scale and its own box count; these pin that the two agree -
    /// a box is drawn at the scale the rest of the menu is, and the strip is only ever as many
    /// boxes wide as the viewport has room for at that scale.
    /// </summary>
    /// <remarks>
    /// Every case runs inside <see cref="LayoutSurfaces.WithSurface"/>, because the surface size
    /// is process-wide and the suite runs serially.
    /// </remarks>
    public sealed class PackStripMetricsTests
    {
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
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(width, height, () =>
                Assert.Equal(expected, MenuController.GetVisibleBoxCount()));
        }

        [Fact]
        public void ABoxIsDrawnAtTheScaleTheRestOfTheMenuIs()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(720, 1280, () =>
            {
                Assert.True(ContentFit.Scale > 1f, "the fixture viewport should boost content above one");
                Assert.Equal(
                    MenuController.GetBoxWidth() * ContentFit.Scale,
                    MenuController.GetScaledBoxWidth(),
                    0.0001);
            });
        }

        [Fact]
        public void TheBoxesTheStripShowsFitInsideIt()
        {
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    float strip = MenuController.GetScaledBoxWidth() * MenuController.GetVisibleBoxCount();
                    Assert.True(
                        strip <= ScreenPresentation.Instance.Snapshot.VisibleBounds.w,
                        $"{surface.Name}: the strip is {strip} wide on a viewport that only shows "
                        + $"{ScreenPresentation.Instance.Snapshot.VisibleBounds.w}");
                });
            }
        }

        [Fact]
        public void TheSelectedBoxIsDrawnWholeAtEveryWidth()
        {
            // The scroll points are laid out for a three-box strip, and a narrower one gives back
            // half a box of scroll per slot it dropped. Get that wrong on the strip that is one
            // box wide - the one with no slack at all - and the selected box is drawn with an
            // edge outside the strip, where it is clipped away.
            _ = HeadlessGame.Boot();

            PackDefinition pack = PackConfig.Packs[0];
            float artLeftInBox = Image.GetQuadOffset(pack.PackSpritesheet, pack.PackQuadIndex).X;
            float artWidth = Image.GetQuadSize(pack.PackSpritesheet, pack.PackQuadIndex).X;

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    float box = MenuController.GetScaledBoxWidth();
                    float scale = box / MenuController.GetBoxWidth();
                    float strip = box * MenuController.GetVisibleBoxCount();

                    // Where the selected box lands inside the strip: the leading spacer and the
                    // gap after it, less the scroll the pack offset holds the strip at.
                    float boxLeft = box + (BoxSpacing * scale) - MenuController.GetPackOffset();

                    Assert.True(
                        boxLeft + (artLeftInBox * scale) >= 0f,
                        $"{surface.Name}: the selected box is drawn off the left of the strip");
                    Assert.True(
                        boxLeft + ((artLeftInBox + artWidth) * scale) <= strip,
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
                    float box = MenuController.GetScaledBoxWidth();
                    float scale = box / MenuController.GetBoxWidth();
                    CTRRectangle strip = FrameworkTypes.MakeRectangle(
                        0f,
                        0f,
                        box * MenuController.GetVisibleBoxCount(),
                        ScreenPresentation.Instance.Snapshot.VisibleBounds.h);
                    float boxLeftAtRest = box + (BoxSpacing * scale) - MenuController.GetPackOffset();

                    CTRRectangle window = MenuController.MonsterSlot.RevealWindow(boxLeftAtRest, scale, strip);
                    CTRRectangle unclipped = MenuController.MonsterSlot.RevealWindow(
                        boxLeftAtRest,
                        scale,
                        FrameworkTypes.MakeRectangle(-strip.w, 0f, strip.w * 3f, strip.h));

                    Assert.Equal(unclipped.w, window.w, 0.0001);
                    Assert.True(unclipped.w > 0f, $"{surface.Name}: the hole has no width");
                });
            }
        }

        /// <summary>Authored gap between two boxes in the strip; they overlap slightly.</summary>
        private const float BoxSpacing = -20f;

    }
}
