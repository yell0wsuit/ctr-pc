using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Gameplay camera policy: where the camera sits inside the range the level gives it.
    /// </summary>
    internal static class GameplayCamera
    {
        /// <summary>
        /// Returns where along an axis's scrollable range the camera should sit, as a fraction.
        /// </summary>
        /// <remarks>
        /// The fit only ever scales the camera window - the design box, or the level when it is
        /// smaller - so a viewport shaped differently from that box exposes world beyond it. That
        /// exposed slack is world the level no longer has to scroll to show. Once it covers the
        /// whole scrollable range the picture has nowhere left to go, and following the tracked
        /// point would only slide a view that already contains the level; the axis holds centered
        /// instead. The test is per axis because a level can exceed the box on one and not the
        /// other, and it is made against the slack the fit actually produces rather than against
        /// the viewport's aspect, which says nothing about how far the level still has to travel.
        /// </remarks>
        /// <param name="tracked">Where the tracking has driven the camera on this axis.</param>
        /// <param name="origin">World coordinate of the level's near edge on this axis.</param>
        /// <param name="scrollable">How far the camera window can travel across the level.</param>
        /// <param name="slack">World the viewport exposes beyond the camera window.</param>
        /// <returns>The anchor, 0 to 1, where 0.5 is centered.</returns>
        public static float Anchor(float tracked, float origin, float scrollable, float slack)
        {
            return scrollable <= slack
                ? 0.5f
                : CTRMathHelper.FIT_TO_BOUNDARIES((tracked - origin) / scrollable, 0f, 1f);
        }
    }
}
