using System;
using System.Collections.Generic;
using CutTheRope.ctr_commons;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.iframework.core
{
	// Token: 0x0200006D RID: 109
	internal class ViewController : NSObject, TouchDelegate
	{
		// Token: 0x0600041E RID: 1054 RVA: 0x000162BD File Offset: 0x000144BD
		public ViewController()
		{
			this.views = new Dictionary<int, View>();
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x000162D0 File Offset: 0x000144D0
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

		// Token: 0x06000420 RID: 1056 RVA: 0x0001631F File Offset: 0x0001451F
		public virtual void activate()
		{
			this.controllerState = ViewController.ControllerState.CONTROLLER_ACTIVE;
			Application.sharedRootController().onControllerActivated(this);
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x00016333 File Offset: 0x00014533
		public virtual void deactivate()
		{
			Application.sharedRootController().onControllerDeactivationRequest(this);
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x00016340 File Offset: 0x00014540
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

		// Token: 0x06000423 RID: 1059 RVA: 0x00016379 File Offset: 0x00014579
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

		// Token: 0x06000424 RID: 1060 RVA: 0x000163A8 File Offset: 0x000145A8
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

		// Token: 0x06000425 RID: 1061 RVA: 0x000163E1 File Offset: 0x000145E1
		public virtual void update(float delta)
		{
			if (this.activeViewID != -1)
			{
				this.activeView().update(delta);
			}
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x000163F8 File Offset: 0x000145F8
		public virtual void addViewwithID(View v, int n)
		{
			View value;
			this.views.TryGetValue(n, out value);
			this.views[n] = v;
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x00016421 File Offset: 0x00014621
		public virtual void deleteView(int n)
		{
			this.views[n] = null;
		}

		// Token: 0x06000428 RID: 1064 RVA: 0x00016430 File Offset: 0x00014630
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

		// Token: 0x06000429 RID: 1065 RVA: 0x0001647C File Offset: 0x0001467C
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

		// Token: 0x0600042A RID: 1066 RVA: 0x000164BD File Offset: 0x000146BD
		public virtual View activeView()
		{
			return this.views[this.activeViewID];
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x000164D0 File Offset: 0x000146D0
		public virtual View getView(int n)
		{
			View value = null;
			this.views.TryGetValue(n, out value);
			return value;
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x000164F0 File Offset: 0x000146F0
		public virtual void addChildwithID(ViewController c, int n)
		{
			ViewController viewController = null;
			if (viewController != null)
			{
				viewController.dealloc();
			}
			this.childs[n] = c;
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x00016518 File Offset: 0x00014718
		public virtual void deleteChild(int n)
		{
			ViewController value = null;
			if (this.childs.TryGetValue(n, out value))
			{
				value.dealloc();
				this.childs[n] = null;
			}
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x0001654A File Offset: 0x0001474A
		public virtual void deactivateActiveChild()
		{
			this.childs[this.activeChildID].deactivate();
			this.activeChildID = -1;
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x00016569 File Offset: 0x00014769
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

		// Token: 0x06000430 RID: 1072 RVA: 0x00016598 File Offset: 0x00014798
		public virtual void onChildDeactivated(int n)
		{
			this.unpause();
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x000165A0 File Offset: 0x000147A0
		public virtual ViewController activeChild()
		{
			return this.childs[this.activeChildID];
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x000165B3 File Offset: 0x000147B3
		public virtual ViewController getChild(int n)
		{
			return this.childs[n];
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x000165C4 File Offset: 0x000147C4
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

		// Token: 0x06000434 RID: 1076 RVA: 0x0001662C File Offset: 0x0001482C
		public Vector convertTouchForLandscape(Vector t)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x00016634 File Offset: 0x00014834
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

		// Token: 0x06000436 RID: 1078 RVA: 0x000166AC File Offset: 0x000148AC
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
				if (value != null)
				{
					value.deactivateAllButtons();
				}
			}
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x00016710 File Offset: 0x00014910
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

		// Token: 0x06000438 RID: 1080 RVA: 0x00016788 File Offset: 0x00014988
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

		// Token: 0x06000439 RID: 1081 RVA: 0x00016800 File Offset: 0x00014A00
		public virtual bool touchesCancelledwithEvent(IList<TouchLocation> touches)
		{
			foreach (TouchLocation touch in touches)
			{
				TouchLocationState state = touch.State;
			}
			return false;
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x0001684C File Offset: 0x00014A4C
		public override void dealloc()
		{
			this.views.Clear();
			this.views = null;
			this.childs.Clear();
			this.childs = null;
			base.dealloc();
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00016878 File Offset: 0x00014A78
		public virtual bool backButtonPressed()
		{
			return false;
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x0001687B File Offset: 0x00014A7B
		public virtual bool menuButtonPressed()
		{
			return false;
		}

		// Token: 0x0600043D RID: 1085 RVA: 0x0001687E File Offset: 0x00014A7E
		public virtual bool mouseMoved(float x, float y)
		{
			return false;
		}

		// Token: 0x0600043E RID: 1086 RVA: 0x00016881 File Offset: 0x00014A81
		public virtual void fullscreenToggled(bool isFullscreen)
		{
		}

		// Token: 0x040002C7 RID: 711
		public const int FAKE_TOUCH_UP_TO_DEACTIVATE_BUTTONS = -10000;

		// Token: 0x040002C8 RID: 712
		public ViewController.ControllerState controllerState;

		// Token: 0x040002C9 RID: 713
		public int activeViewID;

		// Token: 0x040002CA RID: 714
		public Dictionary<int, View> views;

		// Token: 0x040002CB RID: 715
		public int activeChildID;

		// Token: 0x040002CC RID: 716
		public Dictionary<int, ViewController> childs;

		// Token: 0x040002CD RID: 717
		public ViewController parent;

		// Token: 0x040002CE RID: 718
		public int pausedViewID;

		// Token: 0x020000BF RID: 191
		public enum ControllerState
		{
			// Token: 0x040008E3 RID: 2275
			CONTROLLER_DEACTIVE,
			// Token: 0x040008E4 RID: 2276
			CONTROLLER_ACTIVE,
			// Token: 0x040008E5 RID: 2277
			CONTROLLER_PAUSED
		}
	}
}
