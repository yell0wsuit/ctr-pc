using CutTheRopeDX.Commons;
using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Core;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Manages a 2D texture with optional quad-based sprite regions, scaling, and a global linked list for bulk suspend/resume.
    /// </summary>
    internal sealed class CTRTexture2D : FrameworkTypes
    {
        /// <summary>
        /// Draws a rectangular region of <paramref name="texture"/> at <paramref name="point"/>.
        /// </summary>
        /// <param name="texture">Texture to draw from.</param>
        /// <param name="rect">Source rectangle within the texture.</param>
        /// <param name="point">Screen position to draw at.</param>
        public static void DrawRectAtPoint(CTRTexture2D texture, CTRRectangle rect, Vector point)
        {
            float texLeft = texture._invWidth * rect.x;
            float texTop = texture._invHeight * rect.y;
            float texRight = texLeft + (texture._invWidth * rect.w);
            float texBottom = texTop + (texture._invHeight * rect.h);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            VertexPositionNormalTexture[] vertices = QuadVertexCache.GetTexturedQuad(
                point.X, point.Y, rect.w, rect.h,
                texLeft, texTop, texRight, texBottom);
            Renderer.DrawTriangleStrip(vertices);
        }

        /// <summary>
        /// Returns this texture instance (identity helper for renderer binding).
        /// </summary>
        /// <returns>This texture instance.</returns>
        public CTRTexture2D Name()
        {
            return this;
        }

        /// <summary>
        /// Returns <see langword="true"/> if this texture uses WVGA scaling.
        /// </summary>
        /// <returns><see langword="true"/> when WVGA scaling is enabled; otherwise <see langword="false"/>.</returns>
        public bool IsWvga()
        {
            return _isWvga;
        }

        /// <summary>
        /// Allocates quad arrays with the given <paramref name="capacity"/>.
        /// </summary>
        /// <param name="capacity">Number of quads to allocate.</param>
        public void SetQuadsCapacity(int capacity)
        {
            quadsCount = capacity;
            quads = new Quad2D[quadsCount];
            quadRects = new CTRRectangle[quadsCount];
            quadOffsets = new Vector[quadsCount];
        }

        /// <summary>
        /// Sets the texture coordinates and rectangle for the quad at <paramref name="quadIndex"/>.
        /// </summary>
        /// <param name="rect">Source rectangle within the texture.</param>
        /// <param name="quadIndex">Index of the quad to set.</param>
        public void SetQuadAt(CTRRectangle rect, int quadIndex)
        {
            quads[quadIndex] = DrawHelper.GetTextureCoordinates(this, rect);
            quadRects[quadIndex] = rect;
            quadOffsets[quadIndex] = vectZero;
        }

        /// <summary>
        /// Marks this texture as using WVGA scaling.
        /// </summary>
        public void SetWvga()
        {
            _isWvga = true;
        }

        /// <summary>
        /// Sets the texture scale factors and recalculates drawing dimensions.
        /// </summary>
        /// <param name="scaleX">Horizontal scale factor.</param>
        /// <param name="scaleY">Vertical scale factor.</param>
        public void SetScale(float scaleX, float scaleY)
        {
            _scaleX = scaleX;
            _scaleY = scaleY;
            CalculateForQuickDrawing();
        }

        /// <summary>
        /// Draws the quad at <paramref name="quadIndex"/> of <paramref name="texture"/> at <paramref name="point"/>.
        /// </summary>
        /// <param name="texture">Texture containing the quad.</param>
        /// <param name="quadIndex">Index of the quad to draw.</param>
        /// <param name="point">Screen position to draw at.</param>
        public static void DrawQuadAtPoint(CTRTexture2D texture, int quadIndex, Vector point)
        {
            Quad2D quad2D = texture.quads[quadIndex];
            float w = texture.quadRects[quadIndex].w;
            float h = texture.quadRects[quadIndex].h;
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            VertexPositionNormalTexture[] vertices = QuadVertexCache.GetTexturedQuad(
                point.X, point.Y, w, h,
                quad2D.tlX, quad2D.tlY, quad2D.brX, quad2D.brY);
            Renderer.DrawTriangleStrip(vertices);
        }

        /// <summary>
        /// Draws the full <paramref name="texture"/> at <paramref name="point"/>.
        /// </summary>
        /// <param name="texture">Texture to draw.</param>
        /// <param name="point">Screen position to draw at.</param>
        public static void DrawAtPoint(CTRTexture2D texture, Vector point)
        {
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            VertexPositionNormalTexture[] vertices = QuadVertexCache.GetTexturedQuad(
                point.X, point.Y, texture._realWidth, texture._realHeight,
                0f, 0f, texture._maxS, texture._maxT);
            Renderer.DrawTriangleStrip(vertices);
        }

        /// <summary>
        /// Recalculates <see cref="_realWidth"/>, <see cref="_realHeight"/>, <see cref="_invWidth"/>, and <see cref="_invHeight"/> based on current scale and WVGA mode.
        /// </summary>
        public void CalculateForQuickDrawing()
        {
            if (_isWvga)
            {
                _realWidth = (int)(_width * _maxS / _scaleX);
                _realHeight = (int)(_height * _maxT / _scaleY);
                _invWidth = 1f / (_width / _scaleX);
                _invHeight = 1f / (_height / _scaleY);
                return;
            }
            _realWidth = (int)(_width * _maxS);
            _realHeight = (int)(_height * _maxT);
            _invWidth = 1f / _width;
            _invHeight = 1f / _height;
        }

        /// <summary>
        /// Placeholder for setting anti-alias texture parameters.
        /// </summary>
        public static void SetAntiAliasTexParameters()
        {
        }

        /// <summary>
        /// Placeholder for setting alias texture parameters.
        /// </summary>
        public static void SetAliasTexParameters()
        {
        }


        /// <summary>
        /// Registers this texture in the global linked list.
        /// </summary>
        public void Reg()
        {
            prev = tail;
            if (prev != null)
            {
                prev.next = this;
            }
            else
            {
                root = this;
            }
            tail = this;
        }

        /// <summary>
        /// Unregisters this texture from the global linked list.
        /// </summary>
        public void Unreg()
        {
            if (prev != null)
            {
                prev.next = next;
            }
            else
            {
                root = next;
            }
            if (next != null)
            {
                next.prev = prev;
            }
            else
            {
                tail = prev;
            }
            next = prev = null;
        }

        /// <summary>
        /// Loads the texture from the given resource <paramref name="path"/> and registers it.
        /// </summary>
        /// <param name="path">Resource path to load from.</param>
        /// <returns>The initialized texture instance, or <see langword="null"/> if loading fails.</returns>
        public CTRTexture2D InitWithPath(string path)
        {
            _resName = path;
            // _localTexParams = _texParams;
            Reg();
            xnaTexture_ = Images.Get(path);
            if (xnaTexture_ == null)
            {
                return null;
            }
            ImageLoaded(xnaTexture_.Width, xnaTexture_.Height);
            quadsCount = 0;
            CalculateForQuickDrawing();
            Resume();
            return this;
        }

        /// <summary>
        /// Returns the <paramref name="size"/> unchanged (no power-of-two rounding needed).
        /// </summary>
        /// <param name="size">Input size in pixels.</param>
        /// <returns>The real texture dimension used by the renderer.</returns>
        private static int CalcRealSize(int size)
        {
            return size;
        }

        /// <summary>
        /// Stores the loaded image dimensions and computes max S/T texture coordinates.
        /// </summary>
        /// <param name="w">Image width in pixels.</param>
        /// <param name="h">Image height in pixels.</param>
        private void ImageLoaded(int w, int h)
        {
            _lowypoint = h;
            int realWidth = CalcRealSize(w);
            int realHeight = CalcRealSize(h);
            //_size = new Vector(realWidth, realHeight);
            _width = (uint)realWidth;
            _height = (uint)realHeight;
            //_format = _defaultAlphaPixelFormat;
            _maxS = w / realWidth;
            _maxT = h / realHeight;
        }

        /// <summary>
        /// Placeholder for resuming a suspended texture.
        /// </summary>
        private static void Resume()
        {
        }

        /// <summary>
        /// Placeholder for optimizing texture memory.
        /// </summary>
        public static void OptimizeMemory()
        {
        }

        /// <summary>
        /// Placeholder for suspending a texture to free memory.
        /// </summary>
        public static void Suspend()
        {
        }

        /// <summary>
        /// Suspends all registered textures in the global linked list.
        /// </summary>
        public static void SuspendAll()
        {
            for (CTRTexture2D texture2D = root; texture2D != null; texture2D = texture2D.next)
            {
                Suspend();
            }
        }

        /// <summary>
        /// Resumes all registered textures in the global linked list.
        /// </summary>
        public static void ResumeAll()
        {
            for (CTRTexture2D texture2D = root; texture2D != null; texture2D = texture2D.next)
            {
                Resume();
            }
        }

        /// <summary>
        /// Initializes this texture from the current render target with dimensions <paramref name="w"/> x <paramref name="h"/>.
        /// </summary>
        /// <param name="w">Width of the render target in pixels.</param>
        /// <param name="h">Height of the render target in pixels.</param>
        /// <returns>The initialized texture instance.</returns>
        public CTRTexture2D InitFromPixels(int w, int h)
        {
            _lowypoint = -1;
            // _localTexParams = _defaultTexParams;
            Reg();
            int realWidth = CalcRealSize(w);
            int realHeight = CalcRealSize(h);
            float transitionTime = Application.SharedRootController().transitionTime;
            Application.SharedRootController().transitionTime = -1f;
            // Always use the render target since we now use fullscreen-style scaling in all modes
            CtrRenderer.OnDrawFrame();
            RenderTarget2D renderTarget = Renderer.DetachRenderTarget();
            Global.GraphicsDevice.SetRenderTarget(null);
            Application.SharedRootController().transitionTime = transitionTime;
            xnaTexture_ = renderTarget;
            //_format = Texture2DPixelFormat.kTexture2DPixelFormat_RGBA8888;
            //_size = new Vector(realWidth, realHeight);
            _width = (uint)realWidth;
            _height = (uint)realHeight;
            _maxS = w / realWidth;
            _maxT = h / realHeight;
            quadsCount = 0;
            CalculateForQuickDrawing();
            Resume();
            return this;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (xnaTexture_ != null)
                {
                    Images.Free(_resName);
                    xnaTexture_ = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// The underlying XNA texture.
        /// </summary>
        public Texture2D xnaTexture_;

        /// <summary>
        /// Resource name/path used to load this texture.
        /// </summary>
        public string _resName;

        /// <summary>
        /// Precomputed texture coordinate quads for each sprite region.
        /// </summary>
        public Quad2D[] quads;

        /// <summary>
        /// Raw texture width in pixels.
        /// </summary>
        private uint _width;

        /// <summary>
        /// Raw texture height in pixels.
        /// </summary>
        private uint _height;

        /// <summary>
        /// Lowest Y point of the loaded image, or -1 for render-target textures.
        /// </summary>
        public int _lowypoint;

        /// <summary>
        /// Maximum S (horizontal) texture coordinate.
        /// </summary>
        public float _maxS;

        /// <summary>
        /// Maximum T (vertical) texture coordinate.
        /// </summary>
        public float _maxT;

        /// <summary>
        /// Horizontal scale factor for WVGA mode.
        /// </summary>
        private float _scaleX;

        /// <summary>
        /// Vertical scale factor for WVGA mode.
        /// </summary>
        private float _scaleY;

        // private Texture2DPixelFormat _format;

        // private Vector _size;

        /// <summary>
        /// Per-quad position offsets relative to the texture origin.
        /// </summary>
        public Vector[] quadOffsets;

        /// <summary>
        /// Per-quad source rectangles within the texture.
        /// </summary>
        public CTRRectangle[] quadRects;

        /// <summary>
        /// Number of quads defined for this texture.
        /// </summary>
        public int quadsCount;

        /// <summary>
        /// Computed drawable width in pixels, accounting for scale and WVGA.
        /// </summary>
        public int _realWidth;

        /// <summary>
        /// Computed drawable height in pixels, accounting for scale and WVGA.
        /// </summary>
        public int _realHeight;

        /// <summary>
        /// Reciprocal of the texture width, used for UV coordinate calculation.
        /// </summary>
        public float _invWidth;

        /// <summary>
        /// Reciprocal of the texture height, used for UV coordinate calculation.
        /// </summary>
        public float _invHeight;

        /// <summary>
        /// Original size before transparency was trimmed, or <c>vectUndefined</c> if not trimmed.
        /// </summary>
        public Vector preCutSize;

        /// <summary>
        /// Per-quad original sizes before transparency was trimmed.
        /// </summary>
        public Vector[] preCutSizes;

        /// <summary>
        /// Whether this texture uses WVGA scaling.
        /// </summary>
        private bool _isWvga;

        // private TexParams _localTexParams;

        // private static readonly TexParams _defaultTexParams;

        // private static readonly TexParams _texParams;

        /// <summary>
        /// Head of the global texture linked list.
        /// </summary>
        private static CTRTexture2D root;

        /// <summary>
        /// Tail of the global texture linked list.
        /// </summary>
        private static CTRTexture2D tail;

        /// <summary>
        /// Next texture in the global linked list.
        /// </summary>
        private CTRTexture2D next;

        /// <summary>
        /// Previous texture in the global linked list.
        /// </summary>
        private CTRTexture2D prev;

        /// <summary>
        /// Pixel format types for texture storage.
        /// </summary>
        public enum Texture2DPixelFormat
        {
            /// <summary>
            /// 32-bit RGBA (8 bits per channel).
            /// </summary>
            kTexture2DPixelFormat_RGBA8888,

            /// <summary>
            /// 16-bit RGB (5-6-5 bits).
            /// </summary>
            kTexture2DPixelFormat_RGB565,

            /// <summary>
            /// 16-bit RGBA (4 bits per channel).
            /// </summary>
            kTexture2DPixelFormat_RGBA4444,

            /// <summary>
            /// 16-bit RGBA (5-5-5-1 bits).
            /// </summary>
            kTexture2DPixelFormat_RGB5A1,

            /// <summary>
            /// 8-bit alpha only.
            /// </summary>
            kTexture2DPixelFormat_A8,

            /// <summary>
            /// PVRTC 2 bits per pixel compressed format.
            /// </summary>
            kTexture2DPixelFormat_PVRTC2,

            /// <summary>
            /// PVRTC 4 bits per pixel compressed format.
            /// </summary>
            kTexture2DPixelFormat_PVRTC4
        }

        /// <summary>
        /// Placeholder struct for texture parameter storage.
        /// </summary>
        private readonly struct TexParams
        {
        }
    }
}
