using CutTheRope.Framework;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style alphabet finger trace with a golden ribbon, glow overlay, and letter particles.
    /// </summary>
    internal sealed class AlphabetFingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes an alphabet finger trace.
        /// </summary>
        public AlphabetFingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 50f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: 2,
                glowTranslateY: 50f,
                NamedTracePresets.CreateAlphaParticles(62, 5))
        {
        }

        /// <summary>
        /// Initializes an alphabet trace for a touch slot.
        /// </summary>
        /// <param name="_">Unused touch-slot placeholder retained for compatibility with the existing API.</param>
        public AlphabetFingerTrace(int _)
            : this()
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            return NamedTracePresets.GetGoldenRibbonColor(t);
        }
    }
}
