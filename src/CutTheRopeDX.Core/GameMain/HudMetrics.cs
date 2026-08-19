using System;

using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// How much of the screen a piece of corner chrome takes, gap included.
    /// </summary>
    /// <param name="Width">Room it takes across.</param>
    /// <param name="Height">Room it takes up the screen.</param>
    internal readonly record struct ChromeRoom(float Width, float Height);

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
        /// Returns the room a piece of corner chrome takes on this viewport: the size it is drawn
        /// at, plus the gap whatever it is drawn over should keep from it.
        /// </summary>
        /// <remarks>
        /// One answer for every view that has to keep clear of the same button - a scrolling
        /// column of credits, a grid of skins - rather than each working the drawn size back out
        /// of <see cref="ChromeScale"/> and adding a gap of its own.
        /// </remarks>
        /// <param name="snapshot">The viewport to size against.</param>
        /// <param name="authoredWidth">The element's authored width.</param>
        /// <param name="authoredHeight">The element's authored height.</param>
        /// <param name="isMobile">Whether the host is a touch device.</param>
        /// <returns>The room the element takes, in logical units.</returns>
        public static ChromeRoom RoomFor(
            ViewportLayoutSnapshot snapshot,
            float authoredWidth,
            float authoredHeight,
            bool isMobile)
        {
            float scale = ChromeScale(
                snapshot,
                MathF.Max(authoredWidth, authoredHeight),
                isMobile);
            float gap = ChromeGap * scale;
            return new ChromeRoom((authoredWidth * scale) + gap, (authoredHeight * scale) + gap);
        }

        /// <summary>Gap between a piece of chrome and whatever is drawn under it.</summary>
        private const float ChromeGap = 20f;

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
