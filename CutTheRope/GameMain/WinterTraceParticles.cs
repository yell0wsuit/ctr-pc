using System;
using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Lightweight particle emitter used by <see cref="WinterFingerTrace"/> for the CTR2 winter trail.
    /// </summary>
    internal sealed class WinterTraceParticles : FrameworkTypes
    {
        private const int Capacity = 100;
        private const int FirstQuad = 9;
        private const int QuadCount = 5;

        private readonly List<WinterParticle> particles = [];

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
                WinterParticle particle = particles[i];
                particle.Life -= delta;
                if (particle.Life <= 0f)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                particle.Position = VectAdd(particle.Position, VectMult(particle.Velocity, delta));
                particle.Rotation += particle.RotationVelocity * delta;
                particles[i] = particle;
            }
        }

        /// <summary>
        /// Appends the current particle visuals as trace snapshot sprites.
        /// </summary>
        public void AppendSprites(List<FingerTraceSpritePose> sprites)
        {
            foreach (WinterParticle particle in particles)
            {
                float alpha = FIT_TO_BOUNDARIES(particle.Life / particle.MaxLife, 0f, 1f);
                sprites.Add(new FingerTraceSpritePose(
                    FingerTraceSpriteKind.Spark,
                    Resources.Img.FingerTraces,
                    particle.QuadIndex,
                    particle.Position,
                    particle.Rotation,
                    particle.Scale,
                    alpha,
                    FingerTraceBlendMode.Alpha));
            }
        }

        private WinterParticle CreateParticle()
        {
            float angle = DEGREES_TO_RADIANS(emitterRotation + (90f * RND_MINUS1_1));
            Vector direction = new(Cosf(angle), Sinf(angle));
            float speed = 180f + (200f * RND_MINUS1_1);
            float life = MAX(0.35f, 1f + (0.1f * RND_MINUS1_1));

            return new WinterParticle
            {
                Position = emitterPosition,
                Velocity = VectMult(direction, speed),
                Rotation = emitterRotation + (45f * RND_MINUS1_1),
                RotationVelocity = 90f * RND_MINUS1_1,
                Scale = MAX(0.1f, 0.25f + (0.15f * RND_MINUS1_1)),
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

        private struct WinterParticle
        {
            public Vector Position;
            public Vector Velocity;
            public float Rotation;
            public float RotationVelocity;
            public float Scale;
            public float Life;
            public float MaxLife;
            public int QuadIndex;
        }
    }
}
