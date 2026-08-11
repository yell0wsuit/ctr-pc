using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Initializes core Game state and object collections
        /// Resets all state variables and creates fresh collections
        /// </summary>
        private void InitializeGameState()
        {
            CTRSoundMgr.EnableLoopedSounds(true);
            aniPool.RemoveAllChilds();
            particlesAniPool.RemoveAllChilds();
            staticAniPool.RemoveAllChilds();
            decalsLayer?.RemoveAllChilds();
            Lantern.RemoveAllLanterns();
            gravityState.BeginLoad();
            if (waterLayer != null)
            {
                waterLayer.PrepareToRelease();
                waterLayer.Dispose();
                waterLayer = null;
            }
            waterLevel = 0f;
            waterSpeed = 0f;
            CTRSoundMgr.StopLoopedSounds();

            // Initialize object collections
            bungees = [];
            ropes = new RopeRegistry();
            candyConnector = null;
            candiesConnected = false;
            candiesConnectedBreakable = true;
            razors = [];
            spikes = [];
            stars = [];
            bubbles = [];
            pumps = [];
            tubes = [];
            bambooTubes = [];
            socks = [];
            tutorialImages = [];
            tutorials = [];
            bouncers = [];
            rotatedCircles = [];
            rockets = [];
            hands = [];
            snailobjects = [];
            ghosts = [];
            conveyors = new ConveyorBeltObject
            {
                OnDestroyRopesForCandy = DestroyRopesForCandy
            };
            antsPathsSegments = [];
            antsPaths = [];

            // Cleanup old mice before creating new arrays
            if (mice != null)
            {
                foreach (object obj in mice)
                {
                    if (obj is Mouse mouse)
                    {
                        mouse.Cleanup();
                    }
                }
            }

            mice = [];
            miceManager = null;
            pollenDrawer = new PollenDrawer();
            parkedGhostBubble = null;
            pendingLanternCapture = null;
            targets.Clear();
            targetBaseScaleX = 1f;
            targetBaseScaleY = 1f;
            gameplayFlow.ResetOutcome();
            pendingLevelResult = null;
            levelName = null;
        }

        /// <summary>
        /// Builds the shared candy visual stack — root sprite, main/top layers, the collect-glow
        /// blink animation, the reappear timeline (id 2) played by <see cref="Teleport"/> after a
        /// bamboo-tube exit, and the (hidden) normal + ghost bubble overlays. Shared by the primary
        /// candy and every additional candy so they stay identical; callers position the root and
        /// decide where to store the returned bubbles.
        /// </summary>
        private (GameObject candy, GameObject candyMain, GameObject candyTop, Animation candyBlink, Animation candyBubble, CandyInGhostBubbleAnimation candyGhostBubble) CreateCandyVisual()
        {
            // Get selected candy skin from preferences (0-50 for candy_01 to candy_51)
            int selectedCandySkin = Framework.Core.Preferences.GetIntForKey("PREFS_SELECTED_CANDY");
            string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);

            // Initialize main candy
            GameObject candyObj = GameObject.GameObject_createWithResIDQuad(candyResource, 0);
            candyObj.DoRestoreCutTransparency();
            candyObj.anchor = 18;
            candyObj.bb = GetCandyBoundingBox();
            candyObj.passTransformationsToChilds = false;
            candyObj.scaleX = candyObj.scaleY = 0.71f;

            // Candy reappear animation (timeline 2): scale 0→0.71 + transparent→opaque over 0.1s.
            // Mirrors iOS: played by Teleport() after candy exits a bamboo tube.
            Timeline candyReappearTimeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            candyReappearTimeline.AddKeyFrame(KeyFrame.MakeScale(0f, 0f, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            candyReappearTimeline.AddKeyFrame(KeyFrame.MakeScale(candyObj.scaleX, candyObj.scaleY, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            candyReappearTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_IMMEDIATE, 0f));
            candyReappearTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.1f));
            candyReappearTimeline.delegateTimelineDelegate = this;
            candyObj.AddTimelinewithID(candyReappearTimeline, 2);

            // Add candy main visual component
            GameObject candyMainObj = GameObject.GameObject_createWithResIDQuad(candyResource, 1);
            candyMainObj.DoRestoreCutTransparency();
            candyMainObj.anchor = candyMainObj.parentAnchor = 18;
            _ = candyObj.AddChild(candyMainObj);
            candyMainObj.scaleX = candyMainObj.scaleY = 0.71f;

            // Add candy top visual component
            GameObject candyTopObj = GameObject.GameObject_createWithResIDQuad(candyResource, 2);
            candyTopObj.DoRestoreCutTransparency();
            candyTopObj.anchor = candyTopObj.parentAnchor = 18;
            _ = candyObj.AddChild(candyTopObj);
            candyTopObj.scaleX = candyTopObj.scaleY = 0.71f;

            // Setup candy blink animation (highlight_start=2, layer_1-8=3-10, highlight_end=1)
            Animation candyBlinkAnim = Animation.Animation_createWithResID(Resources.Img.ObjCandyFx);
            candyBlinkAnim.AddAnimationWithIDDelayLoopFirstLast(0, 0.07f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 9);
            candyBlinkAnim.AddAnimationWithIDDelayLoopCountSequence(1, 0.3f, Timeline.LoopType.TIMELINE_NO_LOOP, 2, 10, [10]);
            Timeline blinkColorTimeline = candyBlinkAnim.GetTimeline(1);
            blinkColorTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            blinkColorTimeline.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2f));
            candyBlinkAnim.visible = false;
            candyBlinkAnim.anchor = candyBlinkAnim.parentAnchor = 18;
            candyBlinkAnim.scaleX = candyBlinkAnim.scaleY = 0.71f;
            _ = candyObj.AddChild(candyBlinkAnim);

            // Bubble overlays (both start hidden): normal bubble first, then the ghost-form bubble,
            // so draw order matches the legacy primary candy where the ghost bubble was attached last.
            Animation candyBubbleAnim = BubbleAnimationFactory.CreateBubble();
            _ = candyObj.AddChild(candyBubbleAnim);

            CandyInGhostBubbleAnimation candyGhostBubbleAnim = BubbleAnimationFactory.CreateGhostBubble();
            _ = candyObj.AddChild(candyGhostBubbleAnim);

            return (candyObj, candyMainObj, candyTopObj, candyBlinkAnim, candyBubbleAnim, candyGhostBubbleAnim);
        }

        /// <summary>
        /// Initializes candy and constraint point objects
        /// Sets up the main candy, candy variants (left/right), and related animations
        /// </summary>
        private void InitializeCandyObjects()
        {
            candyPairPrevDistance.Clear();

            // Initialize constraint points for ropes
            ConstraintedPoint starPoint = new();
            starPoint.SetWeight(1f);

            (GameObject candyObj, GameObject candyMainObj, GameObject candyTopObj, Animation candyBlinkAnim, Animation primaryBubble, CandyInGhostBubbleAnimation primaryGhostBubble) = CreateCandyVisual();

            // Register the primary candy as candies[0] so multi-candy logic and legacy
            // single-candy code share the same objects. Its candyNumber is unassigned here;
            // the first <candy> element claims it and takes the key from XML.
            candies.Clear();
            primaryCandyClaimed = false;

            CandyBody primaryBody = new(
                starPoint,
                CandyBodyRole.Whole,
                candyObj,
                candyMainObj,
                candyTopObj,
                candyBlinkAnim,
                primaryBubble,
                primaryGhostBubble);

            candies.Add(new CandyContext(primaryBody)
            {
                candyNumber = null,
                Capabilities = CandyCapabilities.Candy,
            });
        }

        /// <summary>
        /// Builds one authored split half: its physics point, the visual that follows it, and the
        /// (hidden) normal + ghost bubble overlays that visual carries. Called as
        /// <c>&lt;candyL&gt;</c>/<c>&lt;candyR&gt;</c> parse, before the halves are handed to the
        /// primary candy's lifecycle. Each half owns its own overlays, exactly like a whole candy,
        /// so bubble capture never has to know which half it is holding.
        /// </summary>
        /// <param name="role">Which half this is.</param>
        /// <param name="x">World X of the half.</param>
        /// <param name="y">World Y of the half.</param>
        /// <returns>The new half body.</returns>
        private static CandyBody CreateSplitHalfBody(CandyBodyRole role, float x, float y)
        {
            ConstraintedPoint point = new();
            point.SetWeight(1f);
            point.pos.X = x;
            point.pos.Y = y;
            point.prevPos = point.pos;

            int selectedCandySkin = Framework.Core.Preferences.GetIntForKey("PREFS_SELECTED_CANDY");
            string candyResource = CandySkinHelper.GetCandyResource(selectedCandySkin);
            GameObject visual = GameObject.GameObject_createWithResIDQuad(
                candyResource,
                role == CandyBodyRole.LeftHalf ? SplitCandyLeftQuad : SplitCandyRightQuad);
            visual.scaleX = visual.scaleY = 0.71f;
            visual.passTransformationsToChilds = false;
            visual.DoRestoreCutTransparency();
            visual.anchor = 18;
            visual.x = x;
            visual.y = y;
            visual.bb = GetSplitCandyBoundingBox();

            // Seed the draw position from the authored one. The merge test reads where a half was
            // last drawn, which is a frame behind its point, so on the very first frame it has to
            // read the half's authored spot - not the origin.
            CalculateTopLeft(visual);

            // Normal bubble first, then the ghost-form bubble, matching the child order the whole
            // candy's visual stack uses.
            Animation bubbleAnim = BubbleAnimationFactory.CreateBubble();
            _ = visual.AddChild(bubbleAnim);
            CandyInGhostBubbleAnimation ghostBubbleAnim = BubbleAnimationFactory.CreateGhostBubble();
            _ = visual.AddChild(ghostBubbleAnim);

            return new CandyBody(point, role, visual, bubbleAnimation: bubbleAnim, ghostBubbleAnimation: ghostBubbleAnim);
        }

        /// <summary>
        /// Hands the primary candy its two authored halves once metadata has built them. A split level
        /// is one logical candy with two bodies, so the halves become a <see cref="SplitCandyState"/>
        /// owned by <c>candies[0]</c>'s lifecycle rather than a second scene-level candy. A map that
        /// declares <c>twoParts</c> but authors only one half is left whole, matching the old
        /// behavior of a null <c>candyL</c>/<c>candyR</c> pair; because reading <c>twoParts</c>
        /// already reserved the primary, such a map keeps an unplaced whole primary and turns its
        /// <c>&lt;candy&gt;</c> elements into extra candies. No shipped map authors that.
        /// </summary>
        private void InstallSplitCandyState()
        {
            if (!levelAuthorsSplitCandy || pendingLeftHalf == null || pendingRightHalf == null)
            {
                return;
            }

            CandyHalf left = new(pendingLeftHalf);
            CandyHalf right = new(pendingRightHalf);
            pendingLeftHalf = null;
            pendingRightHalf = null;

            // candies[0] was already reserved for the split candy when the level's twoParts flag was
            // read, so no <candy> element parsed in between can have taken it.
            _ = candies[0].TryBeginSplit(new SplitCandyState(left, right));
        }

        /// <summary>
        /// Builds one independent candy (point + visual layers) at the given world position and
        /// registers it as a <see cref="CandyContext"/>. Mirrors the primary-candy setup.
        /// </summary>
        private CandyContext CreateCandyContext(string candyNumber, float px, float py)
        {
            ConstraintedPoint p = new();
            p.SetWeight(1f);
            p.pos.X = px;
            p.pos.Y = py;
            p.prevPos = p.pos;

            (GameObject c, GameObject cMain, GameObject cTop, Animation blink, Animation bubbleAnim, CandyInGhostBubbleAnimation ghostBubbleAnim) = CreateCandyVisual();
            c.x = px;
            c.y = py;

            CandyBody body = new(p, CandyBodyRole.Whole, c, cMain, cTop, blink, bubbleAnim, ghostBubbleAnim);

            CandyContext ctx = new(body)
            {
                candyNumber = candyNumber,
                Capabilities = CandyCapabilities.Candy,
            };
            candies.Add(ctx);
            return ctx;
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
                const int HudUiStarFirstQuad = 1;
                hudStar[i].SetDrawQuad(HudUiStarFirstQuad);
            }
        }

    }
}
