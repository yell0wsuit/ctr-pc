namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Guards terminal game outcomes so win and loss cannot both trigger for one level run.
    /// </summary>
    internal static class GameOutcomeTransition
    {
        public static bool CanTriggerWin(bool gameWonTriggered, bool gameLostTriggered)
        {
            return !gameWonTriggered && !gameLostTriggered;
        }

        public static bool CanTriggerLoss(bool gameWonTriggered, bool gameLostTriggered)
        {
            return !gameWonTriggered && !gameLostTriggered;
        }
    }
}
