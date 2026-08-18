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
    /// Covers where a popup lands. Everything a popup is made of is positioned absolutely, in the
    /// design box's own coordinates, so it used to appear centered only on a screen of the shape
    /// the game was drawn for and sat off toward a corner on every other.
    /// </summary>
    public sealed class PopupCenteringTests
    {
        [Fact]
        public void ThePopupLandsInTheMiddleOfTheScreen()
        {
            _ = HeadlessGame.Boot();

            foreach (LayoutSurface surface in LayoutSurfaces.All)
            {
                LayoutSurfaces.WithSurface(surface.Width, surface.Height, () =>
                {
                    MenuController controller = new(
                        (CTRRootController)Application.SharedRootController());
                    try
                    {
                        controller.ShowView(MenuController.VIEW_MAIN_MENU);
                        controller.ShowYesNoPopup(
                            "Are you sure you want to quit?",
                            MenuButtonId.ConfirmResetYes,
                            MenuButtonId.ConfirmResetNo);

                        Popup popup = (Popup)controller.ActiveView().GetChildWithName("popup");
                        Assert.NotNull(popup);
                        BaseElement panel = popup.ContentRoot.GetChild(0);
                        BaseElement.CalculateTopLeft(panel);

                        CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                        float centerX = panel.drawX + popup.ContentRoot.translateX + (panel.width / 2f);
                        float centerY = panel.drawY + popup.ContentRoot.translateY + (panel.height / 2f);

                        Assert.Equal(visible.w / 2f, centerX, 0.5);
                        Assert.Equal(visible.h / 2f, centerY, 0.5);
                    }
                    finally
                    {
                        controller.Dispose();
                    }
                });
            }
        }

        [Fact]
        public void TheDesignShapeMovesThePopupNotAtAll()
        {
            _ = HeadlessGame.Boot();

            LayoutSurfaces.WithSurface(2560, 1440, () =>
            {
                MenuController controller = new(
                    (CTRRootController)Application.SharedRootController());
                try
                {
                    controller.ShowView(MenuController.VIEW_MAIN_MENU);
                    controller.ShowYesNoPopup(
                        "Are you sure you want to quit?",
                        MenuButtonId.ConfirmResetYes,
                        MenuButtonId.ConfirmResetNo);

                    Popup popup = (Popup)controller.ActiveView().GetChildWithName("popup");

                    Assert.Equal(0f, popup.ContentRoot.translateX, 0.001);
                    Assert.Equal(0f, popup.ContentRoot.translateY, 0.001);
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }
    }
}
