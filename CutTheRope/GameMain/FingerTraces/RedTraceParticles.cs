using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// Lightweight particle emitter used by <see cref="RedFingerTrace"/> for the CTR2 (PRODUCT)RED trail.
    /// Supports configurable alpha and blend mode so two instances can serve as glow and core layers.
    /// </summary>
    /// <remarks>
    /// Creates a red trace particle emitter with the specified alpha and blend mode.
    /// </remarks>
    /// <param name="alpha">Constant particle alpha (0.3 for glow, 0.75 for core).</param>
    /// <param name="blend">Blend mode for rendering.</param>
    internal sealed class RedTraceParticles(float alpha, FingerTraceBlendMode blend) : FrameworkTypes
    {
        private const int Capacity = 100;
        private const int FirstQuad = 27;
        private const int QuadCount = 3;
        private const float GravityY = 600f;

        private readonly List<RedParticle> particles = [];
        private readonly float baseAlpha = alpha;
        private readonly FingerTraceBlendMode blendMode = blend;

        private Vector emitterPosition;
        private float emitterRotation;
        private float emissionRate;
        private float emitCounter;

        /// <summary>
        /// Gets a value indicating whether live particles or active emission remain.
        /// </summary>
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
        public void SetPosition(Vector position)
        {
            emitterPosition = position;
        }

        /// <summary>
        /// Sets the center emission rotation in degrees.
        /// </summary>
        public void SetRotation(float rotation)
        {
            emitterRotation = rotation;
        }

        /// <summary>
        /// Sets the requested particle emission rate in particles per second.
        /// </summary>
        public void SetEmissionRate(float rate)
        {
            emissionRate = MAX(0f, rate);
        }

        /// <summary>
        /// Advances the emitter and all live particles for one frame.
        /// </summary>
        public void Update(float delta)
        {
            if (emissionRate > 0f)
            {
                float emissionInterval = 1f / emissionRate;
                emitCounter += delta;
                while (particles.Count < Capacity && emitCounter > emissionInterval)
                {
                    particles.Add(CreateParticle());
                    emitCounter -= emissionInterval;
                }
            }

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                RedParticle particle = particles[i];
                particle.Life -= delta;
                if (particle.Life <= 0f)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                particle.Velocity = new Vector(particle.Velocity.X, particle.Velocity.Y + (GravityY * delta));
                particle.Position = VectAdd(particle.Position, VectMult(particle.Velocity, delta));
                particle.Rotation = RADIANS_TO_DEGREES(MathF.Atan2(particle.Velocity.Y, particle.Velocity.X) + 1.5708f);
                particles[i] = particle;
            }
        }

        /// <summary>
        /// Appends the current particle visuals as trace snapshot sprites.
        /// </summary>
        public void AppendSprites(List<FingerTraceSpritePose> sprites)
        {
            foreach (RedParticle particle in particles)
            {
                float lifeRatio = FIT_TO_BOUNDARIES(particle.Life / particle.MaxLife, 0f, 1f);
                float scale = particle.StartScale + ((particle.EndScale - particle.StartScale) * (1f - lifeRatio));
                sprites.Add(new FingerTraceSpritePose(
                    FingerTraceSpriteKind.Spark,
                    Resources.Img.FingerTraces,
                    particle.QuadIndex,
                    particle.Position,
                    particle.Rotation,
                    scale,
                    baseAlpha,
                    blendMode));
            }
        }

        private RedParticle CreateParticle()
        {
            float angle = DEGREES_TO_RADIANS(emitterRotation + (70f * RND_MINUS1_1));
            Vector direction = new(Cosf(angle), Sinf(angle));
            float speed = 250f + (20f * RND_MINUS1_1);
            float life = MAX(0.05f, 0.45f + (0.2f * RND_MINUS1_1));
            float startScale = 1.0f + (0.6f * RND_MINUS1_1);

            return new RedParticle
            {
                Position = emitterPosition,
                Velocity = VectMult(direction, speed),
                Rotation = RADIANS_TO_DEGREES(MathF.Atan2(direction.Y, direction.X) + 1.5708f),
                StartScale = startScale,
                EndScale = 0f,
                Life = life,
                MaxLife = life,
                QuadIndex = FirstQuad + NextInt(QuadCount),
            };
        }

        private static int NextInt(int upperExclusive)
        {
            return upperExclusive <= 1
                ? 0
                : (int)(Arc4random() % (uint)upperExclusive);
        }

        private struct RedParticle
        {
            public Vector Position;
            public Vector Velocity;
            public float Rotation;
            public float StartScale;
            public float EndScale;
            public float Life;
            public float MaxLife;
            public int QuadIndex;
        }
    }
}
