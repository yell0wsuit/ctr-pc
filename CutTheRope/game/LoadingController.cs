using System;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
	// Token: 0x02000082 RID: 130
	internal class LoadingController : ViewController, ResourceMgrDelegate
	{
		// Token: 0x06000554 RID: 1364 RVA: 0x0002A070 File Offset: 0x00028270
		public override NSObject initWithParent(ViewController p)
		{
			if (base.initWithParent(p) != null)
			{
				LoadingView loadingView = (LoadingView)new LoadingView().initFullscreen();
				this.addViewwithID(loadingView, 0);
				Text text = new Text().initWithFont(Application.getFont(3));
				text.setAlignment(2);
				text.setStringandWidth(Application.getString(655387), 300f);
				text.anchor = (text.parentAnchor = 18);
				loadingView.addChild(text);
			}
			return this;
		}

		// Token: 0x06000555 RID: 1365 RVA: 0x0002A0E5 File Offset: 0x000282E5
		public override void activate()
		{
			FrameworkTypes.AndroidAPI.showBanner();
			base.activate();
			((LoadingView)this.getView(0)).game = this.nextController == 0;
			this.showView(0);
		}

		// Token: 0x06000556 RID: 1366 RVA: 0x0002A113 File Offset: 0x00028313
		public virtual void resourceLoaded(int res)
		{
		}

		// Token: 0x06000557 RID: 1367 RVA: 0x0002A115 File Offset: 0x00028315
		public virtual void allResourcesLoaded()
		{
			GC.Collect();
			FrameworkTypes.AndroidAPI.hideBanner();
			Application.sharedRootController().setViewTransition(4);
			base.deactivate();
		}

		// Token: 0x04000449 RID: 1097
		public int nextController;

		// Token: 0x020000C9 RID: 201
		private enum ViewID
		{
			// Token: 0x040008FD RID: 2301
			VIEW_LOADING
		}
	}
}
