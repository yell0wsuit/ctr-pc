namespace CutTheRopeDX.GameMain
{
    /// <summary>Controller-owned presentation mode for gameplay overlays.</summary>
    internal enum GameControllerOverlayMode
    {
        /// <summary>The level accepts gameplay input and updates normally.</summary>
        Gameplay,

        /// <summary>The pause menu is visible and gameplay is suspended.</summary>
        Paused,

        /// <summary>The result flow owns the screen and gameplay is frozen.</summary>
        Results,
    }
}
