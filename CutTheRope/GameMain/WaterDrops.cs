using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// The water drops visual effect when candy drops into water
    /// </summary>
    internal sealed class WaterDrops : RotateableMultiParticles
    {
        public override Particles InitWithTotalParticlesandImageGrid(int p, Image grid)
        {
            if (base.InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }

            size = 1f;
            sizeVar = 0f;
            life = 1f;
            lifeVar = 0f;
            duration = 1f;
            angle = -90f;
            angleVar = 50f;
            speed = 100f;
            speedVar = 10f;
            radialAccel = 0f;
            radialAccelVar = 1f;
            tangentialAccel = 0f;
            tangentialAccelVar = 1f;
            posVar.X = 0f;
            posVar.Y = 0f;
            gravity.Y = 175f;
            emissionRate = 100f;
            startColor = RGBAColor.solidOpaqueRGBA;
            startColorVar = RGBAColor.transparentRGBA;
            endColor = RGBAColor.transparentRGBA;
            endColorVar = RGBAColor.transparentRGBA;
            rotateSpeed = 0f;
            rotateSpeedVar = 600f;
            blendAdditive = true;
            return this;
        }

        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);

            int randomQuad = RND_RANGE(8, 10);
            Quad2D qt = imageGrid.texture.quads[randomQuad];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);

            CTRRectangle rect = imageGrid.texture.quadRects[randomQuad];
            particle.width = rect.w * size;
            particle.height = rect.h * size;
        }
    }
}
