using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// Lightweight spark emitter used by <see cref="LightningFingerTrace"/> to reproduce the CTR2 lightning trail.
    /// </summary>
    internal sealed class LightningTraceParticles : FrameworkTypes
    {
        private const int Capacity = 100;
        private const int FirstQuad = 24;
        private const int QuadCount = 3;

        private readonly List<SparkParticle> particles = [];

        private Vector emitterPosition;
        private float emitterRotation;
        private float emissionRate;
        private float emitCounter;

        /// <summary>
        /// Gets a value indicating whether emission is active or live spark particles remain.
        /// </summary>
        public bool HasLiveParticles => emissionRate > 0f || particles.Count > 0;

        /// <summary>
        /// Clears all live particles and resets emission state.
        /// </summary>
        public void Reset()
        {
            particles.Clear();
            emitCounter = 0f;
            emissionRate = 0f;
        }

        /// <summary>
        /// Sets the emitter position used for newly spawned sparks.
        /// </summary>
        /// <param name="position">Emitter position in world space.</param>
        public void SetPosition(Vector position)
        {
            emitterPosition = position;
        }

        /// <summary>
        /// Sets the emitter rotation used as the center angle for newly spawned sparks.
        /// </summary>
        /// <param name="rotation">Emitter rotation in degrees.</param>
        public void SetRotation(float rotation)
        {
            emitterRotation = rotation;
        }

        /// <summary>
        /// Sets the requested spark emission rate in particles per second.
        /// </summary>
        /// <param name="rate">Requested emission rate.</param>
        public void SetEmissionRate(float rate)
        {
            emissionRate = MAX(0f, rate);
        }

        /// <summary>
        /// Advances spark emission and moves all live particles for one frame.
        /// </summary>
        /// <param name="delta">Elapsed time in seconds.</param>
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
                SparkParticle particle = particles[i];
                particle.Life -= delta;
                if (particle.Life <= 0f)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                particle.Position = VectAdd(particle.Position, VectMult(particle.Velocity, delta));
                particle.Rotation = RADIANS_TO_DEGREES(MathF.Atan2(particle.Velocity.Y, particle.Velocity.X) + 1.5708f);
                particles[i] = particle;
            }
        }

        /// <summary>
        /// Appends the currently live sparks as sprite poses for snapshot rendering or testing.
        /// </summary>
        /// <param name="sprites">Destination sprite list.</param>
        public void AppendSprites(List<FingerTraceSpritePose> sprites)
        {
            foreach (SparkParticle particle in particles)
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
                    lifeRatio,
                    FingerTraceBlendMode.Additive));
            }
        }

        private SparkParticle CreateParticle()
        {
            float angle = DEGREES_TO_RADIANS(emitterRotation + (70f * RND_MINUS1_1));
            Vector direction = new(Cosf(angle), Sinf(angle));
            float speed = 500f + (20f * RND_MINUS1_1);
            float life = MAX(0.05f, 0.25f + (0.05f * RND_MINUS1_1));

            return new SparkParticle
            {
                Position = emitterPosition,
                Velocity = VectMult(direction, speed),
                Rotation = RADIANS_TO_DEGREES(MathF.Atan2(direction.Y, direction.X) + 1.5708f),
                StartScale = 2.0f,
                EndScale = 0.1f,
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

        private struct SparkParticle
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
