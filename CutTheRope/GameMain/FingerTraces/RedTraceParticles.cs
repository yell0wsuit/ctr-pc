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
        /// <summary>The maximum number of live particles the emitter keeps at once.</summary>
        private const int Capacity = 100;

        /// <summary>The first particle quad index in the finger-trace atlas.</summary>
        private const int FirstQuad = 27;

        /// <summary>The number of particle quads available in the atlas.</summary>
        private const int QuadCount = 3;

        /// <summary>The downward acceleration applied to live particles.</summary>
        private const float GravityY = 600f;

        /// <summary>The live particle list in age order.</summary>
        private readonly List<RedParticle> particles = [];

        /// <summary>The constant alpha used for emitted sprites.</summary>
        private readonly float baseAlpha = alpha;

        /// <summary>The blend mode used when drawing emitted sprites.</summary>
        private readonly FingerTraceBlendMode blendMode = blend;

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
        /// <param name="sprites">Destination list that receives particle sprite poses.</param>
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

        /// <summary>
        /// Creates a newly emitted red particle from the current emitter state.
        /// </summary>
        /// <returns>The initialized particle state.</returns>
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
        /// Stores the state of one live red particle.
        /// </summary>
        private struct RedParticle
        {
            /// <summary>The current particle position.</summary>
            public Vector Position;

            /// <summary>The current particle velocity.</summary>
            public Vector Velocity;

            /// <summary>The current particle rotation in degrees.</summary>
            public float Rotation;

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
