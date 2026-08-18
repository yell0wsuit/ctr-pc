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

        /// <summary>Builds the layout for a surface size.</summary>
        /// <param name="width">Surface width in pixels.</param>
        /// <param name="height">Surface height in pixels.</param>
        /// <returns>The layout.</returns>
        private static SkinSelectionLayout LayoutFor(int width, int height)
        {
            ViewportLayoutSnapshot snapshot = ViewportLayout.Compute(width, height);
            return SkinSelectionLayout.For(
                snapshot.VisibleBounds,
                ContentFit.ScaleForAspect(snapshot.Aspect),
                TabWidth,
                TabHeight,
                TabCount);
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
