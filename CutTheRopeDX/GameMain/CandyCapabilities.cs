namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Per-object opt-in/opt-out flags for the candy-like interactions a physics body supports.
    /// Travels on a <see cref="CandyContext"/> so the shared candy code can gate each interaction
    /// without asking what kind of object it is holding. Defaults describe a normal candy; use the
    /// <see cref="Candy"/>, <see cref="LightBulb"/>, and <see cref="Axe"/> presets rather than constructing ad hoc.
    /// </summary>
    /// <param name="CanCollectStars">Whether touching a star collects it.</param>
    /// <param name="CanOpenMouth">Whether Om Nom opens his mouth when this body is in range.</param>
    /// <param name="CanBeEaten">Whether Om Nom can eat this body; only edible bodies count toward the win condition.</param>
    /// <param name="CanLoseLevelWhenOffScreen">
    /// Whether this body loses the level by leaving the screen on its own. This governs only the
    /// per-object off-screen loss; aggregate rules (e.g. night-level "all light emitters lost") are
    /// separate and not driven by this flag.
    /// </param>
    /// <param name="CanBeGrabbedBySpider">Whether a spider can climb its rope and steal this body.</param>
    /// <param name="CanBeGrabbedByMouse">Whether a mouse can carry this body away.</param>
    /// <param name="CanBeGrabbedByHand">Whether a mechanical hand can grab and hold this body.</param>
    /// <param name="CanEnterLantern">Whether a lantern can capture this body.</param>
    /// <param name="CanEnterTransport">Whether this body can travel through hats/socks and bamboo tubes.</param>
    /// <param name="CanBindRocket">Whether a rocket can bind to and fly this body.</param>
    /// <param name="CanAttachAnts">Whether ants can attach to and carry this body along their path.</param>
    /// <param name="CanCollideWithCandyBodies">Whether this body participates in elastic candy-body separation.</param>
    /// <param name="CanBeBrokenByHazards">Whether hazards (spikes) destroy this body on contact.</param>
    /// <param name="CanFloatInWater">Whether water buoyancy pushes this body upward when submerged.</param>
    /// <param name="CanBeDraggedBySnail">Whether a snail can crawl onto this body and weigh it down.</param>
    /// <param name="CanRotateWithRopes">Whether rope swing logic rotates this body's visual.</param>
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
        bool CanCollideWithCandyBodies = true,
        bool CanBeBrokenByHazards = true,
        bool CanFloatInWater = true,
        bool CanBeDraggedBySnail = true,
        bool CanRotateWithRopes = true)
    {
        /// <summary>A normal candy: every candy-like interaction is enabled.</summary>
        public static CandyCapabilities Candy { get; } = new();

        /// <summary>
        /// A light bulb: a physical, transportable body that is not edible, collectible, grabbable,
        /// or destructible. It still collides, rides ropes, and enters bubbles/socks (it keeps
        /// <see cref="CanEnterTransport"/>), but is excluded from every consumption/grab interaction.
        /// </summary>
        public static CandyCapabilities LightBulb { get; } = new(
            CanCollectStars: false,
            CanOpenMouth: false,
            CanBeEaten: false,
            CanLoseLevelWhenOffScreen: false,
            CanBeGrabbedBySpider: false,
            CanEnterLantern: false,
            CanBeBrokenByHazards: false,
            CanRotateWithRopes: false);

        /// <summary>
        /// An axe blade: a physical, rocket/transport-capable body that behaves like a hazard instead
        /// of candy. It does not collect stars, emit win/loss candy signals, or get destroyed by hazards.
        /// </summary>
        public static CandyCapabilities Axe { get; } = new(
            CanCollectStars: false,
            CanOpenMouth: false,
            CanBeEaten: false,
            CanLoseLevelWhenOffScreen: false,
            CanBeGrabbedBySpider: false,
            CanBeGrabbedByMouse: false,
            CanBeGrabbedByHand: false,
            CanEnterLantern: false,
            CanAttachAnts: false,
            CanCollideWithCandyBodies: false,
            CanBeBrokenByHazards: false,
            CanFloatInWater: false,
            CanBeDraggedBySnail: false);
    }
}
