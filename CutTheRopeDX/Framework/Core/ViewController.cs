using System;
using System.Collections.Generic;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Base controller that manages views, child controllers, and input forwarding.
    /// </summary>
    internal class ViewController : FrameworkTypes, ITouchDelegate
    {
        /// <summary>
        /// Initializes a controller with no parent.
        /// </summary>
        protected ViewController()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a controller with the specified <paramref name="parent"/> controller.
        /// </summary>
        /// <param name="parent">Parent controller that owns this controller as a child.</param>
        protected ViewController(ViewController parent)
        {
            controllerState = ControllerState.CONTROLLER_DEACTIVE;
            views = [];
            childs = [];
            activeViewID = -1;
            activeChildID = -1;
            pausedViewID = -1;
            this.parent = parent;
        }

        /// <summary>
        /// Activates the controller and notifies the root controller.
        /// </summary>
        public virtual void Activate()
        {
            controllerState = ControllerState.CONTROLLER_ACTIVE;
            Application.SharedRootController().OnControllerActivated(this);
        }

        /// <summary>
        /// Requests deactivation through the root controller.
        /// </summary>
        public virtual void Deactivate()
        {
            Application.SharedRootController().OnControllerDeactivationRequest(this);
        }

        /// <summary>
        /// Deactivates the controller immediately, hides the active view, and notifies the parent.
        /// </summary>
        public virtual void DeactivateImmediately()
        {
            controllerState = ControllerState.CONTROLLER_DEACTIVE;
            if (activeViewID != -1)
            {
                HideActiveView();
            }
            Application.SharedRootController().OnControllerDeactivated(this);
            parent.OnChildDeactivated(parent.activeChildID);
        }

        /// <summary>
        /// Pauses the controller and hides the active view until unpaused.
        /// </summary>
        public virtual void Pause()
        {
            controllerState = ControllerState.CONTROLLER_PAUSED;
            Application.SharedRootController().OnControllerPaused(this);
            if (activeViewID != -1)
            {
                pausedViewID = activeViewID;
                HideActiveView();
            }
        }

        /// <summary>
        /// Restores the controller to the active state and re-shows any paused view.
        /// </summary>
        public virtual void Unpause()
        {
            controllerState = ControllerState.CONTROLLER_ACTIVE;
            if (activeChildID != -1)
            {
                activeChildID = -1;
            }
            Application.SharedRootController().OnControllerUnpaused(this);
            if (pausedViewID != -1)
            {
                ShowView(pausedViewID);
            }
        }

        /// <summary>
        /// Updates the active view, if one is currently shown.
        /// </summary>
        /// <param name="delta">Elapsed frame time in seconds.</param>
        public virtual void Update(float delta)
        {
            if (activeViewID != -1)
            {
                ActiveView().Update(delta);
            }
        }

        /// <summary>
        /// Registers a view under the specified identifier.
        /// </summary>
        /// <param name="v">View to register.</param>
        /// <param name="n">View identifier.</param>
        public virtual void AddViewwithID(View v, int n)
        {
            _ = views.TryGetValue(n, out _);
            views[n] = v;
        }

        /// <summary>
        /// Removes the view reference stored under the specified identifier.
        /// </summary>
        /// <param name="n">View identifier.</param>
        public virtual void DeleteView(int n)
        {
            views[n] = null;
        }

        /// <summary>
        /// Hides the currently active view and clears the active view identifier.
        /// </summary>
        public virtual void HideActiveView()
        {
            View view = views[activeViewID];
            Application.SharedRootController().OnControllerViewHide(view);
            if (view != null)
            {
                _ = view.OnTouchUpXY(-10000f, -10000f);
                view.Hide();
            }
            activeViewID = -1;
        }

        /// <summary>
        /// Shows the view with the specified identifier, hiding any currently active view first.
        /// </summary>
        /// <param name="n">View identifier to show.</param>
        public virtual void ShowView(int n)
        {
            if (activeViewID != -1)
            {
                HideActiveView();
            }
            activeViewID = n;
            View view = views[n];
            Application.SharedRootController().OnControllerViewShow(view);
            view.Show();
        }

        /// <summary>
        /// Returns the currently active view.
        /// </summary>
        /// <returns>Active view instance.</returns>
        public virtual View ActiveView()
        {
            return views[activeViewID];
        }

        /// <summary>
        /// Returns the view registered under the specified identifier.
        /// </summary>
        /// <param name="n">View identifier.</param>
        /// <returns>Registered view, or <see langword="null" /> if not found.</returns>
        public virtual View GetView(int n)
        {
            _ = views.TryGetValue(n, out View value);
            return value;
        }

        /// <summary>
        /// Registers a child controller under the specified identifier.
        /// Replaces and disposes any different existing child at that identifier.
        /// </summary>
        /// <param name="c">Child controller to register.</param>
        /// <param name="n">Child identifier.</param>
        public virtual void AddChildwithID(ViewController c, int n)
        {
            if (childs.TryGetValue(n, out ViewController viewController) && viewController != c)
            {
                viewController?.Dispose();
            }
            childs[n] = c;
        }

        /// <summary>
        /// Disposes and removes the child controller registered under the specified identifier.
        /// </summary>
        /// <param name="n">Child identifier.</param>
        public virtual void DeleteChild(int n)
        {
            if (childs.TryGetValue(n, out ViewController value))
            {
                value?.Dispose();
                childs[n] = null;
            }
        }

        /// <summary>
        /// Requests deactivation of the currently active child controller.
        /// </summary>
        public virtual void DeactivateActiveChild()
        {
            childs[activeChildID].Deactivate();
            activeChildID = -1;
        }

        /// <summary>
        /// Activates the specified child controller after pausing this controller.
        /// </summary>
        /// <param name="n">Child identifier to activate.</param>
        public virtual void ActivateChild(int n)
        {
            if (activeChildID != -1)
            {
                DeactivateActiveChild();
            }
            Pause();
            activeChildID = n;
            childs[n].Activate();
        }

        /// <summary>
        /// Called when a child controller has deactivated.
        /// The default implementation simply unpauses this controller.
        /// </summary>
        /// <param name="n">Identifier of the child that deactivated.</param>
        public virtual void OnChildDeactivated(int n)
        {
            Unpause();
        }

        /// <summary>
        /// Returns the currently active child controller.
        /// </summary>
        /// <returns>Active child controller.</returns>
        public virtual ViewController ActiveChild()
        {
            return childs[activeChildID];
        }

        /// <summary>
        /// Returns the child controller registered under the specified identifier.
        /// </summary>
        /// <param name="n">Child identifier.</param>
        /// <returns>Registered child controller.</returns>
        public virtual ViewController GetChild(int n)
        {
            return childs[n];
        }

        /// <summary>
        /// Converts a touch coordinate for landscape orientation handling.
        /// </summary>
        /// <param name="t">Touch position to convert.</param>
        /// <returns>Converted touch position.</returns>
        /// <exception cref="NotImplementedException">Landscape conversion is not implemented in the base controller.</exception>
        public Vector ConvertTouchForLandscape(Vector t)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Forwards the first pressed touch in the collection to the active view.
        /// </summary>
        /// <param name="touches">Touch collection to inspect.</param>
        /// <returns><see langword="true" /> if the active view handled the touch; otherwise <see langword="false" />.</returns>
        public virtual bool TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            if (activeViewID == -1)
            {
                return false;
            }
            View view = ActiveView();
            int processedTouches = -1;
            for (int i = 0; i < touches.Count; i++)
            {
                processedTouches++;
                if (processedTouches > 1)
                {
                    break;
                }
                TouchLocation touchLocation = touches[i];
                if (touchLocation.State == TouchLocationState.Pressed)
                {
                    return view.OnTouchDownXY(CtrRenderer.TransformX(touchLocation.Position.X), CtrRenderer.TransformY(touchLocation.Position.Y));
                }
            }
            return false;
        }

        /// <summary>
        /// Cancels active button presses on the current view or active child controller.
        /// </summary>
        public void DeactivateAllButtons()
        {
            if (activeViewID != -1)
            {
                View view = views[activeViewID];
                if (view != null)
                {
                    _ = view.OnTouchUpXY(-1f, -1f);
                    return;
                }
            }
            else if (childs != null)
            {
                _ = childs.TryGetValue(activeChildID, out ViewController value);
                value?.DeactivateAllButtons();
            }
        }

        /// <summary>
        /// Forwards the first released touch in the collection to the active view.
        /// </summary>
        /// <param name="touches">Touch collection to inspect.</param>
        /// <returns><see langword="true" /> if the active view handled the touch; otherwise <see langword="false" />.</returns>
        public virtual bool TouchesEndedwithEvent(IList<TouchLocation> touches)
        {
            if (activeViewID == -1)
            {
                return false;
            }
            View view = ActiveView();
            int processedTouches = -1;
            for (int i = 0; i < touches.Count; i++)
            {
                processedTouches++;
                if (processedTouches > 1)
                {
                    break;
                }
                TouchLocation touchLocation = touches[i];
                if (touchLocation.State == TouchLocationState.Released)
                {
                    return view.OnTouchUpXY(CtrRenderer.TransformX(touchLocation.Position.X), CtrRenderer.TransformY(touchLocation.Position.Y));
                }
            }
            return false;
        }

        /// <summary>
        /// Forwards the first moved touch in the collection to the active view.
        /// </summary>
        /// <param name="touches">Touch collection to inspect.</param>
        /// <returns><see langword="true" /> if the active view handled the touch; otherwise <see langword="false" />.</returns>
        public virtual bool TouchesMovedwithEvent(IList<TouchLocation> touches)
        {
            if (activeViewID == -1)
            {
                return false;
            }
            View view = ActiveView();
            int processedTouches = -1;
            for (int i = 0; i < touches.Count; i++)
            {
                processedTouches++;
                if (processedTouches > 1)
                {
                    break;
                }
                TouchLocation touchLocation = touches[i];
                if (touchLocation.State == TouchLocationState.Moved)
                {
                    return view.OnTouchMoveXY(CtrRenderer.TransformX(touchLocation.Position.X), CtrRenderer.TransformY(touchLocation.Position.Y));
                }
            }
            return false;
        }

        /// <summary>
        /// Handles touch-cancel notifications.
        /// </summary>
        /// <param name="touches">Cancelled touches.</param>
        /// <returns>Always <see langword="false" />.</returns>
        /// <remarks>
        /// The base implementation performs no action and returns <see langword="false" />.
        /// </remarks>
        public virtual bool TouchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            foreach (TouchLocation touch in touches)
            {
                _ = touch.State;
            }
            return false;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (views != null)
                {
                    foreach (View view in views.Values)
                    {
                        view?.Dispose();
                    }
                    views.Clear();
                    views = null;
                }
                if (childs != null)
                {
                    foreach (ViewController child in childs.Values)
                    {
                        child?.Dispose();
                    }
                    childs.Clear();
                    childs = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Handles a back-button press.
        /// </summary>
        /// <returns>Always <see langword="false" />.</returns>
        /// <remarks>
        /// Present as a platform-compatibility hook. The base implementation does not handle the input.
        /// </remarks>
        public virtual bool BackButtonPressed()
        {
            return false;
        }

        /// <summary>
        /// Handles a menu-button press.
        /// </summary>
        /// <returns>Always <see langword="false" />.</returns>
        /// <remarks>
        /// Present as a platform-compatibility hook. The base implementation does not handle the input.
        /// </remarks>
        public virtual bool MenuButtonPressed()
        {
            return false;
        }

        /// <summary>
        /// Handles mouse-move input.
        /// </summary>
        /// <param name="x">Mouse X coordinate.</param>
        /// <param name="y">Mouse Y coordinate.</param>
        /// <returns>Always <see langword="false" />.</returns>
        public virtual bool MouseMoved(float x, float y)
        {
            return false;
        }

        /// <summary>
        /// Handles mouse wheel scrolling input for the controller.
        /// </summary>
        /// <param name="scrollDelta">
        /// The mouse wheel scroll delta. Positive values indicate scrolling up (away from user),
        /// negative values indicate scrolling down (toward user).
        /// </param>
        /// <remarks>
        /// Override this method in derived controllers to handle mouse wheel input for scrollable views.
        /// The default implementation returns <see langword="false"/> (no handling).
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the scroll input was handled by this controller or its active view, <see langword="false"/> otherwise.
        /// </returns>
        public virtual bool HandleMouseWheel(int scrollDelta)
        {
            return false;
        }

        /// <summary>
        /// Notifies the controller that fullscreen state changed.
        /// </summary>
        /// <param name="isFullscreen">New fullscreen state.</param>
        /// <remarks>
        /// The base implementation does nothing.
        /// </remarks>
        public virtual void FullscreenToggled(bool isFullscreen)
        {
        }

        /// <summary>
        /// Sentinel Y coordinate used when sending a fake touch-up to clear pressed buttons.
        /// </summary>
        public const int FAKE_TOUCH_UP_TO_DEACTIVATE_BUTTONS = -10000;

        /// <summary>
        /// Current lifecycle state of the controller.
        /// </summary>
        public ControllerState controllerState;

        /// <summary>
        /// Identifier of the currently active view, or <c>-1</c> when none is active.
        /// </summary>
        public int activeViewID;

        /// <summary>
        /// Registered views keyed by identifier.
        /// </summary>
        public Dictionary<int, View> views;

        /// <summary>
        /// Identifier of the currently active child controller, or <c>-1</c> when none is active.
        /// </summary>
        public int activeChildID;

        /// <summary>
        /// Registered child controllers keyed by identifier.
        /// </summary>
        public Dictionary<int, ViewController> childs;

        /// <summary>
        /// Parent controller that owns this controller as a child, if any.
        /// </summary>
        public ViewController parent;

        /// <summary>
        /// Identifier of the view that was active when the controller was paused.
        /// </summary>
        public int pausedViewID;

        /// <summary>
        /// Lifecycle states used by <see cref="ViewController"/>.
        /// </summary>
        public enum ControllerState
        {
            /// <summary>
            /// Controller is inactive.
            /// </summary>
            CONTROLLER_DEACTIVE,

            /// <summary>
            /// Controller is active and processing updates/input.
            /// </summary>
            CONTROLLER_ACTIVE,

            /// <summary>
            /// Controller is paused and its active view is hidden.
            /// </summary>
            CONTROLLER_PAUSED
        }
    }
}
