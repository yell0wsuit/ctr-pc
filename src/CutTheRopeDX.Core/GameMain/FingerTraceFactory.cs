using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain.FingerTraces;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Factory for finger trace skin instances.
    /// </summary>
    internal static class FingerTraceFactory
    {
        /// <summary>Total number of selectable finger trace skins.</summary>
        public const int TotalTraceSkins = 11;

        /// <summary>
        /// Creates a finger trace instance for a selected trace skin.
        /// </summary>
        /// <param name="traceIndex">Trace skin index.</param>
        /// <returns>The finger trace for the requested skin index, or the classic trace for unknown indices.</returns>
        public static FingerTrace Create(int traceIndex)
        {
            return traceIndex switch
            {
                0 => new ClassicFingerTrace(),
                1 => new BubbleFingerTrace(),
                2 => new LightningFingerTrace(),
                3 => new StarFingerTrace(),
                4 => new WinterFingerTrace(),
                5 => new RedFingerTrace(),
                6 => new EasterFingerTrace(),
                7 => new OmnomBirthdayFingerTrace(),
                8 => new BackToSchoolFingerTrace(),
                9 => new AlphabetFingerTrace(),
                10 => new Easter2016FingerTrace(),
                _ => new ClassicFingerTrace(),
            };
        }
    }
}
