namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// Receives callbacks from a <see cref="Timeline"/> during playback.
    /// </summary>
    internal interface ITimelineDelegate
    {
        /// <summary>Called when the timeline reaches keyframe <paramref name="k"/> at index <paramref name="i"/>.</summary>
        void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i);

        /// <summary>Called when the timeline finishes playback.</summary>
        void TimelineFinished(Timeline t);
    }
}
