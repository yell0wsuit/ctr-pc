using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Canonical identifiers for static menu buttons.
    /// </summary>
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
        OmNomSelect,
        TraceSelect,
        ShowLanguage,
        BackFromLanguage,
    }

    /// <summary>
    /// Identifier set for menu-related buttons.
    /// </summary>
    internal readonly record struct MenuButtonId(int Value) : IButtonIdentifier
    {
        /// <summary>
        /// Starts the game flow from the main menu.
        /// </summary>
        public static readonly MenuButtonId Play = MenuButton.Play;

        /// <summary>
        /// Opens the options menu.
        /// </summary>
        public static readonly MenuButtonId Options = MenuButton.Options;

        /// <summary>
        /// Starts playing directly for first pack.
        /// </summary>
        public static readonly MenuButtonId PlayPack0 = MenuButton.PlayPack0;

        /// <summary>
        /// Enters survival mode (?).
        /// Unused.
        /// </summary>
        public static readonly MenuButtonId SurvivalMode = MenuButton.SurvivalMode;

        /// <summary>
        /// Opens the full-version/store page.
        /// Leftover from free version on mobile.
        /// </summary>
        public static readonly MenuButtonId OpenFullVersion = MenuButton.OpenFullVersion;

        /// <summary>
        /// Toggles sound effects.
        /// </summary>
        public static readonly MenuButtonId ToggleSound = MenuButton.ToggleSound;

        /// <summary>
        /// Toggles music playback.
        /// </summary>
        public static readonly MenuButtonId ToggleMusic = MenuButton.ToggleMusic;

        /// <summary>
        /// Opens credits.
        /// </summary>
        public static readonly MenuButtonId ShowCredits = MenuButton.ShowCredits;

        /// <summary>
        /// Opens reset progress confirmation.
        /// </summary>
        public static readonly MenuButtonId ShowReset = MenuButton.ShowReset;

        /// <summary>
        /// Opens leaderboards.
        /// </summary>
        public static readonly MenuButtonId Leaderboards = MenuButton.Leaderboards;

        /// <summary>
        /// Navigates back to options.
        /// </summary>
        public static readonly MenuButtonId BackToOptions = MenuButton.BackToOptions;

        /// <summary>
        /// Toggles click-to-cut controls.
        /// </summary>
        public static readonly MenuButtonId ToggleClickToCut = MenuButton.ToggleClickToCut;

        /// <summary>
        /// Opens box pack selection.
        /// </summary>
        public static readonly MenuButtonId PackSelect = MenuButton.PackSelect;

        /// <summary>
        /// Confirms reset (yes).
        /// </summary>
        public static readonly MenuButtonId ConfirmResetYes = MenuButton.ConfirmResetYes;

        /// <summary>
        /// Cancels reset (no).
        /// </summary>
        public static readonly MenuButtonId ConfirmResetNo = MenuButton.ConfirmResetNo;

        /// <summary>
        /// Confirms a single-button popup.
        /// </summary>
        public static readonly MenuButtonId PopupOk = MenuButton.PopupOk;

        /// <summary>
        /// Opens the Twitter/X page.
        /// </summary>
        public static readonly MenuButtonId OpenTwitter = MenuButton.OpenTwitter;

        /// <summary>
        /// Opens the Facebook page.
        /// </summary>
        public static readonly MenuButtonId OpenFacebook = MenuButton.OpenFacebook;

        /// <summary>
        /// Opens the fanwork project website.
        /// </summary>
        public static readonly MenuButtonId FanworkProjectWebsite = MenuButton.FanworkProjectWebsite;

        /// <summary>
        /// Opens the fanwork Cut the Rope Home website.
        /// </summary>
        public static readonly MenuButtonId FanworkCtrhWebsite = MenuButton.FanworkCtrhWebsite;

        /// <summary>
        /// Scrolls to the next pack.
        /// </summary>
        public static readonly MenuButtonId NextPack = MenuButton.NextPack;

        /// <summary>
        /// Scrolls to the previous pack.
        /// </summary>
        public static readonly MenuButtonId PreviousPack = MenuButton.PreviousPack;

        /// <summary>
        /// Opens language selection.
        /// </summary>
        public static readonly MenuButtonId Language = MenuButton.Language;

        /// <summary>
        /// Navigates back from pack selection.
        /// </summary>
        public static readonly MenuButtonId BackFromPackSelect = MenuButton.BackFromPackSelect;

        /// <summary>
        /// Navigates back from options.
        /// </summary>
        public static readonly MenuButtonId BackFromOptions = MenuButton.BackFromOptions;

        /// <summary>
        /// Navigates back from leaderboards.
        /// </summary>
        public static readonly MenuButtonId BackFromLeaderboards = MenuButton.BackFromLeaderboards;

        /// <summary>
        /// Navigates back from achievements.
        /// </summary>
        public static readonly MenuButtonId BackFromAchievements = MenuButton.BackFromAchievements;

        /// <summary>
        /// Quits the game.
        /// </summary>
        public static readonly MenuButtonId QuitGame = MenuButton.QuitGame;

        /// <summary>
        /// Closes the active popup.
        /// </summary>
        public static readonly MenuButtonId ClosePopup = MenuButton.ClosePopup;

        /// <summary>
        /// Opens quit gmae confirmation.
        /// </summary>
        public static readonly MenuButtonId ShowQuitPopup = MenuButton.ShowQuitPopup;

        /// <summary>
        /// Opens candy skin selection.
        /// </summary>
        public static readonly MenuButtonId CandySelect = MenuButton.CandySelect;

        /// <summary>
        /// Opens rope skin selection.
        /// </summary>
        public static readonly MenuButtonId RopeSelect = MenuButton.RopeSelect;

        /// <summary>
        /// Navigates back from candy skin selection.
        /// </summary>
        public static readonly MenuButtonId BackFromCandySelect = MenuButton.BackFromCandySelect;

        /// <summary>
        /// Starts update checker flow.
        /// </summary>
        public static readonly MenuButtonId UpdateDownload = MenuButton.UpdateDownload;

        /// <summary>
        /// Opens Om Nom skin selection.
        /// </summary>
        public static readonly MenuButtonId OmNomSelect = MenuButton.OmNomSelect;

        /// <summary>
        /// Opens finger trace skin selection.
        /// </summary>
        public static readonly MenuButtonId TraceSelect = MenuButton.TraceSelect;

        /// <summary>
        /// Opens language selection view.
        /// </summary>
        public static readonly MenuButtonId ShowLanguage = MenuButton.ShowLanguage;

        /// <summary>
        /// Navigates back from language selection.
        /// </summary>
        public static readonly MenuButtonId BackFromLanguage = MenuButton.BackFromLanguage;

        // Dynamic button IDs encode their type in the high byte and index in the low 24 bits.
        private const int LevelTag = 1 << 24;
        private const int PackTag = 2 << 24;
        private const int CandySlotTag = 3 << 24;
        private const int RopeSlotTag = 4 << 24;
        private const int OmNomSlotTag = 5 << 24;
        private const int TraceSlotTag = 6 << 24;
        private const int LanguageSelectTag = 7 << 24;
        private const int IndexMask = 0x00FFFFFF;

        /// <summary>
        /// Creates an identifier for a level button.
        /// </summary>
        /// <param name="levelIndex">Zero-based level index.</param>
        /// <returns>A tagged menu button identifier for the given level.</returns>
        public static MenuButtonId ForLevel(int levelIndex)
        {
            return new(LevelTag | levelIndex);
        }

        /// <summary>
        /// Creates an identifier for a pack button.
        /// </summary>
        /// <param name="packIndex">Zero-based pack index.</param>
        /// <returns>A tagged menu button identifier for the given pack.</returns>
        public static MenuButtonId ForPack(int packIndex)
        {
            return new(PackTag | packIndex);
        }

        /// <summary>
        /// Creates an identifier for a candy skin slot button.
        /// </summary>
        /// <param name="candyIndex">Zero-based candy skin slot index.</param>
        /// <returns>A tagged menu button identifier for the given candy skin slot.</returns>
        public static MenuButtonId ForCandySlot(int candyIndex)
        {
            return new(CandySlotTag | candyIndex);
        }

        /// <summary>
        /// Creates an identifier for a rope skin slot button.
        /// </summary>
        /// <param name="ropeIndex">Zero-based rope skin slot index.</param>
        /// <returns>A tagged menu button identifier for the given rope skin slot.</returns>
        public static MenuButtonId ForRopeSlot(int ropeIndex)
        {
            return new(RopeSlotTag | ropeIndex);
        }

        /// <summary>
        /// Creates an identifier for an Om Nom skin slot button.
        /// </summary>
        /// <param name="omNomIndex">Zero-based Om Nom skin index.</param>
        /// <returns>A tagged menu button identifier for the given Om Nom skin slot.</returns>
        public static MenuButtonId ForOmNomSlot(int omNomIndex)
        {
            return new(OmNomSlotTag | omNomIndex);
        }

        /// <summary>
        /// Creates an identifier for a finger trace skin slot button.
        /// </summary>
        /// <param name="traceIndex">Zero-based finger trace skin index.</param>
        /// <returns>A tagged menu button identifier for the given finger trace slot.</returns>
        public static MenuButtonId ForTraceSlot(int traceIndex)
        {
            return new(TraceSlotTag | traceIndex);
        }

        /// <summary>
        /// Creates an identifier for a language selection button.
        /// </summary>
        /// <param name="languageIndex">Zero-based language index in the UI language list.</param>
        /// <returns>A tagged menu button identifier for the given language.</returns>
        public static MenuButtonId ForLanguage(int languageIndex)
        {
            return new(LanguageSelectTag | languageIndex);
        }

        /// <summary>
        /// Determines whether this identifier represents a dynamic level button.
        /// </summary>
        /// <returns><see langword="true"/> when this is a level button; otherwise <see langword="false"/>.</returns>
        public bool IsLevelButton()
        {
            return (Value >> 24) == 1;
        }

        /// <summary>
        /// Determines whether this identifier represents a dynamic pack button.
        /// </summary>
        /// <returns><see langword="true"/> when this is a pack button; otherwise <see langword="false"/>.</returns>
        public bool IsPackButton()
        {
            return (Value >> 24) == 2;
        }

        /// <summary>
        /// Determines whether this identifier represents a dynamic candy skin slot button.
        /// </summary>
        /// <returns><see langword="true"/> when this is a candy skin slot button; otherwise <see langword="false"/>.</returns>
        public bool IsCandySlotButton()
        {
            return (Value >> 24) == 3;
        }

        /// <summary>
        /// Determines whether this identifier represents a dynamic rope skin slot button.
        /// </summary>
        /// <returns><see langword="true"/> when this is a rope skin slot button; otherwise <see langword="false"/>.</returns>
        public bool IsRopeSlotButton()
        {
            return (Value >> 24) == 4;
        }

        /// <summary>
        /// Determines whether this identifier represents a dynamic Om Nom skin slot button.
        /// </summary>
        /// <returns><see langword="true"/> when this is an Om Nom skin slot button; otherwise <see langword="false"/>.</returns>
        public bool IsOmNomSlotButton()
        {
            return (Value >> 24) == 5;
        }

        /// <summary>
        /// Determines whether this identifier represents a dynamic finger trace slot button.
        /// </summary>
        /// <returns><see langword="true"/> when this is a finger trace slot button; otherwise <see langword="false"/>.</returns>
        public bool IsTraceSlotButton()
        {
            return (Value >> 24) == 6;
        }

        /// <summary>
        /// Determines whether this identifier represents a dynamic language selection button.
        /// </summary>
        /// <returns><see langword="true"/> when this is a language selection button; otherwise <see langword="false"/>.</returns>
        public bool IsLanguageSelectButton()
        {
            return (Value >> 24) == 7;
        }

        /// <summary>
        /// Gets the zero-based level index when this identifier is a level button.
        /// </summary>
        /// <returns>The level index, or <c>-1</c> when this is not a level button.</returns>
        public int GetLevelIndex()
        {
            return IsLevelButton() ? Value & IndexMask : -1;
        }

        /// <summary>
        /// Gets the zero-based pack index when this identifier is a pack button.
        /// </summary>
        /// <returns>The pack index, or <c>-1</c> when this is not a pack button.</returns>
        public int GetPackIndex()
        {
            return IsPackButton() ? Value & IndexMask : -1;
        }

        /// <summary>
        /// Gets the zero-based candy skin slot index when this identifier is a candy skin slot button.
        /// </summary>
        /// <returns>The candy skin slot index, or <c>-1</c> when this is not a candy skin slot button.</returns>
        public int GetCandyIndex()
        {
            return IsCandySlotButton() ? Value & IndexMask : -1;
        }

        /// <summary>
        /// Gets the zero-based rope skin slot index when this identifier is a rope slot button.
        /// </summary>
        /// <returns>The rope skin slot index, or <c>-1</c> when this is not a rope skin slot button.</returns>
        public int GetRopeIndex()
        {
            return IsRopeSlotButton() ? Value & IndexMask : -1;
        }

        /// <summary>
        /// Gets the zero-based Om Nom skin index when this identifier is an Om Nom slot button.
        /// </summary>
        /// <returns>The Om Nom skin index, or <c>-1</c> when this is not an Om Nom slot button.</returns>
        public int GetOmNomIndex()
        {
            return IsOmNomSlotButton() ? Value & IndexMask : -1;
        }

        /// <summary>
        /// Gets the zero-based finger trace slot index when this identifier is a trace slot button.
        /// </summary>
        /// <returns>The trace slot index, or <c>-1</c> when this is not a trace slot button.</returns>
        public int GetTraceIndex()
        {
            return IsTraceSlotButton() ? Value & IndexMask : -1;
        }

        /// <summary>
        /// Gets the zero-based language index when this identifier is a language selection button.
        /// </summary>
        /// <returns>The language index, or <c>-1</c> when this is not a language selection button.</returns>
        public int GetLanguageSelectIndex()
        {
            return IsLanguageSelectButton() ? Value & IndexMask : -1;
        }

        /// <summary>
        /// Converts a static <see cref="MenuButton"/> value into a typed identifier.
        /// </summary>
        /// <param name="button">Static menu button value.</param>
        public static implicit operator MenuButtonId(MenuButton button)
        {
            return new((int)button);
        }

        /// <summary>
        /// Converts a raw integer identifier into a typed menu identifier.
        /// </summary>
        /// <param name="value">Raw button ID value.</param>
        public static implicit operator MenuButtonId(int value)
        {
            return new(value);
        }

        /// <summary>
        /// Converts this typed menu identifier to the shared <see cref="ButtonId"/> wrapper.
        /// </summary>
        /// <param name="buttonId">Typed menu identifier.</param>
        public static implicit operator ButtonId(MenuButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        /// <summary>
        /// Extracts the raw integer value from a typed menu identifier.
        /// </summary>
        /// <param name="buttonId">Typed menu identifier.</param>
        public static implicit operator int(MenuButtonId buttonId)
        {
            return buttonId.Value;
        }

        /// <summary>
        /// Converts a shared <see cref="ButtonId"/> into a typed menu identifier.
        /// </summary>
        /// <param name="buttonId">Shared button identifier.</param>
        /// <returns>Typed menu identifier.</returns>
        public static MenuButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
