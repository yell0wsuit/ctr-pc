using System;
using System.Collections.Generic;
using System.Xml.Linq;

using CutTheRope.desktop;
using CutTheRope.Helpers;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mover?.Dispose();
                mover = null;
            }
            base.Dispose(disposing);
        }

        public virtual GameObject InitWithTextureIDxOffyOffXML(int t, int tx, int ty, XElement xml)
        {
            if (base.InitWithTexture(Application.GetTexture(t)) != null)
            {
                float num = xml.AttributeAsNSString("x").IntValue();
                float num2 = xml.AttributeAsNSString("y").IntValue();
                x = tx + num;
                y = ty + num2;
                type = t;
                string nSString = xml.AttributeAsNSString("bb");
                if (nSString.Length() != 0)
                {
                    List<string> list = nSString.ComponentsSeparatedByString(',');
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

        public virtual void ParseMover(XElement xml)
        {
            rotation = xml.AttributeAsNSString("angle").FloatValue();
            string nSString = xml.AttributeAsNSString("path");
            if (nSString != null && nSString.Length() != 0)
            {
                int i = 100;
                if (nSString.CharacterAtIndex(0) == 'R')
                {
                    i = (nSString.SubstringFromIndex(2).IntValue() / 2) + 1;
                }
                float m_ = xml.AttributeAsNSString("moveSpeed").FloatValue();
                float r_ = xml.AttributeAsNSString("rotateSpeed").FloatValue();
                Mover mover = new(i, m_, r_)
                {
                    angle_ = rotation
                };
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
