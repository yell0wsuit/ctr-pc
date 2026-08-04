using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Additive sparkle burst spawned alongside the chain-cut debris (the original
    /// <c>ChainFlashLight</c>), using the chain-cut atlas spark quads (1-3).
    /// </summary>
    internal sealed class ChainFlashLight : RotateableMultiParticles
    {
        /// <summary>First spark quad in the chain-cut atlas.</summary>
        private const int FirstSparkQuad = 1;

        /// <summary>Last spark quad in the chain-cut atlas.</summary>
        private const int LastSparkQuad = 3;

        /// <inheritdoc />
        public override Particles InitWithTotalParticlesandImageGrid(int p, Image grid)
        {
            if (base.InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }

            gravity.X = 0f;
            gravity.Y = 90f;
            angle = -90f;
            angleVar = 150f;
            speed = 255f;
            speedVar = 10f;
            tangentialAccel = 0f;
            tangentialAccelVar = 1f;
            radialAccel = 0f;
            radialAccelVar = 1f;
            posVar.X = 15f;
            posVar.Y = 15f;
            life = 0.6f;
            lifeVar = 0f;
            size = 1f;
            sizeVar = 0f;
            duration = 0.6f;
            emissionRate = 0f;
            startColor.RedColor = 0.6f;
            startColor.GreenColor = 0.6f;
            startColor.BlueColor = 0.6f;
            startColor.AlphaChannel = 0.6f;
            startColorVar.RedColor = 0.4f;
            startColorVar.GreenColor = 0.4f;
            startColorVar.BlueColor = 0.4f;
            startColorVar.AlphaChannel = 0.4f;
            endColor.RedColor = 1f;
            endColor.GreenColor = 1f;
            endColor.BlueColor = 1f;
            endColor.AlphaChannel = 1f;
            endColorVar.RedColor = 0f;
            endColorVar.GreenColor = 0f;
            endColorVar.BlueColor = 0f;
            endColorVar.AlphaChannel = 0f;
            rotateSpeed = 200f;
            rotateSpeedVar = 396f;
            blendAdditive = true;
            fadePending = true;
            fadeElapsed = 0f;
            return this;
        }

        /// <inheritdoc />
        public override void StartSystem(int initialParticles)
        {
            fadePending = true;
            fadeElapsed = 0f;
            base.StartSystem(initialParticles);
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            fadeElapsed += delta;
            base.Update(delta);
            if (!fadePending || fadeElapsed <= 0.1f)
            {
                return;
            }

            fadePending = false;
            for (int i = 0; i < particleCount; i++)
            {
                if (particles[i].life <= 0f)
                {
                    continue;
                }

                float fadeRate = -1f / particles[i].life;
                particles[i].deltaColor.RedColor = fadeRate;
                particles[i].deltaColor.GreenColor = fadeRate;
                particles[i].deltaColor.BlueColor = fadeRate;
                particles[i].deltaColor.AlphaChannel = fadeRate;
            }
        }

        /// <summary>
        /// Blend factors to restore after the additive spark pass so later sprites keep normal brightness.
        /// </summary>
        /// <returns>The standard premultiplied-alpha blend factors.</returns>
        internal static (BlendingFactor source, BlendingFactor destination) GetPostDrawBlendFactors()
        {
            return (BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <inheritdoc />
        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            int quadIndex = RND_RANGE(FirstSparkQuad, LastSparkQuad);
            Quad2D qt = imageGrid.texture.quads[quadIndex];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            CTRRectangle rectangle = imageGrid.texture.quadRects[quadIndex];
            particle.width = rectangle.w * particle.size;
            particle.height = rectangle.h * particle.size;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            // Additive blend (the original sets the additive flag) for a glowing spark.
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(drawer.image.texture.Name());
            int quadCount = particleIdx;
            if (quadCount > 0)
            {
                VertexPositionColorTexture[] vertexBuffer = GetVertexBuffer(quadCount * 4);
                Renderer.FillTexturedColoredVertices(drawer.vertices, drawer.texCoordinates, colors, vertexBuffer, quadCount);
                Renderer.DrawTriangleList(vertexBuffer, drawer.indices, quadCount * 6);
            }
            (BlendingFactor source, BlendingFactor destination) = GetPostDrawBlendFactors();
            Renderer.SetBlendFunc(source, destination);
            PostDraw();
        }

        /// <summary>Cached vertex array reused across draw calls to avoid per-frame allocation.</summary>
        private VertexPositionColorTexture[] verticesCache;

        /// <summary>
        /// Returns a cached vertex array, reallocating if the cache is too small.
        /// </summary>
        /// <param name="vertexCount">Minimum required capacity.</param>
        /// <returns>The cached or newly allocated array.</returns>
        private VertexPositionColorTexture[] GetVertexBuffer(int vertexCount)
        {
            if (verticesCache == null || verticesCache.Length < vertexCount)
            {
                verticesCache = new VertexPositionColorTexture[vertexCount];
            }
            return verticesCache;
        }

        /// <summary>Whether the original delayed color fade has not yet been applied.</summary>
        private bool fadePending;

        /// <summary>Elapsed time used by the original delayed fade trigger.</summary>
        private float fadeElapsed;
    }
}
