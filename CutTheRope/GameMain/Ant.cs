using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// A single ant instance marching along an <see cref="AntsPath"/>.
    /// </summary>
    internal sealed class Ant
    {
        /// <summary>The sprite animation used to render this ant.</summary>
        public Animation animation;

        /// <summary>Current position along the path in world units, measured from the path start.</summary>
        public float offset;

        /// <summary>Per-instance scale multiplier, randomised in [0.8, 1.2] at spawn for visual variety.</summary>
        public float baseScale;
    }
}
