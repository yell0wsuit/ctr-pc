using CutTheRope.Framework;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style back-to-school finger trace with a golden ribbon, glow overlay, and school-themed particles.
    /// </summary>
    internal sealed class BackToSchoolFingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes a back-to-school finger trace.
        /// </summary>
        public BackToSchoolFingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 50f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: 2,
                glowTranslateY: 50f,
                NamedTracePresets.CreateAlphaParticles(39, 4))
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            return NamedTracePresets.GetGoldenRibbonColor(t);
        }
    }
}
