using CutTheRope.Framework.Core;
using CutTheRope.Framework.Media;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class StartupController : ViewController, IResourceMgrDelegate, IMovieMgrDelegate
    {
        public StartupController(ViewController parent)
            : base(parent)
        {
            View view = new();
            Image image = Image.Image_createWithResID(Resources.Img.ZeptolabNoLink);
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
            ctrresourceMgr.LoadPack(PackMusic);
            ctrresourceMgr.StartLoading();
        }

        public override void Activate()
        {
            base.Activate();
            resourcesLoaded = false; // Reset flag when activating
            ShowView(1);
            MoviePlaybackFinished(null);
        }

        public void ResourceLoaded(int resName)
        {
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
            Resources.Snd.Button,
            Resources.Snd.BubbleBreak,
            Resources.Snd.Bubble,
            Resources.Snd.CandyBreak,
            Resources.Snd.MonsterChewing,
            Resources.Snd.MonsterClose,
            Resources.Snd.MonsterOpen,
            Resources.Snd.MonsterSad,
            Resources.Snd.Ring,
            Resources.Snd.RopeBleak1,
            Resources.Snd.RopeBleak2,
            Resources.Snd.RopeBleak3,
            Resources.Snd.RopeBleak4,
            Resources.Snd.RopeGet,
            Resources.Snd.Star1,
            Resources.Snd.Star2,
            Resources.Snd.Star3,
            Resources.Snd.Electric,
            Resources.Snd.Pump1,
            Resources.Snd.Pump2,
            Resources.Snd.Pump3,
            Resources.Snd.Pump4,
            Resources.Snd.SpiderActivate,
            Resources.Snd.SpiderFall,
            Resources.Snd.SpiderWin,
            Resources.Snd.Wheel,
            Resources.Snd.Win,
            Resources.Snd.GravityOff,
            Resources.Snd.GravityOn,
            Resources.Snd.CandyLink,
            Resources.Snd.Bouncer,
            Resources.Snd.SpikeRotateIn,
            Resources.Snd.SpikeRotateOut,
            Resources.Snd.Buzz,
            Resources.Snd.Teleport,
            Resources.Snd.ScratchIn,
            Resources.Snd.ScratchOut,
            Resources.Snd.GhostPuff,
            null,
        ];

        private static readonly string[] PackCommonImages =
        [
            Resources.Img.MenuButtonDefault,
            Resources.Fnt.BigFont,
            Resources.Fnt.SmallFont,
            Resources.Img.MenuLoading,
            Resources.Img.MenuNotification,
            Resources.Img.MenuAchievement,
            Resources.Img.MenuOptions,
            null
        ];

        private static readonly string[] PackMenu =
        [
            Resources.Img.MenuBgr,
            Resources.Img.MenuPopup,
            Resources.Img.MenuLogo,
            Resources.Img.MenuLevelSelection,
            Resources.Img.MenuPackSelection,
            Resources.Img.MenuPackSelection2,
            Resources.Img.MenuExtraButtons,
            Resources.Img.MenuScrollbar,
            Resources.Img.MenuLeaderboard,
            Resources.Img.MenuProcessingHd,
            Resources.Img.MenuScrollbarChangename,
            Resources.Img.MenuButtonAchivCup,
            Resources.Img.MenuBgrShadow,
            null
        ];

        private static readonly string[] PackLocalizationMenu = [Resources.Img.MenuExtraButtonsEn, null];
        private static readonly string[] PackMusic =
        [
            Resources.Snd.MenuMusic,
            Resources.Snd.GameMusic,
            Resources.Snd.GameMusic2,
            Resources.Snd.GameMusic3,
            Resources.Snd.GameMusic4,
            null,
        ];
    }
}
