using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal static class AxeDefinition
    {
        /// <summary>Time Travel world units to DX world units: 3 (DX map scale) over 2 (theirs).</summary>
        public const float TimeTravelToWorldScale = 1.5f;

        public static CandyCapabilities Capabilities => CandyCapabilities.Axe;

        public static bool EmitsLight => false;

        /// <summary>Blade-to-chain reach (Time Travel: 64 in its 2x world).</summary>
        public const float ChainCutRadius = 64f * TimeTravelToWorldScale;

        /// <summary>
        /// Blade-to-candy reach that destroys the candy (Time Travel: 64 in its 2x world, the same
        /// distance it cuts chains at). The original pushes the candy apart instead of breaking it
        /// while a superpower is running, and never breaks a disco candy; DX has neither.
        /// </summary>
        public const float HazardCollisionDistance = 64f * TimeTravelToWorldScale;
    }

    internal static class AxeVisualFactory
    {
        public static CandyContext Create(ConstraintedPoint point, string axeNumber)
        {
            Axe axe = new(point, axeNumber);

            CandyBody body = new(
                point,
                CandyBodyRole.Whole,
                visual: axe,
                main: axe,
                bubbleAnimation: axe.bubbleAnimation,
                ghostBubbleAnimation: axe.ghostBubbleAnimation);

            return new CandyContext(body)
            {
                candyNumber = null,
                axeNumber = axe.axeNumber,
                axe = axe,
                Capabilities = AxeDefinition.Capabilities,
                emitsLight = AxeDefinition.EmitsLight,
                collisionDistanceOverride = AxeDefinition.HazardCollisionDistance,
            };
        }
    }
}
