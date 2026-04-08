using System.Collections.Generic;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// Lightweight particle emitter used by <see cref="OmnomBirthdayFingerTrace"/> for the CTR2 birthday trail.
    /// </summary>
    internal sealed class OmnomBirthdayTraceParticles : FrameworkTypes
    {
        /// <summary>The maximum number of live particles the emitter keeps at once.</summary>
        private const int Capacity = 100;

        /// <summary>The first particle quad index in the finger-trace atlas.</summary>
        private const int FirstQuad = 43;

        /// <summary>The number of particle quads available in the atlas.</summary>
        private const int QuadCount = 9;

        /// <summary>The spawn-position jitter applied around the emitter.</summary>
        private const float PositionVariance = 5f;

        /// <summary>The inward radial acceleration applied after spawn.</summary>
        private const float RadialAcceleration = -200f;

        /// <summary>The live particle list in age order.</summary>
        private readonly List<Particle> particles = [];

        /// <summary>The current emitter position used for newly spawned particles.</summary>
        private Vector emitterPosition;

        /// <summary>The center emission rotation in degrees.</summary>
        private float emitterRotation;

        /// <summary>The requested particle emission rate in particles per second.</summary>
        private float emissionRate;

        /// <summary>The accumulated time toward the next emitted particle.</summary>
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
                while (particles.Count < Capacity && emitCounter > emissionInterval)
                {
                    particles.Add(CreateParticle());
                    emitCounter -= emissionInterval;
                }
            }

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle particle = particles[i];
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

                particle.Position = VectAdd(particle.Position, VectMult(particle.Velocity, delta));
                particle.Rotation += particle.RotationVelocity * delta;
                particles[i] = particle;
            }
        }

        /// <summary>
        /// Appends the current particle visuals as trace snapshot sprites.
        /// </summary>
        /// <param name="sprites">Destination list that receives particle sprite poses.</param>
        public void AppendSprites(List<FingerTraceSpritePose> sprites)
        {
            foreach (Particle particle in particles)
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
                    FingerTraceBlendMode.Alpha));
            }
        }

        /// <summary>
        /// Creates a newly emitted birthday particle from the current emitter state.
        /// </summary>
        /// <returns>The initialized particle state.</returns>
        private Particle CreateParticle()
        {
            float angle = DEGREES_TO_RADIANS(emitterRotation + (60f * RND_MINUS1_1));
            Vector direction = new(Cosf(angle), Sinf(angle));
            float speed = 500f + (200f * RND_MINUS1_1);
            float life = MAX(0.05f, 0.6f + (0.2f * RND_MINUS1_1));
            float startScale = 1.0f + (0.5f * RND_MINUS1_1);
            float endSpinDeg = 600f + (180f * RND_MINUS1_1);
            Vector spawnPos = new(
                emitterPosition.X + (PositionVariance * RND_MINUS1_1),
                emitterPosition.Y + (PositionVariance * RND_MINUS1_1));

            return new Particle
            {
                Position = spawnPos,
                SpawnPosition = spawnPos,
                Velocity = VectMult(direction, speed),
                Rotation = 0f,
                RotationVelocity = DEGREES_TO_RADIANS(endSpinDeg) / life,
                StartScale = startScale,
                EndScale = 0.1f,
                Life = life,
                MaxLife = life,
                QuadIndex = FirstQuad + NextInt(QuadCount),
            };
        }

        /// <summary>
        /// Returns a random integer in the range <c>[0, upperExclusive)</c>.
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
        /// Stores the state of one live birthday particle.
        /// </summary>
        private struct Particle
        {
            /// <summary>The current particle position.</summary>
            public Vector Position;

            /// <summary>The original spawn position used for radial acceleration.</summary>
            public Vector SpawnPosition;

            /// <summary>The current particle velocity.</summary>
            public Vector Velocity;

            /// <summary>The current particle rotation in radians.</summary>
            public float Rotation;

            /// <summary>The per-second particle rotation speed.</summary>
            public float RotationVelocity;

            /// <summary>The initial sprite scale.</summary>
            public float StartScale;

            /// <summary>The final sprite scale at the end of life.</summary>
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
