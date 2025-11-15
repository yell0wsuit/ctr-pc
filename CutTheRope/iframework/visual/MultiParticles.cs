using CutTheRope.desktop;
using CutTheRope.iframework.core;

namespace CutTheRope.iframework.visual
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
            OpenGL.GlGenBuffers(1, ref colorsID);
            return this;
        }

        public override void InitParticle(ref Particle particle)
        {
            Image image = imageGrid;
            int num = RND(image.texture.quadsCount - 1);
            Quad2D qt = image.texture.quads[num];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            CTRRectangle rectangle = image.texture.quadRects[num];
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, particleCount);
            base.InitParticle(ref particle);
            particle.width = rectangle.w * particle.size;
            particle.height = rectangle.h * particle.size;
        }

        public override void UpdateParticle(ref Particle p, float delta)
        {
            if (p.life > 0f)
            {
                Vector vector = vectZero;
                if (p.pos.x != 0f || p.pos.y != 0f)
                {
                    vector = VectNormalize(p.pos);
                }
                Vector v = vector;
                vector = VectMult(vector, p.radialAccel);
                float num = v.x;
                v.x = 0f - v.y;
                v.y = num;
                v = VectMult(v, p.tangentialAccel);
                Vector v2 = VectAdd(VectAdd(vector, v), gravity);
                v2 = VectMult(v2, delta);
                p.dir = VectAdd(p.dir, v2);
                v2 = VectMult(p.dir, delta);
                p.pos = VectAdd(p.pos, v2);
                p.color.r += p.deltaColor.r * delta;
                p.color.g += p.deltaColor.g * delta;
                p.color.b += p.deltaColor.b * delta;
                p.color.a += p.deltaColor.a * delta;
                p.life -= delta;
                drawer.vertices[particleIdx] = Quad3D.MakeQuad3D((double)(p.pos.x - (p.width / 2f)), (double)(p.pos.y - (p.height / 2f)), 0.0, p.width, p.height);
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
                float num = 1f / emissionRate;
                emitCounter += delta;
                while (particleCount < totalParticles && emitCounter > num)
                {
                    _ = AddParticle();
                    emitCounter -= num;
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
            OpenGL.GlBindBuffer(2, colorsID);
            OpenGL.GlBufferData(2, colors, 3);
            OpenGL.GlBindBuffer(2, 0U);
        }

        public override void Draw()
        {
            PreDraw();
            if (blendAdditive)
            {
                OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            }
            else
            {
                OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            }
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(drawer.image.texture.Name());
            OpenGL.GlVertexPointer(3, 5, 0, ToFloatArray(drawer.vertices));
            OpenGL.GlTexCoordPointer(2, 5, 0, ToFloatArray(drawer.texCoordinates));
            OpenGL.GlEnableClientState(13);
            OpenGL.GlBindBuffer(2, colorsID);
            OpenGL.GlColorPointer(4, 5, 0, colors);
            OpenGL.GlDrawElements(7, particleIdx * 6, drawer.indices);
            OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            OpenGL.GlBindBuffer(2, 0U);
            OpenGL.GlDisableClientState(13);
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
    }
}
