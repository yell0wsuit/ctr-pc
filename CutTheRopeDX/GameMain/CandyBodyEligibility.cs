namespace CutTheRopeDX.GameMain
{
    /// <summary>A scene system that acts on candy bodies.</summary>
    internal enum CandyInteraction
    {
        /// <summary>Verlet integration and the visual that follows the body.</summary>
        Physics,

        /// <summary>Water splash, the underwater achievement, and buoyancy.</summary>
        Water,

        /// <summary>Pump flow impulses.</summary>
        Pump,

        /// <summary>Steam-tube lift.</summary>
        Steam,

        /// <summary>Free-bubble capture, bubble lift, and bubble popping.</summary>
        Bubble,

        /// <summary>Radius auto-hook attachment and rope-driven rotation.</summary>
        Rope,

        /// <summary>Collectible-star pickup.</summary>
        Star,

        /// <summary>Destruction by spikes and other hazards.</summary>
        Hazard,

        /// <summary>Bouncer impulses.</summary>
        Bouncer,

        /// <summary>Spider theft along a rope.</summary>
        Spider,

        /// <summary>Leaving the playable area.</summary>
        OffScreen,

        /// <summary>Elastic overlap against a light emitter.</summary>
        LightCollision,

        /// <summary>Elastic overlap between two logical candies.</summary>
        CandyCollision,

        /// <summary>Being carried by a mouse.</summary>
        Mouse,

        /// <summary>Being captured in a lantern.</summary>
        Lantern,

        /// <summary>Being bound to and flown by a rocket.</summary>
        Rocket,

        /// <summary>Riding an ant conveyor.</summary>
        Ants,

        /// <summary>Entering a bamboo tube or magic hat.</summary>
        Transport,

        /// <summary>Being held by a mechanical hand.</summary>
        Hand,

        /// <summary>Being weighed down by a snail.</summary>
        Snail,

        /// <summary>Opening a target's mouth and being eaten by it.</summary>
        Eat,
    }

    /// <summary>
    /// Decides which scene systems may act on a body, given the <see cref="CandyBodyRole"/> it plays
    /// inside its logical candy. Physical systems treat every body alike; the carrier and outcome
    /// systems only ever see a whole candy, because a split half has to merge before anything can
    /// carry, transport, or eat it.
    /// </summary>
    internal static class CandyBodyEligibility
    {
        /// <summary>Determines whether <paramref name="interaction"/> may act on a <paramref name="role"/> body.</summary>
        /// <param name="role">The body's role within its logical candy.</param>
        /// <param name="interaction">The scene system asking to act on the body.</param>
        /// <returns>
        /// <see langword="true"/> for every interaction on a <see cref="CandyBodyRole.Whole"/> body,
        /// and for the purely physical interactions on a split half; otherwise <see langword="false"/>.
        /// </returns>
        public static bool Allows(CandyBodyRole role, CandyInteraction interaction)
        {
            return role == CandyBodyRole.Whole || interaction is not (
                CandyInteraction.CandyCollision
                or CandyInteraction.Mouse
                or CandyInteraction.Lantern
                or CandyInteraction.Rocket
                or CandyInteraction.Ants
                or CandyInteraction.Transport
                or CandyInteraction.Hand
                or CandyInteraction.Snail
                or CandyInteraction.Eat);
        }
    }
}
