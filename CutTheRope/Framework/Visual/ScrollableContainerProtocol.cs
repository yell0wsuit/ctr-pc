namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Receives scroll position callbacks from a <see cref="ScrollableContainer"/>.
    /// </summary>
    internal interface IScrollableContainerProtocol
    {
        /// <summary>Called when the container has settled at scroll point <paramref name="i"/>.</summary>
        void ScrollableContainerreachedScrollPoint(ScrollableContainer e, int i);

        /// <summary>Called when the target scroll point changes to <paramref name="i"/>.</summary>
        void ScrollableContainerchangedTargetScrollPoint(ScrollableContainer e, int i);
    }
}
