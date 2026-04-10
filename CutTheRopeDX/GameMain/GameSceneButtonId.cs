using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Identifier set for in-level scene specific controls.
    /// </summary>
    /// <param name="Value">Underlying numeric button identifier.</param>
    internal readonly record struct GameSceneButtonId(int Value) : IButtonIdentifier
    {
        /// <summary>
        /// Toggles gravity in the active scene.
        /// </summary>
        public static GameSceneButtonId GravityToggle => new(0);

        /// <summary>
        /// Converts a raw integer button value to a scene button identifier.
        /// </summary>
        /// <param name="value">Raw button value.</param>
        public static implicit operator GameSceneButtonId(int value)
        {
            return new(value);
        }

        /// <summary>
        /// Converts a scene button identifier to the generic button identifier type.
        /// </summary>
        /// <param name="buttonId">Scene button identifier.</param>
        public static implicit operator ButtonId(GameSceneButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        /// <summary>
        /// Converts a scene button identifier to its raw integer value.
        /// </summary>
        /// <param name="buttonId">Scene button identifier.</param>
        public static implicit operator int(GameSceneButtonId buttonId)
        {
            return buttonId.Value;
        }

        /// <summary>
        /// Converts a generic button identifier to a scene button identifier.
        /// </summary>
        /// <param name="buttonId">Generic button identifier.</param>
        /// <returns>A scene button identifier with the same raw value.</returns>
        public static GameSceneButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
