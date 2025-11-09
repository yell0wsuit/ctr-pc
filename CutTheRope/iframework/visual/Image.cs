using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;
using System;

namespace CutTheRope.iframework.visual
{
    internal class Image : BaseElement
    {
        // (get) Token: 0x060001E5 RID: 485 RVA: 0x00009A46 File Offset: 0x00007C46
        public string _ResName
        {
            get
            {
                if (this.texture != null)
                {
                    return this.texture._resName;
                }
                return "ERROR: texture == null";
            }
        }

        public static Vector getQuadSize(int textureID, int quad)
        {
            Texture2D texture2D = Application.getTexture(textureID);
            return CTRMathHelper.vect(texture2D.quadRects[quad].w, texture2D.quadRects[quad].h);
        }

        public static Vector getQuadOffset(int textureID, int quad)
        {
            return Application.getTexture(textureID).quadOffsets[quad];
        }

        public static Vector getQuadCenter(int textureID, int quad)
        {
            Texture2D texture2D = Application.getTexture(textureID);
            return CTRMathHelper.vectAdd(texture2D.quadOffsets[quad], CTRMathHelper.vect(CTRMathHelper.ceil((double)texture2D.quadRects[quad].w / 2.0), CTRMathHelper.ceil((double)texture2D.quadRects[quad].h / 2.0)));
        }

        public static Vector getRelativeQuadOffset(int textureID, int quadToCountFrom, int quad)
        {
            Vector quadOffset = Image.getQuadOffset(textureID, quadToCountFrom);
            return CTRMathHelper.vectSub(Image.getQuadOffset(textureID, quad), quadOffset);
        }

        public static void setElementPositionWithQuadCenter(BaseElement e, int textureID, int quad)
        {
            Vector quadCenter = Image.getQuadCenter(textureID, quad);
            e.x = quadCenter.x;
            e.y = quadCenter.y;
            e.anchor = 18;
        }

        public static void setElementPositionWithQuadOffset(BaseElement e, int textureID, int quad)
        {
            Vector quadOffset = Image.getQuadOffset(textureID, quad);
            e.x = quadOffset.x;
            e.y = quadOffset.y;
        }

        public static void setElementPositionWithRelativeQuadOffset(BaseElement e, int textureID, int quadToCountFrom, int quad)
        {
            Vector relativeQuadOffset = Image.getRelativeQuadOffset(textureID, quadToCountFrom, quad);
            e.x = relativeQuadOffset.x;
            e.y = relativeQuadOffset.y;
        }

        public static Image Image_create(Texture2D t)
        {
            return new Image().initWithTexture(t);
        }

        public static Image Image_createWithResID(int r)
        {
            return Image.Image_create(Application.getTexture(r));
        }

        public static Image Image_createWithResIDQuad(int r, int q)
        {
            Image image = Image.Image_create(Application.getTexture(r));
            image.setDrawQuad(q);
            return image;
        }

        public virtual Image initWithTexture(Texture2D t)
        {
            if (this.init() != null)
            {
                this.texture = t;
                NSObject.NSRET(this.texture);
                this.restoreCutTransparency = false;
                if (this.texture.quadsCount > 0)
                {
                    this.setDrawQuad(0);
                }
                else
                {
                    this.setDrawFullImage();
                }
            }
            return this;
        }

        public virtual void setDrawFullImage()
        {
            this.quadToDraw = -1;
            this.width = this.texture._realWidth;
            this.height = this.texture._realHeight;
        }

        public virtual void setDrawQuad(int n)
        {
            this.quadToDraw = n;
            if (!this.restoreCutTransparency)
            {
                this.width = (int)this.texture.quadRects[n].w;
                this.height = (int)this.texture.quadRects[n].h;
            }
        }

        public virtual void doRestoreCutTransparency()
        {
            if (this.texture.preCutSize.x != CTRMathHelper.vectUndefined.x)
            {
                this.restoreCutTransparency = true;
                this.width = (int)this.texture.preCutSize.x;
                this.height = (int)this.texture.preCutSize.y;
            }
        }

        public override void draw()
        {
            this.preDraw();
            if (this.quadToDraw == -1)
            {
                GLDrawer.drawImage(this.texture, this.drawX, this.drawY);
            }
            else
            {
                this.drawQuad(this.quadToDraw);
            }
            this.postDraw();
        }

        public virtual void drawQuad(int n)
        {
            float w = this.texture.quadRects[n].w;
            float h = this.texture.quadRects[n].h;
            float num = this.drawX;
            float num2 = this.drawY;
            if (this.restoreCutTransparency)
            {
                num += this.texture.quadOffsets[n].x;
                num2 += this.texture.quadOffsets[n].y;
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
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(this.texture.name());
            OpenGL.glVertexPointer(2, 5, 0, pointer);
            OpenGL.glTexCoordPointer(2, 5, 0, this.texture.quads[n].toFloatArray());
            OpenGL.glDrawArrays(8, 0, 4);
        }

        public override bool handleAction(ActionData a)
        {
            if (base.handleAction(a))
            {
                return true;
            }
            if (a.actionName == "ACTION_SET_DRAWQUAD")
            {
                this.setDrawQuad(a.actionParam);
                return true;
            }
            return false;
        }

        public virtual BaseElement createFromXML(XMLNode xml)
        {
            throw new NotImplementedException();
        }

        public override void dealloc()
        {
            NSObject.NSREL(this.texture);
            base.dealloc();
        }

        public const string ACTION_SET_DRAWQUAD = "ACTION_SET_DRAWQUAD";

        public Texture2D texture;

        public bool restoreCutTransparency;

        public int quadToDraw;
    }
}
