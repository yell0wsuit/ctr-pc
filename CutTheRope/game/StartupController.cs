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
                bar = TiledImage.TiledImage_createWithResID(1);
                bar.parentAnchor = bar.anchor = 9;
                bar.setTile(-1);
                bar.x = 738f;
                bar.y = 1056f;
                image.addChild(bar);
                barTotalWidth = bar.width;
                addViewwithID(view, 1);
                view.release();
            }
            return this;
        }

        public override void update(float t)
        {
            base.update(t);
            float num = Application.sharedResourceMgr().getPercentLoaded();
            bar.width = (int)(barTotalWidth * num / 100f);
        }

        public virtual void moviePlaybackFinished(NSString url)
        {
            CTRResourceMgr ctrresourceMgr = Application.sharedResourceMgr();
            ctrresourceMgr.resourcesDelegate = this;
            ctrresourceMgr.initLoading();
            ctrresourceMgr.loadPack(PACK_COMMON);
            ctrresourceMgr.loadPack(PACK_COMMON_IMAGES);
            ctrresourceMgr.loadPack(PACK_MENU);
            ctrresourceMgr.loadPack(PACK_LOCALIZATION_MENU);
            ctrresourceMgr.loadPack(PACK_MUSIC);
            ctrresourceMgr.startLoading();
        }

        public override void activate()
        {
            base.activate();
            showView(1);
            moviePlaybackFinished(null);
        }

        public virtual void resourceLoaded(int resName)
        {
        }

        public virtual void allResourcesLoaded()
        {
            Application.sharedRootController().setViewTransition(4);
            deactivate();
        }

        private const int VIEW_CHILLINGO_MOVIE = 0;

        private const int VIEW_ZEPTOLAB = 1;

        private float barTotalWidth;

        private TiledImage bar;
    }
}
