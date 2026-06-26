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

        public static bool ShouldSchedulePostEatSleep(int targetCount, bool isNightLevel, bool usesFlashXmlAnimations)
        {
            return targetCount > 1 && !isNightLevel && usesFlashXmlAnimations;
        }
    }
}
