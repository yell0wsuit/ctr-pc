using CutTheRope.ctr_commons;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CutTheRope.iframework.visual
{
    internal class CTRTexture2D : NSObject
    {
        public static void drawRectAtPoint(CTRTexture2D t, CTRRectangle rect, Vector point)
        {
            float num = t._invWidth * rect.x;
            float num2 = t._invHeight * rect.y;
            float num3 = num + t._invWidth * rect.w;
            float num4 = num2 + t._invHeight * rect.h;
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
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(t.name());
            OpenGL.glVertexPointer(3, 5, 0, pointer2);
            OpenGL.glTexCoordPointer(2, 5, 0, pointer);
            OpenGL.glDrawArrays(8, 0, 4);
        }

        public CTRTexture2D name()
        {
            return this;
        }

        public bool isWvga()
        {
            return this._isWvga;
        }

        public virtual void setQuadsCapacity(int n)
        {
            this.quadsCount = n;
            this.quads = new Quad2D[this.quadsCount];
            this.quadRects = new CTRRectangle[this.quadsCount];
            this.quadOffsets = new Vector[this.quadsCount];
        }

        public virtual void setQuadAt(CTRRectangle rect, int n)
        {
            this.quads[n] = GLDrawer.getTextureCoordinates(this, rect);
            this.quadRects[n] = rect;
            this.quadOffsets[n] = CTRMathHelper.vectZero;
        }

        public virtual void setWvga()
        {
            this._isWvga = true;
        }

        public virtual void setScale(float scaleX, float scaleY)
        {
            this._scaleX = scaleX;
            this._scaleY = scaleY;
            this.calculateForQuickDrawing();
        }

        public static void drawQuadAtPoint(CTRTexture2D t, int q, Vector point)
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
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(t.name());
            OpenGL.glVertexPointer(3, 5, 0, pointer);
            OpenGL.glTexCoordPointer(2, 5, 0, quad2D.toFloatArray());
            OpenGL.glDrawArrays(8, 0, 4);
        }

        public static void drawAtPoint(CTRTexture2D t, Vector point)
        {
            float[] pointer = [0f, 0f, t._maxS, 0f, 0f, t._maxT, t._maxS, t._maxT];
            float[] array = new float[12];
            array[0] = point.x;
            array[1] = point.y;
            array[3] = (float)t._realWidth + point.x;
            array[4] = point.y;
            array[6] = point.x;
            array[7] = (float)t._realHeight + point.y;
            array[9] = (float)t._realWidth + point.x;
            array[10] = (float)t._realHeight + point.y;
            float[] pointer2 = array;
            OpenGL.glEnable(0);
            OpenGL.glBindTexture(t.name());
            OpenGL.glVertexPointer(3, 5, 0, pointer2);
            OpenGL.glTexCoordPointer(2, 5, 0, pointer);
            OpenGL.glDrawArrays(8, 0, 4);
        }

        public virtual void calculateForQuickDrawing()
        {
            if (this._isWvga)
            {
                this._realWidth = (int)(this._width * this._maxS / this._scaleX);
                this._realHeight = (int)(this._height * this._maxT / this._scaleY);
                this._invWidth = 1f / (this._width / this._scaleX);
                this._invHeight = 1f / (this._height / this._scaleY);
                return;
            }
            this._realWidth = (int)(this._width * this._maxS);
            this._realHeight = (int)(this._height * this._maxT);
            this._invWidth = 1f / this._width;
            this._invHeight = 1f / this._height;
        }

        public static void setAntiAliasTexParameters()
        {
        }

        public static void setAliasTexParameters()
        {
        }

        public virtual void reg()
        {
            this.prev = CTRTexture2D.tail;
            if (this.prev != null)
            {
                this.prev.next = this;
            }
            else
            {
                CTRTexture2D.root = this;
            }
            CTRTexture2D.tail = this;
        }

        public virtual void unreg()
        {
            if (this.prev != null)
            {
                this.prev.next = this.next;
            }
            else
            {
                CTRTexture2D.root = this.next;
            }
            if (this.next != null)
            {
                this.next.prev = this.prev;
            }
            else
            {
                CTRTexture2D.tail = this.prev;
            }
            this.next = this.prev = null;
        }

        public virtual CTRTexture2D initWithPath(string path, bool assets)
        {
            if (base.init() == null)
            {
                return null;
            }
            this._resName = path;
            this._name = 65536U;
            this._localTexParams = CTRTexture2D._texParams;
            this.reg();
            this.xnaTexture_ = Images.get(path);
            if (this.xnaTexture_ == null)
            {
                return null;
            }
            this.imageLoaded(this.xnaTexture_.Width, this.xnaTexture_.Height);
            this.quadsCount = 0;
            this.calculateForQuickDrawing();
            this.resume();
            return this;
        }

        private static int calcRealSize(int size)
        {
            return size;
        }

        private void imageLoaded(int w, int h)
        {
            this._lowypoint = h;
            int num = CTRTexture2D.calcRealSize(w);
            int num2 = CTRTexture2D.calcRealSize(h);
            this._size = new Vector((float)num, (float)num2);
            this._width = (uint)num;
            this._height = (uint)num2;
            this._format = CTRTexture2D._defaultAlphaPixelFormat;
            this._maxS = (float)w / (float)num;
            this._maxT = (float)h / (float)num2;
            this._hasPremultipliedAlpha = true;
        }

        private void resume()
        {
        }

        public static void setDefaultAlphaPixelFormat(CTRTexture2D.Texture2DPixelFormat format)
        {
            CTRTexture2D._defaultAlphaPixelFormat = format;
        }

        public void optimizeMemory()
        {
            int lowypoint = this._lowypoint;
        }

        public virtual void suspend()
        {
        }

        public static void suspendAll()
        {
            for (CTRTexture2D texture2D = CTRTexture2D.root; texture2D != null; texture2D = texture2D.next)
            {
                texture2D.suspend();
            }
        }

        public static void resumeAll()
        {
            for (CTRTexture2D texture2D = CTRTexture2D.root; texture2D != null; texture2D = texture2D.next)
            {
                texture2D.resume();
            }
        }

        public virtual NSObject initFromPixels(int x, int y, int w, int h)
        {
            if (base.init() == null)
            {
                return null;
            }
            this._name = 65536U;
            this._lowypoint = -1;
            this._localTexParams = CTRTexture2D._defaultTexParams;
            this.reg();
            int num = CTRTexture2D.calcRealSize(w);
            int num2 = CTRTexture2D.calcRealSize(h);
            float transitionTime = Application.sharedRootController().transitionTime;
            Application.sharedRootController().transitionTime = -1f;
            RenderTarget2D renderTarget;
            if (Global.ScreenSizeManager.IsFullScreen)
            {
                CtrRenderer.onDrawFrame();
                renderTarget = OpenGL.DetachRenderTarget();
            }
            else
            {
                renderTarget = new RenderTarget2D(Global.GraphicsDevice, Global.GraphicsDevice.PresentationParameters.BackBufferWidth, Global.GraphicsDevice.PresentationParameters.BackBufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
                Global.GraphicsDevice.SetRenderTarget(renderTarget);
                CtrRenderer.onDrawFrame();
            }
            Global.GraphicsDevice.SetRenderTarget(null);
            Application.sharedRootController().transitionTime = transitionTime;
            this.xnaTexture_ = renderTarget;
            this._format = CTRTexture2D.Texture2DPixelFormat.kTexture2DPixelFormat_RGBA8888;
            this._size = new Vector((float)num, (float)num2);
            this._width = (uint)num;
            this._height = (uint)num2;
            this._maxS = (float)w / (float)num;
            this._maxT = (float)h / (float)num2;
            this._hasPremultipliedAlpha = true;
            this.quadsCount = 0;
            this.calculateForQuickDrawing();
            this.resume();
            return this;
        }

        public override void dealloc()
        {
            if (this.xnaTexture_ != null)
            {
                Images.free(this._resName);
                this.xnaTexture_ = null;
            }
        }

        private const int UNDEFINED_TEXTURE = 65536;

        public Microsoft.Xna.Framework.Graphics.Texture2D xnaTexture_;

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

        private CTRTexture2D.Texture2DPixelFormat _format;

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

        private CTRTexture2D.TexParams _localTexParams;

        private static CTRTexture2D.TexParams _defaultTexParams;

        private static CTRTexture2D.TexParams _texParams;

        private static CTRTexture2D.TexParams _texParamsCopy;

        private bool PixelCorrectionDone;

        private static CTRTexture2D root;

        private static CTRTexture2D tail;

        private CTRTexture2D next;

        private CTRTexture2D prev;

        public static CTRTexture2D.Texture2DPixelFormat kTexture2DPixelFormat_Default = CTRTexture2D.Texture2DPixelFormat.kTexture2DPixelFormat_RGBA8888;

        private static CTRTexture2D.Texture2DPixelFormat _defaultAlphaPixelFormat = CTRTexture2D.kTexture2DPixelFormat_Default;

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

        private struct TexParams
        {
            private uint minFilter;

            private uint magFilter;

            private uint wrapS;

            private uint wrapT;
        }
    }
}
