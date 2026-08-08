namespace CutTheRopeDX.GameMain
{
    /// <summary>Platform or HUD input that may affect the controller overlay.</summary>
    internal enum GameControllerInputKind
    {
        /// <summary>Back or Escape input.</summary>
        Back,

        /// <summary>Platform Menu input.</summary>
        Menu,

        /// <summary>Gameplay HUD pause button.</summary>
        PauseButton,
    }

    /// <summary>Semantic action selected by the controller input policy.</summary>
    internal enum GameControllerInputCommand
    {
        /// <summary>Leave controller state unchanged.</summary>
        Ignore,

        /// <summary>Open the pause overlay.</summary>
        OpenPause,

        /// <summary>Resume gameplay from pause.</summary>
        Resume,

        /// <summary>Leave a stable result screen.</summary>
        ExitResults,
    }

    /// <summary>
    /// Pure input routing decisions for the game controller.
    /// </summary>
    internal static class GameControllerInput
    {
        /// <summary>Resolves controller input into a semantic command.</summary>
        /// <param name="input">Input source.</param>
        /// <param name="overlay">Current controller overlay.</param>
        /// <param name="restartPhase">Authoritative restart phase.</param>
        /// <param name="outcomeTransitionActive">Whether a win/loss transition is active.</param>
        /// <returns>The semantic command allowed by the current state.</returns>
        public static GameControllerInputCommand Resolve(
            GameControllerInputKind input,
            GameControllerOverlayMode overlay,
            RestartPhase restartPhase,
            bool outcomeTransitionActive)
        {
            return restartPhase != RestartPhase.Playing || outcomeTransitionActive
                ? GameControllerInputCommand.Ignore
                : (overlay, input) switch
                {
                    (GameControllerOverlayMode.Gameplay, _) => GameControllerInputCommand.OpenPause,
                    (GameControllerOverlayMode.Paused, GameControllerInputKind.Back or GameControllerInputKind.Menu) => GameControllerInputCommand.Resume,
                    (GameControllerOverlayMode.Results, GameControllerInputKind.Back) => GameControllerInputCommand.ExitResults,
                    _ => GameControllerInputCommand.Ignore,
                };
        }

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
