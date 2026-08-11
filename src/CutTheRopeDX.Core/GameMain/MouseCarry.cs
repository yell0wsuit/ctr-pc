using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>The complete candy payload owned by one mouse.</summary>
    internal sealed class MouseCarry(ConstraintedPoint star, GameObject candy)
    {
        /// <summary>Gets the carried candy's physics point.</summary>
        public ConstraintedPoint Star { get; } = star;

        /// <summary>Gets the carried candy's visual object.</summary>
        public GameObject Candy { get; } = candy;
    }
}
