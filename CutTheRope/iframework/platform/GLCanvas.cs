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
                this._bounds.size.width = (float)currentSize.Width;
                this._bounds.size.height = (float)currentSize.Height;
                this._bounds.origin.x = (float)currentSize.X;
                this._bounds.origin.y = (float)currentSize.Y;
                return this._bounds;
            }
        }

        public virtual NSObject initWithFrame(Rectangle frame)
        {
            return this.initWithFrame(new NSRect(frame));
        }

        public virtual NSObject initWithFrame(NSRect frame_UNUSED)
        {
            this.xOffset = 0;
            this.yOffset = 0;
            this.origWidth = this.backingWidth = 2560;
            this.origHeight = this.backingHeight = 1440;
            this.aspect = (float)this.backingHeight / (float)this.backingWidth;
            this.touchesCount = 0;
            return this;
        }

        public virtual void initFPSMeterWithFont(Font font)
        {
            this.fpsFont = font;
            this.fpsText = new Text().initWithFont(this.fpsFont);
        }

        public virtual void drawFPS(float fps)
        {
            if (this.fpsText != null && this.fpsFont != null)
            {
                NSString @string = NSObject.NSS(fps.ToString("F1"));
                this.fpsText.setString(@string);
                OpenGL.glColor4f(Color.White);
                OpenGL.glEnable(0);
                OpenGL.glEnable(1);
                OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                this.fpsText.x = 5f;
                this.fpsText.y = 5f;
                this.fpsText.draw();
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
            this.setDefaultProjection();
        }

        public virtual void setDefaultProjection()
        {
            if (Global.ScreenSizeManager.IsFullScreen)
            {
                this.xOffset = Global.ScreenSizeManager.ScaledViewRect.X;
                this.xOffsetScaled = (int)((double)((float)-(float)this.xOffset * 1f) / Global.ScreenSizeManager.WidthAspectRatio);
                this.isFullscreen = true;
            }
            else
            {
                this.xOffset = 0;
                this.xOffsetScaled = 0;
                this.isFullscreen = false;
            }
            OpenGL.glViewport(this.xOffset, this.yOffset, this.backingWidth, this.backingHeight);
            OpenGL.glMatrixMode(15);
            OpenGL.glLoadIdentity();
            OpenGL.glOrthof(0.0, (double)this.origWidth, (double)this.origHeight, 0.0, -1.0, 1.0);
            OpenGL.glMatrixMode(14);
            OpenGL.glLoadIdentity();
        }

        public virtual void drawRect(NSRect rect)
        {
        }

        public virtual void show()
        {
            this.setDefaultProjection();
        }

        public virtual void hide()
        {
        }

        public virtual void reshape()
        {
            Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
            this.backingWidth = scaledViewRect.Width;
            this.backingHeight = scaledViewRect.Height;
            this.setDefaultProjection();
        }

        public virtual void swapBuffers()
        {
        }

        public virtual void touchesBeganwithEvent(IList<TouchLocation> touches)
        {
            this.touchDelegate?.touchesBeganwithEvent(touches);
        }

        public virtual void touchesMovedwithEvent(IList<TouchLocation> touches)
        {
            this.touchDelegate?.touchesMovedwithEvent(touches);
        }

        public virtual void touchesEndedwithEvent(IList<TouchLocation> touches)
        {
            this.touchDelegate?.touchesEndedwithEvent(touches);
        }

        public virtual void touchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            this.touchDelegate?.touchesCancelledwithEvent(touches);
        }

        public virtual bool backButtonPressed()
        {
            return this.touchDelegate != null && this.touchDelegate.backButtonPressed();
        }

        public virtual bool menuButtonPressed()
        {
            return this.touchDelegate != null && this.touchDelegate.menuButtonPressed();
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
            this.setDefaultProjection();
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
