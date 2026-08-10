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
        /// Represents the fully prepared rope colors used by the bungee renderer.
        /// </summary>
        /// <param name="BaseColor1">The primary rope color at full brightness.</param>
        /// <param name="BaseColor2">The secondary rope color at full brightness.</param>
        /// <param name="ShadeColor1">The primary rope color used for the darker stripe/stretched ramp.</param>
        /// <param name="ShadeColor2">The secondary rope color used for the darker stripe/stretched ramp.</param>
        public readonly record struct RopeDrawColors(
            RGBAColor BaseColor1,
            RGBAColor BaseColor2,
            RGBAColor ShadeColor1,
            RGBAColor ShadeColor2
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
        /// Creates the rope colors used for bungee rendering after alpha, highlighting, and stretch color are applied.
        /// </summary>
        /// <param name="ropeIndex">The selected rope skin index.</param>
        /// <param name="alphaMultiplier">Alpha applied to all rope colors.</param>
        /// <param name="highlighted">Whether the rope is in the highlighted state.</param>
        /// <param name="segmentLength">Current length of the first rope segment.</param>
        /// <param name="restLength">Rest length used for stretch color calculations.</param>
        /// <param name="stretchRedThreshold">Stretch threshold that enables the red color state.</param>
        /// <returns>The prepared bungee draw colors.</returns>
        public static RopeDrawColors GetDrawColors(
            int ropeIndex,
            float alphaMultiplier,
            bool highlighted,
            float segmentLength,
            float restLength,
            float stretchRedThreshold)
        {
            RopeColors ropeColors = GetRopeColors(ropeIndex);
            bool isStretchColorActive = segmentLength > restLength + stretchRedThreshold;

            // Base colors are the bright end of the gradient.
            // When the stretch color fires on a custom skin, the iOS code draws toward
            // the default rope palette (brown) so that green/blue channels drop naturally.
            // The shade (dark) end keeps the custom skin's own colors.
            RopeColors baseSource = isStretchColorActive && ropeIndex != 0
                ? GetRopeColors(0)
                : ropeColors;
            RGBAColor baseColor1 = RGBAColor.MakeRGBA(
                baseSource.Color1.RedColor * alphaMultiplier,
                baseSource.Color1.GreenColor * alphaMultiplier,
                baseSource.Color1.BlueColor * alphaMultiplier,
                alphaMultiplier
            );
            RGBAColor baseColor2 = RGBAColor.MakeRGBA(
                baseSource.Color2.RedColor * alphaMultiplier,
                baseSource.Color2.GreenColor * alphaMultiplier,
                baseSource.Color2.BlueColor * alphaMultiplier,
                alphaMultiplier
            );

            // The default skin always uses dark shading. Custom skins use full brightness
            // normally, but when stretched the dark factor is restored so the red channel
            // boost can dominate over the suppressed green/blue channels.
            float darkFactor1 = ropeIndex == 0 || isStretchColorActive ? 0.4f : 1f;
            float darkFactor2 = ropeIndex == 0 || isStretchColorActive ? 0.45f : 1f;
            RGBAColor shadeColor1 = RGBAColor.MakeRGBA(
                ropeColors.Color1.RedColor * darkFactor1 * alphaMultiplier,
                ropeColors.Color1.GreenColor * darkFactor1 * alphaMultiplier,
                ropeColors.Color1.BlueColor * darkFactor1 * alphaMultiplier,
                alphaMultiplier
            );
            RGBAColor shadeColor2 = RGBAColor.MakeRGBA(
                ropeColors.Color2.RedColor * darkFactor2 * alphaMultiplier,
                ropeColors.Color2.GreenColor * darkFactor2 * alphaMultiplier,
                ropeColors.Color2.BlueColor * darkFactor2 * alphaMultiplier,
                alphaMultiplier
            );

            if (highlighted)
            {
                float highlightMultiplier = 3f;
                baseColor1.RedColor *= highlightMultiplier;
                baseColor1.GreenColor *= highlightMultiplier;
                baseColor1.BlueColor *= highlightMultiplier;
                baseColor2.RedColor *= highlightMultiplier;
                baseColor2.GreenColor *= highlightMultiplier;
                baseColor2.BlueColor *= highlightMultiplier;
                shadeColor1.RedColor *= highlightMultiplier;
                shadeColor1.GreenColor *= highlightMultiplier;
                shadeColor1.BlueColor *= highlightMultiplier;
                shadeColor2.RedColor *= highlightMultiplier;
                shadeColor2.GreenColor *= highlightMultiplier;
                shadeColor2.BlueColor *= highlightMultiplier;
            }

            if (isStretchColorActive && !highlighted)
            {
                float stretchRedScale = segmentLength / restLength * 2f;
                shadeColor1.RedColor *= stretchRedScale;
                shadeColor2.RedColor *= stretchRedScale;
            }

            return new RopeDrawColors(baseColor1, baseColor2, shadeColor1, shadeColor2);
        }

        /// <summary>
        /// Gets the total number of available rope color schemes.
        /// </summary>
        public const int TotalRopeColors = 9;
    }
}
