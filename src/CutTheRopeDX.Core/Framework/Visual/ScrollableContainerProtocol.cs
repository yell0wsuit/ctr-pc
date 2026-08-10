namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Receives scroll position callbacks from a <see cref="ScrollableContainer"/>.
    /// </summary>
    internal interface IScrollableContainerProtocol
    {
        /// <summary>
        /// Called when the container has settled at scroll point <paramref name="i"/>.
        /// </summary>
        /// <param name="e">Scrollable container that reached the point.</param>
        /// <param name="i">Scroll point index.</param>
        void ScrollableContainerreachedScrollPoint(ScrollableContainer e, int i);

        /// <summary>
        /// Called when the target scroll point changes to <paramref name="i"/>.
        /// </summary>
        /// <param name="e">Scrollable container whose target changed.</param>
        /// <param name="i">New target scroll point index.</param>
        void ScrollableContainerchangedTargetScrollPoint(ScrollableContainer e, int i);
    }
}
