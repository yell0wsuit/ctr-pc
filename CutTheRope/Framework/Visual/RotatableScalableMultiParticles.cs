using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Visual
{
    internal sealed class RotatableScalableMultiParticles : ScalableMultiParticles
    {
        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            particle.angle = initialAngle;
            particle.deltaAngle = DEGREES_TO_RADIANS(rotateSpeed + (rotateSpeedVar * RND_MINUS1_1));
            particle.deltaSize = (endSize - size) / particle.life;
        }

        public override void UpdateParticle(ref Particle p, float delta)
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
                p.size += p.deltaSize * delta;
                p.life -= delta;
                float num2 = p.width * p.size;
                float num3 = p.height * p.size;
                float num4 = p.pos.x - (num2 / 2f);
                float num5 = p.pos.y - (num3 / 2f);
                float num6 = p.pos.x + (num2 / 2f);
                float num7 = p.pos.y - (num3 / 2f);
                float num8 = p.pos.x - (num2 / 2f);
                float num9 = p.pos.y + (num3 / 2f);
                float num11 = p.pos.x + (num2 / 2f);
                float num10 = p.pos.y + (num3 / 2f);
                float cx = p.pos.x;
                float cy = p.pos.y;
                Vector v3 = Vect(num4, num5);
                Vector v4 = Vect(num6, num7);
                Vector v5 = Vect(num8, num9);
                Vector v6 = Vect(num11, num10);
                p.angle += p.deltaAngle * delta;
                float cosA = Cosf(p.angle);
                float sinA = Sinf(p.angle);
                v3 = RotatePreCalc(v3, cosA, sinA, cx, cy);
                v4 = RotatePreCalc(v4, cosA, sinA, cx, cy);
                v5 = RotatePreCalc(v5, cosA, sinA, cx, cy);
                v6 = RotatePreCalc(v6, cosA, sinA, cx, cy);
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3DEx(v3.x, v3.y, v4.x, v4.y, v5.x, v5.y, v6.x, v6.y);
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

        public float initialAngle;

        public float rotateSpeed;

        public float rotateSpeedVar;
    }
}
