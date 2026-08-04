using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Decides which ropes are cut when a grab wraps around a conveyor edge. The candy is identified by
    /// the rope's tail (physics point) rather than <see cref="Grab.candyNumber"/>: the multi-candy loader
    /// assigns every candy-bound grab the number 0, so matching on it would cut ropes on other candies.
    /// </summary>
    internal static class ConveyorRopeCut
    {
        /// <summary>
        /// True when a sibling rope should be cut: it hangs from the same candy point as the wrapped
        /// grab, it is not the wrapped grab's own rope, and it is still uncut.
        /// </summary>
        /// <param name="ropeTail">Tail point of the candidate rope, or null when the grab has no rope.</param>
        /// <param name="wrappedCandyPoint">Candy point of the grab that wrapped.</param>
        /// <param name="isWrappedGrab">True when the candidate is the wrapped grab itself.</param>
        /// <param name="ropeUncut">True when the candidate rope is still uncut.</param>
        public static bool ShouldCut(
            ConstraintedPoint ropeTail,
            ConstraintedPoint wrappedCandyPoint,
            bool isWrappedGrab,
            bool ropeUncut)
        {
            return !isWrappedGrab
                && ropeUncut
                && ropeTail != null
                && ReferenceEquals(ropeTail, wrappedCandyPoint);
        }
    }
}
