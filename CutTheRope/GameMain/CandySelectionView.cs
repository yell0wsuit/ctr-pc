using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal static class CandySelectionView
    {
        private enum SelectionMode { Candy, Rope, OmNom }

        // Store candy slot button data for quick updates
        private static readonly List<SlotButtonData> slotButtons = [];

        // Track current selection mode and UI references
        private static SelectionMode currentMode;
        private static ScrollableContainer currentContainer;
        private static BaseElement gridContainer;
        private static IButtonDelegation currentButtonDelegate;
        private static readonly Random previewRandom = new();
        private static Button candyTabButton;
        private static Button ropeTabButton;
        private static Button omNomTabButton;
        private static ITargetAnimationBackend activePreviewBackend;
        private static GameObject activePreviewObject;

        private sealed class SlotButtonData
        {
            public int CandyIndex { get; set; }
            public Image UpImage { get; set; }
            public Image DownImage { get; set; }
            public BaseElement UpPreview { get; set; }
            public BaseElement DownPreview { get; set; }
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
        /// Switches between candy, rope, and Om Nom selection modes.
        /// </summary>
        private static void SwitchToMode(SelectionMode mode)
        {
            if (currentMode == mode || currentContainer == null)
            {
                return;
            }

            CleanupPreview();
            currentMode = mode;
            UpdateTabButtonStates();
            RebuildGrid();
        }

        public static void SwitchToCandyMode()
        {
            SwitchToMode(SelectionMode.Candy);
        }

        public static void SwitchToRopeMode()
        {
            SwitchToMode(SelectionMode.Rope);
        }

        public static void SwitchToOmNomMode()
        {
            SwitchToMode(SelectionMode.OmNom);
        }

        /// <summary>
        /// Updates the tab button visual states to show which mode is active.
        /// </summary>
        private static void UpdateTabButtonStates()
        {
            if (candyTabButton == null || ropeTabButton == null || omNomTabButton == null)
            {
                return;
            }

            SetTabActive(candyTabButton, currentMode == SelectionMode.Candy);
            SetTabActive(ropeTabButton, currentMode == SelectionMode.Rope);
            SetTabActive(omNomTabButton, currentMode == SelectionMode.OmNom);
        }

        private static void SetTabActive(Button tab, bool active)
        {
            Image upImage = (Image)tab.GetChild(0);
            Image downImage = (Image)tab.GetChild(1);
            int quad = active ? 5 : 4;
            upImage.SetDrawQuad(quad);
            downImage.SetDrawQuad(quad);
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
        /// Creates a slot button for an Om Nom skin with artwork in both button states.
        /// </summary>
        private static Button CreateOmNomSlotButton(int skinIndex, int selectedIndex, float slotScale, MenuButtonId buttonId)
        {
            bool isEquipped = skinIndex == selectedIndex;
            int bgUpQuad = isEquipped ? 2 : 0;
            int bgDownQuad = isEquipped ? 3 : 1;

            Image slotBgUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgUpQuad);
            Image slotBgDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgDownQuad);
            slotBgUp.scaleX = slotBgUp.scaleY = slotScale;
            slotBgDown.scaleX = slotBgDown.scaleY = slotScale;

            Button slotButton = new Button().InitWithUpElementDownElementandID(slotBgUp, slotBgDown, buttonId);
            slotButton.delegateButtonDelegate = currentButtonDelegate;

            SlotButtonData slotButtonData = new()
            {
                CandyIndex = skinIndex,
                UpImage = slotBgUp,
                DownImage = slotBgDown,
                UpPreview = CreateAndAttachOmNomPreview(
                    slotBgUp,
                    skinIndex,
                    OmNomSlotPreviewPolicy.Resolve(skinIndex, selectedIndex),
                    animated: skinIndex == selectedIndex),
                DownPreview = CreateAndAttachOmNomPreview(
                    slotBgDown,
                    skinIndex,
                    GetPressedPreviewMode(skinIndex),
                    animated: false)
            };

            slotButtons.Add(slotButtonData);
            return slotButton;
        }

        /// <summary>
        /// Rebuilds the grid based on the current mode (candy, rope, or Om Nom).
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

            switch (currentMode)
            {
                case SelectionMode.Rope:
                    totalItems = RopeColorHelper.TotalRopeColors;
                    selectedIndex = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_ROPE);
                    baseQuadIndex = 60; // rope01-rope09 are quads 60-68
                    getButtonId = MenuButtonId.ForRopeSlot;
                    break;
                case SelectionMode.OmNom:
                    totalItems = OmNomSkinRegistry.TotalSkinCount;
                    selectedIndex = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_OMNOM);
                    baseQuadIndex = -1; // not used — Om Nom slots created differently
                    getButtonId = MenuButtonId.ForOmNomSlot;
                    break;
                case SelectionMode.Candy:
                default: // Candy
                    const int TOTAL_CANDIES = 52;
                    totalItems = TOTAL_CANDIES;
                    selectedIndex = Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY);
                    baseQuadIndex = 6; // candy01-candy52 are quads 6-57
                    getButtonId = MenuButtonId.ForCandySlot;
                    break;
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

                    Button slotButton;
                    if (currentMode == SelectionMode.OmNom)
                    {
                        slotButton = CreateOmNomSlotButton(itemIndex, selectedIndex, slotScale, getButtonId(itemIndex));
                    }
                    else
                    {
                        int itemQuadIndex = baseQuadIndex + itemIndex;
                        slotButton = CreateSlotButton(itemIndex, selectedIndex, itemQuadIndex, slotScale, getButtonId(itemIndex));
                    }
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

        /// <summary>
        /// Creates and attaches an Om Nom preview matching the requested mode.
        /// </summary>
        private static BaseElement CreateAndAttachOmNomPreview(
            Image parentImage,
            int skinIndex,
            OmNomSlotPreviewMode previewMode,
            bool animated)
        {
            BaseElement preview = CreateOmNomPreview(skinIndex, previewMode, animated);
            _ = parentImage.AddChild(preview);
            return preview;
        }

        private static GameObject CreateOmNomPreview(int skinIndex, OmNomSlotPreviewMode previewMode, bool animated)
        {
            return previewMode switch
            {
                OmNomSlotPreviewMode.ClassicAnimated => CreateClassicOmNomPreview(animated: true),
                OmNomSlotPreviewMode.ClassicStatic => CreateClassicOmNomPreview(animated: false),
                OmNomSlotPreviewMode.Xml => CreateXmlOmNomPreview(skinIndex, animated),
                _ => throw new ArgumentOutOfRangeException(nameof(previewMode), previewMode, null),
            };
        }

        private static GameObject CreateClassicOmNomPreview(bool animated)
        {
            OmNomSlotPreviewLayoutInfo layout = OmNomSlotPreviewLayout.Resolve(
                animated ? OmNomSlotPreviewMode.ClassicAnimated : OmNomSlotPreviewMode.ClassicStatic);
            OriginalTargetAnimationBackend backend = new(isNightLevel: false, isXmas: false);
            GameObject previewObject = backend.TargetObject;
            ConfigureOmNomPreviewLayout(previewObject, layout);

            backend.Initialize(null);
            previewObject.updateable = false;

            if (animated)
            {
                activePreviewBackend = backend;
                activePreviewObject = previewObject;
            }

            return previewObject;
        }

        private static GameObject CreateXmlOmNomPreview(int skinIndex, bool animated)
        {
            OmNomSlotPreviewLayoutInfo layout = OmNomSlotPreviewLayout.Resolve(OmNomSlotPreviewMode.Xml);
            OmNomSkinDefinition skin = OmNomSkinRegistry.GetXmlSkinDefinition(skinIndex);
            FlashXmlTargetAnimationBackend backend = new(skin);
            GameObject previewObject = backend.TargetObject;
            ConfigureOmNomPreviewLayout(previewObject, layout);

            backend.Initialize(null);
            previewObject.updateable = false;

            if (animated)
            {
                activePreviewBackend = backend;
                activePreviewObject = previewObject;
                activePreviewBackend.PlayRandomIdleVariant((min, max) => previewRandom.Next(min, max + 1));
            }

            return previewObject;
        }

        private static void ConfigureOmNomPreviewLayout(GameObject previewObject, OmNomSlotPreviewLayoutInfo layout)
        {
            previewObject.scaleX = layout.Scale;
            previewObject.scaleY = layout.Scale;
            previewObject.anchor = 18;
            previewObject.parentAnchor = 18;
            previewObject.x = 0f;
            previewObject.y = layout.YOffset;
        }

        private static OmNomSlotPreviewMode GetPressedPreviewMode(int skinIndex)
        {
            return skinIndex == 0
                ? OmNomSlotPreviewMode.ClassicStatic
                : OmNomSlotPreviewMode.Xml;
        }

        private static SlotButtonData FindSlotButtonData(int slotIndex)
        {
            for (int i = 0; i < slotButtons.Count; i++)
            {
                if (slotButtons[i].CandyIndex == slotIndex)
                {
                    return slotButtons[i];
                }
            }

            return null;
        }

        private static void ReplaceUpPreview(SlotButtonData slotData, OmNomSlotPreviewMode previewMode, bool animated)
        {
            if (slotData == null)
            {
                return;
            }

            if (slotData.UpPreview != null)
            {
                slotData.UpImage.RemoveChild(slotData.UpPreview);
            }

            slotData.UpPreview = CreateAndAttachOmNomPreview(slotData.UpImage, slotData.CandyIndex, previewMode, animated);
        }

        /// <summary>
        /// Cleans up the current preview animation and removes it from the display tree.
        /// </summary>
        private static void CleanupPreview()
        {
            activePreviewObject?.parent?.RemoveChild(activePreviewObject);
            activePreviewObject = null;
            activePreviewBackend = null;
        }

        /// <summary>
        /// Selects an Om Nom skin slot and swaps the live preview to it.
        /// </summary>
        public static void SelectOmNomSlot(int newSelectedIndex)
        {
            int previousSelectedIndex = -1;
            for (int i = 0; i < slotButtons.Count; i++)
            {
                if (ReferenceEquals(slotButtons[i].UpPreview, activePreviewObject))
                {
                    previousSelectedIndex = slotButtons[i].CandyIndex;
                    break;
                }
            }

            CleanupPreview();
            UpdateCandySlotButtons(newSelectedIndex);

            if (previousSelectedIndex >= 0 && previousSelectedIndex != newSelectedIndex)
            {
                ReplaceUpPreview(
                    FindSlotButtonData(previousSelectedIndex),
                    previousSelectedIndex == 0
                        ? OmNomSlotPreviewMode.ClassicStatic
                        : OmNomSlotPreviewMode.Xml,
                    animated: false);
            }

            ReplaceUpPreview(
                FindSlotButtonData(newSelectedIndex),
                OmNomSlotPreviewPolicy.Resolve(newSelectedIndex, newSelectedIndex),
                animated: true);
        }

        /// <summary>
        /// Ticks the preview animation each frame.
        /// </summary>
        public static void Update(float delta)
        {
            if (currentMode == SelectionMode.OmNom && activePreviewObject != null)
            {
                activePreviewObject.Update(delta);
            }
        }

        public static MenuView CreateCandySelection(
            IButtonDelegation buttonDelegate,
            out ScrollableContainer candyContainer)
        {
            MenuView menuView = new();
            const float tabGap = 24f;

            // Store delegate for later use
            currentButtonDelegate = buttonDelegate;
            currentMode = SelectionMode.Candy;

            BaseElement background = new()
            {
                width = (int)FrameworkTypes.SCREEN_WIDTH,
                height = (int)FrameworkTypes.SCREEN_HEIGHT
            }; // ensure child anchors use the full screen bounds instead of 0x0

            Image bgImage = Image.Image_createWithResID(Resources.BackgroundImg.SkinBackground);
            bgImage.anchor = bgImage.parentAnchor = 18; // center

            // Scale background to cover the whole screen (match other menu backgrounds)
            float bgScale = MathF.Max(FrameworkTypes.SCREEN_WIDTH / bgImage.width, FrameworkTypes.SCREEN_HEIGHT / bgImage.height);
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
            ropeTabButton.y = 50f;

            _ = background.AddChild(ropeTabButton);

            // Om Nom tab button
            Image omNomBtnUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 4);
            Image omNomBtnDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 5);

            Text omNomButtonText = new Text().InitWithFont(font);
            omNomButtonText.SetString(Application.GetString("OM_NOM_BTN"));
            omNomButtonText.anchor = omNomButtonText.parentAnchor = 18;
            _ = omNomBtnUp.AddChild(omNomButtonText);

            Text omNomButtonText2 = new Text().InitWithFont(font);
            omNomButtonText2.SetString(Application.GetString("OM_NOM_BTN"));
            omNomButtonText2.anchor = omNomButtonText2.parentAnchor = 18;
            _ = omNomBtnDown.AddChild(omNomButtonText2);

            omNomTabButton = new Button().InitWithUpElementDownElementandID(omNomBtnUp, omNomBtnDown, MenuButtonId.OmNomSelect);
            omNomTabButton.delegateButtonDelegate = buttonDelegate;
            omNomTabButton.anchor = omNomTabButton.parentAnchor = 10;
            omNomTabButton.y = 50f;

            _ = background.AddChild(omNomTabButton);

            float tabStride = MathF.Max(
                MathF.Max(candyBtnDown.width, ropeBtnDown.width),
                omNomBtnDown.width) + tabGap;
            candyTabButton.x = SkinSelectionTabLayout.GetCenteredX(0, 3, tabStride);
            ropeTabButton.x = SkinSelectionTabLayout.GetCenteredX(1, 3, tabStride);
            omNomTabButton.x = SkinSelectionTabLayout.GetCenteredX(2, 3, tabStride);

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
