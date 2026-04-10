using CutTheRopeDX.Framework;

namespace CutTheRopeDX.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style Easter 2016 finger trace with a golden ribbon, glow overlay, and festive particles.
    /// </summary>
    internal sealed class Easter2016FingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes an Easter 2016 finger trace.
        /// </summary>
        public Easter2016FingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 50f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: 2,
                glowTranslateY: 50f,
                NamedTracePresets.CreateAlphaParticles(30, 9))
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            return NamedTracePresets.GetGoldenRibbonColor(t, clampBlend: true);
        }
    }
}
