using CutTheRope.Desktop;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that batch-draws multiple textured quads from a single <see cref="Image"/>.
    /// </summary>
    internal sealed class ImageMultiDrawer : BaseElement
    {
        /// <summary>
        /// Initializes the multi-drawer with an image and initial quad capacity.
        /// </summary>
        /// <param name="i">Source image containing the texture.</param>
        /// <param name="n">Initial number of quads to allocate.</param>
        /// <returns>The initialized drawer instance.</returns>
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

        /// <summary>
        /// Releases the quad arrays.
        /// </summary>
        private void FreeWithCheck()
        {
            texCoordinates = null;
            vertices = null;
            indices = null;
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Fills the index buffer with triangle-list indices for all quads.
        /// </summary>
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

        /// <summary>
        /// Sets the texture and vertex quads at the specified index, resizing if needed.
        /// </summary>
        /// <param name="qt">Texture coordinate quad.</param>
        /// <param name="qv">Vertex position quad.</param>
        /// <param name="n">Quad index.</param>
        public void SetTextureQuadatVertexQuadatIndex(Quad2D qt, Quad3D qv, int n)
        {
            if (n >= totalQuads)
            {
                ResizeCapacity(n + 1);
            }
            texCoordinates[n] = qt;
            vertices[n] = qv;
        }

        /// <summary>
        /// Maps a texture quad from the image at the specified position and index.
        /// </summary>
        /// <param name="q">Quad index within the source texture.</param>
        /// <param name="dx">X draw offset.</param>
        /// <param name="dy">Y draw offset.</param>
        /// <param name="n">Destination quad index in this drawer.</param>
        public void MapTextureQuadAtXYatIndex(int q, float dx, float dy, int n)
        {
            if (n >= totalQuads)
            {
                ResizeCapacity(n + 1);
            }
            texCoordinates[n] = image.texture.quads[q];
            vertices[n] = Quad3D.MakeQuad3D(dx + image.texture.quadOffsets[q].X, dy + image.texture.quadOffsets[q].Y, 0f, image.texture.quadRects[q].w, image.texture.quadRects[q].h);
        }

        /// <summary>
        /// Draws the specified number of quads using indexed triangle lists.
        /// </summary>
        /// <param name="n">Number of quads to draw.</param>
        private void DrawNumberOfQuads(int n)
        {
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(image.texture.Name());
            VertexPositionNormalTexture[] quadVertices = GetVertexBuffer(n * 4);
            Renderer.FillTexturedVertices(vertices, texCoordinates, quadVertices, n);
            Renderer.DrawTriangleList(quadVertices, indices, n * 6);
        }

        /// <summary>
        /// Stores a pre-built optimized vertex array for faster drawing.
        /// </summary>
        /// <param name="v">Optimized vertex array, or <see langword="null"/> to skip.</param>
        public void Optimize(VertexPositionNormalTexture[] v)
        {
            if (v != null && verticesOptimized == null)
            {
                verticesOptimized = v;
            }
        }

        /// <summary>
        /// Draws all quads, using the optimized vertex array if available.
        /// </summary>
        public void DrawAllQuads()
        {
            if (verticesOptimized == null)
            {
                DrawNumberOfQuads(totalQuads);
                return;
            }
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(image.texture.Name());
            Renderer.DrawTriangleList(verticesOptimized, indices);
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            Renderer.Translate(drawX, drawY, 0f);
            if (numberOfQuadsToDraw == -1)
            {
                DrawAllQuads();
            }
            else if (numberOfQuadsToDraw > 0)
            {
                DrawNumberOfQuads(numberOfQuadsToDraw);
            }
            Renderer.Translate(0f - drawX, 0f - drawY, 0f);
            PostDraw();
        }

        /// <summary>
        /// Resizes all quad arrays to the new capacity.
        /// </summary>
        /// <param name="n">New capacity.</param>
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

        /// <summary>
        /// Source image containing the texture atlas.
        /// </summary>
        public Image image;

        /// <summary>
        /// Total number of allocated quad slots.
        /// </summary>
        public int totalQuads;

        /// <summary>
        /// Texture coordinate quads for each slot.
        /// </summary>
        public Quad2D[] texCoordinates;

        /// <summary>
        /// Vertex position quads for each slot.
        /// </summary>
        public Quad3D[] vertices;

        /// <summary>
        /// Triangle-list index buffer.
        /// </summary>
        public short[] indices;

        /// <summary>
        /// Number of quads to draw, or -1 to draw all.
        /// </summary>
        public int numberOfQuadsToDraw;

        /// <summary>
        /// Pre-built optimized vertex array, or <see langword="null"/>.
        /// </summary>
        private VertexPositionNormalTexture[] verticesOptimized;

        /// <summary>
        /// Cached vertex buffer for non-optimized drawing.
        /// </summary>
        private VertexPositionNormalTexture[] verticesCache;

        /// <summary>
        /// Returns a vertex buffer of at least <paramref name="vertexCount"/> elements, reusing the cache.
        /// </summary>
        /// <param name="vertexCount">Minimum number of vertices needed.</param>
        /// <returns>A reusable vertex buffer with at least the requested capacity.</returns>
        private VertexPositionNormalTexture[] GetVertexBuffer(int vertexCount)
        {
            if (verticesCache == null || verticesCache.Length < vertexCount)
            {
                verticesCache = new VertexPositionNormalTexture[vertexCount];
            }
            return verticesCache;
        }
    }
}
