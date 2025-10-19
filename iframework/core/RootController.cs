using System;
using System.Collections.Generic;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.iframework.core
{
	// Token: 0x02000069 RID: 105
	internal class RootController : ViewController
	{
		// Token: 0x060003F4 RID: 1012 RVA: 0x00015ABC File Offset: 0x00013CBC
		public override NSObject initWithParent(ViewController p)
		{
			if (base.initWithParent(p) != null)
			{
				this.viewTransition = -1;
				this.transitionTime = -1f;
				this.previousView = null;
				this.transitionDelay = 0.4f;
				this.screenGrabber = (Grabber)new Grabber().init();
				this.prevScreenImage = null;
				this.nextScreenImage = null;
				this.deactivateCurrentController = false;
			}
			return this;
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x00015B24 File Offset: 0x00013D24
		public void performTick(float delta)
		{
			this.lastTime += delta;
			if (this.transitionTime == -1f)
			{
				this.currentController.update(delta);
			}
			if (this.deactivateCurrentController)
			{
				this.deactivateCurrentController = false;
				this.currentController.deactivateImmediately();
			}
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x00015B72 File Offset: 0x00013D72
		public bool isTransitionActive()
		{
			return this.transitionTime != -1f;
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x00015B84 File Offset: 0x00013D84
		public void performDraw()
		{
			if (this.currentController.activeViewID == -1)
			{
				return;
			}
			Application.sharedCanvas().beforeRender();
			OpenGL.glPushMatrix();
			this.applyLandscape();
			if (this.transitionTime == -1f)
			{
				this.currentController.activeView().draw();
			}
			else
			{
				this.drawViewTransition();
				if (this.lastTime > this.transitionTime)
				{
					this.transitionTime = -1f;
					NSObject.NSREL(this.prevScreenImage);
					if (this.prevScreenImage != null)
					{
						this.prevScreenImage.xnaTexture_.Dispose();
					}
					this.prevScreenImage = null;
					NSObject.NSREL(this.nextScreenImage);
					if (this.nextScreenImage != null)
					{
						this.nextScreenImage.xnaTexture_.Dispose();
					}
					this.nextScreenImage = null;
				}
			}
			OpenGL.glPopMatrix();
			Application.sharedCanvas().afterRender();
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x00015C56 File Offset: 0x00013E56
		private void applyLandscape()
		{
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x00015C58 File Offset: 0x00013E58
		public virtual void setViewTransition(int transition)
		{
			this.viewTransition = transition;
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x00015C61 File Offset: 0x00013E61
		private void setViewTransitionDelay(float delay)
		{
			this.transitionDelay = delay;
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x00015C6C File Offset: 0x00013E6C
		private void drawViewTransition()
		{
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
			OpenGL.glEnable(1);
			OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			Application.sharedCanvas().setDefaultRealProjection();
			int num2 = this.viewTransition;
			if (num2 - 4 <= 1)
			{
				float num = CutTheRope.iframework.helpers.MathHelper.MIN(1.0, (double)((this.transitionDelay - (this.transitionTime - this.lastTime)) / this.transitionDelay));
				if ((double)num < 0.5)
				{
					if (this.prevScreenImage != null)
					{
						RGBAColor fill = ((this.viewTransition == 4) ? RGBAColor.MakeRGBA(0.0, 0.0, 0.0, (double)num * 2.0) : RGBAColor.MakeRGBA(1.0, 1.0, 1.0, (double)num * 2.0));
						Grabber.drawGrabbedImage(this.prevScreenImage, 0, 0);
						OpenGL.glDisable(0);
						OpenGL.glEnable(1);
						OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
						GLDrawer.drawSolidRectWOBorder(0f, 0f, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT, fill);
						OpenGL.glDisable(1);
					}
					else
					{
						if (this.viewTransition == 4)
						{
							OpenGL.glClearColor(Color.Black);
						}
						else
						{
							OpenGL.glClearColor(Color.White);
						}
						OpenGL.glClear(0);
					}
				}
				else if (this.nextScreenImage != null)
				{
					RGBAColor fill2 = ((this.viewTransition == 4) ? RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 2.0 - (double)num * 2.0) : RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 2.0 - (double)num * 2.0));
					Grabber.drawGrabbedImage(this.nextScreenImage, 0, 0);
					OpenGL.glDisable(0);
					OpenGL.glEnable(1);
					OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
					GLDrawer.drawSolidRectWOBorder(0f, 0f, FrameworkTypes.SCREEN_WIDTH, FrameworkTypes.SCREEN_HEIGHT, fill2);
					OpenGL.glDisable(1);
				}
				else
				{
					if (this.viewTransition == 4)
					{
						OpenGL.glClearColor(Color.Black);
					}
					else
					{
						OpenGL.glClearColor(Color.White);
					}
					OpenGL.glClear(0);
				}
			}
			this.applyLandscape();
			OpenGL.glDisable(0);
			OpenGL.glDisable(1);
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x00015EDD File Offset: 0x000140DD
		public override void activate()
		{
			base.activate();
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x00015EE5 File Offset: 0x000140E5
		private void runLoop()
		{
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00015EE7 File Offset: 0x000140E7
		public virtual void onControllerActivated(ViewController c)
		{
			this.setCurrentController(c);
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x00015EF0 File Offset: 0x000140F0
		public virtual void onControllerDeactivated(ViewController c)
		{
			this.setCurrentController(null);
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00015EF9 File Offset: 0x000140F9
		public virtual void onControllerPaused(ViewController c)
		{
			this.setCurrentController(null);
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x00015F02 File Offset: 0x00014102
		public virtual void onControllerUnpaused(ViewController c)
		{
			this.setCurrentController(c);
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x00015F0B File Offset: 0x0001410B
		public virtual void onControllerDeactivationRequest(ViewController c)
		{
			this.deactivateCurrentController = true;
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x00015F14 File Offset: 0x00014114
		public virtual void onControllerViewShow(View v)
		{
			if (this.viewTransition != -1 && this.previousView != null)
			{
				Application.sharedCanvas().setDefaultProjection();
				OpenGL.glClearColor(Color.Black);
				OpenGL.glClear(0);
				this.transitionTime = this.lastTime + this.transitionDelay;
				this.applyLandscape();
				this.currentController.activeView().draw();
				NSObject.NSREL(this.nextScreenImage);
				if (this.nextScreenImage != null)
				{
					this.nextScreenImage.xnaTexture_.Dispose();
				}
				this.nextScreenImage = this.screenGrabber.grab();
				NSObject.NSRET(this.nextScreenImage);
				OpenGL.glLoadIdentity();
			}
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x00015FC0 File Offset: 0x000141C0
		public virtual void onControllerViewHide(View v)
		{
			this.previousView = v;
			if (this.viewTransition != -1 && this.previousView != null)
			{
				Application.sharedCanvas().setDefaultProjection();
				OpenGL.glClearColor(Color.Black);
				OpenGL.glClear(0);
				this.applyLandscape();
				this.previousView.draw();
				NSObject.NSREL(this.prevScreenImage);
				if (this.prevScreenImage != null)
				{
					this.prevScreenImage.xnaTexture_.Dispose();
				}
				this.prevScreenImage = this.screenGrabber.grab();
				NSObject.NSRET(this.prevScreenImage);
				OpenGL.glLoadIdentity();
			}
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x00016055 File Offset: 0x00014255
		public virtual bool isSuspended()
		{
			return this.suspended;
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x0001605D File Offset: 0x0001425D
		public virtual void suspend()
		{
			this.suspended = true;
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x00016066 File Offset: 0x00014266
		public virtual void resume()
		{
			this.suspended = false;
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x0001606F File Offset: 0x0001426F
		public override bool mouseMoved(float x, float y)
		{
			return this.currentController.mouseMoved(x, y);
		}

		// Token: 0x06000409 RID: 1033 RVA: 0x0001607E File Offset: 0x0001427E
		public override bool backButtonPressed()
		{
			return this.suspended || this.transitionTime != -1f || this.currentController.backButtonPressed();
		}

		// Token: 0x0600040A RID: 1034 RVA: 0x000160A4 File Offset: 0x000142A4
		public override bool menuButtonPressed()
		{
			return this.suspended || this.transitionTime != -1f || this.currentController.menuButtonPressed();
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x000160CA File Offset: 0x000142CA
		public override bool touchesBeganwithEvent(IList<TouchLocation> touches)
		{
			return !this.suspended && (this.transitionTime != -1f || this.currentController.touchesBeganwithEvent(touches));
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x000160F1 File Offset: 0x000142F1
		public override bool touchesMovedwithEvent(IList<TouchLocation> touches)
		{
			return !this.suspended && (this.transitionTime != -1f || this.currentController.touchesMovedwithEvent(touches));
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x00016118 File Offset: 0x00014318
		public override bool touchesEndedwithEvent(IList<TouchLocation> touches)
		{
			return !this.suspended && (this.transitionTime != -1f || this.currentController.touchesEndedwithEvent(touches));
		}

		// Token: 0x0600040E RID: 1038 RVA: 0x0001613F File Offset: 0x0001433F
		public override bool touchesCancelledwithEvent(IList<TouchLocation> touches)
		{
			return this.currentController.touchesCancelledwithEvent(touches);
		}

		// Token: 0x0600040F RID: 1039 RVA: 0x0001614D File Offset: 0x0001434D
		public virtual void setCurrentController(ViewController c)
		{
			this.currentController = c;
		}

		// Token: 0x06000410 RID: 1040 RVA: 0x00016156 File Offset: 0x00014356
		public virtual ViewController getCurrentController()
		{
			return this.currentController;
		}

		// Token: 0x06000411 RID: 1041 RVA: 0x00016160 File Offset: 0x00014360
		public override void fullscreenToggled(bool isFullscreen)
		{
			try
			{
				this.currentController.fullscreenToggled(isFullscreen);
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x040002B0 RID: 688
		public const int TRANSITION_SLIDE_HORIZONTAL_RIGHT = 0;

		// Token: 0x040002B1 RID: 689
		public const int TRANSITION_SLIDE_HORIZONTAL_LEFT = 1;

		// Token: 0x040002B2 RID: 690
		public const int TRANSITION_SLIDE_VERTICAL_UP = 2;

		// Token: 0x040002B3 RID: 691
		public const int TRANSITION_SLIDE_VERTICAL_DON = 3;

		// Token: 0x040002B4 RID: 692
		public const int TRANSITION_FADE_OUT_BLACK = 4;

		// Token: 0x040002B5 RID: 693
		public const int TRANSITION_FADE_OUT_WHITE = 5;

		// Token: 0x040002B6 RID: 694
		public const int TRANSITION_REVEAL = 6;

		// Token: 0x040002B7 RID: 695
		public const int TRANSITIONS_COUNT = 7;

		// Token: 0x040002B8 RID: 696
		public const float TRANSITION_DEFAULT_DELAY = 0.4f;

		// Token: 0x040002B9 RID: 697
		public int viewTransition;

		// Token: 0x040002BA RID: 698
		public float transitionTime;

		// Token: 0x040002BB RID: 699
		private float transitionDelay;

		// Token: 0x040002BC RID: 700
		private View previousView;

		// Token: 0x040002BD RID: 701
		private Texture2D prevScreenImage;

		// Token: 0x040002BE RID: 702
		private Texture2D nextScreenImage;

		// Token: 0x040002BF RID: 703
		private Grabber screenGrabber;

		// Token: 0x040002C0 RID: 704
		private bool deactivateCurrentController;

		// Token: 0x040002C1 RID: 705
		private ViewController currentController;

		// Token: 0x040002C2 RID: 706
		private float lastTime;

		// Token: 0x040002C3 RID: 707
		public bool suspended;
	}
}
