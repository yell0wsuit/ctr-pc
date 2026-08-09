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
        /// <param name="resultExitAllowed">Whether Back may leave a stable result screen.</param>
        /// <returns>The semantic command allowed by the current state.</returns>
        /// <remarks>
        /// An outcome presentation does not gate input. Pausing mid-cutscene is safe because the
        /// pause overlay clears <c>updateable</c>, and the scene's update is the only thing that
        /// pumps the delayed dispatcher, so the win/loss sequence freezes with it.
        /// </remarks>
        public static GameControllerInputCommand Resolve(
            GameControllerInputKind input,
            GameControllerOverlayMode overlay,
            RestartPhase restartPhase,
            bool resultExitAllowed)
        {
            return restartPhase != RestartPhase.Playing
                ? GameControllerInputCommand.Ignore
                : (overlay, input) switch
                {
                    (GameControllerOverlayMode.Gameplay, _) => GameControllerInputCommand.OpenPause,
                    (GameControllerOverlayMode.Paused, GameControllerInputKind.Back or GameControllerInputKind.Menu) => GameControllerInputCommand.Resume,
                    (GameControllerOverlayMode.Results, GameControllerInputKind.Back) when resultExitAllowed => GameControllerInputCommand.ExitResults,
                    _ => GameControllerInputCommand.Ignore,
                };
        }

    }
}
