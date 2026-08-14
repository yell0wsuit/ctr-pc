using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins the pointer path: a position in surface pixels resolves to a logical position, and
    /// that logical position hits the element drawn under it. A mismatch here is invisible in a
    /// screenshot and only shows up as a button that will not press.
    /// </summary>
    public sealed class PointerCharacterizationTests
    {
        [Theory]
        [MemberData(nameof(Surfaces))]
        public void SurfaceCenterMapsToTheLogicalCenter(string name, int width, int height)
        {
            ScreenPresentation presentation = new(2560, 1440) { FullScreenCropWidth = false };
            presentation.SetSurfaceSize(width, height);

            int surfaceX = presentation.ScaledViewX + (presentation.ScaledViewWidth / 2);
            int surfaceY = presentation.ScaledViewY + (presentation.ScaledViewHeight / 2);

            float logicalX = presentation.TransformViewToGameX(
                presentation.TransformWindowToViewX(surfaceX));
            float logicalY = presentation.TransformViewToGameY(
                presentation.TransformWindowToViewY(surfaceY));

            Assert.Equal(1280f, logicalX, 3.5);
            Assert.Equal(720f, logicalY, 3.5);
            Assert.False(string.IsNullOrEmpty(name));
        }

        [Theory]
        [MemberData(nameof(Surfaces))]
        public void ScaledViewCornersMapToTheLogicalCorners(string name, int width, int height)
        {
            ScreenPresentation presentation = new(2560, 1440) { FullScreenCropWidth = false };
            presentation.SetSurfaceSize(width, height);

            float topLeftX = presentation.TransformViewToGameX(
                presentation.TransformWindowToViewX(presentation.ScaledViewX));
            float topLeftY = presentation.TransformViewToGameY(
                presentation.TransformWindowToViewY(presentation.ScaledViewY));
            float bottomRightX = presentation.TransformViewToGameX(
                presentation.TransformWindowToViewX(
                    presentation.ScaledViewX + presentation.ScaledViewWidth));
            float bottomRightY = presentation.TransformViewToGameY(
                presentation.TransformWindowToViewY(
                    presentation.ScaledViewY + presentation.ScaledViewHeight));

            Assert.Equal(0f, topLeftX, 0.01);
            Assert.Equal(0f, topLeftY, 0.01);
            Assert.Equal(2560f, bottomRightX, 0.01);
            Assert.Equal(1440f, bottomRightY, 0.01);
            Assert.False(string.IsNullOrEmpty(name));
        }

        [Fact]
        public void APointerOnAButtonHitsThatButtonToday()
        {
            _ = HeadlessGame.Boot();
            MenuController controller = new(
                (CTRRootController)Application.SharedRootController());

            try
            {
                controller.ShowView(MenuController.VIEW_MAIN_MENU);
                controller.Update(0.016f);
                View view = controller.ActiveView();
                _ = ElementGeometryWalker.Describe(view);

                Button button = FindFirstButton(view);
                Assert.NotNull(button);

                float centerX = button.drawX + (button.width / 2f);
                float centerY = button.drawY + (button.height / 2f);

                Assert.True(button.OnTouchDownXY(centerX, centerY));
            }
            finally
            {
                controller.Dispose();
            }
        }

        [Fact]
        public void APointerOutsideAButtonMissesIt()
        {
            _ = HeadlessGame.Boot();
            MenuController controller = new(
                (CTRRootController)Application.SharedRootController());

            try
            {
                controller.ShowView(MenuController.VIEW_MAIN_MENU);
                controller.Update(0.016f);
                View view = controller.ActiveView();
                _ = ElementGeometryWalker.Describe(view);

                Button button = FindFirstButton(view);
                Assert.NotNull(button);

                Assert.False(button.OnTouchDownXY(button.drawX - 500f, button.drawY - 500f));
            }
            finally
            {
                controller.Dispose();
            }
        }

        [Fact]
        public void HitTestingIgnoresElementScaleToday()
        {
            // Recorded, not endorsed. Rendering applies scaleX/scaleY, but Button hit-tests the
            // unscaled rectangle. Resetting the state between probes ensures the second miss is
            // caused by geometry rather than the first probe leaving the button pressed.
            Button button = BuildButton();
            button.scaleX = 2f;
            button.scaleY = 2f;
            BaseElement.CalculateTopLeft(button);

            Assert.True(button.OnTouchDownXY(50f, 50f));
            button.SetState(Button.BUTTON_STATE.BUTTON_UP);
            Assert.False(button.OnTouchDownXY(150f, 150f));
        }

        private static Button BuildButton()
        {
            Button button = new Button().InitWithUpElementDownElementandID(
                new BaseElement { width = 100, height = 100 },
                new BaseElement { width = 100, height = 100 },
                0);
            button.width = 100;
            button.height = 100;
            return button;
        }

        private static Button FindFirstButton(BaseElement root)
        {
            if (root is Button found)
            {
                return found;
            }

            Dictionary<int, BaseElement> children = root.GetChilds();
            int emitted = 0;
            int childId = 0;
            while (emitted < children.Count)
            {
                if (children.TryGetValue(childId, out BaseElement child))
                {
                    if (child != null)
                    {
                        Button inChild = FindFirstButton(child);
                        if (inChild != null)
                        {
                            return inChild;
                        }
                    }
                    emitted++;
                }
                childId++;
            }

            return null;
        }

        public static TheoryData<string, int, int> Surfaces()
        {
            return LayoutSurfaces.Theory();
        }
    }
}
