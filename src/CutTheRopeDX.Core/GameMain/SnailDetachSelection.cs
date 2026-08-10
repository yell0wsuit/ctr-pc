using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Selects which snails detach when a candy leaves play. A snail detaches only while it is actively
    /// riding and its attached physics point is the candy being removed, so per-candy removals stay
    /// isolated — eating one candy does not drop a snail riding another.
    /// </summary>
    internal static class SnailDetachSelection
    {
        /// <summary>
        /// True when a snail should detach from <paramref name="targetPoint"/>: it is active and its
        /// attached point is exactly that candy point.
        /// </summary>
        /// <param name="snailActive">True when the snail is in its active (riding) state.</param>
        /// <param name="snailAttachedPoint">The point the snail currently rides, or null.</param>
        /// <param name="targetPoint">The candy point being removed.</param>
        public static bool ShouldDetach(
            bool snailActive,
            ConstraintedPoint snailAttachedPoint,
            ConstraintedPoint targetPoint)
        {
            return snailActive
                && snailAttachedPoint != null
                && ReferenceEquals(snailAttachedPoint, targetPoint);
        }
    }
}
