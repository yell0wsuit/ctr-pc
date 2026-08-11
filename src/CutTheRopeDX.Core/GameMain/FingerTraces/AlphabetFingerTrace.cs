using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain.FingerTraces
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

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            return NamedTracePresets.GetGoldenRibbonColor(t);
        }
    }
}
