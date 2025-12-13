using System;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class LoadingController : ViewController, IResourceMgrDelegate
    {
        public LoadingController(ViewController parent)
            : base(parent)
        {
            LoadingView loadingView = new();
            AddViewwithID(loadingView, 0);
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetAlignment(2);
            text.SetStringandWidth(Application.GetString(STR_MENU_LOADING), 300f);
            text.anchor = text.parentAnchor = 18;
            _ = loadingView.AddChild(text);
        }

        public override void Update(float t)
        {
            base.Update(t);

            // Wait for animation to complete before transitioning
            if (resourcesLoaded)
            {
                LoadingView loadingView = (LoadingView)GetView(0);
                if (loadingView.IsAnimationComplete())
                {
                    GC.Collect();
                    Application.SharedRootController().SetViewTransition(4);
                    Deactivate();
                    resourcesLoaded = false; // Reset for next time
                }
            }
        }

        public override void Activate()
        {
            AndroidAPI.ShowBanner();
            base.Activate();
            resourcesLoaded = false; // Reset flag when activating
            ((LoadingView)GetView(0)).game = nextController == 0;
            ShowView(0);
        }

        public override void DeactivateImmediately()
        {
            resourcesLoaded = false; // Clear state
            base.DeactivateImmediately();
        }

        public void ResourceLoaded(int res)
        {
        }

        public void AllResourcesLoaded()
        {
            // Just set flag - Update() will handle transition after animation completes
            resourcesLoaded = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                resourcesLoaded = false;
                nextController = 0;
            }
            base.Dispose(disposing);
        }

        public int nextController;
        private bool resourcesLoaded;

        private enum ViewID
        {
            VIEW_LOADING
        }
    }
}
