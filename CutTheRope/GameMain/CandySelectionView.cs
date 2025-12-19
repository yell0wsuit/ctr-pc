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
        private static readonly List<SlotButtonData> slotButtons = [];

        // Track current selection mode and UI references
        private static bool isRopeMode;
        private static ScrollableContainer currentContainer;
        private static BaseElement gridContainer;
        private static IButtonDelegation currentButtonDelegate;
        private static Button candyTabButton;
        private static Button ropeTabButton;

        private sealed class SlotButtonData
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
            foreach (SlotButtonData buttonData in slotButtons)
            {
                bool isEquipped = buttonData.CandyIndex == newSelectedCandyIndex;
                int bgUpQuad = isEquipped ? 2 : 0;   // button_equipped_idle : button_available_idle
                int bgDownQuad = isEquipped ? 3 : 1; // button_equipped_pressed : button_available_pressed

                buttonData.UpImage.SetDrawQuad(bgUpQuad);
                buttonData.DownImage.SetDrawQuad(bgDownQuad);
            }
        }

        /// <summary>
        /// Switches between candy and rope selection modes.
        /// </summary>
        public static void SwitchToMode(bool ropeMode)
        {
            if (isRopeMode == ropeMode || currentContainer == null)
            {
                return;
            }

            isRopeMode = ropeMode;
            UpdateTabButtonStates();
            RebuildGrid();
        }

        /// <summary>
        /// Updates the tab button visual states to show which mode is active.
        /// </summary>
        private static void UpdateTabButtonStates()
        {
            if (candyTabButton == null || ropeTabButton == null)
            {
                return;
            }

            // Update candy button state
            Image candyUpImage = (Image)candyTabButton.GetChild(0);
            Image candyDownImage = (Image)candyTabButton.GetChild(1);

            // Update rope button state
            Image ropeUpImage = (Image)ropeTabButton.GetChild(0);
            Image ropeDownImage = (Image)ropeTabButton.GetChild(1);

            switch (isRopeMode)
            {
                case true:
                    // Rope mode active: candy = idle, rope = pressed
                    candyUpImage.SetDrawQuad(4);   // button_idle
                    candyDownImage.SetDrawQuad(4); // button_idle (don't show pressed state for inactive tab)
                    ropeUpImage.SetDrawQuad(5);    // button_pressed
                    ropeDownImage.SetDrawQuad(5);  // button_pressed
                    break;
                case false:
                    // Candy mode active: candy = pressed, rope = idle
                    candyUpImage.SetDrawQuad(5);   // button_pressed
                    candyDownImage.SetDrawQuad(5); // button_pressed
                    ropeUpImage.SetDrawQuad(4);    // button_idle
                    ropeDownImage.SetDrawQuad(4);  // button_idle (don't show pressed state for inactive tab)
                    break;
            }
        }

        /// <summary>
        /// Creates a slot button with background and item image.
        /// </summary>
        private static Button CreateSlotButton(int itemIndex, int selectedIndex, int itemQuadIndex, float slotScale, MenuButtonId buttonId)
        {
            bool isEquipped = itemIndex == selectedIndex;
            int bgUpQuad = isEquipped ? 2 : 0;
            int bgDownQuad = isEquipped ? 3 : 1;

            Image slotBgUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgUpQuad);
            Image slotBgDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgDownQuad);

            slotBgUp.scaleX = slotBgUp.scaleY = slotScale;
            slotBgDown.scaleX = slotBgDown.scaleY = slotScale;

            // Add item image to both up and down states
            Image itemImage = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, itemQuadIndex);
            itemImage.anchor = itemImage.parentAnchor = 18;
            itemImage.y = -20f;
            _ = slotBgUp.AddChild(itemImage);

            Image itemImage2 = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, itemQuadIndex);
            itemImage2.anchor = itemImage2.parentAnchor = 18;
            itemImage2.y = -20f;
            _ = slotBgDown.AddChild(itemImage2);

            Button slotButton = new Button().InitWithUpElementDownElementandID(slotBgUp, slotBgDown, buttonId);
            slotButton.delegateButtonDelegate = currentButtonDelegate;

            // Store button data for later updates
            slotButtons.Add(new SlotButtonData
            {
                CandyIndex = itemIndex,
                UpImage = slotBgUp,
                DownImage = slotBgDown
            });

            return slotButton;
        }

        /// <summary>
        /// Rebuilds the grid based on the current mode (candy or rope).
        /// </summary>
        private static void RebuildGrid()
        {
            if (currentContainer == null)
            {
                return;
            }

            // Reset scroll position to top
            currentContainer.SetScroll(new Vector(0f, 0f));

            // Clear existing content - remove the old grid (child at index 0)
            if (currentContainer.ChildsCount() > 0)
            {
                currentContainer.RemoveChildWithID(0);
            }
            slotButtons.Clear();

            const int ITEMS_PER_ROW = 4;

            // Sprite sheet dimensions
            float spriteSheetSlotWidth = 271f;
            float spriteSheetSlotHeight = 336f;
            float spriteSheetScale = 3f;

            // Actual rendered dimensions after sprite sheet scale
            float baseSlotWidth = spriteSheetSlotWidth * spriteSheetScale;
            float baseSlotHeight = spriteSheetSlotHeight * spriteSheetScale;
            float baseSpacing = 20f;

            // Calculate scale to fit 4 columns on screen
            float containerWidth = FrameworkTypes.SCREEN_WIDTH - 20f;
            float totalBaseWidth = (baseSlotWidth * ITEMS_PER_ROW) + (baseSpacing * (ITEMS_PER_ROW - 1));
            float slotScale = containerWidth / totalBaseWidth;

            float slotHeight = baseSlotHeight * slotScale;
            float columnSpacing = baseSpacing;
            float rowSpacing = 10f;
            float rowHeight = slotHeight * 0.4f;

            VBox itemGrid = new VBox().InitWithOffsetAlignWidth(rowSpacing, 2, containerWidth);

            // Get mode-specific configuration
            int totalItems;
            int selectedIndex;
            int baseQuadIndex;
            Func<int, MenuButtonId> getButtonId;

            if (isRopeMode)
            {
                totalItems = RopeColorHelper.TotalRopeColors;
                selectedIndex = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_ROPE);
                baseQuadIndex = 59; // rope01-rope09 are quads 59-67
                getButtonId = MenuButtonId.ForRopeSlot;
            }
            else
            {
                const int TOTAL_CANDIES = 51;
                totalItems = TOTAL_CANDIES;
                selectedIndex = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY);
                baseQuadIndex = 6; // candy01-candy51 are quads 6-56
                getButtonId = MenuButtonId.ForCandySlot;
            }

            // Build grid rows
            for (int row = 0; row < ((totalItems + ITEMS_PER_ROW - 1) / ITEMS_PER_ROW); row++)
            {
                HBox rowBox = new HBox().InitWithOffsetAlignHeight(columnSpacing, 16, rowHeight);

                for (int col = 0; col < ITEMS_PER_ROW; col++)
                {
                    int itemIndex = (row * ITEMS_PER_ROW) + col;
                    if (itemIndex >= totalItems)
                    {
                        break;
                    }

                    int itemQuadIndex = baseQuadIndex + itemIndex;
                    Button slotButton = CreateSlotButton(itemIndex, selectedIndex, itemQuadIndex, slotScale, getButtonId(itemIndex));
                    _ = rowBox.AddChild(slotButton);
                }

                _ = itemGrid.AddChild(rowBox);
            }

            if (gridContainer != null)
            {
                gridContainer.width = itemGrid.width;
                gridContainer.height = itemGrid.height;
            }
            _ = currentContainer.AddChild(itemGrid);
        }

        public static MenuView CreateCandySelection(
            IButtonDelegation buttonDelegate,
            out ScrollableContainer candyContainer)
        {
            MenuView menuView = new();

            // Store delegate for later use
            currentButtonDelegate = buttonDelegate;
            isRopeMode = false;

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

            // Candy tab button
            Image candyBtnUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 4);
            Image candyBtnDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 5);

            // Add "Candy" text to the button images
            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            Text buttonText = new Text().InitWithFont(font);
            buttonText.SetString(Application.GetString("CANDIES_BTN"));
            buttonText.anchor = buttonText.parentAnchor = 18;
            _ = candyBtnUp.AddChild(buttonText);

            Text buttonText2 = new Text().InitWithFont(font);
            buttonText2.SetString(Application.GetString("CANDIES_BTN"));
            buttonText2.anchor = buttonText2.parentAnchor = 18;
            _ = candyBtnDown.AddChild(buttonText2);

            candyTabButton = new Button().InitWithUpElementDownElementandID(candyBtnUp, candyBtnDown, MenuButtonId.CandySelect);
            candyTabButton.delegateButtonDelegate = buttonDelegate;
            candyTabButton.anchor = candyTabButton.parentAnchor = 10;
            candyTabButton.x = -200f;
            candyTabButton.y = 50f;

            _ = background.AddChild(candyTabButton);

            // Rope tab button
            Image ropeBtnUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 4);
            Image ropeBtnDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 5);

            // Add "Rope" text to the button images
            Text ropeButtonText = new Text().InitWithFont(font);
            ropeButtonText.SetString(Application.GetString("ROPE_SKINS_BTN"));
            ropeButtonText.anchor = ropeButtonText.parentAnchor = 18;
            _ = ropeBtnUp.AddChild(ropeButtonText);

            Text ropeButtonText2 = new Text().InitWithFont(font);
            ropeButtonText2.SetString(Application.GetString("ROPE_SKINS_BTN"));
            ropeButtonText2.anchor = ropeButtonText2.parentAnchor = 18;
            _ = ropeBtnDown.AddChild(ropeButtonText2);

            ropeTabButton = new Button().InitWithUpElementDownElementandID(ropeBtnUp, ropeBtnDown, MenuButtonId.RopeSelect);
            ropeTabButton.delegateButtonDelegate = buttonDelegate;
            ropeTabButton.anchor = ropeTabButton.parentAnchor = 10;
            ropeTabButton.x = 200f;
            ropeTabButton.y = 50f;

            _ = background.AddChild(ropeTabButton);

            // Create scrollable container (initially empty, will be populated by RebuildGrid)
            float containerWidth = FrameworkTypes.SCREEN_WIDTH - 20f;
            float containerHeight = 1100f;

            // Create empty container initially
            gridContainer = new BaseElement
            {
                width = (int)containerWidth,
                height = 10
            };

            candyContainer = new ScrollableContainer().InitWithWidthHeightContainer(containerWidth, containerHeight, gridContainer);
            candyContainer.anchor = candyContainer.parentAnchor = 18;
            candyContainer.y = 50f;

            _ = background.AddChild(candyContainer);

            // Store container reference and build initial grid
            currentContainer = candyContainer;
            UpdateTabButtonStates(); // Set initial tab button states (candy active)
            RebuildGrid();

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
