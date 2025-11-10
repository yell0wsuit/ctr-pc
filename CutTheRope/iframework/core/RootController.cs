using CutTheRope.desktop;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
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
                viewTransition = -1;
                transitionTime = -1f;
                previousView = null;
                transitionDelay = 0.4f;
                screenGrabber = (Grabber)new Grabber().init();
                prevScreenImage = null;
                nextScreenImage = null;
                deactivateCurrentController = false;
            }
            return this;
        }

        public void performTick(float delta)
        {
            lastTime += delta;
            if (transitionTime == -1f)
            {
                currentController.update(delta);
            }
            if (deactivateCurrentController)
            {
                deactivateCurrentController = false;
                currentController.deactivateImmediately();
            }
        }

        public bool isTransitionActive()
        {
            return transitionTime != -1f;
        }

        public void performDraw()
        {
            if (currentController.activeViewID == -1)
            {
                return;
            }
            Application.sharedCanvas().beforeRender();
            OpenGL.glPushMatrix();
            applyLandscape();
            if (transitionTime == -1f)
            {
                currentController.activeView().draw();
            }
            else
            {
                drawViewTransition();
                if (lastTime > transitionTime)
                {
                    transitionTime = -1f;
                    NSREL(prevScreenImage);
                    prevScreenImage?.xnaTexture_.Dispose();
                    prevScreenImage = null;
                    NSREL(nextScreenImage);
                    nextScreenImage?.xnaTexture_.Dispose();
                    nextScreenImage = null;
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
            viewTransition = transition;
        }

        private void setViewTransitionDelay(float delay)
        {
            transitionDelay = delay;
        }

        private void drawViewTransition()
        {
            OpenGL.glColor4f(Color.White);
            OpenGL.glEnable(0);
            OpenGL.glEnable(1);
            OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
            Application.sharedCanvas().setDefaultRealProjection();
            int num2 = viewTransition;
            if (num2 - 4 <= 1)
            {
                float num = MIN(1.0, (double)((transitionDelay - (transitionTime - lastTime)) / transitionDelay));
                if ((double)num < 0.5)
                {
                    if (prevScreenImage != null)
                    {
                        RGBAColor fill = (viewTransition == 4) ? RGBAColor.MakeRGBA(0.0, 0.0, 0.0, (double)num * 2.0) : RGBAColor.MakeRGBA(1.0, 1.0, 1.0, (double)num * 2.0);
                        Grabber.drawGrabbedImage(prevScreenImage, 0, 0);
                        OpenGL.glDisable(0);
                        OpenGL.glEnable(1);
                        OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                        GLDrawer.drawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, fill);
                        OpenGL.glDisable(1);
                    }
                    else
                    {
                        if (viewTransition == 4)
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
                else if (nextScreenImage != null)
                {
                    RGBAColor fill2 = (viewTransition == 4) ? RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 2.0 - ((double)num * 2.0)) : RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 2.0 - ((double)num * 2.0));
                    Grabber.drawGrabbedImage(nextScreenImage, 0, 0);
                    OpenGL.glDisable(0);
                    OpenGL.glEnable(1);
                    OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
                    GLDrawer.drawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, fill2);
                    OpenGL.glDisable(1);
                }
                else
                {
                    if (viewTransition == 4)
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
            applyLandscape();
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
            setCurrentController(c);
        }

        public virtual void onControllerDeactivated(ViewController c)
        {
            setCurrentController(null);
        }

        public virtual void onControllerPaused(ViewController c)
        {
            setCurrentController(null);
        }

        public virtual void onControllerUnpaused(ViewController c)
        {
            setCurrentController(c);
        }

        public virtual void onControllerDeactivationRequest(ViewController c)
        {
            deactivateCurrentController = true;
        }

        public virtual void onControllerViewShow(View v)
        {
            if (viewTransition != -1 && previousView != null)
            {
                Application.sharedCanvas().setDefaultProjection();
                OpenGL.glClearColor(Color.Black);
                OpenGL.glClear(0);
                transitionTime = lastTime + transitionDelay;
                applyLandscape();
                currentController.activeView().draw();
                NSREL(nextScreenImage);
                nextScreenImage?.xnaTexture_.Dispose();
                nextScreenImage = screenGrabber.grab();
                NSRET(nextScreenImage);
                OpenGL.glLoadIdentity();
            }
        }

        public virtual void onControllerViewHide(View v)
        {
            previousView = v;
            if (viewTransition != -1 && previousView != null)
            {
                Application.sharedCanvas().setDefaultProjection();
                OpenGL.glClearColor(Color.Black);
                OpenGL.glClear(0);
                applyLandscape();
                previousView.draw();
                NSREL(prevScreenImage);
                prevScreenImage?.xnaTexture_.Dispose();
                prevScreenImage = screenGrabber.grab();
                NSRET(prevScreenImage);
                OpenGL.glLoadIdentity();
            }
        }

        public virtual bool isSuspended()
        {
            return suspended;
        }

        public virtual void suspend()
        {
            suspended = true;
        }

        public virtual void resume()
        {
            suspended = false;
        }

        public override bool mouseMoved(float x, float y)
        {
            return currentController.mouseMoved(x, y);
        }

        public override bool backButtonPressed()
        {
            return suspended || transitionTime != -1f || currentController.backButtonPressed();
        }

        public override bool menuButtonPressed()
        {
            return suspended || transitionTime != -1f || currentController.menuButtonPressed();
        }

        public override bool touchesBeganwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.touchesBeganwithEvent(touches));
        }

        public override bool touchesMovedwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.touchesMovedwithEvent(touches));
        }

        public override bool touchesEndedwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.touchesEndedwithEvent(touches));
        }

        public override bool touchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            return currentController.touchesCancelledwithEvent(touches);
        }

        public virtual void setCurrentController(ViewController c)
        {
            currentController = c;
        }

        public virtual ViewController getCurrentController()
        {
            return currentController;
        }

        public override void fullscreenToggled(bool isFullscreen)
        {
            try
            {
                currentController.fullscreenToggled(isFullscreen);
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
