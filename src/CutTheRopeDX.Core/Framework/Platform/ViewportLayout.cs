namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// Turns a surface size into a <see cref="ViewportLayoutSnapshot"/>. Pure: the same
    /// inputs always produce an equal snapshot, and nothing here touches the renderer.
    /// </summary>
    internal static class ViewportLayout
    {
        /// <summary>
        /// Width of the fixed design coordinate system.
        /// </summary>
        public const float DesignWidth = 2560f;

        /// <summary>
        /// Height of the fixed design coordinate system.
        /// </summary>
        public const float DesignHeight = 1440f;

        /// <summary>
        /// Computes the snapshot for a surface of the given size.
        /// </summary>
        /// <param name="surfaceWidth">Drawable surface width in pixels.</param>
        /// <param name="surfaceHeight">Drawable surface height in pixels.</param>
        /// <param name="fullScreenCropWidth">
        /// Whether portrait surfaces crop width to a 5:4 band instead of fitting the design
        /// aspect. Preserves the behavior the desktop host selects.
        /// </param>
        /// <returns>The snapshot describing that surface.</returns>
        public static ViewportLayoutSnapshot Compute(
            int surfaceWidth,
            int surfaceHeight,
            bool fullScreenCropWidth)
        {
            CTRRectangle legacy = ComputeLegacyContentBounds(
                surfaceWidth,
                surfaceHeight,
                fullScreenCropWidth);

            return new ViewportLayoutSnapshot(
                surfaceWidth,
                surfaceHeight,
                legacy,
                legacy.w / DesignWidth);
        }

        /// <summary>
        /// Reproduces the scaled view rectangle the fixed-layout presentation has always used.
        /// </summary>
        /// <param name="surfaceWidth">Drawable surface width in pixels.</param>
        /// <param name="surfaceHeight">Drawable surface height in pixels.</param>
        /// <param name="fullScreenCropWidth">Whether portrait surfaces crop width to a 5:4 band.</param>
        /// <returns>The destination rectangle for fixed-layout content.</returns>
        private static CTRRectangle ComputeLegacyContentBounds(
            int surfaceWidth,
            int surfaceHeight,
            bool fullScreenCropWidth)
        {
            if (surfaceWidth >= surfaceHeight)
            {
                int scaledHeight = fullScreenCropWidth
                    ? surfaceHeight
                    : ScaledDesignHeight(surfaceWidth);
                int scaledWidth = fullScreenCropWidth
                    ? ScaledDesignWidth(scaledHeight)
                    : surfaceWidth;
                return new CTRRectangle(
                    (surfaceWidth - scaledWidth) / 2,
                    (surfaceHeight - scaledHeight) / 2,
                    scaledWidth,
                    scaledHeight);
            }

            int portraitHeight = fullScreenCropWidth
                ? (int)(surfaceWidth / 5f * 4f)
                : ScaledDesignHeight(surfaceWidth);
            int portraitWidth = fullScreenCropWidth
                ? ScaledDesignWidth(portraitHeight)
                : surfaceWidth;
            return new CTRRectangle(
                (surfaceWidth - portraitWidth) / 2,
                (surfaceHeight - portraitHeight) / 2,
                portraitWidth,
                portraitHeight);
        }

        /// <summary>
        /// Returns the aspect-preserving design width for a scaled height.
        /// </summary>
        /// <param name="scaledHeight">Scaled height in surface pixels.</param>
        /// <returns>Aspect-correct width.</returns>
        private static int ScaledDesignWidth(int scaledHeight)
        {
            return (int)((scaledHeight / (DesignHeight / DesignWidth)) + 0.5);
        }

        /// <summary>
        /// Returns the aspect-preserving design height for a scaled width.
        /// </summary>
        /// <param name="scaledWidth">Scaled width in surface pixels.</param>
        /// <returns>Aspect-correct height.</returns>
        private static int ScaledDesignHeight(int scaledWidth)
        {
            return (int)((scaledWidth * (DesignHeight / DesignWidth)) + 0.5);
        }
    }
}
