using CutTheRope.desktop;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.iframework.visual
{
    internal sealed class ImageMultiDrawer : BaseElement
    {
        public ImageMultiDrawer InitWithImageandCapacity(Image i, int n)
        {
            image = i;
            numberOfQuadsToDraw = -1;
            totalQuads = n;
            texCoordinates = new Quad2D[totalQuads];
            vertices = new Quad3D[totalQuads];
            indices = new short[totalQuads * 6];
            InitIndices();
            return this;
        }

        private void FreeWithCheck()
        {
            texCoordinates = null;
            vertices = null;
            indices = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FreeWithCheck();
                image = null;
                verticesOptimized = null;
            }
            base.Dispose(disposing);
        }

        private void InitIndices()
        {
            for (int i = 0; i < totalQuads; i++)
            {
                indices[i * 6] = (short)(i * 4);
                indices[(i * 6) + 1] = (short)((i * 4) + 1);
                indices[(i * 6) + 2] = (short)((i * 4) + 2);
                indices[(i * 6) + 3] = (short)((i * 4) + 3);
                indices[(i * 6) + 4] = (short)((i * 4) + 2);
                indices[(i * 6) + 5] = (short)((i * 4) + 1);
            }
        }

        public void SetTextureQuadatVertexQuadatIndex(Quad2D qt, Quad3D qv, int n)
        {
            if (n >= totalQuads)
            {
                ResizeCapacity(n + 1);
            }
            texCoordinates[n] = qt;
            vertices[n] = qv;
        }

        public void MapTextureQuadAtXYatIndex(int q, float dx, float dy, int n)
        {
            if (n >= totalQuads)
            {
                ResizeCapacity(n + 1);
            }
            texCoordinates[n] = image.texture.quads[q];
            vertices[n] = Quad3D.MakeQuad3D((double)(dx + image.texture.quadOffsets[q].x), (double)(dy + image.texture.quadOffsets[q].y), 0.0, image.texture.quadRects[q].w, image.texture.quadRects[q].h);
        }

        private void DrawNumberOfQuads(int n)
        {
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(image.texture.Name());
            OpenGL.GlVertexPointer(3, 5, 0, ToFloatArray(vertices));
            OpenGL.GlTexCoordPointer(2, 5, 0, ToFloatArray(texCoordinates));
            OpenGL.GlDrawElements(7, n * 6, indices);
        }

        public void Optimize(VertexPositionNormalTexture[] v)
        {
            if (v != null && verticesOptimized == null)
            {
                verticesOptimized = v;
            }
        }

        public void DrawAllQuads()
        {
            if (verticesOptimized == null)
            {
                DrawNumberOfQuads(totalQuads);
                return;
            }
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(image.texture.Name());
            OpenGL.Optimized_DrawTriangleList(verticesOptimized, indices);
        }

        public override void Draw()
        {
            PreDraw();
            OpenGL.GlTranslatef(drawX, drawY, 0f);
            if (numberOfQuadsToDraw == -1)
            {
                DrawAllQuads();
            }
            else if (numberOfQuadsToDraw > 0)
            {
                DrawNumberOfQuads(numberOfQuadsToDraw);
            }
            OpenGL.GlTranslatef(0f - drawX, 0f - drawY, 0f);
            PostDraw();
        }

        private void ResizeCapacity(int n)
        {
            if (n != totalQuads)
            {
                totalQuads = n;
                texCoordinates = new Quad2D[totalQuads];
                vertices = new Quad3D[totalQuads];
                indices = new short[totalQuads * 6];
                if (texCoordinates == null || vertices == null || indices == null)
                {
                    FreeWithCheck();
                }
                InitIndices();
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
