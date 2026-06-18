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
        /// Returns whether candy may still be eaten by an Om Nom. Once a win or loss transition is
        /// active, remaining candy must not be consumed (e.g. a sad Om Nom must not eat during the
        /// loss reaction).
        /// </summary>
        /// <param name="outcomeTransitionActive">Whether a game win/loss transition is currently active.</param>
        /// <returns><see langword="true"/> while no terminal outcome transition is active; otherwise, <see langword="false"/>.</returns>
        public static bool CanConsumeCandy(bool outcomeTransitionActive)
        {
            return !outcomeTransitionActive;
        }
    }
}
