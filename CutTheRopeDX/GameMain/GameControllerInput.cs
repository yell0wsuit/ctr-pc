namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure input routing decisions for the game controller.
    /// </summary>
    internal static class GameControllerInput
    {
        /// <summary>
        /// Returns whether gameplay input may open the pause menu.
        /// </summary>
        /// <param name="gameplayHudTouchable">Whether the in-game HUD button layer is accepting input.</param>
        /// <param name="outcomeTransitionActive">Whether a game win/loss transition is currently active.</param>
        /// <returns><see langword="true"/> when gameplay input is active and no outcome transition is running.</returns>
        public static bool CanPauseFromGameplay(bool gameplayHudTouchable, bool outcomeTransitionActive)
        {
            return gameplayHudTouchable && !outcomeTransitionActive;
        }

        /// <summary>
        /// Returns whether Back/Escape may leave the result screen.
        /// </summary>
        /// <param name="resultTouchable">Whether the result screen is accepting input.</param>
        /// <param name="outcomeTransitionActive">Whether a game win/loss transition is currently active.</param>
        /// <returns><see langword="true"/> when the result screen is stable enough to handle Back/Escape.</returns>
        public static bool CanExitResultWithBack(bool resultTouchable, bool outcomeTransitionActive)
        {
            return resultTouchable && !outcomeTransitionActive;
        }
    }
}
