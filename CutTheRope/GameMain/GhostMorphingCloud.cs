using CutTheRope.Framework;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Particle effect used when a ghost morphs between visual states.
    /// </summary>
    internal sealed class GhostMorphingCloud : MultiParticles
    {
        /// <inheritdoc />
        public override void InitParticle(ref Particle particle)
        {
            angle += 360f / totalParticles;
            base.InitParticle(ref particle);
            int quadIndex = RND_RANGE(4, 6);
            Quad2D quad = imageGrid.texture.quads[quadIndex];
            Quad3D quad3D = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(quad, quad3D, particleCount);
            CTRRectangle rect = imageGrid.texture.quadRects[quadIndex];
            particle.width = rect.w * size;
            particle.height = rect.h * size;
            particle.deltaColor = RGBAColor.MakeRGBA(0f, 0f, 0f, 0f);
        }

        /// <summary>
        /// Initializes the ghost morphing cloud particle system with its texture and default emission settings.
        /// </summary>
        /// <returns>The initialized ghost morphing cloud.</returns>
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

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            for (int i = 0; i < particleCount; i++)
            {
                Particle particle = particles[i];
                if (particle.life > 0f)
                {
                    float fadeWindow = 0.2f * life;
                    if (particle.life > life - fadeWindow)
                    {
                        float growthScale = 1.025f;
                        particle.width *= growthScale;
                        particle.height *= growthScale;
                    }
                    else
                    {
                        particle.deltaColor.RedColor = (endColor.RedColor - startColor.RedColor) / fadeWindow;
                        particle.deltaColor.GreenColor = (endColor.GreenColor - startColor.GreenColor) / fadeWindow;
                        particle.deltaColor.BlueColor = (endColor.BlueColor - startColor.BlueColor) / fadeWindow;
                        particle.deltaColor.AlphaChannel = (endColor.AlphaChannel - startColor.AlphaChannel) / fadeWindow;
                        float shrinkScale = 0.98f;
                        particle.width *= shrinkScale;
                        particle.height *= shrinkScale;
                    }
                }
            }
        }

        /// <summary>
        /// Starts the ghost morphing cloud using its default particle count.
        /// </summary>
        public void StartSystem()
        {
            StartSystem(5);
        }
    }
}
