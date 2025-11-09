using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.helpers
{
    // Token: 0x02000061 RID: 97
    internal class GameObject : Animation
    {
        // Token: 0x0600033A RID: 826 RVA: 0x00012B54 File Offset: 0x00010D54
        public static GameObject GameObject_createWithResID(int r)
        {
            return GameObject.GameObject_create(Application.getTexture(r));
        }

        // Token: 0x0600033B RID: 827 RVA: 0x00012B61 File Offset: 0x00010D61
        private static GameObject GameObject_create(Texture2D t)
        {
            GameObject gameObject = new();
            gameObject.initWithTexture(t);
            return gameObject;
        }

        // Token: 0x0600033C RID: 828 RVA: 0x00012B70 File Offset: 0x00010D70
        public static GameObject GameObject_createWithResIDQuad(int r, int q)
        {
            GameObject gameObject = GameObject.GameObject_create(Application.getTexture(r));
            gameObject.setDrawQuad(q);
            return gameObject;
        }

        // Token: 0x0600033D RID: 829 RVA: 0x00012B84 File Offset: 0x00010D84
        public override Image initWithTexture(Texture2D t)
        {
            if (base.initWithTexture(t) != null)
            {
                this.bb = new Rectangle(0f, 0f, (float)this.width, (float)this.height);
                this.rbb = new Quad2D(this.bb.x, this.bb.y, this.bb.w, this.bb.h);
                this.anchor = 18;
                this.rotatedBB = false;
                this.topLeftCalculated = false;
            }
            return this;
        }

        // Token: 0x0600033E RID: 830 RVA: 0x00012C0C File Offset: 0x00010E0C
        public override void update(float delta)
        {
            base.update(delta);
            if (!this.topLeftCalculated)
            {
                BaseElement.calculateTopLeft(this);
                this.topLeftCalculated = true;
            }
            if (this.mover != null)
            {
                this.mover.update(delta);
                this.x = this.mover.pos.x;
                this.y = this.mover.pos.y;
                if (this.rotatedBB)
                {
                    this.rotateWithBB((float)this.mover.angle_);
                    return;
                }
                this.rotation = (float)this.mover.angle_;
            }
        }

        // Token: 0x0600033F RID: 831 RVA: 0x00012CA2 File Offset: 0x00010EA2
        public override void draw()
        {
            base.draw();
            if (this.isDrawBB)
            {
                this.drawBB();
            }
        }

        // Token: 0x06000340 RID: 832 RVA: 0x00012CB8 File Offset: 0x00010EB8
        public override void dealloc()
        {
            NSObject.NSREL(this.mover);
            base.dealloc();
        }

        // Token: 0x06000341 RID: 833 RVA: 0x00012CCC File Offset: 0x00010ECC
        public virtual GameObject initWithTextureIDxOffyOffXML(int t, int tx, int ty, XMLNode xml)
        {
            if (base.initWithTexture(Application.getTexture(t)) != null)
            {
                float num = (float)xml["x"].intValue();
                float num2 = (float)xml["y"].intValue();
                this.x = (float)tx + num;
                this.y = (float)ty + num2;
                this.type = t;
                NSString nSString = xml["bb"];
                if (nSString != null)
                {
                    List<NSString> list = nSString.componentsSeparatedByString(',');
                    this.bb = new Rectangle((float)list[0].intValue(), (float)list[1].intValue(), (float)list[2].intValue(), (float)list[3].intValue());
                }
                else
                {
                    this.bb = new Rectangle(0f, 0f, (float)this.width, (float)this.height);
                }
                this.rbb = new Quad2D(this.bb.x, this.bb.y, this.bb.w, this.bb.h);
                this.parseMover(xml);
            }
            return this;
        }

        // Token: 0x06000342 RID: 834 RVA: 0x00012DE8 File Offset: 0x00010FE8
        public virtual void parseMover(XMLNode xml)
        {
            this.rotation = xml["angle"].floatValue();
            NSString nSString = xml["path"];
            if (nSString != null && nSString.length() != 0)
            {
                int i = 100;
                if (nSString.characterAtIndex(0) == 'R')
                {
                    i = nSString.substringFromIndex(2).intValue() / 2 + 1;
                }
                float m_ = xml["moveSpeed"].floatValue();
                float r_ = xml["rotateSpeed"].floatValue();
                Mover mover = new Mover().initWithPathCapacityMoveSpeedRotateSpeed(i, m_, r_);
                mover.angle_ = (double)this.rotation;
                mover.angle_initial = mover.angle_;
                mover.setPathFromStringandStart(nSString, MathHelper.vect(this.x, this.y));
                this.setMover(mover);
                mover.start();
            }
        }

        // Token: 0x06000343 RID: 835 RVA: 0x00012EBC File Offset: 0x000110BC
        public virtual void setMover(Mover m)
        {
            this.mover = m;
        }

        // Token: 0x06000344 RID: 836 RVA: 0x00012EC8 File Offset: 0x000110C8
        public virtual void setBBFromFirstQuad()
        {
            this.bb = new Rectangle((float)Math.Round((double)this.texture.quadOffsets[0].x), (float)Math.Round((double)this.texture.quadOffsets[0].y), this.texture.quadRects[0].w, this.texture.quadRects[0].h);
            this.rbb = new Quad2D(this.bb.x, this.bb.y, this.bb.w, this.bb.h);
        }

        // Token: 0x06000345 RID: 837 RVA: 0x00012F80 File Offset: 0x00011180
        public virtual void rotateWithBB(float a)
        {
            if (!this.rotatedBB)
            {
                this.rotatedBB = true;
            }
            this.rotation = a;
            Vector v = MathHelper.vect(this.bb.x, this.bb.y);
            Vector v2 = MathHelper.vect(this.bb.x + this.bb.w, this.bb.y);
            Vector v3 = MathHelper.vect(this.bb.x + this.bb.w, this.bb.y + this.bb.h);
            Vector v4 = MathHelper.vect(this.bb.x, this.bb.y + this.bb.h);
            v = MathHelper.vectRotateAround(v, (double)MathHelper.DEGREES_TO_RADIANS(a), (float)((double)this.width / 2.0 + (double)this.rotationCenterX), (float)((double)this.height / 2.0 + (double)this.rotationCenterY));
            v2 = MathHelper.vectRotateAround(v2, (double)MathHelper.DEGREES_TO_RADIANS(a), (float)((double)this.width / 2.0 + (double)this.rotationCenterX), (float)((double)this.height / 2.0 + (double)this.rotationCenterY));
            v3 = MathHelper.vectRotateAround(v3, (double)MathHelper.DEGREES_TO_RADIANS(a), (float)((double)this.width / 2.0 + (double)this.rotationCenterX), (float)((double)this.height / 2.0 + (double)this.rotationCenterY));
            v4 = MathHelper.vectRotateAround(v4, (double)MathHelper.DEGREES_TO_RADIANS(a), (float)((double)this.width / 2.0 + (double)this.rotationCenterX), (float)((double)this.height / 2.0 + (double)this.rotationCenterY));
            this.rbb.tlX = v.x;
            this.rbb.tlY = v.y;
            this.rbb.trX = v2.x;
            this.rbb.trY = v2.y;
            this.rbb.brX = v3.x;
            this.rbb.brY = v3.y;
            this.rbb.blX = v4.x;
            this.rbb.blY = v4.y;
        }

        // Token: 0x06000346 RID: 838 RVA: 0x000131D4 File Offset: 0x000113D4
        public virtual void drawBB()
        {
            OpenGL.glDisable(0);
            if (this.rotatedBB)
            {
                OpenGL.drawSegment(this.drawX + this.rbb.tlX, this.drawY + this.rbb.tlY, this.drawX + this.rbb.trX, this.drawY + this.rbb.trY, RGBAColor.redRGBA);
                OpenGL.drawSegment(this.drawX + this.rbb.trX, this.drawY + this.rbb.trY, this.drawX + this.rbb.brX, this.drawY + this.rbb.brY, RGBAColor.redRGBA);
                OpenGL.drawSegment(this.drawX + this.rbb.brX, this.drawY + this.rbb.brY, this.drawX + this.rbb.blX, this.drawY + this.rbb.blY, RGBAColor.redRGBA);
                OpenGL.drawSegment(this.drawX + this.rbb.blX, this.drawY + this.rbb.blY, this.drawX + this.rbb.tlX, this.drawY + this.rbb.tlY, RGBAColor.redRGBA);
            }
            else
            {
                GLDrawer.drawRect(this.drawX + this.bb.x, this.drawY + this.bb.y, this.bb.w, this.bb.h, RGBAColor.redRGBA);
            }
            OpenGL.glEnable(0);
            OpenGL.glColor4f(Color.White);
        }

        // Token: 0x06000347 RID: 839 RVA: 0x00013390 File Offset: 0x00011590
        public static bool objectsIntersect(GameObject o1, GameObject o2)
        {
            float num = o1.drawX + o1.bb.x;
            float num2 = o1.drawY + o1.bb.y;
            float num3 = o2.drawX + o2.bb.x;
            float num4 = o2.drawY + o2.bb.y;
            return MathHelper.rectInRect(num, num2, num + o1.bb.w, num2 + o1.bb.h, num3, num4, num3 + o2.bb.w, num4 + o2.bb.h);
        }

        // Token: 0x06000348 RID: 840 RVA: 0x00013428 File Offset: 0x00011628
        private static bool objectsIntersectRotated(GameObject o1, GameObject o2)
        {
            Vector vector = MathHelper.vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector tr = MathHelper.vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector br = MathHelper.vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector bl = MathHelper.vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector tl2 = MathHelper.vect(o2.drawX + o2.rbb.tlX, o2.drawY + o2.rbb.tlY);
            Vector tr2 = MathHelper.vect(o2.drawX + o2.rbb.trX, o2.drawY + o2.rbb.trY);
            Vector br2 = MathHelper.vect(o2.drawX + o2.rbb.brX, o2.drawY + o2.rbb.brY);
            Vector bl2 = MathHelper.vect(o2.drawX + o2.rbb.blX, o2.drawY + o2.rbb.blY);
            return MathHelper.obbInOBB(vector, tr, br, bl, tl2, tr2, br2, bl2);
        }

        // Token: 0x06000349 RID: 841 RVA: 0x00013598 File Offset: 0x00011798
        private static bool objectsIntersectRotatedWithUnrotated(GameObject o1, GameObject o2)
        {
            Vector vector = MathHelper.vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector tr = MathHelper.vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector br = MathHelper.vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector bl = MathHelper.vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector tl2 = MathHelper.vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y);
            Vector tr2 = MathHelper.vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y);
            Vector br2 = MathHelper.vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y + o2.bb.h);
            Vector bl2 = MathHelper.vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y + o2.bb.h);
            return MathHelper.obbInOBB(vector, tr, br, bl, tl2, tr2, br2, bl2);
        }

        // Token: 0x0600034A RID: 842 RVA: 0x00013738 File Offset: 0x00011938
        public static bool pointInObject(Vector p, GameObject o)
        {
            float checkX = o.drawX + o.bb.x;
            float checkY = o.drawY + o.bb.y;
            return MathHelper.pointInRect(p.x, p.y, checkX, checkY, o.bb.w, o.bb.h);
        }

        // Token: 0x0600034B RID: 843 RVA: 0x00013794 File Offset: 0x00011994
        public static bool rectInObject(float r1x, float r1y, float r2x, float r2y, GameObject o)
        {
            float num = o.drawX + o.bb.x;
            float num2 = o.drawY + o.bb.y;
            return MathHelper.rectInRect(r1x, r1y, r2x, r2y, num, num2, num + o.bb.w, num2 + o.bb.h);
        }

        // Token: 0x04000277 RID: 631
        public const int MAX_MOVER_CAPACITY = 100;

        // Token: 0x04000278 RID: 632
        public int state;

        // Token: 0x04000279 RID: 633
        public int type;

        // Token: 0x0400027A RID: 634
        public Mover mover;

        // Token: 0x0400027B RID: 635
        public Rectangle bb;

        // Token: 0x0400027C RID: 636
        public Quad2D rbb;

        // Token: 0x0400027D RID: 637
        public bool rotatedBB;

        // Token: 0x0400027E RID: 638
        public bool isDrawBB;

        // Token: 0x0400027F RID: 639
        public bool topLeftCalculated;
    }
}
