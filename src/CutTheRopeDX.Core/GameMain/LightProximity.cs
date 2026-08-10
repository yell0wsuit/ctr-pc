using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure light-radius geometry helpers.
    /// </summary>
    internal static class LightProximity
    {
        /// <summary>
        /// Returns true when <paramref name="point"/> is strictly inside the light radius.
        /// </summary>
        public static bool IsWithinLight(Vector point, Vector lightPos, float lightRadius)
        {
            float dx = point.X - lightPos.X;
            float dy = point.Y - lightPos.Y;
            return (dx * dx) + (dy * dy) < lightRadius * lightRadius;
        }
    }
}
