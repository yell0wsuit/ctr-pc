using System;
using System.Xml.Linq;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> backed by a texture, supporting full-image or quad-based drawing.
    /// </summary>
    internal class Image : BaseElement
    {
        /// <summary>
        /// Gets the <paramref name="quad"/> size for the specified texture name.
        /// </summary>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Index of the quad.</param>
        /// <returns>The width and height of the <paramref name="quad"/> as a vector.</returns>
        public static Vector GetQuadSize(string textureResourceName, int quad)
        {
            CTRTexture2D texture2D = Application.GetTexture(textureResourceName);
            return texture2D.quadRects != null
                ? Vect(texture2D.quadRects[quad].w, texture2D.quadRects[quad].h)
                : Vect(texture2D._realWidth, texture2D._realHeight);
        }

        /// <summary>
        /// Gets the <paramref name="quad"/> offset for the specified texture name.
        /// </summary>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Index of the quad.</param>
        /// <returns>The offset of the <paramref name="quad"/>, or (0, 0) if no offsets are defined.</returns>
        public static Vector GetQuadOffset(string textureResourceName, int quad)
        {
            CTRTexture2D texture = Application.GetTexture(textureResourceName);
            return texture.quadOffsets != null ? texture.quadOffsets[quad] : Vect(0, 0);
        }

        /// <summary>
        /// Gets the <paramref name="quad"/> center for the specified texture name.
        /// </summary>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Index of the quad.</param>
        /// <returns>The center point of the <paramref name="quad"/> in texture space.</returns>
        public static Vector GetQuadCenter(string textureResourceName, int quad)
        {
            CTRTexture2D texture2D = Application.GetTexture(textureResourceName);
            Vector offset = texture2D.quadOffsets != null ? texture2D.quadOffsets[quad] : Vect(0, 0);
            Vector size = texture2D.quadRects != null
                ? Vect(texture2D.quadRects[quad].w, texture2D.quadRects[quad].h)
                : Vect(texture2D._realWidth, texture2D._realHeight);
            return VectAdd(offset, Vect(Ceil(size.X / 2), Ceil(size.Y / 2)));
        }

        /// <summary>
        /// Gets the <paramref name="quad"/> offset relative to <paramref name="quadToCountFrom"/> for the specified texture name.
        /// </summary>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quadToCountFrom">Base quad index.</param>
        /// <param name="quad">Target quad index.</param>
        /// <returns>The offset of <paramref name="quad"/> relative to <paramref name="quadToCountFrom"/>.</returns>
        public static Vector GetRelativeQuadOffset(string textureResourceName, int quadToCountFrom, int quad)
        {
            Vector quadOffset = GetQuadOffset(textureResourceName, quadToCountFrom);
            return VectSub(GetQuadOffset(textureResourceName, quad), quadOffset);
        }

        /// <summary>
        /// Positions an element using the offset of the specified <paramref name="quad"/> and texture name.
        /// </summary>
        /// <param name="e">Element to position.</param>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Target quad.</param>
        public static void SetElementPositionWithQuadOffset(BaseElement e, string textureResourceName, int quad)
        {
            Vector quadOffset = GetQuadOffset(textureResourceName, quad);
            e.x = quadOffset.X;
            e.y = quadOffset.Y;
        }

        /// <summary>
        /// Positions an element using the relative offset of the specified <paramref name="quad"/> and texture name.
        /// </summary>
        /// <param name="e">Element to position.</param>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quadToCountFrom">Base quad index.</param>
        /// <param name="quad">Target quad index.</param>
        public static void SetElementPositionWithRelativeQuadOffset(BaseElement e, string textureResourceName, int quadToCountFrom, int quad)
        {
            Vector relativeQuadOffset = GetRelativeQuadOffset(textureResourceName, quadToCountFrom, quad);
            e.x = relativeQuadOffset.X;
            e.y = relativeQuadOffset.Y;
        }

        /// <summary>
        /// Creates an image from the specified texture.
        /// </summary>
        /// <param name="t">Texture to create the image from.</param>
        /// <returns>A new <see cref="Image"/> bound to <paramref name="t"/>.</returns>
        public static Image Image_create(CTRTexture2D t)
        {
            return new Image().InitWithTexture(t);
        }

        /// <summary>
        /// Creates an image from the specified texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <returns>A new <see cref="Image"/> bound to the resolved texture.</returns>
        public static Image Image_createWithResID(string resourceName)
        {
            return Image_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Creates an image from the specified texture resource name and sets the draw quad.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="q">Quad index to draw.</param>
        /// <returns>A new <see cref="Image"/> configured to draw the specified quad.</returns>
        public static Image Image_createWithResIDQuad(string resourceName, int q)
        {
            Image image = Image_create(Application.GetTexture(resourceName));
            image.SetDrawQuad(q);
            return image;
        }

        /// <summary>
        /// Initializes the image with the given texture, setting the first quad or full image.
        /// </summary>
        /// <param name="t">Texture to initialize with.</param>
        /// <returns>This image instance for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="t"/> is <see langword="null"/>.</exception>
        public virtual Image InitWithTexture(CTRTexture2D t)
        {
            texture = t ?? throw new InvalidOperationException("Failed to initialize Image: texture is null. The texture resource may not exist or failed to load.");
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

        /// <summary>
        /// Switches to drawing the entire texture instead of a single quad.
        /// </summary>
        public virtual void SetDrawFullImage()
        {
            quadToDraw = -1;
            width = texture._realWidth;
            height = texture._realHeight;
        }

        /// <summary>
        /// Sets the quad index to draw and updates width/height accordingly.
        /// </summary>
        /// <param name="n">Quad index to draw.</param>
        public virtual void SetDrawQuad(int n)
        {
            quadToDraw = n;
            if (!restoreCutTransparency)
            {
                width = (int)texture.quadRects[n].w;
                height = (int)texture.quadRects[n].h;
            }
            else
            {
                _ = ApplyPerQuadPreCutSize(n);
            }
        }

        /// <summary>
        /// Restores the pre-cut size so that trimmed transparency is accounted for in positioning.
        /// </summary>
        public virtual void DoRestoreCutTransparency()
        {
            if (texture.preCutSize.X != vectUndefined.X)
            {
                restoreCutTransparency = true;
                if (!ApplyPerQuadPreCutSize(quadToDraw))
                {
                    width = (int)texture.preCutSize.X;
                    height = (int)texture.preCutSize.Y;
                }
            }
        }

        /// <summary>
        /// Applies the pre-cut size for the given <paramref name="quad"/> if available. Returns <see langword="true"/> if applied.
        /// </summary>
        /// <param name="quad">Quad index to apply pre-cut size for.</param>
        /// <returns><see langword="true"/> if a pre-cut size was applied; otherwise <see langword="false"/>.</returns>
        private bool ApplyPerQuadPreCutSize(int quad)
        {
            if (quad >= 0 && texture.preCutSizes != null && quad < texture.preCutSizes.Length)
            {
                Vector size = texture.preCutSizes[quad];
                if (size.X > 0f && size.Y > 0f)
                {
                    width = (int)size.X;
                    height = (int)size.Y;
                    return true;
                }
            }
            return false;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            if (quadToDraw == -1)
            {
                DrawHelper.DrawImage(texture, drawX, drawY);
            }
            else
            {
                DrawQuad(quadToDraw);
            }
            PostDraw();
        }

        /// <summary>
        /// Draws the specified quad from the bound texture.
        /// </summary>
        /// <param name="n">Quad index to draw.</param>
        public virtual void DrawQuad(int n)
        {
            float w = texture.quadRects[n].w;
            float h = texture.quadRects[n].h;
            float x = drawX;
            float y = drawY;
            if (restoreCutTransparency)
            {
                x += texture.quadOffsets[n].X;
                y += texture.quadOffsets[n].Y;
            }
            Quad2D quad = texture.quads[n];
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            VertexPositionNormalTexture[] vertices = QuadVertexCache.GetTexturedQuad(
                x, y, w, h,
                quad.tlX, quad.tlY, quad.brX, quad.brY);
            Renderer.DrawTriangleStrip(vertices);
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Creates a <see cref="BaseElement"/> from an XML definition. Not implemented in this class.
        /// </summary>
        /// <param name="xml">XML element to create from.</param>
        /// <returns>The created element.</returns>
        /// <exception cref="NotImplementedException">Always thrown by this base implementation.</exception>
        public virtual BaseElement CreateFromXML(XElement xml)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                texture = null;
            }
            base.Dispose(disposing);
        }


        /// <summary>
        /// Timeline action name for setting the draw quad index.
        /// </summary>
        public const string ACTION_SET_DRAWQUAD = "ACTION_SET_DRAWQUAD";

        /// <summary>
        /// The texture used for drawing this image.
        /// </summary>
        public CTRTexture2D texture;

        /// <summary>
        /// Whether to restore trimmed transparency offsets when drawing quads.
        /// </summary>
        public bool restoreCutTransparency;

        /// <summary>
        /// Index of the quad to draw, or -1 to draw the full image.
        /// </summary>
        public int quadToDraw;
    }
}
