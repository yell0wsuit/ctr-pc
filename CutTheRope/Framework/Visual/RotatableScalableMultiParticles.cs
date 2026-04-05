using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A <see cref="ScalableMultiParticles"/> variant that adds per-particle rotation and size scaling over lifetime.
    /// </summary>
    internal class RotatableScalableMultiParticles : ScalableMultiParticles
    {
        /// <inheritdoc />
        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            particle.angle = initialAngle;
            // particle.deltaAngle = DEGREES_TO_RADIANS(rotateSpeed + (rotateSpeedVar * RND_MINUS1_1));
            particle.deltaSize = (endSize - size) / particle.life;
        }

        /// <inheritdoc />
        public override void UpdateParticle(ref Particle p, float delta)
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
                p.size += p.deltaSize * delta;
                p.life -= delta;
                float scaledWidth = p.width * p.size;
                float scaledHeight = p.height * p.size;
                float halfWidth = scaledWidth / 2f;
                float halfHeight = scaledHeight / 2f;
                float cx = p.pos.X;
                float cy = p.pos.Y;
                Vector topLeft = Vect(p.pos.X - halfWidth, p.pos.Y - halfHeight);
                Vector topRight = Vect(p.pos.X + halfWidth, p.pos.Y - halfHeight);
                Vector bottomLeft = Vect(p.pos.X - halfWidth, p.pos.Y + halfHeight);
                Vector bottomRight = Vect(p.pos.X + halfWidth, p.pos.Y + halfHeight);
                p.angle += p.deltaAngle * delta;
                float cosA = Cosf(p.angle);
                float sinA = Sinf(p.angle);
                topLeft = RotatePreCalc(topLeft, cosA, sinA, cx, cy);
                topRight = RotatePreCalc(topRight, cosA, sinA, cx, cy);
                bottomLeft = RotatePreCalc(bottomLeft, cosA, sinA, cx, cy);
                bottomRight = RotatePreCalc(bottomRight, cosA, sinA, cx, cy);
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3DEx(
                    topLeft.X,
                    topLeft.Y,
                    topRight.X,
                    topRight.Y,
                    bottomLeft.X,
                    bottomLeft.Y,
                    bottomRight.X,
                    bottomRight.Y);
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

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
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

        /// <summary>
        /// Initial rotation angle for new particles in radians.
        /// </summary>
        public float initialAngle;

        // public float rotateSpeed;

        // public float rotateSpeedVar;

        /// <summary>
        /// Target size at end of particle lifetime.
        /// </summary>
        public float endSize;
    }
}
