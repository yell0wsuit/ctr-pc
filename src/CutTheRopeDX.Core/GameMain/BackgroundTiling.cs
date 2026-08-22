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
