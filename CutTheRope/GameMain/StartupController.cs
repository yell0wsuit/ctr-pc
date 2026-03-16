using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Media;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework;

namespace CutTheRope.GameMain
{
    internal sealed class StartupController : ViewController, IResourceMgrDelegate, IMovieMgrDelegate
    {
        public StartupController(ViewController parent)
            : base(parent)
        {
            AddViewwithID(new StartupView(this), 1);
        }

        public override void Update(float t)
        {
            base.Update(t);
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

            // Wait for animation to complete before transitioning
            if (resourcesLoaded && currentPercent >= 99.5f)
            {
                Application.SharedRootController().SetViewTransition(4);
                Deactivate();
                resourcesLoaded = false;
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
            resourcesLoaded = false;
            ShowView(1);
            UpdateChecker.StartIfNeeded();
            Game1.RPC.Setup();
            MoviePlaybackFinished(null);
        }

        public void AllResourcesLoaded()
        {
            resourcesLoaded = true;
        }

        internal float currentPercent;
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

        private sealed class StartupView : View
        {
            private readonly StartupController controller;

            public StartupView(StartupController ctrl)
            {
                controller = ctrl;
            }

            public override void Draw()
            {
                Renderer.Enable(Renderer.GL_BLEND);
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);

                // White background
                Renderer.Disable(Renderer.GL_TEXTURE_2D);
                DrawHelper.DrawSolidRectWOBorder(0f, 0f, SCREEN_WIDTH, SCREEN_HEIGHT, RGBAColor.solidOpaqueRGBA);
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
                Renderer.SetColor(Color.White);

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

                Renderer.Disable(Renderer.GL_BLEND);
            }
        }
    }
}
