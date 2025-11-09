using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;

namespace CutTheRope.game
{
    internal class LoadingController : ViewController, ResourceMgrDelegate
    {
        public override NSObject initWithParent(ViewController p)
        {
            if (base.initWithParent(p) != null)
            {
                LoadingView loadingView = (LoadingView)new LoadingView().initFullscreen();
                this.addViewwithID(loadingView, 0);
                Text text = new Text().initWithFont(Application.getFont(3));
                text.setAlignment(2);
                text.setStringandWidth(Application.getString(655387), 300f);
                text.anchor = text.parentAnchor = 18;
                loadingView.addChild(text);
            }
            return this;
        }

        public override void activate()
        {
            FrameworkTypes.AndroidAPI.showBanner();
            base.activate();
            ((LoadingView)this.getView(0)).game = this.nextController == 0;
            this.showView(0);
        }

        public virtual void resourceLoaded(int res)
        {
        }

        public virtual void allResourcesLoaded()
        {
            GC.Collect();
            FrameworkTypes.AndroidAPI.hideBanner();
            Application.sharedRootController().setViewTransition(4);
            base.deactivate();
        }

        public int nextController;

        private enum ViewID
        {
            VIEW_LOADING
        }
    }
}
