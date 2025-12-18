using System;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal static class CandySelectionView
    {
        public static MenuView CreateCandySelection(
            IButtonDelegation buttonDelegate,
            out ScrollableContainer candyContainer)
        {
            MenuView menuView = new();

            BaseElement background = new()
            {
                width = (int)FrameworkTypes.SCREEN_WIDTH,
                height = (int)FrameworkTypes.SCREEN_HEIGHT
            }; // ensure child anchors use the full screen bounds instead of 0x0

            Image bgImage = Image.Image_createWithResIDQuad(Resources.Img.SkinBackground, 0);
            bgImage.anchor = bgImage.parentAnchor = 18; // center

            // Scale background to cover the whole screen (match other menu backgrounds)
            float bgScale = Math.Max(FrameworkTypes.SCREEN_WIDTH / bgImage.width, FrameworkTypes.SCREEN_HEIGHT / bgImage.height);
            bgImage.scaleX = bgImage.scaleY = bgScale;
            _ = background.AddChild(bgImage);

            // "Candy" button on top middle with button_idle
            Image candyBtnUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 4);
            Image candyBtnDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 5);

            // Add "Candy" text to the button images
            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            Text buttonText = new Text().InitWithFont(font);
            buttonText.SetString(Application.GetString("CANDY"));
            buttonText.anchor = buttonText.parentAnchor = 18;
            _ = candyBtnUp.AddChild(buttonText);

            Text buttonText2 = new Text().InitWithFont(font);
            buttonText2.SetString(Application.GetString("CANDY"));
            buttonText2.anchor = buttonText2.parentAnchor = 18;
            _ = candyBtnDown.AddChild(buttonText2);

            Button candyButton = new Button().InitWithUpElementDownElementandID(candyBtnUp, candyBtnDown, MenuButtonId.CandySelect);
            candyButton.delegateButtonDelegate = buttonDelegate;
            candyButton.anchor = candyButton.parentAnchor = 10;
            candyButton.y = 50f;

            _ = background.AddChild(candyButton);

            // Create scrollable content area with candy slots
            float containerWidth = 1300f;
            float containerHeight = 900f;

            // Create VBox to hold rows of candies
            VBox candyGrid = new VBox().InitWithOffsetAlignWidth(30f, 18, containerWidth);

            // Constants for candy slot layout
            const int CANDIES_PER_ROW = 4;
            const int TOTAL_CANDIES = 33;
            float slotWidth = 271f;  // button_available_idle width
            float slotSpacing = 40f;

            // Create rows of candy slots
            for (int row = 0; row < ((TOTAL_CANDIES + CANDIES_PER_ROW - 1) / CANDIES_PER_ROW); row++)
            {
                HBox rowBox = new HBox().InitWithOffsetAlignHeight(slotSpacing, 18, slotWidth + 80f);

                for (int col = 0; col < CANDIES_PER_ROW; col++)
                {
                    int candyIndex = (row * CANDIES_PER_ROW) + col;
                    if (candyIndex >= TOTAL_CANDIES)
                    {
                        break;
                    }

                    // Create candy slot button (button_available_idle/pressed as background)
                    Image slotBgUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 0);
                    Image slotBgDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 1);

                    // Add candy image on top of slot background (candy01-candy33 are quads 6-38)
                    int candyQuadIndex = 6 + candyIndex;
                    Image candyImage = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, candyQuadIndex);
                    candyImage.anchor = candyImage.parentAnchor = 18;
                    _ = slotBgUp.AddChild(candyImage);

                    Image candyImage2 = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, candyQuadIndex);
                    candyImage2.anchor = candyImage2.parentAnchor = 18;
                    _ = slotBgDown.AddChild(candyImage2);

                    Button slotButton = new Button().InitWithUpElementDownElementandID(
                        slotBgUp, slotBgDown, MenuButtonId.ForCandySlot(candyIndex));
                    slotButton.delegateButtonDelegate = buttonDelegate;

                    _ = rowBox.AddChild(slotButton);
                }

                _ = candyGrid.AddChild(rowBox);
            }

            // Create scrollable container with culling
            candyContainer = new ScrollableContainer().InitWithWidthHeightContainer(containerWidth, containerHeight, candyGrid);
            candyContainer.anchor = candyContainer.parentAnchor = 18;
            candyContainer.y = 100f;

            _ = background.AddChild(candyContainer);

            // Add cut_top border (quad 59)
            Image cutTop = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 59);
            cutTop.anchor = cutTop.parentAnchor = 10;
            cutTop.y = candyContainer.y - (containerHeight / 2f);
            _ = background.AddChild(cutTop);

            // Add cut_bottom border (quad 60)
            Image cutBottom = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 60);
            cutBottom.anchor = cutBottom.parentAnchor = 26;
            cutBottom.y = candyContainer.y + (containerHeight / 2f);
            _ = background.AddChild(cutBottom);

            _ = menuView.AddChild(background);

            // Back button to return to main menu
            Button backButton = MenuController.CreateBackButtonWithDelegateID(buttonDelegate, MenuButtonId.BackFromCandySelect);
            backButton.SetName("backb");
            backButton.x = FrameworkTypes.Canvas.xOffsetScaled;
            _ = menuView.AddChild(backButton);

            return menuView;
        }
    }
}
