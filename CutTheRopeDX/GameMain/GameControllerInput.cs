namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure input routing decisions for the game controller.
    /// </summary>
    internal static class GameControllerInput
    {
        /// <summary>
        /// Returns whether the pause menu may be opened from gameplay.
        /// </summary>
        /// <param name="gameplayHudTouchable">Whether the gameplay HUD is accepting input.</param>
        /// <param name="outcomeTransitionActive">Whether a game win/loss transition is currently active.</param>
        /// <param name="restartDimActive">Whether a restart dim is playing.</param>
        /// <returns><see langword="true"/> when the pause menu may open.</returns>
        /// <remarks>
        /// Pausing mid-dim would have to suspend and resume a partly-finished restart. Refusing
        /// for the ~0.15s the dim lasts avoids that state entirely, and matches the loss-triggered
        /// restart, which is already unpausable via <paramref name="outcomeTransitionActive"/>.
        /// </remarks>
        public static bool CanPauseFromGameplay(bool gameplayHudTouchable, bool outcomeTransitionActive, bool restartDimActive)
        {
            return gameplayHudTouchable && !outcomeTransitionActive && !restartDimActive;
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
