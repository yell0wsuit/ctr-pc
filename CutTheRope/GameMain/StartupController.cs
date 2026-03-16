using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Media;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    internal sealed class StartupController : ViewController, IResourceMgrDelegate, IMovieMgrDelegate, ITimelineDelegate
    {
        private enum Phase { Loading, Animating }

        public StartupController(ViewController parent)
            : base(parent)
        {
            AddViewwithID(new StartupView(this), 1);
        }

        public override void Update(float t)
        {
            base.Update(t);

            if (currentPhase == Phase.Loading)
            {
                float targetPercent = Application.SharedResourceMgr().GetPercentLoaded();

                // Smooth interpolation for loading bar
                if (currentPercent < targetPercent)
                {
                    currentPercent += (targetPercent - currentPercent) * 0.16f;
                    if (targetPercent - currentPercent < 0.5f)
                    {
                        currentPercent = targetPercent;
                    }
                }

                if (resourcesLoaded && currentPercent >= 99.5f)
                {
                    StartSplashAnimation();
                }
            }
            else if (currentPhase == Phase.Animating)
            {
                animRoot?.Update(t);
                if (animFinished)
                {
                    Application.SharedRootController().SetViewTransition(4);
                    Deactivate();
                    animFinished = false;
                }
            }
        }

        private void StartSplashAnimation()
        {
            currentPhase = Phase.Animating;

            FlashXmlAnimationDefinition definition = FlashXmlImporter.ParseFile(
                ContentPaths.GetAnimationXmlAbsolutePath("zepto_splash.xml"));

            animRoot = new FlashXmlStageRoot();
            _ = animRoot.InitWithTexture(Application.GetTexture(Resources.Img.ZeptoLabLogoAnim));
            animRoot.SetDrawQuad(0);
            animRoot.color = RGBAColor.transparentRGBA;
            animRoot.passColorToChilds = false;

            // Leave stage root at origin with no scale — the view applies
            // engine-standard layout values to center and scale the animation.
            animRoot.width = (int)definition.StageWidth;
            animRoot.height = (int)definition.StageHeight;
            animStageWidth = definition.StageWidth;
            animStageHeight = definition.StageHeight;
            UpdateSplashLayout();

            animParts = [];
            FlashXmlTargetAnimationBackend.BuildParts(definition, animRoot, animParts, -1, -1);
            FlashXmlTargetAnimationBackend.BuildRootTimelines(definition, animRoot, -1, -1);
            FlashXmlTargetAnimationBackend.PlayTimeline(animParts, 0);
            FlashXmlTargetAnimationBackend.PlayRootTimeline(animRoot, 0);
            CTRSoundMgr.PlaySound(Resources.Snd.ZeptoLogoBubbles);

            if (animRoot.GetTimeline(0) is { } rootTimeline)
            {
                rootTimeline.delegateTimelineDelegate = this;
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
            currentPhase = Phase.Loading;
            resourcesLoaded = false;
            currentPercent = 0f;
            animFinished = false;
            animRoot = null;
            animParts = null;
            ShowView(1);
            UpdateChecker.StartIfNeeded();
            Game1.RPC.Setup();
            MoviePlaybackFinished(null);
        }

        public void AllResourcesLoaded()
        {
            resourcesLoaded = true;
        }

        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        public void TimelineFinished(Timeline t)
        {
            animFinished = true;
        }

        private void UpdateSplashLayout()
        {
            if (animRoot == null || animStageWidth <= 0f || animStageHeight <= 0f)
            {
                return;
            }

            float widthScale = SCREEN_WIDTH / animStageWidth;
            float heightScale = SCREEN_HEIGHT / animStageHeight;
            float scale = widthScale < heightScale ? widthScale : heightScale;

            animRoot.anchor = 18;
            animRoot.parentAnchor = -1;
            animRoot.x = SCREEN_WIDTH / 2f;
            animRoot.y = SCREEN_HEIGHT / 2f;
            animRoot.scaleX = scale;
            animRoot.scaleY = scale;
        }

        private Phase currentPhase;
        internal float currentPercent;
        private bool resourcesLoaded;
        private FlashXmlStageRoot animRoot;
        private List<Image> animParts;
        private bool animFinished;
        private float animStageWidth;
        private float animStageHeight;

        private static readonly string[] PackCommon =
        [
            Resources.Snd.Tap,
            Resources.Snd.ZeptoLogoBubbles,
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

        private sealed class StartupView(StartupController ctrl) : View
        {
            private readonly StartupController controller = ctrl;

            public override void Draw()
            {
                Renderer.Enable(Renderer.GL_BLEND);
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);

                // White background
                Renderer.Disable(Renderer.GL_TEXTURE_2D);
                DrawHelper.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.solidOpaqueRGBA);
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
                Renderer.SetColor(Color.White);

                switch (controller.currentPhase)
                {
                    case Phase.Loading:
                        CTRTexture2D barTex = Application.GetTexture(Resources.Img.ZeptoLabLogoLoading);
                        float barW = barTex.quadRects[0].w;
                        float barH = barTex.quadRects[0].h;
                        float barX = (SCREEN_WIDTH - barW) / 2f;
                        float barY = (SCREEN_HEIGHT - barH) / 2f;

                        // Empty bar centered
                        DrawHelper.DrawImageQuad(barTex, 0, barX, barY);

                        // Full bar with scissor from bottom up
                        float fillH = barH * controller.currentPercent / 100f;
                        if (fillH > 0f)
                        {
                            Renderer.Enable(Renderer.GL_SCISSOR_TEST);
                            Renderer.SetScissor(barX, barY + barH - fillH, barW, fillH);
                            DrawHelper.DrawImageQuad(barTex, 1, barX, barY);
                            Renderer.Disable(Renderer.GL_SCISSOR_TEST);
                        }

                        break;
                    case Phase.Animating:
                        controller.UpdateSplashLayout();
                        controller.animRoot.Draw();
                        break;
                    default:
                        break;
                }

                Renderer.Disable(Renderer.GL_BLEND);
            }
        }
    }
}
