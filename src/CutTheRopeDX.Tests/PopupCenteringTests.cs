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
                        BaseElement.CalculateTopLeft(popup);
                        BaseElement.CalculateTopLeft(popup.ContentRoot);
                        BaseElement.CalculateTopLeft(panel);

                        CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                        float centerX = panel.drawX + (panel.width / 2f);
                        float centerY = panel.drawY + (panel.height / 2f);

                        // Within a unit: the anchor math centers with an integer halving, so a
                        // viewport of odd logical width loses the fraction.
                        Assert.Equal(visible.w / 2f, centerX, 1.01);
                        Assert.Equal(visible.h / 2f, centerY, 1.01);
                    }
                    finally
                    {
                        controller.Dispose();
                    }
                });
            }
        }

        [Fact]
        public void APopupButtonIsPressedWhereItIsDrawn()
        {
            // The popup used to be carried to the middle of the screen by moving its drawing
            // alone, which left every rectangle it is pressed by back where the design box would
            // have put it - a button that answered a click a few hundred units to its left.
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
                        Button button = FirstButton(popup.ContentRoot);
                        Assert.NotNull(button);

                        BaseElement.CalculateTopLeft(popup);
                        BaseElement.CalculateTopLeft(popup.ContentRoot);
                        BaseElement.CalculateTopLeft(button);

                        // A press at the middle of where the button is drawn has to reach it, and
                        // one at the middle of where the design box alone would have put it must
                        // not - otherwise the two have merely been left in the same place.
                        float drawnX = button.drawX + (button.width / 2f);
                        float drawnY = button.drawY + (button.height / 2f);
                        Assert.True(
                            button.OnTouchDownXY(drawnX, drawnY),
                            $"{surface.Name}: the button ignored a press at {drawnX},{drawnY}");
                    }
                    finally
                    {
                        controller.Dispose();
                    }
                });
            }
        }

        /// <summary>Returns the first button in an element tree.</summary>
        /// <param name="element">Element to search from.</param>
        /// <returns>The button, or <see langword="null"/> when the tree holds none.</returns>
        private static Button FirstButton(BaseElement element)
        {
            if (element is Button button)
            {
                return button;
            }

            foreach (BaseElement child in element.GetChilds().Values)
            {
                Button found = child == null ? null : FirstButton(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
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

                    BaseElement panel = popup.ContentRoot.GetChild(0);
                    BaseElement.CalculateTopLeft(popup.ContentRoot);
                    BaseElement.CalculateTopLeft(panel);

                    Assert.Equal(0f, popup.ContentRoot.drawX, 0.001);
                    Assert.Equal(0f, popup.ContentRoot.drawY, 0.001);
                    Assert.Equal(0f, panel.drawX, 0.001);
                }
                finally
                {
                    controller.Dispose();
                }
            });
        }
    }
}
