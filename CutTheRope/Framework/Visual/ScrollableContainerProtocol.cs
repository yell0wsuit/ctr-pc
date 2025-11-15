namespace CutTheRope.Framework.Visual
{
    internal interface IScrollableContainerProtocol
    {
        void ScrollableContainerreachedScrollPoint(ScrollableContainer e, int i);

        void ScrollableContainerchangedTargetScrollPoint(ScrollableContainer e, int i);
    }
}
