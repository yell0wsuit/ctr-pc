namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Gameplay camera policy: whether the camera follows the action, and where it sits when it
    /// does not.
    /// </summary>
    internal static class GameplayCamera
    {
        /// <summary>
        /// Returns whether the camera should stop following the tracked point because the
        /// viewport already contains the whole level.
        /// </summary>
        /// <param name="levelWidth">Level width in world units.</param>
        /// <param name="levelHeight">Level height in world units.</param>
        /// <param name="viewportWidth">Viewport width in logical units.</param>
        /// <param name="viewportHeight">Viewport height in logical units.</param>
        /// <returns>
        /// <see langword="true"/> when the camera should center and hold still.
        /// </returns>
        public static bool ScrollIsLocked(
            float levelWidth,
            float levelHeight,
            float viewportWidth,
            float viewportHeight)
        {
            float levelAspect = levelWidth / levelHeight;
            float viewportAspect = viewportWidth / viewportHeight;

            // Both landscape and the viewport proportionally wider than the level: every part of
            // the level is on screen at once, so following anything would move a picture that has
            // nowhere left to go.
            return levelAspect > 1f && viewportAspect > 1f && viewportAspect > levelAspect;
        }
    }
}
