using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Coordinates the startup loading screen, ZeptoLab splash animation, and transition into the root controller.
    /// </summary>
    internal sealed class StartupController : ViewController, IResourceMgrDelegate, IMovieMgrDelegate, ITimelineDelegate
    {
        /// <summary>
        /// Startup phases displayed by the controller.
        /// </summary>
        private enum Phase
        {
            /// <summary>Resource packs are loading and the progress bar is shown.</summary>
            Loading,

            /// <summary>The splash animation is playing after resources have loaded.</summary>
            Animating
        }

        /// <summary>
        /// Initializes a startup controller.
        /// </summary>
        /// <param name="parent">Parent view controller.</param>
        public StartupController(ViewController parent)
            : base(parent)
        {
            AddViewwithID(new StartupView(this), 1);
        }

        /// <inheritdoc />
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
                PlatformServices.Cursor?.Enable(true);
                animRoot?.Update(t);
                animElapsed += t;

                // Start fading the disclaimer text before the animation ends
                float fadeStart = animTotalDuration - DisclaimerFadeDuration;
                if (animElapsed >= fadeStart)
                {
                    disclaimerFadeElapsed = animElapsed - fadeStart;
                    UpdateDisclaimerAlpha();
                }

                if (animFinished)
                {
                    Application.SharedRootController().SetViewTransition(4);
                    Deactivate();
                }
            }
        }

        /// <summary>
        /// Builds and starts the Flash XML splash animation once loading has completed.
        /// </summary>
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
            EnsureDisclaimerText();

            if (animRoot.GetTimeline(0) is { } rootTimeline)
            {
                rootTimeline.delegateTimelineDelegate = this;
            }

            animElapsed = 0f;
            animTotalDuration = definition.RootTimelines.TryGetValue(0, out float duration) ? duration : 3.214f;
        }

        /// <summary>
        /// Starts resource loading after startup movie playback has finished.
        /// </summary>
        /// <param name="url">Movie URL reported by the movie manager, or <see langword="null"/> when loading starts directly.</param>
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

        /// <inheritdoc />
        public override void Activate()
        {
            base.Activate();
            currentPhase = Phase.Loading;
            resourcesLoaded = false;
            currentPercent = 0f;
            animFinished = false;
            disclaimerFadeElapsed = 0f;
            animRoot = null;
            animParts = null;
            ShowView(1);
            PlatformServices.Updates?.StartIfNeeded();
            // RPC belongs to the desktop host and is absent in headless runs.
            PlatformServices.RichPresence?.Setup();
            MoviePlaybackFinished(null);
        }

        /// <summary>
        /// Marks startup resource loading as complete.
        /// </summary>
        public void AllResourcesLoaded()
        {
            // Building the menu is the last expensive step of startup, and it used to run in the
            // frame that hands this controller over — after the bar had already reported 100%,
            // so it read as a freeze on a blank screen. Doing it here puts it under the bar,
            // which is still short of full and goes on animating while this returns.
            ((CTRRootController)Application.SharedRootController()).PrebuildMenuControllers();
            resourcesLoaded = true;
        }

        /// <inheritdoc />
        public override bool TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            if (currentPhase == Phase.Animating)
            {
                CTRSoundMgr.StopSounds();
                animFinished = true;
                return true;
            }
            return base.TouchesBeganwithEvent(touches);
        }

        /// <inheritdoc />
        public void TimelinereachedKeyFramewithIndex(Timeline t, KeyFrame k, int i)
        {
        }

        /// <inheritdoc />
        public void TimelineFinished(Timeline t)
        {
            animFinished = true;
        }

        /// <inheritdoc />
        protected override void Relayout(ViewportLayoutSnapshot snapshot)
        {
            base.Relayout(snapshot);
            UpdateSplashLayout();
        }

        /// <summary>
        /// Updates the splash animation root and legal disclaimer position for the current screen size.
        /// </summary>
        /// <remarks>
        /// Called from <see cref="Relayout"/> and once at build time, not from the draw path. The
        /// splash used to lay itself out inside its own <c>Draw</c>, which hid the fact that
        /// nothing was re-wrapping the disclaimer on a resize: the stage tracked the window every
        /// frame while the text it sat under kept the wrap width of the viewport that built it.
        /// </remarks>
        private void UpdateSplashLayout()
        {
            if (animRoot == null || animStageWidth <= 0f || animStageHeight <= 0f)
            {
                return;
            }

            // The splash is a single stage centered on screen, so it measures against what the
            // viewport exposes rather than the fixed design size. The two are the same thing at
            // the design shape, and the stage sat off toward one corner at every other.
            CTRRectangle visible = VisibleBounds;
            SplashLayout layout = SplashLayout.For(visible, animStageWidth, animStageHeight);
            float scale = layout.Stage.w / animStageWidth;

            animRoot.anchor = 18;
            animRoot.parentAnchor = -1;
            animRoot.x = visible.w / 2f;
            animRoot.y = visible.h / 2f;
            animRoot.scaleX = scale;
            animRoot.scaleY = scale;

            if (legalDisclaimerText != null)
            {
                // Scaled here rather than where the text is made, because how large it is drawn is
                // a property of the viewport like its wrap width and its place are.
                legalDisclaimerText.scaleX = legalDisclaimerText.scaleY = layout.DisclaimerScale;
                legalDisclaimerText.SetStringandWidth(
                    Application.GetString("STARTUP_LEGAL_DISCLAIMER"),
                    layout.DisclaimerWrapWidth);
                legalDisclaimerText.x = visible.w / 2f;
                legalDisclaimerText.y = layout.DisclaimerBottom;
            }
        }

        /// <summary>
        /// Creates or refreshes the legal disclaimer text shown during the splash animation.
        /// </summary>
        private void EnsureDisclaimerText()
        {
            if (legalDisclaimerText == null)
            {
                legalDisclaimerText = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
                legalDisclaimerText.SetAlignment(2);
                legalDisclaimerText.anchor = legalDisclaimerText.parentAnchor = 34;
            }

            UpdateDisclaimerAlpha();

            // Sized, wrapped and positioned by the layout pass, which is the only place that reads
            // the viewport, so a resize re-does all three and creating it here cannot disagree
            // with one.
            UpdateSplashLayout();
        }

        /// <summary>
        /// Applies the legal disclaimer alpha based on the current fade timer.
        /// </summary>
        private void UpdateDisclaimerAlpha()
        {
            float alpha = DisclaimerTextBaseAlpha;
            if (disclaimerFadeElapsed > 0f)
            {
                float fadeProgress = disclaimerFadeElapsed / DisclaimerFadeDuration;
                if (fadeProgress > 1f)
                {
                    fadeProgress = 1f;
                }

                alpha *= 1f - fadeProgress;
            }

            _ = (legalDisclaimerText?.color = RGBAColor.MakeRGBA(0f, 0f, 0f, alpha * 0.9f));
        }

        /// <summary>Current startup phase.</summary>
        private Phase currentPhase;

        /// <summary>Smoothed visible loading percentage for the startup progress bar.</summary>
        internal float currentPercent;

        /// <summary>Whether all requested startup resource packs have finished loading.</summary>
        private bool resourcesLoaded;

        /// <summary>Root node for the Flash XML splash animation.</summary>
        private FlashXmlStageRoot animRoot;

        /// <summary>Flat list of splash animation parts addressed by timeline targets.</summary>
        private List<Image> animParts;

        /// <summary>Whether the splash animation should finish and transition away.</summary>
        private bool animFinished;

        /// <summary>Elapsed splash animation time in seconds.</summary>
        private float animElapsed;

        /// <summary>Total expected splash animation duration in seconds.</summary>
        private float animTotalDuration;

        /// <summary>Original Flash XML stage width for splash layout scaling.</summary>
        private float animStageWidth;

        /// <summary>Original Flash XML stage height for splash layout scaling.</summary>
        private float animStageHeight;

        /// <summary>Legal disclaimer text drawn over the splash animation.</summary>
        private Text legalDisclaimerText;

        /// <summary>Elapsed fade time for the legal disclaimer text.</summary>
        private float disclaimerFadeElapsed;

        /// <summary>Duration of the legal disclaimer fade-out in seconds.</summary>
        private const float DisclaimerFadeDuration = 0.25f;

        /// <summary>Base alpha applied to the legal disclaimer text before fade-out.</summary>
        private const float DisclaimerTextBaseAlpha = 0.85f;

        /// <summary>
        /// Common startup resources loaded before entering the menu, terminated by <see langword="null"/>.
        /// </summary>
        private static readonly string[] PackCommon =
        [
            Resources.Snd.Tap,
            Resources.Snd.ZeptoLogoBubbles,
            Resources.Str.MenuStrings,
            Resources.Fnt.BigFont,
            null,
        ];

        /// <summary>
        /// Shared menu image resources loaded before entering the menu, terminated by <see langword="null"/>.
        /// </summary>
        private static readonly string[] PackCommonImages =
        [
            Resources.Img.MenuButtons,
            Resources.Img.MenuLevelUi,
            Resources.Img.MenuOptions,
            null
        ];

        /// <summary>
        /// Main menu image resources loaded before entering the menu, terminated by <see langword="null"/>.
        /// </summary>
        /// <remarks>
        /// Every image the menu controller's constructor reaches for belongs here. Anything left
        /// out is still decoded and uploaded — just in the single frame that builds the menu,
        /// after the progress bar has already reported completion, which reads as a freeze on
        /// slower texture paths like the browser's.
        /// </remarks>
        private static readonly string[] PackMenu =
        [
            Resources.Img.MenuBgr,
            Resources.Img.MenuPopup,
            Resources.Img.MenuLogo,
            Resources.Img.MenuLogoNew,
            Resources.Img.CutTheRopeDXLogo,
            Resources.Img.MenuPackSelection,
            Resources.Img.MenuPackSelection2,
            Resources.Img.MenuPackUI,
            Resources.Img.MenuExtraButtons,
            Resources.Img.MenuBgrShadow,
            Resources.Img.MenuBgrXmas,
            Resources.BackgroundImg.SkinBackground,
            Resources.Img.SkinSelection,
            Resources.Img.CandySelectionFx,
            null
        ];

        /// <summary>
        /// Localized menu resources loaded before entering the menu, terminated by <see langword="null"/>.
        /// </summary>
        private static readonly string[] PackLocalizationMenu = [Resources.Img.MenuExtraButtonsEn, null];

        /// <summary>
        /// View responsible for drawing the startup loading bar and splash animation.
        /// </summary>
        /// <param name="ctrl">Startup controller that owns the view state.</param>
        private sealed class StartupView(StartupController ctrl) : View
        {
            /// <summary>Controller that owns the startup state drawn by the view.</summary>
            private readonly StartupController controller = ctrl;

            /// <inheritdoc />
            public override void Draw()
            {
                Renderer.Enable(Renderer.GL_BLEND);
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);

                // White background
                Renderer.Disable(Renderer.GL_TEXTURE_2D);
                DrawHelper.DrawSolidRectWOBorder(0f, 0f, VisibleBounds.w, VisibleBounds.h, RGBAColor.solidOpaqueRGBA);
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
                Renderer.SetColor(Color.White);

                switch (controller.currentPhase)
                {
                    case Phase.Loading:
                        CTRTexture2D barTex = Application.GetTexture(Resources.Img.ZeptoLabLogoLoading);
                        float barW = barTex.quadRects[0].w;
                        float barH = barTex.quadRects[0].h;
                        float barX = (VisibleBounds.w - barW) / 2f;
                        float barY = (VisibleBounds.h - barH) / 2f;

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
                        controller.animRoot?.Draw();
                        controller.legalDisclaimerText?.Draw();
                        break;
                    default:
                        break;
                }

                Renderer.Disable(Renderer.GL_BLEND);
            }
        }
    }
}
