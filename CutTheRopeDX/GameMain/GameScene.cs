using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Core gameplay scene that owns the loaded level state, interactive objects, and HUD.
    /// </summary>
    internal sealed partial class GameScene : BaseElement, ITimelineDelegate, IButtonDelegation, IRocketDelegate
    {
        /// <summary>
        /// Returns the largest value from four candidates.
        /// </summary>
        /// <param name="v1">The first candidate value.</param>
        /// <param name="v2">The second candidate value.</param>
        /// <param name="v3">The third candidate value.</param>
        /// <param name="v4">The fourth candidate value.</param>
        /// <returns>The maximum of the supplied values.</returns>
        private static float MaxOf4(float v1, float v2, float v3, float v4)
        {
            return v1 >= v2 && v1 >= v3 && v1 >= v4
                ? v1
                : v2 >= v1 && v2 >= v3 && v2 >= v4 ? v2 : v3 >= v2 && v3 >= v1 && v3 >= v4 ? v3 : v4 >= v2 && v4 >= v3 && v4 >= v1 ? v4 : -1f;
        }

        /// <summary>
        /// Returns the smallest value from four candidates.
        /// </summary>
        /// <param name="v1">The first candidate value.</param>
        /// <param name="v2">The second candidate value.</param>
        /// <param name="v3">The third candidate value.</param>
        /// <param name="v4">The fourth candidate value.</param>
        /// <returns>The minimum of the supplied values.</returns>
        private static float MinOf4(float v1, float v2, float v3, float v4)
        {
            return v1 <= v2 && v1 <= v3 && v1 <= v4
                ? v1
                : v2 <= v1 && v2 <= v3 && v2 <= v4 ? v2 : v3 <= v2 && v3 <= v1 && v3 <= v4 ? v3 : v4 <= v2 && v4 <= v3 && v4 <= v1 ? v4 : -1f;
        }

        /// <summary>
        /// Determines whether a constrained point has moved outside the playable screen bounds.
        /// </summary>
        /// <param name="p">The point to evaluate.</param>
        /// <returns><see langword="true"/> when the point is beyond the allowed bounds; otherwise, <see langword="false"/>.</returns>
        public bool PointOutOfScreen(ConstraintedPoint p)
        {
            // Mobile matches the WP7 kill bounds (+100/-50, scaled x3); the horizontal
            // margin stays as a safety net in both modes (the reference kills on Y only).
            float bottomMargin = ActivePhysicsConstants.UseMobilePhysicsModel ? 300f : 400f;
            float topMargin = ActivePhysicsConstants.UseMobilePhysicsModel ? 150f : 400f;
            return p.pos.Y > mapHeight + bottomMargin || p.pos.Y < -topMargin
                || p.pos.X < -SCREEN_WIDTH || p.pos.X > mapWidth + SCREEN_WIDTH;
        }

        /// <summary>
        /// Handles completion of XML loading for the current map and restarts the scene state.
        /// </summary>
        /// <param name="rootNode">The loaded XML root node.</param>
        /// <param name="_">The XML loader source identifier.</param>
        /// <param name="_1">The XML loader success flag.</param>
        public void XmlLoaderFinishedWithfromwithSuccess(XElement rootNode, string _, bool _1)
        {
            CTRRootController rootController = (CTRRootController)Application.SharedRootController();
            string resolvedMapName = ResolveMapName(_);
            rootController.PrepareMapAndEnsureResources(rootNode, resolvedMapName);
            if (animateRestartDim)
            {
                AnimateLevelRestart();
                return;
            }
            Restart();
        }

        /// <summary>
        /// Resolves the persisted map name from an XML loader source string.
        /// </summary>
        /// <param name="source">The XML loader source path or virtual identifier.</param>
        /// <returns>The filename for disk-backed maps, or the current root-controller map name for virtual reload sources.</returns>
        private static string ResolveMapName(string source)
        {
            return string.IsNullOrWhiteSpace(source) || source.Contains("://", StringComparison.Ordinal)
                ? ((CTRRootController)Application.SharedRootController()).GetMapName()
                : Path.GetFileName(source);
        }

        /// <summary>
        /// Determines whether a tutorial element should be skipped for the active language.
        /// </summary>
        /// <param name="c">The tutorial XML element to inspect.</param>
        /// <returns><see langword="true"/> when the element does not match the active locale; otherwise, <see langword="false"/>.</returns>
        public static bool ShouldSkipTutorialElement(XElement c)
        {
            string currentLang = LanguageHelper.CurrentCode;
            string locale = c.Attribute("locale")?.Value ?? string.Empty;
            return LanguageHelper.IsUiLanguageCode(currentLang) ? locale != currentLang : locale != "en";
        }

        /// <summary>
        /// Plays the active Om Nom greeting animation and matching sounds.
        /// </summary>
        public void ShowGreeting()
        {
            // On a two-Om-Nom level, randomly greet with the mutual chat instead of the wave.
            // TryShowChatGreeting returns false for diagonal/coincident pairs, falling back here.
            if (targets.Count == 2 && RND_RANGE(0, 1) == 0 && TryShowChatGreeting())
            {
                return;
            }

            // General greeting: the primary Om Nom waves.
            targetAnimationController?.PlayGreeting();
            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterGreeting, targetAnimationController?.SkinDefinition);
            if (SpecialEvents.IsXmas && Preferences.GetIntForKey("PREFS_SELECTED_OMNOM") == 0)
            {
                CTRSoundMgr.PlaySound(Resources.Snd.XmasBell);
            }
        }

        /// <summary>
        /// Rolls for the two-Om-Nom chat greeting as a random idle reaction. Called when an
        /// Om Nom's idle timer fires on an eligible level. When it starts a chat, both Om Noms'
        /// idle timers are reset so the reaction is not re-triggered mid-hand-off.
        /// </summary>
        /// <returns><see langword="true"/> when a chat greeting was started; otherwise, <see langword="false"/>.</returns>
        private bool TryStartChatReaction()
        {
            if (chatReactionActive
                || targets.Count != 2
                || targets[0].asleep
                || targets[1].asleep
                || RND_RANGE(0, ChatReactionOdds - 1) != 0)
            {
                return false;
            }

            if (!TryShowChatGreeting())
            {
                return false;
            }

            targets[0].idlesTimer = RND_RANGE(5, 20);
            targets[1].idlesTimer = RND_RANGE(5, 20);
            return true;
        }

        /// <summary>
        /// Makes the two Om Noms turn their heads toward each other. A randomly chosen Om Nom
        /// turns first; the other follows a fixed 1.0s later, matching Time Travel's chat
        /// hand-off. Each Om Nom's direction is auto-detected from position; diagonal or
        /// coincident pairs are skipped.
        /// </summary>
        /// <returns><see langword="true"/> when a chat greeting was started; otherwise, <see langword="false"/>.</returns>
        private bool TryShowChatGreeting()
        {
            if (targets.Count != 2 || targets[0].targetObject == null || targets[1].targetObject == null)
            {
                return false;
            }

            (TargetAnimationState first, TargetAnimationState second)? states = ChatGreeting.ResolveStates(
                targets[0].targetObject.x, targets[0].targetObject.y,
                targets[1].targetObject.x, targets[1].targetObject.y);
            if (!states.HasValue)
            {
                return false;
            }

            // Each Om Nom's direction is fixed by position; only the order is randomized.
            int firstIndex = RND_RANGE(0, 1);
            int secondIndex = 1 - firstIndex;
            TargetAnimationState firstState = firstIndex == 0 ? states.Value.first : states.Value.second;
            pendingChatGreetState = secondIndex == 0 ? states.Value.first : states.Value.second;
            pendingChatGreetIndex = secondIndex;
            chatReactionActive = true;

            // Initiator turns now; the other follows a fixed beat later.
            targets[firstIndex].controller?.PlayGreetingTurn(firstState);
            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterGreeting, targets[firstIndex].controller?.SkinDefinition);

            dd.CallObjectSelectorParamafterDelay(
                new DelayedDispatcher.DispatchFunc(Selector_showSecondChatGreeting), null, ChatGreetingGapSeconds);
            return true;
        }

        /// <summary>
        /// Timeline selector callback that plays the second Om Nom's chat greeting one
        /// fixed beat after the first.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_showSecondChatGreeting(FrameworkTypes param)
        {
            chatReactionActive = false;

            if (targets.Count != 2 || pendingChatGreetIndex >= targets.Count)
            {
                return;
            }

            TargetContext second = targets[pendingChatGreetIndex];
            if (second?.targetObject == null)
            {
                return;
            }

            second.controller?.PlayGreetingTurn(pendingChatGreetState);
            CTRSoundMgr.PlayOmNomSound(Resources.Snd.MonsterGreeting, second.controller?.SkinDefinition);
        }

        /// <inheritdoc />
        public override void Hide()
        {
            if (gravityButton != null)
            {
                RemoveChild(gravityButton);
            }
            if (waterLayer != null)
            {
                waterLayer.PrepareToRelease();
                waterLayer.Dispose();
                waterLayer = null;
            }
            candyL = null;
            candyR = null;
            starL = null;
            starR = null;
            Lantern.RemoveAllLanterns();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dd?.Dispose();
                dd = null;
                camera?.Dispose();
                camera = null;
                back?.Dispose();
                back = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Updates scene layout when fullscreen mode changes.
        /// </summary>
        /// <param name="isFullscreen">Whether fullscreen mode is enabled.</param>
        public void FullscreenToggled(bool isFullscreen)
        {
            _ = isFullscreen;
            BaseElement childWithName = staticAniPool.GetChildWithName("levelLabel");
            _ = (childWithName?.x = 15f + Canvas.xOffsetScaled);
            for (int i = 0; i < 3; i++)
            {
                int starSize = hudStar[i].width;
                hudStar[i].x = (starSize * i) + (starSize / 2) + Canvas.xOffsetScaled;
            }
            UpdateBackgroundScale();
        }

        /// <summary>
        /// Computes a width-based scale so a background texture matches the internal screen width.
        /// </summary>
        /// <param name="texture">Background texture to measure.</param>
        /// <returns>A safe width scale for the background texture.</returns>
        private static float GetBackgroundWidthScale(CTRTexture2D texture)
        {
            if (texture == null || texture._realWidth <= 0)
            {
                return 1f;
            }

            float scale = SCREEN_WIDTH / texture._realWidth;
            return scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale) ? 1f : scale;
        }

        /// <summary>
        /// Updates background scaling using the internal resolution.
        /// </summary>
        private void UpdateBackgroundScale()
        {
            // Keep backgrounds aligned to internal width
            backgroundScale = GetBackgroundWidthScale(backTexture);
            if (back != null)
            {
                back.scaleX = backgroundScale;
                back.scaleY = backgroundScale;
            }
        }

        /// <summary>
        /// Timeline selector callback that transitions the scene to the lost state.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_gameLost(FrameworkTypes param)
        {
            GameLost();
        }

        /// <summary>
        /// Timeline selector callback that transitions the scene to the won state.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_gameWon(FrameworkTypes param)
        {
            CTRSoundMgr.EnableLoopedSounds(false);
            outcomeTransitionActive = false;
            gameSceneDelegate?.GameWon();
        }

        /// <summary>
        /// Timeline selector callback that begins the level restart animation.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_animateLevelRestart(FrameworkTypes param)
        {
            AnimateLevelRestart();
        }

        /// <summary>
        /// Timeline selector callback that triggers the greeting animation and sound.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_showGreeting(FrameworkTypes param)
        {
            ShowGreeting();
        }

        /// <summary>
        /// Timeline selector callback that starts the candy blink animation.
        /// </summary>
        /// <param name="param">Unused timeline payload.</param>
        private void Selector_doCandyBlink(FrameworkTypes param)
        {
            DoCandyBlink();
        }

        /// <summary>
        /// Timeline selector callback that teleports the active object.
        /// </summary>
        /// <param name="param">Candy physics point payload.</param>
        private void Selector_teleport(FrameworkTypes param)
        {
            Teleport(param is ConstraintedPoint p ? CandyForPoint(p) : candies[0]);
        }

        /// <summary>
        /// Restores the candy transform and color after temporary visual effects (singleton = candies[0]).
        /// </summary>
        private void RestoreCandyProperties()
        {
            RestoreCandyProperties(candies[0]);
        }

        /// <summary>
        /// Restores a specific candy's transform and color after temporary visual effects.
        /// </summary>
        private static void RestoreCandyProperties(CandyContext ctx)
        {
            ctx.candy.passTransformationsToChilds = false;
            foreach (BaseElement visual in ctx.HandCatchVisuals())
            {
                visual.scaleX = visual.scaleY = ctx.HandCatchScale;
            }
            ctx.candy.color = RGBAColor.solidOpaqueRGBA;
        }

        /// <summary>
        /// Resolves the candy whose physics point is <paramref name="point"/>; falls back to candies[0].
        /// </summary>
        private CandyContext CandyForPoint(ConstraintedPoint point)
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (candies[i].point == point)
                {
                    return candies[i];
                }
            }

            return candies[0];
        }

        /// <summary>
        /// Timeline selector callback that restores the candy after it leaves a lantern.
        /// </summary>
        /// <param name="param">Released candy physics point.</param>
        private void Selector_revealCandyFromLantern(FrameworkTypes param)
        {
            ConstraintedPoint releasedPoint = param as ConstraintedPoint;
            _ = LanternRelease.RestoreReleasedCandy(candies, releasedPoint);
        }

        /// <summary>
        /// Normalizes an angle into the range of negative PI to positive PI.
        /// </summary>
        /// <param name="a">The angle to normalize.</param>
        /// <returns>The normalized angle.</returns>
        public static float FBOUND_PI(float a)
        {
            return a > MathF.PI ? a - MathF.Tau : a < -MathF.PI ? a + MathF.Tau : a;
        }

        /// <summary>
        /// Handles exhaustion of a rocket currently carrying the candy.
        /// </summary>
        /// <param name="r">The rocket that has exhausted its fuel.</param>
        public void Exhausted(Rocket r)
        {
            CandyContext ctx = RocketBoundCandy(r);
            if (ctx != null)
            {
                ctx.activeRocket = null;
                ctx.point.disableGravity = false;
            }
        }

        /// <summary>
        /// Chooses the equivalent source angle nearest to a target angle.
        /// </summary>
        /// <param name="ta">The target angle to compare against.</param>
        /// <param name="fa">The source angle to normalize near the target.</param>
        /// <returns>The source angle adjusted by full rotations to be closest to the target angle.</returns>
        private static float NearestAngleTofrom(float ta, float fa)
        {
            float minus360 = fa - DEG_360;
            float plus360 = fa + DEG_360;
            return MathF.Abs(fa - ta) < MathF.Abs(minus360 - ta) && MathF.Abs(fa - ta) < MathF.Abs(plus360 - ta)
                ? fa
                : MathF.Abs(minus360 - ta) < MathF.Abs(plus360 - ta) ? minus360 : NearestAngleTofrom(ta, plus360);
        }

        /// <summary>
        /// Calculates the smallest angular difference between two angles.
        /// </summary>
        /// <param name="a">The first angle.</param>
        /// <param name="b">The second angle.</param>
        /// <returns>The absolute minimum angle between the two inputs.</returns>
        private static float MinAngleBetweenAandB(float a, float b)
        {
            float normalizedDelta;
            for (normalizedDelta = MathF.Abs(a - b); normalizedDelta > DEG_360; normalizedDelta -= DEG_360)
            {
            }
            normalizedDelta = MathF.Abs(normalizedDelta);
            if (normalizedDelta > DEG_180)
            {
                normalizedDelta -= DEG_360;
            }
            return MathF.Abs(normalizedDelta);
        }

        /// <summary>
        /// The maximum number of concurrent touches tracked by the scene.
        /// </summary>
        public const int MAX_TOUCHES = 5;

        /// <summary>
        /// The delay before the restart dim animation advances.
        /// </summary>
        public const float DIM_TIMEOUT = 0.15f;

        /// <summary>
        /// Restart state value for fading the dim overlay in.
        /// </summary>
        public const int RESTART_STATE_FADE_IN = 0;

        /// <summary>
        /// Restart state value for fading the dim overlay out.
        /// </summary>
        public const int RESTART_STATE_FADE_OUT = 1;

        /// <summary>
        /// Selector state for moving an element downward.
        /// </summary>
        public const int S_MOVE_DOWN = 0;

        /// <summary>
        /// Selector state for waiting between movements.
        /// </summary>
        public const int S_WAIT = 1;

        /// <summary>
        /// Selector state for moving an element upward.
        /// </summary>
        public const int S_MOVE_UP = 2;

        /// <summary>
        /// Camera mode that tracks a split candy half.
        /// </summary>
        public const int CAMERA_MOVE_TO_CANDY_PART = 0;

        /// <summary>
        /// Camera mode that tracks the full candy body.
        /// </summary>
        public const int CAMERA_MOVE_TO_CANDY = 1;

        /// <summary>
        /// Button identifier for the gravity toggle.
        /// </summary>
        public const int BUTTON_GRAVITY = 0;

        /// <summary>
        /// Split-candy mode where both candy parts move independently.
        /// </summary>
        public const int PARTS_SEPARATE = 0;

        /// <summary>
        /// Split-candy mode where the part distance is animated.
        /// </summary>
        public const int PARTS_DIST = 1;

        /// <summary>
        /// Split-candy mode where no extra part behavior is active.
        /// </summary>
        public const int PARTS_NONE = 2;

        /// <summary>
        /// The timeout window for combo rope cuts.
        /// </summary>
        public const float SCOMBO_TIMEOUT = 0.2f;

        /// <summary>
        /// The score awarded for a rope cut.
        /// </summary>
        public const int SCUT_SCORE = 10;

        /// <summary>
        /// The maximum number of candy losses allowed before game over handling changes.
        /// </summary>
        public const int MAX_LOST_CANDIES = 3;

        /// <summary>
        /// The time window for counting multiple ropes cut at once.
        /// </summary>
        public const float ROPE_CUT_AT_ONCE_TIMEOUT = 0.1f;

        /// <summary>
        /// The pickup radius used for stars.
        /// </summary>
        public const int STAR_RADIUS = 42;

        /// <summary>
        /// The radius around Om Nom that triggers mouth-open behavior.
        /// </summary>
        public const float MOUTH_OPEN_RADIUS = 200f;

        /// <summary>
        /// The frame skip interval applied to blink timing.
        /// </summary>
        public const int BLINK_SKIP = 3;

        /// <summary>
        /// The duration to keep the mouth-open state active.
        /// </summary>
        public const float MOUTH_OPEN_TIME = 1f;

        /// <summary>
        /// The minimum delay between pump activations.
        /// </summary>
        public const float PUMP_TIMEOUT = 0.05f;

        /// <summary>
        /// The default camera follow speed.
        /// </summary>
        public const int CAMERA_SPEED = 14;

        /// <summary>
        /// The speed multiplier applied to moving socks.
        /// </summary>
        public const float SOCK_SPEED_K = 0.9f;

        /// <summary>
        /// The vertical offset used when testing sock collisions.
        /// </summary>
        public const int SOCK_COLLISION_Y_OFFSET = 85;

        /// <summary>
        /// The interaction radius used for bubbles.
        /// </summary>
        public const int BUBBLE_RADIUS = 60;

        /// <summary>
        /// The interaction radius used for wheel objects.
        /// </summary>
        public const int WHEEL_RADIUS = 110;

        /// <summary>
        /// The movement radius used for grab interactions.
        /// </summary>
        public const int GRAB_MOVE_RADIUS = 65;

        /// <summary>
        /// The target-relative Y pivot ratio used by the night sleep pulse overlay.
        /// </summary>
        private const float SleepPulsePivotYRatio = 433f / 480f;

        /// <summary>
        /// The interval between looping night sleep sounds.
        /// </summary>
        private const float NightSleepSoundInterval = 4f;

        /// <summary>
        /// The interaction radius for remote-control objects.
        /// </summary>
        public const int RC_CONTROLLER_RADIUS = 90;

        /// <summary>
        /// Initial state for the candy blink sequence.
        /// </summary>
        public const int CANDY_BLINK_INITIAL = 0;

        /// <summary>
        /// State for blinking the candy star highlight.
        /// </summary>
        public const int CANDY_BLINK_STAR = 1;

        /// <summary>
        /// Tutorial animation state for showing the prompt.
        /// </summary>
        public const int TUTORIAL_SHOW_ANIM = 0;

        /// <summary>
        /// Tutorial animation state for hiding the prompt.
        /// </summary>
        public const int TUTORIAL_HIDE_ANIM = 1;

        /// <summary>
        /// Animation index for the normal earth state.
        /// </summary>
        public const int EARTH_NORMAL_ANIM = 0;

        /// <summary>
        /// Animation index for the inverted earth state.
        /// </summary>
        public const int EARTH_UPSIDEDOWN_ANIM = 1;

        /// <summary>
        /// Dispatcher used for delayed selector callbacks.
        /// </summary>
        private DelayedDispatcher dd;

        /// <summary>
        /// Delegate notified when the scene reaches win or loss conditions.
        /// </summary>
        public IGameSceneDelegate gameSceneDelegate;

        /// <summary>
        /// Animation pool for gameplay objects.
        /// </summary>
        private readonly AnimationsPool aniPool;

        /// <summary>
        /// Animation pool for particle effects.
        /// </summary>
        private readonly AnimationsPool particlesAniPool;

        /// <summary>
        /// Layer containing decals and other overlay visuals.
        /// </summary>
        private readonly BaseElement decalsLayer;

        /// <summary>
        /// Animation pool for static HUD and decoration objects.
        /// </summary>
        private readonly AnimationsPool staticAniPool;

        /// <summary>
        /// Pollen particle renderer used by applicable levels.
        /// </summary>
        private PollenDrawer pollenDrawer;

        /// <summary>
        /// Primary background tile map.
        /// </summary>
        private TileMap back;

        /// <summary>
        /// Primary background texture used for computing scale.
        /// </summary>
        private readonly CTRTexture2D backTexture;

        /// <summary>
        /// Cached background scale derived from internal screen width.
        /// </summary>
        private float backgroundScale = 1f;

        /// <summary>
        /// The active Om Nom gameplay object.
        /// </summary>
#pragma warning disable IDE1006
        private GameObject targetObject => targets.Count > 0 ? targets[0].targetObject : null;

        /// <summary>
        /// Controller for Om Nom animation state transitions.
        /// </summary>
        private TargetAnimationController targetAnimationController => targets.Count > 0 ? targets[0].controller : null;
#pragma warning restore IDE1006

        /// <summary>
        /// Support visual attached to certain level setups.
        /// </summary>
        private Image support;

        /// <summary>
        /// The main candy gameplay object.
        /// </summary>
#pragma warning disable IDE1006
        private GameObject candy => candies[0].candy;

        /// <summary>
        /// The base candy sprite for split or layered visuals.
        /// </summary>
        private GameObject candyMain => candies[0].candyMain;

        /// <summary>
        /// The top candy sprite for split or layered visuals.
        /// </summary>
        private GameObject candyTop => candies[0].candyTop;

        /// <summary>
        /// Animation used for the candy blink effect.
        /// </summary>
        private Animation candyBlink => candies[0].candyBlink;

        /// <summary>
        /// Animation used for the main candy bubble effect.
        /// </summary>
        private Animation candyBubbleAnimation;

        /// <summary>
        /// Animation used for the left split candy bubble effect.
        /// </summary>
        private Animation candyBubbleAnimationL;

        /// <summary>
        /// Animation used for the right split candy bubble effect.
        /// </summary>
        private Animation candyBubbleAnimationR;

        /// <summary>
        /// Ghost bubble animation for the main candy.
        /// </summary>
        private CandyInGhostBubbleAnimation candyGhostBubbleAnimation;

        /// <summary>
        /// Ghost bubble animation for the left candy half.
        /// </summary>
        private CandyInGhostBubbleAnimation candyGhostBubbleAnimationL;

        /// <summary>
        /// Ghost bubble animation for the right candy half.
        /// </summary>
        private CandyInGhostBubbleAnimation candyGhostBubbleAnimationR;

        /// <summary>
        /// The constrained point currently representing the candy anchor.
        /// </summary>
        private ConstraintedPoint star => candies[0].point;
#pragma warning restore IDE1006

        /// <summary>All independent candies in the level. Single-candy packs hold one element.</summary>
        private readonly List<CandyContext> candies = [];

        /// <summary>All Om Noms in the level. Single-target packs hold one element.</summary>
#pragma warning disable IDE0052
        private readonly List<TargetContext> targets = [];
#pragma warning restore IDE0052

        /// <summary>True once the first &lt;candy&gt; element has claimed the pre-built primary candy (candies[0]).</summary>
        private bool primaryCandyClaimed;

        /// <summary>
        /// All active grab/bungee objects in the loaded level.
        /// </summary>
        private List<Grab> bungees;

        /// <summary>The elastic rope joining the two candies in a candiesConnected level, or null.</summary>
        private Bungee candyConnector;

        /// <summary>Whether the level joins its two candies with the connecting elastic.</summary>
        private bool candiesConnected;

        /// <summary>Rest/limit length of the connecting elastic, already scaled.</summary>
        private float candiesConnectedLength;

        /// <summary>Fixed delay between the first and second Om Nom in a two-Om-Nom chat greeting (Time Travel hand-off).</summary>
        private const float ChatGreetingGapSeconds = 1.0f;

        /// <summary>
        /// One-in-N odds that an idle reaction on a two-Om-Nom level becomes a chat greeting.
        /// </summary>
        private const int ChatReactionOdds = 3;

        /// <summary>Greet state queued for the second Om Nom in a staggered two-Om-Nom chat greeting.</summary>
        private TargetAnimationState pendingChatGreetState;

        /// <summary>Target index queued to greet second in a staggered two-Om-Nom chat greeting.</summary>
        private int pendingChatGreetIndex;

        /// <summary>Whether a two-Om-Nom chat greeting hand-off is currently in progress.</summary>
        private bool chatReactionActive;

        /// <summary>
        /// All active razor objects in the loaded level.
        /// </summary>
        private List<Razor> razors;

        /// <summary>
        /// All active spike objects in the loaded level.
        /// </summary>
        private List<Spikes> spikes;

        /// <summary>
        /// All collectible stars in the loaded level.
        /// </summary>
        private List<Star> stars;

        /// <summary>
        /// All active bubble objects in the loaded level.
        /// </summary>
        private List<Bubble> bubbles;

        /// <summary>
        /// All active pump objects in the loaded level.
        /// </summary>
        private List<Pump> pumps;

        /// <summary>
        /// All active steam tube objects in the loaded level.
        /// </summary>
        private List<SteamTube> tubes;

        /// <summary>
        /// All active bamboo tube objects in the loaded level.
        /// </summary>
        private List<BambooTube> bambooTubes;

        /// <summary>
        /// All active sock objects in the loaded level.
        /// </summary>
        private List<Sock> socks;

        /// <summary>
        /// All active bouncer objects in the loaded level.
        /// </summary>
        private List<Bouncer> bouncers;

        /// <summary>
        /// All active rotated circle objects in the loaded level.
        /// </summary>
        private List<RotatedCircle> rotatedCircles;

        /// <summary>
        /// All active rocket objects in the loaded level.
        /// </summary>
        private List<Rocket> rockets;

        /// <summary>
        /// All active mechanical hand objects in the loaded level.
        /// </summary>
        private List<MechanicalHand> hands;

        /// <summary>
        /// All active snail objects in the loaded level.
        /// </summary>
        private List<Snail> snailobjects;

        /// <summary>
        /// All tutorial image objects attached to the scene.
        /// </summary>
        private List<CTRGameObject> tutorialImages;

        /// <summary>
        /// All tutorial text labels attached to the scene.
        /// </summary>
        private List<Text> tutorials;

        /// <summary>
        /// All active ghost objects in the loaded level.
        /// </summary>
        private List<Ghost> ghosts;

        /// <summary>
        /// All active mouse objects in the loaded level.
        /// </summary>
        private List<Mouse> mice;

        /// <summary>
        /// Manager for composite mouse interactions.
        /// </summary>
        private MiceObject miceManager;

        /// <summary>
        /// Manager for conveyor belt interactions.
        /// </summary>
        private ConveyorBeltObject conveyors;

        /// <summary>
        /// All ant path segments in the current level.
        /// </summary>
        private List<AntsPathSegment> antsPathsSegments;

        /// <summary>
        /// All ant paths in the current level.
        /// </summary>
        private List<AntsPath> antsPaths;

        /// <summary>
        /// The left candy half when split mode is active.
        /// </summary>
        private GameObject candyL;

        /// <summary>
        /// The right candy half when split mode is active.
        /// </summary>
        private GameObject candyR;

        /// <summary>
        /// The left constrained point when split mode is active.
        /// </summary>
        private ConstraintedPoint starL;

        /// <summary>
        /// The right constrained point when split mode is active.
        /// </summary>
        private ConstraintedPoint starR;

        /// <summary>
        /// The default horizontal scale applied to the Om Nom target.
        /// </summary>
        private float targetBaseScaleX = 1f;

        /// <summary>
        /// The default vertical scale applied to the Om Nom target.
        /// </summary>
        private float targetBaseScaleY = 1f;

        /// <summary>
        /// Per-touch flags indicating which touches are currently dragging.
        /// </summary>
        private readonly bool[] dragging = new bool[5];

        /// <summary>
        /// Starting positions for active touches.
        /// </summary>
        private readonly Vector[] startPos = new Vector[5];

        /// <summary>
        /// Previous starting positions for active touches.
        /// </summary>
        private readonly Vector[] prevStartPos = new Vector[5];

        /// <summary>
        /// Current rope physics time scale.
        /// </summary>
        private float ropePhysicsSpeed;

        /// <summary>
        /// Current water surface height.
        /// </summary>
        private float waterLevel;

        /// <summary>
        /// Current water movement speed.
        /// </summary>
        private float waterSpeed;

        /// <summary>
        /// Water simulation layer for underwater levels.
        /// </summary>
        private WaterElement waterLayer;

        /// <summary>
        /// The primary candy's bubble gameplay object. Sealed onto <c>candies[0]</c> so the primary
        /// candy's bubble is stored like every other candy's (the split-half bubbles
        /// <see cref="candyBubbleL"/>/<see cref="candyBubbleR"/> stay separate).
        /// </summary>
#pragma warning disable IDE1006
        private GameObject candyBubble
        {
            get => candies[0].bubble;
            set => candies[0].bubble = value;
        }
#pragma warning restore IDE1006

        /// <summary>
        /// The left split candy bubble gameplay object.
        /// </summary>
        private GameObject candyBubbleL;

        /// <summary>
        /// The right split candy bubble gameplay object.
        /// </summary>
        private GameObject candyBubbleR;

        /// <summary>
        /// The HUD star animations that show collected stars.
        /// </summary>
        private readonly Animation[] hudStar = new Animation[3];

        /// <summary>
        /// The gameplay camera.
        /// </summary>
        private Camera2D camera;

        /// <summary>
        /// The width of the loaded map in world units.
        /// </summary>
        private float mapWidth;

        /// <summary>
        /// The height of the loaded map in world units.
        /// </summary>
        private float mapHeight;

        /// <summary>
        /// The X origin of the loaded map.
        /// </summary>
        private float mapOriginX;

        /// <summary>
        /// The Y origin of the loaded map.
        /// </summary>
        private float mapOriginY;

        /// <summary>
        /// Whether the main candy has been removed from play.
        /// </summary>
        private bool noCandy;

        /// <summary>
        /// Cached rotation delta for the main candy.
        /// </summary>
        private float lastCandyRotateDelta;

        /// <summary>
        /// Cached rotation delta for the left candy half.
        /// </summary>
        private float lastCandyRotateDeltaL;

        /// <summary>
        /// Cached rotation delta for the right candy half.
        /// </summary>
        private float lastCandyRotateDeltaR;

        // private bool spiderTookCandy;

        /// <summary>
        /// Special-case level behavior flag.
        /// </summary>
        private int special;

        /// <summary>
        /// Whether the camera should remain locked to its current target.
        /// </summary>
        private bool fastenCamera;

        /// <summary>
        /// Whether the main ghost bubble animation has been loaded.
        /// </summary>
        private bool isCandyInGhostBubbleAnimationLoaded;

        /// <summary>
        /// Whether the left ghost bubble animation has been loaded.
        /// </summary>
        private bool isCandyInGhostBubbleAnimationLeftLoaded;

        /// <summary>
        /// Whether the right ghost bubble animation has been loaded.
        /// </summary>
        private bool isCandyInGhostBubbleAnimationRightLoaded;

        /// <summary>
        /// Whether the second ghost should be restored after a transition.
        /// </summary>
        private bool shouldRestoreSecondGhost;

        /// <summary>
        /// The number of ropes cut within the active combo window.
        /// </summary>
        private int ropesCutAtOnce;

        /// <summary>
        /// Remaining time in the active rope-cut combo window.
        /// </summary>
        private float ropeAtOnceTimer;

        /// <summary>
        /// Whether click-to-cut input mode is enabled.
        /// </summary>
        private readonly bool clickToCut;

        /// <summary>
        /// The number of stars collected in the level.
        /// </summary>
        public int starsCollected;

        /// <summary>
        /// The score bonus awarded from collected stars.
        /// </summary>
        public int starBonus;

        /// <summary>
        /// The score bonus awarded from remaining time.
        /// </summary>
        public int timeBonus;

        /// <summary>
        /// The current total score.
        /// </summary>
        public int score;

        /// <summary>
        /// The elapsed level time.
        /// </summary>
        public float time;

        /// <summary>
        /// The initial camera distance to the candy anchor.
        /// </summary>
        public float initialCameraToStarDistance;

        /// <summary>
        /// The current dim animation timer.
        /// </summary>
        public float dimTime;

        /// <summary>
        /// The current restart animation state.
        /// </summary>
        public int restartState;

        /// <summary>
        /// Whether restart should animate through the dim overlay.
        /// </summary>
        public bool animateRestartDim;

        /// <summary>
        /// Whether camera motion is temporarily frozen.
        /// </summary>
        public bool freezeCamera;

        /// <summary>
        /// The current camera follow mode.
        /// </summary>
        public int cameraMoveMode;

        /// <summary>
        /// Whether touch input is temporarily ignored.
        /// </summary>
        public bool ignoreTouches;

        /// <summary>
        /// Whether the loaded level uses night-specific behavior.
        /// </summary>
        public bool nightLevel;

        /// <summary>
        /// Whether the game-lost state has already been triggered.
        /// </summary>
        public bool gameLostTriggered;

        /// <summary>
        /// Whether the game-won state has already been triggered (prevents the multi-candy win
        /// check from re-invoking GameWon every frame, which would cancel the pending win dispatch).
        /// </summary>
        public bool gameWonTriggered;

        /// <summary>
        /// Whether a game win/loss scene transition is currently active.
        /// </summary>
        public bool outcomeTransitionActive;

        /// <summary>
        /// Whether gravity is currently in the normal orientation.
        /// </summary>
        public bool gravityNormal;

        /// <summary>
        /// The UI button that toggles gravity orientation.
        /// </summary>
        public ToggleButton gravityButton;

        /// <summary>
        /// The touch index currently holding the gravity button, or an invalid marker.
        /// </summary>
        public int gravityTouchDown;

        /// <summary>
        /// The current split-candy state.
        /// </summary>
        public int twoParts;

        /// <summary>
        /// Whether the left candy half has been removed from play.
        /// </summary>
        public bool noCandyL;

        /// <summary>
        /// Whether the right candy half has been removed from play.
        /// </summary>
        public bool noCandyR;

        /// <summary>
        /// The current distance between split candy parts.
        /// </summary>
        public float partsDist;

        /// <summary>
        /// The X value for the global gravity.
        /// </summary>
        public float globalGravityX;

        /// <summary>
        /// The Y value for the global gravity.
        /// </summary>
        public float globalGravityY;

        /// <summary>
        /// Earth animation images used by gravity-switch levels.
        /// </summary>
        public List<Image> earthAnims;

        /// <summary>
        /// Optional string to load as a name for the level.
        /// </summary>
        public string levelName;

        /// <summary>
        /// Count of tummy teaser interactions completed in the scene.
        /// </summary>
        public int tummyTeasers;

        /// <summary>
        /// The last recorded touch position for scene-level gestures.
        /// </summary>
        public Vector slastTouch;

        /// <summary>
        /// Per-touch finger cut ribbons used for cut visualization.
        /// </summary>
        public List<FingerCut>[] fingerCuts = new List<FingerCut>[5];

        /// <summary>
        /// Per-touch finger trace effects.
        /// </summary>
        public FingerTrace[] fingerTraces = new FingerTrace[5];

        /// <summary>
        /// Touch-down positions used by the finger trace system.
        /// </summary>
        private readonly Vector[] fingerTraceDownPos = new Vector[5];

        /// <summary>
        /// Whether each touch is actively driving a finger trace drag.
        /// </summary>
        private readonly bool[] fingerTraceDragging = new bool[5];

        /// <summary>
        /// Represents one rendered cut segment between two touch positions.
        /// </summary>
        public sealed class FingerCut : FrameworkTypes
        {
            /// <summary>
            /// The world-space start position of the cut segment.
            /// </summary>
            public Vector start;

            /// <summary>
            /// The world-space end position of the cut segment.
            /// </summary>
            public Vector end;

            /// <summary>
            /// The rendered width at the start of the cut segment.
            /// </summary>
            public float startSize;

            /// <summary>
            /// The rendered width at the end of the cut segment.
            /// </summary>
            public float endSize;

            /// <summary>
            /// The color used to render the cut segment.
            /// </summary>
            public RGBAColor c;
        }

        // private sealed class SCandy : ConstraintedPoint
        // {
        // public bool good;

        // public float speed;

        // public float angle;

        // public float lastAngleChange;
        // }

        /// <summary>
        /// Specialized tutorial text element that stores a behavior flag.
        /// </summary>
        private sealed class TutorialText : Text
        {
            /// <summary>
            /// Special-case behavior identifier for the tutorial text.
            /// </summary>
            public int special;
        }

        /// <summary>
        /// Specialized gameplay object that carries a behavior flag.
        /// </summary>
        private sealed class GameObjectSpecial : CTRGameObject
        {
            /// <summary>
            /// Creates a special game object from a texture.
            /// </summary>
            /// <param name="t">The texture assigned to the new object.</param>
            /// <returns>A new special game object initialized with the provided texture.</returns>
            private static GameObjectSpecial GameObjectSpecial_create(CTRTexture2D t)
            {
                GameObjectSpecial gameObjectSpecial = new();
                _ = gameObjectSpecial.InitWithTexture(t);
                return gameObjectSpecial;
            }

            /// <summary>
            /// Creates a special game object from a resource texture and draw quad.
            /// </summary>
            /// <param name="resourceName">The texture resource identifier.</param>
            /// <param name="q">The draw quad index to use.</param>
            /// <returns>A new special game object configured for the requested texture quad.</returns>
            public static GameObjectSpecial GameObjectSpecial_createWithResIDQuad(string resourceName, int q)
            {
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial_create(Application.GetTexture(resourceName));
                gameObjectSpecial.SetDrawQuad(q);
                return gameObjectSpecial;
            }

            /// <summary>
            /// Special-case behavior identifier for this object.
            /// </summary>
            public int special;
        }
    }
}
