using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// Lightweight particle emitter used by <see cref="WinterFingerTrace"/> for the CTR2 winter trail.
    /// </summary>
    internal sealed class WinterTraceParticles : FrameworkTypes
    {
        private const int Capacity = 100;
        private const int FirstQuad = 9;
        private const int QuadCount = 5;
        private const float RadialAcceleration = -200f;
        private const float GravityY = 500f;

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

                Vector toEmitter = VectSub(particle.SpawnPosition, particle.Position);
                float distance = VectLength(toEmitter);
                if (distance > 0.0001f)
                {
                    Vector radialDir = VectDiv(toEmitter, distance);
                    particle.Velocity = VectAdd(particle.Velocity, VectMult(radialDir, RadialAcceleration * delta));
                }

                particle.Velocity = new Vector(particle.Velocity.X, particle.Velocity.Y + (GravityY * delta));
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

        private WinterParticle CreateParticle()
        {
            float angle = DEGREES_TO_RADIANS(emitterRotation + (90f * RND_MINUS1_1));
            Vector direction = new(Cosf(angle), Sinf(angle));
            float speed = 200f + (20f * RND_MINUS1_1);
            float life = MAX(0.05f, 0.6f + (0.2f * RND_MINUS1_1));

            return new WinterParticle
            {
                Position = emitterPosition,
                SpawnPosition = emitterPosition,
                Velocity = VectMult(direction, speed),
                Rotation = 0f,
                RotationVelocity = DEGREES_TO_RADIANS(180f * RND_MINUS1_1),
                StartScale = 1.0f,
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

        private struct WinterParticle
        {
            public Vector Position;
            public Vector SpawnPosition;
            public Vector Velocity;
            public float Rotation;
            public float RotationVelocity;
            public float StartScale;
            public float EndScale;
            public float Life;
            public float MaxLife;
            public int QuadIndex;
        }
    }
}
