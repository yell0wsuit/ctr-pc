using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal static class CandySelectionView
    {
        // Store candy slot button data for quick updates
        private static readonly List<CandyButtonData> candyButtons = [];

        private class CandyButtonData
        {
            public int CandyIndex { get; set; }
            public Image UpImage { get; set; }
            public Image DownImage { get; set; }
        }

        /// <summary>
        /// Updates all candy slot buttons to reflect the newly selected candy skin.
        /// This updates the button backgrounds without recreating the entire view.
        /// </summary>
        public static void UpdateCandySlotButtons(int newSelectedCandyIndex)
        {
            // Update all stored button backgrounds
            foreach (CandyButtonData buttonData in candyButtons)
            {
                bool isEquipped = buttonData.CandyIndex == newSelectedCandyIndex;
                int bgUpQuad = isEquipped ? 2 : 0;   // button_equipped_idle : button_available_idle
                int bgDownQuad = isEquipped ? 3 : 1; // button_equipped_pressed : button_available_pressed

                buttonData.UpImage.SetDrawQuad(bgUpQuad);
                buttonData.DownImage.SetDrawQuad(bgDownQuad);
            }
        }

        public static MenuView CreateCandySelection(
            IButtonDelegation buttonDelegate,
            out ScrollableContainer candyContainer)
        {
            MenuView menuView = new();

            // Get current selected candy skin (0-50 for candy01-candy51)
            int selectedCandySkin = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY);

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

            // Candy button on top middle with button_idle
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
            const int CANDIES_PER_ROW = 4;
            const int TOTAL_CANDIES = 51;

            // Sprite sheet dimensions
            float spriteSheetSlotWidth = 271f;
            float spriteSheetSlotHeight = 336f;
            float spriteSheetScale = 3f; // bigger => smaller rendering

            // Actual rendered dimensions after sprite sheet scale
            float baseSlotWidth = spriteSheetSlotWidth * spriteSheetScale;
            float baseSlotHeight = spriteSheetSlotHeight * spriteSheetScale;
            float baseSpacing = 20f;

            // Calculate scale to fit 4 columns on screen
            float containerWidth = FrameworkTypes.SCREEN_WIDTH - 20f; // Leave margin
            float totalBaseWidth = (baseSlotWidth * CANDIES_PER_ROW) + (baseSpacing * (CANDIES_PER_ROW - 1));
            float slotScale = containerWidth / totalBaseWidth;

            float slotHeight = baseSlotHeight * slotScale;
            float columnSpacing = baseSpacing;
            float rowSpacing = 10f;

            // Reduce row height to account for padding in button sprites
            float rowHeight = slotHeight * 0.4f; // 40% of full height to remove spacious vertical padding

            // Container height
            float containerHeight = 1100f; // Borrowed from credits view height

            // Clear previous button data
            candyButtons.Clear();

            // Create VBox to hold rows of candies (align 2 = top center)
            VBox candyGrid = new VBox().InitWithOffsetAlignWidth(rowSpacing, 2, containerWidth);

            // Create rows of candy slots
            for (int row = 0; row < ((TOTAL_CANDIES + CANDIES_PER_ROW - 1) / CANDIES_PER_ROW); row++)
            {
                HBox rowBox = new HBox().InitWithOffsetAlignHeight(columnSpacing, 16, rowHeight);

                for (int col = 0; col < CANDIES_PER_ROW; col++)
                {
                    int candyIndex = (row * CANDIES_PER_ROW) + col;
                    if (candyIndex >= TOTAL_CANDIES)
                    {
                        break;
                    }

                    // Create candy slot button using equipped state if this is the selected candy
                    bool isEquipped = candyIndex == selectedCandySkin;
                    int bgUpQuad = isEquipped ? 2 : 0;   // button_equipped_idle : button_available_idle
                    int bgDownQuad = isEquipped ? 3 : 1; // button_equipped_pressed : button_available_pressed

                    Image slotBgUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgUpQuad);
                    Image slotBgDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgDownQuad);

                    // Scale the button backgrounds to fit the grid
                    slotBgUp.scaleX = slotBgUp.scaleY = slotScale;
                    slotBgDown.scaleX = slotBgDown.scaleY = slotScale;

                    // Add candy image on top of slot background (candy01-candy51 are quads 6-56 in JSON)
                    int candyQuadIndex = 6 + candyIndex;
                    Image candyImage = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, candyQuadIndex);
                    candyImage.anchor = candyImage.parentAnchor = 18;
                    candyImage.y = -20f; // Move up to avoid covering checkmark on button_equipped
                    _ = slotBgUp.AddChild(candyImage);

                    Image candyImage2 = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, candyQuadIndex);
                    candyImage2.anchor = candyImage2.parentAnchor = 18;
                    candyImage2.y = -20f; // Move up to avoid covering checkmark on button_equipped
                    _ = slotBgDown.AddChild(candyImage2);

                    Button slotButton = new Button().InitWithUpElementDownElementandID(
                        slotBgUp, slotBgDown, MenuButtonId.ForCandySlot(candyIndex));
                    slotButton.delegateButtonDelegate = buttonDelegate;

                    // Store button data for later updates
                    candyButtons.Add(new CandyButtonData
                    {
                        CandyIndex = candyIndex,
                        UpImage = slotBgUp,
                        DownImage = slotBgDown
                    });

                    _ = rowBox.AddChild(slotButton);
                }

                _ = candyGrid.AddChild(rowBox);
            }

            // Create scrollable container with culling (top and bottom)
            candyContainer = new ScrollableContainer().InitWithWidthHeightContainer(containerWidth, containerHeight, candyGrid);
            candyContainer.anchor = candyContainer.parentAnchor = 18;
            candyContainer.y = 50f;

            _ = background.AddChild(candyContainer);

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
