namespace CutTheRope.GameMain
{
    /// <summary>
    /// Callback interface for receiving rocket lifecycle events.
    /// </summary>
    internal interface IRocketDelegate
    {
        /// <summary>
        /// Called when a rocket has exhausted its fuel and finished its scale-down animation.
        /// </summary>
        /// <param name="rocket">The rocket that has been exhausted.</param>
        void Exhausted(Rocket rocket);
    }
}
