using System.Collections.Generic;

using CutTheRope.commons;
using CutTheRope.desktop;
using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRope.game
{
    internal sealed class GameController : ViewController, IButtonDelegation, IGameSceneDelegate
    {
        public override void Update(float t)
        {
            if (!isGamePaused && Global.XnaGame.IsKeyPressed(Keys.F5))
            {
                OnButtonPressed(1);
            }
            base.Update(t);
        }

        public GameController(ViewController parent)
            : base(parent)
        {
            CreateGameView();
        }

        public override void Activate()
        {
            PostFlurryLevelEvent("LEVEL_STARTED");
            Application.SharedRootController().SetViewTransition(-1);
            base.Activate();
            CTRSoundMgr.StopMusic();
            CTRSoundMgr.PlayRandomMusic(146, 148);
            InitGameView();
            ShowView(0);
        }

        public void CreateGameView()
        {
            for (int i = 0; i < 5; i++)
            {
                touchAddressMap[i] = 0;
            }
            GameView gameView = new();
            GameScene gameScene = new()
            {
                gameSceneDelegate = this
            };
            _ = gameView.AddChildwithID(gameScene, 0);
            Button button = MenuController.CreateButtonWithImageQuad1Quad2IDDelegate(69, 0, 1, 6, this);
            button.x = -(float)Canvas.xOffsetScaled;
            _ = gameView.AddChildwithID(button, 1);
            Button button2 = MenuController.CreateButtonWithImageQuad1Quad2IDDelegate(62, 0, 1, 1, this);
            button2.x = -(float)Canvas.xOffsetScaled;
            _ = gameView.AddChildwithID(button2, 2);
            Image image = Image.Image_createWithResIDQuad(66, 0);
            image.anchor = image.parentAnchor = 10;
            image.scaleX = image.scaleY = 1.25f;
            image.rotationCenterY = -(float)image.height / 2;
            image.passTransformationsToChilds = false;
            mapNameLabel = new Text().InitWithFont(Application.GetFont(4));
            mapNameLabel.SetName("mapNameLabel");
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            _ = CTRPreferences.GetScoreForPackLevel(cTRRootController.GetPack(), cTRRootController.GetLevel());
            mapNameLabel.anchor = mapNameLabel.parentAnchor = 12;
            mapNameLabel.x = RTD(-10.0) - Canvas.xOffsetScaled + 256f;
            mapNameLabel.y = RTD(-5.0);
            _ = image.AddChild(mapNameLabel);
            VBox vBox = new VBox().InitWithOffsetAlignWidth(5.0, 2, SCREEN_WIDTH);
            Button c = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(655397), 0, this);
            _ = vBox.AddChild(c);
            Button c2 = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(655398), 2, this);
            _ = vBox.AddChild(c2);
            Button c3 = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(655399), 3, this);
            _ = vBox.AddChild(c3);
            Button c4 = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(655400), 4, this);
            _ = vBox.AddChild(c4);
            vBox.anchor = vBox.parentAnchor = 10;
            Vector offset = VectSub(Image.GetQuadCenter(8, 0), Image.GetQuadOffset(8, 12));
            ToggleButton toggleButton = MenuController.CreateAudioButtonWithQuadDelegateIDiconOffset(3, this, 10, vectZero);
            ToggleButton toggleButton2 = MenuController.CreateAudioButtonWithQuadDelegateIDiconOffset(2, this, 11, offset);
            HBox hBox = new HBox().InitWithOffsetAlignHeight(-10f, 16, toggleButton.height);
            _ = hBox.AddChild(toggleButton2);
            _ = hBox.AddChild(toggleButton);
            _ = vBox.AddChild(hBox);
            vBox.y = (SCREEN_HEIGHT - vBox.height) / 2f;
            bool flag3 = Preferences.GetBooleanForKey("SOUND_ON");
            bool flag2 = Preferences.GetBooleanForKey("MUSIC_ON");
            if (!flag3)
            {
                toggleButton2.Toggle();
            }
            if (!flag2)
            {
                toggleButton.Toggle();
            }
            _ = image.AddChild(vBox);
            _ = gameView.AddChildwithID(image, 3);
            AddViewwithID(gameView, 0);
            BoxOpenClose boxOpenClose = new BoxOpenClose().InitWithButtonDelegate(this);
            boxOpenClose.delegateboxClosed = new BoxOpenClose.boxClosed(BoxClosed);
            _ = gameView.AddChildwithID(boxOpenClose, 4);
        }

        public void InitGameView()
        {
            SetPaused(false);
            LevelFirstStart();
        }

        public void LevelFirstStart()
        {
            View view = GetView(0);
            ((BoxOpenClose)view.GetChild(4)).LevelFirstStart();
            isGamePaused = false;
            view.GetChild(0).touchable = true;
            view.GetChild(1).touchable = true;
            view.GetChild(2).touchable = true;
        }

        public void LevelStart()
        {
            View view = GetView(0);
            ((BoxOpenClose)view.GetChild(4)).LevelStart();
            isGamePaused = false;
            view.GetChild(0).touchable = true;
            view.GetChild(1).touchable = true;
            view.GetChild(2).touchable = true;
            view.GetChild(4).touchable = false;
        }

        public void LevelQuit()
        {
            View view = GetView(0);
            ((BoxOpenClose)view.GetChild(4)).LevelQuit();
            view.GetChild(0).touchable = false;
        }

        public static void CheckForBoxPerfect(int pack)
        {
            if (CTRPreferences.IsPackPerfect(pack))
            {
                CTRRootController.PostAchievementName(name[pack]);
            }
        }

        public void BoxClosed()
        {
            CTRPreferences cTRPreferences = Application.SharedPreferences();
            CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
            int pack = ctrrootController.GetPack();
            _ = ctrrootController.GetLevel();
            bool flag = true;
            for (int num = CTRPreferences.GetLevelsInPackCount() - 1; num >= 0; num--)
            {
                if (CTRPreferences.GetScoreForPackLevel(pack, num) <= 0)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                CTRRootController.PostAchievementName(nameArray[pack]);
            }
            CheckForBoxPerfect(pack);
            int totalStars = CTRPreferences.GetTotalStars();
            if (totalStars is >= 50 and < 150)
            {
                CTRRootController.PostAchievementName("677900534", ACHIEVEMENT_STRING("\"Bronze Scissors\""));
            }
            else if (totalStars is >= 150 and < 300)
            {
                CTRRootController.PostAchievementName("681508185", ACHIEVEMENT_STRING("\"Silver Scissors\""));
            }
            else if (totalStars >= 300)
            {
                CTRRootController.PostAchievementName("681473653", ACHIEVEMENT_STRING("\"Golden Scissors\""));
            }
            Preferences.RequestSave();
            int num2 = 0;
            for (int i = 0; i < CTRPreferences.GetLevelsInPackCount(); i++)
            {
                num2 += CTRPreferences.GetScoreForPackLevel(pack, i);
            }
            if (!CTRRootController.IsHacked())
            {
                CTRPreferences.SetScoreHash();
                Preferences.RequestSave();
            }
            boxCloseHandled = true;
        }

        public void LevelWon()
        {
            boxCloseHandled = false;
            _ = Application.SharedPreferences();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (!CTRPreferences.IsScoreHashValid())
            {
                CTRRootController.SetHacked();
            }
            CTRSoundMgr.PlaySound(37);
            View view = GetView(0);
            view.GetChild(4).touchable = true;
            GameScene gameScene = (GameScene)view.GetChild(0);
            BoxOpenClose boxOpenClose = (BoxOpenClose)view.GetChild(4);
            Image image = (Image)boxOpenClose.result.GetChildWithName("star1");
            Image image2 = (Image)boxOpenClose.result.GetChildWithName("star2");
            Image image3 = (Image)boxOpenClose.result.GetChildWithName("star3");
            image.SetDrawQuad(gameScene.starsCollected > 0 ? 13 : 14);
            image2.SetDrawQuad(gameScene.starsCollected > 1 ? 13 : 14);
            image3.SetDrawQuad(gameScene.starsCollected > 2 ? 13 : 14);
            ((Text)boxOpenClose.result.GetChildWithName("passText")).SetString(Application.GetString(655372 + gameScene.starsCollected));
            boxOpenClose.time = gameScene.time;
            boxOpenClose.starBonus = gameScene.starBonus;
            boxOpenClose.timeBonus = gameScene.timeBonus;
            boxOpenClose.score = gameScene.score;
            isGamePaused = true;
            gameScene.touchable = false;
            view.GetChild(2).touchable = false;
            view.GetChild(1).touchable = false;
            int pack = cTRRootController.GetPack();
            int level = cTRRootController.GetLevel();
            int scoreForPackLevel = CTRPreferences.GetScoreForPackLevel(pack, level);
            int starsForPackLevel = CTRPreferences.GetStarsForPackLevel(pack, level);
            boxOpenClose.shouldShowImprovedResult = false;
            if (gameScene.score > scoreForPackLevel)
            {
                CTRPreferences.SetScoreForPackLevel(gameScene.score, pack, level);
                if (scoreForPackLevel > 0)
                {
                    boxOpenClose.shouldShowImprovedResult = true;
                }
            }
            if (gameScene.starsCollected > starsForPackLevel)
            {
                CTRPreferences.SetStarsForPackLevel(gameScene.starsCollected, pack, level);
                if (starsForPackLevel > 0)
                {
                    boxOpenClose.shouldShowImprovedResult = true;
                }
            }
            boxOpenClose.shouldShowConfetti = gameScene.starsCollected == 3;
            boxOpenClose.LevelWon();
            UnlockNextLevel();
        }

        public void LevelLost()
        {
            ((BoxOpenClose)GetView(0).GetChild(4)).LevelLost();
        }

        public void GameWon()
        {
            PostFlurryLevelEvent("LEVEL_WON");
            LevelWon();
        }

        public void GameLost()
        {
            PostFlurryLevelEvent("LEVEL_LOST");
        }

        public bool LastLevelInPack()
        {
            if (((CTRRootController)Application.SharedRootController()).GetLevel() == CTRPreferences.GetLevelsInPackCount() - 1)
            {
                exitCode = 2;
                CTRSoundMgr.StopAll();
                return true;
            }
            return false;
        }

        public static void UnlockNextLevel()
        {
            CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
            int pack = ctrrootController.GetPack();
            int level = ctrrootController.GetLevel();
            if (level < CTRPreferences.GetLevelsInPackCount() - 1 && CTRPreferences.GetUnlockedForPackLevel(pack, level + 1) == UNLOCKEDSTATE.LOCKED)
            {
                CTRPreferences.SetUnlockedForPackLevel(UNLOCKEDSTATE.UNLOCKED, pack, level + 1);
            }
        }

        public void OnButtonPressed(int n)
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            CTRSoundMgr.PlaySound(9);
            View view = GetView(0);
            switch (n)
            {
                case 0:
                    ((GameScene)view.GetChild(0)).dimTime = tmpDimTime;
                    tmpDimTime = 0;
                    SetPaused(false);
                    CTRRootController.LogEvent("IM_CONTINUE_PRESSED");
                    return;
                case 1:
                    break;
                case 2:
                    PostFlurryLevelEvent("LEVEL_SKIPPED");
                    if (LastLevelInPack() && !cTRRootController.IsPicker())
                    {
                        LevelQuit();
                        return;
                    }
                    UnlockNextLevel();
                    SetPaused(false);
                    ((GameScene)view.GetChild(0)).LoadNextMap();
                    CTRRootController.LogEvent("IM_SKIP_PRESSED");
                    return;
                case 3:
                    exitCode = 1;
                    CTRSoundMgr.StopAll();
                    LevelQuit();
                    CTRRootController.LogEvent("IM_LEVEL_SELECT_PRESSED");
                    return;
                case 4:
                    exitCode = 0;
                    CTRSoundMgr.StopAll();
                    LevelQuit();
                    CTRRootController.LogEvent("IM_MAIN_MENU");
                    return;
                case 5:
                    exitCode = 1;
                    CTRSoundMgr.StopAll();
                    if (!boxCloseHandled)
                    {
                        BoxClosed();
                    }
                    CTRRootController.LogEvent("LC_MENU_PRESSED");
                    Deactivate();
                    return;
                case 6:
                    {
                        GameScene gameScene4 = (GameScene)view.GetChild(0);
                        tmpDimTime = (int)gameScene4.dimTime;
                        gameScene4.dimTime = 0f;
                        SetPaused(true);
                        CTRRootController.LogEvent("IG_MENU_PRESSED");
                        CTRRootController.LogEvent("IM_SHOWN");
                        return;
                    }
                case 7:
                    goto IL_013D;
                case 8:
                    if (!boxCloseHandled)
                    {
                        BoxClosed();
                    }
                    break;
                case 9:
                    CTRSoundMgr.StopLoopedSounds();
                    if (!boxCloseHandled)
                    {
                        BoxClosed();
                    }
                    CTRRootController.LogEvent("LC_NEXT_PRESSED");
                    goto IL_013D;
                case 10:
                    {
                        bool flag = Preferences.GetBooleanForKey("MUSIC_ON");
                        Preferences.SetBooleanForKey(!flag, "MUSIC_ON", true);
                        if (flag)
                        {
                            CTRRootController.LogEvent("IM_MUSIC_OFF_PRESSED");
                            CTRSoundMgr.StopMusic();
                            return;
                        }
                        CTRRootController.LogEvent("IM_MUSIC_ON_PRESSED");
                        CTRSoundMgr.PlayRandomMusic(146, 148);
                        return;
                    }
                case 11:
                    {
                        bool flag2 = Preferences.GetBooleanForKey("SOUND_ON");
                        Preferences.SetBooleanForKey(!flag2, "SOUND_ON", true);
                        if (flag2)
                        {
                            CTRRootController.LogEvent("IM_SOUND_OFF_PRESSED");
                            return;
                        }
                        CTRRootController.LogEvent("IM_SOUND_ON_PRESSED");
                        return;
                    }
                default:
                    return;
            }
            GameScene gameScene5 = (GameScene)view.GetChild(0);
            if (!gameScene5.IsEnabled())
            {
                LevelStart();
            }
            gameScene5.animateRestartDim = n == 1;
            gameScene5.Reload();
            SetPaused(false);
            CTRRootController.LogEvent(n != 8 ? "IG_REPLAY_PRESSED" : "LC_REPLAY_PRESSED");
            return;
        IL_013D:
            if (LastLevelInPack() && !cTRRootController.IsPicker())
            {
                Deactivate();
                return;
            }
    ((GameScene)view.GetChild(0)).LoadNextMap();
            LevelStart();
        }

        public void SetPaused(bool p)
        {
            if (!p)
            {
                DeactivateAllButtons();
            }
            isGamePaused = p;
            View view = GetView(0);
            view.GetChild(3).SetEnabled(p);
            view.GetChild(1).SetEnabled(!p);
            view.GetChild(2).SetEnabled(!p);
            view.GetChild(0).touchable = !p;
            view.GetChild(0).updateable = !p;
            if (!isGamePaused)
            {
                CTRSoundMgr.Unpause();
                return;
            }
            CTRSoundMgr.Pause();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (cTRRootController.IsPicker())
            {
                mapNameLabel.SetString("");
                return;
            }
            int scoreForPackLevel = CTRPreferences.GetScoreForPackLevel(cTRRootController.GetPack(), cTRRootController.GetLevel());
            mapNameLabel.SetString(Application.GetString(655380) + ": " + scoreForPackLevel);
        }

        public override bool TouchesBeganwithEvent(IList<TouchLocation> touches)
        {
            View view = GetView(0);
            GameScene gameScene = (GameScene)view.GetChild(0);
            if (base.TouchesBeganwithEvent(touches))
            {
                return true;
            }
            if (!gameScene.touchable)
            {
                return false;
            }
            foreach (TouchLocation touch in touches)
            {
                if (touch.State == TouchLocationState.Pressed)
                {
                    int num = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (touchAddressMap[i] == 0)
                        {
                            touchAddressMap[i] = touch.Id;
                            num = i;
                            break;
                        }
                    }
                    if (num != -1)
                    {
                        _ = gameScene.TouchDownXYIndex(CtrRenderer.TransformX(touch.Position.X), CtrRenderer.TransformY(touch.Position.Y), num);
                    }
                }
            }
            return true;
        }

        public override bool TouchesEndedwithEvent(IList<TouchLocation> touches)
        {
            GameScene gameScene = (GameScene)GetView(0).GetChild(0);
            if (base.TouchesEndedwithEvent(touches))
            {
                return true;
            }
            if (!gameScene.touchable)
            {
                return false;
            }
            foreach (TouchLocation touch in touches)
            {
                if (touch.State == TouchLocationState.Released)
                {
                    int num = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (touchAddressMap[i] == touch.Id)
                        {
                            touchAddressMap[i] = 0;
                            num = i;
                            break;
                        }
                    }
                    if (num != -1)
                    {
                        _ = gameScene.TouchUpXYIndex(CtrRenderer.TransformX(touch.Position.X), CtrRenderer.TransformY(touch.Position.Y), num);
                    }
                    else
                    {
                        ReleaseAllTouches(gameScene);
                    }
                }
            }
            return true;
        }

        public override bool TouchesMovedwithEvent(IList<TouchLocation> touches)
        {
            GameScene gameScene = (GameScene)GetView(0).GetChild(0);
            if (base.TouchesMovedwithEvent(touches))
            {
                return true;
            }
            if (!gameScene.touchable)
            {
                return false;
            }
            foreach (TouchLocation touch in touches)
            {
                if (touch.State == TouchLocationState.Moved)
                {
                    int num = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (touchAddressMap[i] == touch.Id)
                        {
                            num = i;
                            break;
                        }
                    }
                    if (num != -1)
                    {
                        _ = gameScene.TouchMoveXYIndex(CtrRenderer.TransformX(touch.Position.X), CtrRenderer.TransformY(touch.Position.Y), num);
                    }
                }
            }
            return true;
        }

        private static void PostFlurryLevelEvent(string s)
        {
        }

        public override bool BackButtonPressed()
        {
            View view = GetView(0);
            if (view.GetChild(1).touchable)
            {
                OnButtonPressed(6);
            }
            else if (view.GetChild(3).IsEnabled())
            {
                OnButtonPressed(0);
            }
            else if (view.GetChild(4).touchable)
            {
                OnButtonPressed(5);
            }
            return true;
        }

        public override bool MenuButtonPressed()
        {
            View view = GetView(0);
            if (view.GetChild(1).touchable)
            {
                OnButtonPressed(6);
            }
            else if (view.GetChild(3).IsEnabled())
            {
                OnButtonPressed(0);
            }
            return true;
        }

        public void OnNextLevel()
        {
            CTRPreferences.GameViewChanged("game");
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            View view = GetView(0);
            if (LastLevelInPack() && !cTRRootController.IsPicker())
            {
                Deactivate();
                return;
            }
    ((GameScene)view.GetChild(0)).LoadNextMap();
            LevelStart();
        }

        public void ReleaseAllTouches(GameScene gs)
        {
            for (int i = 0; i < 5; i++)
            {
                touchAddressMap[i] = 0;
                _ = gs.TouchUpXYIndex(-500f, -500f, i);
            }
        }

        public void SetAdSkipper(object skipper)
        {
            _ = (GameView)GetView(0);
        }

        public override bool MouseMoved(float x, float y)
        {
            View view = GetView(0);
            if (view == null)
            {
                return false;
            }
            GameScene gameScene = (GameScene)view.GetChild(0);
            if (gameScene == null || !gameScene.touchable)
            {
                return false;
            }
            _ = gameScene.TouchDraggedXYIndex(x, y, 0);
            return true;
        }

        public override void FullscreenToggled(bool isFullscreen)
        {
            View view = GetView(0);
            view.GetChild(2).x = -(float)Canvas.xOffsetScaled;
            view.GetChild(1).x = -(float)Canvas.xOffsetScaled;
            mapNameLabel.x = RTD(-10.0) - Canvas.xOffsetScaled + 256f;
            GameScene gameScene = (GameScene)view.GetChild(0);
            gameScene?.FullscreenToggled(isFullscreen);
        }

        public const int BUTTON_WIN_EXIT = 5;
        public const int BUTTON_WIN_RESTART = 8;

        public const int BUTTON_WIN_NEXT_LEVEL = 9;
        public const int EXIT_CODE_FROM_PAUSE_MENU = 0;

        public const int EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT = 1;

        public const int EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT_NEXT_PACK = 2;

        private bool isGamePaused;

        public int exitCode;

        private Text mapNameLabel;

        private readonly int[] touchAddressMap = new int[5];

        private int tmpDimTime;

        private bool boxCloseHandled;
        internal static readonly string[] name =
                [
                    "1058364368",
                    "1058328727",
                    "1058324751",
                    "1515793567",
                    "1432760157",
                    "1058327768",
                    "1058407145",
                    "1991641832",
                    "1335599628",
                    "99928734496",
                    "com.zeptolab.ctr.djboxperfect"
                ];
        internal static readonly string[] nameArray =
                [
                    "681486798",
                    "681462993",
                    "681520253",
                    "1515813296",
                    "1432721430",
                    "681512374",
                    "1058310903",
                    "1991474812",
                    "1321820679",
                    "23523272771",
                    "com.zeptolab.ctr.djboxcompleted"
                ];
    }
}
