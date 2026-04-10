namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Defines the OpenGL-style blend factors supported by the desktop renderer.
    /// </summary>
    /// <remarks>
    /// Values match the OpenGL constants defined by Khronos in <c>glcorearb.h</c>.
    /// Reference: https://registry.khronos.org/OpenGL/api/GL/glcorearb.h
    /// </remarks>
    public enum BlendingFactor
    {
        /// <summary>
        /// Uses a factor of zero.
        /// </summary>
        GLZERO,

        /// <summary>
        /// Uses a factor of one.
        /// </summary>
        GLONE,

        /// <summary>
        /// Uses the source color.
        /// </summary>
        GLSRCCOLOR = 768,

        /// <summary>
        /// Uses one minus the source color.
        /// </summary>
        GLONEMINUSSRCCOLOR,

        /// <summary>
        /// Uses the source alpha.
        /// </summary>
        GLSRCALPHA,

        /// <summary>
        /// Uses one minus the source alpha.
        /// </summary>
        GLONEMINUSSRCALPHA,

        /// <summary>
        /// Uses the destination alpha.
        /// </summary>
        GLDSTALPHA,

        /// <summary>
        /// Uses one minus the destination alpha.
        /// </summary>
        GLONEMINUSDSTALPHA,

        /// <summary>
        /// Uses the destination color.
        /// </summary>
        GLDSTCOLOR,

        /// <summary>
        /// Uses one minus the destination color.
        /// </summary>
        GLONEMINUSDSTCOLOR,

        /// <summary>
        /// Uses saturated source alpha.
        /// </summary>
        GLSRCALPHASATURATE
    }
}
