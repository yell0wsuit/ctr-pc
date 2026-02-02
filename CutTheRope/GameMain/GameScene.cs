using System.Xml.Linq;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Sfe;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene : BaseElement, ITimelineDelegate, IButtonDelegation
    {
        private static float MaxOf4(float v1, float v2, float v3, float v4)
        {
            return v1 >= v2 && v1 >= v3 && v1 >= v4
                ? v1
                : v2 >= v1 && v2 >= v3 && v2 >= v4 ? v2 : v3 >= v2 && v3 >= v1 && v3 >= v4 ? v3 : v4 >= v2 && v4 >= v3 && v4 >= v1 ? v4 : -1f;
        }

        private static float MinOf4(float v1, float v2, float v3, float v4)
        {
            return v1 <= v2 && v1 <= v3 && v1 <= v4
                ? v1
                : v2 <= v1 && v2 <= v3 && v2 <= v4 ? v2 : v3 <= v2 && v3 <= v1 && v3 <= v4 ? v3 : v4 <= v2 && v4 <= v3 && v4 <= v1 ? v4 : -1f;
        }

        public bool PointOutOfScreen(ConstraintedPoint p)
        {
            return p.pos.Y > mapHeight + 400f || p.pos.Y < -400f;
        }

        public void XmlLoaderFinishedWithfromwithSuccess(XElement rootNode, string _, bool _1)
        {
            ((CTRRootController)Application.SharedRootController()).SetMap(rootNode);
            if (animateRestartDim)
            {
                AnimateLevelRestart();
                return;
            }
            Restart();
        }

        public static bool ShouldSkipTutorialElement(XElement c)
        {
            string currentLang = LanguageHelper.CurrentCode;
            string locale = c.AttributeAsNSString("locale");
            return LanguageHelper.IsUiLanguageCode(currentLang) ? !locale.IsEqualToString(currentLang) : !locale.IsEqualToString("en");
        }

        public void ShowGreeting()
        {
            if (SpecialEvents.IsXmas)
            {
                target.PlayAnimationtimeline(Resources.Img.CharGreetingXmas, 11);
                CTRSoundMgr.PlaySound(Resources.Snd.XmasBell);
            }
            else
            {
                target.PlayAnimationtimeline(Resources.Img.CharAnimations2, 10);
            }
        }

        public override void Hide()
        {
            if (gravityButton != null)
            {
                RemoveChild(gravityButton);
            }
            candyL = null;
            candyR = null;
            starL = null;
            starR = null;
            Lantern.RemoveAllLanterns();
            isCandyInLantern = false;
        }

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

        public void FullscreenToggled(bool isFullscreen)
        {
            _ = isFullscreen;
            BaseElement childWithName = staticAniPool.GetChildWithName("levelLabel");
            _ = (childWithName?.x = 15f + Canvas.xOffsetScaled);
            for (int i = 0; i < 3; i++)
            {
                hudStar[i].x = (hudStar[i].width * i) + Canvas.xOffsetScaled;
            }
            UpdateBackgroundScale();
        }

        /// <summary>
        /// Computes a width-based scale so a background texture matches the internal screen width.
        /// </summary>
        /// <param name="texture">Background texture to measure.</param>
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

        private void Selector_gameLost(FrameworkTypes param)
        {
            GameLost();
        }

        private void Selector_gameWon(FrameworkTypes param)
        {
            CTRSoundMgr.EnableLoopedSounds(false);
            gameSceneDelegate?.GameWon();
        }

        private void Selector_animateLevelRestart(FrameworkTypes param)
        {
            AnimateLevelRestart();
        }

        private void Selector_showGreeting(FrameworkTypes param)
        {
            ShowGreeting();
        }

        private void Selector_doCandyBlink(FrameworkTypes param)
        {
            DoCandyBlink();
        }

        private void Selector_teleport(FrameworkTypes param)
        {
            Teleport();
        }

        private void Selector_dropLightBulbFromSock(FrameworkTypes param)
        {
            if (param is LightBulb bulb)
            {
                DropLightBulbFromSock(bulb);
            }
        }

        private void Selector_revealCandyFromLantern(FrameworkTypes param)
        {
            isCandyInLantern = false;
            candy.color = RGBAColor.solidOpaqueRGBA;
            candy.passTransformationsToChilds = false;
            candy.scaleX = candy.scaleY = 0.71f;
            candyMain.scaleX = candyMain.scaleY = 0.71f;
            candyTop.scaleX = candyTop.scaleY = 0.71f;
        }

        public static float FBOUND_PI(float a)
        {
            return (float)((double)a > 3.141592653589793 ? (double)a - 6.283185307179586 : (double)a < -3.141592653589793 ? (double)a + 6.283185307179586 : (double)a);
        }

        public const int MAX_TOUCHES = 5;

        public const float DIM_TIMEOUT = 0.15f;

        public const int RESTART_STATE_FADE_IN = 0;

        public const int RESTART_STATE_FADE_OUT = 1;

        public const int S_MOVE_DOWN = 0;

        public const int S_WAIT = 1;

        public const int S_MOVE_UP = 2;

        public const int CAMERA_MOVE_TO_CANDY_PART = 0;

        public const int CAMERA_MOVE_TO_CANDY = 1;

        public const int BUTTON_GRAVITY = 0;

        public const int PARTS_SEPARATE = 0;

        public const int PARTS_DIST = 1;

        public const int PARTS_NONE = 2;

        public const float SCOMBO_TIMEOUT = 0.2f;

        public const int SCUT_SCORE = 10;

        public const int MAX_LOST_CANDIES = 3;

        public const float ROPE_CUT_AT_ONCE_TIMEOUT = 0.1f;

        public const int STAR_RADIUS = 42;

        public const float MOUTH_OPEN_RADIUS = 200f;

        public const int BLINK_SKIP = 3;

        public const float MOUTH_OPEN_TIME = 1f;

        public const float PUMP_TIMEOUT = 0.05f;

        public const int CAMERA_SPEED = 14;

        public const float SOCK_SPEED_K = 0.9f;

        public const int SOCK_COLLISION_Y_OFFSET = 85;

        public const int BUBBLE_RADIUS = 60;

        public const int WHEEL_RADIUS = 110;

        public const int GRAB_MOVE_RADIUS = 65;

        private const int CharAnimationSleeping = 15;

        private const int SleepAnimStart = 0;

        private const int SleepAnimEnd = 6;

        private const int SleepZzzStart = 7;

        private const int SleepZzzEnd = 43;

        private const float SleepAnimFrameDelay = 0.05f;

        private const float SleepPulsePivotYRatio = 433f / 480f;

        private const float NightSleepSoundInterval = 4f;

        private const int NightConstraintRelaxationSteps = 30;

        public const int RC_CONTROLLER_RADIUS = 90;

        public const int CANDY_BLINK_INITIAL = 0;

        public const int CANDY_BLINK_STAR = 1;

        public const int TUTORIAL_SHOW_ANIM = 0;

        public const int TUTORIAL_HIDE_ANIM = 1;

        public const int EARTH_NORMAL_ANIM = 0;

        public const int EARTH_UPSIDEDOWN_ANIM = 1;
        private DelayedDispatcher dd;

        public IGameSceneDelegate gameSceneDelegate;

        private readonly AnimationsPool aniPool;

        private readonly AnimationsPool kickStainsPool;

        private readonly AnimationsPool staticAniPool;

        private PollenDrawer pollenDrawer;

        private TileMap back;

        /// <summary>Primary background texture used for computing scale.</summary>
        private readonly CTRTexture2D backTexture;

        /// <summary>Cached background scale derived from internal screen width.</summary>
        private float backgroundScale = 1f;

        private CharAnimations target;

        private Image support;

        private GameObject candy;

        private GameObject candyMain;

        private GameObject candyTop;

        private Animation candyBlink;

        private Animation candyBubbleAnimation;

        private Animation candyBubbleAnimationL;

        private Animation candyBubbleAnimationR;

        private CandyInGhostBubbleAnimation candyGhostBubbleAnimation;

        private CandyInGhostBubbleAnimation candyGhostBubbleAnimationL;

        private CandyInGhostBubbleAnimation candyGhostBubbleAnimationR;

        private ConstraintedPoint star;

        private DynamicArray<Grab> bungees;

        private DynamicArray<Razor> razors;

        private DynamicArray<Spikes> spikes;

        private DynamicArray<Star> stars;

        private DynamicArray<Bubble> bubbles;

        private DynamicArray<Pump> pumps;

        private DynamicArray<SteamTube> tubes;

        private DynamicArray<LightBulb> lightBulbs;

        private DynamicArray<Sock> socks;

        private DynamicArray<Bouncer> bouncers;

        private DynamicArray<RotatedCircle> rotatedCircles;

        private DynamicArray<CTRGameObject> tutorialImages;

        private DynamicArray<Text> tutorials;

        private DynamicArray<Ghost> ghosts;

        private DynamicArray<Mouse> mice;

        private MiceObject miceManager;

        private ConveyorBeltObject conveyors;

        private GameObject candyL;

        private GameObject candyR;

        private ConstraintedPoint starL;

        private ConstraintedPoint starR;

        private Animation blink;

        private Animation sleepAnimPrimary;

        private Animation sleepAnimSecondary;

        private bool? isNightTargetAwake;

        private bool sleepPulseActive;

        private float sleepPulseTime;

        private float sleepPulseDelay;

        private float sleepPulseBaseY;

        private float sleepSoundTimer;

        private readonly bool[] dragging = new bool[5];

        private readonly Vector[] startPos = new Vector[5];

        private readonly Vector[] prevStartPos = new Vector[5];

        private float ropePhysicsSpeed;

        private GameObject candyBubble;

        private GameObject candyBubbleL;

        private GameObject candyBubbleR;

        private readonly Animation[] hudStar = new Animation[3];

        private Camera2D camera;

        private float mapWidth;

        private float mapHeight;

        private float mapOriginX;

        private float mapOriginY;

        private bool mouthOpen;

        private bool noCandy;

        private int blinkTimer;

        private int idlesTimer;

        private float mouthCloseTimer;

        private float lastCandyRotateDelta;

        private float lastCandyRotateDeltaL;

        private float lastCandyRotateDeltaR;

        // private bool spiderTookCandy;

        private int special;

        private bool fastenCamera;

        private bool isCandyInGhostBubbleAnimationLoaded;

        private bool isCandyInGhostBubbleAnimationLeftLoaded;

        private bool isCandyInGhostBubbleAnimationRightLoaded;

        private bool shouldRestoreSecondGhost;

        private float savedSockSpeed;

        private Sock targetSock;

        private int ropesCutAtOnce;

        private float ropeAtOnceTimer;

        private readonly bool clickToCut;

        public int starsCollected;

        public int starBonus;

        public int timeBonus;

        public int score;

        public float time;

        public float initialCameraToStarDistance;

        public float dimTime;

        public int restartState;

        public bool animateRestartDim;

        public bool freezeCamera;

        public int cameraMoveMode;

        public bool ignoreTouches;

        public bool nightLevel;

        public bool gameLostTriggered;

        public bool gravityNormal;

        public ToggleButton gravityButton;

        public int gravityTouchDown;

        private bool isCandyInLantern;

        public int twoParts;

        public bool noCandyL;

        public bool noCandyR;

        public float partsDist;

        public DynamicArray<Image> earthAnims;

        public int tummyTeasers;

        public Vector slastTouch;

        public DynamicArray<FingerCut>[] fingerCuts = new DynamicArray<FingerCut>[5];

        public sealed class FingerCut : FrameworkTypes
        {
            public Vector start;

            public Vector end;

            public float startSize;

            public float endSize;

            public RGBAColor c;
        }

        // private sealed class SCandy : ConstraintedPoint
        // {
        // public bool good;

        // public float speed;

        // public float angle;

        // public float lastAngleChange;
        // }

        private sealed class TutorialText : Text
        {
            public int special;
        }

        private sealed class GameObjectSpecial : CTRGameObject
        {
            private static GameObjectSpecial GameObjectSpecial_create(CTRTexture2D t)
            {
                GameObjectSpecial gameObjectSpecial = new();
                _ = gameObjectSpecial.InitWithTexture(t);
                return gameObjectSpecial;
            }

            public static GameObjectSpecial GameObjectSpecial_createWithResIDQuad(int r, int q)
            {
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
                gameObjectSpecial.SetDrawQuad(q);
                return gameObjectSpecial;
            }

            public static GameObjectSpecial GameObjectSpecial_createWithResIDQuad(string resourceName, int q)
            {
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial_create(Application.GetTexture(resourceName));
                gameObjectSpecial.SetDrawQuad(q);
                return gameObjectSpecial;
            }

            public int special;
        }
    }
}
