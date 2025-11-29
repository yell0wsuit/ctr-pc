using System;
using System.Xml.Linq;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.GameMain;

namespace CutTheRope.Framework.Visual
{
    internal class Image : BaseElement
    {
        // (get) Token: 0x060001E5 RID: 485 RVA: 0x00009A46 File Offset: 0x00007C46
        public string ResName => texture != null ? texture._resName : "ERROR: texture == null";

        public static Vector GetQuadSize(int textureID, int quad)
        {
            CTRTexture2D texture2D = Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(textureID));
            return Vect(texture2D.quadRects[quad].w, texture2D.quadRects[quad].h);
        }

        /// <summary>
        /// Gets the quad size for the specified texture name.
        /// </summary>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Index of the quad.</param>
        public static Vector GetQuadSize(string textureResourceName, int quad)
        {
            CTRTexture2D texture2D = Application.GetTexture(textureResourceName);
            return Vect(texture2D.quadRects[quad].w, texture2D.quadRects[quad].h);
        }

        public static Vector GetQuadOffset(int textureID, int quad)
        {
            return Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(textureID)).quadOffsets[quad];
        }

        /// <summary>
        /// Gets the quad offset for the specified texture name.
        /// </summary>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Index of the quad.</param>
        public static Vector GetQuadOffset(string textureResourceName, int quad)
        {
            return Application.GetTexture(textureResourceName).quadOffsets[quad];
        }

        public static Vector GetQuadCenter(int textureID, int quad)
        {
            CTRTexture2D texture2D = Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(textureID));
            return VectAdd(texture2D.quadOffsets[quad], Vect(Ceil(texture2D.quadRects[quad].w / 2.0), Ceil(texture2D.quadRects[quad].h / 2.0)));
        }

        /// <summary>
        /// Gets the quad center for the specified texture name.
        /// </summary>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Index of the quad.</param>
        public static Vector GetQuadCenter(string textureResourceName, int quad)
        {
            CTRTexture2D texture2D = Application.GetTexture(textureResourceName);
            return VectAdd(texture2D.quadOffsets[quad], Vect(Ceil(texture2D.quadRects[quad].w / 2.0), Ceil(texture2D.quadRects[quad].h / 2.0)));
        }

        public static Vector GetRelativeQuadOffset(int textureID, int quadToCountFrom, int quad)
        {
            Vector quadOffset = GetQuadOffset(textureID, quadToCountFrom);
            return VectSub(GetQuadOffset(textureID, quad), quadOffset);
        }

        /// <summary>
        /// Gets the quad offset relative to another quad for the specified texture name.
        /// </summary>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quadToCountFrom">Base quad index.</param>
        /// <param name="quad">Target quad index.</param>
        public static Vector GetRelativeQuadOffset(string textureResourceName, int quadToCountFrom, int quad)
        {
            Vector quadOffset = GetQuadOffset(textureResourceName, quadToCountFrom);
            return VectSub(GetQuadOffset(textureResourceName, quad), quadOffset);
        }

        public static void SetElementPositionWithQuadCenter(BaseElement e, int textureID, int quad)
        {
            Vector quadCenter = GetQuadCenter(textureID, quad);
            e.x = quadCenter.x;
            e.y = quadCenter.y;
            e.anchor = 18;
        }

        /// <summary>
        /// Positions an element using the center of the specified quad and texture name.
        /// </summary>
        /// <param name="e">Element to position.</param>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Target quad.</param>
        public static void SetElementPositionWithQuadCenter(BaseElement e, string textureResourceName, int quad)
        {
            Vector quadCenter = GetQuadCenter(textureResourceName, quad);
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

        /// <summary>
        /// Positions an element using the offset of the specified quad and texture name.
        /// </summary>
        /// <param name="e">Element to position.</param>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quad">Target quad.</param>
        public static void SetElementPositionWithQuadOffset(BaseElement e, string textureResourceName, int quad)
        {
            Vector quadOffset = GetQuadOffset(textureResourceName, quad);
            e.x = quadOffset.x;
            e.y = quadOffset.y;
        }

        public static void SetElementPositionWithRelativeQuadOffset(BaseElement e, int textureID, int quadToCountFrom, int quad)
        {
            Vector relativeQuadOffset = GetRelativeQuadOffset(textureID, quadToCountFrom, quad);
            e.x = relativeQuadOffset.x;
            e.y = relativeQuadOffset.y;
        }

        /// <summary>
        /// Positions an element using the relative offset of the specified quad and texture name.
        /// </summary>
        /// <param name="e">Element to position.</param>
        /// <param name="textureResourceName">Texture resource name.</param>
        /// <param name="quadToCountFrom">Base quad index.</param>
        /// <param name="quad">Target quad index.</param>
        public static void SetElementPositionWithRelativeQuadOffset(BaseElement e, string textureResourceName, int quadToCountFrom, int quad)
        {
            Vector relativeQuadOffset = GetRelativeQuadOffset(textureResourceName, quadToCountFrom, quad);
            e.x = relativeQuadOffset.x;
            e.y = relativeQuadOffset.y;
        }

        public static Image Image_create(CTRTexture2D t)
        {
            return new Image().InitWithTexture(t);
        }

        /// <summary>
        /// Creates an image from the specified texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        public static Image Image_createWithResID(string resourceName)
        {
            return Image_create(Application.GetTexture(resourceName));
        }

        public static Image Image_createWithResID(int r)
        {
            return Image_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
        }

        public static Image Image_createWithResIDQuad(int r, int q)
        {
            Image image = Image_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
            image.SetDrawQuad(q);
            return image;
        }

        /// <summary>
        /// Creates an image from the specified texture resource name and sets the draw quad.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="q">Quad index to draw.</param>
        public static Image Image_createWithResIDQuad(string resourceName, int q)
        {
            Image image = Image_create(Application.GetTexture(resourceName));
            image.SetDrawQuad(q);
            return image;
        }

        public virtual Image InitWithTexture(CTRTexture2D t)
        {
            texture = t;
            if (texture == null)
            {
                throw new InvalidOperationException("Failed to initialize Image: texture is null. The texture resource may not exist or failed to load.");
            }
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
