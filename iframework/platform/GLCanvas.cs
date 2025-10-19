using System;
using System.Collections.Generic;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.iframework.platform
{
	// Token: 0x02000059 RID: 89
	internal class GLCanvas : NSObject
	{
		// Token: 0x17000023 RID: 35
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

		// Token: 0x060002F4 RID: 756 RVA: 0x00011FC6 File Offset: 0x000101C6
		public virtual NSObject initWithFrame(Rectangle frame)
		{
			return this.initWithFrame(new NSRect(frame));
		}

		// Token: 0x060002F5 RID: 757 RVA: 0x00011FD4 File Offset: 0x000101D4
		public virtual NSObject initWithFrame(NSRect frame_UNUSED)
		{
			this.xOffset = 0;
			this.yOffset = 0;
			this.origWidth = (this.backingWidth = 2560);
			this.origHeight = (this.backingHeight = 1440);
			this.aspect = (float)this.backingHeight / (float)this.backingWidth;
			this.touchesCount = 0;
			return this;
		}

		// Token: 0x060002F6 RID: 758 RVA: 0x00012034 File Offset: 0x00010234
		public virtual void initFPSMeterWithFont(Font font)
		{
			this.fpsFont = font;
			this.fpsText = new Text().initWithFont(this.fpsFont);
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x00012054 File Offset: 0x00010254
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

		// Token: 0x060002F8 RID: 760 RVA: 0x000120EE File Offset: 0x000102EE
		public virtual void prepareOpenGL()
		{
			OpenGL.glEnableClientState(11);
			OpenGL.glEnableClientState(12);
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x000120FE File Offset: 0x000102FE
		public virtual void setDefaultRealProjection()
		{
			this.setDefaultProjection();
		}

		// Token: 0x060002FA RID: 762 RVA: 0x00012108 File Offset: 0x00010308
		public virtual void setDefaultProjection()
		{
			if (Global.ScreenSizeManager.IsFullScreen)
			{
				this.xOffset = Global.ScreenSizeManager.ScaledViewRect.X;
				this.xOffsetScaled = (int)((double)((float)(-(float)this.xOffset) * 1f) / Global.ScreenSizeManager.WidthAspectRatio);
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

		// Token: 0x060002FB RID: 763 RVA: 0x000121E1 File Offset: 0x000103E1
		public virtual void drawRect(NSRect rect)
		{
		}

		// Token: 0x060002FC RID: 764 RVA: 0x000121E3 File Offset: 0x000103E3
		public virtual void show()
		{
			this.setDefaultProjection();
		}

		// Token: 0x060002FD RID: 765 RVA: 0x000121EB File Offset: 0x000103EB
		public virtual void hide()
		{
		}

		// Token: 0x060002FE RID: 766 RVA: 0x000121F0 File Offset: 0x000103F0
		public virtual void reshape()
		{
			Rectangle scaledViewRect = Global.ScreenSizeManager.ScaledViewRect;
			this.backingWidth = scaledViewRect.Width;
			this.backingHeight = scaledViewRect.Height;
			this.setDefaultProjection();
		}

		// Token: 0x060002FF RID: 767 RVA: 0x00012226 File Offset: 0x00010426
		public virtual void swapBuffers()
		{
		}

		// Token: 0x06000300 RID: 768 RVA: 0x00012228 File Offset: 0x00010428
		public virtual void touchesBeganwithEvent(IList<TouchLocation> touches)
		{
			if (this.touchDelegate != null)
			{
				this.touchDelegate.touchesBeganwithEvent(touches);
			}
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0001223F File Offset: 0x0001043F
		public virtual void touchesMovedwithEvent(IList<TouchLocation> touches)
		{
			if (this.touchDelegate != null)
			{
				this.touchDelegate.touchesMovedwithEvent(touches);
			}
		}

		// Token: 0x06000302 RID: 770 RVA: 0x00012256 File Offset: 0x00010456
		public virtual void touchesEndedwithEvent(IList<TouchLocation> touches)
		{
			if (this.touchDelegate != null)
			{
				this.touchDelegate.touchesEndedwithEvent(touches);
			}
		}

		// Token: 0x06000303 RID: 771 RVA: 0x0001226D File Offset: 0x0001046D
		public virtual void touchesCancelledwithEvent(IList<TouchLocation> touches)
		{
			if (this.touchDelegate != null)
			{
				this.touchDelegate.touchesCancelledwithEvent(touches);
			}
		}

		// Token: 0x06000304 RID: 772 RVA: 0x00012284 File Offset: 0x00010484
		public virtual bool backButtonPressed()
		{
			return this.touchDelegate != null && this.touchDelegate.backButtonPressed();
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0001229B File Offset: 0x0001049B
		public virtual bool menuButtonPressed()
		{
			return this.touchDelegate != null && this.touchDelegate.menuButtonPressed();
		}

		// Token: 0x06000306 RID: 774 RVA: 0x000122B2 File Offset: 0x000104B2
		public List<TouchLocation> convertTouches(List<TouchLocation> touches)
		{
			return touches;
		}

		// Token: 0x06000307 RID: 775 RVA: 0x000122B5 File Offset: 0x000104B5
		public virtual bool acceptsFirstResponder()
		{
			return true;
		}

		// Token: 0x06000308 RID: 776 RVA: 0x000122B8 File Offset: 0x000104B8
		public virtual bool becomeFirstResponder()
		{
			return true;
		}

		// Token: 0x06000309 RID: 777 RVA: 0x000122BB File Offset: 0x000104BB
		public virtual void beforeRender()
		{
			this.setDefaultProjection();
			OpenGL.glDisable(1);
			OpenGL.glEnableClientState(11);
			OpenGL.glEnableClientState(12);
		}

		// Token: 0x0600030A RID: 778 RVA: 0x000122D7 File Offset: 0x000104D7
		public virtual void afterRender()
		{
		}

		// Token: 0x0400024C RID: 588
		public const float MASTER_WIDTH = 2560f;

		// Token: 0x0400024D RID: 589
		public const float MASTER_HEIGHT = 1440f;

		// Token: 0x0400024E RID: 590
		private int origWidth;

		// Token: 0x0400024F RID: 591
		private int origHeight;

		// Token: 0x04000250 RID: 592
		public TouchDelegate touchDelegate;

		// Token: 0x04000251 RID: 593
		private NSPoint startPos;

		// Token: 0x04000252 RID: 594
		private Font fpsFont;

		// Token: 0x04000253 RID: 595
		private Text fpsText;

		// Token: 0x04000254 RID: 596
		private bool mouseDown;

		// Token: 0x04000255 RID: 597
		private NSSize cursorOrigSize;

		// Token: 0x04000256 RID: 598
		private NSSize cursorActiveOrigSize;

		// Token: 0x04000257 RID: 599
		private NSRect _bounds;

		// Token: 0x04000258 RID: 600
		public bool isFullscreen;

		// Token: 0x04000259 RID: 601
		public float aspect;

		// Token: 0x0400025A RID: 602
		public int touchesCount;

		// Token: 0x0400025B RID: 603
		public int xOffset;

		// Token: 0x0400025C RID: 604
		public int yOffset;

		// Token: 0x0400025D RID: 605
		public int xOffsetScaled;

		// Token: 0x0400025E RID: 606
		public int yOffsetScaled;

		// Token: 0x0400025F RID: 607
		public int backingWidth;

		// Token: 0x04000260 RID: 608
		public int backingHeight;
	}
}
