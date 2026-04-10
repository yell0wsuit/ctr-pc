using CutTheRope.Framework;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style star finger trace with a rainbow ribbon and colorful star particles.
    /// </summary>
    internal sealed class StarFingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes a star finger trace.
        /// </summary>
        public StarFingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 50f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: 0,
                glowTranslateY: 48f,
                NamedTracePresets.CreateStarParticles())
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            if (t < 0.33f)
            {
                float blend = t * 3f;
                return RGBAColor.MakeRGBA(
                    1f,
                    MathHelper.Lerp(0.30196f, 0.64314f, blend),
                    MathHelper.Lerp(0.99216f, 0.29412f, blend),
                    1f);
            }

            if (t < 0.66f)
            {
                float blend = (t - 0.33f) * 3f;
                return RGBAColor.MakeRGBA(
                    1f,
                    MathHelper.Lerp(0.64314f, 0.95294f, blend),
                    MathHelper.Lerp(0.29412f, 0.20392f, blend),
                    1f);
            }

            float fade = (t - 0.66f) * 3f;
            return RGBAColor.MakeRGBA(
                1f,
                MathHelper.Lerp(0.95294f, 1f, fade),
                MathHelper.Lerp(0.20392f, 1f, fade),
                1f);
        }
    }
}
