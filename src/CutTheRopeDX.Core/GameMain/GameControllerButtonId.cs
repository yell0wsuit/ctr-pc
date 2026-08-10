using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Identifier set for in-game HUD and pause menu controls.
    /// </summary>
    /// <param name="Value">Underlying numeric button identifier.</param>
    internal readonly record struct GameControllerButtonId(int Value) : IButtonIdentifier
    {
        /// <summary>
        /// Continues from an interstitial or results screen.
        /// </summary>
        public static GameControllerButtonId Continue => new(0);

        /// <summary>
        /// Restarts the current level.
        /// </summary>
        public static GameControllerButtonId Restart => new(1);

        /// <summary>
        /// Skips the current level.
        /// </summary>
        public static GameControllerButtonId SkipLevel => new(2);

        /// <summary>
        /// Opens level selection.
        /// </summary>
        public static GameControllerButtonId LevelSelect => new(3);

        /// <summary>
        /// Returns to the main menu.
        /// </summary>
        public static GameControllerButtonId MainMenu => new(4);

        /// <summary>
        /// Exits from the win screen.
        /// </summary>
        public static GameControllerButtonId ExitFromWin => new(5);

        /// <summary>
        /// Opens or represents the pause action.
        /// </summary>
        public static GameControllerButtonId Pause => new(6);

        /// <summary>
        /// Continues from the win screen.
        /// </summary>
        public static GameControllerButtonId WinContinue => new(7);

        /// <summary>
        /// Exits from the lose screen.
        /// </summary>
        public static GameControllerButtonId ExitFromLose => new(8);

        /// <summary>
        /// Advances to the next level.
        /// </summary>
        public static GameControllerButtonId NextLevel => new(9);

        /// <summary>
        /// Toggles music playback.
        /// </summary>
        public static GameControllerButtonId ToggleMusic => new(10);

        /// <summary>
        /// Toggles sound effects playback.
        /// </summary>
        public static GameControllerButtonId ToggleSound => new(11);

        /// <summary>
        /// Converts a raw integer button value to a game-controller button identifier.
        /// </summary>
        /// <param name="value">Raw button value.</param>
        public static implicit operator GameControllerButtonId(int value)
        {
            return new(value);
        }

        /// <summary>
        /// Converts a game-controller button identifier to the generic button identifier type.
        /// </summary>
        /// <param name="buttonId">Game-controller button identifier.</param>
        public static implicit operator ButtonId(GameControllerButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        /// <summary>
        /// Converts a game-controller button identifier to its raw integer value.
        /// </summary>
        /// <param name="buttonId">Game-controller button identifier.</param>
        public static implicit operator int(GameControllerButtonId buttonId)
        {
            return buttonId.Value;
        }

        /// <summary>
        /// Converts a generic button identifier to a game-controller button identifier.
        /// </summary>
        /// <param name="buttonId">Generic button identifier.</param>
        /// <returns>A game-controller button identifier with the same raw value.</returns>
        public static GameControllerButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
