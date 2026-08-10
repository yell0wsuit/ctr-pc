using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;

namespace CutTheRopeDX.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style winter finger trace with an icy ribbon, winter glow, and snowflake particles.
    /// </summary>
    internal sealed class WinterFingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes a winter finger trace.
        /// </summary>
        public WinterFingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 50f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: 1,
                glowTranslateY: 48f,
                NamedTracePresets.CreateWinterParticles())
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            if (t < 0.5f)
            {
                float blend = t * 2f;
                return RGBAColor.MakeRGBA(
                    CTRMathHelper.Lerp(0.13f, 0.51765f, blend),
                    CTRMathHelper.Lerp(0.59608f, 1f, blend),
                    CTRMathHelper.Lerp(0.75686f, 1f, blend),
                    CTRMathHelper.Lerp(0f, 1f, blend));
            }

            float fade = (t - 0.5f) * 2f;
            return RGBAColor.MakeRGBA(
                CTRMathHelper.Lerp(0.51765f, 1f, fade),
                1f,
                1f,
                1f);
        }
    }
}
