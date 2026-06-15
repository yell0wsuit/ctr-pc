namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Guards terminal game outcomes so win and loss cannot both trigger for one level run.
    /// </summary>
    internal static class GameOutcomeTransition
    {
        /// <summary>
        /// Returns whether the win sequence can start for the current level run.
        /// </summary>
        /// <param name="gameWonTriggered">Whether the win sequence has already been triggered.</param>
        /// <param name="gameLostTriggered">Whether the loss sequence has already been triggered.</param>
        /// <returns><see langword="true"/> when neither terminal outcome has started; otherwise, <see langword="false"/>.</returns>
        public static bool CanTriggerWin(bool gameWonTriggered, bool gameLostTriggered)
        {
            return !gameWonTriggered && !gameLostTriggered;
        }

        /// <summary>
        /// Returns whether the loss sequence can start for the current level run.
        /// </summary>
        /// <param name="gameWonTriggered">Whether the win sequence has already been triggered.</param>
        /// <param name="gameLostTriggered">Whether the loss sequence has already been triggered.</param>
        /// <returns><see langword="true"/> when neither terminal outcome has started; otherwise, <see langword="false"/>.</returns>
        public static bool CanTriggerLoss(bool gameWonTriggered, bool gameLostTriggered)
        {
            return !gameWonTriggered && !gameLostTriggered;
        }
    }
}
