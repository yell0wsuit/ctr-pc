using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Creates target animation backend implementations.
    /// </summary>
    internal static class TargetAnimationBackendFactory
    {
        /// <summary>
        /// Creates the original timeline-based Om Nom animation backend.
        /// </summary>
        /// <param name="isNightLevel">Whether sleep animations should be configured.</param>
        /// <param name="isXmas">Whether Christmas animation variants should be configured.</param>
        /// <returns>The configured original backend.</returns>
        public static ITargetAnimationBackend CreateOriginal(bool isNightLevel, bool isXmas, string animationStyle)
        {
            return string.Equals(animationStyle, "flash", System.StringComparison.OrdinalIgnoreCase)
                ? new FlashXmlTargetAnimationBackend()
                : new OriginalTargetAnimationBackend(isNightLevel, isXmas);
        }
    }
}
