using CutTheRope.desktop;
using CutTheRope.Helpers;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed partial class GameScene
    {
        public static ToggleButton CreateGravityButtonWithDelegate(IButtonDelegation d)
        {
            Image u = Image.Image_createWithResIDQuad(78, 56);
            Image d2 = Image.Image_createWithResIDQuad(78, 56);
            Image u2 = Image.Image_createWithResIDQuad(78, 57);
            Image d3 = Image.Image_createWithResIDQuad(78, 57);
            ToggleButton toggleButton = new ToggleButton().InitWithUpElement1DownElement1UpElement2DownElement2andID(u, d2, u2, d3, 0);
            toggleButton.delegateButtonDelegate = d;
            return toggleButton;
        }

        public GameScene()
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            dd = new DelayedDispatcher();
            initialCameraToStarDistance = -1f;
            restartState = -1;
            aniPool = new AnimationsPool
            {
                visible = false
            };
            _ = AddChild(aniPool);
            staticAniPool = new AnimationsPool
            {
                visible = false
            };
            _ = AddChild(staticAniPool);
            camera = new Camera2D().InitWithSpeedandType(14f, CAMERATYPE.CAMERASPEEDDELAY);
            int textureResID = 104 + (cTRRootController.GetPack() * 2);
            back = new TileMap().InitWithRowsColumns(1, 1);
            back.SetRepeatHorizontally(TileMap.Repeat.NONE);
            back.SetRepeatVertically(TileMap.Repeat.ALL);
            back.AddTileQuadwithID(Application.GetTexture(textureResID), 0, 0);
            back.FillStartAtRowColumnRowsColumnswithTile(0, 0, 1, 1, 0);
            if (Canvas.isFullscreen)
            {
                back.scaleX = Global.ScreenSizeManager.ScreenWidth / (float)Canvas.backingWidth;
            }
            back.scaleX *= 1.25f;
            back.scaleY *= 1.25f;
            for (int i = 0; i < 3; i++)
            {
                hudStar[i] = Animation.Animation_createWithResID(79);
                hudStar[i].DoRestoreCutTransparency();
                _ = hudStar[i].AddAnimationDelayLoopFirstLast(0.05, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 10);
                hudStar[i].SetPauseAtIndexforAnimation(10, 0);
                hudStar[i].x = (hudStar[i].width * i) + Canvas.xOffsetScaled;
                hudStar[i].y = 0f;
                _ = AddChild(hudStar[i]);
            }
            for (int j = 0; j < 5; j++)
            {
                fingerCuts[j] = new DynamicArray<FingerCut>();
            }
            clickToCut = Preferences.GetBooleanForKey("PREFS_CLICK_TO_CUT");
        }

        public void Reload()
        {
            dd.CancelAllDispatches();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (cTRRootController.IsPicker())
            {
                XmlLoaderFinishedWithfromwithSuccess(XElementExtensions.LoadContentXml("mappicker://reload"), "mappicker://reload", true);
                return;
            }
            int pack = cTRRootController.GetPack();
            int level = cTRRootController.GetLevel();
            XmlLoaderFinishedWithfromwithSuccess(XElementExtensions.LoadContentXml("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), "maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString(), true);
        }

        public void LoadNextMap()
        {
            dd.CancelAllDispatches();
            initialCameraToStarDistance = -1f;
            animateRestartDim = false;
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (cTRRootController.IsPicker())
            {
                XmlLoaderFinishedWithfromwithSuccess(XElementExtensions.LoadContentXml("mappicker://next"), "mappicker://next", true);
                return;
            }
            int pack = cTRRootController.GetPack();
            int level = cTRRootController.GetLevel();
            if (level < CTRPreferences.GetLevelsInPackCount() - 1)
            {
                cTRRootController.SetLevel(++level);
                cTRRootController.SetMapName(LevelsList.LEVEL_NAMES[pack, level]);
                XmlLoaderFinishedWithfromwithSuccess(XElementExtensions.LoadContentXml("maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString()), "maps/" + LevelsList.LEVEL_NAMES[pack, level].ToString(), true);
            }
        }

        public void Restart()
        {
            Hide();
            Show();
        }

        public void CreateEarthImageWithOffsetXY(float xs, float ys)
        {
            Image image = Image.Image_createWithResIDQuad(78, 58);
            image.anchor = 18;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(180.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            image.AddTimelinewithID(timeline, 1);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(180.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0.0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3));
            image.AddTimelinewithID(timeline, 0);
            Image.SetElementPositionWithQuadOffset(image, 118, 1);
            if (Canvas.isFullscreen)
            {
                _ = Global.ScreenSizeManager.ScreenWidth;
            }
            image.scaleX = 0.8f;
            image.scaleY = 0.8f;
            image.x += xs;
            image.y += ys;
            _ = earthAnims.AddObject(image);
        }
    }
}
