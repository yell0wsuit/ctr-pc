namespace CutTheRopeDX.GameMain
{
    internal sealed record CandyCapabilities(
        bool CanCollectStars = true,
        bool CanOpenMouth = true,
        bool CanBeEaten = true,
        bool CanLoseLevelWhenOffScreen = true,
        bool CanBeGrabbedBySpider = true,
        bool CanBeGrabbedByMouse = true,
        bool CanBeGrabbedByHand = true,
        bool CanEnterLantern = true,
        bool CanEnterTransport = true,
        bool CanBindRocket = true,
        bool CanAttachAnts = true,
        bool CanBeBrokenByHazards = true)
    {
        public static CandyCapabilities Candy { get; } = new();

        public static CandyCapabilities LightBulb { get; } = new(
            CanCollectStars: false,
            CanOpenMouth: false,
            CanBeEaten: false,
            CanLoseLevelWhenOffScreen: false,
            CanBeGrabbedBySpider: false,
            CanBeGrabbedByMouse: false,
            CanBeGrabbedByHand: false,
            CanEnterLantern: false,
            CanBindRocket: false,
            CanAttachAnts: false,
            CanBeBrokenByHazards: false);
    }
}
