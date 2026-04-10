using CutTheRopeDX.Framework.Core;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that emits and simulates point-sprite particles with gravity, acceleration, and color transitions.
    /// </summary>
    internal class Particles : BaseElement
    {
        /// <summary>
        /// Rotates a vector around a center point using precomputed cos/sin values.
        /// </summary>
        /// <param name="v">Vector to rotate.</param>
        /// <param name="cosA">Cosine of the rotation angle.</param>
        /// <param name="sinA">Sine of the rotation angle.</param>
        /// <param name="cx">Center X.</param>
        /// <param name="cy">Center Y.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector RotatePreCalc(Vector v, float cosA, float sinA, float cx, float cy)
        {
            Vector result = v;
            result.X -= cx;
            result.Y -= cy;
            float rotatedX = (result.X * cosA) - (result.Y * sinA);
            float rotatedY = (result.X * sinA) + (result.Y * cosA);
            result.X = rotatedX + cx;
            result.Y = rotatedY + cy;
            return result;
        }

        /// <summary>
        /// Updates a single particle's physics, color, and lifetime. Removes it if dead.
        /// </summary>
        /// <param name="p">Particle to update.</param>
        /// <param name="delta">Elapsed time in seconds.</param>
        public virtual void UpdateParticle(ref Particle p, float delta)
        {
            if (p.life > 0f)
            {
                Vector vector = vectZero;
                if (p.pos.X != 0f || p.pos.Y != 0f)
                {
                    vector = VectNormalize(p.pos);
                }
                Vector v = vector;
                vector = VectMult(vector, p.radialAccel);
                float tangentX = v.X;
                v.X = 0f - v.Y;
                v.Y = tangentX;
                v = VectMult(v, p.tangentialAccel);
                Vector v2 = VectAdd(VectAdd(vector, v), gravity);
                v2 = VectMult(v2, delta);
                p.dir = VectAdd(p.dir, v2);
                v2 = VectMult(p.dir, delta);
                p.pos = VectAdd(p.pos, v2);
                p.color.RedColor += p.deltaColor.RedColor * delta;
                p.color.GreenColor += p.deltaColor.GreenColor * delta;
                p.color.BlueColor += p.deltaColor.BlueColor * delta;
                p.color.AlphaChannel += p.deltaColor.AlphaChannel * delta;
                p.life -= delta;
                vertices[particleIdx].x = p.pos.X;
                vertices[particleIdx].y = p.pos.Y;
                vertices[particleIdx].size = p.size;
                colors[particleIdx] = p.color;
                particleIdx++;
                return;
            }
            if (particleIdx != particleCount - 1)
            {
                particles[particleIdx] = particles[particleCount - 1];
            }
            particleCount--;
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            if (particlesDelegate != null && particleCount == 0 && !active)
            {
                particlesDelegate(this);
                return;
            }
            if (vertices == null)
            {
                return;
            }
            if (active && emissionRate != 0f)
            {
                float rate = 1f / emissionRate;
                emitCounter += delta;
                while (particleCount < totalParticles && emitCounter > rate)
                {
                    _ = AddParticle();
                    emitCounter -= rate;
                }
                elapsed += delta;
                if (duration != -1f && duration < elapsed)
                {
                    StopSystem();
                }
            }
            particleIdx = 0;
            while (particleIdx < particleCount)
            {
                UpdateParticle(ref particles[particleIdx], delta);
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                particles = null;
                vertices = null;
                colors = null;
                texture = null;
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            PostDraw();
        }

        /// <summary>
        /// Initializes the particle system with the specified capacity.
        /// </summary>
        /// <param name="numberOfParticles">Maximum number of particles.</param>
        /// <returns>The initialized particle system, or <see langword="null"/> if allocation fails.</returns>
        public virtual Particles InitWithTotalParticles(int numberOfParticles)
        {
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
            totalParticles = numberOfParticles;
            particles = new Particle[totalParticles];
            vertices = new PointSprite[totalParticles];
            colors = new RGBAColor[totalParticles];
            if (particles == null || vertices == null || colors == null)
            {
                particles = null;
                vertices = null;
                colors = null;
                return null;
            }
            active = false;
            blendAdditive = false;
            return this;
        }

        /// <summary>
        /// Adds a new particle if the system is not full. Returns <see langword="true"/> if added.
        /// </summary>
        /// <returns><see langword="true"/> when a particle was added; otherwise <see langword="false"/>.</returns>
        public virtual bool AddParticle()
        {
            if (IsFull())
            {
                return false;
            }
            InitParticle(ref particles[particleCount]);
            particleCount++;
            return true;
        }

        /// <summary>
        /// Initializes a <paramref name="particle"/> with randomized values based on emitter settings.
        /// </summary>
        /// <param name="particle">Particle to initialize.</param>
        public virtual void InitParticle(ref Particle particle)
        {
            particle.pos.X = x + (posVar.X * RND_MINUS1_1);
            particle.pos.Y = y + (posVar.Y * RND_MINUS1_1);
            particle.startPos = particle.pos;
            float angleRad = DEGREES_TO_RADIANS(angle + (angleVar * RND_MINUS1_1));
            Vector v = default;
            v.Y = Sinf(angleRad);
            v.X = Cosf(angleRad);
            float s = speed + (speedVar * RND_MINUS1_1);
            particle.dir = VectMult(v, s);
            particle.radialAccel = radialAccel + (radialAccelVar * RND_MINUS1_1);
            particle.tangentialAccel = tangentialAccel + (tangentialAccelVar * RND_MINUS1_1);
            particle.life = life + (lifeVar * RND_MINUS1_1);
            RGBAColor rgbaColor = default;
            rgbaColor.RedColor = startColor.RedColor + (startColorVar.RedColor * RND_MINUS1_1);
            rgbaColor.GreenColor = startColor.GreenColor + (startColorVar.GreenColor * RND_MINUS1_1);
            rgbaColor.BlueColor = startColor.BlueColor + (startColorVar.BlueColor * RND_MINUS1_1);
            rgbaColor.AlphaChannel = startColor.AlphaChannel + (startColorVar.AlphaChannel * RND_MINUS1_1);
            RGBAColor rgbaColor2 = default;
            rgbaColor2.RedColor = endColor.RedColor + (endColorVar.RedColor * RND_MINUS1_1);
            rgbaColor2.GreenColor = endColor.GreenColor + (endColorVar.GreenColor * RND_MINUS1_1);
            rgbaColor2.BlueColor = endColor.BlueColor + (endColorVar.BlueColor * RND_MINUS1_1);
            rgbaColor2.AlphaChannel = endColor.AlphaChannel + (endColorVar.AlphaChannel * RND_MINUS1_1);
            particle.color = rgbaColor;
            particle.deltaColor.RedColor = (rgbaColor2.RedColor - rgbaColor.RedColor) / particle.life;
            particle.deltaColor.GreenColor = (rgbaColor2.GreenColor - rgbaColor.GreenColor) / particle.life;
            particle.deltaColor.BlueColor = (rgbaColor2.BlueColor - rgbaColor.BlueColor) / particle.life;
            particle.deltaColor.AlphaChannel = (rgbaColor2.AlphaChannel - rgbaColor.AlphaChannel) / particle.life;
            particle.size = size + (sizeVar * RND_MINUS1_1);
        }

        /// <summary>
        /// Starts the particle system, spawning an initial batch of particles.
        /// </summary>
        /// <param name="initialParticles">Number of particles to spawn immediately.</param>
        public virtual void StartSystem(int initialParticles)
        {
            particleCount = 0;
            while (particleCount < initialParticles)
            {
                _ = AddParticle();
            }
            active = true;
        }

        /// <summary>
        /// Stops emitting new particles. Existing particles continue until they expire.
        /// </summary>
        public virtual void StopSystem()
        {
            active = false;
            elapsed = duration;
            emitCounter = 0f;
        }

        /// <summary>
        /// Resets the elapsed time and emission counter without stopping the system.
        /// </summary>
        public virtual void ResetSystem()
        {
            elapsed = 0f;
            emitCounter = 0f;
        }

        /// <summary>
        /// Returns <see langword="true"/> if the particle count has reached the maximum capacity.
        /// </summary>
        /// <returns><see langword="true"/> when the system is full; otherwise <see langword="false"/>.</returns>
        public virtual bool IsFull()
        {
            return particleCount == totalParticles;
        }

        /// <summary>
        /// Sets whether particles use additive blending.
        /// </summary>
        /// <param name="b"><see langword="true"/> for additive blending, <see langword="false"/> for alpha blending.</param>
        public virtual void SetBlendAdditive(bool b)
        {
            blendAdditive = b;
        }

        /// <summary>
        /// Whether the system is currently emitting particles.
        /// </summary>
        public bool active;

        /// <summary>
        /// Emission duration in seconds, or -1 for infinite.
        /// </summary>
        public float duration;

        /// <summary>
        /// Elapsed time since the system started.
        /// </summary>
        public float elapsed;

        /// <summary>
        /// Gravity vector applied to all particles.
        /// </summary>
        public Vector gravity;

        /// <summary>
        /// Position variance for randomized spawn offsets.
        /// </summary>
        public Vector posVar;

        /// <summary>
        /// Emission angle in degrees.
        /// </summary>
        public float angle;

        /// <summary>
        /// Emission angle variance in degrees.
        /// </summary>
        public float angleVar;

        /// <summary>
        /// Initial particle speed.
        /// </summary>
        public float speed;

        /// <summary>
        /// Speed variance.
        /// </summary>
        public float speedVar;

        /// <summary>
        /// Tangential acceleration.
        /// </summary>
        public float tangentialAccel;

        /// <summary>
        /// Tangential acceleration variance.
        /// </summary>
        public float tangentialAccelVar;

        /// <summary>
        /// Radial acceleration.
        /// </summary>
        public float radialAccel;

        /// <summary>
        /// Radial acceleration variance.
        /// </summary>
        public float radialAccelVar;

        /// <summary>
        /// Initial particle size.
        /// </summary>
        public float size;

        // public float endSize;

        /// <summary>
        /// Size variance.
        /// </summary>
        public float sizeVar;

        /// <summary>
        /// Particle lifetime in seconds.
        /// </summary>
        public float life;

        /// <summary>
        /// Lifetime variance in seconds.
        /// </summary>
        public float lifeVar;

        /// <summary>
        /// Start color for new particles.
        /// </summary>
        public RGBAColor startColor;

        /// <summary>
        /// Start color variance.
        /// </summary>
        public RGBAColor startColorVar;

        /// <summary>
        /// End color particles transition to over their lifetime.
        /// </summary>
        public RGBAColor endColor;

        /// <summary>
        /// End color variance.
        /// </summary>
        public RGBAColor endColorVar;

        /// <summary>
        /// Array of all particle instances.
        /// </summary>
        public Particle[] particles;

        /// <summary>
        /// Maximum number of particles.
        /// </summary>
        public int totalParticles;

        /// <summary>
        /// Current number of live particles.
        /// </summary>
        public int particleCount;

        /// <summary>
        /// Whether additive blending is enabled.
        /// </summary>
        public bool blendAdditive;

        // public bool colorModulate;

        /// <summary>
        /// Number of particles emitted per second.
        /// </summary>
        public float emissionRate;

        /// <summary>
        /// Accumulated time for emission rate tracking.
        /// </summary>
        public float emitCounter;

        /// <summary>
        /// Texture used for rendering particles.
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// Point sprite positions and sizes for rendering.
        /// </summary>
        public PointSprite[] vertices;

        /// <summary>
        /// Per-particle colors for rendering.
        /// </summary>
        public RGBAColor[] colors;

        /// <summary>
        /// Current particle index during update iteration.
        /// </summary>
        public int particleIdx;

        /// <summary>
        /// Callback invoked when all particles have expired after the system stops.
        /// </summary>
        public ParticlesFinished particlesDelegate;

        /// <summary>
        /// Delegate type for particle system completion callbacks.
        /// </summary>
        /// <param name="p">Particle system that finished.</param>
        public delegate void ParticlesFinished(Particles p);
    }
}
