using System.Xml.Linq;

using CutTheRope.desktop;
using CutTheRope.Helpers;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
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
            return p.pos.y > mapHeight + 400f || p.pos.y < -400f;
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
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 1)
            {
                return true;
            }
            string @string = Application.SharedAppSettings().GetString(8);
            string nSString = c.AttributeAsNSString("locale");
            if (@string.IsEqualToString("en") || @string.IsEqualToString("ru") || @string.IsEqualToString("de") || @string.IsEqualToString("fr"))
            {
                if (!nSString.IsEqualToString(@string))
                {
                    return true;
                }
            }
            else if (!nSString.IsEqualToString("en"))
            {
                return true;
            }
            return false;
        }

        public void ShowGreeting()
        {
            target.PlayAnimationtimeline(101, 10);
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
            BaseElement childWithName = staticAniPool.GetChildWithName("levelLabel");
            if (childWithName != null)
            {
                childWithName.x = 15f + Canvas.xOffsetScaled;
            }
            for (int i = 0; i < 3; i++)
            {
                hudStar[i].x = (hudStar[i].width * i) + Canvas.xOffsetScaled;
            }
            if (isFullscreen)
            {
                float num = Global.ScreenSizeManager.ScreenWidth;
                back.scaleX = num / Canvas.backingWidth * 1.25f;
                return;
            }
            back.scaleX = 1.25f;
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

        private readonly AnimationsPool staticAniPool;

        private PollenDrawer pollenDrawer;

        private TileMap back;

        private CharAnimations target;

        private Image support;

        private GameObject candy;

        private GameObject candyMain;

        private GameObject candyTop;

        private Animation candyBlink;

        private Animation candyBubbleAnimation;

        private Animation candyBubbleAnimationL;

        private Animation candyBubbleAnimationR;

        private ConstraintedPoint star;

        private DynamicArray<Grab> bungees;

        private DynamicArray<Razor> razors;

        private DynamicArray<Spikes> spikes;

        private DynamicArray<Star> stars;

        private DynamicArray<Bubble> bubbles;

        private DynamicArray<Pump> pumps;

        private DynamicArray<Sock> socks;

        private DynamicArray<Bouncer> bouncers;

        private DynamicArray<RotatedCircle> rotatedCircles;

        private DynamicArray<CTRGameObject> tutorialImages;

        private DynamicArray<Text> tutorials;

        private GameObject candyL;

        private GameObject candyR;

        private ConstraintedPoint starL;

        private ConstraintedPoint starR;

        private Animation blink;

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

        public bool gravityNormal;

        public ToggleButton gravityButton;

        public int gravityTouchDown;

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

        private sealed class SCandy : ConstraintedPoint
        {
            public bool good;

            public float speed;

            public float angle;

            public float lastAngleChange;
        }

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
                GameObjectSpecial gameObjectSpecial = GameObjectSpecial_create(Application.GetTexture(r));
                gameObjectSpecial.SetDrawQuad(q);
                return gameObjectSpecial;
            }

            public int special;
        }
    }
}
