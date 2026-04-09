using CutTheRope.Framework;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style classic finger trace that renders a solid white bezier ribbon with no particles.
    /// </summary>
    internal sealed class ClassicFingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes a classic finger trace.
        /// </summary>
        public ClassicFingerTrace()
            : base(
                segmentLife: 0.1f,
                particleBurstDuration: 0f,
                particleEmissionRate: 0f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: null,
                glowTranslateY: 0f)
        {
        }

        /// <summary>
        /// Initializes a classic trace for a touch slot.
        /// </summary>
        /// <param name="_">Unused touch-slot placeholder retained for compatibility with the existing API.</param>
        public ClassicFingerTrace(int _)
            : this()
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            return RGBAColor.MakeRGBA(1f, 1f, 1f, 1f);
        }
    }
}
