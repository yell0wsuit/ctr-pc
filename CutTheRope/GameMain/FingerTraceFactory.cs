using CutTheRope.Framework.Visual;
using CutTheRope.GameMain.FingerTraces;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Factory for finger trace skin instances.
    /// </summary>
    internal static class FingerTraceFactory
    {
        /// <summary>Total number of selectable finger trace skins.</summary>
        public const int TotalTraceSkins = 11;

        /// <summary>
        /// Creates a finger trace instance for a trace skin slot.
        /// </summary>
        /// <param name="traceIndex">Trace skin index.</param>
        /// <param name="touchSlot">Touch slot that owns the trace.</param>
        /// <returns>The finger trace for the requested skin index, or the classic trace for unknown indices.</returns>
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
