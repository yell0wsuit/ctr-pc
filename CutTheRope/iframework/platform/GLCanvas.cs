using System.Collections.Generic;
using System.Globalization;

using CutTheRope.desktop;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.iframework.platform
{
    internal sealed class GLCanvas : FrameworkTypes
    {
        // (get) Token: 0x060002F3 RID: 755 RVA: 0x00011F34 File Offset: 0x00010134
        public Rectangle Bounds
        {
            get
            {
                _ = Global.XnaGame.GraphicsDevice.Viewport.Bounds;
                Rectangle currentSize = Global.ScreenSizeManager.CurrentSize;
                _bounds.Width = currentSize.Width;
                _bounds.Height = currentSize.Height;
                _bounds.X = currentSize.X;
                _bounds.Y = currentSize.Y;
                return _bounds;
            }
        }

        public GLCanvas InitWithFrame(Rectangle frame_UNUSED)
        {
            xOffset = 0;
            yOffset = 0;
            origWidth = backingWidth = 2560;
            origHeight = backingHeight = 1440;
            aspect = backingHeight / (float)backingWidth;
            touchesCount = 0;
            return this;
        }

        public void InitFPSMeterWithFont(Font font)
        {
            fpsFont = font;
            fpsText = new Text().InitWithFont(fpsFont);
        }

        public void DrawFPS(float fps)
        {
            if (fpsText != null && fpsFont != null)
            {
                string @string = fps.ToString("F1", CultureInfo.InvariantCulture);
                fpsText.SetString(@string);
                OpenGL.GlColor4f(Color.White);
                OpenGL.GlEnable(0);
                OpenGL.GlEnable(1);
                OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                fpsText.x = 5f;
                fpsText.y = 5f;
                fpsText.Draw();
                OpenGL.GlDisable(1);
                OpenGL.GlDisable(0);
            }
        }

        public static void PrepareOpenGL()
        {
            OpenGL.GlEnableClientState(11);
            OpenGL.GlEnableClientState(12);
        }

        public void SetDefaultRealProjection()
        {
            SetDefaultProjection();
        }

        public void SetDefaultProjection()
        {
            if (Global.ScreenSizeManager.IsFullScreen)
            {
                xOffset = Global.ScreenSizeManager.ScaledViewRect.X;
                xOffsetScaled = (int)((double)((float)-(float)xOffset * 1f) / Global.ScreenSizeManager.WidthAspectRatio);
                isFullscreen = true;
            }
            else
            {
                xOffset = 0;
                xOffsetScaled = 0;
                isFullscreen = false;
            }
            OpenGL.GlViewport(xOffset, yOffset, backingWidth, backingHeight);
            OpenGL.GlMatrixMode(15);
            OpenGL.GlLoadIdentity();
            OpenGL.GlOrthof(0.0, origWidth, origHeight, 0.0, -1.0, 1.0);
            OpenGL.GlMatrixMode(14);
            OpenGL.GlLoadIdentity();
        }

        public static void DrawRect(Rectangle rect)
        {
        }

        public void Show()
        {
            SetDefaultProjection();
        }

        public static void Hide()
        {
        }

        public void Reshape()
        {
            Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
            backingWidth = scaledViewRect.Width;
            backingHeight = scaledViewRect.Height;
            SetDefaultProjection();
        }

        public static void SwapBuffers()
        {
        }

        public void TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            _ = (touchDelegate?.TouchesBeganwithEvent(touches));
        }

        public void TouchesMovedwithEvent(IList<TouchLocation> touches)
        {
            _ = (touchDelegate?.TouchesMovedwithEvent(touches));
        }

        public void TouchesEndedwithEvent(IList<TouchLocation> touches)
        {
            _ = (touchDelegate?.TouchesEndedwithEvent(touches));
        }

        public void TouchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            _ = (touchDelegate?.TouchesCancelledwithEvent(touches));
        }

        public bool BackButtonPressed()
        {
            return touchDelegate != null && touchDelegate.BackButtonPressed();
        }

        public bool MenuButtonPressed()
        {
            return touchDelegate != null && touchDelegate.MenuButtonPressed();
        }

        public static List<TouchLocation> ConvertTouches(List<TouchLocation> touches)
        {
            return touches;
        }

        public static bool AcceptsFirstResponder()
        {
            return true;
        }

        public static bool BecomeFirstResponder()
        {
            return true;
        }

        public void BeforeRender()
        {
            SetDefaultProjection();
            OpenGL.GlDisable(1);
            OpenGL.GlEnableClientState(11);
            OpenGL.GlEnableClientState(12);
        }

        public static void AfterRender()
        {
        }

        public const float MASTER_WIDTH = 2560f;

        public const float MASTER_HEIGHT = 1440f;

        private int origWidth;

        private int origHeight;

        public ITouchDelegate touchDelegate;
        private Font fpsFont;

        private Text fpsText;
        private Rectangle _bounds;

        public bool isFullscreen;

        public float aspect;

        public int touchesCount;

        public int xOffset;

        public int yOffset;

        public int xOffsetScaled;

        public int yOffsetScaled;

        public int backingWidth;

        public int backingHeight;
    }
}
