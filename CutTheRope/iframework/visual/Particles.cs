using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;
using System;

namespace CutTheRope.iframework.visual
{
    internal class Particles : BaseElement
    {
        public static Vector rotatePreCalc(Vector v, float cosA, float sinA, float cx, float cy)
        {
            Vector result = v;
            result.x -= cx;
            result.y -= cy;
            float num = result.x * cosA - result.y * sinA;
            float num2 = result.x * sinA + result.y * cosA;
            result.x = num + cx;
            result.y = num2 + cy;
            return result;
        }

        public virtual void updateParticle(ref Particle p, float delta)
        {
            if (p.life > 0f)
            {
                Vector vector = vectZero;
                if (p.pos.x != 0f || p.pos.y != 0f)
                {
                    vector = vectNormalize(p.pos);
                }
                Vector v = vector;
                vector = vectMult(vector, p.radialAccel);
                float num = v.x;
                v.x = 0f - v.y;
                v.y = num;
                v = vectMult(v, p.tangentialAccel);
                Vector v2 = vectAdd(vectAdd(vector, v), gravity);
                v2 = vectMult(v2, delta);
                p.dir = vectAdd(p.dir, v2);
                v2 = vectMult(p.dir, delta);
                p.pos = vectAdd(p.pos, v2);
                p.color.r = p.color.r + p.deltaColor.r * delta;
                p.color.g = p.color.g + p.deltaColor.g * delta;
                p.color.b = p.color.b + p.deltaColor.b * delta;
                p.color.a = p.color.a + p.deltaColor.a * delta;
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

        public override void update(float delta)
        {
            base.update(delta);
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
                    addParticle();
                    emitCounter -= num;
                }
                elapsed += delta;
                if (duration != -1f && duration < elapsed)
                {
                    stopSystem();
                }
            }
            particleIdx = 0;
            while (particleIdx < particleCount)
            {
                updateParticle(ref particles[particleIdx], delta);
            }
            OpenGL.glBindBuffer(2, verticesID);
            OpenGL.glBufferData(2, vertices, 3);
            OpenGL.glBindBuffer(2, colorsID);
            OpenGL.glBufferData(2, colors, 3);
            OpenGL.glBindBuffer(2, 0U);
        }

        public override void dealloc()
        {
            particles = null;
            vertices = null;
            colors = null;
            OpenGL.glDeleteBuffers(1, ref verticesID);
            OpenGL.glDeleteBuffers(1, ref colorsID);
            texture = null;
            base.dealloc();
        }

        public override void draw()
        {
            preDraw();
            postDraw();
        }

        public virtual Particles initWithTotalParticles(int numberOfParticles)
        {
            if (init() == null)
            {
                return null;
            }
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
            OpenGL.glGenBuffers(1, ref verticesID);
            OpenGL.glGenBuffers(1, ref colorsID);
            return this;
        }

        public virtual bool addParticle()
        {
            if (isFull())
            {
                return false;
            }
            initParticle(ref particles[particleCount]);
            particleCount++;
            return true;
        }

        public virtual void initParticle(ref Particle particle)
        {
            particle.pos.x = x + posVar.x * RND_MINUS1_1;
            particle.pos.y = y + posVar.y * RND_MINUS1_1;
            particle.startPos = particle.pos;
            float num = DEGREES_TO_RADIANS(angle + angleVar * RND_MINUS1_1);
            Vector v = default(Vector);
            v.y = sinf(num);
            v.x = cosf(num);
            float s = speed + speedVar * RND_MINUS1_1;
            particle.dir = vectMult(v, s);
            particle.radialAccel = radialAccel + radialAccelVar * RND_MINUS1_1;
            particle.tangentialAccel = tangentialAccel + tangentialAccelVar * RND_MINUS1_1;
            particle.life = life + lifeVar * RND_MINUS1_1;
            RGBAColor rGBAColor = default(RGBAColor);
            rGBAColor.r = startColor.r + startColorVar.r * RND_MINUS1_1;
            rGBAColor.g = startColor.g + startColorVar.g * RND_MINUS1_1;
            rGBAColor.b = startColor.b + startColorVar.b * RND_MINUS1_1;
            rGBAColor.a = startColor.a + startColorVar.a * RND_MINUS1_1;
            RGBAColor rGBAColor2 = default(RGBAColor);
            rGBAColor2.r = endColor.r + endColorVar.r * RND_MINUS1_1;
            rGBAColor2.g = endColor.g + endColorVar.g * RND_MINUS1_1;
            rGBAColor2.b = endColor.b + endColorVar.b * RND_MINUS1_1;
            rGBAColor2.a = endColor.a + endColorVar.a * RND_MINUS1_1;
            particle.color = rGBAColor;
            particle.deltaColor.r = (rGBAColor2.r - rGBAColor.r) / particle.life;
            particle.deltaColor.g = (rGBAColor2.g - rGBAColor.g) / particle.life;
            particle.deltaColor.b = (rGBAColor2.b - rGBAColor.b) / particle.life;
            particle.deltaColor.a = (rGBAColor2.a - rGBAColor.a) / particle.life;
            particle.size = size + sizeVar * RND_MINUS1_1;
        }

        public virtual void startSystem(int initialParticles)
        {
            particleCount = 0;
            while (particleCount < initialParticles)
            {
                addParticle();
            }
            active = true;
        }

        public virtual void stopSystem()
        {
            active = false;
            elapsed = duration;
            emitCounter = 0f;
        }

        public virtual void resetSystem()
        {
            elapsed = 0f;
            emitCounter = 0f;
        }

        public virtual bool isFull()
        {
            return particleCount == totalParticles;
        }

        public virtual void setBlendAdditive(bool b)
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
