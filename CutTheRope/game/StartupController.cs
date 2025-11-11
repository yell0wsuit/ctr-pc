using CutTheRope.iframework.core;
using CutTheRope.iframework.media;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
    internal sealed class StartupController : ViewController, IResourceMgrDelegate, IMovieMgrDelegate
    {
        public override NSObject InitWithParent(ViewController p)
        {
            if (base.InitWithParent(p) != null)
            {
                View view = (View)new View().InitFullscreen();
                Image image = Image.Image_createWithResID(0);
                image.parentAnchor = image.anchor = 18;
                image.scaleX = image.scaleY = 1.25f;
                _ = view.AddChild(image);
                bar = TiledImage.TiledImage_createWithResID(1);
                bar.parentAnchor = bar.anchor = 9;
                bar.SetTile(-1);
                bar.x = 738f;
                bar.y = 1056f;
                _ = image.AddChild(bar);
                barTotalWidth = bar.width;
                AddViewwithID(view, 1);
                view.Release();
            }
            return this;
        }

        public override void Update(float t)
        {
            base.Update(t);
            float num = Application.SharedResourceMgr().GetPercentLoaded();
            bar.width = (int)(barTotalWidth * num / 100f);
        }

        public void MoviePlaybackFinished(NSString url)
        {
            CTRResourceMgr ctrresourceMgr = Application.SharedResourceMgr();
            ctrresourceMgr.resourcesDelegate = this;
            ctrresourceMgr.InitLoading();
            ctrresourceMgr.LoadPack(PACK_COMMON);
            ctrresourceMgr.LoadPack(PACK_COMMON_IMAGES);
            ctrresourceMgr.LoadPack(PACK_MENU);
            ctrresourceMgr.LoadPack(PACK_LOCALIZATION_MENU);
            ctrresourceMgr.LoadPack(PACK_MUSIC);
            ctrresourceMgr.StartLoading();
        }

        public override void Activate()
        {
            base.Activate();
            ShowView(1);
            MoviePlaybackFinished(null);
        }

        public void ResourceLoaded(int resName)
        {
        }

        public void AllResourcesLoaded()
        {
            Application.SharedRootController().SetViewTransition(4);
            Deactivate();
        }

        private const int VIEW_CHILLINGO_MOVIE = 0;

        private const int VIEW_ZEPTOLAB = 1;

        private float barTotalWidth;

        private TiledImage bar;
    }
}
