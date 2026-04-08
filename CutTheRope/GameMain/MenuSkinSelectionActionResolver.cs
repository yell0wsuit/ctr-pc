namespace CutTheRope.GameMain
{
    /// <summary>
    /// Skin selection modes available from the menu.
    /// </summary>
    internal enum SkinSelectionMode
    {
        /// <summary>Candy skin selection mode.</summary>
        Candy,

        /// <summary>Rope skin selection mode.</summary>
        Rope,

        /// <summary>Om Nom skin selection mode.</summary>
        OmNom,

        /// <summary>Finger trace skin selection mode.</summary>
        Trace,
    }

    /// <summary>
    /// Action kinds produced by menu skin selection buttons.
    /// </summary>
    internal enum MenuSkinSelectionActionKind
    {
        /// <summary>No skin selection action should be performed.</summary>
        None,

        /// <summary>Switch to another skin selection mode.</summary>
        SwitchMode,

        /// <summary>Select a skin slot in the current mode.</summary>
        SelectSlot,
    }

    /// <summary>
    /// Resolved action for a skin selection menu button.
    /// </summary>
    /// <param name="Kind">Kind of skin selection action to perform.</param>
    /// <param name="Mode">Skin selection mode associated with the action.</param>
    /// <param name="PreferenceKey">Preference key to store for a slot selection, or <see langword="null"/> when no preference write is needed.</param>
    /// <param name="SelectedIndex">Selected slot index, or <see langword="null"/> when the action does not select a slot.</param>
    internal readonly record struct MenuSkinSelectionAction(
        MenuSkinSelectionActionKind Kind,
        SkinSelectionMode Mode,
        string PreferenceKey,
        int? SelectedIndex);

    /// <summary>
    /// Resolves menu button identifiers into skin selection actions.
    /// </summary>
    internal static class MenuSkinSelectionActionResolver
    {
        /// <summary>
        /// Resolves a menu button identifier into a skin selection action.
        /// </summary>
        /// <param name="buttonId">Menu button identifier to resolve.</param>
        /// <returns>The action represented by the button identifier.</returns>
        public static MenuSkinSelectionAction Resolve(MenuButtonId buttonId)
        {
            return (int)buttonId switch
            {
                var _ when buttonId == MenuButtonId.CandySelect => new(MenuSkinSelectionActionKind.SwitchMode, SkinSelectionMode.Candy, null, null),
                var _ when buttonId == MenuButtonId.RopeSelect => new(MenuSkinSelectionActionKind.SwitchMode, SkinSelectionMode.Rope, null, null),
                var _ when buttonId == MenuButtonId.OmNomSelect => new(MenuSkinSelectionActionKind.SwitchMode, SkinSelectionMode.OmNom, null, null),
                var _ when buttonId == MenuButtonId.TraceSelect => new(MenuSkinSelectionActionKind.SwitchMode, SkinSelectionMode.Trace, null, null),
                var _ when buttonId.IsCandySlotButton() => new(
                    MenuSkinSelectionActionKind.SelectSlot,
                    SkinSelectionMode.Candy,
                    "PREFS_SELECTED_CANDY",
                    buttonId.GetCandyIndex()
                ),
                var _ when buttonId.IsRopeSlotButton() => new(
                    MenuSkinSelectionActionKind.SelectSlot,
                    SkinSelectionMode.Rope,
                    "PREFS_SELECTED_ROPE",
                    buttonId.GetRopeIndex()
                ),
                var _ when buttonId.IsOmNomSlotButton() => new(
                    MenuSkinSelectionActionKind.SelectSlot,
                    SkinSelectionMode.OmNom,
                    "PREFS_SELECTED_OMNOM",
                    buttonId.GetOmNomIndex()
                ),
                var _ when buttonId.IsTraceSlotButton() => new(
                    MenuSkinSelectionActionKind.SelectSlot,
                    SkinSelectionMode.Trace,
                    "PREFS_SELECTED_TRACE",
                    buttonId.GetTraceIndex()
                ),
                _ => new(MenuSkinSelectionActionKind.None, SkinSelectionMode.Candy, null, null),
            };
        }
    }
}
