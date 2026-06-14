namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Decides whether the legacy singleton target should play chewing during the win sequence.
    /// </summary>
    internal static class GameWinChewing
    {
        public static bool ShouldPlayPrimaryChewingOnGameWon(int targetCount)
        {
            return targetCount <= 1;
        }
    }
}
