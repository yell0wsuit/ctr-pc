using System;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Pure-math helpers for the ant-conveyor system, ported from decompiled iOS code.
    /// All geometry constants scale linearly with the device pixel-density multiplier
    /// returned by <c>deviceScale</c>.
    /// </summary>
    public static class AntConveyorLogic
    {
        /// <summary>
        /// Seconds of interaction time after which the candy snaps hard to the carrier position
        /// instead of lerping toward it.
        /// </summary>
        public const float CarrierSnapTimeThreshold = 0.15f;

        /// <summary>
        /// Converts a path vertex from level-local space into world space.
        /// </summary>
        /// <param name="startPos">World-space origin of the ant-path object.</param>
        /// <param name="relativePoint">Vertex coordinates relative to <paramref name="startPos"/>, in level units.</param>
        /// <param name="offsetX">Horizontal world-space offset (map origin + camera).</param>
        /// <param name="offsetY">Vertical world-space offset (map origin + camera).</param>
        /// <param name="scale">Level scale factor applied to the path coordinates.</param>
        /// <returns>The vertex position in world space.</returns>
        public static Vector ComputePathPoint(Vector startPos, Vector relativePoint, float offsetX, float offsetY, float scale)
        {
            return new Vector(
                ((startPos.X + relativePoint.X) * scale) + offsetX,
                ((startPos.Y + relativePoint.Y) * scale) + offsetY);
        }

        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="touchWorld"/> falls within the square touch zone
        /// centred on the candy, used to let the player manually detach the candy from the conveyor.
        /// </summary>
        /// <param name="touchWorld">Touch position in world space.</param>
        /// <param name="candyPoint">Candy centre in world space.</param>
        /// <param name="halfSize">Half the side length of the touch zone (see <see cref="GetCarrierTouchHalfSize"/>).</param>
        /// <returns><see langword="true"/> if the touch is inside the square zone; otherwise <see langword="false"/>.</returns>
        public static bool IsPointInCarrierTouchZone(Vector touchWorld, Vector candyPoint, float halfSize)
        {
            return touchWorld.X >= candyPoint.X - halfSize
                && touchWorld.X < candyPoint.X + halfSize
                && touchWorld.Y >= candyPoint.Y - halfSize
                && touchWorld.Y < candyPoint.Y + halfSize;
        }

        /// <summary>
        /// Computes the next candy position while it is being carried by the ant conveyor.
        /// Snaps the candy directly to <paramref name="interactionPosition"/> once it is within
        /// <paramref name="snapDistance"/> or after <see cref="CarrierSnapTimeThreshold"/> seconds;
        /// otherwise moves it 30% of the remaining distance each frame (decompiled iOS behaviour).
        /// </summary>
        /// <param name="candyPosition">Current candy world position.</param>
        /// <param name="interactionPosition">Current carrier-marker world position.</param>
        /// <param name="interactionTime">Elapsed seconds since the candy attached to this segment.</param>
        /// <param name="snapDistance">Distance below which the candy snaps instantly (see <see cref="GetCarrierSnapDistance"/>).</param>
        /// <returns>Updated candy world position for this frame.</returns>
        public static Vector ComputeCarrierFollowPosition(Vector candyPosition, Vector interactionPosition, float interactionTime, float snapDistance)
        {
            float dx = candyPosition.X - interactionPosition.X;
            float dy = candyPosition.Y - interactionPosition.Y;
            float distance = MathF.Sqrt((dx * dx) + (dy * dy));

            if (distance <= snapDistance || interactionTime >= CarrierSnapTimeThreshold)
            {
                return interactionPosition;
            }

            // Decompiled behavior: pos += (pos - interaction) * -0.3f
            return new Vector(
                candyPosition.X + (dx * -0.3f),
                candyPosition.Y + (dy * -0.3f));
        }

        /// <summary>Spacing between spawned ants along the path. iOS constant: 35 × deviceScale.</summary>
        /// <param name="deviceScale">Device pixel-density multiplier (see <c>deviceScale</c>).</param>
        /// <returns>The gap distance in world units.</returns>
        public static float GetSpawnGap(float deviceScale)
        {
            return 35f * deviceScale;
        }

        /// <summary>
        /// Distance from each path endpoint within which ant sprites fade in or out.
        /// iOS constant: 15 × deviceScale.
        /// </summary>
        /// <param name="deviceScale">Device pixel-density multiplier (see <c>deviceScale</c>).</param>
        /// <returns>The fade distance in world units.</returns>
        public static float GetEdgeFadeDistance(float deviceScale)
        {
            return 15f * deviceScale;
        }

        /// <summary>
        /// Half-height of the segment interaction rectangle, measured perpendicular to the path direction.
        /// iOS constant: 13.5 × deviceScale.
        /// </summary>
        /// <param name="deviceScale">Device pixel-density multiplier (see <c>deviceScale</c>).</param>
        /// <returns>The half-height in world units.</returns>
        public static float GetSegmentHalfHeight(float deviceScale)
        {
            return 13.5f * deviceScale;
        }

        /// <summary>
        /// Half-size of the square touch zone the player can tap to manually detach the candy from the conveyor.
        /// iOS constant: 30 × deviceScale.
        /// </summary>
        /// <param name="deviceScale">Device pixel-density multiplier (see <c>deviceScale</c>).</param>
        /// <returns>The touch zone half-size in world units.</returns>
        public static float GetCarrierTouchHalfSize(float deviceScale)
        {
            return 30f * deviceScale;
        }

        /// <summary>
        /// Distance within which the candy snaps directly to the carrier position without lerping.
        /// iOS constant: 2 × deviceScale.
        /// </summary>
        /// <param name="deviceScale">Device pixel-density multiplier (see <c>deviceScale</c>).</param>
        /// <returns>The snap distance in world units.</returns>
        public static float GetCarrierSnapDistance(float deviceScale)
        {
            return 2f * deviceScale;
        }

    }
}
