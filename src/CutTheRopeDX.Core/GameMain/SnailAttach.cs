namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Gate for an inactive snail attaching to a candy. Judged per candy so a multi-candy level skips
    /// a gone or non-draggable body and lets the snail ride the next candy it overlaps.
    /// </summary>
    internal static class SnailAttach
    {
        /// <summary>
        /// True when the snail should attach: the candy is still in play, opts into snail dragging,
        /// and the snail overlaps it.
        /// </summary>
        /// <param name="candyGone">True when the candy has been consumed or otherwise removed.</param>
        /// <param name="canBeDraggedBySnail">True when the body opts into snail dragging.</param>
        /// <param name="snailIntersectsCandy">True when the snail overlaps the candy.</param>
        public static bool ShouldAttach(bool candyGone, bool canBeDraggedBySnail, bool snailIntersectsCandy)
        {
            return !candyGone && canBeDraggedBySnail && snailIntersectsCandy;
        }
    }
}
