using System;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Batches and animates pollen particles along bee movement paths.
    /// </summary>
    internal sealed class PollenDrawer : BaseElement
    {
        /// <summary>Quad index for the pollen sprite in the bee texture.</summary>
        private const int PollenQuad = 5;

        /// <summary>
        /// Initializes the pollen drawer, source sprite, particle storage, and color buffers.
        /// </summary>
        public PollenDrawer()
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjBee, PollenQuad);
            qw = image.width * 1.5f;
            qh = image.height * 1.5f;
            totalCapacity = 200;
            drawer = new ImageMultiDrawer().InitWithImageandCapacity(image, totalCapacity);
            pollens = new Pollen[totalCapacity];
            colors = new RGBAColor[4 * totalCapacity];
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pollens = null;
                colors = null;
                drawer?.Dispose();
                drawer = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Adds one pollen particle at a world position.
        /// </summary>
        /// <param name="v">World-space particle position.</param>
        /// <param name="pi">Path point index that owns or originated the particle.</param>
        public void AddPollenAtparentIndex(Vector v, int pi)
        {
            float scaleX = 1f;
            float scaleY = 1f;
            float[] array = [0.3f, 0.3f, 0.5f, 0.5f, 0.6f];
            int scaleOptionsCount = array.Length;
            float randomScaleX = array[RND_RANGE(0, scaleOptionsCount - 1)];
            float randomScaleY = randomScaleX;
            if (RND(1) == 1)
            {
                randomScaleX *= 1f + (RND(1) / 10f);
            }
            else
            {
                randomScaleY *= 1f + (RND(1) / 10f);
            }
            scaleX *= randomScaleX;
            scaleY *= randomScaleY;
            int quadWidth = (int)qw;
            int quadHeight = (int)qh;
            quadWidth *= (int)scaleX;
            quadHeight *= (int)scaleY;
            Pollen pollen = default;
            pollen.parentIndex = pi;
            pollen.x = v.X;
            pollen.y = v.Y;
            float fullScale = 1f;
            float scaleOffset = MathF.Min(fullScale - scaleX, fullScale - scaleY);
            float rND_0_ = RND_0_1;
            pollen.startScaleX = scaleOffset + scaleX;
            pollen.startScaleY = scaleOffset + scaleY;
            pollen.scaleX = pollen.startScaleX * rND_0_;
            pollen.scaleY = pollen.startScaleY * rND_0_;
            pollen.endScaleX = scaleX;
            pollen.endScaleY = scaleY;
            pollen.endAlpha = 0.3f;
            pollen.startAlpha = 1f;
            pollen.alpha = (0.7f * rND_0_) + 0.3f;
            Quad2D qt = drawer.image.texture.quads[PollenQuad];
            Quad3D qv = Quad3D.MakeQuad3D(v.X - (quadWidth / 2), v.Y - (quadHeight / 2), 0f, quadWidth, quadHeight);
            drawer.SetTextureQuadatVertexQuadatIndex(qt, qv, pollenCount);
            if (pollenCount >= totalCapacity)
            {
                totalCapacity = pollenCount;
                pollens = new Pollen[totalCapacity + 1];
                colors = new RGBAColor[4 * (totalCapacity + 1)];
            }
            for (int i = 0; i < 4; i++)
            {
                colors[(pollenCount * 4) + i] = RGBAColor.whiteRGBA;
            }
            pollens[pollenCount] = pollen;
            pollenCount++;
        }

        /// <summary>
        /// Fills a mover path segment with pollen particles.
        /// </summary>
        /// <param name="p1">Starting mover path point index.</param>
        /// <param name="p2">Ending mover path point index.</param>
        /// <param name="g">Grab whose mover path supplies the particle segment.</param>
        public void FillWithPolenFromPathIndexToPathIndexGrab(int p1, int p2, Grab g)
        {
            int segmentSpacing = 44;
            Vector vector = g.mover.path[p1];
            Vector vector2 = VectSub(g.mover.path[p2], vector);
            int segmentCount = (int)(VectLength(vector2) / segmentSpacing);
            Vector v3 = VectNormalize(vector2);
            for (int i = 0; i <= segmentCount; i++)
            {
                Vector v4 = VectAdd(vector, VectMult(v3, i * segmentSpacing));
                v4.X += RND_RANGE((int)RTPD(-2), (int)RTPD(2));
                v4.Y += RND_RANGE((int)RTPD(-2), (int)RTPD(2));
                AddPollenAtparentIndex(v4, p1);
            }
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            drawer.Update(delta);
            for (int i = 0; i < pollenCount; i++)
            {
                if (Mover.MoveVariableToTarget(ref pollens[i].scaleX, pollens[i].endScaleX, 1f, delta))
                {
                    (pollens[i].endScaleX, pollens[i].startScaleX) = (pollens[i].startScaleX, pollens[i].endScaleX);
                }
                if (Mover.MoveVariableToTarget(ref pollens[i].scaleY, pollens[i].endScaleY, 1f, delta))
                {
                    (pollens[i].endScaleY, pollens[i].startScaleY) = (pollens[i].startScaleY, pollens[i].endScaleY);
                }
                float pollenWidth = qw * pollens[i].scaleX;
                float pollenHeight = qh * pollens[i].scaleY;
                drawer.vertices[i] = Quad3D.MakeQuad3D(pollens[i].x - (pollenWidth / 2f), pollens[i].y - (pollenHeight / 2f), 0f, pollenWidth, pollenHeight);
                if (Mover.MoveVariableToTarget(ref pollens[i].alpha, pollens[i].endAlpha, 1f, delta))
                {
                    (pollens[i].endAlpha, pollens[i].startAlpha) = (pollens[i].startAlpha, pollens[i].endAlpha);
                }
                float alpha = pollens[i].alpha;
                for (int j = 0; j < 4; j++)
                {
                    colors[(i * 4) + j] = RGBAColor.MakeRGBA(alpha, alpha, alpha, alpha);
                }
            }
        }

        /// <inheritdoc />
        public override void Draw()
        {
            if (pollenCount >= 2)
            {
                PreDraw();
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
                Renderer.BindTexture(drawer.image.texture.Name());
                int quadCount = pollenCount - 1;
                if (quadCount > 0)
                {
                    VertexPositionColorTexture[] vertexBuffer = GetVertexBuffer(quadCount * 4);
                    Renderer.FillTexturedColoredVertices(drawer.vertices, drawer.texCoordinates, colors, vertexBuffer, quadCount);
                    Renderer.DrawTriangleList(vertexBuffer, drawer.indices, quadCount * 6);
                }
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                PostDraw();
            }
        }

        /// <summary>Drawer that owns shared image, texture coordinates, vertex, and index data.</summary>
        private ImageMultiDrawer drawer;

        /// <summary>Number of active pollen particles.</summary>
        private int pollenCount;

        /// <summary>Current storage capacity for pollen particles.</summary>
        private int totalCapacity;

        /// <summary>Particle state array.</summary>
        private Pollen[] pollens;

        /// <summary>Base pollen quad width before per-particle scaling.</summary>
        private readonly float qw;

        /// <summary>Base pollen quad height before per-particle scaling.</summary>
        private readonly float qh;

        /// <summary>Per-vertex colors used for pollen particle rendering.</summary>
        private RGBAColor[] colors;

        /// <summary>Reusable vertex buffer used for pollen draw calls.</summary>
        private VertexPositionColorTexture[] verticesCache;

        /// <summary>
        /// Gets a reusable vertex buffer with at least the requested capacity.
        /// </summary>
        /// <param name="vertexCount">Minimum vertex count required.</param>
        /// <returns>A reusable vertex buffer.</returns>
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
