using CutTheRope.desktop;
using CutTheRope.iframework.core;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.iframework.visual
{
    internal class Particles : BaseElement
    {
        public static Vector RotatePreCalc(Vector v, float cosA, float sinA, float cx, float cy)
        {
            Vector result = v;
            result.x -= cx;
            result.y -= cy;
            float num = (result.x * cosA) - (result.y * sinA);
            float num2 = (result.x * sinA) + (result.y * cosA);
            result.x = num + cx;
            result.y = num2 + cy;
            return result;
        }

        public virtual void UpdateParticle(ref Particle p, float delta)
        {
            if (p.life > 0f)
            {
                Vector vector = vectZero;
                if (p.pos.x != 0f || p.pos.y != 0f)
                {
                    vector = VectNormalize(p.pos);
                }
                Vector v = vector;
                vector = VectMult(vector, p.radialAccel);
                float num = v.x;
                v.x = 0f - v.y;
                v.y = num;
                v = VectMult(v, p.tangentialAccel);
                Vector v2 = VectAdd(VectAdd(vector, v), gravity);
                v2 = VectMult(v2, delta);
                p.dir = VectAdd(p.dir, v2);
                v2 = VectMult(p.dir, delta);
                p.pos = VectAdd(p.pos, v2);
                p.color.r += p.deltaColor.r * delta;
                p.color.g += p.deltaColor.g * delta;
                p.color.b += p.deltaColor.b * delta;
                p.color.a += p.deltaColor.a * delta;
                p.life -= delta;
                vertices[particleIdx].x = p.pos.x;
                vertices[particleIdx].y = p.pos.y;
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
                float num = 1f / emissionRate;
                emitCounter += delta;
                while (particleCount < totalParticles && emitCounter > num)
                {
                    _ = AddParticle();
                    emitCounter -= num;
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
            OpenGL.GlBindBuffer(2, verticesID);
            OpenGL.GlBufferData(2, vertices, 3);
            OpenGL.GlBindBuffer(2, colorsID);
            OpenGL.GlBufferData(2, colors, 3);
            OpenGL.GlBindBuffer(2, 0U);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                particles = null;
                vertices = null;
                colors = null;
                if (verticesID != 0)
                {
                    OpenGL.GlDeleteBuffers(1, ref verticesID);
                    verticesID = 0;
                }
                if (colorsID != 0)
                {
                    OpenGL.GlDeleteBuffers(1, ref colorsID);
                    colorsID = 0;
                }
                texture = null;
            }
            base.Dispose(disposing);
        }

        public override void Draw()
        {
            PreDraw();
            PostDraw();
        }

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
            OpenGL.GlGenBuffers(1, ref verticesID);
            OpenGL.GlGenBuffers(1, ref colorsID);
            return this;
        }

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

        public virtual void InitParticle(ref Particle particle)
        {
            particle.pos.x = x + (posVar.x * RND_MINUS1_1);
            particle.pos.y = y + (posVar.y * RND_MINUS1_1);
            particle.startPos = particle.pos;
            float num = DEGREES_TO_RADIANS(angle + (angleVar * RND_MINUS1_1));
            Vector v = default;
            v.y = Sinf(num);
            v.x = Cosf(num);
            float s = speed + (speedVar * RND_MINUS1_1);
            particle.dir = VectMult(v, s);
            particle.radialAccel = radialAccel + (radialAccelVar * RND_MINUS1_1);
            particle.tangentialAccel = tangentialAccel + (tangentialAccelVar * RND_MINUS1_1);
            particle.life = life + (lifeVar * RND_MINUS1_1);
            RGBAColor rgbaColor = default;
            rgbaColor.r = startColor.r + (startColorVar.r * RND_MINUS1_1);
            rgbaColor.g = startColor.g + (startColorVar.g * RND_MINUS1_1);
            rgbaColor.b = startColor.b + (startColorVar.b * RND_MINUS1_1);
            rgbaColor.a = startColor.a + (startColorVar.a * RND_MINUS1_1);
            RGBAColor rgbaColor2 = default;
            rgbaColor2.r = endColor.r + (endColorVar.r * RND_MINUS1_1);
            rgbaColor2.g = endColor.g + (endColorVar.g * RND_MINUS1_1);
            rgbaColor2.b = endColor.b + (endColorVar.b * RND_MINUS1_1);
            rgbaColor2.a = endColor.a + (endColorVar.a * RND_MINUS1_1);
            particle.color = rgbaColor;
            particle.deltaColor.r = (rgbaColor2.r - rgbaColor.r) / particle.life;
            particle.deltaColor.g = (rgbaColor2.g - rgbaColor.g) / particle.life;
            particle.deltaColor.b = (rgbaColor2.b - rgbaColor.b) / particle.life;
            particle.deltaColor.a = (rgbaColor2.a - rgbaColor.a) / particle.life;
            particle.size = size + (sizeVar * RND_MINUS1_1);
        }

        public virtual void StartSystem(int initialParticles)
        {
            particleCount = 0;
            while (particleCount < initialParticles)
            {
                _ = AddParticle();
            }
            active = true;
        }

        public virtual void StopSystem()
        {
            active = false;
            elapsed = duration;
            emitCounter = 0f;
        }

        public virtual void ResetSystem()
        {
            elapsed = 0f;
            emitCounter = 0f;
        }

        public virtual bool IsFull()
        {
            return particleCount == totalParticles;
        }

        public virtual void SetBlendAdditive(bool b)
        {
            blendAdditive = b;
        }

        public bool active;

        public float duration;

        public float elapsed;

        public Vector gravity;

        public Vector posVar;

        public float angle;

        public float angleVar;

        public float speed;

        public float speedVar;

        public float tangentialAccel;

        public float tangentialAccelVar;

        public float radialAccel;

        public float radialAccelVar;

        public float size;

        public float endSize;

        public float sizeVar;

        public float life;

        public float lifeVar;

        public RGBAColor startColor;

        public RGBAColor startColorVar;

        public RGBAColor endColor;

        public RGBAColor endColorVar;

        public Particle[] particles;

        public int totalParticles;

        public int particleCount;

        public bool blendAdditive;

        public bool colorModulate;

        public float emissionRate;

        public float emitCounter;

        public Texture2D texture;

        public PointSprite[] vertices;

        public RGBAColor[] colors;

        private uint verticesID;

        public uint colorsID;

        public int particleIdx;

        public ParticlesFinished particlesDelegate;

        // (Invoke) Token: 0x06000668 RID: 1640
        public delegate void ParticlesFinished(Particles p);
    }
}
