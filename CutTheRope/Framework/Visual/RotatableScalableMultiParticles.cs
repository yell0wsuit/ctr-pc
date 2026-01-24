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
                if (p.pos.XAxis != 0f || p.pos.YAxis != 0f)
                {
                    vector = VectNormalize(p.pos);
                }
                Vector v = vector;
                vector = VectMult(vector, p.radialAccel);
                float num = v.XAxis;
                v.XAxis = 0f - v.YAxis;
                v.YAxis = num;
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
                p.size += p.deltaSize * delta;
                p.life -= delta;
                float num2 = p.width * p.size;
                float num3 = p.height * p.size;
                float num4 = p.pos.XAxis - (num2 / 2f);
                float num5 = p.pos.YAxis - (num3 / 2f);
                float num6 = p.pos.XAxis + (num2 / 2f);
                float num7 = p.pos.YAxis - (num3 / 2f);
                float num8 = p.pos.XAxis - (num2 / 2f);
                float num9 = p.pos.YAxis + (num3 / 2f);
                float num11 = p.pos.XAxis + (num2 / 2f);
                float num10 = p.pos.YAxis + (num3 / 2f);
                float cx = p.pos.XAxis;
                float cy = p.pos.YAxis;
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
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3DEx(v3.XAxis, v3.YAxis, v4.XAxis, v4.YAxis, v5.XAxis, v5.YAxis, v6.XAxis, v6.YAxis);
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
        }

#pragma warning disable CS0649
        public float initialAngle;
#pragma warning restore CS0649

#pragma warning disable CS0649
        public float rotateSpeed;
#pragma warning restore CS0649

#pragma warning disable CS0649
        public float rotateSpeedVar;
#pragma warning restore CS0649

#pragma warning disable CS0649
        private readonly float endSize;
#pragma warning restore CS0649
    }
}
