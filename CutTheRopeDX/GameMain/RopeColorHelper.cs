using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Helper class for managing rope color schemes.
    /// </summary>
    internal static class RopeColorHelper
    {
        /// <summary>
        /// Represents a rope color scheme with two base colors.
        /// </summary>
        /// <param name="Color1">The primary rope color.</param>
        /// <param name="Color2">The secondary rope color used for alternating segments.</param>
        public readonly record struct RopeColors(
            RGBAColor Color1,
            RGBAColor Color2
        );

        /// <summary>
        /// Gets the rope color scheme for a given rope index (0-8).
        /// </summary>
        /// <param name="ropeIndex">The rope skin index. Values outside 0–8 return the default scheme.</param>
        /// <returns>The <see cref="RopeColors"/> for the specified index.</returns>
        public static RopeColors GetRopeColors(int ropeIndex)
        {
            return ropeIndex switch
            {
                0 => new RopeColors(
                    RGBAColor.MakeRGBA(0.475f, 0.305f, 0.185f, 1),  // Original brown (default)
                    RGBAColor.MakeRGBA(0.6755555555555556f, 0.44f, 0.27555555555555555f, 1)
                ),
                1 => new RopeColors(
                    RGBAColor.MakeRGBA(0.624f, 0.294f, 0.114f, 1),  // Brown
                    RGBAColor.MakeRGBA(1, 0.627f, 0.463f, 1)   // Light brown
                ),
                2 => new RopeColors(
                    RGBAColor.MakeRGBA(0.404f, 0.612f, 0.635f, 1),  // Teal
                    RGBAColor.MakeRGBA(0.773f, 0.898f, 0.902f, 1)   // Light teal
                ),
                3 => new RopeColors(
                    RGBAColor.MakeRGBA(0.757f, 0.533f, 0, 1),  // Gold
                    RGBAColor.MakeRGBA(0.98f, 0.843f, 0.2f, 1)   // Light gold
                ),
                4 => new RopeColors(
                    RGBAColor.MakeRGBA(0.980f, 0.243f, 0.243f, 1),  // Red
                    RGBAColor.MakeRGBA(0.282f, 0.525f, 0.153f, 1)   // Green
                ),
                5 => new RopeColors(
                    RGBAColor.MakeRGBA(0.176f, 0.318f, 0.659f, 1),  // Blue
                    RGBAColor.MakeRGBA(1, 1, 1, 1)   // White
                ),
                6 => new RopeColors(
                    RGBAColor.MakeRGBA(0.631f, 0.957f, 1, 1),  // Cyan
                    RGBAColor.MakeRGBA(0.996f, 0.631f, 0.953f, 1)   // Pink
                ),
                7 => new RopeColors(
                    RGBAColor.MakeRGBA(1, 0.329f, 0.318f, 1),  // Red-orange
                    RGBAColor.MakeRGBA(1, 0.992f, 0.941f, 1)   // Cream
                ),
                8 => new RopeColors(
                    RGBAColor.MakeRGBA(1, 0.831f, 0.404f, 1),  // Orange
                    RGBAColor.MakeRGBA(0.251f, 0.239f, 0.278f, 1)   // Dark purple
                ),
                _ => new RopeColors(
                    RGBAColor.MakeRGBA(0.475f, 0.305f, 0.185f, 1),  // Default to original
                    RGBAColor.MakeRGBA(0.6755555555555556f, 0.44f, 0.27555555555555555f, 1)
                )
            };
        }

        /// <summary>
        /// Gets the total number of available rope color schemes.
        /// </summary>
        public const int TotalRopeColors = 9;
    }
}
