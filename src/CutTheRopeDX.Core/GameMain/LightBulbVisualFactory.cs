using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    internal static class LightBulbDefinition
    {
        public static CandyCapabilities Capabilities => CandyCapabilities.LightBulb;

        public static bool EmitsLight => true;

        public static float CollisionDistance => 2.25f * GameScene.STAR_RADIUS;
    }

    internal static class LightBulbVisualFactory
    {
        public static CandyContext Create(float lightRadius, ConstraintedPoint point, string bulbNumber)
        {
            LightBulb bulb = new(lightRadius, point, bulbNumber);

            CandyBody body = new(point, CandyBodyRole.Whole, visual: bulb, main: bulb);

            return new CandyContext(body)
            {
                candyNumber = null,
                lightBulbNumber = bulb.bulbNumber,
                Capabilities = LightBulbDefinition.Capabilities,
                lightRadius = lightRadius,
                emitsLight = LightBulbDefinition.EmitsLight,
                collisionDistanceOverride = LightBulbDefinition.CollisionDistance,
            };
        }
    }
}
