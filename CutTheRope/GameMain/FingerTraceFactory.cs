using CutTheRope.Framework.Visual;
using CutTheRope.GameMain.FingerTraces;

namespace CutTheRope.GameMain
{
    internal static class FingerTraceFactory
    {
        public const int TotalTraceSkins = 9;

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
                6 => new EasterFingerTrace(touchSlot),
                7 => new BackToSchoolFingerTrace(touchSlot),
                8 => new OmnomBirthdayFingerTrace(touchSlot),
                _ => new ClassicFingerTrace(touchSlot),
            };
        }
    }
}
