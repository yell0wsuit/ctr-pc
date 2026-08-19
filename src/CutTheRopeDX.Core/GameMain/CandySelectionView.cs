using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Builds and updates the candy, rope, Om Nom, and finger trace skin selection menu.
    /// </summary>
    internal static class CandySelectionView
    {
        /// <summary>Skin ID used by the original Flash-backed Om Nom skin.</summary>
        private const string OriginalFlashSkinName = "OM_NOM_ORIGINAL_FLASH";

        /// <summary>Number of Flash XML frames skipped for static XML Om Nom previews.</summary>
        private const int XmlPreviewSkipFrames = 14;

        /// <summary>Number of Om Nom preview slots built per warmup tick.</summary>
        private const int OmNomWarmupSlotsPerTick = 1;

        /// <summary>Slot button data for the currently active selection mode.</summary>
        private static List<SlotButtonData> slotButtons = [];

        /// <summary>Currently active selection mode.</summary>
        private static CandySelectionMode currentMode;

        /// <summary>Cached grids and preview state for each selection mode.</summary>
        private static CandySelectionModeCache modeCache = new();

        /// <summary>Scrollable container that hosts the active selection grid.</summary>
        private static ScrollableContainer currentContainer;

        /// <summary>Outer grid container passed to the scrollable container.</summary>
        private static BaseElement gridContainer;

        /// <summary>Delegate used by slot and tab buttons.</summary>
        private static IButtonDelegation currentButtonDelegate;

        /// <summary>Random source used for animated preview idle variants.</summary>
        private static readonly Random previewRandom = new();

        /// <summary>Candy tab button.</summary>
        private static Button candyTabButton;

        /// <summary>Rope tab button.</summary>
        private static Button ropeTabButton;

        /// <summary>Om Nom tab button.</summary>
        private static Button omNomTabButton;

        /// <summary>Finger trace tab button.</summary>
        private static Button traceTabButton;

        /// <summary>Backend currently driving the active preview object.</summary>
        private static ITargetAnimationBackend activePreviewBackend;

        /// <summary>Preview object currently animated in the active grid.</summary>
        private static GameObject activePreviewObject;

        /// <summary>Incremental warmup state for building the Om Nom grid.</summary>
        private static OmNomWarmupState omNomWarmupState;

        /// <summary>Background task that preparses Om Nom Flash XML definitions.</summary>
        private static Task omNomXmlPreparseTask;

        /// <summary>
        /// Cached references for a built skin slot button.
        /// </summary>
        private sealed class SlotButtonData
        {
            /// <summary>Skin index represented by the slot.</summary>
            public int CandyIndex { get; set; }

            /// <summary>The slot's button, so a layout pass can move and resize it.</summary>
            public Button Slot { get; set; }

            /// <summary>Up-state slot background image.</summary>
            public Image UpImage { get; set; }

            /// <summary>Down-state slot background image.</summary>
            public Image DownImage { get; set; }

            /// <summary>Preview object attached to the up-state image.</summary>
            public BaseElement UpPreview { get; set; }

            /// <summary>Preview object attached to the down-state image.</summary>
            public BaseElement DownPreview { get; set; }
        }

        /// <summary>
        /// Preview state produced while building Om Nom preview slots.
        /// </summary>
        private sealed class OmNomPreviewBuildState
        {
            /// <summary>Backend currently driving the animated preview.</summary>
            public ITargetAnimationBackend ActivePreviewBackend { get; set; }

            /// <summary>Animated preview object selected during slot building.</summary>
            public GameObject ActivePreviewObject { get; set; }
        }

        /// <summary>
        /// Incremental builder state for the Om Nom selection grid.
        /// </summary>
        private sealed class OmNomWarmupState
        {
            /// <summary>Grid being built incrementally.</summary>
            public VBox Grid { get; set; }

            /// <summary>Slot buttons built so far.</summary>
            public List<SlotButtonData> SlotButtons { get; } = [];

            /// <summary>Preview state produced while building slots.</summary>
            public OmNomPreviewBuildState PreviewState { get; } = new();

            /// <summary>Currently selected Om Nom skin index.</summary>
            public int SelectedIndex { get; init; }

            /// <summary>
            /// How the screen is divided at the viewport the warmup is building for. Settable, so
            /// a viewport that changes shape mid-warmup carries the rest of the slots with it.
            /// </summary>
            public SkinSelectionLayout Layout { get; set; }

            /// <summary>Total number of Om Nom skin slots to build.</summary>
            public int TotalItems { get; init; }

            /// <summary>Next skin index to build.</summary>
            public int NextItemIndex { get; set; }

            /// <summary>Number of items already added to the current row.</summary>
            public int ItemsInCurrentRow { get; set; }

            /// <summary>Current row receiving incremental slot buttons.</summary>
            public HBox CurrentRow { get; set; }
        }

        /// <summary>
        /// Updates all candy slot buttons to reflect the newly selected candy skin.
        /// This updates the button backgrounds without recreating the entire view.
        /// </summary>
        /// <param name="newSelectedCandyIndex">Newly selected slot index.</param>
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
        /// <param name="mode">Selection mode to switch to.</param>
        private static void SwitchToMode(CandySelectionMode mode)
        {
            if (currentMode == mode || currentContainer == null)
            {
                return;
            }

            StoreCurrentModeState();
            DetachModeGrid(currentMode);
            currentMode = mode;
            UpdateTabButtonStates();
            AttachCurrentModeGrid();
            currentContainer.SetScroll(new Vector(0f, 0f));
        }

        /// <summary>
        /// Switches the selection menu to candy skin mode.
        /// </summary>
        public static void SwitchToCandyMode()
        {
            SwitchToMode(CandySelectionMode.Candy);
        }

        /// <summary>
        /// Switches the selection menu to rope skin mode.
        /// </summary>
        public static void SwitchToRopeMode()
        {
            SwitchToMode(CandySelectionMode.Rope);
        }

        /// <summary>
        /// Switches the selection menu to Om Nom skin mode.
        /// </summary>
        public static void SwitchToOmNomMode()
        {
            SwitchToMode(CandySelectionMode.OmNom);
        }

        /// <summary>
        /// Switches the selection menu to finger trace skin mode.
        /// </summary>
        public static void SwitchToTraceMode()
        {
            SwitchToMode(CandySelectionMode.Trace);
        }

        /// <summary>
        /// Updates the tab button visual states to show which mode is active.
        /// </summary>
        private static void UpdateTabButtonStates()
        {
            if (candyTabButton == null || ropeTabButton == null || omNomTabButton == null || traceTabButton == null)
            {
                return;
            }

            SetTabActive(candyTabButton, currentMode == CandySelectionMode.Candy);
            SetTabActive(ropeTabButton, currentMode == CandySelectionMode.Rope);
            SetTabActive(omNomTabButton, currentMode == CandySelectionMode.OmNom);
            SetTabActive(traceTabButton, currentMode == CandySelectionMode.Trace);
        }

        /// <summary>
        /// Updates one tab button to its active or inactive visual state.
        /// </summary>
        /// <param name="tab">Tab button to update.</param>
        /// <param name="active">Whether the tab should appear active.</param>
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
        /// <param name="itemIndex">Skin item index represented by the slot.</param>
        /// <param name="selectedIndex">Currently selected skin index for the mode.</param>
        /// <param name="itemResourceName">Texture resource name for the slot item.</param>
        /// <param name="itemQuadIndex">Quad index for the slot item.</param>
        /// <param name="layout">The layout the screen is drawn at.</param>
        /// <param name="buttonId">Button identifier assigned to the slot.</param>
        /// <param name="itemYOffset">Vertical offset applied to the item image.</param>
        /// <param name="doRestoreTransparency">Whether to restore cut transparency on the item image.</param>
        /// <returns>The configured slot button.</returns>
        private static Button CreateSlotButton(
            int itemIndex,
            int selectedIndex,
            string itemResourceName,
            int itemQuadIndex,
            SkinSelectionLayout layout,
            MenuButtonId buttonId,
            float itemYOffset = -20f,
            bool doRestoreTransparency = false)
        {
            bool isEquipped = itemIndex == selectedIndex;
            int bgUpQuad = isEquipped ? 2 : 0;
            int bgDownQuad = isEquipped ? 3 : 1;

            Image slotBgUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgUpQuad);
            Image slotBgDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgDownQuad);

            // Add item image to both up and down states
            Image itemImage = Image.Image_createWithResIDQuad(itemResourceName, itemQuadIndex);
            itemImage.anchor = itemImage.parentAnchor = 18;
            itemImage.y = itemYOffset;
            if (doRestoreTransparency)
            {
                itemImage.DoRestoreCutTransparency();
            }
            _ = slotBgUp.AddChild(itemImage);

            Image itemImage2 = Image.Image_createWithResIDQuad(itemResourceName, itemQuadIndex);
            itemImage2.anchor = itemImage2.parentAnchor = 18;
            itemImage2.y = itemYOffset;
            if (doRestoreTransparency)
            {
                itemImage2.DoRestoreCutTransparency();
            }
            _ = slotBgDown.AddChild(itemImage2);

            Button slotButton = new Button().InitWithUpElementDownElementandID(slotBgUp, slotBgDown, buttonId);
            slotButton.delegateButtonDelegate = currentButtonDelegate;

            // Store button data for later updates
            SlotButtonData slotData = new()
            {
                CandyIndex = itemIndex,
                Slot = slotButton,
                UpImage = slotBgUp,
                DownImage = slotBgDown
            };
            SizeSlot(slotData, layout);
            slotButtons.Add(slotData);

            return slotButton;
        }

        /// <summary>
        /// Stores the active mode grid, slot button list, and preview state in the mode cache.
        /// </summary>
        private static void StoreCurrentModeState()
        {
            CandySelectionModeState state = modeCache.GetState(currentMode);
            state.SlotButtons = slotButtons;
            state.ActivePreviewBackend = activePreviewBackend;
            state.ActivePreviewObject = activePreviewObject;

            if (state.Grid == null && currentContainer?.ChildsCount() > 0)
            {
                state.Grid = currentContainer.GetChild(0);
            }
        }

        /// <summary>
        /// Detaches the cached grid for a mode from the current scroll container.
        /// </summary>
        /// <param name="mode">Selection mode whose grid should be detached.</param>
        private static void DetachModeGrid(CandySelectionMode mode)
        {
            if (currentContainer == null)
            {
                return;
            }

            BaseElement grid = modeCache.GetState(mode).Grid;
            if (grid != null)
            {
                currentContainer.RemoveChild(grid);
                return;
            }

            if (currentContainer.ChildsCount() > 0)
            {
                currentContainer.RemoveChildWithID(0);
            }
        }

        /// <summary>
        /// Attaches the cached or newly built grid for the current selection mode.
        /// </summary>
        private static void AttachCurrentModeGrid()
        {
            if (currentMode == CandySelectionMode.OmNom)
            {
                EnsureOmNomGridReady();
            }

            CandySelectionModeActivation activation = modeCache.ActivateMode(currentMode);
            if (activation.RequiresBuild)
            {
                BuildAndAttachGrid(currentMode);
                return;
            }

            AttachCachedGrid(activation.State);
        }

        /// <summary>
        /// Attaches a cached mode grid and restores its slot and preview state.
        /// </summary>
        /// <param name="state">Cached mode state to attach.</param>
        private static void AttachCachedGrid(CandySelectionModeState state)
        {
            slotButtons = state.SlotButtons as List<SlotButtonData> ?? [];
            activePreviewBackend = state.ActivePreviewBackend;
            activePreviewObject = state.ActivePreviewObject;

            if (state.Grid == null || currentContainer == null)
            {
                return;
            }

            if (gridContainer != null)
            {
                gridContainer.width = state.Grid.width;
                gridContainer.height = state.Grid.height;
            }

            _ = currentContainer.AddChild(state.Grid);
        }

        /// <summary>
        /// Builds a grid for a mode and attaches it to the current scroll container.
        /// </summary>
        /// <param name="mode">Selection mode whose grid should be built.</param>
        private static void BuildAndAttachGrid(CandySelectionMode mode)
        {
            if (mode == CandySelectionMode.OmNom)
            {
                CompleteOmNomWarmupSynchronously();

                CandySelectionModeState omNomState = modeCache.GetState(CandySelectionMode.OmNom);
                if (omNomState.Grid == null)
                {
                    OmNomWarmupState warmupState = CreateOmNomWarmupState();
                    BuildOmNomWarmupSlots(warmupState, warmupState.TotalItems);
                    StoreCompletedOmNomWarmupState(warmupState);
                    omNomState = modeCache.GetState(CandySelectionMode.OmNom);
                }

                AttachCachedGrid(omNomState);
                return;
            }

            slotButtons = [];
            activePreviewBackend = null;
            activePreviewObject = null;
            BaseElement grid = CreateGrid(mode);
            modeCache.StoreState(mode, grid, slotButtons, activePreviewObject, activePreviewBackend);
            AttachCachedGrid(modeCache.GetState(mode));
        }

        /// <summary>
        /// Builds the grid for the requested mode.
        /// </summary>
        /// <param name="mode">Selection mode whose grid should be created.</param>
        /// <returns>The created grid.</returns>
        private static VBox CreateGrid(CandySelectionMode mode)
        {
            SkinSelectionLayout layout = CurrentLayout();
            VBox itemGrid = new VBox().InitWithOffsetAlignWidth(layout.RowSpacing, 2, layout.GridWidth);

            // Get mode-specific configuration
            int totalItems;
            int selectedIndex;
            int baseQuadIndex;
            string itemResourceName = Resources.Img.SkinSelection;
            float itemYOffset = -20f;
            bool doRestoreTransparency = false;
            Func<int, MenuButtonId> getButtonId;

            switch (mode)
            {
                case CandySelectionMode.Rope:
                    totalItems = RopeColorHelper.TotalRopeColors;
                    selectedIndex = Preferences.GetIntForKey("PREFS_SELECTED_ROPE");
                    baseQuadIndex = 60; // rope01-rope09 are quads 60-68
                    getButtonId = MenuButtonId.ForRopeSlot;
                    break;
                case CandySelectionMode.OmNom:
                    throw new InvalidOperationException("Om Nom grids are built through the warmup pipeline.");
                case CandySelectionMode.Trace:
                    totalItems = FingerTraceFactory.TotalTraceSkins;
                    selectedIndex = Preferences.GetIntForKey("PREFS_SELECTED_TRACE");
                    baseQuadIndex = 0;
                    itemResourceName = Resources.Img.FingerTraceSkinOptions;
                    itemYOffset = -20f;
                    doRestoreTransparency = true;
                    getButtonId = MenuButtonId.ForTraceSlot;
                    break;
                case CandySelectionMode.Candy:
                default: // Candy
                    const int TOTAL_CANDIES = 52;
                    totalItems = TOTAL_CANDIES;
                    selectedIndex = Preferences.GetIntForKey("PREFS_SELECTED_CANDY");
                    baseQuadIndex = 6; // candy01-candy52 are quads 6-57
                    getButtonId = MenuButtonId.ForCandySlot;
                    break;
            }

            // Build grid rows
            for (int row = 0; row < ((totalItems + layout.Columns - 1) / layout.Columns); row++)
            {
                HBox rowBox = NewRow(layout);

                for (int col = 0; col < layout.Columns; col++)
                {
                    int itemIndex = (row * layout.Columns) + col;
                    if (itemIndex >= totalItems)
                    {
                        break;
                    }

                    int itemQuadIndex = baseQuadIndex + itemIndex;
                    Button slotButton = CreateSlotButton(
                        itemIndex,
                        selectedIndex,
                        itemResourceName,
                        itemQuadIndex,
                        layout,
                        getButtonId(itemIndex),
                        itemYOffset,
                        doRestoreTransparency);
                    _ = rowBox.AddChild(slotButton);
                }

                _ = itemGrid.AddChild(rowBox);
            }

            if (gridContainer != null)
            {
                gridContainer.width = itemGrid.width;
                gridContainer.height = itemGrid.height;
            }
            return itemGrid;
        }

        /// <summary>
        /// How the screen divides the viewport it is being drawn on.
        /// </summary>
        /// <returns>The current layout.</returns>
        private static SkinSelectionLayout CurrentLayout()
        {
            return LayoutFor(ScreenPresentation.Instance.Snapshot);
        }

        /// <summary>
        /// How the screen divides a viewport, measuring the tabs it has already built.
        /// </summary>
        /// <param name="snapshot">The viewport to lay out against.</param>
        /// <returns>The layout for that viewport.</returns>
        private static SkinSelectionLayout LayoutFor(ViewportLayoutSnapshot snapshot)
        {
            float tabWidth = 0f;
            float tabHeight = 0f;
            foreach (Button tab in Tabs())
            {
                tabWidth = MathF.Max(tabWidth, tab?.width ?? 0f);
                tabHeight = MathF.Max(tabHeight, tab?.height ?? 0f);
            }

            ChromeRoom room = backButton == null
                ? default
                : HudMetrics.RoomFor(
                    snapshot,
                    backButton.width,
                    backButton.height,
                    HudMetrics.IsTouchHost);

            return SkinSelectionLayout.For(
                snapshot.VisibleBounds,
                ContentFit.ScaleForAspect(snapshot.Aspect),
                tabWidth,
                tabHeight,
                TabCount,
                room.Width,
                room.Height);
        }

        /// <summary>How many tabs the screen has.</summary>
        private const int TabCount = 4;

        /// <summary>The screen's tab buttons, in the order they are shown.</summary>
        /// <returns>The tabs.</returns>
        private static Button[] Tabs()
        {
            return [candyTabButton, ropeTabButton, omNomTabButton, traceTabButton];
        }

        /// <summary>
        /// Places the tabs across the top of the screen, wrapping onto as many rows as it takes to
        /// keep them on it.
        /// </summary>
        /// <param name="layout">The layout the screen is drawn at.</param>
        private static void PlaceTabs(SkinSelectionLayout layout)
        {
            Button[] tabs = Tabs();
            for (int i = 0; i < tabs.Length; i++)
            {
                Button tab = tabs[i];
                if (tab == null)
                {
                    continue;
                }

                // Scaled about its own center: that leaves the horizontal offset alone, since the
                // offset is the center's, and lifts the top by half the growth, which comes back
                // out of the row's own top here.
                tab.scaleX = tab.scaleY = layout.Scale;
                tab.x = layout.TabX(i, tabs.Length);
                tab.y = layout.TabTopFor(i) - (tab.height * (1f - layout.Scale) / 2f);
            }
        }

        /// <summary>
        /// Sizes the window the grid scrolls inside and places it below the tabs, keeping the
        /// reader inside what the resized window can reach.
        /// </summary>
        /// <param name="layout">The layout the screen is drawn at.</param>
        private static void PlaceWindow(SkinSelectionLayout layout)
        {
            if (currentContainer == null)
            {
                return;
            }

            currentContainer.anchor = currentContainer.parentAnchor = 10;
            currentContainer.width = (int)layout.GridWidth;
            currentContainer.height = (int)layout.WindowHeight;
            currentContainer.y = layout.WindowTop;

            Vector scroll = currentContainer.GetScroll();
            scroll.Y = CTRMathHelper.FIT_TO_BOUNDARIES(scroll.Y, 0f, currentContainer.GetMaxScroll().Y);
            currentContainer.SetScroll(scroll);
        }

        /// <summary>Creates an empty row of slots at a layout's metrics.</summary>
        /// <param name="layout">The layout the screen is drawn at.</param>
        /// <returns>The row.</returns>
        private static HBox NewRow(SkinSelectionLayout layout)
        {
            return new HBox().InitWithOffsetAlignHeight(layout.ColumnSpacing, 16, layout.RowHeight);
        }

        /// <summary>
        /// Sizes one slot to a layout's cell. The button's own rectangle is the cell, so the row
        /// spaces it by what is drawn and a touch lands where the slot looks like it is, and the
        /// artwork is centered in that cell and scaled about its own middle.
        /// </summary>
        /// <param name="slot">Slot to size.</param>
        /// <param name="layout">The layout the screen is drawn at.</param>
        private static void SizeSlot(SlotButtonData slot, SkinSelectionLayout layout)
        {
            foreach (Image face in new[] { slot.UpImage, slot.DownImage })
            {
                if (face == null)
                {
                    continue;
                }

                face.anchor = face.parentAnchor = 18;
                face.scaleX = face.scaleY = layout.Scale;
            }

            if (slot.Slot != null)
            {
                slot.Slot.width = (int)MathF.Round(layout.CellWidth);
                slot.Slot.height = (int)MathF.Round(layout.CellHeight);
            }
        }

        /// <summary>
        /// Ensures the Om Nom grid has been fully built before it is displayed.
        /// </summary>
        private static void EnsureOmNomGridReady()
        {
            if (modeCache.GetState(CandySelectionMode.OmNom).Grid != null)
            {
                return;
            }

            CompleteOmNomWarmupSynchronously();
        }

        /// <summary>
        /// Starts incremental Om Nom grid warmup and background XML preparse work.
        /// </summary>
        private static void StartOmNomWarmup()
        {
            if (modeCache.GetState(CandySelectionMode.OmNom).Grid != null || omNomWarmupState != null)
            {
                return;
            }

            omNomWarmupState = CreateOmNomWarmupState();
            omNomXmlPreparseTask = Task.Run(() =>
            {
                try
                {
                    PreparseOmNomXmlDefinitions();
                }
                catch
                {
                    // Warmup failures should not block on-demand Om Nom creation.
                }
            });
        }

        /// <summary>
        /// Creates initial incremental warmup state for the Om Nom grid.
        /// </summary>
        /// <returns>The initialized warmup state.</returns>
        private static OmNomWarmupState CreateOmNomWarmupState()
        {
            SkinSelectionLayout layout = CurrentLayout();
            return new OmNomWarmupState
            {
                Grid = new VBox().InitWithOffsetAlignWidth(layout.RowSpacing, 2, layout.GridWidth),
                SelectedIndex = Preferences.GetIntForKey("PREFS_SELECTED_OMNOM"),
                Layout = layout,
                TotalItems = OmNomSkinRegistry.TotalSkinCount
            };
        }

        /// <summary>
        /// Preparses XML animation definitions used by Om Nom preview skins.
        /// </summary>
        private static void PreparseOmNomXmlDefinitions()
        {
            for (int i = 0; i < OmNomSkinRegistry.XmlSkins.Count; i++)
            {
                _ = FlashXmlImporter.ParseFile(OmNomSkinRegistry.XmlSkins[i].AnimationXmlPath);
            }

            _ = FlashXmlImporter.ParseFile(ContentPaths.GetAnimationXmlAbsolutePath("fx_sleep.xml"));
            _ = FlashXmlImporter.ParseFile(ContentPaths.GetAnimationXmlAbsolutePath("fx_bubbles.xml"));
        }

        /// <summary>
        /// Builds a small batch of Om Nom grid slots when background preparse work is complete.
        /// </summary>
        private static void WarmOmNomGridIncrementally()
        {
            if (omNomWarmupState == null || modeCache.GetState(CandySelectionMode.OmNom).Grid != null)
            {
                return;
            }

            if (omNomXmlPreparseTask is { IsCompleted: false })
            {
                return;
            }

            BuildOmNomWarmupSlots(omNomWarmupState, OmNomWarmupSlotsPerTick);
            if (omNomWarmupState.NextItemIndex >= omNomWarmupState.TotalItems)
            {
                StoreCompletedOmNomWarmupState(omNomWarmupState);
            }
        }

        /// <summary>
        /// Completes any remaining Om Nom grid warmup work synchronously.
        /// </summary>
        private static void CompleteOmNomWarmupSynchronously()
        {
            if (modeCache.GetState(CandySelectionMode.OmNom).Grid != null)
            {
                return;
            }

            omNomWarmupState ??= CreateOmNomWarmupState();

            int remainingSlots = omNomWarmupState.TotalItems - omNomWarmupState.NextItemIndex;
            if (remainingSlots > 0)
            {
                BuildOmNomWarmupSlots(omNomWarmupState, remainingSlots);
            }

            StoreCompletedOmNomWarmupState(omNomWarmupState);
        }

        /// <summary>
        /// Builds a batch of Om Nom skin slots into an in-progress warmup grid.
        /// </summary>
        /// <param name="warmupState">Warmup state that owns the grid under construction.</param>
        /// <param name="slotCount">Maximum number of slots to build.</param>
        private static void BuildOmNomWarmupSlots(OmNomWarmupState warmupState, int slotCount)
        {
            int targetItemIndex = Math.Min(warmupState.TotalItems, warmupState.NextItemIndex + slotCount);
            while (warmupState.NextItemIndex < targetItemIndex)
            {
                if (warmupState.CurrentRow == null)
                {
                    warmupState.CurrentRow = NewRow(warmupState.Layout);
                    warmupState.ItemsInCurrentRow = 0;
                    _ = warmupState.Grid.AddChild(warmupState.CurrentRow);
                }

                int itemIndex = warmupState.NextItemIndex;
                Button slotButton = CreateOmNomSlotButton(
                    itemIndex,
                    warmupState.SelectedIndex,
                    warmupState.Layout,
                    MenuButtonId.ForOmNomSlot(itemIndex),
                    warmupState.SlotButtons,
                    warmupState.PreviewState);

                _ = warmupState.CurrentRow.AddChild(slotButton);
                warmupState.NextItemIndex++;
                warmupState.ItemsInCurrentRow++;

                if (warmupState.ItemsInCurrentRow >= warmupState.Layout.Columns)
                {
                    warmupState.CurrentRow = null;
                }
            }
        }

        /// <summary>
        /// Stores a completed Om Nom warmup grid in the mode cache.
        /// </summary>
        /// <param name="warmupState">Completed warmup state to store.</param>
        private static void StoreCompletedOmNomWarmupState(OmNomWarmupState warmupState)
        {
            modeCache.StoreState(
                CandySelectionMode.OmNom,
                warmupState.Grid,
                warmupState.SlotButtons,
                warmupState.PreviewState.ActivePreviewObject,
                warmupState.PreviewState.ActivePreviewBackend);

            omNomWarmupState = null;
        }

        /// <summary>
        /// Creates and attaches an Om Nom preview matching the requested mode.
        /// </summary>
        /// <param name="parentImage">Slot image that receives the preview child.</param>
        /// <param name="skinIndex">Om Nom skin index to preview.</param>
        /// <param name="previewMode">Preview mode to create.</param>
        /// <param name="animated">Whether the preview should be animated.</param>
        /// <param name="previewState">Preview build state updated when an animated preview is created.</param>
        /// <returns>The attached preview object.</returns>
        private static BaseElement CreateAndAttachOmNomPreview(
            Image parentImage,
            int skinIndex,
            OmNomSlotPreviewMode previewMode,
            bool animated,
            OmNomPreviewBuildState previewState)
        {
            BaseElement preview = CreateOmNomPreview(skinIndex, previewMode, animated, previewState);
            _ = parentImage.AddChild(preview);
            return preview;
        }

        /// <summary>
        /// Creates an Om Nom preview object for a skin and preview mode.
        /// </summary>
        /// <param name="skinIndex">Om Nom skin index to preview.</param>
        /// <param name="previewMode">Preview mode to create.</param>
        /// <param name="animated">Whether the preview should be animated.</param>
        /// <param name="previewState">Preview build state updated when an animated preview is created.</param>
        /// <returns>The created preview object.</returns>
        private static GameObject CreateOmNomPreview(
            int skinIndex,
            OmNomSlotPreviewMode previewMode,
            bool animated,
            OmNomPreviewBuildState previewState)
        {
            return previewMode switch
            {
                OmNomSlotPreviewMode.ClassicAnimated => CreateClassicOmNomPreview(animated: true, previewState),
                OmNomSlotPreviewMode.ClassicStatic => CreateClassicOmNomPreview(animated: false, previewState),
                OmNomSlotPreviewMode.Xml => CreateXmlOmNomPreview(skinIndex, animated, previewState),
                _ => throw new ArgumentOutOfRangeException(nameof(previewMode), previewMode, null),
            };
        }

        /// <summary>
        /// Creates a classic Om Nom preview object.
        /// </summary>
        /// <param name="animated">Whether the preview should be animated.</param>
        /// <param name="previewState">Preview build state updated when an animated preview is created.</param>
        /// <returns>The created classic Om Nom preview object.</returns>
        private static GameObject CreateClassicOmNomPreview(bool animated, OmNomPreviewBuildState previewState)
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
                previewState.ActivePreviewBackend = backend;
                previewState.ActivePreviewObject = previewObject;
            }

            return previewObject;
        }

        /// <summary>
        /// Creates a Flash XML-backed Om Nom preview object.
        /// </summary>
        /// <param name="skinIndex">Om Nom skin index to preview.</param>
        /// <param name="animated">Whether the preview should be animated.</param>
        /// <param name="previewState">Preview build state updated when an animated preview is created.</param>
        /// <returns>The created XML-backed Om Nom preview object.</returns>
        private static GameObject CreateXmlOmNomPreview(int skinIndex, bool animated, OmNomPreviewBuildState previewState)
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
                previewState.ActivePreviewBackend = backend;
                previewState.ActivePreviewObject = previewObject;
                backend.PlayRandomIdleVariant((min, max) => previewRandom.Next(min, max + 1));
            }
            else
            {
                PlayStaticXmlPreviewState(backend, skin);
                backend.SkipCurrentTimelineFrames(XmlPreviewSkipFrames);
            }

            return previewObject;
        }

        /// <summary>
        /// Plays a representative static state for an XML-backed Om Nom preview.
        /// </summary>
        /// <param name="backend">Backend that owns the preview animation.</param>
        /// <param name="skin">Skin definition for the preview.</param>
        private static void PlayStaticXmlPreviewState(FlashXmlTargetAnimationBackend backend, OmNomSkinDefinition skin)
        {
            if (string.Equals(skin.Id, OriginalFlashSkinName, StringComparison.Ordinal)
                && skin.GetTimelineId(TargetAnimationState.IdleVariationThree) >= 0)
            {
                backend.Play(TargetAnimationState.IdleVariationThree);
                return;
            }

            if (skin.GetTimelineId(TargetAnimationState.Excited) >= 0)
            {
                backend.Play(TargetAnimationState.Excited);
                return;
            }

            backend.Play(TargetAnimationState.IdleLoop);
        }

        /// <summary>
        /// Applies slot preview layout values to an Om Nom preview object.
        /// </summary>
        /// <param name="previewObject">Preview object to configure.</param>
        /// <param name="layout">Layout values to apply.</param>
        private static void ConfigureOmNomPreviewLayout(GameObject previewObject, OmNomSlotPreviewLayoutInfo layout)
        {
            previewObject.scaleX = layout.Scale;
            previewObject.scaleY = layout.Scale;
            previewObject.useCustomAnchor = false;
            previewObject.anchor = 18;
            previewObject.parentAnchor = 18;
            previewObject.x = 0f;
            previewObject.y = layout.YOffset;
        }

        /// <summary>
        /// Gets the preview mode used by a pressed Om Nom slot.
        /// </summary>
        /// <param name="skinIndex">Om Nom skin index represented by the slot.</param>
        /// <returns>The pressed-state preview mode.</returns>
        private static OmNomSlotPreviewMode GetPressedPreviewMode(int skinIndex)
        {
            return skinIndex == 0
                ? OmNomSlotPreviewMode.ClassicStatic
                : OmNomSlotPreviewMode.Xml;
        }

        /// <summary>
        /// Creates a slot button for an Om Nom skin.
        /// </summary>
        /// <param name="skinIndex">Om Nom skin index represented by the slot.</param>
        /// <param name="selectedIndex">Currently selected Om Nom skin index.</param>
        /// <param name="layout">The layout the screen is drawn at.</param>
        /// <param name="buttonId">Button identifier assigned to the slot.</param>
        /// <param name="targetSlotButtons">Slot button list that receives cached slot data.</param>
        /// <param name="previewState">Preview build state updated when an animated preview is created.</param>
        /// <returns>The configured Om Nom slot button.</returns>
        private static Button CreateOmNomSlotButton(
            int skinIndex,
            int selectedIndex,
            SkinSelectionLayout layout,
            MenuButtonId buttonId,
            List<SlotButtonData> targetSlotButtons,
            OmNomPreviewBuildState previewState)
        {
            bool isEquipped = skinIndex == selectedIndex;
            int bgUpQuad = isEquipped ? 2 : 0;
            int bgDownQuad = isEquipped ? 3 : 1;

            Image slotBgUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgUpQuad);
            Image slotBgDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, bgDownQuad);

            Button slotButton = new Button().InitWithUpElementDownElementandID(slotBgUp, slotBgDown, buttonId);
            slotButton.delegateButtonDelegate = currentButtonDelegate;

            SlotButtonData slotButtonData = new()
            {
                CandyIndex = skinIndex,
                Slot = slotButton,
                UpImage = slotBgUp,
                DownImage = slotBgDown,
                UpPreview = CreateAndAttachOmNomPreview(
                    slotBgUp,
                    skinIndex,
                    OmNomSlotPreviewPolicy.Resolve(skinIndex, selectedIndex),
                    animated: skinIndex == selectedIndex,
                    previewState),
                DownPreview = CreateAndAttachOmNomPreview(
                    slotBgDown,
                    skinIndex,
                    GetPressedPreviewMode(skinIndex),
                    animated: false,
                    previewState)
            };

            SizeSlot(slotButtonData, layout);
            targetSlotButtons.Add(slotButtonData);
            return slotButton;
        }

        /// <summary>
        /// Finds cached slot button data for a skin slot index.
        /// </summary>
        /// <param name="slotIndex">Skin slot index to find.</param>
        /// <returns>The matching slot data, or <see langword="null"/> if no slot matches.</returns>
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

        /// <summary>
        /// Replaces the up-state Om Nom preview for a slot.
        /// </summary>
        /// <param name="slotData">Slot data whose up-state preview should be replaced.</param>
        /// <param name="previewMode">Preview mode to create.</param>
        /// <param name="animated">Whether the replacement preview should be animated.</param>
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

            OmNomPreviewBuildState previewState = new();
            slotData.UpPreview = CreateAndAttachOmNomPreview(
                slotData.UpImage,
                slotData.CandyIndex,
                previewMode,
                animated,
                previewState);

            if (animated)
            {
                activePreviewBackend = previewState.ActivePreviewBackend;
                activePreviewObject = previewState.ActivePreviewObject;
            }
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
        /// <param name="newSelectedIndex">Newly selected Om Nom skin index.</param>
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
        /// <param name="delta">Elapsed time in seconds since the last update.</param>
        public static void Update(float delta)
        {
            WarmOmNomGridIncrementally();

            if (currentMode == CandySelectionMode.OmNom && activePreviewObject != null)
            {
                activePreviewObject.Update(delta);
            }
        }

        /// <summary>
        /// Creates a tab button for a selection mode.
        /// </summary>
        /// <param name="textKey">Localization key for the tab text.</param>
        /// <param name="buttonId">Button identifier assigned to the tab.</param>
        /// <param name="font">Font used by the tab text.</param>
        /// <param name="buttonDelegate">Button delegate that receives tab press events.</param>
        /// <returns>The configured tab button.</returns>
        private static Button CreateTabButton(
            string textKey,
            MenuButtonId buttonId,
            FontGeneric font,
            IButtonDelegation buttonDelegate)
        {
            Image buttonUp = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 4);
            Image buttonDown = Image.Image_createWithResIDQuad(Resources.Img.SkinSelection, 5);

            Text upText = new Text().InitWithFont(font);
            upText.SetString(Application.GetString(textKey));
            upText.anchor = upText.parentAnchor = 18;
            _ = buttonUp.AddChild(upText);

            Text downText = new Text().InitWithFont(font);
            downText.SetString(Application.GetString(textKey));
            downText.anchor = downText.parentAnchor = 18;
            _ = buttonDown.AddChild(downText);

            Button button = new Button().InitWithUpElementDownElementandID(buttonUp, buttonDown, buttonId);
            button.delegateButtonDelegate = buttonDelegate;
            button.anchor = button.parentAnchor = 10;
            return button;
        }

        /// <summary>
        /// Covers the visible bounds with the selection background.
        /// </summary>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private static void CoverBackground(CTRRectangle visible)
        {
            backgroundImage.scaleX = backgroundImage.scaleY =
                LayoutMath.Cover(backgroundImage.width, backgroundImage.height, visible).Scale;
        }

        /// <summary>
        /// Lays the selection screen out for the viewport: the backdrop, the tabs across the top,
        /// the window below them, and the grids inside it.
        /// </summary>
        /// <remarks>
        /// The grids are flowed again rather than built again, because a slot owns an animated
        /// preview that is expensive to make and cheap to move: the same buttons are dealt into
        /// rows of whatever width the new viewport takes. Every mode's grid is flowed, not only
        /// the one on screen, so switching tabs after a resize does not show a stale one.
        /// </remarks>
        /// <param name="snapshot">The viewport to lay out against.</param>
        public static void Relayout(ViewportLayoutSnapshot snapshot)
        {
            if (backgroundRoot == null)
            {
                return;
            }

            CTRRectangle visible = snapshot.VisibleBounds;
            backgroundRoot.width = (int)visible.w;
            backgroundRoot.height = (int)visible.h;
            CoverBackground(visible);

            SkinSelectionLayout layout = LayoutFor(snapshot);
            PlaceTabs(layout);
            ReflowGrids(layout);
            PlaceWindow(layout);
        }

        /// <summary>
        /// Deals every built grid's slots into rows again at a new layout, including one a warmup
        /// is still filling.
        /// </summary>
        /// <param name="layout">The layout the screen is drawn at.</param>
        private static void ReflowGrids(SkinSelectionLayout layout)
        {
            foreach (CandySelectionMode mode in Enum.GetValues<CandySelectionMode>())
            {
                CandySelectionModeState state = modeCache.GetState(mode);
                if (state.Grid == null || state.SlotButtons is not List<SlotButtonData> slots)
                {
                    continue;
                }

                bool onScreen = mode == currentMode;
                if (onScreen)
                {
                    DetachModeGrid(mode);
                }

                state.Grid = FlowGrid(slots, layout, out _, out _);

                if (onScreen)
                {
                    AttachCachedGrid(state);
                }
            }

            if (omNomWarmupState != null)
            {
                omNomWarmupState.Layout = layout;
                omNomWarmupState.Grid = FlowGrid(
                    omNomWarmupState.SlotButtons,
                    layout,
                    out HBox lastRow,
                    out int lastRowCount);

                // The warmup carries on where it left off, so it is handed back the row it was
                // filling rather than starting a fresh one, which would leave a short row wherever
                // the viewport happened to change.
                bool rowHasRoom = lastRow != null && lastRowCount < layout.Columns;
                omNomWarmupState.CurrentRow = rowHasRoom ? lastRow : null;
                omNomWarmupState.ItemsInCurrentRow = rowHasRoom ? lastRowCount : 0;
            }
        }

        /// <summary>
        /// Deals slots into rows of a fresh grid, sizing each to the layout's cell.
        /// </summary>
        /// <param name="slots">Slots to deal, in the order they are shown.</param>
        /// <param name="layout">The layout the screen is drawn at.</param>
        /// <param name="lastRow">Receives the row the last slot went into.</param>
        /// <param name="lastRowCount">Receives how many slots that row holds.</param>
        /// <returns>The grid.</returns>
        private static VBox FlowGrid(
            List<SlotButtonData> slots,
            SkinSelectionLayout layout,
            out HBox lastRow,
            out int lastRowCount)
        {
            VBox grid = new VBox().InitWithOffsetAlignWidth(layout.RowSpacing, 2, layout.GridWidth);
            lastRow = null;
            lastRowCount = 0;

            foreach (SlotButtonData slot in slots)
            {
                if (slot?.Slot == null)
                {
                    continue;
                }

                SizeSlot(slot, layout);
                if (lastRow == null || lastRowCount == layout.Columns)
                {
                    lastRow = NewRow(layout);
                    lastRowCount = 0;
                    _ = grid.AddChild(lastRow);
                }

                _ = lastRow.AddChild(slot.Slot);
                lastRowCount++;
            }

            return grid;
        }

        /// <summary>The element the selection view's contents hang from.</summary>
        private static BaseElement backgroundRoot;

        /// <summary>The painted background behind the selection grid.</summary>
        private static Image backgroundImage;

        /// <summary>The button back to the main menu, drawn in the corner over the grid.</summary>
        private static Button backButton;

        /// <summary>
        /// Creates the full candy and skin selection menu view.
        /// </summary>
        /// <param name="buttonDelegate">Button delegate that receives tab, slot, and back button events.</param>
        /// <param name="candyContainer">Receives the scrollable selection container.</param>
        /// <returns>The configured candy selection menu view.</returns>
        public static MenuView CreateCandySelection(
            IButtonDelegation buttonDelegate,
            out ScrollableContainer candyContainer)
        {
            MenuView menuView = new();

            // Store delegate for later use
            currentButtonDelegate = buttonDelegate;
            currentMode = CandySelectionMode.Candy;
            modeCache = new();
            slotButtons = [];
            activePreviewBackend = null;
            activePreviewObject = null;
            omNomWarmupState = null;
            omNomXmlPreparseTask = null;

            CTRRectangle visibleBounds = ScreenPresentation.Instance.Snapshot.VisibleBounds;
            BaseElement background = new()
            {
                width = (int)visibleBounds.w,
                height = (int)visibleBounds.h
            }; // ensure child anchors use the full screen bounds instead of 0x0

            Image bgImage = Image.Image_createWithResID(Resources.BackgroundImg.SkinBackground);
            bgImage.anchor = bgImage.parentAnchor = 18; // center
            _ = background.AddChild(bgImage);
            backgroundRoot = background;
            backgroundImage = bgImage;
            CoverBackground(visibleBounds);

            FontGeneric font = Application.GetFont(Resources.Fnt.BigFont);
            candyTabButton = CreateTabButton("CANDIES_BTN", MenuButtonId.CandySelect, font, buttonDelegate);
            _ = background.AddChild(candyTabButton);
            ropeTabButton = CreateTabButton("ROPE_SKINS_BTN", MenuButtonId.RopeSelect, font, buttonDelegate);
            _ = background.AddChild(ropeTabButton);
            omNomTabButton = CreateTabButton("OM_NOM_BTN", MenuButtonId.OmNomSelect, font, buttonDelegate);
            _ = background.AddChild(omNomTabButton);
            traceTabButton = CreateTabButton("TRACES_BTN", MenuButtonId.TraceSelect, font, buttonDelegate);
            _ = background.AddChild(traceTabButton);

            SkinSelectionLayout layout = LayoutFor(ScreenPresentation.Instance.Snapshot);
            PlaceTabs(layout);

            // Create empty container initially; the grids that go in it are built per mode.
            gridContainer = new BaseElement
            {
                width = (int)layout.GridWidth,
                height = 10
            };

            candyContainer = new ScrollableContainer().InitWithWidthHeightContainer(
                layout.GridWidth,
                layout.WindowHeight,
                gridContainer);

            _ = background.AddChild(candyContainer);

            // Store container reference and build initial grid
            currentContainer = candyContainer;
            PlaceWindow(layout);
            UpdateTabButtonStates(); // Set initial tab button states (candy active)
            AttachCurrentModeGrid();
            StartOmNomWarmup();

            _ = menuView.AddChild(background);

            // Back button to return to main menu
            backButton = MenuController.CreateBackButtonWithDelegateID(buttonDelegate, MenuButtonId.BackFromCandySelect);
            backButton.SetName("backb");
            _ = menuView.AddChild(backButton);

            return menuView;
        }
    }
}
