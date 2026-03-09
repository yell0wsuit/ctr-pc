namespace CutTheRope.GameMain
{
    internal static class FlashXmlTargetTimelineRules
    {
        public const int IdleLoopTimeline = 0;
        public const int IdleVariationOneTimeline = 1;
        public const int IdleVariationTwoTimeline = 17;
        public const int ExcitedTimeline = 2;
        public const int MouthOpeningTimeline = 3;
        public const int MouthClosingTimeline = 4;
        public const int PuzzledTimeline = 5;
        public const int SadTimeline = 6;
        public const int ChewingTimeline = 7;
        public const int PostChewTimeline = 8;
        public const int SleepingTimeline = 9;
        public const int GreetingTimeline = 18;
        public const int IdleVariationThreeTimeline = 12;

        public static bool ShouldBindFollowupDelegate(int timelineId)
        {
            return TryGetFollowupTimeline(timelineId, out _);
        }

        public static bool TryGetFollowupTimeline(int finishedTimelineId, out int followupTimelineId)
        {
            followupTimelineId = finishedTimelineId switch
            {
                IdleVariationOneTimeline => IdleLoopTimeline,
                ExcitedTimeline => IdleLoopTimeline,
                MouthClosingTimeline => PuzzledTimeline,
                PuzzledTimeline => IdleLoopTimeline,
                IdleVariationTwoTimeline => IdleLoopTimeline,
                IdleVariationThreeTimeline => IdleLoopTimeline,
                ChewingTimeline => ChewingTimeline,
                GreetingTimeline => IdleLoopTimeline,
                _ => -1
            };

            return followupTimelineId >= 0;
        }
    }
}
