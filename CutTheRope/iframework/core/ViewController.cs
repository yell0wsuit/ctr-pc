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
            views = new Dictionary<int, View>();
        }

        public virtual NSObject initWithParent(ViewController p)
        {
            if (base.init() != null)
            {
                controllerState = ControllerState.CONTROLLER_DEACTIVE;
                views = new Dictionary<int, View>();
                childs = new Dictionary<int, ViewController>();
                activeViewID = -1;
                activeChildID = -1;
                pausedViewID = -1;
                parent = p;
            }
            return this;
        }

        public virtual void activate()
        {
            controllerState = ControllerState.CONTROLLER_ACTIVE;
            Application.sharedRootController().onControllerActivated(this);
        }

        public virtual void deactivate()
        {
            Application.sharedRootController().onControllerDeactivationRequest(this);
        }

        public virtual void deactivateImmediately()
        {
            controllerState = ControllerState.CONTROLLER_DEACTIVE;
            if (activeViewID != -1)
            {
                hideActiveView();
            }
            Application.sharedRootController().onControllerDeactivated(this);
            parent.onChildDeactivated(parent.activeChildID);
        }

        public virtual void pause()
        {
            controllerState = ControllerState.CONTROLLER_PAUSED;
            Application.sharedRootController().onControllerPaused(this);
            if (activeViewID != -1)
            {
                pausedViewID = activeViewID;
                hideActiveView();
            }
        }

        public virtual void unpause()
        {
            controllerState = ControllerState.CONTROLLER_ACTIVE;
            if (activeChildID != -1)
            {
                activeChildID = -1;
            }
            Application.sharedRootController().onControllerUnpaused(this);
            if (pausedViewID != -1)
            {
                showView(pausedViewID);
            }
        }

        public virtual void update(float delta)
        {
            if (activeViewID != -1)
            {
                activeView().update(delta);
            }
        }

        public virtual void addViewwithID(View v, int n)
        {
            View value;
            views.TryGetValue(n, out value);
            views[n] = v;
        }

        public virtual void deleteView(int n)
        {
            views[n] = null;
        }

        public virtual void hideActiveView()
        {
            View view = views[activeViewID];
            Application.sharedRootController().onControllerViewHide(view);
            if (view != null)
            {
                view.onTouchUpXY(-10000f, -10000f);
                view.hide();
            }
            activeViewID = -1;
        }

        public virtual void showView(int n)
        {
            if (activeViewID != -1)
            {
                hideActiveView();
            }
            activeViewID = n;
            View view = views[n];
            Application.sharedRootController().onControllerViewShow(view);
            view.show();
        }

        public virtual View activeView()
        {
            return views[activeViewID];
        }

        public virtual View getView(int n)
        {
            View value = null;
            views.TryGetValue(n, out value);
            return value;
        }

        public virtual void addChildwithID(ViewController c, int n)
        {
            ViewController viewController = null;
            viewController?.dealloc();
            childs[n] = c;
        }

        public virtual void deleteChild(int n)
        {
            ViewController value = null;
            if (childs.TryGetValue(n, out value))
            {
                value.dealloc();
                childs[n] = null;
            }
        }

        public virtual void deactivateActiveChild()
        {
            childs[activeChildID].deactivate();
            activeChildID = -1;
        }

        public virtual void activateChild(int n)
        {
            if (activeChildID != -1)
            {
                deactivateActiveChild();
            }
            pause();
            activeChildID = n;
            childs[n].activate();
        }

        public virtual void onChildDeactivated(int n)
        {
            unpause();
        }

        public virtual ViewController activeChild()
        {
            return childs[activeChildID];
        }

        public virtual ViewController getChild(int n)
        {
            return childs[n];
        }

        private bool checkNoChildsActive()
        {
            foreach (KeyValuePair<int, ViewController> child in childs)
            {
                ViewController value = child.Value;
                if (value != null && value.controllerState != ControllerState.CONTROLLER_DEACTIVE)
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
            if (activeViewID == -1)
            {
                return false;
            }
            View view = activeView();
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
            if (activeViewID != -1)
            {
                View view = views[activeViewID];
                if (view != null)
                {
                    view.onTouchUpXY(-1f, -1f);
                    return;
                }
            }
            else if (childs != null)
            {
                ViewController value;
                childs.TryGetValue(activeChildID, out value);
                value?.deactivateAllButtons();
            }
        }

        public virtual bool touchesEndedwithEvent(IList<TouchLocation> touches)
        {
            if (activeViewID == -1)
            {
                return false;
            }
            View view = activeView();
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
            if (activeViewID == -1)
            {
                return false;
            }
            View view = activeView();
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
            views.Clear();
            views = null;
            childs.Clear();
            childs = null;
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
