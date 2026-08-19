using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the painting behind a menu. It is authored at a scale that covers the design shape
    /// alone, so every viewport of another shape needs it re-covered - including the ones a scene
    /// is built at rather than resized into.
    /// </summary>
    public sealed class MenuBackdropCoverageTests
    {
        [Theory]
        [InlineData(MenuController.VIEW_PACK_SELECT)]
        [InlineData(MenuController.VIEW_MAIN_MENU)]
        public void ResizingLeavesTheBackdropCoveringTheScreen(int viewId)
        {
            // The pack picker rebuilds itself from inside the layout pass, after the pass has
            // already covered the backdrop it is about to throw away. Its replacement used to be
            // left at the authored scale, which on a phone painted the top half of the screen
            // black.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(viewId);

                    CtrRenderer.OnSurfaceChanged(720, 1280);

                    // The real resize path reaches a controller through the root's active child
                    // chain, which a controller built for a test is not on.
                    controller.RelayoutTree(ScreenPresentation.Instance.Snapshot);

                    CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                    Image backdrop = Backdrop(controller, viewId);
                    Assert.True(
                        backdrop.scaleX * backdrop.width >= visible.w,
                        $"the backdrop is {backdrop.scaleX * backdrop.width} wide on a {visible.w} viewport");
                    Assert.True(
                        backdrop.scaleY * backdrop.height >= visible.h,
                        $"the backdrop is {backdrop.scaleY * backdrop.height} tall on a {visible.h} viewport");
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        /// <summary>Reads the painted layer of a view's backdrop.</summary>
        /// <param name="controller">Controller owning the view.</param>
        /// <param name="viewId">View whose backdrop to read.</param>
        /// <returns>The backdrop image.</returns>
        private static Image Backdrop(MenuController controller, int viewId)
        {
            return (Image)controller.GetView(viewId).GetChild(0).GetChild(0);
        }
    }
}
