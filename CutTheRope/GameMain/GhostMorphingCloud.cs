using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class GhostMorphingCloud : MultiParticles
    {
        public override void InitParticle(ref Particle particle)
        {
            angle += 360f / totalParticles;
            base.InitParticle(ref particle);
            int num = RND_RANGE(4, 6);
            Quad2D quad = imageGrid.texture.quads[num];
            Quad3D quad3D = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(quad, quad3D, particleCount);
            CTRRectangle rect = imageGrid.texture.quadRects[num];
            particle.width = rect.w * size;
            particle.height = rect.h * size;
            particle.deltaColor = RGBAColor.MakeRGBA(0f, 0f, 0f, 0f);
        }

        public GhostMorphingCloud Init()
        {
            if (InitWithTotalParticlesandImageGrid(5, Image.Image_createWithResID(Resources.Img.ObjGhost)) != null)
            {
                angle = RND_RANGE(0, 360);
                size = 1.6f;
                angleVar = 360f;
                life = 0.5f;
                duration = 1.5f;
                speed = 30f;
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
                    float num = 0.2f * life;
                    if (particle.life > life - num)
                    {
                        float num2 = 1.025f;
                        particle.width *= num2;
                        particle.height *= num2;
                    }
                    else
                    {
                        particle.deltaColor.r = (endColor.r - startColor.r) / num;
                        particle.deltaColor.g = (endColor.g - startColor.g) / num;
                        particle.deltaColor.b = (endColor.b - startColor.b) / num;
                        particle.deltaColor.a = (endColor.a - startColor.a) / num;
                        float num3 = 0.98f;
                        particle.width *= num3;
                        particle.height *= num3;
                    }
                }
            }
        }

        public void StartSystem()
        {
            StartSystem(5);
        }
    }
}
