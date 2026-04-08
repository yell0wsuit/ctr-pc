using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework.Platform;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.Framework.Core
{
    /// <summary>
    /// Top-level controller that owns the currently active controller, routes input,
    /// and manages screen-transition capture/drawing.
    /// </summary>
    /// <param name="parent">Parent controller reference passed to the base controller.</param>
    internal class RootController(ViewController parent) : ViewController(parent)
    {
        /// <summary>
        /// Advances the active controller and applies any pending deactivation requests.
        /// </summary>
        /// <param name="delta">Elapsed frame time in seconds.</param>
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

        /// <summary>
        /// Returns whether a view transition is currently in progress.
        /// </summary>
        /// <returns><see langword="true" /> if a transition is active; otherwise <see langword="false" />.</returns>
        public bool IsTransitionActive()
        {
            return transitionTime != -1f;
        }

        /// <summary>
        /// Draws the active controller view or the current transition frame.
        /// </summary>
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

        /// <summary>
        /// Applies any renderer transforms required for landscape orientation.
        /// Retained as a compatibility hook.
        /// </summary>
        private static void ApplyLandscape()
        {
        }

        /// <summary>
        /// Sets the <paramref name="transition"/> effect used when switching views.
        /// </summary>
        /// <param name="transition">Transition type constant to use.</param>
        public virtual void SetViewTransition(int transition)
        {
            viewTransition = transition;
        }

        /// <summary>
        /// Draws the current transition frame using the captured previous and next view images.
        /// </summary>
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

        /// <inheritdoc />
        public override void Activate()
        {
            base.Activate();
        }

        /// <summary>
        /// Called when a <paramref name="controller"/> becomes active and should become the current routed controller.
        /// </summary>
        /// <param name="controller">Controller that was activated.</param>
        public virtual void OnControllerActivated(ViewController controller)
        {
            SetCurrentController(controller);
        }

        /// <summary>
        /// Called when a <paramref name="controller"/> has deactivated and should no longer receive routed input.
        /// </summary>
        /// <param name="controller">Controller that was deactivated.</param>
        public virtual void OnControllerDeactivated(ViewController controller)
        {
            SetCurrentController(null);
        }

        /// <summary>
        /// Called when a <paramref name="controller"/> pauses and should temporarily stop receiving routed input.
        /// </summary>
        /// <param name="controller">Controller that was paused.</param>
        public virtual void OnControllerPaused(ViewController controller)
        {
            SetCurrentController(null);
        }

        /// <summary>
        /// Called when a <paramref name="controller"/> resumes and should once again receive routed input.
        /// </summary>
        /// <param name="controller">Controller that was unpaused.</param>
        public virtual void OnControllerUnpaused(ViewController controller)
        {
            SetCurrentController(controller);
        }

        /// <summary>
        /// Marks the current <paramref name="controller"/> for deferred deactivation on the next tick.
        /// </summary>
        /// <param name="controller">Controller requesting deactivation.</param>
        public virtual void OnControllerDeactivationRequest(ViewController controller)
        {
            deactivateCurrentController = true;
        }

        /// <summary>
        /// Called before a controller <paramref name="view"/> is shown.
        /// Captures the incoming <paramref name="view"/> image when transitions are enabled.
        /// </summary>
        /// <param name="view">View that is about to be shown.</param>
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

        /// <summary>
        /// Called before a controller <paramref name="view"/> is hidden.
        /// Captures the outgoing <paramref name="view"/> image when transitions are enabled.
        /// </summary>
        /// <param name="view">View that is about to be hidden.</param>
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

        /// <summary>
        /// Returns whether the root controller is currently suspended.
        /// </summary>
        /// <returns><see langword="true" /> if suspended; otherwise <see langword="false" />.</returns>
        public virtual bool IsSuspended()
        {
            return suspended;
        }

        /// <summary>
        /// Suspends input routing and other root-controller activity.
        /// </summary>
        public virtual void Suspend()
        {
            suspended = true;
        }

        /// <summary>
        /// Resumes input routing and other root-controller activity.
        /// </summary>
        public virtual void Resume()
        {
            suspended = false;
        }

        /// <inheritdoc />
        public override bool MouseMoved(float x, float y)
        {
            return currentController.MouseMoved(x, y);
        }

        /// <inheritdoc />
        public override bool HandleMouseWheel(int scrollDelta)
        {
            return currentController != null && !suspended && transitionTime == -1f && currentController.HandleMouseWheel(scrollDelta);
        }

        /// <inheritdoc />
        public override bool BackButtonPressed()
        {
            return suspended || transitionTime != -1f || currentController.BackButtonPressed();
        }

        /// <inheritdoc />
        public override bool MenuButtonPressed()
        {
            return suspended || transitionTime != -1f || currentController.MenuButtonPressed();
        }

        /// <inheritdoc />
        public override bool TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.TouchesBeganwithEvent(touches));
        }

        /// <inheritdoc />
        public override bool TouchesMovedwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.TouchesMovedwithEvent(touches));
        }

        /// <inheritdoc />
        public override bool TouchesEndedwithEvent(IList<TouchLocation> touches)
        {
            return !suspended && (transitionTime != -1f || currentController.TouchesEndedwithEvent(touches));
        }

        /// <inheritdoc />
        public override bool TouchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            return currentController.TouchesCancelledwithEvent(touches);
        }

        /// <summary>
        /// Sets the <paramref name="controller"/> that currently receives routed updates and input.
        /// </summary>
        /// <param name="controller">Controller to make current, or <see langword="null" /> to clear routing.</param>
        public virtual void SetCurrentController(ViewController controller)
        {
            currentController = controller;
        }

        /// <summary>
        /// Returns the controller that currently receives routed updates and input.
        /// </summary>
        /// <returns>Current controller, or <see langword="null" /> if none is active.</returns>
        public virtual ViewController GetCurrentController()
        {
            return currentController;
        }

        /// <inheritdoc />
        public override void FullscreenToggled(bool isFullscreen)
        {
            currentController?.FullscreenToggled(isFullscreen);
        }

        /// <summary>
        /// Horizontal slide transition entering from the right.
        /// </summary>
        public const int TRANSITION_SLIDE_HORIZONTAL_RIGHT = 0;

        /// <summary>
        /// Horizontal slide transition entering from the left.
        /// </summary>
        public const int TRANSITION_SLIDE_HORIZONTAL_LEFT = 1;

        /// <summary>
        /// Vertical slide transition moving upward.
        /// </summary>
        public const int TRANSITION_SLIDE_VERTICAL_UP = 2;

        /// <summary>
        /// Vertical slide transition moving downward.
        /// </summary>
        public const int TRANSITION_SLIDE_VERTICAL_DON = 3;

        /// <summary>
        /// Fade-out transition using a black overlay.
        /// </summary>
        public const int TRANSITION_FADE_OUT_BLACK = 4;

        /// <summary>
        /// Fade-out transition using a white overlay.
        /// </summary>
        public const int TRANSITION_FADE_OUT_WHITE = 5;

        /// <summary>
        /// Reveal transition identifier.
        /// </summary>
        public const int TRANSITION_REVEAL = 6;

        /// <summary>
        /// Total number of transition identifiers.
        /// </summary>
        public const int TRANSITIONS_COUNT = 7;

        /// <summary>
        /// Default transition duration in seconds.
        /// </summary>
        public const float TRANSITION_DEFAULT_DELAY = 0.4f;

        /// <summary>
        /// Currently selected transition type, or <c>-1</c> when transitions are disabled.
        /// </summary>
        public int viewTransition = -1;

        /// <summary>
        /// Absolute time when the current transition should finish, or <c>-1</c> when inactive.
        /// </summary>
        public float transitionTime = -1f;

        /// <summary>
        /// Duration of each transition in seconds.
        /// </summary>
        private readonly float transitionDelay = 0.4f;

        /// <summary>
        /// Last view hidden by the root controller, used for transition capture.
        /// </summary>
        private View previousView;

        /// <summary>
        /// Captured image of the previous view during transitions.
        /// </summary>
        private CTRTexture2D prevScreenImage;

        /// <summary>
        /// Captured image of the next view during transitions.
        /// </summary>
        private CTRTexture2D nextScreenImage;

        // private readonly Grabber screenGrabber = new();

        /// <summary>
        /// Whether the current controller should be deactivated on the next tick.
        /// </summary>
        private bool deactivateCurrentController;

        /// <summary>
        /// Controller currently receiving routed updates and input.
        /// </summary>
        private ViewController currentController;

        /// <summary>
        /// Accumulated root-controller time in seconds.
        /// </summary>
        private float lastTime;

        /// <summary>
        /// Whether the root controller is suspended.
        /// </summary>
        public bool suspended;
    }
}
