namespace CutTheRope.GameMain
{
    internal enum SkinSelectionMode
    {
        Candy,
        Rope,
        OmNom,
    }

    internal enum MenuSkinSelectionActionKind
    {
        None,
        SwitchMode,
        SelectSlot,
    }

    internal readonly record struct MenuSkinSelectionAction(
        MenuSkinSelectionActionKind Kind,
        SkinSelectionMode Mode,
        string PreferenceKey,
        int? SelectedIndex);

    internal static class MenuSkinSelectionActionResolver
    {
        public static MenuSkinSelectionAction Resolve(MenuButtonId buttonId)
        {
            return (int)buttonId switch
            {
                var _ when buttonId == MenuButtonId.CandySelect => new(MenuSkinSelectionActionKind.SwitchMode, SkinSelectionMode.Candy, null, null),
                var _ when buttonId == MenuButtonId.RopeSelect => new(MenuSkinSelectionActionKind.SwitchMode, SkinSelectionMode.Rope, null, null),
                var _ when buttonId == MenuButtonId.OmNomSelect => new(MenuSkinSelectionActionKind.SwitchMode, SkinSelectionMode.OmNom, null, null),
                var _ when buttonId.IsCandySlotButton() => new(
                    MenuSkinSelectionActionKind.SelectSlot,
                    SkinSelectionMode.Candy,
                    CTRPreferences.PREFS_SELECTED_CANDY,
                    buttonId.GetCandyIndex()
                ),
                var _ when buttonId.IsRopeSlotButton() => new(
                    MenuSkinSelectionActionKind.SelectSlot,
                    SkinSelectionMode.Rope,
                    CTRPreferences.PREFS_SELECTED_ROPE,
                    buttonId.GetRopeIndex()
                ),
                var _ when buttonId.IsOmNomSlotButton() => new(
                    MenuSkinSelectionActionKind.SelectSlot,
                    SkinSelectionMode.OmNom,
                    CTRPreferences.PREFS_SELECTED_OMNOM,
                    buttonId.GetOmNomIndex()
                ),
                _ => new(MenuSkinSelectionActionKind.None, SkinSelectionMode.Candy, null, null),
            };
        }
    }
}
