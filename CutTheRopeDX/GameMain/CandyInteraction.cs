namespace CutTheRopeDX.GameMain
{
    internal static class CandyInteraction
    {
        public static bool CanCollectStar(CandyContext ctx)
        {
            return ctx != null && !ctx.noCandy && ctx.Capabilities.CanCollectStars;
        }

        public static bool CanBeGrabbedByHand(CandyContext ctx)
        {
            return ctx != null && !ctx.noCandy && ctx.Capabilities.CanBeGrabbedByHand;
        }

        public static bool CanAttachAnts(CandyContext ctx)
        {
            return ctx != null && !ctx.noCandy && ctx.Capabilities.CanAttachAnts;
        }

        public static bool CanBeBrokenByHazards(CandyContext ctx)
        {
            return ctx != null && !ctx.noCandy && ctx.Capabilities.CanBeBrokenByHazards;
        }
    }
}
