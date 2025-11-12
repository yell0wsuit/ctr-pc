using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CutTheRope.game
{
    internal sealed partial class GameScene : BaseElement, ITimelineDelegate, IButtonDelegation
    {
        private static void DrawCut(Vector fls, Vector frs, Vector start, Vector end, float startSize, float endSize, RGBAColor c, ref Vector le, ref Vector re)
        {
            Vector vector5 = VectNormalize(VectSub(end, start));
            Vector v3 = VectRperp(vector5);
            Vector v4 = VectPerp(vector5);
            Vector vector = VectEqual(frs, vectUndefined) ? VectAdd(start, VectMult(v3, startSize)) : frs;
            Vector vector2 = VectEqual(fls, vectUndefined) ? VectAdd(start, VectMult(v4, startSize)) : fls;
            Vector vector3 = VectAdd(end, VectMult(v3, endSize));
            Vector vector4 = VectAdd(end, VectMult(v4, endSize));
            GLDrawer.DrawSolidPolygonWOBorder([vector2.x, vector2.y, vector.x, vector.y, vector3.x, vector3.y, vector4.x, vector4.y], 4, c);
            le = vector4;
            re = vector3;
        }

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

        public void XmlLoaderFinishedWithfromwithSuccess(XMLNode rootNode, NSString url, bool success)
        {
            ((CTRRootController)Application.SharedRootController()).SetMap(rootNode);
            if (animateRestartDim)
            {
                AnimateLevelRestart();
                return;
            }
            Restart();
        }

        public static bool ShouldSkipTutorialElement(XMLNode c)
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (cTRRootController.GetPack() == 0 && cTRRootController.GetLevel() == 1)
            {
                return true;
            }
            NSString @string = Application.SharedAppSettings().GetString(8);
            NSString nSString = c["locale"];
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
            pollenDrawer.Release();
            earthAnims?.Release();
            candy.Release();
            star.Release();
            candyL?.Release();
            candyR?.Release();
            starL.Release();
            starR.Release();
            razors.Release();
            spikes.Release();
            bungees.Release();
            stars.Release();
            bubbles.Release();
            pumps.Release();
            socks.Release();
            bouncers.Release();
            rotatedCircles.Release();
            target.Release();
            support.Release();
            tutorialImages.Release();
            tutorials.Release();
            candyL = null;
            candyR = null;
            starL = null;
            starR = null;
        }

        public override void Dealloc()
        {
            for (int i = 0; i < 5; i++)
            {
                fingerCuts[i].Release();
            }
            dd.Release();
            camera.Release();
            back.Release();
            base.Dealloc();
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

        private void Selector_gameLost(NSObject param)
        {
            GameLost();
        }

        private void Selector_gameWon(NSObject param)
        {
            CTRSoundMgr.EnableLoopedSounds(false);
            gameSceneDelegate?.GameWon();
        }

        private void Selector_animateLevelRestart(NSObject param)
        {
            AnimateLevelRestart();
        }

        private void Selector_showGreeting(NSObject param)
        {
            ShowGreeting();
        }

        private void Selector_doCandyBlink(NSObject param)
        {
            DoCandyBlink();
        }

        private void Selector_teleport(NSObject param)
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

        private const int CHAR_ANIMATION_IDLE = 0;

        private const int CHAR_ANIMATION_IDLE2 = 1;

        private const int CHAR_ANIMATION_IDLE3 = 2;

        private const int CHAR_ANIMATION_EXCITED = 3;

        private const int CHAR_ANIMATION_PUZZLED = 4;

        private const int CHAR_ANIMATION_FAIL = 5;

        private const int CHAR_ANIMATION_WIN = 6;

        private const int CHAR_ANIMATION_MOUTH_OPEN = 7;

        private const int CHAR_ANIMATION_MOUTH_CLOSE = 8;

        private const int CHAR_ANIMATION_CHEW = 9;

        private const int CHAR_ANIMATION_GREETING = 10;

        private DelayedDispatcher dd;

        public IGameSceneDelegate gameSceneDelegate;

        private AnimationsPool aniPool;

        private AnimationsPool staticAniPool;

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

        private DynamicArray bungees;

        private DynamicArray razors;

        private DynamicArray spikes;

        private DynamicArray stars;

        private DynamicArray bubbles;

        private DynamicArray pumps;

        private DynamicArray socks;

        private DynamicArray bouncers;

        private DynamicArray rotatedCircles;

        private DynamicArray tutorialImages;

        private DynamicArray tutorials;

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

        private bool spiderTookCandy;

        private int special;

        private bool fastenCamera;

        private float savedSockSpeed;

        private Sock targetSock;

        private int ropesCutAtOnce;

        private float ropeAtOnceTimer;

        private bool clickToCut;

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

        public DynamicArray earthAnims;

        public int tummyTeasers;

        public Vector slastTouch;

        public DynamicArray[] fingerCuts = new DynamicArray[5];

        private sealed class FingerCut : NSObject
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
