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
            return GameObject_create(Application.getTexture(r));
        }

        private static GameObject GameObject_create(CTRTexture2D t)
        {
            GameObject gameObject = new();
            gameObject.initWithTexture(t);
            return gameObject;
        }

        public static GameObject GameObject_createWithResIDQuad(int r, int q)
        {
            GameObject gameObject = GameObject_create(Application.getTexture(r));
            gameObject.setDrawQuad(q);
            return gameObject;
        }

        public override Image initWithTexture(CTRTexture2D t)
        {
            if (base.initWithTexture(t) != null)
            {
                bb = new CTRRectangle(0f, 0f, width, height);
                rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
                anchor = 18;
                rotatedBB = false;
                topLeftCalculated = false;
            }
            return this;
        }

        public override void update(float delta)
        {
            base.update(delta);
            if (!topLeftCalculated)
            {
                calculateTopLeft(this);
                topLeftCalculated = true;
            }
            if (mover != null)
            {
                mover.update(delta);
                x = mover.pos.x;
                y = mover.pos.y;
                if (rotatedBB)
                {
                    rotateWithBB((float)mover.angle_);
                    return;
                }
                rotation = (float)mover.angle_;
            }
        }

        public override void draw()
        {
            base.draw();
            if (isDrawBB)
            {
                drawBB();
            }
        }

        public override void dealloc()
        {
            NSREL(mover);
            base.dealloc();
        }

        public virtual GameObject initWithTextureIDxOffyOffXML(int t, int tx, int ty, XMLNode xml)
        {
            if (base.initWithTexture(Application.getTexture(t)) != null)
            {
                float num = xml["x"].intValue();
                float num2 = xml["y"].intValue();
                x = tx + num;
                y = ty + num2;
                type = t;
                NSString nSString = xml["bb"];
                if (nSString != null)
                {
                    List<NSString> list = nSString.componentsSeparatedByString(',');
                    bb = new CTRRectangle(list[0].intValue(), list[1].intValue(), list[2].intValue(), list[3].intValue());
                }
                else
                {
                    bb = new CTRRectangle(0f, 0f, width, height);
                }
                rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
                parseMover(xml);
            }
            return this;
        }

        public virtual void parseMover(XMLNode xml)
        {
            rotation = xml["angle"].floatValue();
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
                mover.angle_ = rotation;
                mover.angle_initial = mover.angle_;
                mover.setPathFromStringandStart(nSString, vect(x, y));
                setMover(mover);
                mover.start();
            }
        }

        public virtual void setMover(Mover m)
        {
            mover = m;
        }

        public virtual void setBBFromFirstQuad()
        {
            bb = new CTRRectangle((float)Math.Round(texture.quadOffsets[0].x), (float)Math.Round(texture.quadOffsets[0].y), texture.quadRects[0].w, texture.quadRects[0].h);
            rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
        }

        public virtual void rotateWithBB(float a)
        {
            if (!rotatedBB)
            {
                rotatedBB = true;
            }
            rotation = a;
            Vector v = vect(bb.x, bb.y);
            Vector v2 = vect(bb.x + bb.w, bb.y);
            Vector v3 = vect(bb.x + bb.w, bb.y + bb.h);
            Vector v4 = vect(bb.x, bb.y + bb.h);
            v = vectRotateAround(v, (double)DEGREES_TO_RADIANS(a), (float)(width / 2.0 + rotationCenterX), (float)(height / 2.0 + rotationCenterY));
            v2 = vectRotateAround(v2, (double)DEGREES_TO_RADIANS(a), (float)(width / 2.0 + rotationCenterX), (float)(height / 2.0 + rotationCenterY));
            v3 = vectRotateAround(v3, (double)DEGREES_TO_RADIANS(a), (float)(width / 2.0 + rotationCenterX), (float)(height / 2.0 + rotationCenterY));
            v4 = vectRotateAround(v4, (double)DEGREES_TO_RADIANS(a), (float)(width / 2.0 + rotationCenterX), (float)(height / 2.0 + rotationCenterY));
            rbb.tlX = v.x;
            rbb.tlY = v.y;
            rbb.trX = v2.x;
            rbb.trY = v2.y;
            rbb.brX = v3.x;
            rbb.brY = v3.y;
            rbb.blX = v4.x;
            rbb.blY = v4.y;
        }

        public virtual void drawBB()
        {
            OpenGL.glDisable(0);
            if (rotatedBB)
            {
                OpenGL.drawSegment(drawX + rbb.tlX, drawY + rbb.tlY, drawX + rbb.trX, drawY + rbb.trY, RGBAColor.redRGBA);
                OpenGL.drawSegment(drawX + rbb.trX, drawY + rbb.trY, drawX + rbb.brX, drawY + rbb.brY, RGBAColor.redRGBA);
                OpenGL.drawSegment(drawX + rbb.brX, drawY + rbb.brY, drawX + rbb.blX, drawY + rbb.blY, RGBAColor.redRGBA);
                OpenGL.drawSegment(drawX + rbb.blX, drawY + rbb.blY, drawX + rbb.tlX, drawY + rbb.tlY, RGBAColor.redRGBA);
            }
            else
            {
                GLDrawer.drawRect(drawX + bb.x, drawY + bb.y, bb.w, bb.h, RGBAColor.redRGBA);
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
            return rectInRect(num, num2, num + o1.bb.w, num2 + o1.bb.h, num3, num4, num3 + o2.bb.w, num4 + o2.bb.h);
        }

        private static bool objectsIntersectRotated(GameObject o1, GameObject o2)
        {
            Vector vector = vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector tr = vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector br = vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector bl = vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector tl2 = vect(o2.drawX + o2.rbb.tlX, o2.drawY + o2.rbb.tlY);
            Vector tr2 = vect(o2.drawX + o2.rbb.trX, o2.drawY + o2.rbb.trY);
            Vector br2 = vect(o2.drawX + o2.rbb.brX, o2.drawY + o2.rbb.brY);
            Vector bl2 = vect(o2.drawX + o2.rbb.blX, o2.drawY + o2.rbb.blY);
            return obbInOBB(vector, tr, br, bl, tl2, tr2, br2, bl2);
        }

        private static bool objectsIntersectRotatedWithUnrotated(GameObject o1, GameObject o2)
        {
            Vector vector = vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector tr = vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector br = vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector bl = vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector tl2 = vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y);
            Vector tr2 = vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y);
            Vector br2 = vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y + o2.bb.h);
            Vector bl2 = vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y + o2.bb.h);
            return obbInOBB(vector, tr, br, bl, tl2, tr2, br2, bl2);
        }

        public static bool pointInObject(Vector p, GameObject o)
        {
            float checkX = o.drawX + o.bb.x;
            float checkY = o.drawY + o.bb.y;
            return pointInRect(p.x, p.y, checkX, checkY, o.bb.w, o.bb.h);
        }

        public static bool rectInObject(float r1x, float r1y, float r2x, float r2y, GameObject o)
        {
            float num = o.drawX + o.bb.x;
            float num2 = o.drawY + o.bb.y;
            return rectInRect(r1x, r1y, r2x, r2y, num, num2, num + o.bb.w, num2 + o.bb.h);
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
