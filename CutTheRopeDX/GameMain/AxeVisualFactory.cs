using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal static class AxeDefinition
    {
        public static CandyCapabilities Capabilities => CandyCapabilities.Axe;

        public static bool EmitsLight => false;

        public const float ChainCutRadius = 64f;

        public const float HazardCollisionDistance = 1.35f * GameScene.STAR_RADIUS;
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
