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

            return new CandyContext
            {
                candyNumber = null,
                lightBulbNumber = bulb.bulbNumber,
                point = point,
                candy = bulb,
                candyMain = bulb,
                lightBulb = bulb,
                Capabilities = LightBulbDefinition.Capabilities,
                lightRadius = lightRadius,
                emitsLight = LightBulbDefinition.EmitsLight,
                collisionDistanceOverride = LightBulbDefinition.CollisionDistance,
                noCandy = false,
            };
        }
    }
}
