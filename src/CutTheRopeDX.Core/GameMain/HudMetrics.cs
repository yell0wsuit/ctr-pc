using System;

using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Sizes chrome that must stay physically reachable regardless of how dense the display is
    /// or how the window is shaped.
    /// </summary>
    internal static class HudMetrics
    {
        /// <summary>
        /// Returns the size for a chrome element, in logical units.
        /// </summary>
        /// <param name="snapshot">The viewport to size against.</param>
        /// <param name="isMobile">Whether the host is a touch device.</param>
        /// <returns>The element's size in logical units.</returns>
        public static float ChromeSize(ViewportLayoutSnapshot snapshot, bool isMobile)
        {
            float dpr = snapshot.DevicePixelRatio;
            float longEdge = MathF.Max(snapshot.SurfaceWidth, snapshot.SurfaceHeight);
            bool smallScreen =
                MathF.Min(snapshot.SurfaceWidth, snapshot.SurfaceHeight) <= 800f
                && longEdge <= 1280f
                && dpr <= 2f;

            float factor = isMobile
                ? (smallScreen ? 0.08f : 0.04f)
                : dpr <= 1f ? 0.05f : dpr <= 1.25f ? 0.06f : 0.07f;

            float physical = longEdge * factor;
            if (snapshot.Aspect > 1f)
            {
                physical *= 0.9f;
            }
            if (!isMobile)
            {
                // Applied after the landscape trim so the floor is a true final guarantee
                // rather than a value the trim can push back under it.
                physical = MathF.Max(physical, PhysicalFloor);
            }

            return physical / snapshot.Scale;
        }

        /// <summary>
        /// Returns the scale to draw a chrome element at: the larger of the scale the content
        /// around it is drawn at and the size this surface needs it to be to stay reachable.
        /// </summary>
        /// <remarks>
        /// The two answer different questions - how far this viewport is from the shape the game
        /// was drawn for, and how small the element may get in the player's hand - so an element
        /// that has to satisfy both takes whichever asks for more. Derived rather than read off a
        /// placed button, so anything that has to reason about the room a piece of chrome takes -
        /// a scrolling view keeping its content clear of it, say - gets the same answer without
        /// having to be laid out after it.
        /// </remarks>
        /// <param name="snapshot">The viewport to size against.</param>
        /// <param name="longestSide">The element's longest authored side.</param>
        /// <param name="isMobile">Whether the host is a touch device.</param>
        /// <returns>The uniform scale to draw the element at.</returns>
        public static float ChromeScale(ViewportLayoutSnapshot snapshot, float longestSide, bool isMobile)
        {
            float content = ContentFit.ScaleForAspect(snapshot.Aspect);
            return longestSide <= 0f
                ? content
                : MathF.Max(content, ChromeSize(snapshot, isMobile) / longestSide);
        }

        /// <summary>
        /// Whether the host drives the game by touch. No host reports this yet, so the pointer
        /// branch applies everywhere; it is the conservative one, because it carries the floor.
        /// </summary>
        public const bool IsTouchHost = false;

        /// <summary>
        /// Smallest chrome size in physical pixels on a pointer-driven host.
        /// </summary>
        private const float PhysicalFloor = 70f;
    }
}
