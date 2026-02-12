namespace CutTheRope.Desktop
{
    internal interface IBackingScaleProvider
    {
        bool TryGetCurrentScale(out double scale);
    }

    internal sealed class FallbackBackingScaleProvider : IBackingScaleProvider
    {
        public bool TryGetCurrentScale(out double scale)
        {
            scale = 1d;
            return true;
        }
    }
}
