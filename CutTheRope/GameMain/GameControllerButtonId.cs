using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Identifier set for in-game HUD and pause menu controls.
    /// </summary>
    internal readonly record struct GameControllerButtonId(int Value) : IButtonIdentifier
    {
        public static GameControllerButtonId Continue => new(0);

        public static GameControllerButtonId Restart => new(1);

        public static GameControllerButtonId SkipLevel => new(2);

        public static GameControllerButtonId LevelSelect => new(3);

        public static GameControllerButtonId MainMenu => new(4);

        public static GameControllerButtonId ExitFromWin => new(5);

        public static GameControllerButtonId Pause => new(6);

        public static GameControllerButtonId WinContinue => new(7);

        public static GameControllerButtonId ExitFromLose => new(8);

        public static GameControllerButtonId NextLevel => new(9);

        public static GameControllerButtonId ToggleMusic => new(10);

        public static GameControllerButtonId ToggleSound => new(11);

        public static implicit operator GameControllerButtonId(int value)
        {
            return new(value);
        }

        public static implicit operator ButtonId(GameControllerButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        public static implicit operator int(GameControllerButtonId buttonId)
        {
            return buttonId.Value;
        }

        public static GameControllerButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
