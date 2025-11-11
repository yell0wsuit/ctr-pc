namespace CutTheRope.iframework.visual
{
    internal interface ITimelineDelegate
    {
        void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i);

        void TimelineFinished(Timeline t);
    }
}
