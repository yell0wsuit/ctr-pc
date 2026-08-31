using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Tuning for the Time Travel bomb, converted from the original's own world units.
    /// </summary>
    /// <remarks>
    /// Time Travel doubles every authored map coordinate as it loads, so its world is 640x960 for
    /// the 320x480 grid the maps are drawn on. DX scales the same grid by <c>mapScale = 3</c>, so
    /// its world is 960x1440 and every distance and speed read out of the original has to be
    /// stretched by <see cref="TimeTravelToWorldScale"/> to cover the same part of a level.
    /// The axe's constants predate this and are still carried raw, so they reach about a third
    /// less far than the original's.
    /// </remarks>
    internal static class BombDefinition
    {
        /// <summary>Time Travel world units to DX world units: 3 (DX map scale) over 2 (theirs).</summary>
        public const float TimeTravelToWorldScale = 1.5f;

        public static CandyCapabilities Capabilities => CandyCapabilities.Bomb;

        public static bool EmitsLight => false;

        /// <summary>Center distance at which a candy or an axe sets a bomb off (theirs: 72).</summary>
        public const float ContactTriggerDistance = 72f * TimeTravelToWorldScale;

        /// <summary>Center distance at which two live bombs set each other off (theirs: 80).</summary>
        public const float BombPairTriggerDistance = 80f * TimeTravelToWorldScale;

        /// <summary>Radius over which the blast pushes candies, axes, and other bombs (theirs: 400).</summary>
        public const float BlastRadius = 400f * TimeTravelToWorldScale;

        /// <summary>
        /// Impulse applied at the blast center, in world units per second (theirs: 1200); it falls
        /// off linearly to zero at the radius.
        /// </summary>
        public const float BlastImpulse = 1200f * TimeTravelToWorldScale;

        /// <summary>Half-extent of the square a cut stroke must cross to detonate a bomb (theirs: 10).</summary>
        public const float SwipeHalfExtent = 10f * TimeTravelToWorldScale;

        /// <summary>Delay between the explosion animation and the debris burst that removes the bomb.</summary>
        public const float DebrisDelay = 0.1f;

        /// <summary>Number of debris fragments the burst emits.</summary>
        public const int DebrisParticleCount = 7;

        /// <summary>
        /// Impulse the blast applies to a body at <paramref name="distance"/> from the center,
        /// falling off linearly to zero at <see cref="BlastRadius"/>.
        /// </summary>
        /// <param name="offset">Vector from the blast center to the body.</param>
        /// <param name="distance">Length of <paramref name="offset"/>; must be greater than zero.</param>
        /// <returns>The impulse to apply to the body.</returns>
        public static Vector BlastImpulseFor(Vector offset, float distance)
        {
            float falloff = (BlastRadius - distance) / BlastRadius;
            float magnitude = BlastImpulse * falloff / distance;
            return new Vector(offset.X * magnitude, offset.Y * magnitude);
        }
    }

    internal static class BombVisualFactory
    {
        public static CandyContext Create(ConstraintedPoint point, string bombNumber)
        {
            Bomb bomb = new(point, bombNumber);

            CandyBody body = new(
                point,
                CandyBodyRole.Whole,
                visual: bomb,
                main: bomb,
                bubbleAnimation: bomb.bubbleAnimation,
                ghostBubbleAnimation: bomb.ghostBubbleAnimation);

            return new CandyContext(body)
            {
                candyNumber = null,
                bombNumber = bomb.bombNumber,
                bomb = bomb,
                Capabilities = BombDefinition.Capabilities,
                emitsLight = BombDefinition.EmitsLight,
                collisionDistanceOverride = BombDefinition.ContactTriggerDistance,
            };
        }
    }
}
