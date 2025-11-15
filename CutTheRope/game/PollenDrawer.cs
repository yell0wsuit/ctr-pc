using System;

using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed class PollenDrawer : BaseElement
    {
        public PollenDrawer()
        {
            Image image = Image.Image_createWithResID(99);
            qw = image.width * 1.5f;
            qh = image.height * 1.5f;
            totalCapacity = 200;
            drawer = new ImageMultiDrawer().InitWithImageandCapacity(image, totalCapacity);
            pollens = new Pollen[totalCapacity];
            colors = new RGBAColor[4 * totalCapacity];
            OpenGL.GlGenBuffers(1, ref colorsID);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                pollens = null;
                if (colorsID != 0)
                {
                    OpenGL.GlDeleteBuffers(1, ref colorsID);
                    colorsID = 0;
                }
                colors = null;
                if (verticesID != 0)
                {
                    OpenGL.GlDeleteBuffers(1, ref verticesID);
                    verticesID = 0;
                }
                vertices = null;
                drawer?.Dispose();
                drawer = null;
            }
            base.Dispose(disposing);
        }

        public void AddPollenAtparentIndex(Vector v, int pi)
        {
            float num = 1f;
            float num2 = 1f;
            float[] array = [0.3f, 0.3f, 0.5f, 0.5f, 0.6f];
            int num3 = array.Length;
            float num4 = array[RND_RANGE(0, num3 - 1)];
            float num5 = num4;
            if (RND(1) == 1)
            {
                num4 *= 1f + (RND(1) / 10f);
            }
            else
            {
                num5 *= 1f + (RND(1) / 10f);
            }
            num *= num4;
            num2 *= num5;
            int num6 = (int)qw;
            int num7 = (int)qh;
            num6 *= (int)num;
            num7 *= (int)num2;
            Pollen pollen = default;
            pollen.parentIndex = pi;
            pollen.x = v.x;
            pollen.y = v.y;
            float num8 = 1f;
            float num9 = Math.Min(num8 - num, num8 - num2);
            float rND_0_ = RND_0_1;
            pollen.startScaleX = num9 + num;
            pollen.startScaleY = num9 + num2;
            pollen.scaleX = pollen.startScaleX * rND_0_;
            pollen.scaleY = pollen.startScaleY * rND_0_;
            pollen.endScaleX = num;
            pollen.endScaleY = num2;
            pollen.endAlpha = 0.3f;
            pollen.startAlpha = 1f;
            pollen.alpha = (0.7f * rND_0_) + 0.3f;
            Quad2D qt = drawer.image.texture.quads[0];
            Quad3D qv = Quad3D.MakeQuad3D((double)(v.x - (num6 / 2)), (double)(v.y - (num7 / 2)), 0.0, num6, num7);
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

        public void FillWithPolenFromPathIndexToPathIndexGrab(int p1, int p2, Grab g)
        {
            int num = 44;
            Vector vector = g.mover.path[p1];
            Vector vector2 = VectSub(g.mover.path[p2], vector);
            int num2 = (int)(VectLength(vector2) / num);
            Vector v3 = VectNormalize(vector2);
            for (int i = 0; i <= num2; i++)
            {
                Vector v4 = VectAdd(vector, VectMult(v3, i * num));
                v4.x += RND_RANGE((int)RTPD(-2.0), (int)RTPD(2.0));
                v4.y += RND_RANGE((int)RTPD(-2.0), (int)RTPD(2.0));
                AddPollenAtparentIndex(v4, p1);
            }
        }

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
                float num = qw * pollens[i].scaleX;
                float num2 = qh * pollens[i].scaleY;
                drawer.vertices[i] = Quad3D.MakeQuad3D((double)(pollens[i].x - (num / 2f)), (double)(pollens[i].y - (num2 / 2f)), 0.0, (double)num, (double)num2);
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
            OpenGL.GlBindBuffer(2, colorsID);
            OpenGL.GlBufferData(2, colors, 3);
            OpenGL.GlBindBuffer(2, 0U);
        }

        public override void Draw()
        {
            if (pollenCount >= 2)
            {
                PreDraw();
                OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
                OpenGL.GlEnable(0);
                OpenGL.GlBindTexture(drawer.image.texture.Name());
                OpenGL.GlVertexPointer(3, 5, 0, ToFloatArray(drawer.vertices));
                OpenGL.GlTexCoordPointer(2, 5, 0, ToFloatArray(drawer.texCoordinates));
                OpenGL.GlEnableClientState(13);
                OpenGL.GlBindBuffer(2, colorsID);
                OpenGL.GlBufferData(2, colors, 3);
                OpenGL.GlColorPointer(4, 5, 0, colors);
                OpenGL.GlDrawElements(7, (pollenCount - 1) * 6, drawer.indices);
                OpenGL.GlBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                OpenGL.GlBindBuffer(2, 0U);
                OpenGL.GlDisableClientState(13);
                PostDraw();
            }
        }

        private ImageMultiDrawer drawer;

        private int pollenCount;

        private int totalCapacity;

        private Pollen[] pollens;

        private readonly float qw;

        private readonly float qh;

        private RGBAColor[] colors;

        private uint colorsID;

        private PointSprite[] vertices;

        private uint verticesID;
    }
}
