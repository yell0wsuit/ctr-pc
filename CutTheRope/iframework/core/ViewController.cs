using System;
using System.Collections.Generic;

using CutTheRope.commons;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.iframework.core
{
    internal class ViewController : FrameworkTypes, ITouchDelegate
    {
        protected ViewController()
            : this(null)
        {
        }

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

        public virtual void Activate()
        {
            controllerState = ControllerState.CONTROLLER_ACTIVE;
            Application.SharedRootController().OnControllerActivated(this);
        }

        public virtual void Deactivate()
        {
            Application.SharedRootController().OnControllerDeactivationRequest(this);
        }

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

        public virtual void Update(float delta)
        {
            if (activeViewID != -1)
            {
                ActiveView().Update(delta);
            }
        }

        public virtual void AddViewwithID(View v, int n)
        {
            _ = views.TryGetValue(n, out _);
            views[n] = v;
        }

        public virtual void DeleteView(int n)
        {
            views[n] = null;
        }

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

        public virtual View ActiveView()
        {
            return views[activeViewID];
        }

        public virtual View GetView(int n)
        {
            _ = views.TryGetValue(n, out View value);
            return value;
        }

        public virtual void AddChildwithID(ViewController c, int n)
        {
            if (childs.TryGetValue(n, out ViewController viewController) && viewController != c)
            {
                viewController?.Dispose();
            }
            childs[n] = c;
        }

        public virtual void DeleteChild(int n)
        {
            if (childs.TryGetValue(n, out ViewController value))
            {
                value?.Dispose();
                childs[n] = null;
            }
        }

        public virtual void DeactivateActiveChild()
        {
            childs[activeChildID].Deactivate();
            activeChildID = -1;
        }

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

        public virtual void OnChildDeactivated(int n)
        {
            Unpause();
        }

        public virtual ViewController ActiveChild()
        {
            return childs[activeChildID];
        }

        public virtual ViewController GetChild(int n)
        {
            return childs[n];
        }

        public Vector ConvertTouchForLandscape(Vector t)
        {
            throw new NotImplementedException();
        }

        public virtual bool TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            if (activeViewID == -1)
            {
                return false;
            }
            View view = ActiveView();
            int num = -1;
            for (int i = 0; i < touches.Count; i++)
            {
                num++;
                if (num > 1)
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

        public virtual bool TouchesEndedwithEvent(IList<TouchLocation> touches)
        {
            if (activeViewID == -1)
            {
                return false;
            }
            View view = ActiveView();
            int num = -1;
            for (int i = 0; i < touches.Count; i++)
            {
                num++;
                if (num > 1)
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

        public virtual bool TouchesMovedwithEvent(IList<TouchLocation> touches)
        {
            if (activeViewID == -1)
            {
                return false;
            }
            View view = ActiveView();
            int num = -1;
            for (int i = 0; i < touches.Count; i++)
            {
                num++;
                if (num > 1)
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

        public virtual bool TouchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            foreach (TouchLocation touch in touches)
            {
                _ = touch.State;
            }
            return false;
        }

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

        public virtual bool BackButtonPressed()
        {
            return false;
        }

        public virtual bool MenuButtonPressed()
        {
            return false;
        }

        public virtual bool MouseMoved(float x, float y)
        {
            return false;
        }

        public virtual void FullscreenToggled(bool isFullscreen)
        {
        }

        public const int FAKE_TOUCH_UP_TO_DEACTIVATE_BUTTONS = -10000;

        public ControllerState controllerState;

        public int activeViewID;

        public Dictionary<int, View> views;

        public int activeChildID;

        public Dictionary<int, ViewController> childs;

        public ViewController parent;

        public int pausedViewID;

        public enum ControllerState
        {
            CONTROLLER_DEACTIVE,
            CONTROLLER_ACTIVE,
            CONTROLLER_PAUSED
        }
    }
}
