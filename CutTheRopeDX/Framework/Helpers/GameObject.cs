using System;
using System.Xml.Linq;

using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.Framework.Helpers
{
    /// <summary>
    /// An <see cref="Animation"/> with a bounding box, mover support, and collision testing.
    /// Base class for all interactive game objects.
    /// </summary>
    internal class GameObject : Animation
    {
        /// <summary>
        /// Creates a game object from the specified <paramref name="texture"/>.
        /// </summary>
        /// <param name="texture">Texture to create the object from.</param>
        /// <returns>A new game object initialized with <paramref name="texture"/>.</returns>
        private static GameObject GameObject_create(CTRTexture2D texture)
        {
            GameObject gameObject = new();
            _ = gameObject.InitWithTexture(texture);
            return gameObject;
        }

        /// <summary>
        /// Creates a game object from the specified texture resource and quad index.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="quadIndex">Quad index to draw.</param>
        /// <returns>A new game object configured to draw the selected quad.</returns>
        public static GameObject GameObject_createWithResIDQuad(string resourceName, int quadIndex)
        {
            GameObject gameObject = GameObject_create(Application.GetTexture(resourceName));
            gameObject.SetDrawQuad(quadIndex);
            return gameObject;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void Draw()
        {
            base.Draw();
            // if (isDrawBB)
            // {
            //     DrawBB();
            // }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mover?.Dispose();
                mover = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Parses mover path and speed attributes from the XML element.
        /// </summary>
        /// <param name="xml">XML element containing mover attributes.</param>
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

        /// <summary>
        /// Assigns a mover to control this object's position and rotation.
        /// </summary>
        /// <param name="moverValue">Mover instance to assign.</param>
        public virtual void SetMover(Mover moverValue)
        {
            mover = moverValue;
        }

        /// <summary>
        /// Sets the bounding box from the first quad's offset and size.
        /// </summary>
        public virtual void SetBBFromFirstQuad()
        {
            bb = new CTRRectangle(MathF.Round(texture.quadOffsets[0].X), MathF.Round(texture.quadOffsets[0].Y), texture.quadRects[0].w, texture.quadRects[0].h);
            rbb = new Quad2D(bb.x, bb.y, bb.w, bb.h);
        }

        /// <summary>
        /// Rotates the object and its bounding box by the specified <paramref name="angle"/> in degrees.
        /// </summary>
        /// <param name="angle">Rotation angle in degrees.</param>
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

        /// <summary>
        /// Draws the bounding box outline for debugging.
        /// </summary>
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

        /// <summary>
        /// Tests axis-aligned bounding box intersection between two objects.
        /// </summary>
        /// <param name="o1">First object.</param>
        /// <param name="o2">Second object.</param>
        /// <returns><see langword="true"/> when the objects' AABBs intersect; otherwise <see langword="false"/>.</returns>
        public static bool ObjectsIntersect(GameObject o1, GameObject o2)
        {
            float o1x = o1.drawX + o1.bb.x;
            float o1y = o1.drawY + o1.bb.y;
            float o2x = o2.drawX + o2.bb.x;
            float o2y = o2.drawY + o2.bb.y;
            return RectInRect(o1x, o1y, o1x + o1.bb.w, o1y + o1.bb.h, o2x, o2y, o2x + o2.bb.w, o2y + o2.bb.h);
        }

        /// <summary>
        /// Tests OBB intersection between a rotated object <paramref name="o1"/> and an unrotated object <paramref name="o2"/>.
        /// </summary>
        /// <param name="o1">Rotated object.</param>
        /// <param name="o2">Unrotated object.</param>
        /// <returns><see langword="true"/> when the objects intersect; otherwise <see langword="false"/>.</returns>
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

        /// <summary>
        /// Tests whether point <paramref name="p"/> is inside the bounding box of <paramref name="o"/>.
        /// </summary>
        /// <param name="p">Point to test.</param>
        /// <param name="o">Object whose bounding box to test against.</param>
        /// <returns><see langword="true"/> when the point is inside the object bounds; otherwise <see langword="false"/>.</returns>
        public static bool PointInObject(Vector p, GameObject o)
        {
            float checkX = o.drawX + o.bb.x;
            float checkY = o.drawY + o.bb.y;
            return PointInRect(p.X, p.Y, checkX, checkY, o.bb.w, o.bb.h);
        }

        /// <summary>
        /// Tests whether the rectangle defined by corners (<paramref name="r1x"/>,<paramref name="r1y"/>)–(<paramref name="r2x"/>,<paramref name="r2y"/>) intersects the bounding box of <paramref name="o"/>.
        /// </summary>
        /// <param name="r1x">Left X of the rectangle.</param>
        /// <param name="r1y">Top Y of the rectangle.</param>
        /// <param name="r2x">Right X of the rectangle.</param>
        /// <param name="r2y">Bottom Y of the rectangle.</param>
        /// <param name="o">Object whose bounding box to test against.</param>
        /// <returns><see langword="true"/> when the rectangles intersect; otherwise <see langword="false"/>.</returns>
        public static bool RectInObject(float r1x, float r1y, float r2x, float r2y, GameObject o)
        {
            float objectX = o.drawX + o.bb.x;
            float objectY = o.drawY + o.bb.y;
            return RectInRect(r1x, r1y, r2x, r2y, objectX, objectY, objectX + o.bb.w, objectY + o.bb.h);
        }

        /// <summary>
        /// Maximum number of path points a mover can hold.
        /// </summary>
        public const int MAX_MOVER_CAPACITY = 100;

        /// <summary>
        /// Current state of this game object.
        /// </summary>
        public int state;

        /// <summary>
        /// Mover controlling this object's position and rotation, or <see langword="null"/>.
        /// </summary>
        public Mover mover;

        /// <summary>
        /// Axis-aligned bounding box relative to the element origin.
        /// </summary>
        public CTRRectangle bb;

        /// <summary>
        /// Rotated bounding box quad, updated when <see cref="rotatedBB"/> is <see langword="true"/>.
        /// </summary>
        public Quad2D rbb;

        /// <summary>
        /// Whether the bounding box has been rotated.
        /// </summary>
        public bool rotatedBB;

        // public bool isDrawBB;

        /// <summary>
        /// Whether <see cref="BaseElement.CalculateTopLeft"/> has been called this frame.
        /// </summary>
        public bool topLeftCalculated;
    }
}
