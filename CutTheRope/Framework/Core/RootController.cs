using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework.Platform;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.Framework.Core
{
    internal class RootController(ViewController parent) : ViewController(parent)
    {
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
            Renderer.PushMatrix();
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
                    prevScreenImage?.xnaTexture_.Dispose();
                    prevScreenImage = null;
                    nextScreenImage?.xnaTexture_.Dispose();
                    nextScreenImage = null;
                }
            }
            Renderer.PopMatrix();
            GLCanvas.AfterRender();
        }

        private static void ApplyLandscape()
        {
        }

        public virtual void SetViewTransition(int transition)
        {
            viewTransition = transition;
        }

        private void DrawViewTransition()
        {
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.Enable(Renderer.GL_BLEND);
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            Application.SharedCanvas().SetDefaultRealProjection();
            int transitionType = viewTransition;
            if (transitionType - 4 <= 1)
            {
                float transitionProgress = MIN(1, (transitionDelay - (transitionTime - lastTime)) / transitionDelay);
                if (transitionProgress < 0.5f)
                {
                    if (prevScreenImage != null)
                    {
                        RGBAColor fadeOverlayColor = viewTransition == 4
                            ? RGBAColor.MakeRGBA(0, 0, 0, transitionProgress * 2)
                            : RGBAColor.MakeRGBA(1, 1, 1, transitionProgress * 2);
                        Grabber.DrawGrabbedImage(prevScreenImage, 0, 0);
                        Renderer.Disable(Renderer.GL_TEXTURE_2D);
                        Renderer.Enable(Renderer.GL_BLEND);
                        Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                        DrawHelper.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, fadeOverlayColor);
                        Renderer.Disable(Renderer.GL_BLEND);
                    }
                    else
                    {
                        if (viewTransition == 4)
                        {
                            Renderer.SetClearColor(Color.Black);
                        }
                        else
                        {
                            Renderer.SetClearColor(Color.White);
                        }
                        Renderer.Clear(0);
                    }
                }
                else if (nextScreenImage != null)
                {
                    RGBAColor revealOverlayColor = viewTransition == 4
                        ? RGBAColor.MakeRGBA(0, 0, 0, 2 - (transitionProgress * 2))
                        : RGBAColor.MakeRGBA(1, 1, 1, 2 - (transitionProgress * 2));
                    Grabber.DrawGrabbedImage(nextScreenImage, 0, 0);
                    Renderer.Disable(Renderer.GL_TEXTURE_2D);
                    Renderer.Enable(Renderer.GL_BLEND);
                    Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
                    DrawHelper.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, revealOverlayColor);
                    Renderer.Disable(Renderer.GL_BLEND);
                }
                else
                {
                    if (viewTransition == 4)
                    {
                        Renderer.SetClearColor(Color.Black);
                    }
                    else
                    {
                        Renderer.SetClearColor(Color.White);
                    }
                    Renderer.Clear(0);
                }
            }
            ApplyLandscape();
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.Disable(Renderer.GL_BLEND);
        }

        public override void Activate()
        {
            base.Activate();
        }

        public virtual void OnControllerActivated(ViewController controller)
        {
            SetCurrentController(controller);
        }

        public virtual void OnControllerDeactivated(ViewController controller)
        {
            SetCurrentController(null);
        }

        public virtual void OnControllerPaused(ViewController controller)
        {
            SetCurrentController(null);
        }

        public virtual void OnControllerUnpaused(ViewController controller)
        {
            SetCurrentController(controller);
        }

        public virtual void OnControllerDeactivationRequest(ViewController controller)
        {
            deactivateCurrentController = true;
        }

        public virtual void OnControllerViewShow(View view)
        {
            if (viewTransition != -1 && previousView != null)
            {
                Application.SharedCanvas().SetDefaultProjection();
                Renderer.SetClearColor(Color.Black);
                Renderer.Clear(0);
                transitionTime = lastTime + transitionDelay;
                ApplyLandscape();
                currentController.ActiveView().Draw();
                nextScreenImage?.xnaTexture_.Dispose();
                nextScreenImage = Grabber.Grab();
                Renderer.LoadIdentity();
            }
        }

        public virtual void OnControllerViewHide(View view)
        {
            previousView = view;
            if (viewTransition != -1 && previousView != null)
            {
                Application.SharedCanvas().SetDefaultProjection();
                Renderer.SetClearColor(Color.Black);
                Renderer.Clear(0);
                ApplyLandscape();
                previousView.Draw();
                prevScreenImage?.xnaTexture_.Dispose();
                prevScreenImage = Grabber.Grab();
                Renderer.LoadIdentity();
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

        /// <summary>
        /// Handles mouse wheel scrolling by forwarding it to the current active controller.
        /// </summary>
        /// <param name="scrollDelta">
        /// The mouse wheel scroll delta from the input system.
        /// </param>
        /// <returns>
        /// True if the scroll was handled by the active controller, false if ignored
        /// (due to no controller, suspended state, or active transition).
        /// </returns>
        /// <remarks>
        /// This method acts as a router, forwarding scroll events from the game loop
        /// to the currently active controller. Events are blocked during transitions
        /// or when the application is suspended.
        /// </remarks>
        public override bool HandleMouseWheel(int scrollDelta)
        {
            return currentController != null && !suspended && transitionTime == -1f && currentController.HandleMouseWheel(scrollDelta);
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

        public virtual void SetCurrentController(ViewController controller)
        {
            currentController = controller;
        }

        public virtual ViewController GetCurrentController()
        {
            return currentController;
        }

        public override void FullscreenToggled(bool isFullscreen)
        {
            currentController?.FullscreenToggled(isFullscreen);
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

        public int viewTransition = -1;

        public float transitionTime = -1f;

        private readonly float transitionDelay = 0.4f;

        private View previousView;

        private CTRTexture2D prevScreenImage;

        private CTRTexture2D nextScreenImage;

        // private readonly Grabber screenGrabber = new();

        private bool deactivateCurrentController;

        private ViewController currentController;

        private float lastTime;

        public bool suspended;
    }
}
