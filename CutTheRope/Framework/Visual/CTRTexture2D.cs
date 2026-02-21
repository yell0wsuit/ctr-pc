using CutTheRope.Commons;
using CutTheRope.Desktop;
using CutTheRope.Framework.Core;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    internal sealed class CTRTexture2D : FrameworkTypes
    {
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

        public CTRTexture2D Name()
        {
            return this;
        }

        public bool IsWvga()
        {
            return _isWvga;
        }

        public void SetQuadsCapacity(int capacity)
        {
            quadsCount = capacity;
            quads = new Quad2D[quadsCount];
            quadRects = new CTRRectangle[quadsCount];
            quadOffsets = new Vector[quadsCount];
        }

        public void SetQuadAt(CTRRectangle rect, int quadIndex)
        {
            quads[quadIndex] = DrawHelper.GetTextureCoordinates(this, rect);
            quadRects[quadIndex] = rect;
            quadOffsets[quadIndex] = vectZero;
        }

        public void SetWvga()
        {
            _isWvga = true;
        }

        public void SetScale(float scaleX, float scaleY)
        {
            _scaleX = scaleX;
            _scaleY = scaleY;
            CalculateForQuickDrawing();
        }

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

        public static void DrawAtPoint(CTRTexture2D texture, Vector point)
        {
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            VertexPositionNormalTexture[] vertices = QuadVertexCache.GetTexturedQuad(
                point.X, point.Y, texture._realWidth, texture._realHeight,
                0f, 0f, texture._maxS, texture._maxT);
            Renderer.DrawTriangleStrip(vertices);
        }

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

        public static void SetAntiAliasTexParameters()
        {
        }

        public static void SetAliasTexParameters()
        {
        }


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

        private static int CalcRealSize(int size)
        {
            return size;
        }

        private void ImageLoaded(int w, int h)
        {
            _lowypoint = h;
            int realWidth = CalcRealSize(w);
            int realHeight = CalcRealSize(h);
            //_size = new Vector(realWidth, realHeight);
            _width = (uint)realWidth;
            _height = (uint)realHeight;
            //_format = _defaultAlphaPixelFormat;
            _maxS = w / (float)realWidth;
            _maxT = h / (float)realHeight;
        }

        private static void Resume()
        {
        }

        public static void OptimizeMemory()
        {
        }

        public static void Suspend()
        {
        }

        public static void SuspendAll()
        {
            for (CTRTexture2D texture2D = root; texture2D != null; texture2D = texture2D.next)
            {
                Suspend();
            }
        }

        public static void ResumeAll()
        {
            for (CTRTexture2D texture2D = root; texture2D != null; texture2D = texture2D.next)
            {
                Resume();
            }
        }

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
            _maxS = w / (float)realWidth;
            _maxT = h / (float)realHeight;
            quadsCount = 0;
            CalculateForQuickDrawing();
            Resume();
            return this;
        }

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

        public Texture2D xnaTexture_;

        public string _resName;

        public Quad2D[] quads;

        private uint _width;

        private uint _height;

        public int _lowypoint;

        public float _maxS;

        public float _maxT;

        private float _scaleX;

        private float _scaleY;

        // private Texture2DPixelFormat _format;

        // private Vector _size;

        public Vector[] quadOffsets;

        public CTRRectangle[] quadRects;

        public int quadsCount;

        public int _realWidth;

        public int _realHeight;

        public float _invWidth;

        public float _invHeight;

        public Vector preCutSize;

        private bool _isWvga;

        // private TexParams _localTexParams;

        // private static readonly TexParams _defaultTexParams;

        // private static readonly TexParams _texParams;
        private static CTRTexture2D root;

        private static CTRTexture2D tail;

        private CTRTexture2D next;

        private CTRTexture2D prev;

        public enum Texture2DPixelFormat
        {
            kTexture2DPixelFormat_RGBA8888,
            kTexture2DPixelFormat_RGB565,
            kTexture2DPixelFormat_RGBA4444,
            kTexture2DPixelFormat_RGB5A1,
            kTexture2DPixelFormat_A8,
            kTexture2DPixelFormat_PVRTC2,
            kTexture2DPixelFormat_PVRTC4
        }

        private readonly struct TexParams
        {
        }
    }
}
