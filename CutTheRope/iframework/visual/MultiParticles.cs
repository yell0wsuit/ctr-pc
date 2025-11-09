using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;
using System;

namespace CutTheRope.iframework.visual
{
    internal class MultiParticles : Particles
    {
        public virtual Particles initWithTotalParticlesandImageGrid(int numberOfParticles, Image image)
        {
            if (this.init() == null)
            {
                return null;
            }
            this.imageGrid = image;
            this.drawer = new ImageMultiDrawer().initWithImageandCapacity(this.imageGrid, numberOfParticles);
            this.width = (int)FrameworkTypes.SCREEN_WIDTH;
            this.height = (int)FrameworkTypes.SCREEN_HEIGHT;
            this.totalParticles = numberOfParticles;
            this.particles = new Particle[this.totalParticles];
            this.colors = new RGBAColor[4 * this.totalParticles];
            if (this.particles == null || this.colors == null)
            {
                this.particles = null;
                this.colors = null;
                return null;
            }
            this.active = false;
            this.blendAdditive = false;
            OpenGL.glGenBuffers(1, ref this.colorsID);
            return this;
        }

        public override void initParticle(ref Particle particle)
        {
            Image image = this.imageGrid;
            int num = CTRMathHelper.RND(image.texture.quadsCount - 1);
            Quad2D qt = image.texture.quads[num];
            Quad3D qv = Quad3D.MakeQuad3D(0f, 0f, 0f, 0f, 0f);
            CTRRectangle rectangle = image.texture.quadRects[num];
            this.drawer.setTextureQuadatVertexQuadatIndex(qt, qv, this.particleCount);
            base.initParticle(ref particle);
            particle.width = rectangle.w * particle.size;
            particle.height = rectangle.h * particle.size;
        }

        public override void updateParticle(ref Particle p, float delta)
        {
            if (p.life > 0f)
            {
                Vector vector = CTRMathHelper.vectZero;
                if (p.pos.x != 0f || p.pos.y != 0f)
                {
                    vector = CTRMathHelper.vectNormalize(p.pos);
                }
                Vector v = vector;
                vector = CTRMathHelper.vectMult(vector, p.radialAccel);
                float num = v.x;
                v.x = 0f - v.y;
                v.y = num;
                v = CTRMathHelper.vectMult(v, p.tangentialAccel);
                Vector v2 = CTRMathHelper.vectAdd(CTRMathHelper.vectAdd(vector, v), this.gravity);
                v2 = CTRMathHelper.vectMult(v2, delta);
                p.dir = CTRMathHelper.vectAdd(p.dir, v2);
                v2 = CTRMathHelper.vectMult(p.dir, delta);
                p.pos = CTRMathHelper.vectAdd(p.pos, v2);
                p.color.r = p.color.r + p.deltaColor.r * delta;
                p.color.g = p.color.g + p.deltaColor.g * delta;
                p.color.b = p.color.b + p.deltaColor.b * delta;
                p.color.a = p.color.a + p.deltaColor.a * delta;
                p.life -= delta;
                this.drawer.vertices[this.particleIdx] = Quad3D.MakeQuad3D((double)(p.pos.x - p.width / 2f), (double)(p.pos.y - p.height / 2f), 0.0, (double)p.width, (double)p.height);
                for (int i = 0; i < 4; i++)
                {
                    this.colors[this.particleIdx * 4 + i] = p.color;
                }
                this.particleIdx++;
                return;
            }
            if (this.particleIdx != this.particleCount - 1)
            {
                this.particles[this.particleIdx] = this.particles[this.particleCount - 1];
                this.drawer.vertices[this.particleIdx] = this.drawer.vertices[this.particleCount - 1];
                this.drawer.texCoordinates[this.particleIdx] = this.drawer.texCoordinates[this.particleCount - 1];
            }
            this.particleCount--;
        }

        public override void update(float delta)
        {
            base.update(delta);
            if (this.active && this.emissionRate != 0f)
            {
                float num = 1f / this.emissionRate;
                this.emitCounter += delta;
                while (this.particleCount < this.totalParticles && this.emitCounter > num)
                {
                    this.addParticle();
                    this.emitCounter -= num;
                }
                this.elapsed += delta;
                if (this.duration != -1f && this.duration < this.elapsed)
                {
                    this.stopSystem();
                }
            }
            this.particleIdx = 0;
            while (this.particleIdx < this.particleCount)
            {
                this.updateParticle(ref this.particles[this.particleIdx], delta);
            }
            OpenGL.glBindBuffer(2, this.colorsID);
            OpenGL.glBufferData(2, this.colors, 3);
            OpenGL.glBindBuffer(2, 0U);
        }

        public override void draw()
        {
            this.preDraw();
            if (this.blendAdditive)
            {
                OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE);
            }
            else
            {
                OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            }
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(this.drawer.image.texture.name());
            OpenGL.glVertexPointer(3, 5, 0, FrameworkTypes.toFloatArray(this.drawer.vertices));
            OpenGL.glTexCoordPointer(2, 5, 0, FrameworkTypes.toFloatArray(this.drawer.texCoordinates));
            OpenGL.glEnableClientState(13);
            OpenGL.glBindBuffer(2, this.colorsID);
            OpenGL.glColorPointer(4, 5, 0, this.colors);
            OpenGL.glDrawElements(7, this.particleIdx * 6, this.drawer.indices);
            OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            OpenGL.glBindBuffer(2, 0U);
            OpenGL.glDisableClientState(13);
            this.postDraw();
        }

        public override void dealloc()
        {
            this.drawer = null;
            this.imageGrid = null;
            base.dealloc();
        }

        public ImageMultiDrawer drawer;

        public Image imageGrid;
    }
}
