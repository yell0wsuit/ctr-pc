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
        /// <param name="devicePixelRatio">Physical pixels per logical pixel on the host surface.</param>
        /// <returns>The snapshot describing that surface.</returns>
        public static ViewportLayoutSnapshot Compute(
            int surfaceWidth,
            int surfaceHeight,
            float devicePixelRatio = 1f)
        {
            CTRRectangle render = ClampToSupportedAspect(surfaceWidth, surfaceHeight);
            float scale = MathF.Min(render.w, render.h) / LogicalShortSide;

            return new ViewportLayoutSnapshot(
                surfaceWidth,
                surfaceHeight,
                render,
                new CTRRectangle(0f, 0f, render.w / scale, render.h / scale),
                scale,
                devicePixelRatio,
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
    }
}
