using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    internal class MultiParticles : Particles
    {
        public virtual Particles InitWithTotalParticlesandImageGrid(int numberOfParticles, Image image)
        {
            imageGrid = image;
            drawer = new ImageMultiDrawer().InitWithImageandCapacity(imageGrid, numberOfParticles);
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
            totalParticles = numberOfParticles;
            particles = new Particle[totalParticles];
            colors = new RGBAColor[4 * totalParticles];
            if (particles == null || colors == null)
            {
                particles = null;
                colors = null;
                return null;
            }
            active = false;
            blendAdditive = false;
            return this;
        }

        public override void InitParticle(ref Particle particle)
        {
            Image image = imageGrid;
            int quadIndex = RND(image.texture.quadsCount - 1);
            Quad2D textureQuad = image.texture.quads[quadIndex];
            Quad3D vertexQuad = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            CTRRectangle textureRect = image.texture.quadRects[quadIndex];
            drawer.SetTextureQuadatVertexQuadatIndex(textureQuad, vertexQuad, particleCount);
            base.InitParticle(ref particle);
            particle.width = textureRect.w * particle.size;
            particle.height = textureRect.h * particle.size;
        }

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
                p.life -= delta;
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3D(p.pos.X - (p.width / 2f), p.pos.Y - (p.height / 2f), 0f, p.width, p.height);
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

        public override void Draw()
        {
            PreDraw();
            if (blendAdditive)
            {
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            }
            else
            {
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            }
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(drawer.image.texture.Name());
            int quadCount = particleIdx;
            if (quadCount > 0)
            {
                VertexPositionColorTexture[] vertexBuffer = GetVertexBuffer(quadCount * 4);
                Renderer.FillTexturedColoredVertices(drawer.vertices, drawer.texCoordinates, colors, vertexBuffer, quadCount);
                Renderer.DrawTriangleList(vertexBuffer, drawer.indices, quadCount * 6);
            }
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            PostDraw();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                drawer?.Dispose();
                drawer = null;
                imageGrid = null;
            }
            base.Dispose(disposing);
        }

        public ImageMultiDrawer drawer;

        public Image imageGrid;

        private VertexPositionColorTexture[] verticesCache;

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
