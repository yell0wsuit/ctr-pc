namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Shared contract for identifiers that can be routed through button presses.
    /// </summary>
    public interface IButtonIdentifier
    {
        /// <summary>
        /// Numeric representation of the identifier.
        /// </summary>
        int Value { get; }
    }

    /// <summary>
    /// Strongly typed identifier passed alongside button press events.
    /// </summary>
    public readonly record struct ButtonId(int Value) : IButtonIdentifier
    {
        /// <summary>
        /// Implicitly creates a <see cref="ButtonId"/> from a raw numeric value.
        /// </summary>
        /// <param name="value">Button identifier previously represented as an <see cref="int"/>.</param>
        public static implicit operator ButtonId(int value)
        {
            return new(value);
        }

        /// <summary>
        /// Implicitly converts a <see cref="ButtonId"/> into its numeric representation.
        /// </summary>
        /// <param name="id">The button identifier to unwrap.</param>
        public static implicit operator int(ButtonId id)
        {
            return id.Value;
        }

        /// <summary>
        /// Creates a <see cref="ButtonId"/> from a typed identifier wrapper.
        /// </summary>
        /// <param name="identifier">Source identifier that exposes a numeric value.</param>
        /// <returns>Wrapped identifier usable by the generic button infrastructure.</returns>
        public static ButtonId From(IButtonIdentifier identifier)
        {
            return new(identifier.Value);
        }
    }

    public interface IButtonDelegation
    {
        /// <summary>
        /// Invoked when the button is pressed.
        /// </summary>
        /// <param name="buttonId">Typed button identifier.</param>
        void OnButtonPressed(ButtonId buttonId);
    }
}
