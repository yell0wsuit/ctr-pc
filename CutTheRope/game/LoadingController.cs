using System;

using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed class LoadingController : ViewController, IResourceMgrDelegate
    {
        public LoadingController(ViewController parent)
            : base(parent)
        {
            LoadingView loadingView = new();
            AddViewwithID(loadingView, 0);
            Text text = new Text().InitWithFont(Application.GetFont(3));
            text.SetAlignment(2);
            text.SetStringandWidth(Application.GetString(655387), 300f);
            text.anchor = text.parentAnchor = 18;
            _ = loadingView.AddChild(text);
        }

        public override void Activate()
        {
            AndroidAPI.ShowBanner();
            base.Activate();
            ((LoadingView)GetView(0)).game = nextController == 0;
            ShowView(0);
        }

        public void ResourceLoaded(int res)
        {
        }

        public void AllResourcesLoaded()
        {
            GC.Collect();
            AndroidAPI.HideBanner();
            Application.SharedRootController().SetViewTransition(4);
            Deactivate();
        }

        public int nextController;

        private enum ViewID
        {
            VIEW_LOADING
        }
    }
}
