namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Receives callbacks from a <see cref="Timeline"/> during playback.
    /// </summary>
    internal interface ITimelineDelegate
    {
        /// <summary>
        /// Called when the timeline reaches keyframe <paramref name="k"/> at index <paramref name="i"/>.
        /// </summary>
        /// <param name="t">Timeline that reached the keyframe.</param>
        /// <param name="k">Keyframe that was reached.</param>
        /// <param name="i">Index of the keyframe.</param>
        void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i);

        /// <summary>
        /// Called when the timeline finishes playback.
        /// </summary>
        /// <param name="t">Timeline that finished.</param>
        void TimelineFinished(Timeline t);
    }
}
