using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class PumpDirt : MultiParticles
    {
        public PumpDirt InitWithTotalParticlesAngleandImageGrid(int p, float a, Image grid)
        {
            if (InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }
            duration = 0.6f;
            gravity.x = 0f;
            gravity.y = 0f;
            angle = a;
            angleVar = 10f;
            speed = 1000f;
            speedVar = 100f;
            radialAccel = 0f;
            radialAccelVar = 0f;
            tangentialAccel = 0f;
            tangentialAccelVar = 0f;
            posVar.x = 0f;
            posVar.y = 0f;
            life = 0.6f;
            lifeVar = 0f;
            size = 2f;
            sizeVar = 0f;
            emissionRate = 100f;
            startColor.r = 1f;
            startColor.g = 1f;
            startColor.b = 1f;
            startColor.a = 0.6f;
            startColorVar.r = 0f;
            startColorVar.g = 0f;
            startColorVar.b = 0f;
            startColorVar.a = 0f;
            endColor.r = 1f;
            endColor.g = 1f;
            endColor.b = 1f;
            endColor.a = 0f;
            endColorVar.r = 0f;
            endColorVar.g = 0f;
            endColorVar.b = 0f;
            endColorVar.a = 0f;
            blendAdditive = true;
            return this;
        }

        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            int num = RND_RANGE(6, 8);
            Quad2D qt = imageGrid.texture.quads[num];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            CTRRectangle rectangle = imageGrid.texture.quadRects[num];
            particle.width = rectangle.w * particle.size;
            particle.height = rectangle.h * particle.size;
        }

        public override void UpdateParticle(ref Particle p, float delta)
        {
            if (p.life > 0f)
            {
                p.dir = VectMult(p.dir, 0.9);
                Vector v = VectMult(p.dir, delta);
                v = VectAdd(v, gravity);
                p.pos = VectAdd(p.pos, v);
                p.color.r += p.deltaColor.r * delta;
                p.color.g += p.deltaColor.g * delta;
                p.color.b += p.deltaColor.b * delta;
                p.color.a += p.deltaColor.a * delta;
                p.life -= delta;
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3D((double)(p.pos.x - (p.width / 2f)), (double)(p.pos.y - (p.height / 2f)), 0.0, p.width, p.height);
                for (int i = 0; i < 4; i++)
                {
                    colors[(particleIdx * 4) + i] = p.color;
                }
                particleIdx++;
                return;
            }
            if (particleIdx != particleCount - 1)
            {
                particles[particleIdx] = particles[particleCount - 1];
                drawer.vertices[particleIdx] = drawer.vertices[particleCount - 1];
                drawer.texCoordinates[particleIdx] = drawer.texCoordinates[particleCount - 1];
            }
            particleCount--;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
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
            OpenGL.GlBindBuffer(2, colorsID);
            OpenGL.GlBufferData(2, colors, 3);
            OpenGL.GlBindBuffer(2, 0U);
        }
    }
}
