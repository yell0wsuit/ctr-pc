namespace CutTheRope.GameMain
{
    /// <summary>
    /// Receives win/lose notifications from the game scene.
    /// </summary>
    internal interface IGameSceneDelegate
    {
        /// <summary>Called when the player wins the level.</summary>
        void GameWon();

        /// <summary>Called when the player loses the level.</summary>
        void GameLost();
    }
}
