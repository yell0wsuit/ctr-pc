using CutTheRope.Framework;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style Easter finger trace with a pastel ribbon, glow overlay, and egg-themed particles.
    /// </summary>
    internal sealed class EasterFingerTrace : RibbonFingerTrace
    {
        /// <summary>
        /// Initializes an Easter finger trace.
        /// </summary>
        public EasterFingerTrace()
            : base(
                segmentLife: 0.15f,
                particleBurstDuration: 0.1f,
                particleEmissionRate: 50f,
                ribbonBaseWidth: 12f,
                minimumRibbonHalfWidth: 1f,
                glowQuadIndex: 2,
                glowTranslateY: 50f,
                NamedTracePresets.CreateAlphaParticles(52, 10))
        {
        }

        /// <inheritdoc />
        protected override RGBAColor GetRibbonColor(float t)
        {
            if (t < 0.35f)
            {
                float blend = t * 3f;
                return RGBAColor.MakeRGBA(
                    MathHelper.Lerp(0.14118f, 1f, blend),
                    MathHelper.Lerp(0.81961f, 1f, blend),
                    MathHelper.Lerp(0.87451f, 1f, blend),
                    1f);
            }

            return RGBAColor.MakeRGBA(1f, 1f, 1f, 1f);
        }
    }
}
