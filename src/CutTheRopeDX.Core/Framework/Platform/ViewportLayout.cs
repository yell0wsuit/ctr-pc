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
        /// Narrowest width-to-height ratio the content scale distinguishes. A surface narrower
        /// than this is drawn whole, at the scale this shape is drawn at.
        /// </summary>
        /// <remarks>
        /// An endpoint of <see cref="ContentFit"/>'s curve rather than a bound on what may be
        /// shown: the window is the player's to shape and the layout follows whatever they choose.
        /// Cropping to the nearest endpoint instead is what put black bars down the sides of an
        /// ultrawide window, and every background in the game covers the region it is handed, so
        /// there was nothing for the crop to protect.
        /// </remarks>
        public const float MinAspect = 0.4f;

        /// <summary>
        /// Widest width-to-height ratio the content scale distinguishes. A surface wider than this
        /// is drawn whole, at the scale this shape is drawn at.
        /// </summary>
        /// <remarks>See <see cref="MinAspect"/>.</remarks>
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
            // The whole surface, whatever shape it is: what the game draws into is what the host
            // gives it.
            CTRRectangle render = new(0f, 0f, surfaceWidth, surfaceHeight);
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
    }
}
