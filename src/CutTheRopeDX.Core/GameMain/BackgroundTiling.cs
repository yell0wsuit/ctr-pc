using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Calculates placement shared by the authored gameplay background pieces.</summary>
    internal static class BackgroundTiling
    {
        private const float Epsilon = 0.001f;

        /// <summary>Returns how many seams between P1 sections need a P2 overlay.</summary>
        /// <param name="mapHeight">Height of the level map.</param>
        /// <param name="mapSectionHeight">Map height represented by one P1 section.</param>
        /// <returns>One fewer than the number of P1 sections used by the map.</returns>
        public static int GetP2Count(float mapHeight, float mapSectionHeight)
        {
            if (mapSectionHeight <= 0f)
            {
                return 0;
            }

            int p1Count = Math.Max(
                1,
                (int)MathF.Ceiling((mapHeight / mapSectionHeight) - Epsilon));
            return Math.Max(0, p1Count - 1);
        }

        /// <summary>
        /// Returns the inclusive range of repeated sections a view window touches.
        /// </summary>
        /// <remarks>
        /// The P1 tile map repeats in both directions to fill a design-sized window around the
        /// camera, so everything anchored to a single P1 section — the P2 seam overlay, the earth
        /// art — has to be repeated over the same sections or it dresses only the section it was
        /// authored on and leaves every other one bare.
        /// </remarks>
        /// <param name="sectionOrigin">Where the authored section starts along this axis.</param>
        /// <param name="sectionSize">Size of one repeated section along this axis.</param>
        /// <param name="windowStart">Where the view window starts along this axis.</param>
        /// <param name="windowSize">Size of the view window along this axis.</param>
        /// <returns>First and last section index, both inclusive and possibly negative.</returns>
        public static (int First, int Last) GetSectionRange(
            float sectionOrigin,
            float sectionSize,
            float windowStart,
            float windowSize)
        {
            if (sectionSize <= 0f || float.IsNaN(sectionSize) || float.IsInfinity(sectionSize))
            {
                return (0, 0);
            }

            int first = (int)MathF.Floor((windowStart - sectionOrigin) / sectionSize);
            // The nudge is a fraction of a source unit, not of a section: it exists only to keep a
            // window that ends exactly on a boundary from claiming the section past it, and a
            // section-sized nudge would swallow a genuine sliver of the next one instead.
            int last = (int)MathF.Ceiling(
                (windowStart + windowSize - Epsilon - sectionOrigin) / sectionSize) - 1;
            return (first, Math.Max(first, last));
        }

        /// <summary>Places a P2 overlay at the requested seam between repeated P1 sections.</summary>
        /// <param name="originalP2Y">Authored P2 offset for the first seam.</param>
        /// <param name="p1Height">Height of one repeated P1 texture.</param>
        /// <param name="seamIndex">Zero-based seam index from the top of the map.</param>
        /// <returns>The P2 offset for that seam.</returns>
        public static float ResolveP2Y(float originalP2Y, float p1Height, int seamIndex)
        {
            return originalP2Y + (Math.Max(0, seamIndex) * p1Height);
        }
    }
}
