using CutTheRope.iframework;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    /// <summary>
    /// GameScene.Initialize - Partial class handling game state initialization
    /// Initializes game state and collections for each new level
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Initializes core game state and object collections
        /// Resets all state variables and creates fresh DynamicArray collections
        /// </summary>
        private void InitializeGameState()
        {
            CTRSoundMgr.EnableLoopedSounds(true);
            aniPool.RemoveAllChilds();
            staticAniPool.RemoveAllChilds();
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
            socks = new DynamicArray<Sock>();
            tutorialImages = new DynamicArray<CTRGameObject>();
            tutorials = new DynamicArray<Text>();
            bouncers = new DynamicArray<Bouncer>();
            rotatedCircles = new DynamicArray<RotatedCircle>();
            earthAnims = null;
            pollenDrawer = new PollenDrawer();
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

            // Initialize main candy
            candy = GameObject.GameObject_createWithResIDQuad(63, 0);
            candy.DoRestoreCutTransparency();
            candy.anchor = 18;
            candy.bb = MakeRectangle(142f, 157f, 112f, 104f);
            candy.passTransformationsToChilds = false;
            candy.scaleX = candy.scaleY = 0.71f;

            // Add candy main visual component
            candyMain = GameObject.GameObject_createWithResIDQuad(63, 1);
            candyMain.DoRestoreCutTransparency();
            candyMain.anchor = candyMain.parentAnchor = 18;
            _ = candy.AddChild(candyMain);
            candyMain.scaleX = candyMain.scaleY = 0.71f;

            // Add candy top visual component
            candyTop = GameObject.GameObject_createWithResIDQuad(63, 2);
            candyTop.DoRestoreCutTransparency();
            candyTop.anchor = candyTop.parentAnchor = 18;
            _ = candy.AddChild(candyTop);
            candyTop.scaleX = candyTop.scaleY = 0.71f;

            // Setup candy blink animation
            candyBlink = Animation.Animation_createWithResID(63);
            candyBlink.AddAnimationWithIDDelayLoopFirstLast(0, 0.07f, Timeline.LoopType.TIMELINE_NO_LOOP, 8, 17);
            candyBlink.AddAnimationWithIDDelayLoopCountSequence(1, 0.3f, Timeline.LoopType.TIMELINE_NO_LOOP, 2, 18, [18]);
            Timeline timeline7 = candyBlink.GetTimeline(1);
            timeline7.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.solidOpaqueRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline7.AddKeyFrame(KeyFrame.MakeColor(RGBAColor.transparentRGBA, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.2));
            candyBlink.visible = false;
            candyBlink.anchor = candyBlink.parentAnchor = 18;
            candyBlink.scaleX = candyBlink.scaleY = 0.71f;
            _ = candy.AddChild(candyBlink);

            // Setup candy bubble animation
            candyBubbleAnimation = Animation.Animation_createWithResID(72);
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
    }
}
