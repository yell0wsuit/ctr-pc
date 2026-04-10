using System.Collections;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Selection modes shown by the candy and skin selection menu.
    /// </summary>
    internal enum CandySelectionMode
    {
        /// <summary>Candy skin selection mode.</summary>
        Candy,

        /// <summary>Rope skin selection mode.</summary>
        Rope,

        /// <summary>Om Nom skin selection mode.</summary>
        OmNom,

        /// <summary>Finger trace skin selection mode.</summary>
        Trace
    }

    /// <summary>
    /// Cached view state for one candy selection mode.
    /// </summary>
    internal sealed class CandySelectionModeState
    {
        /// <summary>Grid element built for the mode.</summary>
        public BaseElement Grid { get; set; }

        /// <summary>Slot buttons built for the mode.</summary>
        public IList SlotButtons { get; set; }

        /// <summary>Animation backend currently driving the active preview object.</summary>
        public ITargetAnimationBackend ActivePreviewBackend { get; set; }

        /// <summary>Preview object currently active for the mode.</summary>
        public GameObject ActivePreviewObject { get; set; }
    }

    /// <summary>
    /// Result of activating a candy selection mode.
    /// </summary>
    /// <param name="Mode">Activated selection mode.</param>
    /// <param name="State">Cached state for the activated mode.</param>
    /// <param name="RequiresBuild">Whether the mode needs its grid to be built before display.</param>
    internal readonly record struct CandySelectionModeActivation(
        CandySelectionMode Mode,
        CandySelectionModeState State,
        bool RequiresBuild);

    /// <summary>
    /// Caches built grids and preview state for candy selection modes.
    /// </summary>
    internal sealed class CandySelectionModeCache
    {
        /// <summary>Cached state keyed by selection mode.</summary>
        private readonly Dictionary<CandySelectionMode, CandySelectionModeState> states = new()
        {
            [CandySelectionMode.Candy] = new(),
            [CandySelectionMode.Rope] = new(),
            [CandySelectionMode.OmNom] = new(),
            [CandySelectionMode.Trace] = new()
        };

        /// <summary>
        /// Activates a selection mode and reports whether the mode still needs to be built.
        /// </summary>
        /// <param name="mode">Selection mode to activate.</param>
        /// <returns>Activation information for the requested mode.</returns>
        public CandySelectionModeActivation ActivateMode(CandySelectionMode mode)
        {
            CandySelectionModeState state = GetState(mode);
            return new CandySelectionModeActivation(mode, state, state.Grid == null);
        }

        /// <summary>
        /// Gets the cached state for a selection mode.
        /// </summary>
        /// <param name="mode">Selection mode whose state should be returned.</param>
        /// <returns>The cached state for the mode.</returns>
        public CandySelectionModeState GetState(CandySelectionMode mode)
        {
            return states[mode];
        }

        /// <summary>
        /// Stores the grid element for a selection mode.
        /// </summary>
        /// <param name="mode">Selection mode whose grid should be stored.</param>
        /// <param name="grid">Grid element to cache.</param>
        public void StoreGrid(CandySelectionMode mode, BaseElement grid)
        {
            GetState(mode).Grid = grid;
        }

        /// <summary>
        /// Stores the full cached state for a selection mode.
        /// </summary>
        /// <param name="mode">Selection mode whose state should be stored.</param>
        /// <param name="grid">Grid element to cache.</param>
        /// <param name="slotButtons">Slot buttons to cache.</param>
        /// <param name="activePreviewObject">Active preview object to cache.</param>
        /// <param name="activePreviewBackend">Active preview backend to cache, or <see langword="null"/> when the preview has no backend.</param>
        public void StoreState(
            CandySelectionMode mode,
            BaseElement grid,
            IList slotButtons,
            GameObject activePreviewObject,
            ITargetAnimationBackend activePreviewBackend = null)
        {
            CandySelectionModeState state = GetState(mode);
            state.Grid = grid;
            state.SlotButtons = slotButtons;
            state.ActivePreviewObject = activePreviewObject;
            state.ActivePreviewBackend = activePreviewBackend;
        }
    }
}
