using CutTheRope.Framework;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// GameScene.Initialize - Partial class handling Game state initialization
    /// Initializes Game state and collections for each new level
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Initializes core Game state and object collections
        /// Resets all state variables and creates fresh DynamicArray collections
        /// </summary>
        private void InitializeGameState()
        {
            CTRSoundMgr.EnableLoopedSounds(true);
            aniPool.RemoveAllChilds();
            staticAniPool.RemoveAllChilds();
            Lantern.RemoveAllLanterns();
            isCandyInLantern = false;
            gravityButton = null;
            gravityTouchDown = -1;
            twoParts = 2;
            partsDist = 0f;
            targetSock = null;
            CTRSoundMgr.StopLoopedSounds();

            // Initialize object collections
            bungees = new DynamicArray<Grab>();
            razors = new DynamicArray<Razor>();
            spikes = new DynamicArray<Spikes>();
            stars = new DynamicArray<Star>();
            bubbles = new DynamicArray<Bubble>();
            pumps = new DynamicArray<Pump>();
            tubes = new DynamicArray<SteamTube>();
            socks = new DynamicArray<Sock>();
            tutorialImages = new DynamicArray<CTRGameObject>();
            tutorials = new DynamicArray<Text>();
            bouncers = new DynamicArray<Bouncer>();
            rotatedCircles = new DynamicArray<RotatedCircle>();
            ghosts = new DynamicArray<Ghost>();
            earthAnims = null;
            pollenDrawer = new PollenDrawer();
            isCandyInGhostBubbleAnimationLoaded = false;
            isCandyInGhostBubbleAnimationLeftLoaded = false;
            isCandyInGhostBubbleAnimationRightLoaded = false;
            shouldRestoreSecondGhost = false;
        }

        /// <summary>
        /// Initializes candy and constraint point objects
        /// Sets up the main candy, candy variants (left/right), and related animations
        /// </summary>
        private void InitializeCandyObjects()
        {
            // Initialize constraint points for ropes
            star = new ConstraintedPoint();
            star.SetWeight(1f);
            starL = new ConstraintedPoint();
            starL.SetWeight(1f);
            starR = new ConstraintedPoint();
            starR.SetWeight(1f);

            // Get selected candy skin from preferences (0-50 for candy_01 to candy_51)
            int selectedCandySkin = Framework.Core.Preferences.GetIntForKey(CTRPreferences.PREFS_SELECTED_CANDY);
            string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);

            // Initialize main candy
            candy = GameObject.GameObject_createWithResIDQuad(candyResource, 0);
            candy.DoRestoreCutTransparency();
            candy.anchor = 18;
            candy.bb = MakeRectangle(142f, 157f, 112f, 104f);
            candy.passTransformationsToChilds = false;
            candy.scaleX = candy.scaleY = 0.71f;

            // Add candy main visual component
            candyMain = GameObject.GameObject_createWithResIDQuad(candyResource, 1);
            candyMain.DoRestoreCutTransparency();
            candyMain.anchor = candyMain.parentAnchor = 18;
            _ = candy.AddChild(candyMain);
            candyMain.scaleX = candyMain.scaleY = 0.71f;

            // Add candy top visual component
            candyTop = GameObject.GameObject_createWithResIDQuad(candyResource, 2);
            candyTop.DoRestoreCutTransparency();
            candyTop.anchor = candyTop.parentAnchor = 18;
            _ = candy.AddChild(candyTop);
            candyTop.scaleX = candyTop.scaleY = 0.71f;

            // Setup candy blink animation (highlight_start=2, layer_1-8=3-10, highlight_end=1)
            candyBlink = Animation.Animation_createWithResID(Resources.Img.ObjCandyFx);
            candyBlink.AddAnimationWithIDDelayLoopFirstLast(0, 0.07f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 9);
            candyBlink.AddAnimationWithIDDelayLoopCountSequence(1, 0.3f, Timeline.LoopType.TIMELINE_NO_LOOP, 2, 10, [10]);
            Timeline timeline7 = candyBlink.GetTimeline(1);
            timeline7.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline7.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
            candyBlink.visible = false;
            candyBlink.anchor = candyBlink.parentAnchor = 18;
            candyBlink.scaleX = candyBlink.scaleY = 0.71f;
            _ = candy.AddChild(candyBlink);

            // Setup candy bubble animation
            candyBubbleAnimation = Animation.Animation_createWithResID(Resources.Img.ObjBubbleFlight);
            candyBubbleAnimation.x = candy.x;
            candyBubbleAnimation.y = candy.y;
            candyBubbleAnimation.parentAnchor = candyBubbleAnimation.anchor = 18;
            _ = candyBubbleAnimation.AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
            candyBubbleAnimation.PlayTimeline(0);
            _ = candy.AddChild(candyBubbleAnimation);
            candyBubbleAnimation.visible = false;
        }

        /// <summary>
        /// Initializes HUD stars visibility
        /// Resets the HUD star timeline animations
        /// </summary>
        private void InitializeHUDStars()
        {
            for (int i = 0; i < 3; i++)
            {
                Timeline timeline2 = hudStar[i].GetCurrentTimeline();
                timeline2?.StopTimeline();
                hudStar[i].SetDrawQuad(0);
            }
        }

        private void EnsureCandyGhostBubbleAnimations()
        {
            if (!isCandyInGhostBubbleAnimationLoaded && candy != null)
            {
                candyGhostBubbleAnimation = CandyInGhostBubbleAnimation.CIGBAnimation_createWithResID(Resources.Img.ObjBubbleFlight);
                candyGhostBubbleAnimation.parentAnchor = candyGhostBubbleAnimation.anchor = 18;
                _ = candy.AddChild(candyGhostBubbleAnimation);
                candyGhostBubbleAnimation.visible = false;
                candyGhostBubbleAnimation.AddSupportingCloudsTimelines();
                _ = candyGhostBubbleAnimation.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyGhostBubbleAnimation.PlayTimeline(0);
                isCandyInGhostBubbleAnimationLoaded = true;
            }
            if (!isCandyInGhostBubbleAnimationLeftLoaded && candyL != null)
            {
                candyGhostBubbleAnimationL = CandyInGhostBubbleAnimation.CIGBAnimation_createWithResID(Resources.Img.ObjBubbleFlight);
                candyGhostBubbleAnimationL.parentAnchor = candyGhostBubbleAnimationL.anchor = 18;
                _ = candyL.AddChild(candyGhostBubbleAnimationL);
                candyGhostBubbleAnimationL.visible = false;
                candyGhostBubbleAnimationL.AddSupportingCloudsTimelines();
                _ = candyGhostBubbleAnimationL.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyGhostBubbleAnimationL.PlayTimeline(0);
                isCandyInGhostBubbleAnimationLeftLoaded = true;
            }
            if (!isCandyInGhostBubbleAnimationRightLoaded && candyR != null)
            {
                candyGhostBubbleAnimationR = CandyInGhostBubbleAnimation.CIGBAnimation_createWithResID(Resources.Img.ObjBubbleFlight);
                candyGhostBubbleAnimationR.parentAnchor = candyGhostBubbleAnimationR.anchor = 18;
                _ = candyR.AddChild(candyGhostBubbleAnimationR);
                candyGhostBubbleAnimationR.visible = false;
                candyGhostBubbleAnimationR.AddSupportingCloudsTimelines();
                _ = candyGhostBubbleAnimationR.AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_REPLAY, 0, 12);
                candyGhostBubbleAnimationR.PlayTimeline(0);
                isCandyInGhostBubbleAnimationRightLoaded = true;
            }
        }
    }
}
