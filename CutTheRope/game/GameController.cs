using CutTheRope.ctr_commons;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace CutTheRope.game
{
    internal class GameController : ViewController, ButtonDelegate, GameSceneDelegate
    {
        public override void update(float t)
        {
            if (!isGamePaused && Global.XnaGame.IsKeyPressed(Keys.F5))
            {
                onButtonPressed(1);
            }
            base.update(t);
        }

        public override NSObject initWithParent(ViewController p)
        {
            if (base.initWithParent(p) != null)
            {
                createGameView();
            }
            return this;
        }

        public override void activate()
        {
            postFlurryLevelEvent("LEVEL_STARTED");
            Application.sharedRootController().setViewTransition(-1);
            base.activate();
            CTRSoundMgr._stopMusic();
            CTRSoundMgr._playRandomMusic(146, 148);
            initGameView();
            showView(0);
        }

        public virtual void createGameView()
        {
            for (int i = 0; i < 5; i++)
            {
                touchAddressMap[i] = 0;
            }
            GameView gameView = (GameView)new GameView().initFullscreen();
            GameScene gameScene = (GameScene)new GameScene().init();
            gameScene.gameSceneDelegate = this;
            gameView.addChildwithID(gameScene, 0);
            Button button = MenuController.createButtonWithImageQuad1Quad2IDDelegate(69, 0, 1, 6, this);
            button.x = -(float)canvas.xOffsetScaled;
            gameView.addChildwithID(button, 1);
            Button button2 = MenuController.createButtonWithImageQuad1Quad2IDDelegate(62, 0, 1, 1, this);
            button2.x = -(float)canvas.xOffsetScaled;
            gameView.addChildwithID(button2, 2);
            Image image = Image.Image_createWithResIDQuad(66, 0);
            image.anchor = image.parentAnchor = 10;
            image.scaleX = image.scaleY = 1.25f;
            image.rotationCenterY = -(float)image.height / 2;
            image.passTransformationsToChilds = false;
            mapNameLabel = new Text().initWithFont(Application.getFont(4));
            mapNameLabel.setName("mapNameLabel");
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            CTRPreferences.getScoreForPackLevel(cTRRootController.getPack(), cTRRootController.getLevel());
            mapNameLabel.anchor = mapNameLabel.parentAnchor = 12;
            mapNameLabel.x = RTD(-10.0) - canvas.xOffsetScaled + 256f;
            mapNameLabel.y = RTD(-5.0);
            image.addChild(mapNameLabel);
            VBox vBox = new VBox().initWithOffsetAlignWidth(5.0, 2, SCREEN_WIDTH);
            Button c = MenuController.createButtonWithTextIDDelegate(Application.getString(655397), 0, this);
            vBox.addChild(c);
            Button c2 = MenuController.createButtonWithTextIDDelegate(Application.getString(655398), 2, this);
            vBox.addChild(c2);
            Button c3 = MenuController.createButtonWithTextIDDelegate(Application.getString(655399), 3, this);
            vBox.addChild(c3);
            Button c4 = MenuController.createButtonWithTextIDDelegate(Application.getString(655400), 4, this);
            vBox.addChild(c4);
            vBox.anchor = vBox.parentAnchor = 10;
            Vector offset = vectSub(Image.getQuadCenter(8, 0), Image.getQuadOffset(8, 12));
            ToggleButton toggleButton = MenuController.createAudioButtonWithQuadDelegateIDiconOffset(3, this, 10, vectZero);
            ToggleButton toggleButton2 = MenuController.createAudioButtonWithQuadDelegateIDiconOffset(2, this, 11, offset);
            HBox hBox = new HBox().initWithOffsetAlignHeight(-10f, 16, toggleButton.height);
            hBox.addChild(toggleButton2);
            hBox.addChild(toggleButton);
            vBox.addChild(hBox);
            vBox.y = (SCREEN_HEIGHT - vBox.height) / 2f;
            bool flag3 = Preferences._getBooleanForKey("SOUND_ON");
            bool flag2 = Preferences._getBooleanForKey("MUSIC_ON");
            if (!flag3)
            {
                toggleButton2.toggle();
            }
            if (!flag2)
            {
                toggleButton.toggle();
            }
            image.addChild(vBox);
            gameView.addChildwithID(image, 3);
            addViewwithID(gameView, 0);
            BoxOpenClose boxOpenClose = (BoxOpenClose)new BoxOpenClose().initWithButtonDelegate(this);
            boxOpenClose.delegateboxClosed = new BoxOpenClose.boxClosed(boxClosed);
            gameView.addChildwithID(boxOpenClose, 4);
        }

        public virtual void initGameView()
        {
            setPaused(false);
            levelFirstStart();
        }

        public virtual void levelFirstStart()
        {
            View view = getView(0);
            ((BoxOpenClose)view.getChild(4)).levelFirstStart();
            isGamePaused = false;
            view.getChild(0).touchable = true;
            view.getChild(1).touchable = true;
            view.getChild(2).touchable = true;
        }

        public virtual void levelStart()
        {
            View view = getView(0);
            ((BoxOpenClose)view.getChild(4)).levelStart();
            isGamePaused = false;
            view.getChild(0).touchable = true;
            view.getChild(1).touchable = true;
            view.getChild(2).touchable = true;
            view.getChild(4).touchable = false;
        }

        public virtual void levelQuit()
        {
            View view = getView(0);
            ((BoxOpenClose)view.getChild(4)).levelQuit();
            view.getChild(0).touchable = false;
        }

        public static void checkForBoxPerfect(int pack)
        {
            if (CTRPreferences.isPackPerfect(pack))
            {
                CTRRootController.postAchievementName((new NSString[]
                {
                    NSS("1058364368"),
                    NSS("1058328727"),
                    NSS("1058324751"),
                    NSS("1515793567"),
                    NSS("1432760157"),
                    NSS("1058327768"),
                    NSS("1058407145"),
                    NSS("1991641832"),
                    NSS("1335599628"),
                    NSS("99928734496"),
                    NSS("com.zeptolab.ctr.djboxperfect")
                })[pack]);
            }
        }

        public virtual void boxClosed()
        {
            CTRPreferences cTRPreferences = Application.sharedPreferences();
            CTRRootController ctrrootController = (CTRRootController)Application.sharedRootController();
            int pack = ctrrootController.getPack();
            ctrrootController.getLevel();
            bool flag = true;
            for (int num = CTRPreferences.getLevelsInPackCount() - 1; num >= 0; num--)
            {
                if (CTRPreferences.getScoreForPackLevel(pack, num) <= 0)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                CTRRootController.postAchievementName((new NSString[]
                {
                    NSS("681486798"),
                    NSS("681462993"),
                    NSS("681520253"),
                    NSS("1515813296"),
                    NSS("1432721430"),
                    NSS("681512374"),
                    NSS("1058310903"),
                    NSS("1991474812"),
                    NSS("1321820679"),
                    NSS("23523272771"),
                    NSS("com.zeptolab.ctr.djboxcompleted")
                })[pack]);
            }
            checkForBoxPerfect(pack);
            int totalStars = CTRPreferences.getTotalStars();
            if (totalStars >= 50 && totalStars < 150)
            {
                CTRRootController.postAchievementName("677900534", ACHIEVEMENT_STRING("\"Bronze Scissors\""));
            }
            else if (totalStars >= 150 && totalStars < 300)
            {
                CTRRootController.postAchievementName("681508185", ACHIEVEMENT_STRING("\"Silver Scissors\""));
            }
            else if (totalStars >= 300)
            {
                CTRRootController.postAchievementName("681473653", ACHIEVEMENT_STRING("\"Golden Scissors\""));
            }
            Preferences._savePreferences();
            int num2 = 0;
            for (int i = 0; i < CTRPreferences.getLevelsInPackCount(); i++)
            {
                num2 += CTRPreferences.getScoreForPackLevel(pack, i);
            }
            if (!CTRRootController.isHacked())
            {
                cTRPreferences.setScoreHash();
                Preferences._savePreferences();
            }
            boxCloseHandled = true;
        }

        public virtual void levelWon()
        {
            boxCloseHandled = false;
            CTRPreferences ctrpreferences = Application.sharedPreferences();
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            if (!ctrpreferences.isScoreHashValid())
            {
                CTRRootController.setHacked();
            }
            CTRSoundMgr._playSound(37);
            View view = getView(0);
            view.getChild(4).touchable = true;
            GameScene gameScene = (GameScene)view.getChild(0);
            BoxOpenClose boxOpenClose = (BoxOpenClose)view.getChild(4);
            Image image = (Image)boxOpenClose.result.getChildWithName("star1");
            Image image2 = (Image)boxOpenClose.result.getChildWithName("star2");
            Image image3 = (Image)boxOpenClose.result.getChildWithName("star3");
            image.setDrawQuad((gameScene.starsCollected > 0) ? 13 : 14);
            image2.setDrawQuad((gameScene.starsCollected > 1) ? 13 : 14);
            image3.setDrawQuad((gameScene.starsCollected > 2) ? 13 : 14);
            ((Text)boxOpenClose.result.getChildWithName("passText")).setString(Application.getString(655372 + gameScene.starsCollected));
            boxOpenClose.time = gameScene.time;
            boxOpenClose.starBonus = gameScene.starBonus;
            boxOpenClose.timeBonus = gameScene.timeBonus;
            boxOpenClose.score = gameScene.score;
            isGamePaused = true;
            gameScene.touchable = false;
            view.getChild(2).touchable = false;
            view.getChild(1).touchable = false;
            int pack = cTRRootController.getPack();
            int level = cTRRootController.getLevel();
            int scoreForPackLevel = CTRPreferences.getScoreForPackLevel(pack, level);
            int starsForPackLevel = CTRPreferences.getStarsForPackLevel(pack, level);
            boxOpenClose.shouldShowImprovedResult = false;
            if (gameScene.score > scoreForPackLevel)
            {
                CTRPreferences.setScoreForPackLevel(gameScene.score, pack, level);
                if (scoreForPackLevel > 0)
                {
                    boxOpenClose.shouldShowImprovedResult = true;
                }
            }
            if (gameScene.starsCollected > starsForPackLevel)
            {
                CTRPreferences.setStarsForPackLevel(gameScene.starsCollected, pack, level);
                if (starsForPackLevel > 0)
                {
                    boxOpenClose.shouldShowImprovedResult = true;
                }
            }
            boxOpenClose.shouldShowConfetti = gameScene.starsCollected == 3;
            boxOpenClose.levelWon();
            unlockNextLevel();
        }

        public virtual void levelLost()
        {
            ((BoxOpenClose)getView(0).getChild(4)).levelLost();
        }

        public virtual void gameWon()
        {
            postFlurryLevelEvent(NSS("LEVEL_WON"));
            levelWon();
        }

        public virtual void gameLost()
        {
            postFlurryLevelEvent(NSS("LEVEL_LOST"));
        }

        public virtual bool lastLevelInPack()
        {
            if (((CTRRootController)Application.sharedRootController()).getLevel() == CTRPreferences.getLevelsInPackCount() - 1)
            {
                exitCode = 2;
                CTRSoundMgr._stopAll();
                return true;
            }
            return false;
        }

        public virtual void unlockNextLevel()
        {
            CTRRootController ctrrootController = (CTRRootController)Application.sharedRootController();
            int pack = ctrrootController.getPack();
            int level = ctrrootController.getLevel();
            if (level < CTRPreferences.getLevelsInPackCount() - 1 && CTRPreferences.getUnlockedForPackLevel(pack, level + 1) == UNLOCKED_STATE.UNLOCKED_STATE_LOCKED)
            {
                CTRPreferences.setUnlockedForPackLevel(UNLOCKED_STATE.UNLOCKED_STATE_UNLOCKED, pack, level + 1);
            }
        }

        public virtual void onButtonPressed(int n)
        {
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            CTRSoundMgr._playSound(9);
            View view = getView(0);
            switch (n)
            {
                case 0:
                    ((GameScene)view.getChild(0)).dimTime = tmpDimTime;
                    tmpDimTime = 0;
                    setPaused(false);
                    CTRRootController.logEvent("IM_CONTINUE_PRESSED");
                    return;
                case 1:
                    break;
                case 2:
                    postFlurryLevelEvent("LEVEL_SKIPPED");
                    if (lastLevelInPack() && !cTRRootController.isPicker())
                    {
                        levelQuit();
                        return;
                    }
                    unlockNextLevel();
                    setPaused(false);
                    ((GameScene)view.getChild(0)).loadNextMap();
                    CTRRootController.logEvent("IM_SKIP_PRESSED");
                    return;
                case 3:
                    exitCode = 1;
                    CTRSoundMgr._stopAll();
                    levelQuit();
                    CTRRootController.logEvent("IM_LEVEL_SELECT_PRESSED");
                    return;
                case 4:
                    exitCode = 0;
                    CTRSoundMgr._stopAll();
                    levelQuit();
                    CTRRootController.logEvent("IM_MAIN_MENU");
                    return;
                case 5:
                    exitCode = 1;
                    CTRSoundMgr._stopAll();
                    if (!boxCloseHandled)
                    {
                        boxClosed();
                    }
                    CTRRootController.logEvent("LC_MENU_PRESSED");
                    deactivate();
                    return;
                case 6:
                    {
                        GameScene gameScene4 = (GameScene)view.getChild(0);
                        tmpDimTime = (int)gameScene4.dimTime;
                        gameScene4.dimTime = 0f;
                        setPaused(true);
                        CTRRootController.logEvent("IG_MENU_PRESSED");
                        CTRRootController.logEvent("IM_SHOWN");
                        return;
                    }
                case 7:
                    goto IL_013D;
                case 8:
                    if (!boxCloseHandled)
                    {
                        boxClosed();
                    }
                    break;
                case 9:
                    CTRSoundMgr._stopLoopedSounds();
                    if (!boxCloseHandled)
                    {
                        boxClosed();
                    }
                    CTRRootController.logEvent("LC_NEXT_PRESSED");
                    goto IL_013D;
                case 10:
                    {
                        bool flag = Preferences._getBooleanForKey("MUSIC_ON");
                        Preferences._setBooleanforKey(!flag, "MUSIC_ON", true);
                        if (flag)
                        {
                            CTRRootController.logEvent("IM_MUSIC_OFF_PRESSED");
                            CTRSoundMgr._stopMusic();
                            return;
                        }
                        CTRRootController.logEvent("IM_MUSIC_ON_PRESSED");
                        CTRSoundMgr._playRandomMusic(146, 148);
                        return;
                    }
                case 11:
                    {
                        bool flag2 = Preferences._getBooleanForKey("SOUND_ON");
                        Preferences._setBooleanforKey(!flag2, "SOUND_ON", true);
                        if (flag2)
                        {
                            CTRRootController.logEvent("IM_SOUND_OFF_PRESSED");
                            return;
                        }
                        CTRRootController.logEvent("IM_SOUND_ON_PRESSED");
                        return;
                    }
                default:
                    return;
            }
            GameScene gameScene5 = (GameScene)view.getChild(0);
            if (!gameScene5.isEnabled())
            {
                levelStart();
            }
            gameScene5.animateRestartDim = n == 1;
            gameScene5.reload();
            setPaused(false);
            CTRRootController.logEvent((n != 8) ? "IG_REPLAY_PRESSED" : "LC_REPLAY_PRESSED");
            return;
        IL_013D:
            if (lastLevelInPack() && !cTRRootController.isPicker())
            {
                deactivate();
                return;
            }
    ((GameScene)view.getChild(0)).loadNextMap();
            levelStart();
        }

        public virtual void setPaused(bool p)
        {
            if (!p)
            {
                deactivateAllButtons();
            }
            isGamePaused = p;
            View view = getView(0);
            view.getChild(3).setEnabled(p);
            view.getChild(1).setEnabled(!p);
            view.getChild(2).setEnabled(!p);
            view.getChild(0).touchable = !p;
            view.getChild(0).updateable = !p;
            if (!isGamePaused)
            {
                CTRSoundMgr._unpause();
                return;
            }
            CTRSoundMgr._pause();
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            if (cTRRootController.isPicker())
            {
                mapNameLabel.setString(NSS(""));
                return;
            }
            int scoreForPackLevel = CTRPreferences.getScoreForPackLevel(cTRRootController.getPack(), cTRRootController.getLevel());
            mapNameLabel.setString(NSS(Application.getString(655380) + ": " + scoreForPackLevel));
        }

        public override bool touchesBeganwithEvent(IList<TouchLocation> touches)
        {
            View view = getView(0);
            GameView gameView = (GameView)view;
            GameScene gameScene = (GameScene)view.getChild(0);
            if (base.touchesBeganwithEvent(touches))
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
                        gameScene.touchDownXYIndex(CtrRenderer.transformX(touch.Position.X), CtrRenderer.transformY(touch.Position.Y), num);
                    }
                }
            }
            return true;
        }

        public override bool touchesEndedwithEvent(IList<TouchLocation> touches)
        {
            GameScene gameScene = (GameScene)getView(0).getChild(0);
            if (base.touchesEndedwithEvent(touches))
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
                        gameScene.touchUpXYIndex(CtrRenderer.transformX(touch.Position.X), CtrRenderer.transformY(touch.Position.Y), num);
                    }
                    else
                    {
                        releaseAllTouches(gameScene);
                    }
                }
            }
            return true;
        }

        public override bool touchesMovedwithEvent(IList<TouchLocation> touches)
        {
            GameScene gameScene = (GameScene)getView(0).getChild(0);
            if (base.touchesMovedwithEvent(touches))
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
                        gameScene.touchMoveXYIndex(CtrRenderer.transformX(touch.Position.X), CtrRenderer.transformY(touch.Position.Y), num);
                    }
                }
            }
            return true;
        }

        private static void postFlurryLevelEvent(string s)
        {
        }

        private static void postFlurryLevelEvent(NSString s)
        {
        }

        public override bool backButtonPressed()
        {
            View view = getView(0);
            if (view.getChild(1).touchable)
            {
                onButtonPressed(6);
            }
            else if (view.getChild(3).isEnabled())
            {
                onButtonPressed(0);
            }
            else if (view.getChild(4).touchable)
            {
                onButtonPressed(5);
            }
            return true;
        }

        public override bool menuButtonPressed()
        {
            View view = getView(0);
            if (view.getChild(1).touchable)
            {
                onButtonPressed(6);
            }
            else if (view.getChild(3).isEnabled())
            {
                onButtonPressed(0);
            }
            return true;
        }

        public virtual void onNextLevel()
        {
            CTRPreferences.gameViewChanged(NSS("game"));
            CTRRootController cTRRootController = (CTRRootController)Application.sharedRootController();
            View view = getView(0);
            GameView gameView = (GameView)view;
            if (lastLevelInPack() && !cTRRootController.isPicker())
            {
                deactivate();
                return;
            }
    ((GameScene)view.getChild(0)).loadNextMap();
            levelStart();
        }

        public virtual void releaseAllTouches(GameScene gs)
        {
            for (int i = 0; i < 5; i++)
            {
                touchAddressMap[i] = 0;
                gs.touchUpXYIndex(-500f, -500f, i);
            }
        }

        public virtual void setAdSkipper(object skipper)
        {
            GameView gameView = (GameView)getView(0);
        }

        public override bool mouseMoved(float x, float y)
        {
            View view = getView(0);
            if (view == null)
            {
                return false;
            }
            GameScene gameScene = (GameScene)view.getChild(0);
            if (gameScene == null || !gameScene.touchable)
            {
                return false;
            }
            gameScene.touchDraggedXYIndex(x, y, 0);
            return true;
        }

        public override void fullscreenToggled(bool isFullscreen)
        {
            View view = getView(0);
            view.getChild(2).x = -(float)canvas.xOffsetScaled;
            view.getChild(1).x = -(float)canvas.xOffsetScaled;
            mapNameLabel.x = RTD(-10.0) - canvas.xOffsetScaled + 256f;
            GameScene gameScene = (GameScene)view.getChild(0);
            gameScene?.fullscreenToggled(isFullscreen);
        }

        private const int BUTTON_PAUSE_RESUME = 0;

        private const int BUTTON_PAUSE_RESTART = 1;

        private const int BUTTON_PAUSE_SKIP = 2;

        private const int BUTTON_PAUSE_LEVEL_SELECT = 3;

        private const int BUTTON_PAUSE_EXIT = 4;

        public const int BUTTON_WIN_EXIT = 5;

        private const int BUTTON_PAUSE = 6;

        private const int BUTTON_NEXT_LEVEL = 7;

        public const int BUTTON_WIN_RESTART = 8;

        public const int BUTTON_WIN_NEXT_LEVEL = 9;

        private const int BUTTON_MUSIC_TOGGLE = 10;

        private const int BUTTON_SOUND_TOGGLE = 11;

        public const int EXIT_CODE_FROM_PAUSE_MENU = 0;

        public const int EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT = 1;

        public const int EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT_NEXT_PACK = 2;

        private bool isGamePaused;

        public int exitCode;

        private Text mapNameLabel;

        private int[] touchAddressMap = new int[5];

        private int tmpDimTime;

        private bool boxCloseHandled;
    }
}
