using System.IO;
using System.Linq;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        public static ToggleButton CreateGravityButtonWithDelegate(IButtonDelegation d)
        {
            Image u = Image.Image_createWithResIDQuad(Resources.Img.ObjStarIdle, 21);
            Image d2 = Image.Image_createWithResIDQuad(Resources.Img.ObjStarIdle, 21);
            Image u2 = Image.Image_createWithResIDQuad(Resources.Img.ObjStarIdle, 22);
            Image d3 = Image.Image_createWithResIDQuad(Resources.Img.ObjStarIdle, 22);
            ToggleButton toggleButton = new ToggleButton().InitWithUpElement1DownElement1UpElement2DownElement2andID(u, d2, u2, d3, GameSceneButtonId.GravityToggle);
            toggleButton.delegateButtonDelegate = d;
            return toggleButton;
        }

        /// <summary>
        /// Initializes the game scene and the pack background layers.
        /// </summary>
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
            particlesAniPool = new AnimationsPool
            {
                visible = false
            };
            _ = AddChild(particlesAniPool);
            decalsLayer = new BaseElement();
            staticAniPool = new AnimationsPool
            {
                visible = false
            };
            _ = AddChild(staticAniPool);
            camera = new Camera2D().InitWithSpeedandType(14f, CAMERATYPE.CAMERASPEEDDELAY);
            string[] boxBackgrounds = PackConfig.GetBoxBackgrounds(cTRRootController.GetPack());
            string boxBackground = boxBackgrounds.FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
            if (string.IsNullOrWhiteSpace(boxBackground))
            {
                throw new InvalidDataException($"Pack config is missing boxBackground for pack {cTRRootController.GetPack()}.");
            }
            back = new TileMap().InitWithRowsColumns(1, 1);
            back.SetRepeatHorizontally(TileMap.Repeat.NONE);
            back.SetRepeatVertically(TileMap.Repeat.ALL);
            // Cache the background texture so we can keep scaling tied to internal width.
            backTexture = Application.GetTexture(boxBackground);
            back.AddTileQuadwithID(backTexture, 0, 0);
            back.FillStartAtRowColumnRowsColumnswithTile(0, 0, 1, 1, 0);
            // Use internal-resolution scale rather than a fixed multiplier.
            UpdateBackgroundScale();
            for (int i = 0; i < 3; i++)
            {
                const int HudUiStarFirstQuad = 2;
                const int HudUiStarLastQuad = 12;
                hudStar[i] = Animation.Animation_createWithResID(Resources.Img.HudUi);
                hudStar[i].SetDrawQuad(HudUiStarFirstQuad);
                _ = hudStar[i].AddAnimationDelayLoopFirstLast(0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, HudUiStarFirstQuad, HudUiStarLastQuad);
                hudStar[i].SetPauseAtIndexforAnimation(10, 0);
                int starSize = hudStar[i].width;
                hudStar[i].anchor = 18;
                hudStar[i].x = (starSize * i) + (starSize / 2) + Canvas.xOffsetScaled;
                hudStar[i].y = hudStar[i].height / 2;
                _ = AddChild(hudStar[i]);
            }
            for (int j = 0; j < 5; j++)
            {
                fingerCuts[j] = [];
            }
            clickToCut = Preferences.GetBooleanForKey("PREFS_CLICK_TO_CUT");
        }

        public void Reload()
        {
            dd.CancelAllDispatches();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (cTRRootController.IsPicker())
            {
                XmlLoaderFinishedWithfromwithSuccess(ContentPaths.LoadXml("mappicker://reload"), "mappicker://reload", true);
                return;
            }
            int pack = cTRRootController.GetPack();
            int level = cTRRootController.GetLevel();
            string mapPath = Path.Combine(ContentPaths.MapsDirectory, LevelsList.LEVEL_NAMES[pack, level]);
            XmlLoaderFinishedWithfromwithSuccess(ContentPaths.LoadXml(mapPath), mapPath, true);
        }

        public void LoadNextMap()
        {
            dd.CancelAllDispatches();
            initialCameraToStarDistance = -1f;
            animateRestartDim = false;
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (cTRRootController.IsPicker())
            {
                XmlLoaderFinishedWithfromwithSuccess(ContentPaths.LoadXml("mappicker://next"), "mappicker://next", true);
                return;
            }
            int pack = cTRRootController.GetPack();
            int level = cTRRootController.GetLevel();
            if (level < CTRPreferences.GetLevelsInPackCount(pack) - 1)
            {
                cTRRootController.SetLevel(++level);
                cTRRootController.SetMapName(LevelsList.LEVEL_NAMES[pack, level]);
                string mapPath = Path.Combine(ContentPaths.MapsDirectory, LevelsList.LEVEL_NAMES[pack, level]);
                XmlLoaderFinishedWithfromwithSuccess(ContentPaths.LoadXml(mapPath), mapPath, true);
            }
        }

        public void Restart()
        {
            Hide();
            Show();
        }

        public void CreateEarthImageWithOffsetXY(float xs, float ys)
        {
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjStarIdle, 23);
            image.anchor = 18;
            Timeline timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(180, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3f));
            image.AddTimelinewithID(timeline, 1);
            timeline = new Timeline().InitWithMaxKeyFramesOnTrack(2);
            timeline.AddKeyFrame(KeyFrame.MakeRotation(180, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0));
            timeline.AddKeyFrame(KeyFrame.MakeRotation(0, KeyFrame.TransitionType.FRAME_TRANSITION_LINEAR, 0.3f));
            image.AddTimelinewithID(timeline, 0);

            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            Vector? earthBgPosition = PackConfig.GetEarthBgPosition(cTRRootController.GetPack());
            if (earthBgPosition.HasValue)
            {
                image.x = earthBgPosition.Value.X;
                image.y = earthBgPosition.Value.Y;
            }

            if (Canvas.isFullscreen)
            {
                _ = Global.ScreenSizeManager.ScreenWidth;
            }
            image.scaleX = 1f;
            image.scaleY = 1f;
            image.x += xs;
            image.y += ys;
            earthAnims.Add(image);
        }
    }
}
