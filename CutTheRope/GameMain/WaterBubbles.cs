using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// The water bubble visual effects
    /// </summary>
    internal sealed class WaterBubbles : MultiParticles
    {
        private static readonly int[] s_bubbleQuads = [5, 4];

        private float startSize;
        private float endSize;

        public override Particles InitWithTotalParticlesandImageGrid(int p, Image grid)
        {
            if (base.InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }

            duration = -1f;
            gravity.X = 0f;
            gravity.Y = 0f;
            speed = 150f;
            speedVar = 0f;
            angle = -90f;
            posVar.X = width / 2f;
            posVar.Y = 0f;
            life = 5f;
            lifeVar = 1f;
            size = 0.7f;
            sizeVar = 0.3f;
            startSize = size + sizeVar;
            endSize = 0.7f;
            emissionRate = 2f;

            startColor.RedColor = 1f;
            startColor.GreenColor = 1f;
            startColor.BlueColor = 1f;
            startColor.AlphaChannel = 0.6f;
            startColorVar.RedColor = 0f;
            startColorVar.GreenColor = 0f;
            startColorVar.BlueColor = 0f;
            startColorVar.AlphaChannel = 0f;

            endColor.RedColor = 1f;
            endColor.GreenColor = 1f;
            endColor.BlueColor = 1f;
            endColor.AlphaChannel = 0f;
            endColorVar.RedColor = 0f;
            endColorVar.GreenColor = 0f;
            endColorVar.BlueColor = 0f;
            endColorVar.AlphaChannel = 0f;

            blendAdditive = true;
            return this;
        }

        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);

            int randomQuad = s_bubbleQuads[RND_RANGE(0, s_bubbleQuads.Length - 1)];
            Quad2D qt = imageGrid.texture.quads[randomQuad];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);

            CTRRectangle rect = imageGrid.texture.quadRects[randomQuad];
            particle.width = rect.w * particle.size;
            particle.height = rect.h * particle.size;
            particle.deltaSize = endSize;
        }

        public override void Update(float delta)
        {
            base.Update(delta);

            particleIdx = 0;
            while (particleIdx < particleCount)
            {
                Particle particle = particles[particleIdx];
                if (particle.life > 0f && Mover.MoveVariableToTarget(ref particle.size, particle.deltaSize, 0.5f, delta))
                {
                    particle.deltaSize = particle.size == endSize ? startSize : endSize;
                }

                float t = 1f - ((1f - particle.size) / sizeVar);
                float scaledWidth = particle.width * particle.size;
                float scaledHeight = particle.height - (particle.height * sizeVar * t);
                float halfWidth = scaledWidth / 2f;
                float halfHeight = scaledHeight / 2f;

                Vector tl = Vect(particle.pos.X - halfWidth, particle.pos.Y - halfHeight);
                Vector tr = Vect(particle.pos.X + halfWidth, particle.pos.Y - halfHeight);
                Vector bl = Vect(particle.pos.X - halfWidth, particle.pos.Y + halfHeight);
                Vector br = Vect(particle.pos.X + halfWidth, particle.pos.Y + halfHeight);

                particle.angle += particle.deltaAngle * delta;
                float cosA = Cosf(particle.angle);
                float sinA = Sinf(particle.angle);
                tl = RotatePreCalc(tl, cosA, sinA, particle.pos.X, particle.pos.Y);
                tr = RotatePreCalc(tr, cosA, sinA, particle.pos.X, particle.pos.Y);
                bl = RotatePreCalc(bl, cosA, sinA, particle.pos.X, particle.pos.Y);
                br = RotatePreCalc(br, cosA, sinA, particle.pos.X, particle.pos.Y);

                particles[particleIdx] = particle;
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3DEx(tl.X, tl.Y, tr.X, tr.Y, bl.X, bl.Y, br.X, br.Y);
                particleIdx++;
            }
        }
    }
}
