using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the skin selection screen across a resize. A slot owns an animated preview that is
    /// expensive to build, so a viewport that changes shape has to move the slots it already has
    /// rather than make new ones.
    /// </summary>
    public sealed class SkinSelectionResizeTests
    {
        [Fact]
        public void ResizingDealsTheSameSlotsIntoRowsOfTheNewWidth()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(MenuController.VIEW_CANDY_SELECT);
                    Assert.Equal(4, ColumnsOnScreen(controller));
                    BaseElement firstSlot = FirstSlot(controller);

                    CtrRenderer.OnSurfaceChanged(720, 1280);
                    controller.RelayoutTree(ScreenPresentation.Instance.Snapshot);

                    Assert.Equal(3, ColumnsOnScreen(controller));
                    Assert.Same(firstSlot, FirstSlot(controller));
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        [Fact]
        public void ResizingDealsTheOmNomSlotsIntoRowsToo()
        {
            // Om Nom's slots are the expensive ones - each carries an animated preview - and they
            // are built on a tab the resize did not have on screen, so they have to be dealt into
            // new rows where they sit in the cache.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(MenuController.VIEW_CANDY_SELECT);
                    CandySelectionView.SwitchToOmNomMode();
                    Assert.Equal(4, ColumnsOnScreen(controller));
                    BaseElement firstSlot = FirstSlot(controller);

                    CtrRenderer.OnSurfaceChanged(720, 1280);
                    controller.RelayoutTree(ScreenPresentation.Instance.Snapshot);

                    Assert.Equal(3, ColumnsOnScreen(controller));
                    Assert.Same(firstSlot, FirstSlot(controller));
                }
                finally
                {
                    CandySelectionView.SwitchToCandyMode();
                    controller.Dispose();
                }
            });
        }

        [Fact]
        public void ResizingFitsTheWindowToTheNewViewport()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(MenuController.VIEW_CANDY_SELECT);

                    CtrRenderer.OnSurfaceChanged(720, 1280);
                    controller.RelayoutTree(ScreenPresentation.Instance.Snapshot);

                    ScrollableContainer window = Window(controller);
                    float visibleHeight = ScreenPresentation.Instance.Snapshot.VisibleBounds.h;
                    Assert.True(
                        window.height > 1100f,
                        $"a {window.height} window on a {visibleHeight} viewport");
                    Assert.True(
                        window.y + window.height <= visibleHeight,
                        "the window runs past the bottom of the screen");
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        /// <summary>How many slots the first row of the grid on screen holds.</summary>
        /// <param name="controller">Controller owning the selection view.</param>
        /// <returns>The column count.</returns>
        private static int ColumnsOnScreen(MenuController controller)
        {
            return Grid(controller).GetChild(0).ChildsCount();
        }

        /// <summary>The first slot of the grid on screen.</summary>
        /// <param name="controller">Controller owning the selection view.</param>
        /// <returns>The slot button.</returns>
        private static BaseElement FirstSlot(MenuController controller)
        {
            return Grid(controller).GetChild(0).GetChild(0);
        }

        /// <summary>The grid attached to the scrolling window.</summary>
        /// <param name="controller">Controller owning the selection view.</param>
        /// <returns>The grid.</returns>
        private static BaseElement Grid(MenuController controller)
        {
            BaseElement grid = Window(controller).GetChild(0);
            Assert.NotNull(grid);
            return grid;
        }

        /// <summary>The window the grid scrolls inside.</summary>
        /// <param name="controller">Controller owning the selection view.</param>
        /// <returns>The container.</returns>
        private static ScrollableContainer Window(MenuController controller)
        {
            ScrollableContainer found = FindScroller(controller.GetView(MenuController.VIEW_CANDY_SELECT));
            Assert.NotNull(found);
            return found;
        }

        /// <summary>Returns the first scrolling container in an element tree.</summary>
        /// <param name="element">Element to search from.</param>
        /// <returns>The container, or <see langword="null"/> when the tree holds none.</returns>
        private static ScrollableContainer FindScroller(BaseElement element)
        {
            if (element is ScrollableContainer scroller)
            {
                return scroller;
            }

            foreach (BaseElement child in element.GetChilds().Values)
            {
                ScrollableContainer found = child == null ? null : FindScroller(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
