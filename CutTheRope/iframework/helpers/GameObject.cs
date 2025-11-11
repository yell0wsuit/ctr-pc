using CutTheRope.desktop;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.helpers
{
    internal class GameObject : Animation
    {
        public static GameObject GameObject_createWithResID(int r)
        {
            return GameObject_create(Application.GetTexture(r));
        }

        private static GameObject GameObject_create(CTRTexture2D t)
        {
            GameObject gameObject = new();
            _ = gameObject.InitWithTexture(t);
            return gameObject;
        }

        public static GameObject GameObject_createWithResIDQuad(int r, int q)
        {
            GameObject gameObject = GameObject_create(Application.GetTexture(r));
            gameObject.SetDrawQuad(q);
            return gameObject;
        }

        public override Image InitWithTexture(CTRTexture2D t)
        {
            if (base.InitWithTexture(t) != null)
            {
                bb = new CTRRectangle(0f, 0f, width, height);
                rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
                anchor = 18;
                rotatedBB = false;
                topLeftCalculated = false;
            }
            return this;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (!topLeftCalculated)
            {
                CalculateTopLeft(this);
                topLeftCalculated = true;
            }
            if (mover != null)
            {
                mover.Update(delta);
                x = mover.pos.x;
                y = mover.pos.y;
                if (rotatedBB)
                {
                    RotateWithBB((float)mover.angle_);
                    return;
                }
                rotation = (float)mover.angle_;
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (isDrawBB)
            {
                DrawBB();
            }
        }

        public override void Dealloc()
        {
            NSREL(mover);
            base.Dealloc();
        }

        public virtual GameObject InitWithTextureIDxOffyOffXML(int t, int tx, int ty, XMLNode xml)
        {
            if (base.InitWithTexture(Application.GetTexture(t)) != null)
            {
                float num = xml["x"].IntValue();
                float num2 = xml["y"].IntValue();
                x = tx + num;
                y = ty + num2;
                type = t;
                NSString nSString = xml["bb"];
                if (nSString != null)
                {
                    List<NSString> list = nSString.ComponentsSeparatedByString(',');
                    bb = new CTRRectangle(list[0].IntValue(), list[1].IntValue(), list[2].IntValue(), list[3].IntValue());
                }
                else
                {
                    bb = new CTRRectangle(0f, 0f, width, height);
                }
                rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
                ParseMover(xml);
            }
            return this;
        }

        public virtual void ParseMover(XMLNode xml)
        {
            rotation = xml["angle"].FloatValue();
            NSString nSString = xml["path"];
            if (nSString != null && nSString.Length() != 0)
            {
                int i = 100;
                if (nSString.CharacterAtIndex(0) == 'R')
                {
                    i = (nSString.SubstringFromIndex(2).IntValue() / 2) + 1;
                }
                float m_ = xml["moveSpeed"].FloatValue();
                float r_ = xml["rotateSpeed"].FloatValue();
                Mover mover = new Mover().InitWithPathCapacityMoveSpeedRotateSpeed(i, m_, r_);
                mover.angle_ = rotation;
                mover.angle_initial = mover.angle_;
                mover.SetPathFromStringandStart(nSString, Vect(x, y));
                SetMover(mover);
                mover.Start();
            }
        }

        public virtual void SetMover(Mover m)
        {
            mover = m;
        }

        public virtual void SetBBFromFirstQuad()
        {
            bb = new CTRRectangle((float)Math.Round(texture.quadOffsets[0].x), (float)Math.Round(texture.quadOffsets[0].y), texture.quadRects[0].w, texture.quadRects[0].h);
            rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
        }

        public virtual void RotateWithBB(float a)
        {
            if (!rotatedBB)
            {
                rotatedBB = true;
            }
            rotation = a;
            Vector v = Vect(bb.x, bb.y);
            Vector v2 = Vect(bb.x + bb.w, bb.y);
            Vector v3 = Vect(bb.x + bb.w, bb.y + bb.h);
            Vector v4 = Vect(bb.x, bb.y + bb.h);
            v = VectRotateAround(v, (double)DEGREES_TO_RADIANS(a), (float)((width / 2.0) + rotationCenterX), (float)((height / 2.0) + rotationCenterY));
            v2 = VectRotateAround(v2, (double)DEGREES_TO_RADIANS(a), (float)((width / 2.0) + rotationCenterX), (float)((height / 2.0) + rotationCenterY));
            v3 = VectRotateAround(v3, (double)DEGREES_TO_RADIANS(a), (float)((width / 2.0) + rotationCenterX), (float)((height / 2.0) + rotationCenterY));
            v4 = VectRotateAround(v4, (double)DEGREES_TO_RADIANS(a), (float)((width / 2.0) + rotationCenterX), (float)((height / 2.0) + rotationCenterY));
            rbb.tlX = v.x;
            rbb.tlY = v.y;
            rbb.trX = v2.x;
            rbb.trY = v2.y;
            rbb.brX = v3.x;
            rbb.brY = v3.y;
            rbb.blX = v4.x;
            rbb.blY = v4.y;
        }

        public virtual void DrawBB()
        {
            OpenGL.GlDisable(0);
            if (rotatedBB)
            {
                OpenGL.DrawSegment(drawX + rbb.tlX, drawY + rbb.tlY, drawX + rbb.trX, drawY + rbb.trY, RGBAColor.redRGBA);
                OpenGL.DrawSegment(drawX + rbb.trX, drawY + rbb.trY, drawX + rbb.brX, drawY + rbb.brY, RGBAColor.redRGBA);
                OpenGL.DrawSegment(drawX + rbb.brX, drawY + rbb.brY, drawX + rbb.blX, drawY + rbb.blY, RGBAColor.redRGBA);
                OpenGL.DrawSegment(drawX + rbb.blX, drawY + rbb.blY, drawX + rbb.tlX, drawY + rbb.tlY, RGBAColor.redRGBA);
            }
            else
            {
                GLDrawer.DrawRect(drawX + bb.x, drawY + bb.y, bb.w, bb.h, RGBAColor.redRGBA);
            }
            OpenGL.GlEnable(0);
            OpenGL.GlColor4f(Color.White);
        }

        public static bool ObjectsIntersect(GameObject o1, GameObject o2)
        {
            float num = o1.drawX + o1.bb.x;
            float num2 = o1.drawY + o1.bb.y;
            float num3 = o2.drawX + o2.bb.x;
            float num4 = o2.drawY + o2.bb.y;
            return RectInRect(num, num2, num + o1.bb.w, num2 + o1.bb.h, num3, num4, num3 + o2.bb.w, num4 + o2.bb.h);
        }

        private static bool ObjectsIntersectRotated(GameObject o1, GameObject o2)
        {
            Vector vector = Vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector tr = Vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector br = Vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector bl = Vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector tl2 = Vect(o2.drawX + o2.rbb.tlX, o2.drawY + o2.rbb.tlY);
            Vector tr2 = Vect(o2.drawX + o2.rbb.trX, o2.drawY + o2.rbb.trY);
            Vector br2 = Vect(o2.drawX + o2.rbb.brX, o2.drawY + o2.rbb.brY);
            Vector bl2 = Vect(o2.drawX + o2.rbb.blX, o2.drawY + o2.rbb.blY);
            return ObbInOBB(vector, tr, br, bl, tl2, tr2, br2, bl2);
        }

        private static bool ObjectsIntersectRotatedWithUnrotated(GameObject o1, GameObject o2)
        {
            Vector vector = Vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector tr = Vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector br = Vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector bl = Vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector tl2 = Vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y);
            Vector tr2 = Vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y);
            Vector br2 = Vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y + o2.bb.h);
            Vector bl2 = Vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y + o2.bb.h);
            return ObbInOBB(vector, tr, br, bl, tl2, tr2, br2, bl2);
        }

        public static bool PointInObject(Vector p, GameObject o)
        {
            float checkX = o.drawX + o.bb.x;
            float checkY = o.drawY + o.bb.y;
            return PointInRect(p.x, p.y, checkX, checkY, o.bb.w, o.bb.h);
        }

        public static bool RectInObject(float r1x, float r1y, float r2x, float r2y, GameObject o)
        {
            float num = o.drawX + o.bb.x;
            float num2 = o.drawY + o.bb.y;
            return RectInRect(r1x, r1y, r2x, r2y, num, num2, num + o.bb.w, num2 + o.bb.h);
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
