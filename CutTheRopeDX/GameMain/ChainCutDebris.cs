using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Particle burst that flings the two chain-link debris fragments (the final two quads of the
    /// chain-cut atlas) outward with gravity and spin when a chain is cut.
    /// </summary>
    internal sealed class ChainCutDebris : RotateableMultiParticles
    {
        /// <summary>First debris quad in the chain-cut atlas.</summary>
        private const int FirstDebrisQuad = 4;

        /// <summary>Last debris quad in the chain-cut atlas.</summary>
        private const int LastDebrisQuad = 5;

        /// <inheritdoc />
        public override Particles InitWithTotalParticlesandImageGrid(int p, Image grid)
        {
            if (base.InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }

            gravity.X = 0f;
            gravity.Y = 300f;
            angle = -90f;
            angleVar = 50f;
            speed = 50f;
            speedVar = 10f;
            tangentialAccel = 0f;
            tangentialAccelVar = 1f;
            radialAccel = 0f;
            radialAccelVar = 1f;
            posVar.X = 0f;
            posVar.Y = 0f;
            life = 4f;
            lifeVar = 0.0f;
            size = 1f;
            sizeVar = 0f;
            duration = 4f;
            emissionRate = 0f;
            startColor.RedColor = 1f;
            startColor.GreenColor = 1f;
            startColor.BlueColor = 1f;
            startColor.AlphaChannel = 1f;
            startColorVar.RedColor = 0f;
            startColorVar.GreenColor = 0f;
            startColorVar.BlueColor = 0f;
            startColorVar.AlphaChannel = 0f;
            endColor.RedColor = 0f;
            endColor.GreenColor = 0f;
            endColor.BlueColor = 0f;
            endColor.AlphaChannel = 0f;
            endColorVar.RedColor = 0f;
            endColorVar.GreenColor = 0f;
            endColorVar.BlueColor = 0f;
            endColorVar.AlphaChannel = 0f;
            rotateSpeed = 0f;
            rotateSpeedVar = 100f;
            blendAdditive = false;
            return this;
        }

        /// <inheritdoc />
        public override void InitParticle(ref Particle particle)
        {
            base.InitParticle(ref particle);
            int quadIndex = RND_RANGE(FirstDebrisQuad, LastDebrisQuad);
            Quad2D qt = imageGrid.texture.quads[quadIndex];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            CTRRectangle rectangle = imageGrid.texture.quadRects[quadIndex];
            particle.width = rectangle.w * particle.size;
            particle.height = rectangle.h * particle.size;
        }

        /// <summary>
        /// Blend factors to restore after the straight-alpha debris pass so later sprites keep normal brightness.
        /// </summary>
        /// <returns>The standard premultiplied-alpha blend factors.</returns>
        internal static (BlendingFactor source, BlendingFactor destination) GetPostDrawBlendFactors()
        {
            return (BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            // Straight-alpha blend so the per-particle alpha fade reads correctly.
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
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
    }
}
