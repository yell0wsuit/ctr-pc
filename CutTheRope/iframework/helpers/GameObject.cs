using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.helpers
{
    internal class GameObject : Animation
    {
        public static GameObject GameObject_createWithResID(int r)
        {
            return GameObject.GameObject_create(Application.getTexture(r));
        }

        private static GameObject GameObject_create(CTRTexture2D t)
        {
            GameObject gameObject = new();
            gameObject.initWithTexture(t);
            return gameObject;
        }

        public static GameObject GameObject_createWithResIDQuad(int r, int q)
        {
            GameObject gameObject = GameObject.GameObject_create(Application.getTexture(r));
            gameObject.setDrawQuad(q);
            return gameObject;
        }

        public override Image initWithTexture(CTRTexture2D t)
        {
            if (base.initWithTexture(t) != null)
            {
                this.bb = new CTRRectangle(0f, 0f, (float)this.width, (float)this.height);
                this.rbb = new Quad2D(this.bb.x, this.bb.y, this.bb.w, this.bb.h);
                this.anchor = 18;
                this.rotatedBB = false;
                this.topLeftCalculated = false;
            }
            return this;
        }

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

        public override void draw()
        {
            base.draw();
            if (this.isDrawBB)
            {
                this.drawBB();
            }
        }

        public override void dealloc()
        {
            NSObject.NSREL(this.mover);
            base.dealloc();
        }

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
                    this.bb = new CTRRectangle((float)list[0].intValue(), (float)list[1].intValue(), (float)list[2].intValue(), (float)list[3].intValue());
                }
                else
                {
                    this.bb = new CTRRectangle(0f, 0f, (float)this.width, (float)this.height);
                }
                this.rbb = new Quad2D(this.bb.x, this.bb.y, this.bb.w, this.bb.h);
                this.parseMover(xml);
            }
            return this;
        }

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
                mover.setPathFromStringandStart(nSString, CTRMathHelper.vect(this.x, this.y));
                this.setMover(mover);
                mover.start();
            }
        }

        public virtual void setMover(Mover m)
        {
            this.mover = m;
        }

        public virtual void setBBFromFirstQuad()
        {
            this.bb = new CTRRectangle((float)Math.Round((double)this.texture.quadOffsets[0].x), (float)Math.Round((double)this.texture.quadOffsets[0].y), this.texture.quadRects[0].w, this.texture.quadRects[0].h);
            this.rbb = new Quad2D(this.bb.x, this.bb.y, this.bb.w, this.bb.h);
        }

        public virtual void rotateWithBB(float a)
        {
            if (!this.rotatedBB)
            {
                this.rotatedBB = true;
            }
            this.rotation = a;
            Vector v = CTRMathHelper.vect(this.bb.x, this.bb.y);
            Vector v2 = CTRMathHelper.vect(this.bb.x + this.bb.w, this.bb.y);
            Vector v3 = CTRMathHelper.vect(this.bb.x + this.bb.w, this.bb.y + this.bb.h);
            Vector v4 = CTRMathHelper.vect(this.bb.x, this.bb.y + this.bb.h);
            v = CTRMathHelper.vectRotateAround(v, (double)CTRMathHelper.DEGREES_TO_RADIANS(a), (float)((double)this.width / 2.0 + (double)this.rotationCenterX), (float)((double)this.height / 2.0 + (double)this.rotationCenterY));
            v2 = CTRMathHelper.vectRotateAround(v2, (double)CTRMathHelper.DEGREES_TO_RADIANS(a), (float)((double)this.width / 2.0 + (double)this.rotationCenterX), (float)((double)this.height / 2.0 + (double)this.rotationCenterY));
            v3 = CTRMathHelper.vectRotateAround(v3, (double)CTRMathHelper.DEGREES_TO_RADIANS(a), (float)((double)this.width / 2.0 + (double)this.rotationCenterX), (float)((double)this.height / 2.0 + (double)this.rotationCenterY));
            v4 = CTRMathHelper.vectRotateAround(v4, (double)CTRMathHelper.DEGREES_TO_RADIANS(a), (float)((double)this.width / 2.0 + (double)this.rotationCenterX), (float)((double)this.height / 2.0 + (double)this.rotationCenterY));
            this.rbb.tlX = v.x;
            this.rbb.tlY = v.y;
            this.rbb.trX = v2.x;
            this.rbb.trY = v2.y;
            this.rbb.brX = v3.x;
            this.rbb.brY = v3.y;
            this.rbb.blX = v4.x;
            this.rbb.blY = v4.y;
        }

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

        public static bool objectsIntersect(GameObject o1, GameObject o2)
        {
            float num = o1.drawX + o1.bb.x;
            float num2 = o1.drawY + o1.bb.y;
            float num3 = o2.drawX + o2.bb.x;
            float num4 = o2.drawY + o2.bb.y;
            return CTRMathHelper.rectInRect(num, num2, num + o1.bb.w, num2 + o1.bb.h, num3, num4, num3 + o2.bb.w, num4 + o2.bb.h);
        }

        private static bool objectsIntersectRotated(GameObject o1, GameObject o2)
        {
            Vector vector = CTRMathHelper.vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector tr = CTRMathHelper.vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector br = CTRMathHelper.vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector bl = CTRMathHelper.vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector tl2 = CTRMathHelper.vect(o2.drawX + o2.rbb.tlX, o2.drawY + o2.rbb.tlY);
            Vector tr2 = CTRMathHelper.vect(o2.drawX + o2.rbb.trX, o2.drawY + o2.rbb.trY);
            Vector br2 = CTRMathHelper.vect(o2.drawX + o2.rbb.brX, o2.drawY + o2.rbb.brY);
            Vector bl2 = CTRMathHelper.vect(o2.drawX + o2.rbb.blX, o2.drawY + o2.rbb.blY);
            return CTRMathHelper.obbInOBB(vector, tr, br, bl, tl2, tr2, br2, bl2);
        }

        private static bool objectsIntersectRotatedWithUnrotated(GameObject o1, GameObject o2)
        {
            Vector vector = CTRMathHelper.vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector tr = CTRMathHelper.vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector br = CTRMathHelper.vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector bl = CTRMathHelper.vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector tl2 = CTRMathHelper.vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y);
            Vector tr2 = CTRMathHelper.vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y);
            Vector br2 = CTRMathHelper.vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y + o2.bb.h);
            Vector bl2 = CTRMathHelper.vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y + o2.bb.h);
            return CTRMathHelper.obbInOBB(vector, tr, br, bl, tl2, tr2, br2, bl2);
        }

        public static bool pointInObject(Vector p, GameObject o)
        {
            float checkX = o.drawX + o.bb.x;
            float checkY = o.drawY + o.bb.y;
            return CTRMathHelper.pointInRect(p.x, p.y, checkX, checkY, o.bb.w, o.bb.h);
        }

        public static bool rectInObject(float r1x, float r1y, float r2x, float r2y, GameObject o)
        {
            float num = o.drawX + o.bb.x;
            float num2 = o.drawY + o.bb.y;
            return CTRMathHelper.rectInRect(r1x, r1y, r2x, r2y, num, num2, num + o.bb.w, num2 + o.bb.h);
        }

        public const int MAX_MOVER_CAPACITY = 100;

        public int state;

        public int type;

        public Mover mover;

        public CTRRectangle bb;

        public Quad2D rbb;

        public bool rotatedBB;

        public bool isDrawBB;

        public bool topLeftCalculated;
    }
}
