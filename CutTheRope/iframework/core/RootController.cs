using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.core
{
    internal class RootController : ViewController
    {
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

        public bool isTransitionActive()
        {
            return this.transitionTime != -1f;
        }

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
                    this.prevScreenImage?.xnaTexture_.Dispose();
                    this.prevScreenImage = null;
                    NSObject.NSREL(this.nextScreenImage);
                    this.nextScreenImage?.xnaTexture_.Dispose();
                    this.nextScreenImage = null;
                }
            }
            OpenGL.glPopMatrix();
            Application.sharedCanvas().afterRender();
        }

        private void applyLandscape()
        {
        }

        public virtual void setViewTransition(int transition)
        {
            this.viewTransition = transition;
        }

        private void setViewTransitionDelay(float delay)
        {
            this.transitionDelay = delay;
        }

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
                float num = CTRMathHelper.MIN(1.0, (double)((this.transitionDelay - (this.transitionTime - this.lastTime)) / this.transitionDelay));
                if ((double)num < 0.5)
                {
                    if (this.prevScreenImage != null)
                    {
                        RGBAColor fill = (this.viewTransition == 4) ? RGBAColor.MakeRGBA(0.0, 0.0, 0.0, (double)num * 2.0) : RGBAColor.MakeRGBA(1.0, 1.0, 1.0, (double)num * 2.0);
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
                    RGBAColor fill2 = (this.viewTransition == 4) ? RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 2.0 - (double)num * 2.0) : RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 2.0 - (double)num * 2.0);
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

        public override void activate()
        {
            base.activate();
        }

        private void runLoop()
        {
        }

        public virtual void onControllerActivated(ViewController c)
        {
            this.setCurrentController(c);
        }

        public virtual void onControllerDeactivated(ViewController c)
        {
            this.setCurrentController(null);
        }

        public virtual void onControllerPaused(ViewController c)
        {
            this.setCurrentController(null);
        }

        public virtual void onControllerUnpaused(ViewController c)
        {
            this.setCurrentController(c);
        }

        public virtual void onControllerDeactivationRequest(ViewController c)
        {
            this.deactivateCurrentController = true;
        }

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
                this.nextScreenImage?.xnaTexture_.Dispose();
                this.nextScreenImage = this.screenGrabber.grab();
                NSObject.NSRET(this.nextScreenImage);
                OpenGL.glLoadIdentity();
            }
        }

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
                this.prevScreenImage?.xnaTexture_.Dispose();
                this.prevScreenImage = this.screenGrabber.grab();
                NSObject.NSRET(this.prevScreenImage);
                OpenGL.glLoadIdentity();
            }
        }

        public virtual bool isSuspended()
        {
            return this.suspended;
        }

        public virtual void suspend()
        {
            this.suspended = true;
        }

        public virtual void resume()
        {
            this.suspended = false;
        }

        public override bool mouseMoved(float x, float y)
        {
            return this.currentController.mouseMoved(x, y);
        }

        public override bool backButtonPressed()
        {
            return this.suspended || this.transitionTime != -1f || this.currentController.backButtonPressed();
        }

        public override bool menuButtonPressed()
        {
            return this.suspended || this.transitionTime != -1f || this.currentController.menuButtonPressed();
        }

        public override bool touchesBeganwithEvent(IList<TouchLocation> touches)
        {
            return !this.suspended && (this.transitionTime != -1f || this.currentController.touchesBeganwithEvent(touches));
        }

        public override bool touchesMovedwithEvent(IList<TouchLocation> touches)
        {
            return !this.suspended && (this.transitionTime != -1f || this.currentController.touchesMovedwithEvent(touches));
        }

        public override bool touchesEndedwithEvent(IList<TouchLocation> touches)
        {
            return !this.suspended && (this.transitionTime != -1f || this.currentController.touchesEndedwithEvent(touches));
        }

        public override bool touchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            return this.currentController.touchesCancelledwithEvent(touches);
        }

        public virtual void setCurrentController(ViewController c)
        {
            this.currentController = c;
        }

        public virtual ViewController getCurrentController()
        {
            return this.currentController;
        }

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

        public const int TRANSITION_SLIDE_HORIZONTAL_RIGHT = 0;

        public const int TRANSITION_SLIDE_HORIZONTAL_LEFT = 1;

        public const int TRANSITION_SLIDE_VERTICAL_UP = 2;

        public const int TRANSITION_SLIDE_VERTICAL_DON = 3;

        public const int TRANSITION_FADE_OUT_BLACK = 4;

        public const int TRANSITION_FADE_OUT_WHITE = 5;

        public const int TRANSITION_REVEAL = 6;

        public const int TRANSITIONS_COUNT = 7;

        public const float TRANSITION_DEFAULT_DELAY = 0.4f;

        public int viewTransition;

        public float transitionTime;

        private float transitionDelay;

        private View previousView;

        private CTRTexture2D prevScreenImage;

        private CTRTexture2D nextScreenImage;

        private Grabber screenGrabber;

        private bool deactivateCurrentController;

        private ViewController currentController;

        private float lastTime;

        public bool suspended;
    }
}
