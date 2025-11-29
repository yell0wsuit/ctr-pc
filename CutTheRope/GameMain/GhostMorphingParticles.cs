using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class GhostMorphingParticles : RotateableMultiParticles
    {
        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            angle += 360f / totalParticles;
            int num = RND_RANGE(4, 6);
            Quad2D quad = imageGrid.texture.quads[num];
            Quad3D quad3D = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(quad, quad3D, particleCount);
            CTRRectangle rect = imageGrid.texture.quadRects[num];
            float scale = size + (RND_MINUS1_1 * sizeVar);
            particle.width = rect.w * scale;
            particle.height = rect.h * scale;
            particle.deltaColor = RGBAColor.MakeRGBA(0f, 0f, 0f, 0f);
        }

        public override GhostMorphingParticles InitWithTotalParticles(int numberOfParticles)
        {
            if (InitWithTotalParticlesandImageGrid(numberOfParticles, Image.Image_createWithResID(Resources.Img.ObjGhost)) != null)
            {
                size = 0.8f;
                sizeVar = 0.4f;
                angle = RND_RANGE(0, 360);
                angleVar = 15f;
                rotateSpeedVar = 30f;
                life = 0.6f;
                lifeVar = 0.15f;
                duration = 1.5f;
                speed = 60f;
                speedVar = 15f;
                startColor = RGBAColor.solidOpaqueRGBA;
                endColor = RGBAColor.transparentRGBA;
            }
            return this;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            for (int i = 0; i < particleCount; i++)
            {
                Particle particle = particles[i];
                if (particle.life > 0f)
                {
                    float fadeThreshold = 0.7f * life;
                    if (particle.life < fadeThreshold)
                    {
                        particle.deltaColor.r = (endColor.r - startColor.r) / fadeThreshold;
                        particle.deltaColor.g = (endColor.g - startColor.g) / fadeThreshold;
                        particle.deltaColor.b = (endColor.b - startColor.b) / fadeThreshold;
                        particle.deltaColor.a = (endColor.a - startColor.a) / fadeThreshold;
                    }
                    particle.dir = VectMult(particle.dir, 0.83);
                    particle.width *= 1.015f;
                    particle.height *= 1.015f;
                }
            }
        }
    }
}
