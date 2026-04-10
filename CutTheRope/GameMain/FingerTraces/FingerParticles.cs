using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// Configuration for one reusable finger-trace particle emitter.
    /// </summary>
    /// <param name="FirstQuad">The first particle quad index in the finger-trace atlas.</param>
    /// <param name="QuadCount">The number of particle quads available starting at <paramref name="FirstQuad"/>.</param>
    /// <param name="AngleVarianceDegrees">The random angular spread in degrees around the emitter rotation.</param>
    /// <param name="Speed">The base particle launch speed.</param>
    /// <param name="SpeedVariance">The symmetric random speed delta applied around <paramref name="Speed"/>.</param>
    /// <param name="Life">The base lifetime in seconds for each spawned particle.</param>
    /// <param name="LifeVariance">The symmetric random lifetime delta applied around <paramref name="Life"/>.</param>
    /// <param name="StartScale">The base starting scale of each particle sprite.</param>
    /// <param name="StartScaleVariance">The symmetric random scale delta applied around <paramref name="StartScale"/>.</param>
    /// <param name="EndScale">The final scale approached at the end of the particle lifetime.</param>
    /// <param name="SpinDegrees">The base spin in degrees applied to non-velocity-aligned particles.</param>
    /// <param name="SpinVarianceDegrees">The symmetric random spin delta applied around <paramref name="SpinDegrees"/>.</param>
    /// <param name="SpinIsTotalDegreesOverLife">
    /// <see langword="true"/> to interpret <paramref name="SpinDegrees"/> as total rotation over the whole lifetime;
    /// otherwise as per-second rotation.
    /// </param>
    /// <param name="SpawnPositionVariance">The symmetric spawn-position jitter applied around the emitter position.</param>
    /// <param name="RadialAcceleration">The inward radial acceleration toward the spawn point after emission.</param>
    /// <param name="GravityY">The constant Y acceleration applied each frame.</param>
    /// <param name="BlendMode">The blend mode used for emitted particle sprites.</param>
    /// <param name="Alpha">The base alpha multiplier applied to emitted particle sprites.</param>
    /// <param name="FadeAlphaWithLife">
    /// <see langword="true"/> to scale sprite alpha by remaining life ratio; otherwise keep <paramref name="Alpha"/> constant.
    /// </param>
    /// <param name="RotateToVelocity">
    /// <see langword="true"/> to align particle rotation to velocity; otherwise advance the configured spin value.
    /// </param>
    /// <param name="Capacity">The maximum number of live particles retained by the emitter.</param>
    internal readonly record struct FingerParticlesConfig(
        int FirstQuad,
        int QuadCount,
        float AngleVarianceDegrees,
        float Speed,
        float SpeedVariance,
        float Life,
        float LifeVariance,
        float StartScale,
        float StartScaleVariance,
        float EndScale,
        float SpinDegrees,
        float SpinVarianceDegrees,
        bool SpinIsTotalDegreesOverLife,
        float SpawnPositionVariance,
        float RadialAcceleration,
        float GravityY,
        FingerTraceBlendMode BlendMode,
        float Alpha,
        bool FadeAlphaWithLife,
        bool RotateToVelocity,
        int Capacity = 100);

    /// <summary>
    /// Shared particle emitter used by named finger traces.
    /// </summary>
    /// <param name="config">The particle-emitter configuration that controls spawn and motion behavior.</param>
    internal sealed class FingerParticles(FingerParticlesConfig config) : FrameworkTypes
    {
        /// <summary>
        /// Live particles currently managed by the emitter.
        /// </summary>
        private readonly List<FingerParticle> particles = [];

        /// <summary>
        /// Immutable emitter configuration controlling spawn and update behavior.
        /// </summary>
        private readonly FingerParticlesConfig config = config;

        /// <summary>
        /// Current emitter position used for newly spawned particles.
        /// </summary>
        private Vector emitterPosition;

        /// <summary>
        /// Current emitter rotation in degrees used as the center emission direction.
        /// </summary>
        private float emitterRotation;

        /// <summary>
        /// Requested particle emission rate in particles per second.
        /// </summary>
        private float emissionRate;

        /// <summary>
        /// Accumulated elapsed time toward the next particle emission.
        /// </summary>
        private float emitCounter;

        /// <summary>
        /// Gets a value indicating whether live particles or active emission remain.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when the emitter still has live particles or a positive emission rate; otherwise, <see langword="false"/>.
        /// </value>
        public bool HasLiveParticles => emissionRate > 0f || particles.Count > 0;

        /// <summary>
        /// Clears all particles and resets the emitter.
        /// </summary>
        public void Reset()
        {
            particles.Clear();
            emissionRate = 0f;
            emitCounter = 0f;
        }

        /// <summary>
        /// Sets the position used for newly emitted particles.
        /// </summary>
        /// <param name="position">Emitter position for newly spawned particles.</param>
        public void SetPosition(Vector position)
        {
            emitterPosition = position;
        }

        /// <summary>
        /// Sets the center emission rotation in degrees.
        /// </summary>
        /// <param name="rotation">Emitter rotation in degrees.</param>
        public void SetRotation(float rotation)
        {
            emitterRotation = rotation;
        }

        /// <summary>
        /// Sets the requested particle emission rate in particles per second.
        /// </summary>
        /// <param name="rate">Requested emission rate in particles per second.</param>
        public void SetEmissionRate(float rate)
        {
            emissionRate = MAX(0f, rate);
        }

        /// <summary>
        /// Advances the emitter and all live particles for one frame.
        /// </summary>
        /// <param name="delta">Elapsed frame time in seconds.</param>
        public void Update(float delta)
        {
            if (emissionRate > 0f)
            {
                float emissionInterval = 1f / emissionRate;
                emitCounter += delta;
                while (particles.Count < config.Capacity && emitCounter > emissionInterval)
                {
                    particles.Add(CreateParticle());
                    emitCounter -= emissionInterval;
                }
            }

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                FingerParticle particle = particles[i];
                particle.Life -= delta;
                if (particle.Life <= 0f)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                if (config.RadialAcceleration != 0f)
                {
                    Vector toEmitter = VectSub(particle.SpawnPosition, particle.Position);
                    float distance = VectLength(toEmitter);
                    if (distance > 0.0001f)
                    {
                        Vector radialDir = VectDiv(toEmitter, distance);
                        particle.Velocity = VectAdd(
                            particle.Velocity,
                            VectMult(radialDir, config.RadialAcceleration * delta));
                    }
                }

                if (config.GravityY != 0f)
                {
                    particle.Velocity = new Vector(particle.Velocity.X, particle.Velocity.Y + (config.GravityY * delta));
                }

                particle.Position = VectAdd(particle.Position, VectMult(particle.Velocity, delta));
                if (config.RotateToVelocity)
                {
                    particle.Rotation = RADIANS_TO_DEGREES(MathF.Atan2(particle.Velocity.Y, particle.Velocity.X) + 1.5708f);
                }
                else
                {
                    particle.Rotation += particle.RotationVelocity * delta;
                }

                particles[i] = particle;
            }
        }

        /// <summary>
        /// Appends the current particle visuals as trace snapshot sprites.
        /// </summary>
        /// <param name="sprites">Destination list that receives particle sprite poses.</param>
        public void AppendSprites(List<FingerTraceSpritePose> sprites)
        {
            foreach (FingerParticle particle in particles)
            {
                float lifeRatio = FIT_TO_BOUNDARIES(particle.Life / particle.MaxLife, 0f, 1f);
                float scale = particle.StartScale + ((particle.EndScale - particle.StartScale) * (1f - lifeRatio));
                float alpha = config.FadeAlphaWithLife
                    ? config.Alpha * lifeRatio
                    : config.Alpha;

                sprites.Add(new FingerTraceSpritePose(
                    FingerTraceSpriteKind.Spark,
                    Resources.Img.FingerTraces,
                    particle.QuadIndex,
                    particle.Position,
                    particle.Rotation,
                    scale,
                    alpha,
                    config.BlendMode));
            }
        }

        /// <summary>
        /// Creates one new particle from the current emitter state and configuration.
        /// </summary>
        /// <returns>The initialized particle state for the next emitted particle.</returns>
        private FingerParticle CreateParticle()
        {
            float angle = DEGREES_TO_RADIANS(emitterRotation + (config.AngleVarianceDegrees * RND_MINUS1_1));
            Vector direction = new(Cosf(angle), Sinf(angle));
            float speed = config.Speed + (config.SpeedVariance * RND_MINUS1_1);
            float life = MAX(0.05f, config.Life + (config.LifeVariance * RND_MINUS1_1));
            float startScale = config.StartScale + (config.StartScaleVariance * RND_MINUS1_1);
            float spinDegrees = config.SpinDegrees + (config.SpinVarianceDegrees * RND_MINUS1_1);
            Vector spawnPos = new(
                emitterPosition.X + (config.SpawnPositionVariance * RND_MINUS1_1),
                emitterPosition.Y + (config.SpawnPositionVariance * RND_MINUS1_1));

            return new FingerParticle
            {
                Position = spawnPos,
                SpawnPosition = spawnPos,
                Velocity = VectMult(direction, speed),
                Rotation = config.RotateToVelocity
                    ? RADIANS_TO_DEGREES(MathF.Atan2(direction.Y, direction.X) + 1.5708f)
                    : 0f,
                RotationVelocity = config.RotateToVelocity
                    ? 0f
                    : config.SpinIsTotalDegreesOverLife
                    ? DEGREES_TO_RADIANS(spinDegrees) / life
                    : DEGREES_TO_RADIANS(spinDegrees),
                StartScale = startScale,
                EndScale = config.EndScale,
                Life = life,
                MaxLife = life,
                QuadIndex = config.FirstQuad + NextInt(config.QuadCount),
            };
        }

        /// <summary>
        /// Returns a random integer in the range <c>[0, <paramref name="upperExclusive"/>)</c>.
        /// </summary>
        /// <param name="upperExclusive">The exclusive upper bound.</param>
        /// <returns>A random integer less than <paramref name="upperExclusive"/>.</returns>
        private static int NextInt(int upperExclusive)
        {
            return upperExclusive <= 1
                ? 0
                : (int)(Arc4random() % (uint)upperExclusive);
        }

        /// <summary>
        /// Stores the transient state of one live particle managed by <see cref="FingerParticles"/>.
        /// </summary>
        private struct FingerParticle
        {
            /// <summary>The current particle position.</summary>
            public Vector Position;

            /// <summary>The original spawn position used by radial acceleration calculations.</summary>
            public Vector SpawnPosition;

            /// <summary>The current particle velocity.</summary>
            public Vector Velocity;

            /// <summary>The current particle rotation.</summary>
            public float Rotation;

            /// <summary>The current angular velocity.</summary>
            public float RotationVelocity;

            /// <summary>The initial particle scale.</summary>
            public float StartScale;

            /// <summary>The final particle scale at the end of life.</summary>
            public float EndScale;

            /// <summary>The remaining particle lifetime in seconds.</summary>
            public float Life;

            /// <summary>The starting particle lifetime in seconds.</summary>
            public float MaxLife;

            /// <summary>The selected atlas quad index.</summary>
            public int QuadIndex;
        }
    }
}
