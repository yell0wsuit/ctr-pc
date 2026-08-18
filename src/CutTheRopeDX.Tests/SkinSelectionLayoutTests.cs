using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers how the skin selection screen divides a viewport: the slot size, how many fit
    /// across, how the tabs wrap, and what is left for the grid to scroll in.
    /// </summary>
    public sealed class SkinSelectionLayoutTests
    {
        /// <summary>Authored width of a tab button.</summary>
        private const float TabWidth = 340f;

        /// <summary>Authored height of a tab button.</summary>
        private const float TabHeight = 140f;

        /// <summary>How many tabs the screen has.</summary>
        private const int TabCount = 4;

        [Theory]
        [InlineData(2560, 1440, 4)]
        [InlineData(1280, 720, 4)]
        [InlineData(2560, 1080, 4)]
        [InlineData(1024, 768, 4)]
        [InlineData(1000, 1000, 4)]
        [InlineData(720, 1280, 3)]
        [InlineData(400, 1280, 3)]
        public void TheGridIsAsManySlotsWideAsTheViewportHasRoomFor(int width, int height, int expected)
        {
            Assert.Equal(expected, LayoutFor(width, height).Columns);
        }

        [Fact]
        public void ASlotIsTheSameSizeRelativeToEveryViewport()
        {
            // The slot used to be scaled by the container width divided between three of them, so
            // it shrank to 0.57 on a phone and grew past one on a desktop while the row spacing
            // stayed put. It is now the one content scale the rest of the menus use.
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                SkinSelectionLayout layout = LayoutFor(surface.Width, surface.Height);
                float scale = ContentFitFor(surface);

                Assert.Equal(271f * scale, layout.CellWidth, 0.01);
                Assert.Equal(336f * scale, layout.CellHeight, 0.01);
            }
        }

        [Fact]
        public void TheGridFitsInsideTheViewportItIsBuiltFor()
        {
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                CTRRectangle visible = VisibleFor(surface);
                SkinSelectionLayout layout = LayoutFor(surface.Width, surface.Height);

                Assert.True(
                    layout.GridWidth <= visible.w,
                    $"{surface.Name}: a {layout.GridWidth} grid on a {visible.w} viewport");
                Assert.True(
                    layout.WindowTop + layout.WindowHeight <= visible.h,
                    $"{surface.Name}: the window runs past the bottom of the screen");
                Assert.True(
                    layout.WindowHeight >= layout.CellHeight,
                    $"{surface.Name}: the window is shorter than one slot");
            }
        }

        [Fact]
        public void TheTabsWrapOntoEvenRowsWhenTheyDoNotFitAcross()
        {
            // A phone cannot hold four tabs across at the scale the rest of the screen is drawn.
            SkinSelectionLayout phone = LayoutFor(720, 1280);
            Assert.Equal(2, phone.TabRows);
            Assert.Equal(2, phone.TabsPerRow);

            SkinSelectionLayout desktop = LayoutFor(2560, 1440);
            Assert.Equal(1, desktop.TabRows);
            Assert.Equal(TabCount, desktop.TabsPerRow);
        }

        [Fact]
        public void EveryTabStaysOnTheScreen()
        {
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                CTRRectangle visible = VisibleFor(surface);
                SkinSelectionLayout layout = LayoutFor(surface.Width, surface.Height);
                float half = TabWidth * layout.Scale / 2f;

                for (int tab = 0; tab < TabCount; tab++)
                {
                    float center = (visible.w / 2f) + layout.TabX(tab, TabCount);
                    Assert.True(
                        center - half >= 0f && center + half <= visible.w,
                        $"{surface.Name}: tab {tab} spans {center - half} to {center + half} on a "
                        + $"{visible.w} viewport");
                }
            }
        }

        [Fact]
        public void TheGridStartsBelowTheLastRowOfTabs()
        {
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                SkinSelectionLayout layout = LayoutFor(surface.Width, surface.Height);
                float lastTabBottom = layout.TabTopFor(TabCount - 1) + layout.TabHeight;

                Assert.True(
                    layout.WindowTop > lastTabBottom,
                    $"{surface.Name}: the grid starts at {layout.WindowTop}, under tabs that end "
                    + $"at {lastTabBottom}");
            }
        }

        [Fact]
        public void TheGridStopsAboveTheButtonInTheCornerWhereItReachesIt()
        {
            // The button is drawn over the bottom of the window. Where the grid is wide enough to
            // reach that corner - a phone, where it spans most of the screen - the window has to
            // stop above it, or the bottom row is drawn behind it.
            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                CTRRectangle visible = VisibleFor(surface);
                SkinSelectionLayout layout = LayoutFor(surface.Width, surface.Height, ChromeSide, ChromeSide);

                if ((visible.w - layout.GridWidth) / 2f >= ChromeSide)
                {
                    continue;
                }

                Assert.True(
                    layout.WindowTop + layout.WindowHeight <= visible.h - ChromeSide,
                    $"{surface.Name}: the grid ends at {layout.WindowTop + layout.WindowHeight} on a "
                    + $"{visible.h} screen whose button rises to {visible.h - ChromeSide}");
            }
        }

        [Fact]
        public void AGridThatNeverReachesTheCornerKeepsItsAuthoredMargin()
        {
            // A wide screen draws the grid as a column in the middle, nowhere near the button, so
            // no height is given up for it.
            CTRRectangle visible = VisibleFor(new LayoutSurface("Native", 2560, 1440));
            SkinSelectionLayout withChrome = LayoutFor(2560, 1440, ChromeSide, ChromeSide);
            SkinSelectionLayout without = LayoutFor(2560, 1440, 0f, 0f);

            Assert.True((visible.w - withChrome.GridWidth) / 2f >= ChromeSide, "the fixture grid should clear the corner");
            Assert.Equal(without.WindowHeight, withChrome.WindowHeight, 0.0001);
        }

        /// <summary>Drawn size of the button in the corner, on the surfaces these cases use.</summary>
        private const float ChromeSide = 284f;

        /// <summary>Builds the layout for a surface size.</summary>
        /// <param name="width">Surface width in pixels.</param>
        /// <param name="height">Surface height in pixels.</param>
        /// <returns>The layout.</returns>
        private static SkinSelectionLayout LayoutFor(int width, int height)
        {
            return LayoutFor(width, height, 0f, 0f);
        }

        /// <summary>Builds the layout for a surface size, with chrome in the bottom corner.</summary>
        /// <param name="width">Surface width in pixels.</param>
        /// <param name="height">Surface height in pixels.</param>
        /// <param name="chromeWidth">Drawn width of the chrome.</param>
        /// <param name="chromeHeight">Drawn height of the chrome.</param>
        /// <returns>The layout.</returns>
        private static SkinSelectionLayout LayoutFor(
            int width,
            int height,
            float chromeWidth,
            float chromeHeight)
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(width, height);
            return SkinSelectionLayout.For(
                snapshot.VisibleBounds,
                ContentFit.ScaleForAspect(snapshot.Aspect),
                TabWidth,
                TabHeight,
                TabCount,
                chromeWidth,
                chromeHeight);
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
        private static float ContentFitFor(LayoutSurface surface)
        {
            return ContentFit.ScaleForAspect(ViewportLayout.Compute(surface.Width, surface.Height).Aspect);
        }
    }
}
