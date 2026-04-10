namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Holds all possible parameter types for a keyframe (action, position, scale, rotation, skew, color).
    /// </summary>
    internal sealed class KeyFrameValue
    {
        /// <summary>
        /// Initializes a new <see cref="KeyFrameValue"/> with default parameter instances.
        /// </summary>
        public KeyFrameValue()
        {
            action = new ActionParams();
            scale = new ScaleParams();
            pos = new PosParams();
            rotation = new RotationParams();
            skew = new SkewParams();
            color = new ColorParams();
        }

        /// <summary>
        /// Position parameters.
        /// </summary>
        public PosParams pos;

        /// <summary>
        /// Scale parameters.
        /// </summary>
        public ScaleParams scale;

        /// <summary>
        /// Rotation parameters.
        /// </summary>
        public RotationParams rotation;

        /// <summary>
        /// Skew parameters.
        /// </summary>
        public SkewParams skew;

        /// <summary>
        /// Color parameters.
        /// </summary>
        public ColorParams color;

        /// <summary>
        /// Action parameters.
        /// </summary>
        public ActionParams action;
    }
}
