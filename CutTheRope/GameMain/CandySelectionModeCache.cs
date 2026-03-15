using System.Collections;
using System.Collections.Generic;

using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal enum CandySelectionMode
    {
        Candy,
        Rope,
        OmNom
    }

    internal sealed class CandySelectionModeState
    {
        public BaseElement Grid { get; set; }
        public IList SlotButtons { get; set; }
        public ITargetAnimationBackend ActivePreviewBackend { get; set; }
        public GameObject ActivePreviewObject { get; set; }
    }

    internal readonly record struct CandySelectionModeActivation(
        CandySelectionMode Mode,
        CandySelectionModeState State,
        bool RequiresBuild);

    internal sealed class CandySelectionModeCache
    {
        private readonly Dictionary<CandySelectionMode, CandySelectionModeState> states = new()
        {
            [CandySelectionMode.Candy] = new(),
            [CandySelectionMode.Rope] = new(),
            [CandySelectionMode.OmNom] = new()
        };

        public CandySelectionModeActivation ActivateMode(CandySelectionMode mode)
        {
            CandySelectionModeState state = GetState(mode);
            return new CandySelectionModeActivation(mode, state, state.Grid == null);
        }

        public CandySelectionModeState GetState(CandySelectionMode mode)
        {
            return states[mode];
        }

        public void StoreGrid(CandySelectionMode mode, BaseElement grid)
        {
            GetState(mode).Grid = grid;
        }

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
