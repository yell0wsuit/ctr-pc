using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.platform
{
    internal class GLCanvas : NSObject
    {
        // (get) Token: 0x060002F3 RID: 755 RVA: 0x00011F34 File Offset: 0x00010134
        public NSRect bounds
        {
            get
            {
                Rectangle bounds = Global.XnaGame.GraphicsDevice.Viewport.Bounds;
                Rectangle currentSize = Global.ScreenSizeManager.CurrentSize;
                _bounds.size.width = currentSize.Width;
                _bounds.size.height = currentSize.Height;
                _bounds.origin.x = currentSize.X;
                _bounds.origin.y = currentSize.Y;
                return _bounds;
            }
        }

        public virtual NSObject initWithFrame(Rectangle frame)
        {
            return initWithFrame(new NSRect(frame));
        }

        public virtual NSObject initWithFrame(NSRect frame_UNUSED)
        {
            xOffset = 0;
            yOffset = 0;
            origWidth = backingWidth = 2560;
            origHeight = backingHeight = 1440;
            aspect = backingHeight / (float)backingWidth;
            touchesCount = 0;
            return this;
        }

        public virtual void initFPSMeterWithFont(Font font)
        {
            fpsFont = font;
            fpsText = new Text().initWithFont(fpsFont);
        }

        public virtual void drawFPS(float fps)
        {
            if (fpsText != null && fpsFont != null)
            {
                NSString @string = NSS(fps.ToString("F1"));
                fpsText.setString(@string);
                OpenGL.glColor4f(Color.White);
                OpenGL.glEnable(0);
                OpenGL.glEnable(1);
                OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                fpsText.x = 5f;
                fpsText.y = 5f;
                fpsText.draw();
                OpenGL.glDisable(1);
                OpenGL.glDisable(0);
            }
        }

        public virtual void prepareOpenGL()
        {
            OpenGL.glEnableClientState(11);
            OpenGL.glEnableClientState(12);
        }

        public virtual void setDefaultRealProjection()
        {
            setDefaultProjection();
        }

        public virtual void setDefaultProjection()
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
            OpenGL.glViewport(xOffset, yOffset, backingWidth, backingHeight);
            OpenGL.glMatrixMode(15);
            OpenGL.glLoadIdentity();
            OpenGL.glOrthof(0.0, origWidth, origHeight, 0.0, -1.0, 1.0);
            OpenGL.glMatrixMode(14);
            OpenGL.glLoadIdentity();
        }

        public virtual void drawRect(NSRect rect)
        {
        }

        public virtual void show()
        {
            setDefaultProjection();
        }

        public virtual void hide()
        {
        }

        public virtual void reshape()
        {
            Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
            backingWidth = scaledViewRect.Width;
            backingHeight = scaledViewRect.Height;
            setDefaultProjection();
        }

        public virtual void swapBuffers()
        {
        }

        public virtual void touchesBeganwithEvent(IList<TouchLocation> touches)
        {
            touchDelegate?.touchesBeganwithEvent(touches);
        }

        public virtual void touchesMovedwithEvent(IList<TouchLocation> touches)
        {
            touchDelegate?.touchesMovedwithEvent(touches);
        }

        public virtual void touchesEndedwithEvent(IList<TouchLocation> touches)
        {
            touchDelegate?.touchesEndedwithEvent(touches);
        }

        public virtual void touchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            touchDelegate?.touchesCancelledwithEvent(touches);
        }

        public virtual bool backButtonPressed()
        {
            return touchDelegate != null && touchDelegate.backButtonPressed();
        }

        public virtual bool menuButtonPressed()
        {
            return touchDelegate != null && touchDelegate.menuButtonPressed();
        }

        public List<TouchLocation> convertTouches(List<TouchLocation> touches)
        {
            return touches;
        }

        public virtual bool acceptsFirstResponder()
        {
            return true;
        }

        public virtual bool becomeFirstResponder()
        {
            return true;
        }

        public virtual void beforeRender()
        {
            setDefaultProjection();
            OpenGL.glDisable(1);
            OpenGL.glEnableClientState(11);
            OpenGL.glEnableClientState(12);
        }

        public virtual void afterRender()
        {
        }

        public const float MASTER_WIDTH = 2560f;

        public const float MASTER_HEIGHT = 1440f;

        private int origWidth;

        private int origHeight;

        public TouchDelegate touchDelegate;

        private NSPoint startPos;

        private Font fpsFont;

        private Text fpsText;

        private bool mouseDown;

        private NSSize cursorOrigSize;

        private NSSize cursorActiveOrigSize;

        private NSRect _bounds;

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
