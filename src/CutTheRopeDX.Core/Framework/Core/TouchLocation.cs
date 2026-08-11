using System.Numerics;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Mirror of XNA's touch states, Core-owned. Numeric values MUST match
    /// XNA's TouchLocationState so behavior cannot drift on comparisons.
    /// </summary>
    internal enum TouchLocationState { Invalid = 0, Moved = 1, Pressed = 2, Released = 3 }

    /// <summary>One touch point. Mirrors the XNA members the game uses.</summary>
    internal readonly struct TouchLocation(int id, TouchLocationState state, Vector2 position)
    {
        public int Id { get; } = id;
        public TouchLocationState State { get; } = state;
        public Vector2 Position { get; } = position;
    }
}
