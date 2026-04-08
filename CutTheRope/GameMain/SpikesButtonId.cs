using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Identifier set for spike rotation controls.
    /// </summary>
    /// <param name="Value">Underlying button identifier value.</param>
    internal readonly record struct SpikesButtonId(int Value) : IButtonIdentifier
    {
        /// <summary>Button identifier for rotating spikes.</summary>
        public static SpikesButtonId Rotate => new(0);

        /// <summary>
        /// Converts an integer value to a spike button identifier.
        /// </summary>
        /// <param name="value">Underlying button identifier value.</param>
        public static implicit operator SpikesButtonId(int value)
        {
            return new(value);
        }

        /// <summary>
        /// Converts a spike button identifier to a generic button identifier.
        /// </summary>
        /// <param name="buttonId">Spike button identifier to convert.</param>
        public static implicit operator ButtonId(SpikesButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        /// <summary>
        /// Converts a spike button identifier to its integer value.
        /// </summary>
        /// <param name="buttonId">Spike button identifier to convert.</param>
        public static implicit operator int(SpikesButtonId buttonId)
        {
            return buttonId.Value;
        }

        /// <summary>
        /// Creates a spike button identifier from a generic button identifier.
        /// </summary>
        /// <param name="buttonId">Generic button identifier to convert.</param>
        /// <returns>The converted spike button identifier.</returns>
        public static SpikesButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
