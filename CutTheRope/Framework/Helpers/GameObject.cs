using System;
using System.Xml.Linq;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Helpers
{
    internal class GameObject : Animation
    {
        private static GameObject GameObject_create(CTRTexture2D texture)
        {
            GameObject gameObject = new();
            _ = gameObject.InitWithTexture(texture);
            return gameObject;
        }

        public static GameObject GameObject_createWithResIDQuad(string resourceName, int quadIndex)
        {
            GameObject gameObject = GameObject_create(Application.GetTexture(resourceName));
            gameObject.SetDrawQuad(quadIndex);
            return gameObject;
        }

        public override Image InitWithTexture(CTRTexture2D texture)
        {
            if (base.InitWithTexture(texture) != null)
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
                x = mover.pos.X;
                y = mover.pos.Y;
                if (rotatedBB)
                {
                    RotateWithBB(mover.angle_);
                    return;
                }
                rotation = mover.angle_;
            }
        }

        public override void Draw()
        {
            base.Draw();
            // if (isDrawBB)
            // {
            //     DrawBB();
            // }
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

        public virtual void ParseMover(XElement xml)
        {
            rotation = ParseFloatOrZero(xml.Attribute("angle")?.Value);
            string pathString = xml.Attribute("path")?.Value ?? string.Empty;
            if (pathString != null && pathString.Length != 0)
            {
                int moverCapacity = 100;
                if (pathString[0] == 'R')
                {
                    moverCapacity = (ParseIntOrZero(pathString[2..]) / 2) + 1;
                }
                float moveSpeed = ParseFloatOrZero(xml.Attribute("moveSpeed")?.Value);
                float rotateSpeed = ParseFloatOrZero(xml.Attribute("rotateSpeed")?.Value);
                Mover parsedMover = new(moverCapacity, moveSpeed, rotateSpeed)
                {
                    angle_ = rotation
                };
                parsedMover.angle_initial = parsedMover.angle_;
                parsedMover.SetPathFromStringandStart(pathString, Vect(x, y));
                SetMover(parsedMover);
                parsedMover.Start();
            }
        }

        public virtual void SetMover(Mover moverValue)
        {
            mover = moverValue;
        }

        public virtual void SetBBFromFirstQuad()
        {
            bb = new CTRRectangle(MathF.Round(texture.quadOffsets[0].X), MathF.Round(texture.quadOffsets[0].Y), texture.quadRects[0].w, texture.quadRects[0].h);
            rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
        }

        public virtual void RotateWithBB(float angle)
        {
            if (!rotatedBB)
            {
                rotatedBB = true;
            }
            rotation = angle;
            Vector topLeft = Vect(bb.x, bb.y);
            Vector topRight = Vect(bb.x + bb.w, bb.y);
            Vector bottomRight = Vect(bb.x + bb.w, bb.y + bb.h);
            Vector bottomLeft = Vect(bb.x, bb.y + bb.h);
            topLeft = VectRotateAround(topLeft, DEGREES_TO_RADIANS(angle), (width / 2) + rotationCenterX, (height / 2) + rotationCenterY);
            topRight = VectRotateAround(topRight, DEGREES_TO_RADIANS(angle), (width / 2) + rotationCenterX, (height / 2) + rotationCenterY);
            bottomRight = VectRotateAround(bottomRight, DEGREES_TO_RADIANS(angle), (width / 2) + rotationCenterX, (height / 2) + rotationCenterY);
            bottomLeft = VectRotateAround(bottomLeft, DEGREES_TO_RADIANS(angle), (width / 2) + rotationCenterX, (height / 2) + rotationCenterY);
            rbb.tlX = topLeft.X;
            rbb.tlY = topLeft.Y;
            rbb.trX = topRight.X;
            rbb.trY = topRight.Y;
            rbb.brX = bottomRight.X;
            rbb.brY = bottomRight.Y;
            rbb.blX = bottomLeft.X;
            rbb.blY = bottomLeft.Y;
        }

        public virtual void DrawBB()
        {
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            if (rotatedBB)
            {
                Renderer.DrawSegment(drawX + rbb.tlX, drawY + rbb.tlY, drawX + rbb.trX, drawY + rbb.trY, RGBAColor.redRGBA);
                Renderer.DrawSegment(drawX + rbb.trX, drawY + rbb.trY, drawX + rbb.brX, drawY + rbb.brY, RGBAColor.redRGBA);
                Renderer.DrawSegment(drawX + rbb.brX, drawY + rbb.brY, drawX + rbb.blX, drawY + rbb.blY, RGBAColor.redRGBA);
                Renderer.DrawSegment(drawX + rbb.blX, drawY + rbb.blY, drawX + rbb.tlX, drawY + rbb.tlY, RGBAColor.redRGBA);
            }
            else
            {
                DrawHelper.DrawRect(drawX + bb.x, drawY + bb.y, bb.w, bb.h, RGBAColor.redRGBA);
            }
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.SetColor(Color.White);
        }

        public static bool ObjectsIntersect(GameObject o1, GameObject o2)
        {
            float o1x = o1.drawX + o1.bb.x;
            float o1y = o1.drawY + o1.bb.y;
            float o2x = o2.drawX + o2.bb.x;
            float o2y = o2.drawY + o2.bb.y;
            return RectInRect(o1x, o1y, o1x + o1.bb.w, o1y + o1.bb.h, o2x, o2y, o2x + o2.bb.w, o2y + o2.bb.h);
        }

        public static bool ObjectsIntersectRotatedWithUnrotated(GameObject o1, GameObject o2)
        {
            Vector o1TopLeft = Vect(o1.drawX + o1.rbb.tlX, o1.drawY + o1.rbb.tlY);
            Vector o1TopRight = Vect(o1.drawX + o1.rbb.trX, o1.drawY + o1.rbb.trY);
            Vector o1BottomRight = Vect(o1.drawX + o1.rbb.brX, o1.drawY + o1.rbb.brY);
            Vector o1BottomLeft = Vect(o1.drawX + o1.rbb.blX, o1.drawY + o1.rbb.blY);
            Vector o2TopLeft = Vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y);
            Vector o2TopRight = Vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y);
            Vector o2BottomRight = Vect(o2.drawX + o2.bb.x + o2.bb.w, o2.drawY + o2.bb.y + o2.bb.h);
            Vector o2BottomLeft = Vect(o2.drawX + o2.bb.x, o2.drawY + o2.bb.y + o2.bb.h);
            return ObbInOBB(o1TopLeft, o1TopRight, o1BottomRight, o1BottomLeft, o2TopLeft, o2TopRight, o2BottomRight, o2BottomLeft);
        }

        public static bool PointInObject(Vector p, GameObject o)
        {
            float checkX = o.drawX + o.bb.x;
            float checkY = o.drawY + o.bb.y;
            return PointInRect(p.X, p.Y, checkX, checkY, o.bb.w, o.bb.h);
        }

        public static bool RectInObject(float r1x, float r1y, float r2x, float r2y, GameObject o)
        {
            float objectX = o.drawX + o.bb.x;
            float objectY = o.drawY + o.bb.y;
            return RectInRect(r1x, r1y, r2x, r2y, objectX, objectY, objectX + o.bb.w, objectY + o.bb.h);
        }

        public const int MAX_MOVER_CAPACITY = 100;

        public int state;

        public Mover mover;

        public CTRRectangle bb;

        public Quad2D rbb;

        public bool rotatedBB;

        // public bool isDrawBB;

        public bool topLeftCalculated;
    }
}
