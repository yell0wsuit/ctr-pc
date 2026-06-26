namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Guards terminal game outcomes so win and loss cannot both trigger for one level run.
    /// </summary>
    internal static class GameOutcomeTransition
    {
        /// <summary>
        /// Returns whether any terminal outcome sequence can start for the current level run.
        /// </summary>
        /// <param name="gameWonTriggered">Whether the win sequence has already been triggered.</param>
        /// <param name="gameLostTriggered">Whether the loss sequence has already been triggered.</param>
        /// <returns><see langword="true"/> when neither terminal outcome has started; otherwise, <see langword="false"/>.</returns>
        public static bool CanTriggerTerminalOutcome(bool gameWonTriggered, bool gameLostTriggered)
        {
            return !gameWonTriggered && !gameLostTriggered;
        }

        /// <summary>
        /// Returns whether Om Nom may react to candy or light-driven gameplay. Once a win/loss
        /// transition is active, or this Om Nom has already eaten, gameplay reactions must not
        /// replace the current target animation.
        /// </summary>
        /// <param name="outcomeTransitionActive">Whether a game win/loss transition is currently active.</param>
        /// <param name="targetAlreadyFed">Whether this Om Nom has already eaten a candy.</param>
        /// <returns><see langword="true"/> while no terminal outcome transition is active; otherwise, <see langword="false"/>.</returns>
        public static bool CanReactToCandyOrLight(bool outcomeTransitionActive, bool targetAlreadyFed = false)
        {
            return !outcomeTransitionActive && !targetAlreadyFed;
        }
    }
}
