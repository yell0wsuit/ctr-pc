using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the size of a menu's back button. Two rules bear on it - the scale the menus around
    /// it are drawn at, and the size the surface needs it to be to stay reachable - and it has to
    /// satisfy both.
    /// </summary>
    public sealed class BackButtonScaleTests
    {
        [Fact]
        public void TheDesignShapeDrawsTheButtonAtItsAuthoredSize()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () => Assert.Equal(1f, BackButton().scaleX, 0.001));
        }

        [Fact]
        public void TheButtonIsNeverSmallerThanTheMenuAroundIt()
        {
            // Sized by the reachability floor alone, it stayed at its authored size on every
            // ordinary window - a 16:9-sized button in the corner of a menu drawn half again as
            // large.
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    Button button = BackButton();

                    Assert.True(
                        button.scaleX >= ContentFit.Scale - 0.001f,
                        $"{surface.Name}: a {button.scaleX} button in a {ContentFit.Scale} menu");
                });
            }
        }

        [Fact]
        public void TheReachabilityFloorStillWinsWhereItAsksForMore()
        {
            // A small dense surface needs the button larger than the menus around it, and that is
            // the whole point of the floor.
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(320, 480, () =>
            {
                Button button = BackButton();
                float floor = HudMetrics.ChromeSize(ScreenPresentation.Instance.Snapshot, false)
                    / MathF.Max(button.width, button.height);

                Assert.True(floor > ContentFit.Scale, "the fixture surface should be floor-driven");
                Assert.Equal(floor, button.scaleX, 0.001);
            });
        }

        [Fact]
        public void TheButtonTakesWhicheverRuleAsksForMore()
        {
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    Button button = BackButton();
                    float floor = HudMetrics.ChromeSize(ScreenPresentation.Instance.Snapshot, false)
                        / MathF.Max(button.width, button.height);

                    Assert.Equal(MathF.Max(ContentFit.Scale, floor), button.scaleX, 0.001);
                    Assert.Equal(button.scaleX, button.scaleY, 0.001);
                });
            }
        }

        /// <summary>Builds a menu and returns the back button of its About view.</summary>
        /// <returns>The back button, laid out for the current surface.</returns>
        private static Button BackButton()
        {
            MenuController controller = new((CTRRootController)Application.SharedRootController());
            try
            {
                controller.ShowView(MenuController.VIEW_ABOUT);
                Button button = controller.GetView(MenuController.VIEW_ABOUT)
                    .GetChildWithName("backb") as Button;
                Assert.NotNull(button);
                return button;
            }
            finally
            {
                controller.Dispose();
            }
        }
    }
}
