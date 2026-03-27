using CutTheRope.Framework.Visual;
using CutTheRope.GameMain.FingerTraces;

namespace CutTheRope.GameMain
{
    internal static class FingerTraceFactory
    {
        public const int TotalTraceSkins = 11;

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
                7 => new OmnomBirthdayFingerTrace(touchSlot),
                8 => new BackToSchoolFingerTrace(touchSlot),
                9 => new AlphabetFingerTrace(touchSlot),
                10 => new Easter2016FingerTrace(touchSlot),
                _ => new ClassicFingerTrace(touchSlot),
            };
        }
    }
}
