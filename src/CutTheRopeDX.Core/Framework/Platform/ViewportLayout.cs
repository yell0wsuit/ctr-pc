using System;

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
        /// Narrowest supported width-to-height ratio. Taller surfaces are cropped to it.
        /// </summary>
        public const float MinAspect = 0.4f;

        /// <summary>
        /// Widest supported width-to-height ratio. Wider surfaces are cropped to it.
        /// </summary>
        public const float MaxAspect = 2.5f;

        /// <summary>
        /// Logical length of the viewport's shorter side. Anchoring the short side keeps asset
        /// scale stable as the window changes shape.
        /// </summary>
        public const float LogicalShortSide = 1440f;

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
            CTRRectangle render = ClampToSupportedAspect(surfaceWidth, surfaceHeight);
            float scale = MathF.Min(render.w, render.h) / LogicalShortSide;

            return new ViewportLayoutSnapshot(
                surfaceWidth,
                surfaceHeight,
                legacy,
                legacy.w / DesignWidth,
                render,
                new CTRRectangle(0f, 0f, render.w / scale, render.h / scale),
                scale,
                surfaceWidth >= surfaceHeight
                    ? LayoutOrientation.Landscape
                    : LayoutOrientation.Portrait);
        }

        /// <summary>
        /// Returns the centered sub-rectangle of the surface whose aspect ratio lies within the
        /// supported range. Surfaces already inside the range are returned whole.
        /// </summary>
        /// <param name="surfaceWidth">Drawable surface width in pixels.</param>
        /// <param name="surfaceHeight">Drawable surface height in pixels.</param>
        /// <returns>The rectangle the game draws into.</returns>
        private static CTRRectangle ClampToSupportedAspect(int surfaceWidth, int surfaceHeight)
        {
            float aspect = surfaceWidth / (float)surfaceHeight;
            if (aspect > MaxAspect)
            {
                float width = surfaceHeight * MaxAspect;
                return new CTRRectangle((surfaceWidth - width) / 2f, 0f, width, surfaceHeight);
            }
            if (aspect < MinAspect)
            {
                float height = surfaceWidth / MinAspect;
                return new CTRRectangle(0f, (surfaceHeight - height) / 2f, surfaceWidth, height);
            }
            return new CTRRectangle(0f, 0f, surfaceWidth, surfaceHeight);
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
