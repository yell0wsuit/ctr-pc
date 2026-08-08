namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Receives win/lose notifications from the game scene.
    /// </summary>
    internal interface IGameSceneDelegate
    {
        /// <summary>Called when the player wins the level.</summary>
        /// <param name="result">The completed level's immutable result.</param>
        void GameWon(LevelResult result);

        /// <summary>Called when the player loses the level.</summary>
        void GameLost();
    }
}
