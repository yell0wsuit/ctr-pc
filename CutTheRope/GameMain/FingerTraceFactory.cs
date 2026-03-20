using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal static class FingerTraceFactory
    {
        public const int TotalTraceSkins = 6;

        public static FingerTrace CreateForSlot(int traceIndex, int touchSlot)
        {
            return traceIndex switch
            {
                0 => new ClassicFingerTrace(touchSlot),
                1 => new BubbleFingerTrace(touchSlot),
                2 => new LightningFingerTrace(touchSlot),
                3 => new StarFingerTrace(touchSlot),
                4 => new WinterFingerTrace(touchSlot),
                5 => new RedFingerTrace(touchSlot),
                _ => new ClassicFingerTrace(touchSlot),
            };
        }
    }
}
