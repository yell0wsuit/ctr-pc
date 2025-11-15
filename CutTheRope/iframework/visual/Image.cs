using System;
using System.Xml.Linq;

using CutTheRope.desktop;
using CutTheRope.iframework.core;

namespace CutTheRope.iframework.visual
{
    internal class Image : BaseElement
    {
        // (get) Token: 0x060001E5 RID: 485 RVA: 0x00009A46 File Offset: 0x00007C46
        public string ResName => texture != null ? texture._resName : "ERROR: texture == null";

        public static Vector GetQuadSize(int textureID, int quad)
        {
            CTRTexture2D texture2D = Application.GetTexture(textureID);
            return Vect(texture2D.quadRects[quad].w, texture2D.quadRects[quad].h);
        }

        public static Vector GetQuadOffset(int textureID, int quad)
        {
            return Application.GetTexture(textureID).quadOffsets[quad];
        }

        public static Vector GetQuadCenter(int textureID, int quad)
        {
            CTRTexture2D texture2D = Application.GetTexture(textureID);
            return VectAdd(texture2D.quadOffsets[quad], Vect(Ceil(texture2D.quadRects[quad].w / 2.0), Ceil(texture2D.quadRects[quad].h / 2.0)));
        }

        public static Vector GetRelativeQuadOffset(int textureID, int quadToCountFrom, int quad)
        {
            Vector quadOffset = GetQuadOffset(textureID, quadToCountFrom);
            return VectSub(GetQuadOffset(textureID, quad), quadOffset);
        }

        public static void SetElementPositionWithQuadCenter(BaseElement e, int textureID, int quad)
        {
            Vector quadCenter = GetQuadCenter(textureID, quad);
            e.x = quadCenter.x;
            e.y = quadCenter.y;
            e.anchor = 18;
        }

        public static void SetElementPositionWithQuadOffset(BaseElement e, int textureID, int quad)
        {
            Vector quadOffset = GetQuadOffset(textureID, quad);
            e.x = quadOffset.x;
            e.y = quadOffset.y;
        }

        public static void SetElementPositionWithRelativeQuadOffset(BaseElement e, int textureID, int quadToCountFrom, int quad)
        {
            Vector relativeQuadOffset = GetRelativeQuadOffset(textureID, quadToCountFrom, quad);
            e.x = relativeQuadOffset.x;
            e.y = relativeQuadOffset.y;
        }

        public static Image Image_create(CTRTexture2D t)
        {
            return new Image().InitWithTexture(t);
        }

        public static Image Image_createWithResID(int r)
        {
            return Image_create(Application.GetTexture(r));
        }

        public static Image Image_createWithResIDQuad(int r, int q)
        {
            Image image = Image_create(Application.GetTexture(r));
            image.SetDrawQuad(q);
            return image;
        }

        public virtual Image InitWithTexture(CTRTexture2D t)
        {
            texture = t;
            restoreCutTransparency = false;
            if (texture.quadsCount > 0)
            {
                SetDrawQuad(0);
            }
            else
            {
                SetDrawFullImage();
            }
            return this;
        }

        public virtual void SetDrawFullImage()
        {
            quadToDraw = -1;
            width = texture._realWidth;
            height = texture._realHeight;
        }

        public virtual void SetDrawQuad(int n)
        {
            quadToDraw = n;
            if (!restoreCutTransparency)
            {
                width = (int)texture.quadRects[n].w;
                height = (int)texture.quadRects[n].h;
            }
        }

        public virtual void DoRestoreCutTransparency()
        {
            if (texture.preCutSize.x != vectUndefined.x)
            {
                restoreCutTransparency = true;
                width = (int)texture.preCutSize.x;
                height = (int)texture.preCutSize.y;
            }
        }

        public override void Draw()
        {
            PreDraw();
            if (quadToDraw == -1)
            {
                GLDrawer.DrawImage(texture, drawX, drawY);
            }
            else
            {
                DrawQuad(quadToDraw);
            }
            PostDraw();
        }

        public virtual void DrawQuad(int n)
        {
            float w = texture.quadRects[n].w;
            float h = texture.quadRects[n].h;
            float num = drawX;
            float num2 = drawY;
            if (restoreCutTransparency)
            {
                num += texture.quadOffsets[n].x;
                num2 += texture.quadOffsets[n].y;
            }
            float[] pointer =
            [
                num,
                num2,
                num + w,
                num2,
                num,
                num2 + h,
                num + w,
                num2 + h
            ];
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(texture.Name());
            OpenGL.GlVertexPointer(2, 5, 0, pointer);
            OpenGL.GlTexCoordPointer(2, 5, 0, texture.quads[n].ToFloatArray());
            OpenGL.GlDrawArrays(8, 0, 4);
        }

        public override bool HandleAction(ActionData a)
        {
            if (base.HandleAction(a))
            {
                return true;
            }
            if (a.actionName == "ACTION_SET_DRAWQUAD")
            {
                SetDrawQuad(a.actionParam);
                return true;
            }
            return false;
        }

        public virtual BaseElement CreateFromXML(XElement xml)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                texture = null;
            }
            base.Dispose(disposing);
        }

        public const string ACTION_SET_DRAWQUAD = "ACTION_SET_DRAWQUAD";

        public CTRTexture2D texture;

        public bool restoreCutTransparency;

        public int quadToDraw;
    }
}
