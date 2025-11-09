using CutTheRope.ctr_commons;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.core
{
    internal class ViewController : NSObject, TouchDelegate
    {
        public ViewController()
        {
            this.views = new Dictionary<int, View>();
        }

        public virtual NSObject initWithParent(ViewController p)
        {
            if (base.init() != null)
            {
                this.controllerState = ViewController.ControllerState.CONTROLLER_DEACTIVE;
                this.views = new Dictionary<int, View>();
                this.childs = new Dictionary<int, ViewController>();
                this.activeViewID = -1;
                this.activeChildID = -1;
                this.pausedViewID = -1;
                this.parent = p;
            }
            return this;
        }

        public virtual void activate()
        {
            this.controllerState = ViewController.ControllerState.CONTROLLER_ACTIVE;
            Application.sharedRootController().onControllerActivated(this);
        }

        public virtual void deactivate()
        {
            Application.sharedRootController().onControllerDeactivationRequest(this);
        }

        public virtual void deactivateImmediately()
        {
            this.controllerState = ViewController.ControllerState.CONTROLLER_DEACTIVE;
            if (this.activeViewID != -1)
            {
                this.hideActiveView();
            }
            Application.sharedRootController().onControllerDeactivated(this);
            this.parent.onChildDeactivated(this.parent.activeChildID);
        }

        public virtual void pause()
        {
            this.controllerState = ViewController.ControllerState.CONTROLLER_PAUSED;
            Application.sharedRootController().onControllerPaused(this);
            if (this.activeViewID != -1)
            {
                this.pausedViewID = this.activeViewID;
                this.hideActiveView();
            }
        }

        public virtual void unpause()
        {
            this.controllerState = ViewController.ControllerState.CONTROLLER_ACTIVE;
            if (this.activeChildID != -1)
            {
                this.activeChildID = -1;
            }
            Application.sharedRootController().onControllerUnpaused(this);
            if (this.pausedViewID != -1)
            {
                this.showView(this.pausedViewID);
            }
        }

        public virtual void update(float delta)
        {
            if (this.activeViewID != -1)
            {
                this.activeView().update(delta);
            }
        }

        public virtual void addViewwithID(View v, int n)
        {
            View value;
            this.views.TryGetValue(n, out value);
            this.views[n] = v;
        }

        public virtual void deleteView(int n)
        {
            this.views[n] = null;
        }

        public virtual void hideActiveView()
        {
            View view = this.views[this.activeViewID];
            Application.sharedRootController().onControllerViewHide(view);
            if (view != null)
            {
                view.onTouchUpXY(-10000f, -10000f);
                view.hide();
            }
            this.activeViewID = -1;
        }

        public virtual void showView(int n)
        {
            if (this.activeViewID != -1)
            {
                this.hideActiveView();
            }
            this.activeViewID = n;
            View view = this.views[n];
            Application.sharedRootController().onControllerViewShow(view);
            view.show();
        }

        public virtual View activeView()
        {
            return this.views[this.activeViewID];
        }

        public virtual View getView(int n)
        {
            View value = null;
            this.views.TryGetValue(n, out value);
            return value;
        }

        public virtual void addChildwithID(ViewController c, int n)
        {
            ViewController viewController = null;
            viewController?.dealloc();
            this.childs[n] = c;
        }

        public virtual void deleteChild(int n)
        {
            ViewController value = null;
            if (this.childs.TryGetValue(n, out value))
            {
                value.dealloc();
                this.childs[n] = null;
            }
        }

        public virtual void deactivateActiveChild()
        {
            this.childs[this.activeChildID].deactivate();
            this.activeChildID = -1;
        }

        public virtual void activateChild(int n)
        {
            if (this.activeChildID != -1)
            {
                this.deactivateActiveChild();
            }
            this.pause();
            this.activeChildID = n;
            this.childs[n].activate();
        }

        public virtual void onChildDeactivated(int n)
        {
            this.unpause();
        }

        public virtual ViewController activeChild()
        {
            return this.childs[this.activeChildID];
        }

        public virtual ViewController getChild(int n)
        {
            return this.childs[n];
        }

        private bool checkNoChildsActive()
        {
            foreach (KeyValuePair<int, ViewController> child in this.childs)
            {
                ViewController value = child.Value;
                if (value != null && value.controllerState != ViewController.ControllerState.CONTROLLER_DEACTIVE)
                {
                    return false;
                }
            }
            return true;
        }

        public Vector convertTouchForLandscape(Vector t)
        {
            throw new NotImplementedException();
        }

        public virtual bool touchesBeganwithEvent(IList<TouchLocation> touches)
        {
            if (this.activeViewID == -1)
            {
                return false;
            }
            View view = this.activeView();
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
                    return view.onTouchDownXY(CtrRenderer.transformX(touchLocation.Position.X), CtrRenderer.transformY(touchLocation.Position.Y));
                }
            }
            return false;
        }

        public void deactivateAllButtons()
        {
            if (this.activeViewID != -1)
            {
                View view = this.views[this.activeViewID];
                if (view != null)
                {
                    view.onTouchUpXY(-1f, -1f);
                    return;
                }
            }
            else if (this.childs != null)
            {
                ViewController value;
                this.childs.TryGetValue(this.activeChildID, out value);
                value?.deactivateAllButtons();
            }
        }

        public virtual bool touchesEndedwithEvent(IList<TouchLocation> touches)
        {
            if (this.activeViewID == -1)
            {
                return false;
            }
            View view = this.activeView();
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
                    return view.onTouchUpXY(CtrRenderer.transformX(touchLocation.Position.X), CtrRenderer.transformY(touchLocation.Position.Y));
                }
            }
            return false;
        }

        public virtual bool touchesMovedwithEvent(IList<TouchLocation> touches)
        {
            if (this.activeViewID == -1)
            {
                return false;
            }
            View view = this.activeView();
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
                    return view.onTouchMoveXY(CtrRenderer.transformX(touchLocation.Position.X), CtrRenderer.transformY(touchLocation.Position.Y));
                }
            }
            return false;
        }

        public virtual bool touchesCancelledwithEvent(IList<TouchLocation> touches)
        {
            foreach (TouchLocation touch in touches)
            {
                TouchLocationState state = touch.State;
            }
            return false;
        }

        public override void dealloc()
        {
            this.views.Clear();
            this.views = null;
            this.childs.Clear();
            this.childs = null;
            base.dealloc();
        }

        public virtual bool backButtonPressed()
        {
            return false;
        }

        public virtual bool menuButtonPressed()
        {
            return false;
        }

        public virtual bool mouseMoved(float x, float y)
        {
            return false;
        }

        public virtual void fullscreenToggled(bool isFullscreen)
        {
        }

        public const int FAKE_TOUCH_UP_TO_DEACTIVATE_BUTTONS = -10000;

        public ViewController.ControllerState controllerState;

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
