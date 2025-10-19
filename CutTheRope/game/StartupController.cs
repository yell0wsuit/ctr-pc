using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.media;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
	// Token: 0x02000098 RID: 152
	internal class StartupController : ViewController, ResourceMgrDelegate, MovieMgrDelegate
	{
		// Token: 0x06000607 RID: 1543 RVA: 0x000327E8 File Offset: 0x000309E8
		public override NSObject initWithParent(ViewController p)
		{
			if (base.initWithParent(p) != null)
			{
				View view = (View)new View().initFullscreen();
				Image image = Image.Image_createWithResID(0);
				image.parentAnchor = (image.anchor = 18);
				image.scaleX = (image.scaleY = 1.25f);
				view.addChild(image);
				this.bar = TiledImage.TiledImage_createWithResID(1);
				this.bar.parentAnchor = (this.bar.anchor = 9);
				this.bar.setTile(-1);
				this.bar.x = 738f;
				this.bar.y = 1056f;
				image.addChild(this.bar);
				this.barTotalWidth = (float)this.bar.width;
				this.addViewwithID(view, 1);
				view.release();
			}
			return this;
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x000328C8 File Offset: 0x00030AC8
		public override void update(float t)
		{
			base.update(t);
			float num = (float)Application.sharedResourceMgr().getPercentLoaded();
			this.bar.width = (int)(this.barTotalWidth * num / 100f);
		}

		// Token: 0x06000609 RID: 1545 RVA: 0x00032904 File Offset: 0x00030B04
		public virtual void moviePlaybackFinished(NSString url)
		{
			CTRResourceMgr ctrresourceMgr = Application.sharedResourceMgr();
			ctrresourceMgr.resourcesDelegate = this;
			ctrresourceMgr.initLoading();
			ctrresourceMgr.loadPack(ResDataPhoneFull.PACK_COMMON);
			ctrresourceMgr.loadPack(ResDataPhoneFull.PACK_COMMON_IMAGES);
			ctrresourceMgr.loadPack(ResDataPhoneFull.PACK_MENU);
			ctrresourceMgr.loadPack(ResDataPhoneFull.PACK_LOCALIZATION_MENU);
			ctrresourceMgr.loadPack(ResDataPhoneFull.PACK_MUSIC);
			ctrresourceMgr.startLoading();
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x0003295F File Offset: 0x00030B5F
		public override void activate()
		{
			base.activate();
			this.showView(1);
			this.moviePlaybackFinished(null);
		}

		// Token: 0x0600060B RID: 1547 RVA: 0x00032975 File Offset: 0x00030B75
		public virtual void resourceLoaded(int resName)
		{
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x00032977 File Offset: 0x00030B77
		public virtual void allResourcesLoaded()
		{
			Application.sharedRootController().setViewTransition(4);
			this.deactivate();
		}

		// Token: 0x0400083A RID: 2106
		private const int VIEW_CHILLINGO_MOVIE = 0;

		// Token: 0x0400083B RID: 2107
		private const int VIEW_ZEPTOLAB = 1;

		// Token: 0x0400083C RID: 2108
		private float barTotalWidth;

		// Token: 0x0400083D RID: 2109
		private TiledImage bar;
	}
}
