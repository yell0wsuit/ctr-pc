using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
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
                p.color.RedColor += p.deltaColor.RedColor * delta;
                p.color.GreenColor += p.deltaColor.GreenColor * delta;
                p.color.BlueColor += p.deltaColor.BlueColor * delta;
                p.color.AlphaChannel += p.deltaColor.AlphaChannel * delta;
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
        }

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

        // public float endSize;

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

        // public bool colorModulate;

        public float emissionRate;

        public float emitCounter;

        public Texture2D texture;

        public PointSprite[] vertices;

        public RGBAColor[] colors;

        public int particleIdx;

        public ParticlesFinished particlesDelegate;

        public delegate void ParticlesFinished(Particles p);
    }
}
