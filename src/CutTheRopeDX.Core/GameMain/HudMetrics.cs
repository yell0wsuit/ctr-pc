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
        /// Smallest chrome size in physical pixels on a pointer-driven host.
        /// </summary>
        private const float PhysicalFloor = 70f;
    }
}
