using System;
using System.Collections.Generic;
using System.Globalization;

using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Identifier set for menu-related buttons.
    /// </summary>
    internal readonly record struct MenuButtonId(int Value) : IButtonIdentifier
    {
        // String-based button identifiers with auto-assigned numeric IDs
        public static readonly MenuButtonId Play;
        public static readonly MenuButtonId Options;
        public static readonly MenuButtonId PlayPack0;
        public static readonly MenuButtonId SurvivalMode;
        public static readonly MenuButtonId OpenFullVersion;
        public static readonly MenuButtonId ToggleSound;
        public static readonly MenuButtonId ToggleMusic;
        public static readonly MenuButtonId ShowCredits;
        public static readonly MenuButtonId ShowReset;
        public static readonly MenuButtonId Leaderboards;
        public static readonly MenuButtonId BackToOptions;
        public static readonly MenuButtonId ToggleClickToCut;
        public static readonly MenuButtonId PackSelect;
        public static readonly MenuButtonId ConfirmResetYes;
        public static readonly MenuButtonId ConfirmResetNo;
        public static readonly MenuButtonId PopupOk;
        public static readonly MenuButtonId OpenTwitter;
        public static readonly MenuButtonId OpenFacebook;
        public static readonly MenuButtonId LeaderboardsAchievementsUnused;
        public static readonly MenuButtonId MoreGamesUnused;
        public static readonly MenuButtonId NextPack;
        public static readonly MenuButtonId PreviousPack;
        public static readonly MenuButtonId Language;
        public static readonly MenuButtonId PackSelectBase;
        public static readonly MenuButtonId BackFromPackSelect;
        public static readonly MenuButtonId BackFromOptions;
        public static readonly MenuButtonId BackFromLeaderboards;
        public static readonly MenuButtonId BackFromAchievements;
        public static readonly MenuButtonId QuitGame;
        public static readonly MenuButtonId ClosePopup;
        public static readonly MenuButtonId ShowQuitPopup;
        public static readonly MenuButtonId LevelButtonBase;

        static MenuButtonId()
        {
            // Initialize all IDs in a predictable order to ensure consistent auto-assignment
            Play = FromName(nameof(Play));
            Options = FromName(nameof(Options));
            PlayPack0 = FromName(nameof(PlayPack0));
            SurvivalMode = FromName(nameof(SurvivalMode));
            OpenFullVersion = FromName(nameof(OpenFullVersion));
            ToggleSound = FromName(nameof(ToggleSound));
            ToggleMusic = FromName(nameof(ToggleMusic));
            ShowCredits = FromName(nameof(ShowCredits));
            ShowReset = FromName(nameof(ShowReset));
            Leaderboards = FromName(nameof(Leaderboards));
            BackToOptions = FromName(nameof(BackToOptions));
            ToggleClickToCut = FromName(nameof(ToggleClickToCut));
            PackSelect = FromName(nameof(PackSelect));
            ConfirmResetYes = FromName(nameof(ConfirmResetYes));
            ConfirmResetNo = FromName(nameof(ConfirmResetNo));
            PopupOk = FromName(nameof(PopupOk));
            OpenTwitter = FromName(nameof(OpenTwitter));
            OpenFacebook = FromName(nameof(OpenFacebook));
            LeaderboardsAchievementsUnused = FromName(nameof(LeaderboardsAchievementsUnused));
            MoreGamesUnused = FromName(nameof(MoreGamesUnused));
            NextPack = FromName(nameof(NextPack));
            PreviousPack = FromName(nameof(PreviousPack));
            Language = FromName(nameof(Language));
            PackSelectBase = FromName(nameof(PackSelectBase));
            BackFromPackSelect = FromName(nameof(BackFromPackSelect));
            BackFromOptions = FromName(nameof(BackFromOptions));
            BackFromLeaderboards = FromName(nameof(BackFromLeaderboards));
            BackFromAchievements = FromName(nameof(BackFromAchievements));
            QuitGame = FromName(nameof(QuitGame));
            ClosePopup = FromName(nameof(ClosePopup));
            ShowQuitPopup = FromName(nameof(ShowQuitPopup));
            LevelButtonBase = FromName(nameof(LevelButtonBase));
        }

        /// <summary>
        /// Creates a dynamic level button ID by combining the base with a level index.
        /// </summary>
        public static MenuButtonId ForLevel(int levelIndex)
        {
            return FromName($"Level_{levelIndex}");
        }

        /// <summary>
        /// Creates a dynamic pack button ID by combining the base with a pack index.
        /// </summary>
        public static MenuButtonId ForPack(int packIndex)
        {
            return FromName($"Pack_{packIndex}");
        }

        /// <summary>
        /// Checks if this button ID represents a level button.
        /// </summary>
        public bool IsLevelButton()
        {
            string name = GetName(this);
            return name?.StartsWith("Level_", StringComparison.Ordinal) ?? false;
        }

        /// <summary>
        /// Checks if this button ID represents a pack button.
        /// </summary>
        public bool IsPackButton()
        {
            string name = GetName(this);
            return name?.StartsWith("Pack_", StringComparison.Ordinal) ?? false;
        }

        /// <summary>
        /// Gets the level index from a level button ID.
        /// </summary>
        public int GetLevelIndex()
        {
            string name = GetName(this);
            return name?.StartsWith("Level_", StringComparison.Ordinal) ?? false ? int.Parse(name[6..], CultureInfo.InvariantCulture) : -1;
        }

        /// <summary>
        /// Gets the pack index from a pack button ID.
        /// </summary>
        public int GetPackIndex()
        {
            string name = GetName(this);
            return name?.StartsWith("Pack_", StringComparison.Ordinal) ?? false ? int.Parse(name[5..], CultureInfo.InvariantCulture) : -1;
        }

        /// <summary>
        /// Creates a MenuButtonId from a string name, auto-assigning a numeric ID if needed.
        /// </summary>
        private static MenuButtonId FromName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new(-1);
            }

            if (stringToIntMap_.TryGetValue(name, out int existingId))
            {
                return new(existingId);
            }

            // Auto-assign a new ID for this button
            int newId = nextAutoId_++;
            stringToIntMap_[name] = newId;
            intToStringMap_[newId] = name;
            return new(newId);
        }

        /// <summary>
        /// Gets the string name for a MenuButtonId. Returns null if not found.
        /// </summary>
        public static string GetName(MenuButtonId buttonId)
        {
            _ = intToStringMap_.TryGetValue(buttonId.Value, out string name);
            return name;
        }

        /// <summary>
        /// Implicitly wraps a raw value into a typed <see cref="MenuButtonId"/>.
        /// </summary>
        /// <param name="value">Numeric identifier previously represented as an <see cref="int"/>.</param>
        public static implicit operator MenuButtonId(int value)
        {
            return new(value);
        }

        /// <summary>
        /// Converts the typed identifier into a generic <see cref="ButtonId"/>.
        /// </summary>
        /// <param name="buttonId">Typed identifier to wrap.</param>
        public static implicit operator ButtonId(MenuButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        /// <summary>
        /// Extracts the raw numeric value.
        /// </summary>
        /// <param name="buttonId">Typed identifier to unwrap.</param>
        public static implicit operator int(MenuButtonId buttonId)
        {
            return buttonId.Value;
        }

        /// <summary>
        /// Constructs a typed identifier from a shared <see cref="ButtonId"/>.
        /// </summary>
        /// <param name="buttonId">Identifier emitted by the button infrastructure.</param>
        /// <returns>Typed menu identifier.</returns>
        public static MenuButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }

        // Auto-assignment tracking
        private static readonly Dictionary<string, int> stringToIntMap_ = [];
        private static readonly Dictionary<int, string> intToStringMap_ = [];
        private static int nextAutoId_;
    }
}
