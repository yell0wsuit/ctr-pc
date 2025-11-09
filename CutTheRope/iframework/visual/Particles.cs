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
                Vector vector = CTRMathHelper.vectZero;
                if (p.pos.x != 0f || p.pos.y != 0f)
                {
                    vector = CTRMathHelper.vectNormalize(p.pos);
                }
                Vector v = vector;
                vector = CTRMathHelper.vectMult(vector, p.radialAccel);
                float num = v.x;
                v.x = 0f - v.y;
                v.y = num;
                v = CTRMathHelper.vectMult(v, p.tangentialAccel);
                Vector v2 = CTRMathHelper.vectAdd(CTRMathHelper.vectAdd(vector, v), this.gravity);
                v2 = CTRMathHelper.vectMult(v2, delta);
                p.dir = CTRMathHelper.vectAdd(p.dir, v2);
                v2 = CTRMathHelper.vectMult(p.dir, delta);
                p.pos = CTRMathHelper.vectAdd(p.pos, v2);
                p.color.r = p.color.r + p.deltaColor.r * delta;
                p.color.g = p.color.g + p.deltaColor.g * delta;
                p.color.b = p.color.b + p.deltaColor.b * delta;
                p.color.a = p.color.a + p.deltaColor.a * delta;
                p.life -= delta;
                this.vertices[this.particleIdx].x = p.pos.x;
                this.vertices[this.particleIdx].y = p.pos.y;
                this.vertices[this.particleIdx].size = p.size;
                this.colors[this.particleIdx] = p.color;
                this.particleIdx++;
                return;
            }
            if (this.particleIdx != this.particleCount - 1)
            {
                this.particles[this.particleIdx] = this.particles[this.particleCount - 1];
            }
            this.particleCount--;
        }

        public override void update(float delta)
        {
            base.update(delta);
            if (this.particlesDelegate != null && this.particleCount == 0 && !this.active)
            {
                this.particlesDelegate(this);
                return;
            }
            if (this.vertices == null)
            {
                return;
            }
            if (this.active && this.emissionRate != 0f)
            {
                float num = 1f / this.emissionRate;
                this.emitCounter += delta;
                while (this.particleCount < this.totalParticles && this.emitCounter > num)
                {
                    this.addParticle();
                    this.emitCounter -= num;
                }
                this.elapsed += delta;
                if (this.duration != -1f && this.duration < this.elapsed)
                {
                    this.stopSystem();
                }
            }
            this.particleIdx = 0;
            while (this.particleIdx < this.particleCount)
            {
                this.updateParticle(ref this.particles[this.particleIdx], delta);
            }
            OpenGL.glBindBuffer(2, this.verticesID);
            OpenGL.glBufferData(2, this.vertices, 3);
            OpenGL.glBindBuffer(2, this.colorsID);
            OpenGL.glBufferData(2, this.colors, 3);
            OpenGL.glBindBuffer(2, 0U);
        }

        public override void dealloc()
        {
            this.particles = null;
            this.vertices = null;
            this.colors = null;
            OpenGL.glDeleteBuffers(1, ref this.verticesID);
            OpenGL.glDeleteBuffers(1, ref this.colorsID);
            this.texture = null;
            base.dealloc();
        }

        public override void draw()
        {
            this.preDraw();
            this.postDraw();
        }

        public virtual Particles initWithTotalParticles(int numberOfParticles)
        {
            if (this.init() == null)
            {
                return null;
            }
            this.width = (int)FrameworkTypes.SCREEN_WIDTH;
            this.height = (int)FrameworkTypes.SCREEN_HEIGHT;
            this.totalParticles = numberOfParticles;
            this.particles = new Particle[this.totalParticles];
            this.vertices = new PointSprite[this.totalParticles];
            this.colors = new RGBAColor[this.totalParticles];
            if (this.particles == null || this.vertices == null || this.colors == null)
            {
                this.particles = null;
                this.vertices = null;
                this.colors = null;
                return null;
            }
            this.active = false;
            this.blendAdditive = false;
            OpenGL.glGenBuffers(1, ref this.verticesID);
            OpenGL.glGenBuffers(1, ref this.colorsID);
            return this;
        }

        public virtual bool addParticle()
        {
            if (this.isFull())
            {
                return false;
            }
            this.initParticle(ref this.particles[this.particleCount]);
            this.particleCount++;
            return true;
        }

        public virtual void initParticle(ref Particle particle)
        {
            particle.pos.x = this.x + this.posVar.x * CTRMathHelper.RND_MINUS1_1;
            particle.pos.y = this.y + this.posVar.y * CTRMathHelper.RND_MINUS1_1;
            particle.startPos = particle.pos;
            float num = CTRMathHelper.DEGREES_TO_RADIANS(this.angle + this.angleVar * CTRMathHelper.RND_MINUS1_1);
            Vector v = default(Vector);
            v.y = CTRMathHelper.sinf(num);
            v.x = CTRMathHelper.cosf(num);
            float s = this.speed + this.speedVar * CTRMathHelper.RND_MINUS1_1;
            particle.dir = CTRMathHelper.vectMult(v, s);
            particle.radialAccel = this.radialAccel + this.radialAccelVar * CTRMathHelper.RND_MINUS1_1;
            particle.tangentialAccel = this.tangentialAccel + this.tangentialAccelVar * CTRMathHelper.RND_MINUS1_1;
            particle.life = this.life + this.lifeVar * CTRMathHelper.RND_MINUS1_1;
            RGBAColor rGBAColor = default(RGBAColor);
            rGBAColor.r = this.startColor.r + this.startColorVar.r * CTRMathHelper.RND_MINUS1_1;
            rGBAColor.g = this.startColor.g + this.startColorVar.g * CTRMathHelper.RND_MINUS1_1;
            rGBAColor.b = this.startColor.b + this.startColorVar.b * CTRMathHelper.RND_MINUS1_1;
            rGBAColor.a = this.startColor.a + this.startColorVar.a * CTRMathHelper.RND_MINUS1_1;
            RGBAColor rGBAColor2 = default(RGBAColor);
            rGBAColor2.r = this.endColor.r + this.endColorVar.r * CTRMathHelper.RND_MINUS1_1;
            rGBAColor2.g = this.endColor.g + this.endColorVar.g * CTRMathHelper.RND_MINUS1_1;
            rGBAColor2.b = this.endColor.b + this.endColorVar.b * CTRMathHelper.RND_MINUS1_1;
            rGBAColor2.a = this.endColor.a + this.endColorVar.a * CTRMathHelper.RND_MINUS1_1;
            particle.color = rGBAColor;
            particle.deltaColor.r = (rGBAColor2.r - rGBAColor.r) / particle.life;
            particle.deltaColor.g = (rGBAColor2.g - rGBAColor.g) / particle.life;
            particle.deltaColor.b = (rGBAColor2.b - rGBAColor.b) / particle.life;
            particle.deltaColor.a = (rGBAColor2.a - rGBAColor.a) / particle.life;
            particle.size = this.size + this.sizeVar * CTRMathHelper.RND_MINUS1_1;
        }

        public virtual void startSystem(int initialParticles)
        {
            this.particleCount = 0;
            while (this.particleCount < initialParticles)
            {
                this.addParticle();
            }
            this.active = true;
        }

        public virtual void stopSystem()
        {
            this.active = false;
            this.elapsed = this.duration;
            this.emitCounter = 0f;
        }

        public virtual void resetSystem()
        {
            this.elapsed = 0f;
            this.emitCounter = 0f;
        }

        public virtual bool isFull()
        {
            return this.particleCount == this.totalParticles;
        }

        public virtual void setBlendAdditive(bool b)
        {
            this.blendAdditive = b;
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

        public Particles.ParticlesFinished particlesDelegate;

        // (Invoke) Token: 0x06000668 RID: 1640
        public delegate void ParticlesFinished(Particles p);
    }
}
