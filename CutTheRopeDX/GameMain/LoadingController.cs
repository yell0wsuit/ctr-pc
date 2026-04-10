using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Controller for the pack loading screen and transition back to the root controller.
    /// </summary>
    internal sealed class LoadingController : ViewController, IResourceMgrDelegate
    {
        /// <summary>
        /// Initializes a loading controller and its loading label.
        /// </summary>
        /// <param name="parent">Parent view controller.</param>
        public LoadingController(ViewController parent)
            : base(parent)
        {
            LoadingView loadingView = new();
            AddViewwithID(loadingView, 0);
            Text text = new Text().InitWithFont(Application.GetFont(Resources.Fnt.BigFont));
            text.SetAlignment(2);
            text.SetStringandWidth(Application.GetString("LOADING"), 300f);
            text.anchor = text.parentAnchor = 18;
            _ = loadingView.AddChild(text);
        }

        /// <inheritdoc />
        public override void Update(float t)
        {
            base.Update(t);

            // Wait for animation to complete before transitioning
            if (resourcesLoaded)
            {
                LoadingView loadingView = (LoadingView)GetView(0);
                if (loadingView.IsAnimationComplete())
                {
                    Application.SharedRootController().SetViewTransition(4);
                    Deactivate();
                    resourcesLoaded = false; // Reset for next time
                }
            }
        }

        /// <inheritdoc />
        public override void Activate()
        {
            AndroidAPI.ShowBanner();
            base.Activate();
            resourcesLoaded = false; // Reset flag when activating
            ((LoadingView)GetView(0)).game = nextController == 0;
            ShowView(0);
        }

        /// <inheritdoc />
        public override void DeactivateImmediately()
        {
            resourcesLoaded = false; // Clear state
            base.DeactivateImmediately();
        }

        /// <summary>
        /// Marks pending resources as loaded so the controller can transition after the loading animation finishes.
        /// </summary>
        public void AllResourcesLoaded()
        {
            // Just set flag - Update() will handle transition after animation completes
            resourcesLoaded = true;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                resourcesLoaded = false;
                nextController = 0;
            }
            base.Dispose(disposing);
        }

        /// <summary>Controller ID to activate after the loading screen completes.</summary>
        public int nextController;

        /// <summary>Whether the resource manager has finished loading the requested resources.</summary>
        private bool resourcesLoaded;

        /// <summary>
        /// View identifiers owned by the loading controller.
        /// </summary>
        private enum ViewID
        {
            /// <summary>Loading view identifier.</summary>
            VIEW_LOADING
        }
    }
}
