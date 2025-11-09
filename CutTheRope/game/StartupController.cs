using CutTheRope.iframework.core;
using CutTheRope.iframework.media;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;

namespace CutTheRope.game
{
    internal class StartupController : ViewController, ResourceMgrDelegate, MovieMgrDelegate
    {
        public override NSObject initWithParent(ViewController p)
        {
            if (base.initWithParent(p) != null)
            {
                View view = (View)new View().initFullscreen();
                Image image = Image.Image_createWithResID(0);
                image.parentAnchor = image.anchor = 18;
                image.scaleX = image.scaleY = 1.25f;
                view.addChild(image);
                this.bar = TiledImage.TiledImage_createWithResID(1);
                this.bar.parentAnchor = this.bar.anchor = 9;
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

        public override void update(float t)
        {
            base.update(t);
            float num = (float)Application.sharedResourceMgr().getPercentLoaded();
            this.bar.width = (int)(this.barTotalWidth * num / 100f);
        }

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

        public override void activate()
        {
            base.activate();
            this.showView(1);
            this.moviePlaybackFinished(null);
        }

        public virtual void resourceLoaded(int resName)
        {
        }

        public virtual void allResourcesLoaded()
        {
            Application.sharedRootController().setViewTransition(4);
            this.deactivate();
        }

        private const int VIEW_CHILLINGO_MOVIE = 0;

        private const int VIEW_ZEPTOLAB = 1;

        private float barTotalWidth;

        private TiledImage bar;
    }
}
