using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Identifier set for menu-related buttons.
    /// </summary>
    internal readonly record struct MenuButtonId(int Value) : IButtonIdentifier
    {
        public static MenuButtonId Play => new(0);

        public static MenuButtonId Options => new(1);

        public static MenuButtonId PlayPack0 => new(2);

        public static MenuButtonId SurvivalMode => new(3);

        public static MenuButtonId OpenFullVersion => new(4);

        public static MenuButtonId ToggleSound => new(5);

        public static MenuButtonId ToggleMusic => new(6);

        public static MenuButtonId ShowCredits => new(7);

        public static MenuButtonId ShowReset => new(8);

        public static MenuButtonId Leaderboards => new(9);

        public static MenuButtonId BackToOptions => new(10);

        public static MenuButtonId ToggleClickToCut => new(11);

        public static MenuButtonId PackSelect => new(12);

        public static MenuButtonId ConfirmResetYes => new(13);

        public static MenuButtonId ConfirmResetNo => new(14);

        public static MenuButtonId PopupOk => new(15);

        public static MenuButtonId OpenTwitter => new(16);

        public static MenuButtonId OpenFacebook => new(17);

        public static MenuButtonId LeaderboardsAchievementsUnused => new(18);

        public static MenuButtonId MoreGamesUnused => new(19);

        public static MenuButtonId NextPack => new(20);

        public static MenuButtonId PreviousPack => new(21);

        public static MenuButtonId Language => new(22);

        public static MenuButtonId PackSelectBase => new(23);

        public static MenuButtonId BackFromPackSelect => new(35);

        public static MenuButtonId BackFromOptions => new(36);

        public static MenuButtonId BackFromLeaderboards => new(37);

        public static MenuButtonId BackFromAchievements => new(38);

        public static MenuButtonId QuitGame => new(39);

        public static MenuButtonId ClosePopup => new(40);

        public static MenuButtonId ShowQuitPopup => new(41);

        public static MenuButtonId LevelButtonBase => new(1000);

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
    }
}
