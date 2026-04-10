using CutTheRope.Framework;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style Om Nom birthday finger trace with a golden ribbon, glow overlay, and party-themed particles.
    /// </summary>
    internal sealed class OmnomBirthdayFingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes an Om Nom birthday finger trace.
        /// </summary>
        public OmnomBirthdayFingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 50f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: 2,
                glowTranslateY: 50f,
                NamedTracePresets.CreateAlphaParticles(43, 9))
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            return NamedTracePresets.GetGoldenRibbonColor(t);
        }
    }
}
