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
            if (init() == null)
            {
                return null;
            }
            image = (Image)NSRET(i);
            numberOfQuadsToDraw = -1;
            totalQuads = n;
            texCoordinates = new Quad2D[totalQuads];
            vertices = new Quad3D[totalQuads];
            indices = new short[totalQuads * 6];
            initIndices();
            return this;
        }

        private void freeWithCheck()
        {
            texCoordinates = null;
            vertices = null;
            indices = null;
        }

        public override void dealloc()
        {
            freeWithCheck();
            image = null;
            base.dealloc();
        }

        private void initIndices()
        {
            for (int i = 0; i < totalQuads; i++)
            {
                indices[i * 6] = (short)(i * 4);
                indices[i * 6 + 1] = (short)(i * 4 + 1);
                indices[i * 6 + 2] = (short)(i * 4 + 2);
                indices[i * 6 + 3] = (short)(i * 4 + 3);
                indices[i * 6 + 4] = (short)(i * 4 + 2);
                indices[i * 6 + 5] = (short)(i * 4 + 1);
            }
        }

        public void setTextureQuadatVertexQuadatIndex(Quad2D qt, Quad3D qv, int n)
        {
            if (n >= totalQuads)
            {
                resizeCapacity(n + 1);
            }
            texCoordinates[n] = qt;
            vertices[n] = qv;
        }

        public void mapTextureQuadAtXYatIndex(int q, float dx, float dy, int n)
        {
            if (n >= totalQuads)
            {
                resizeCapacity(n + 1);
            }
            texCoordinates[n] = image.texture.quads[q];
            vertices[n] = Quad3D.MakeQuad3D((double)(dx + image.texture.quadOffsets[q].x), (double)(dy + image.texture.quadOffsets[q].y), 0.0, image.texture.quadRects[q].w, image.texture.quadRects[q].h);
        }

        private void drawNumberOfQuads(int n)
        {
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(image.texture.name());
            OpenGL.glVertexPointer(3, 5, 0, toFloatArray(vertices));
            OpenGL.glTexCoordPointer(2, 5, 0, toFloatArray(texCoordinates));
            OpenGL.glDrawElements(7, n * 6, indices);
        }

        private void drawNumberOfQuadsStartingFrom(int n, int s)
        {
            throw new NotImplementedException();
        }

        public void optimize(VertexPositionNormalTexture[] v)
        {
            if (v != null && verticesOptimized == null)
            {
                verticesOptimized = v;
            }
        }

        public void drawAllQuads()
        {
            if (verticesOptimized == null)
            {
                drawNumberOfQuads(totalQuads);
                return;
            }
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(image.texture.name());
            OpenGL.Optimized_DrawTriangleList(verticesOptimized, indices);
        }

        public override void draw()
        {
            preDraw();
            OpenGL.glTranslatef(drawX, drawY, 0f);
            if (numberOfQuadsToDraw == -1)
            {
                drawAllQuads();
            }
            else if (numberOfQuadsToDraw > 0)
            {
                drawNumberOfQuads(numberOfQuadsToDraw);
            }
            OpenGL.glTranslatef(0f - drawX, 0f - drawY, 0f);
            postDraw();
        }

        private void resizeCapacity(int n)
        {
            if (n != totalQuads)
            {
                totalQuads = n;
                texCoordinates = new Quad2D[totalQuads];
                vertices = new Quad3D[totalQuads];
                indices = new short[totalQuads * 6];
                if (texCoordinates == null || vertices == null || indices == null)
                {
                    freeWithCheck();
                }
                initIndices();
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
