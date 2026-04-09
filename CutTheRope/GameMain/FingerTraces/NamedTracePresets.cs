using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// Shared particle and gradient presets for thin named finger traces.
    /// </summary>
    internal static class NamedTracePresets
    {
        /// <summary>
        /// Creates the preset particle emitter used by the bubble trace.
        /// </summary>
        /// <returns>The configured bubble particle emitter.</returns>
        public static FingerParticles CreateBubbleParticles()
        {
            return new FingerParticles(new FingerParticlesConfig(
                FirstQuad: 5,
                QuadCount: 4,
                AngleVarianceDegrees: 45f,
                Speed: 200f,
                SpeedVariance: 20f,
                Life: 0.6f,
                LifeVariance: 0.2f,
                StartScale: 1f,
                StartScaleVariance: 0.3f,
                EndScale: 0.1f,
                SpinDegrees: 0f,
                SpinVarianceDegrees: 180f,
                SpinIsTotalDegreesOverLife: true,
                SpawnPositionVariance: 5f,
                RadialAcceleration: -200f,
                GravityY: 0f,
                BlendMode: FingerTraceBlendMode.Additive,
                Alpha: 1f,
                FadeAlphaWithLife: true,
                RotateToVelocity: false));
        }

        /// <summary>
        /// Creates the preset particle emitter used by the winter trace.
        /// </summary>
        /// <returns>The configured winter particle emitter.</returns>
        public static FingerParticles CreateWinterParticles()
        {
            return new FingerParticles(new FingerParticlesConfig(
                FirstQuad: 9,
                QuadCount: 5,
                AngleVarianceDegrees: 90f,
                Speed: 200f,
                SpeedVariance: 20f,
                Life: 0.6f,
                LifeVariance: 0.2f,
                StartScale: 1f,
                StartScaleVariance: 0f,
                EndScale: 0.1f,
                SpinDegrees: 0f,
                SpinVarianceDegrees: 180f,
                SpinIsTotalDegreesOverLife: false,
                SpawnPositionVariance: 0f,
                RadialAcceleration: -200f,
                GravityY: 500f,
                BlendMode: FingerTraceBlendMode.Additive,
                Alpha: 1f,
                FadeAlphaWithLife: true,
                RotateToVelocity: false));
        }

        /// <summary>
        /// Creates the preset particle emitter used by the star trace.
        /// </summary>
        /// <returns>The configured star particle emitter.</returns>
        public static FingerParticles CreateStarParticles()
        {
            return new FingerParticles(new FingerParticlesConfig(
                FirstQuad: 0,
                QuadCount: 5,
                AngleVarianceDegrees: 60f,
                Speed: 500f,
                SpeedVariance: 200f,
                Life: 0.6f,
                LifeVariance: 0.2f,
                StartScale: 2f,
                StartScaleVariance: 0.5f,
                EndScale: 0.1f,
                SpinDegrees: 600f,
                SpinVarianceDegrees: 180f,
                SpinIsTotalDegreesOverLife: true,
                SpawnPositionVariance: 5f,
                RadialAcceleration: -200f,
                GravityY: 0f,
                BlendMode: FingerTraceBlendMode.Additive,
                Alpha: 1f,
                FadeAlphaWithLife: true,
                RotateToVelocity: false));
        }

        /// <summary>
        /// Creates the shared alpha-blended particle preset used by several named traces.
        /// </summary>
        /// <param name="firstQuad">The first atlas quad index used by the preset.</param>
        /// <param name="quadCount">The number of atlas quads available starting at <paramref name="firstQuad"/>.</param>
        /// <returns>The configured alpha-blended particle emitter.</returns>
        public static FingerParticles CreateAlphaParticles(int firstQuad, int quadCount)
        {
            return new FingerParticles(new FingerParticlesConfig(
                FirstQuad: firstQuad,
                QuadCount: quadCount,
                AngleVarianceDegrees: 60f,
                Speed: 500f,
                SpeedVariance: 200f,
                Life: 0.6f,
                LifeVariance: 0.2f,
                StartScale: 1f,
                StartScaleVariance: 0.5f,
                EndScale: 0.1f,
                SpinDegrees: 600f,
                SpinVarianceDegrees: 180f,
                SpinIsTotalDegreesOverLife: true,
                SpawnPositionVariance: 5f,
                RadialAcceleration: -200f,
                GravityY: 0f,
                BlendMode: FingerTraceBlendMode.Alpha,
                Alpha: 1f,
                FadeAlphaWithLife: true,
                RotateToVelocity: false));
        }

        /// <summary>
        /// Creates one of the two layered particle emitters used by the red trace.
        /// </summary>
        /// <param name="alpha">The constant alpha multiplier applied to the emitted sprites.</param>
        /// <param name="blendMode">The blend mode used to draw the emitted sprites.</param>
        /// <returns>The configured red particle emitter.</returns>
        public static FingerParticles CreateRedParticles(float alpha, FingerTraceBlendMode blendMode)
        {
            return new FingerParticles(new FingerParticlesConfig(
                FirstQuad: 27,
                QuadCount: 3,
                AngleVarianceDegrees: 70f,
                Speed: 250f,
                SpeedVariance: 20f,
                Life: 0.45f,
                LifeVariance: 0.2f,
                StartScale: 1f,
                StartScaleVariance: 0.6f,
                EndScale: 0f,
                SpinDegrees: 0f,
                SpinVarianceDegrees: 0f,
                SpinIsTotalDegreesOverLife: false,
                SpawnPositionVariance: 0f,
                RadialAcceleration: 0f,
                GravityY: 600f,
                BlendMode: blendMode,
                Alpha: alpha,
                FadeAlphaWithLife: false,
                RotateToVelocity: true));
        }

        /// <summary>
        /// Creates the preset spark emitter used by the lightning trace.
        /// </summary>
        /// <returns>The configured lightning particle emitter.</returns>
        public static FingerParticles CreateLightningParticles()
        {
            return new FingerParticles(new FingerParticlesConfig(
                FirstQuad: 24,
                QuadCount: 3,
                AngleVarianceDegrees: 70f,
                Speed: 500f,
                SpeedVariance: 20f,
                Life: 0.25f,
                LifeVariance: 0.05f,
                StartScale: 2f,
                StartScaleVariance: 0f,
                EndScale: 0.1f,
                SpinDegrees: 0f,
                SpinVarianceDegrees: 0f,
                SpinIsTotalDegreesOverLife: false,
                SpawnPositionVariance: 0f,
                RadialAcceleration: 0f,
                GravityY: 0f,
                BlendMode: FingerTraceBlendMode.Additive,
                Alpha: 1f,
                FadeAlphaWithLife: true,
                RotateToVelocity: true));
        }

        /// <summary>
        /// Returns the shared golden ribbon gradient used by multiple named traces.
        /// </summary>
        /// <param name="t">The normalized ribbon position in the range <c>[0, 1]</c>.</param>
        /// <param name="clampBlend">
        /// <see langword="true"/> to clamp the initial fade blend into the valid range; otherwise use the raw scaled value.
        /// </param>
        /// <returns>The golden ribbon color at <paramref name="t"/>.</returns>
        public static RGBAColor GetGoldenRibbonColor(float t, bool clampBlend = false)
        {
            if (t >= 0.5f)
            {
                return RGBAColor.MakeRGBA(1f, 1f, 1f, 1f);
            }

            float blend = clampBlend
                ? MathHelper.Clamp(t * 3f, 0f, 1f)
                : t * 3f;

            return RGBAColor.MakeRGBA(
                1f,
                MathHelper.Lerp(0.87451f, 1f, blend),
                MathHelper.Lerp(0.05490f, 1f, blend),
                1f);
        }
    }
}
