using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Per-candy mouse ownership. Answers "is the active mouse carrying THIS candy" by physics-point
    /// identity, so multi-candy interactions gate on the candy the mouse actually holds rather than a
    /// global "mouse holds any candy" flag that would couple unrelated candies together.
    /// </summary>
    internal static class MouseOwnership
    {
        /// <summary>
        /// True when the point the active mouse is carrying is exactly <paramref name="candyPoint"/>.
        /// </summary>
        /// <param name="carriedByActiveMouse">The point the active mouse currently carries, or null.</param>
        /// <param name="candyPoint">The candy physics point being tested.</param>
        public static bool CarriesCandy(ConstraintedPoint carriedByActiveMouse, ConstraintedPoint candyPoint)
        {
            return carriedByActiveMouse != null && ReferenceEquals(carriedByActiveMouse, candyPoint);
        }
    }
}
