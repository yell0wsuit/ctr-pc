using CutTheRope.commons;
using CutTheRope.desktop;
using CutTheRope.iframework.core;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.iframework.visual
{
    internal sealed class CTRTexture2D : FrameworkTypes
    {
        public static void DrawRectAtPoint(CTRTexture2D t, CTRRectangle rect, Vector point)
        {
            float num = t._invWidth * rect.x;
            float num2 = t._invHeight * rect.y;
            float num3 = num + (t._invWidth * rect.w);
            float num4 = num2 + (t._invHeight * rect.h);
            float[] pointer = [num, num2, num3, num2, num, num4, num3, num4];
            float[] array = new float[12];
            array[0] = point.x;
            array[1] = point.y;
            array[3] = rect.w + point.x;
            array[4] = point.y;
            array[6] = point.x;
            array[7] = rect.h + point.y;
            array[9] = rect.w + point.x;
            array[10] = rect.h + point.y;
            float[] pointer2 = array;
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(t.Name());
            OpenGL.GlVertexPointer(3, 5, 0, pointer2);
            OpenGL.GlTexCoordPointer(2, 5, 0, pointer);
            OpenGL.GlDrawArrays(8, 0, 4);
        }

        public CTRTexture2D Name()
        {
            return this;
        }

        public bool IsWvga()
        {
            return _isWvga;
        }

        public void SetQuadsCapacity(int n)
        {
            quadsCount = n;
            quads = new Quad2D[quadsCount];
            quadRects = new CTRRectangle[quadsCount];
            quadOffsets = new Vector[quadsCount];
        }

        public void SetQuadAt(CTRRectangle rect, int n)
        {
            quads[n] = GLDrawer.GetTextureCoordinates(this, rect);
            quadRects[n] = rect;
            quadOffsets[n] = vectZero;
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

        public static void DrawQuadAtPoint(CTRTexture2D t, int q, Vector point)
        {
            Quad2D quad2D = t.quads[q];
            float[] array = new float[12];
            array[0] = point.x;
            array[1] = point.y;
            array[3] = t.quadRects[q].w + point.x;
            array[4] = point.y;
            array[6] = point.x;
            array[7] = t.quadRects[q].h + point.y;
            array[9] = t.quadRects[q].w + point.x;
            array[10] = t.quadRects[q].h + point.y;
            float[] pointer = array;
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(t.Name());
            OpenGL.GlVertexPointer(3, 5, 0, pointer);
            OpenGL.GlTexCoordPointer(2, 5, 0, quad2D.ToFloatArray());
            OpenGL.GlDrawArrays(8, 0, 4);
        }

        public static void DrawAtPoint(CTRTexture2D t, Vector point)
        {
            float[] pointer = [0f, 0f, t._maxS, 0f, 0f, t._maxT, t._maxS, t._maxT];
            float[] array = new float[12];
            array[0] = point.x;
            array[1] = point.y;
            array[3] = t._realWidth + point.x;
            array[4] = point.y;
            array[6] = point.x;
            array[7] = t._realHeight + point.y;
            array[9] = t._realWidth + point.x;
            array[10] = t._realHeight + point.y;
            float[] pointer2 = array;
            OpenGL.GlEnable(0);
            OpenGL.GlBindTexture(t.Name());
            OpenGL.GlVertexPointer(3, 5, 0, pointer2);
            OpenGL.GlTexCoordPointer(2, 5, 0, pointer);
            OpenGL.GlDrawArrays(8, 0, 4);
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

        public CTRTexture2D InitWithPath(string path, bool assets)
        {
            _resName = path;
            _name = 65536U;
            _localTexParams = _texParams;
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
            int num = CalcRealSize(w);
            int num2 = CalcRealSize(h);
            _size = new Vector(num, num2);
            _width = (uint)num;
            _height = (uint)num2;
            _format = _defaultAlphaPixelFormat;
            _maxS = w / (float)num;
            _maxT = h / (float)num2;
            _hasPremultipliedAlpha = true;
        }

        private static void Resume()
        {
        }

        public static void SetDefaultAlphaPixelFormat(Texture2DPixelFormat format)
        {
            _defaultAlphaPixelFormat = format;
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

        public CTRTexture2D InitFromPixels(int x, int y, int w, int h)
        {
            _name = 65536U;
            _lowypoint = -1;
            _localTexParams = _defaultTexParams;
            Reg();
            int num = CalcRealSize(w);
            int num2 = CalcRealSize(h);
            float transitionTime = Application.SharedRootController().transitionTime;
            Application.SharedRootController().transitionTime = -1f;
            RenderTarget2D renderTarget;
            if (Global.ScreenSizeManager.IsFullScreen)
            {
                CtrRenderer.OnDrawFrame();
                renderTarget = OpenGL.DetachRenderTarget();
            }
            else
            {
                renderTarget = new RenderTarget2D(Global.GraphicsDevice, Global.GraphicsDevice.PresentationParameters.BackBufferWidth, Global.GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
                Global.GraphicsDevice.SetRenderTarget(renderTarget);
                CtrRenderer.OnDrawFrame();
            }
            Global.GraphicsDevice.SetRenderTarget(null);
            Application.SharedRootController().transitionTime = transitionTime;
            xnaTexture_ = renderTarget;
            _format = Texture2DPixelFormat.kTexture2DPixelFormat_RGBA8888;
            _size = new Vector(num, num2);
            _width = (uint)num;
            _height = (uint)num2;
            _maxS = w / (float)num;
            _maxT = h / (float)num2;
            _hasPremultipliedAlpha = true;
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

        private uint _name;

        public Quad2D[] quads;

        private uint _width;

        private uint _height;

        public int _lowypoint;

        public float _maxS;

        public float _maxT;

        private float _scaleX;

        private float _scaleY;

        private Texture2DPixelFormat _format;

        private Vector _size;

        private bool _hasPremultipliedAlpha;

        public Vector[] quadOffsets;

        public CTRRectangle[] quadRects;

        public int quadsCount;

        public int _realWidth;

        public int _realHeight;

        public float _invWidth;

        public float _invHeight;

        public Vector preCutSize;

        private bool _isWvga;

        private TexParams _localTexParams;

        private static readonly TexParams _defaultTexParams;

        private static readonly TexParams _texParams;
        private static CTRTexture2D root;

        private static CTRTexture2D tail;

        private CTRTexture2D next;

        private CTRTexture2D prev;

        public static Texture2DPixelFormat kTexture2DPixelFormat_Default = Texture2DPixelFormat.kTexture2DPixelFormat_RGBA8888;

        private static Texture2DPixelFormat _defaultAlphaPixelFormat = kTexture2DPixelFormat_Default;

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
