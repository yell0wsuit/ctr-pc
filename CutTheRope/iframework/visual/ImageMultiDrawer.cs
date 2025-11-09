using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CutTheRope.iframework.visual
{
    internal class ImageMultiDrawer : BaseElement
    {
        public virtual ImageMultiDrawer initWithImageandCapacity(Image i, int n)
        {
            if (this.init() == null)
            {
                return null;
            }
            this.image = (Image)NSObject.NSRET(i);
            this.numberOfQuadsToDraw = -1;
            this.totalQuads = n;
            this.texCoordinates = new Quad2D[this.totalQuads];
            this.vertices = new Quad3D[this.totalQuads];
            this.indices = new short[this.totalQuads * 6];
            this.initIndices();
            return this;
        }

        private void freeWithCheck()
        {
            this.texCoordinates = null;
            this.vertices = null;
            this.indices = null;
        }

        public override void dealloc()
        {
            this.freeWithCheck();
            this.image = null;
            base.dealloc();
        }

        private void initIndices()
        {
            for (int i = 0; i < this.totalQuads; i++)
            {
                this.indices[i * 6] = (short)(i * 4);
                this.indices[i * 6 + 1] = (short)(i * 4 + 1);
                this.indices[i * 6 + 2] = (short)(i * 4 + 2);
                this.indices[i * 6 + 3] = (short)(i * 4 + 3);
                this.indices[i * 6 + 4] = (short)(i * 4 + 2);
                this.indices[i * 6 + 5] = (short)(i * 4 + 1);
            }
        }

        public void setTextureQuadatVertexQuadatIndex(Quad2D qt, Quad3D qv, int n)
        {
            if (n >= this.totalQuads)
            {
                this.resizeCapacity(n + 1);
            }
            this.texCoordinates[n] = qt;
            this.vertices[n] = qv;
        }

        public void mapTextureQuadAtXYatIndex(int q, float dx, float dy, int n)
        {
            if (n >= this.totalQuads)
            {
                this.resizeCapacity(n + 1);
            }
            this.texCoordinates[n] = this.image.texture.quads[q];
            this.vertices[n] = Quad3D.MakeQuad3D((double)(dx + this.image.texture.quadOffsets[q].x), (double)(dy + this.image.texture.quadOffsets[q].y), 0.0, (double)this.image.texture.quadRects[q].w, (double)this.image.texture.quadRects[q].h);
        }

        private void drawNumberOfQuads(int n)
        {
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(this.image.texture.name());
            OpenGL.glVertexPointer(3, 5, 0, FrameworkTypes.toFloatArray(this.vertices));
            OpenGL.glTexCoordPointer(2, 5, 0, FrameworkTypes.toFloatArray(this.texCoordinates));
            OpenGL.glDrawElements(7, n * 6, this.indices);
        }

        private void drawNumberOfQuadsStartingFrom(int n, int s)
        {
            throw new NotImplementedException();
        }

        public void optimize(VertexPositionNormalTexture[] v)
        {
            if (v != null && this.verticesOptimized == null)
            {
                this.verticesOptimized = v;
            }
        }

        public void drawAllQuads()
        {
            if (this.verticesOptimized == null)
            {
                this.drawNumberOfQuads(this.totalQuads);
                return;
            }
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(this.image.texture.name());
            OpenGL.Optimized_DrawTriangleList(this.verticesOptimized, this.indices);
        }

        public override void draw()
        {
            this.preDraw();
            OpenGL.glTranslatef(this.drawX, this.drawY, 0f);
            if (this.numberOfQuadsToDraw == -1)
            {
                this.drawAllQuads();
            }
            else if (this.numberOfQuadsToDraw > 0)
            {
                this.drawNumberOfQuads(this.numberOfQuadsToDraw);
            }
            OpenGL.glTranslatef(0f - this.drawX, 0f - this.drawY, 0f);
            this.postDraw();
        }

        private void resizeCapacity(int n)
        {
            if (n != this.totalQuads)
            {
                this.totalQuads = n;
                this.texCoordinates = new Quad2D[this.totalQuads];
                this.vertices = new Quad3D[this.totalQuads];
                this.indices = new short[this.totalQuads * 6];
                if (this.texCoordinates == null || this.vertices == null || this.indices == null)
                {
                    this.freeWithCheck();
                }
                this.initIndices();
            }
        }

        public Image image;

        public int totalQuads;

        public Quad2D[] texCoordinates;

        public Quad3D[] vertices;

        public short[] indices;

        public int numberOfQuadsToDraw;

        private VertexPositionNormalTexture[] verticesOptimized;
    }
}
