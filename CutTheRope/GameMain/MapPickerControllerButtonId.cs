using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Identifier set for the map picker view.
    /// </summary>
    internal readonly record struct MapPickerControllerButtonId(int Value) : IButtonIdentifier
    {
        /// <summary>
        /// Button that triggers loading the selected map list.
        /// </summary>
        public static MapPickerControllerButtonId Start => new(0);

        /// <summary>
        /// Implicitly wraps a raw value into a typed <see cref="MapPickerControllerButtonId"/>.
        /// </summary>
        /// <param name="value">Numeric identifier previously represented as an <see cref="int"/>.</param>
        public static implicit operator MapPickerControllerButtonId(int value)
        {
            return new(value);
        }

        /// <summary>
        /// Converts the typed identifier back into the shared <see cref="ButtonId"/> representation.
        /// </summary>
        /// <param name="buttonId">Typed identifier to wrap.</param>
        public static implicit operator ButtonId(MapPickerControllerButtonId buttonId)
        {
            return ButtonId.From(buttonId);
        }

        /// <summary>
        /// Converts the typed identifier into its raw numeric value.
        /// </summary>
        /// <param name="buttonId">Typed identifier to unwrap.</param>
        public static implicit operator int(MapPickerControllerButtonId buttonId)
        {
            return buttonId.Value;
        }

        /// <summary>
        /// Creates a <see cref="MapPickerControllerButtonId"/> from a generic <see cref="ButtonId"/>.
        /// </summary>
        /// <param name="buttonId">Shared identifier emitted by the button infrastructure.</param>
        /// <returns>Typed identifier specific to the map picker.</returns>
        public static MapPickerControllerButtonId FromButtonId(ButtonId buttonId)
        {
            return new(buttonId.Value);
        }
    }
}
