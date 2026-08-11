namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Decides whether an Om Nom should sleep after eating while other targets remain.
    /// </summary>
    internal static class GameWinChewing
    {
        public static bool ShouldSchedulePostEatSleep(int targetCount, bool isNightLevel, bool usesFlashXmlAnimations)
        {
            return targetCount > 1 && !isNightLevel && usesFlashXmlAnimations;
        }
    }
}
