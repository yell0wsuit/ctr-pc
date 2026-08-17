using System;
using System.Collections.Generic;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Base controller that manages views, child controllers, and input forwarding.
    /// </summary>
    internal class ViewController : FrameworkTypes, ITouchDelegate
    {
        /// <summary>
        /// The coordinate box this controller's content is authored in, recomputed from the
        /// published viewport on every read. Its width is always the design width; its height is a
        /// function of the viewport's aspect ratio.
        /// </summary>
        /// <remarks>
        /// <para>
        /// At or below the design aspect ratio the box keeps the authored height, so the
        /// composition stays the size the viewport's shorter side already gives it and a squarer
        /// or taller window simply reveals more background around it. Above it the box shortens,
        /// which raises the fit scale and zooms the composition in, so a wide screen shows the
        /// menu larger rather than merely spreading it further apart. That upper rule is the shape
        /// of Famobi's height curve, at its rate, anchored to the authored size.
        /// </para>
        /// <para>
        /// Both branches return exactly the design size at the design aspect ratio, which is what
        /// leaves the shipped layout untouched at the ratio the game ships at.
        /// </para>
        /// <para>
        /// Computed on read rather than assigned during a layout pass: an assigned box would be a
        /// cached derivation of the snapshot and could disagree with it. A controller whose content
        /// needs a fixed shape overrides this getter; it never writes one.
        /// </para>
        /// </remarks>
        protected virtual CTRRectangle DesignBox
        {
            get
            {
                float aspect = ScreenPresentation.Instance.Snapshot.Aspect;
                float height = aspect <= DesignAspect
                    ? ViewportLayout.DesignHeight
                    : LayoutMath.Remap(
                        MathF.Min(aspect, ViewportLayout.MaxAspect),
                        DesignAspect,
                        ViewportLayout.MaxAspect,
                        ViewportLayout.DesignHeight,
                        WidestDesignHeight);
                return new CTRRectangle(0f, 0f, ViewportLayout.DesignWidth, height);
            }
        }

        /// <summary>
        /// Aspect ratio of the fixed design size, where the two branches of <see cref="DesignBox"/>
        /// meet and the box is exactly the size the game's content is authored at.
        /// </summary>
        private const float DesignAspect =
            ViewportLayout.DesignWidth / ViewportLayout.DesignHeight;

        /// <summary>
        /// Design-box height at the widest supported aspect ratio.
        /// </summary>
        /// <remarks>
        /// Famobi shortens its box by roughly 28% per unit of aspect ratio, which from the design
        /// size would reach about 1150 here. This stops short of that: the menus are composed
        /// across nearly the whole authored height, and a box shorter than about 1210 brings the
        /// bottom-anchored content up far enough to meet what hangs from the top. Famobi has the
        /// headroom for its full rate because it shrinks its logo on the same curve.
        /// </remarks>
        private const float WidestDesignHeight = 1250f;

        /// <summary>
        /// Where <see cref="DesignBox"/> lands in logical space at the current viewport. Derived
        /// on read from the published viewport rather than cached, so it is correct the instant
        /// the viewport changes and there is no second copy to keep in step.
        /// </summary>
        /// <remarks>
        /// Centred, and free to be wider than the viewport. Containing the box instead would size
        /// the composition from the design width, and since logical space already normalizes the
        /// viewport's shorter side, that shrinks content on any viewport narrower than the design
        /// shape - a menu whose content column occupies the middle third of its box would be
        /// scaled down for the sake of two empty margins. What overflows is margin; the background
        /// covers it separately.
        /// </remarks>
        protected CTRRectangle FittedBox
        {
            get
            {
                CTRRectangle visible = ScreenPresentation.Instance.Snapshot.VisibleBounds;
                CTRRectangle design = DesignBox;
                float scale = FittedScale;
                float width = design.w * scale;
                float height = design.h * scale;
                return new CTRRectangle(
                    (visible.w - width) / 2f,
                    (visible.h - height) / 2f,
                    width,
                    height);
            }
        }

        /// <summary>
        /// Uniform scale from design-box coordinates to logical space.
        /// </summary>
        /// <remarks>
        /// Logical space always exposes <see cref="ViewportLayout.LogicalShortSide"/> units across
        /// its shorter side, so a box of the authored height sits at exactly one and the scale is
        /// entirely the box's own shrinkage. That is what makes a wide viewport zoom in - the box
        /// gets shorter, so the same content is drawn larger - while every other shape leaves the
        /// composition at the size the short side already gives it.
        /// </remarks>
        protected float FittedScale => ViewportLayout.LogicalShortSide / DesignBox.h;

        /// <summary>
        /// Sizes, scales and positions the element that carries this controller's design-space
        /// content, so everything under it is drawn as though the design box were the screen.
        /// </summary>
        /// <remarks>
        /// The position is not simply the fitted box's corner. <see cref="BaseElement"/> scales
        /// about its own centre, so a group placed at that corner would drift by half the box
        /// times the shortfall in scale. Taking that back out here is what makes a child authored
        /// at <c>x</c> land at <c>FittedBox.x + x * FittedScale</c>, which is the placement rule
        /// every scene's constants assume.
        /// </remarks>
        /// <param name="group">Element holding the design-space content.</param>
        protected void PlaceFittedGroup(BaseElement group)
        {
            if (group == null)
            {
                return;
            }

            CTRRectangle design = DesignBox;
            CTRRectangle fitted = FittedBox;
            float scale = fitted.w / design.w;

            group.width = (int)design.w;
            group.height = (int)design.h;
            group.scaleX = group.scaleY = scale;

            // The centre taken back out here is the one the renderer will scale about, integer
            // shift and all. Using the exact box height instead would leave the content off by
            // half a unit times the shortfall in scale, on any box whose height is odd.
            group.x = fitted.x - ((group.width >> 1) * (1f - scale));
            group.y = fitted.y - ((group.height >> 1) * (1f - scale));
        }

        /// <summary>
        /// Converts a pointer position in logical space into this controller's design space, so a
        /// hit test against unscaled element rectangles lands where the element was drawn.
        /// </summary>
        /// <param name="logicalX">Pointer X in logical space.</param>
        /// <param name="logicalY">Pointer Y in logical space.</param>
        /// <returns>The pointer position in design space.</returns>
        protected Vector PointerToDesignSpace(float logicalX, float logicalY)
        {
            CTRRectangle fitted = FittedBox;
            float scale = fitted.w / DesignBox.w;
            return Vect((logicalX - fitted.x) / scale, (logicalY - fitted.y) / scale);
        }

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
        /// Positions this controller's content for the given viewport. Called when the viewport
        /// changes and when this controller becomes active, never on an ordinary frame.
        /// </summary>
        /// <param name="snapshot">The viewport to lay out against.</param>
        protected virtual void Relayout(ViewportLayoutSnapshot snapshot)
        {
        }

        /// <summary>
        /// Lays out this controller and every active descendant. Inactive children are skipped;
        /// they lay out when they are activated.
        /// </summary>
        /// <param name="snapshot">The viewport to lay out against.</param>
        public void RelayoutTree(ViewportLayoutSnapshot snapshot)
        {
            Relayout(snapshot);
            if (activeChildID != -1)
            {
                GetChild(activeChildID)?.RelayoutTree(snapshot);
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
            Relayout(ScreenPresentation.Instance.Snapshot);
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
        /// Returns the child controller registered under the specified identifier,
        /// or <see langword="null"/> when no child is registered under it.
        /// </summary>
        /// <param name="n">Child identifier.</param>
        /// <returns>Registered child controller, or <see langword="null"/>.</returns>
        public virtual ViewController GetChild(int n)
        {
            return childs.TryGetValue(n, out ViewController child) ? child : null;
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
