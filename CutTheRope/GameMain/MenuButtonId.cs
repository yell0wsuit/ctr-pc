using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal enum MenuButton
    {
        Play,
        Options,
        PlayPack0,
        SurvivalMode,
        OpenFullVersion,
        ToggleSound,
        ToggleMusic,
        ShowCredits,
        ShowReset,
        Leaderboards,
        BackToOptions,
        ToggleClickToCut,
        PackSelect,
        ConfirmResetYes,
        ConfirmResetNo,
        PopupOk,
        OpenTwitter,
        OpenFacebook,
        FanworkProjectWebsite,
        FanworkCtrhWebsite,
        NextPack,
        PreviousPack,
        Language,
        BackFromPackSelect,
        BackFromOptions,
        BackFromLeaderboards,
        BackFromAchievements,
        QuitGame,
        ClosePopup,
        ShowQuitPopup,
        CandySelect,
        RopeSelect,
        BackFromCandySelect,
        UpdateDownload,
    }

    /// <summary>
    /// Identifier set for menu-related buttons.
    /// </summary>
    internal readonly record struct MenuButtonId(int Value) : IButtonIdentifier
    {
        public static readonly MenuButtonId Play = MenuButton.Play;
        public static readonly MenuButtonId Options = MenuButton.Options;
        public static readonly MenuButtonId PlayPack0 = MenuButton.PlayPack0;
        public static readonly MenuButtonId SurvivalMode = MenuButton.SurvivalMode;
        public static readonly MenuButtonId OpenFullVersion = MenuButton.OpenFullVersion;
        public static readonly MenuButtonId ToggleSound = MenuButton.ToggleSound;
        public static readonly MenuButtonId ToggleMusic = MenuButton.ToggleMusic;
        public static readonly MenuButtonId ShowCredits = MenuButton.ShowCredits;
        public static readonly MenuButtonId ShowReset = MenuButton.ShowReset;
        public static readonly MenuButtonId Leaderboards = MenuButton.Leaderboards;
        public static readonly MenuButtonId BackToOptions = MenuButton.BackToOptions;
        public static readonly MenuButtonId ToggleClickToCut = MenuButton.ToggleClickToCut;
        public static readonly MenuButtonId PackSelect = MenuButton.PackSelect;
        public static readonly MenuButtonId ConfirmResetYes = MenuButton.ConfirmResetYes;
        public static readonly MenuButtonId ConfirmResetNo = MenuButton.ConfirmResetNo;
        public static readonly MenuButtonId PopupOk = MenuButton.PopupOk;
        public static readonly MenuButtonId OpenTwitter = MenuButton.OpenTwitter;
        public static readonly MenuButtonId OpenFacebook = MenuButton.OpenFacebook;
        public static readonly MenuButtonId FanworkProjectWebsite = MenuButton.FanworkProjectWebsite;
        public static readonly MenuButtonId FanworkCtrhWebsite = MenuButton.FanworkCtrhWebsite;
        public static readonly MenuButtonId NextPack = MenuButton.NextPack;
        public static readonly MenuButtonId PreviousPack = MenuButton.PreviousPack;
        public static readonly MenuButtonId Language = MenuButton.Language;
        public static readonly MenuButtonId BackFromPackSelect = MenuButton.BackFromPackSelect;
        public static readonly MenuButtonId BackFromOptions = MenuButton.BackFromOptions;
        public static readonly MenuButtonId BackFromLeaderboards = MenuButton.BackFromLeaderboards;
        public static readonly MenuButtonId BackFromAchievements = MenuButton.BackFromAchievements;
        public static readonly MenuButtonId QuitGame = MenuButton.QuitGame;
        public static readonly MenuButtonId ClosePopup = MenuButton.ClosePopup;
        public static readonly MenuButtonId ShowQuitPopup = MenuButton.ShowQuitPopup;
        public static readonly MenuButtonId CandySelect = MenuButton.CandySelect;
        public static readonly MenuButtonId RopeSelect = MenuButton.RopeSelect;
        public static readonly MenuButtonId BackFromCandySelect = MenuButton.BackFromCandySelect;
        public static readonly MenuButtonId UpdateDownload = MenuButton.UpdateDownload;

        // Dynamic button IDs encode their type in the high byte and index in the low 24 bits.
        private const int LevelTag = 1 << 24;
        private const int PackTag = 2 << 24;
        private const int CandySlotTag = 3 << 24;
        private const int RopeSlotTag = 4 << 24;
        private const int IndexMask = 0x00FFFFFF;

        public static MenuButtonId ForLevel(int levelIndex)
        {
            return new(LevelTag | levelIndex);
        }

        public static MenuButtonId ForPack(int packIndex)
        {
            return new(PackTag | packIndex);
        }

        public static MenuButtonId ForCandySlot(int candyIndex)
        {
            return new(CandySlotTag | candyIndex);
        }

        public static MenuButtonId ForRopeSlot(int ropeIndex)
        {
            return new(RopeSlotTag | ropeIndex);
        }

        public bool IsLevelButton()
        {
            return (Value >> 24) == 1;
        }

        public bool IsPackButton()
        {
            return (Value >> 24) == 2;
        }

        public bool IsCandySlotButton()
        {
            return (Value >> 24) == 3;
        }

        public bool IsRopeSlotButton()
        {
            return (Value >> 24) == 4;
        }

        public int GetLevelIndex()
        {
            return IsLevelButton() ? Value & IndexMask : -1;
        }

        public int GetPackIndex()
        {
            return IsPackButton() ? Value & IndexMask : -1;
        }

        public int GetCandyIndex()
        {
            return IsCandySlotButton() ? Value & IndexMask : -1;
        }

        public int GetRopeIndex()
        {
            return IsRopeSlotButton() ? Value & IndexMask : -1;
        }

        public static implicit operator MenuButtonId(MenuButton button)
        {
            return new((int)button);
        }

        public static implicit operator MenuButtonId(int value)
        {
            return new(value);
        }

        public static implicit operator ButtonId(MenuButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        public static implicit operator int(MenuButtonId buttonId)
        {
            return buttonId.Value;
        }

        public static MenuButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
