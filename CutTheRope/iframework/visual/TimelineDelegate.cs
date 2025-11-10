namespace CutTheRope.iframework.visual
{
    internal interface TimelineDelegate
    {
        void timelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i);

        void timelineFinished(Timeline t);
    }
}
