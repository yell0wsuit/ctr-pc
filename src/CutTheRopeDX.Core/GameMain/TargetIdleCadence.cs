namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Decides which target timeline may advance the external blink/idle cadence.
    /// </summary>
    internal static class TargetIdleCadence
    {
        /// <summary>
        /// Determines whether the active driver timeline should advance the blink and
        /// idle-variant cadence handled by the external timeline delegate.
        /// </summary>
        /// <remarks>
        /// Only the idle loop drives that cadence. Other looping or root-driven timelines
        /// (for example chewing) also emit a keyframe at index 1; forwarding those to the
        /// idle handler would be misread as an idle tick and interrupt the animation with a
        /// random idle variant.
        /// </remarks>
        /// <param name="driverTimelineId">Active driver timeline ID, or a negative value when none is bound.</param>
        /// <param name="idleLoopTimelineId">Timeline ID configured as the idle loop.</param>
        /// <returns><see langword="true"/> when the driver is the idle loop; otherwise, <see langword="false"/>.</returns>
        public static bool DrivesIdleCadence(int driverTimelineId, int idleLoopTimelineId)
        {
            return driverTimelineId >= 0 && driverTimelineId == idleLoopTimelineId;
        }
    }
}
