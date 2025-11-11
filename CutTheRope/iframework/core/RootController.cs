using CutTheRope.desktop;
using CutTheRope.iframework.platform;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.core
{
    internal class RootController : ViewController
    {
        public override NSObject InitWithParent(ViewController p)
        {
            if (base.InitWithParent(p) != null)
            {
                viewTransition = -1;
                transitionTime = -1f;
                previousView = null;
                transitionDelay = 0.4f;
                screenGrabber = (Grabber)new Grabber().Init();
                prevScreenImage = null;
                nextScreenImage = null;
                deactivateCurrentController = false;
            }
            return this;
        }

        public void PerformTick(float delta)
        {
            lastTime += delta;
            if (transitionTime == -1f)
            {
                currentController.Update(delta);
            }
            if (deactivateCurrentController)
            {
                deactivateCurrentController = false;
                currentController.DeactivateImmediately();
            }
        }

        public bool IsTransitionActive()
        {
            return transitionTime != -1f;
        }

        public void PerformDraw()
        {
            if (currentController.activeViewID == -1)
            {
                return;
            }
            Application.SharedCanvas().BeforeRender();
            OpenGL.GlPushMatrix();
            ApplyLandscape();
            if (transitionTime == -1f)
            {
                currentController.ActiveView().Draw();
            }
            else
            {
                DrawViewTransition();
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
            OpenGL.GlPopMatrix();
            GLCanvas.AfterRender();
        }

        private static void ApplyLandscape()
        {
        }

        public virtual void SetViewTransition(int transition)
        {
            viewTransition = transition;
        }

        private void SetViewTransitionDelay(float delay)
        {
            transitionDelay = delay;
        }

        private void DrawViewTransition()
        {
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(0);
            OpenGL.GlEnable(1);
            OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            Application.SharedCanvas().SetDefaultRealProjection();
            int num2 = viewTransition;
            if (num2 - 4 <= 1)
            {
                float num = MIN(1.0, (double)((transitionDelay - (transitionTime - lastTime)) / transitionDelay));
                if ((double)num < 0.5)
                {
                    if (prevScreenImage != null)
                    {
                        RGBAColor fill = viewTransition == 4 ? RGBAColor.MakeRGBA(0.0, 0.0, 0.0, (double)num * 2.0) : RGBAColor.MakeRGBA(1.0, 1.0, 1.0, (double)num * 2.0);
                        Grabber.DrawGrabbedImage(prevScreenImage, 0, 0);
                        OpenGL.GlDisable(0);
                        OpenGL.GlEnable(1);
                        OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                        GLDrawer.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, fill);
                        OpenGL.GlDisable(1);
                    }
                    else
                    {
                        if (viewTransition == 4)
                        {
                            OpenGL.GlClearColor(Color.Black);
                        }
                        else
                        {
                            OpenGL.GlClearColor(Color.White);
                        }
                        OpenGL.GlClear(0);
                    }
                }
                else if (nextScreenImage != null)
                {
                    RGBAColor fill2 = viewTransition == 4 ? RGBAColor.MakeRGBA(0.0, 0.0, 0.0, 2.0 - ((double)num * 2.0)) : RGBAColor.MakeRGBA(1.0, 1.0, 1.0, 2.0 - ((double)num * 2.0));
                    Grabber.DrawGrabbedImage(nextScreenImage, 0, 0);
                    OpenGL.GlDisable(0);
                    OpenGL.GlEnable(1);
                    OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                    GLDrawer.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, fill2);
                    OpenGL.GlDisable(1);
                }
                else
                {
                    if (viewTransition == 4)
                    {
                        OpenGL.GlClearColor(Color.Black);
                    }
                    else
                    {
                        OpenGL.GlClearColor(Color.White);
                    }
                    OpenGL.GlClear(0);
                }
            }
            ApplyLandscape();
            OpenGL.GlDisable(0);
            OpenGL.GlDisable(1);
        }

        public override void Activate()
        {
            base.Activate();
        }

        private static void RunLoop()
        {
        }

        public virtual void OnControllerActivated(ViewController c)
        {
            SetCurrentController(c);
        }

        public virtual void OnControllerDeactivated(ViewController c)
        {
            SetCurrentController(null);
        }

        public virtual void OnControllerPaused(ViewController c)
        {
            SetCurrentController(null);
        }

        public virtual void OnControllerUnpaused(ViewController c)
        {
            SetCurrentController(c);
        }

        public virtual void OnControllerDeactivationRequest(ViewController c)
        {
            deactivateCurrentController = true;
        }

        public virtual void OnControllerViewShow(View v)
        {
            if (viewTransition != -1 && previousView != null)
            {
                Application.SharedCanvas().SetDefaultProjection();
                OpenGL.GlClearColor(Color.Black);
                OpenGL.GlClear(0);
                transitionTime = lastTime + transitionDelay;
                ApplyLandscape();
                currentController.ActiveView().Draw();
                NSREL(nextScreenImage);
                nextScreenImage?.xnaTexture_.Dispose();
                nextScreenImage = Grabber.Grab();
                _ = NSRET(nextScreenImage);
                OpenGL.GlLoadIdentity();
            }
        }

        public virtual void OnControllerViewHide(View v)
        {
            previousView = v;
            if (viewTransition != -1 && previousView != null)
            {
                Application.SharedCanvas().SetDefaultProjection();
                OpenGL.GlClearColor(Color.Black);
                OpenGL.GlClear(0);
                ApplyLandscape();
                previousView.Draw();
                NSREL(prevScreenImage);
                prevScreenImage?.xnaTexture_.Dispose();
                prevScreenImage = Grabber.Grab();
                _ = NSRET(prevScreenImage);
                OpenGL.GlLoadIdentity();
            }
        }

        public virtual bool IsSuspended()
        {
            return suspended;
        }

        public virtual void Suspend()
        {
            suspended = true;
        }

        public virtual void Resume()
        {
            suspended = false;
        }

        public override bool MouseMoved(float x, float y)
        {
            return currentController.MouseMoved(x, y);
        }

        public override bool BackButtonPressed()
        {
            return suspended || transitionTime != -1f || currentController.BackButtonPressed();
        }

        public override bool MenuButtonPressed()
        {
            return suspended || transitionTime != -1f || currentController.MenuButtonPressed();
        }

        public override bool TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.TouchesBeganwithEvent(touches));
        }

        public override bool TouchesMovedwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.TouchesMovedwithEvent(touches));
        }

        public override bool TouchesEndedwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.TouchesEndedwithEvent(touches));
        }

        public override bool TouchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            return currentController.TouchesCancelledwithEvent(touches);
        }

        public virtual void SetCurrentController(ViewController c)
        {
            currentController = c;
        }

        public virtual ViewController GetCurrentController()
        {
            return currentController;
        }

        public override void FullscreenToggled(bool isFullscreen)
        {
            try
            {
                currentController.FullscreenToggled(isFullscreen);
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
