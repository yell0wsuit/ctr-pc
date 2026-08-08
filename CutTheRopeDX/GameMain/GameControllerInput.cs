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

    }
}
