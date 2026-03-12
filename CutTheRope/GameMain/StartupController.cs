using CutTheRope.Framework.Core;
using CutTheRope.Framework.Media;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed class StartupController : ViewController, IResourceMgrDelegate, IMovieMgrDelegate
    {
        public StartupController(ViewController parent)
            : base(parent)
        {
            View view = new();
            Image image = Image.Image_createWithResID(Resources.BackgroundImg.ZeptolabNoLink);
            image.parentAnchor = image.anchor = 18;
            image.scaleX = image.scaleY = 1.25f;
            _ = view.AddChild(image);
            bar = TiledImage.TiledImage_createWithResID(Resources.Img.LoaderbarFull);
            bar.parentAnchor = bar.anchor = 9;
            bar.SetTile(-1);
            bar.x = 738f;
            bar.y = 1056f;
            _ = image.AddChild(bar);
            barTotalWidth = bar.width;
            AddViewwithID(view, 1);
        }

        public override void Update(float t)
        {
            base.Update(t);
            float targetPercent = Application.SharedResourceMgr().GetPercentLoaded();

            // Smooth interpolation for loading bar
            if (currentPercent < targetPercent)
            {
                currentPercent += (targetPercent - currentPercent) * 0.16f; // Fast smooth lerp
                if (targetPercent - currentPercent < 0.5f)
                {
                    currentPercent = targetPercent; // Snap when close enough
                }
            }

            bar.width = (int)(barTotalWidth * currentPercent / 100f);

            // Wait for animation to complete before transitioning
            if (resourcesLoaded && currentPercent >= 99.5f)
            {
                Application.SharedRootController().SetViewTransition(4);
                Deactivate();
                resourcesLoaded = false; // Reset for next time
            }
        }

        public void MoviePlaybackFinished(string url)
        {
            CTRResourceMgr ctrresourceMgr = Application.SharedResourceMgr();
            ctrresourceMgr.resourcesDelegate = this;
            ctrresourceMgr.InitLoading();
            ctrresourceMgr.LoadPack(PackCommon);
            ctrresourceMgr.LoadPack(PackCommonImages);
            ctrresourceMgr.LoadPack(PackMenu);
            ctrresourceMgr.LoadPack(PackLocalizationMenu);
            ctrresourceMgr.StartLoading();
        }

        public override void Activate()
        {
            base.Activate();
            resourcesLoaded = false; // Reset flag when activating
            ShowView(1);
            UpdateChecker.StartIfNeeded();
            Game1.RPC.Setup();
            MoviePlaybackFinished(null);
        }

        public void AllResourcesLoaded()
        {
            // Just set flag - Update() will handle transition after animation completes
            resourcesLoaded = true;
        }

        private readonly float barTotalWidth;

        private readonly TiledImage bar;

        private float currentPercent;
        private bool resourcesLoaded;

        private static readonly string[] PackCommon =
        [
            Resources.Snd.Tap,
            Resources.Str.MenuStrings,
            Resources.Fnt.BigFont,
            null,
        ];

        private static readonly string[] PackCommonImages =
        [
            Resources.Img.MenuButtonDefault,
            Resources.Img.MenuLoading,
            Resources.Img.MenuOptions,
            null
        ];

        private static readonly string[] PackMenu =
        [
            Resources.Img.MenuBgr,
            Resources.Img.MenuPopup,
            Resources.Img.MenuLogo,
            Resources.Img.CutTheRopeDXLogo,
            Resources.Img.MenuLevelSelection,
            Resources.Img.MenuPackSelection,
            Resources.Img.MenuPackSelection2,
            Resources.Img.MenuExtraButtons,
            Resources.Img.MenuBgrShadow,
            Resources.Img.MenuBgrXmas,
            null
        ];

        private static readonly string[] PackLocalizationMenu = [Resources.Img.MenuExtraButtonsEn, null];
    }
}
