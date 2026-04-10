using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style (PRODUCT)RED finger trace with dual particle layers (glow + core) and a warm-toned ribbon.
    /// </summary>
    internal sealed class RedFingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes a red finger trace.
        /// </summary>
        public RedFingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 35f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: null,
                glowTranslateY: 0f,
                NamedTracePresets.CreateRedParticles(alpha: 0.3f, FingerTraceBlendMode.Additive),
                NamedTracePresets.CreateRedParticles(alpha: 0.75f, FingerTraceBlendMode.Alpha))
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            if (t < 0.7f)
            {
                float blend = t * 2f;
                return RGBAColor.MakeRGBA(
                    MathHelper.Lerp(0.7882f, 0.98824f, blend),
                    MathHelper.Lerp(0.2157f, 0.20784f, blend),
                    MathHelper.Lerp(0.2980f, 0.16863f, blend),
                    MathHelper.Lerp(0f, 1f, blend));
            }

            float fade = (t - 0.7f) * 2f;
            return RGBAColor.MakeRGBA(
                MathHelper.Lerp(0.98824f, 0.95294f, fade),
                MathHelper.Lerp(0.20784f, 0.61961f, fade),
                MathHelper.Lerp(0.16863f, 0.41961f, fade),
                1f);
        }
    }
}
