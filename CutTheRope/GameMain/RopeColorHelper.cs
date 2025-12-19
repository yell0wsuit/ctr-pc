using CutTheRope.Framework;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Helper class for managing rope color schemes.
    /// </summary>
    internal static class RopeColorHelper
    {
        /// <summary>
        /// Represents a rope color scheme with two base colors.
        /// </summary>
        public readonly record struct RopeColors(
            RGBAColor Color1,
            RGBAColor Color2
        );

        /// <summary>
        /// Gets the rope color scheme for a given rope index (0-8).
        /// </summary>
        public static RopeColors GetRopeColors(int ropeIndex)
        {
            return ropeIndex switch
            {
                0 => new RopeColors(
                    RGBAColor.MakeRGBA(0.475, 0.305, 0.185, 1.0),  // Original brown (default)
                    RGBAColor.MakeRGBA(0.6755555555555556, 0.44, 0.27555555555555555, 1.0)
                ),
                1 => new RopeColors(
                    RGBAColor.MakeRGBA(0.624, 0.294, 0.114, 1.0),  // Brown
                    RGBAColor.MakeRGBA(1.000, 0.627, 0.463, 1.0)   // Light brown
                ),
                2 => new RopeColors(
                    RGBAColor.MakeRGBA(0.404, 0.612, 0.635, 1.0),  // Teal
                    RGBAColor.MakeRGBA(0.773, 0.898, 0.902, 1.0)   // Light teal
                ),
                3 => new RopeColors(
                    RGBAColor.MakeRGBA(0.757, 0.533, 0.000, 1.0),  // Gold
                    RGBAColor.MakeRGBA(0.980, 0.843, 0.200, 1.0)   // Light gold
                ),
                4 => new RopeColors(
                    RGBAColor.MakeRGBA(0.980, 0.243, 0.243, 1.0),  // Red
                    RGBAColor.MakeRGBA(0.282, 0.525, 0.153, 1.0)   // Green
                ),
                5 => new RopeColors(
                    RGBAColor.MakeRGBA(0.176, 0.318, 0.659, 1.0),  // Blue
                    RGBAColor.MakeRGBA(1.000, 1.000, 1.000, 1.0)   // White
                ),
                6 => new RopeColors(
                    RGBAColor.MakeRGBA(0.631, 0.957, 1.000, 1.0),  // Cyan
                    RGBAColor.MakeRGBA(0.996, 0.631, 0.953, 1.0)   // Pink
                ),
                7 => new RopeColors(
                    RGBAColor.MakeRGBA(1.000, 0.329, 0.318, 1.0),  // Red-orange
                    RGBAColor.MakeRGBA(1.000, 0.992, 0.941, 1.0)   // Cream
                ),
                8 => new RopeColors(
                    RGBAColor.MakeRGBA(1.000, 0.831, 0.404, 1.0),  // Orange
                    RGBAColor.MakeRGBA(0.251, 0.239, 0.278, 1.0)   // Dark purple
                ),
                _ => new RopeColors(
                    RGBAColor.MakeRGBA(0.475, 0.305, 0.185, 1.0),  // Default to original
                    RGBAColor.MakeRGBA(0.6755555555555556, 0.44, 0.27555555555555555, 1.0)
                )
            };
        }

        /// <summary>
        /// Gets the total number of available rope color schemes.
        /// </summary>
        public const int TotalRopeColors = 9;
    }
}
