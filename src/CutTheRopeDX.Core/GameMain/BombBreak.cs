using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Particle burst that scatters the bomb's casing fragments (the bomb atlas quads after the
    /// intact body) when it detonates. The fragments hold full colour for
    /// <see cref="FadeStartDelay"/> and only then start fading, which is why the system drives the
    /// fade itself instead of leaning on an <c>endColor</c>.
    /// </summary>
    internal sealed class BombBreak : RotateableMultiParticles
    {
        /// <summary>First debris quad in the bomb atlas; quad 0 is the intact bomb.</summary>
        private const int FirstDebrisQuad = 1;

        /// <summary>Last debris quad in the bomb atlas.</summary>
        private const int LastDebrisQuad = 5;

        /// <summary>Seconds the fragments stay at full colour before they begin fading out.</summary>
        private const float FadeStartDelay = 0.5f;

        /// <summary>Seconds elapsed since the burst started.</summary>
        private float elapsedSinceStart;

        /// <summary>False once the fade has been armed, so it is armed only once.</summary>
        private bool fadePending = true;

        /// <inheritdoc />
        public override Particles InitWithTotalParticlesandImageGrid(int p, Image grid)
        {
            if (base.InitWithTotalParticlesandImageGrid(p, grid) == null)
            {
                return null;
            }

            duration = 1.5f;
            life = 1.5f;
            lifeVar = 0f;
            gravity.X = 57f;
            gravity.Y = 20f;
            posVar.X = 50f;
            posVar.Y = 50f;
            angle = -90f;
            angleVar = 180f;
            speed = 100f;
            speedVar = 70f;
            radialAccel = 0f;
            radialAccelVar = 1f;
            tangentialAccel = 0f;
            tangentialAccelVar = 1f;
            size = 0.5f;
            sizeVar = 0.5f;
            emissionRate = 0f;
            startColor.RedColor = 1f;
            startColor.GreenColor = 1f;
            startColor.BlueColor = 1f;
            startColor.AlphaChannel = 1f;
            startColorVar.RedColor = 0f;
            startColorVar.GreenColor = 0f;
            startColorVar.BlueColor = 0f;
            startColorVar.AlphaChannel = 0f;
            endColor.RedColor = 1f;
            endColor.GreenColor = 1f;
            endColor.BlueColor = 1f;
            endColor.AlphaChannel = 1f;
            endColorVar.RedColor = 0f;
            endColorVar.GreenColor = 0f;
            endColorVar.BlueColor = 0f;
            endColorVar.AlphaChannel = 0f;
            rotateSpeed = 380f;
            rotateSpeedVar = 0f;
            blendAdditive = false;
            // The original's blending mode 3: premultiplied additive, so the fragments read as hot
            // while they hold full colour and disappear as the fade takes their alpha down.
            blendingMode = 3;
            return this;
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            elapsedSinceStart += delta;
            base.Update(delta);

            if (fadePending && elapsedSinceStart > FadeStartDelay)
            {
                fadePending = false;
                ArmFadeOut();
            }
        }

        /// <summary>
        /// Gives every live fragment a colour rate that runs it to zero over its remaining life.
        /// </summary>
        private void ArmFadeOut()
        {
            for (int i = 0; i < particleIdx; i++)
            {
                ref Particle particle = ref particles[i];
                float rate = -1f / particle.life;
                particle.deltaColor.RedColor = rate;
                particle.deltaColor.GreenColor = rate;
                particle.deltaColor.BlueColor = rate;
                particle.deltaColor.AlphaChannel = rate;
            }
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

        /// <inheritdoc />
        public override void Draw()
        {
            // PreDraw applies blendingMode 3 and PostDraw puts the previous blend back.
            PreDraw();
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(drawer.image.texture.Name());
            int quadCount = particleIdx;
            if (quadCount > 0)
            {
                VertexPositionColorTexture[] vertexBuffer = GetVertexBuffer(quadCount * 4);
                Renderer.FillTexturedColoredVertices(drawer.vertices, drawer.texCoordinates, colors, vertexBuffer, quadCount);
                Renderer.DrawTriangleList(vertexBuffer, drawer.indices, quadCount * 6);
            }
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
