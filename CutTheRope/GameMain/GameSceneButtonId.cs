using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Identifier set for in-level scene specific controls.
    /// </summary>
    internal readonly record struct GameSceneButtonId(int Value) : IButtonIdentifier
    {
        public static GameSceneButtonId GravityToggle => new(0);

        public static implicit operator GameSceneButtonId(int value)
        {
            return new(value);
        }

        public static implicit operator ButtonId(GameSceneButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        public static implicit operator int(GameSceneButtonId buttonId)
        {
            return buttonId.Value;
        }

        public static GameSceneButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
