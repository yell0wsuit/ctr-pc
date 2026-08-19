using System.Reflection;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the About view's response to a resize. Its content scale sets the wrap width every
    /// credits block was measured at, so it cannot simply be reapplied - the view is rebuilt when
    /// the viewport no longer matches the one it was built for.
    /// </summary>
    public sealed class AboutViewResizeTests
    {
        [Fact]
        public void ResizingToADifferentShapeRebuildsTheCreditsAtTheNewScale()
        {
            // Built landscape, then resized to portrait. The credits used to keep the scale they
            // were constructed with for the life of the view, so a window dragged into portrait
            // left them sized for a shape that was no longer on screen.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(MenuController.VIEW_ABOUT);
                    float builtLandscape = ReadBuiltForScale(controller);
                    Assert.Equal(ContentFit.Scale, builtLandscape, 0.0001);

                    CtrRenderer.OnSurfaceChanged(720, 1280);
                    controller.ShowView(MenuController.VIEW_ABOUT);

                    Assert.Equal(ContentFit.Scale, ReadBuiltForScale(controller), 0.0001);
                    Assert.True(
                        ReadBuiltForScale(controller) > builtLandscape,
                        "portrait should boost the credits above the landscape scale");
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        [Fact]
        public void ALayoutPassAtTheSameShapeLeavesTheCreditsAlone()
        {
            // Rebuilding is only warranted by a scale change. A pass that rebuilt regardless would
            // throw away the reader's scroll position on every unrelated relayout.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(1280, 720, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(MenuController.VIEW_ABOUT);
                    object firstView = controller.GetView(MenuController.VIEW_ABOUT);

                    controller.ShowView(MenuController.VIEW_ABOUT);

                    Assert.Same(firstView, controller.GetView(MenuController.VIEW_ABOUT));
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }

        private static float ReadBuiltForScale(MenuController controller)
        {
            FieldInfo field = typeof(MenuController).GetField(
                "aboutView",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            AboutView about = (AboutView)field.GetValue(controller);
            Assert.NotNull(about);
            return about.BuiltForScale;
        }
    }
}
