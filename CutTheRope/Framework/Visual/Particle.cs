using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// State data for a single particle in a <see cref="Particles"/> system.
    /// </summary>
    internal struct Particle
    {
        /// <summary>
        /// Position at spawn time.
        /// </summary>
        public Vector startPos;

        /// <summary>
        /// Current position relative to the emitter.
        /// </summary>
        public Vector pos;

        /// <summary>
        /// Current velocity direction and magnitude.
        /// </summary>
        public Vector dir;

        /// <summary>
        /// Radial acceleration away from the emitter center.
        /// </summary>
        public float radialAccel;

        /// <summary>
        /// Tangential acceleration perpendicular to the radial direction.
        /// </summary>
        public float tangentialAccel;

        /// <summary>
        /// Current color.
        /// </summary>
        public RGBAColor color;

        /// <summary>
        /// Per-second color change rate.
        /// </summary>
        public RGBAColor deltaColor;

        /// <summary>
        /// Current particle size.
        /// </summary>
        public float size;

        /// <summary>
        /// Per-second size change rate.
        /// </summary>
        public float deltaSize;

        /// <summary>
        /// Remaining lifetime in seconds.
        /// </summary>
        public float life;

        /// <summary>
        /// Per-second rotation change in degrees.
        /// </summary>
        public float deltaAngle;

        /// <summary>
        /// Current rotation angle in degrees.
        /// </summary>
        public float angle;

        /// <summary>
        /// Particle width.
        /// </summary>
        public float width;

        /// <summary>
        /// Particle height.
        /// </summary>
        public float height;
    }
}
