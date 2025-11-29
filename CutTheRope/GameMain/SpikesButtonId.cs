using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Identifier set for spike rotation controls.
    /// </summary>
    internal readonly record struct SpikesButtonId(int Value) : IButtonIdentifier
    {
        public static SpikesButtonId Rotate => new(0);

        public static implicit operator SpikesButtonId(int value)
        {
            return new(value);
        }

        public static implicit operator ButtonId(SpikesButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        public static implicit operator int(SpikesButtonId buttonId)
        {
            return buttonId.Value;
        }

        public static SpikesButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
