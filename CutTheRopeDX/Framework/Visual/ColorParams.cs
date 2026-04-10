namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Holds an RGBA color value used as a keyframe parameter in timeline tracks.
    /// </summary>
    internal sealed class ColorParams
    {
        /// <summary>
        /// Initializes a new <see cref="ColorParams"/> with a transparent black color.
        /// </summary>
        public ColorParams()
        {
            rgba = new RGBAColor(0f, 0f, 0f, 0f);
        }

        /// <summary>
        /// The RGBA color value.
        /// </summary>
        public RGBAColor rgba;
    }
}
