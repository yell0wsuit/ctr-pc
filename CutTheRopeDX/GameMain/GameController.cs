using System;
using System.Collections.Generic;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Coordinates the active game scene, pause menu, level result flow, input routing, and game-view transitions.
    /// </summary>
    internal sealed class GameController : ViewController, IButtonDelegation, IGameSceneDelegate
    {
        /// <inheritdoc />
        public override void Update(float t)
        {
            if (!isGamePaused && Global.XnaGame.IsKeyPressed(Keys.F5))
            {
                OnButtonPressed(GameControllerButtonId.Restart);
            }
            base.Update(t);

            if (levelWatcher != null && levelWatcher.TryConsumeChange(DateTime.UtcNow))
            {
                ApplyCustomLevelChange();
            }
        }

        /// <summary>
        /// Applies an external edit to the custom level, reloading in place when possible.
        /// </summary>
        private void ApplyCustomLevelChange()
        {
            if (!CustomLevelFile.TryLoad(CustomLevelSession.LevelPath, out System.Xml.Linq.XElement map, out string error))
            {
                Console.Error.WriteLine(error);
                return;
            }

            CTRRootController root = (CTRRootController)Application.SharedRootController();
            string[] required = LevelResourceScanner.GetRequiredResources(map);
            CustomLevelReloadKind kind = CustomLevelReloadDecision.Decide(required, root.GetSessionResources());

            if (kind == CustomLevelReloadKind.Instant)
            {
                GameScene scene = (GameScene)GetView(0).GetChild(0);
                if (!scene.IsEnabled())
                {
                    LevelStart();
                }
                // Flash the restart dim, matching what the restart button does, so an
                // external edit reads as a deliberate restart rather than a glitch.
                scene.animateRestartDim = true;
                scene.Reload();
                SetPaused(false);
                return;
            }

            root.SetMap(map);
            exitCode = EXIT_CODE_CUSTOM_RELOAD;
            CTRSoundMgr.StopAll();
            Deactivate();
        }

        /// <summary>
        /// Initializes a new game controller.
        /// </summary>
        /// <param name="parent">Parent view controller.</param>
        public GameController(ViewController parent)
            : base(parent)
        {
            CreateGameView();
        }

        /// <inheritdoc />
        public override void Activate()
        {
            PostFlurryLevelEvent("LEVEL_STARTED");
            Application.SharedRootController().SetViewTransition(-1);
            base.Activate();
            CTRSoundMgr.StopMusic();
            PlayMusic();
            InitGameView();
            ShowView(0);

            if (CustomLevelSession.IsActive && levelWatcher == null)
            {
                levelWatcher = new CustomLevelWatcher(
                    CustomLevelSession.LevelPath,
                    TimeSpan.FromMilliseconds(100));
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                levelWatcher?.Dispose();
                levelWatcher = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates the game scene, HUD buttons, pause menu, result box, and optional overlays.
        /// </summary>
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
            int hudQuadOffset = CTRResourceMgr.GetHudButtonQuadOffset();
            Button button = MenuController.CreateButtonWithImageQuadIDDelegate(Resources.Img.HudUi, hudQuadOffset, GameControllerButtonId.Pause, this);
            button.anchor = button.parentAnchor = 12;
            button.x = -Canvas.xOffsetScaled - 8f;
            button.y = 8f;
            _ = gameView.AddChildwithID(button, 1);
            const int HudUiRestartQuad = 0;
            Button button2 = MenuController.CreateButtonWithImageQuadIDDelegate(Resources.Img.HudUi, HudUiRestartQuad, GameControllerButtonId.Restart, this);
            button2.anchor = button2.parentAnchor = 12;
            button2.x = -Canvas.xOffsetScaled - button.width - 16f;
            button2.y = 8f;
            _ = gameView.AddChildwithID(button2, 2);
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuPause, 0);
            image.anchor = image.parentAnchor = 10;
            image.scaleX = image.scaleY = 1.25f;
            image.rotationCenterY = -image.height / 2;
            image.passTransformationsToChilds = false;
            mapNameLabel = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            mapNameLabel.SetName("mapNameLabel");
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            _ = CTRPreferences.GetScoreForPackLevel(cTRRootController.GetBox(), cTRRootController.GetPack(), cTRRootController.GetLevel());
            mapNameLabel.anchor = mapNameLabel.parentAnchor = 12;
            float labelXOffset = LanguageHelper.IsCurrent(Language.LANGJA) ? 200f : 256f;
            mapNameLabel.x = RTD(-10) - Canvas.xOffsetScaled + labelXOffset;
            mapNameLabel.y = RTD(-5);
            _ = image.AddChild(mapNameLabel);
            VBox vBox = new VBox().InitWithOffsetAlignWidth(5, 2, SCREEN_WIDTH);
            Button c = MenuController.CreateButtonWithTextIDDelegate(Application.GetString("CONTINUE"), GameControllerButtonId.Continue, this);
            _ = vBox.AddChild(c);
            if (!CustomLevelSession.IsActive)
            {
                Button c2 = MenuController.CreateButtonWithTextIDDelegate(Application.GetString("SKIP_LEVEL"), GameControllerButtonId.SkipLevel, this);
                _ = vBox.AddChild(c2);
                Button c3 = MenuController.CreateButtonWithTextIDDelegate(Application.GetString("LEVEL_SELECT"), GameControllerButtonId.LevelSelect, this);
                _ = vBox.AddChild(c3);
            }
            string exitLabel = CustomLevelSession.IsActive ? "QUIT_BUTTON" : "MAIN_MENU";
            Button c4 = MenuController.CreateButtonWithTextIDDelegate(Application.GetString(exitLabel), GameControllerButtonId.MainMenu, this);
            _ = vBox.AddChild(c4);
            vBox.anchor = vBox.parentAnchor = 10;
            ToggleButton toggleButton = MenuController.CreateAudioButtonWithQuadDelegateIDiconOffset(3, this, GameControllerButtonId.ToggleMusic);
            ToggleButton toggleButton2 = MenuController.CreateAudioButtonWithQuadDelegateIDiconOffset(2, this, GameControllerButtonId.ToggleSound);
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
            BoxOpenClose boxOpenClose = new BoxOpenClose().InitWithButtonDelegate(this);
            boxOpenClose.delegateboxClosed = new BoxOpenClose.boxClosed(BoxClosed);
            _ = gameView.AddChildwithID(boxOpenClose, 4);
            SnowfallOverlay overlay = SnowfallOverlay.CreateIfEnabled();
            if (overlay != null)
            {
                overlay.anchor = overlay.parentAnchor = 9;
                overlay.Start();
                _ = gameView.AddChildwithID(overlay, 5);
            }
            AddViewwithID(gameView, 0);
        }

        /// <summary>
        /// Initializes the game view for a fresh level start.
        /// </summary>
        public void InitGameView()
        {
            SetPaused(false);
            LevelFirstStart();
        }

        /// <summary>
        /// Starts the first-level open transition and enables gameplay input.
        /// </summary>
        public void LevelFirstStart()
        {
            View view = GetView(0);
            ((BoxOpenClose)view.GetChild(4)).LevelFirstStart();
            isGamePaused = false;
            view.GetChild(0).touchable = true;
            view.GetChild(0).updateable = true;
            view.GetChild(1).touchable = true;
            view.GetChild(2).touchable = true;
        }

        /// <summary>
        /// Starts a normal level open transition and enables gameplay input.
        /// </summary>
        public void LevelStart()
        {
            View view = GetView(0);
            ((BoxOpenClose)view.GetChild(4)).LevelStart();
            isGamePaused = false;
            view.GetChild(0).touchable = true;
            view.GetChild(0).updateable = true;
            view.GetChild(1).touchable = true;
            view.GetChild(2).touchable = true;
            view.GetChild(4).touchable = false;
        }

        /// <summary>
        /// Starts the level quit transition and disables gameplay input.
        /// </summary>
        public void LevelQuit()
        {
            View view = GetView(0);
            ((BoxOpenClose)view.GetChild(4)).LevelQuit();
            view.GetChild(0).touchable = false;
        }

        /// <summary>
        /// Posts the box-perfect achievement when every level in a pack have 3 stars.
        /// </summary>
        /// <param name="box">Box index containing the pack.</param>
        /// <param name="pack">Pack index to check.</param>
        public static void CheckForBoxPerfect(int box, int pack)
        {
            if (CTRPreferences.IsPackPerfect(box, pack) && pack < name.Length)
            {
                CTRRootController.PostAchievementName(name[pack]);
            }
        }

        /// <summary>
        /// Posts the box-perfect achievement for a pack using its configured box index.
        /// </summary>
        /// <param name="pack">Pack index to check.</param>
        public static void CheckForBoxPerfect(int pack)
        {
            CheckForBoxPerfect(CTRPreferences.GetBoxForPack(pack), pack);
        }

        /// <summary>
        /// Handles result-box close completion, achievements, score persistence, and close state.
        /// </summary>
        public void BoxClosed()
        {
            _ = Application.SharedPreferences();
            CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
            int box = ctrrootController.GetBox();
            int pack = ctrrootController.GetPack();
            _ = ctrrootController.GetLevel();
            bool flag = true;
            for (int levelIndex = CTRPreferences.GetLevelsInPackCount(pack) - 1; levelIndex >= 0; levelIndex--)
            {
                if (CTRPreferences.GetScoreForPackLevel(box, pack, levelIndex) <= 0)
                {
                    flag = false;
                    break;
                }
            }
            if (flag && pack < nameArray.Length)
            {
                CTRRootController.PostAchievementName(nameArray[pack]);
            }
            CheckForBoxPerfect(box, pack);
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
            int totalPackScore = 0;
            for (int i = 0; i < CTRPreferences.GetLevelsInPackCount(pack); i++)
            {
                totalPackScore += CTRPreferences.GetScoreForPackLevel(box, pack, i);
            }
            //if (!CTRRootController.IsHacked())
            //{
            //    CTRPreferences.SetScoreHash();
            //    Preferences.RequestSave();
            //}
            boxCloseHandled = true;
        }

        /// <summary>
        /// Updates level result UI, persists improved score data, and starts the level-won result flow.
        /// </summary>
        public void LevelWon()
        {
            boxCloseHandled = false;
            _ = Application.SharedPreferences();
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            //if (!CTRPreferences.IsScoreHashValid())
            //{
            //CTRRootController.SetHacked();
            //}
            CTRSoundMgr.PlaySound(Resources.Snd.Win);
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
            string clearText = gameScene.starsCollected switch
            {
                1 => "LEVEL_CLEARED2",
                2 => "LEVEL_CLEARED3",
                3 => "LEVEL_CLEARED4",
                _ => "LEVEL_CLEARED1"
            };
            ((Text)boxOpenClose.result.GetChildWithName("passText")).SetString(Application.GetString(clearText));
            boxOpenClose.time = gameScene.time;
            boxOpenClose.starBonus = gameScene.starBonus;
            boxOpenClose.timeBonus = gameScene.timeBonus;
            boxOpenClose.score = gameScene.score;
            isGamePaused = true;
            gameScene.touchable = false;
            view.GetChild(2).touchable = false;
            view.GetChild(1).touchable = false;
            int box = cTRRootController.GetBox();
            int pack = cTRRootController.GetPack();
            int level = cTRRootController.GetLevel();
            int scoreForPackLevel = CTRPreferences.GetScoreForPackLevel(box, pack, level);
            int starsForPackLevel = CTRPreferences.GetStarsForPackLevel(box, pack, level);
            boxOpenClose.shouldShowImprovedResult = false;
            if (LevelProgressPersistence.ShouldPersist(CustomLevelSession.IsActive, gameScene.score, scoreForPackLevel))
            {
                CTRPreferences.SetScoreForPackLevel(box, gameScene.score, pack, level);
                if (scoreForPackLevel > 0)
                {
                    boxOpenClose.shouldShowImprovedResult = true;
                }
            }
            if (LevelProgressPersistence.ShouldPersist(CustomLevelSession.IsActive, gameScene.starsCollected, starsForPackLevel))
            {
                CTRPreferences.SetStarsForPackLevel(box, gameScene.starsCollected, pack, level);
                if (starsForPackLevel > 0)
                {
                    boxOpenClose.shouldShowImprovedResult = true;
                }
            }
            boxOpenClose.shouldShowConfetti = gameScene.starsCollected == 3;
            boxOpenClose.delegateboxClosed = () =>
            {
                // Freeze the game scene a bit after the door closing animation finishes
                TimerManager.RegisterDelayedObjectCall(
                    (_) =>
                    {
                        // Only freeze if still in result screen (not when replaying/moving to next level)
                        if (isGamePaused)
                        {
                            gameScene.updateable = false;
                        }
                    },
                    gameScene,
                    0.5f);
            };
            boxOpenClose.LevelWon();

            // Update RPC to show win state with stars and score
            CTRRootController ctrRoot = (CTRRootController)Application.SharedRootController();
            Game1.RPC.SetLevelPresence(ctrRoot.GetPack(), ctrRoot.GetLevel(), gameScene.starsCollected, true, gameScene.levelName, gameScene.score, (int)gameScene.time);

            if (!CustomLevelSession.IsActive)
            {
                UnlockNextLevel();
            }
        }

        /// <summary>
        /// Starts the level-lost box transition.
        /// </summary>
        public void LevelLost()
        {
            ((BoxOpenClose)GetView(0).GetChild(4)).LevelLost();
        }

        /// <summary>
        /// Handles the game-scene win callback.
        /// </summary>
        public void GameWon()
        {
            PostFlurryLevelEvent("LEVEL_WON");
            LevelWon();
        }

        /// <summary>
        /// Handles the game-scene loss callback.
        /// </summary>
        public void GameLost()
        {
            PostFlurryLevelEvent("LEVEL_LOST");
        }

        /// <summary>
        /// Determines whether the current level is the last level in the active pack.
        /// </summary>
        /// <returns><see langword="true"/> when the current level is the final pack level; otherwise, <see langword="false"/>.</returns>
        public bool LastLevelInPack()
        {
            CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
            if (ctrrootController.GetLevel() == CTRPreferences.GetLevelsInPackCount(ctrrootController.GetPack()) - 1)
            {
                exitCode = 2;
                CTRSoundMgr.StopAll();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unlocks the next level in the active pack when one exists and is still locked.
        /// </summary>
        public static void UnlockNextLevel()
        {
            CTRRootController ctrrootController = (CTRRootController)Application.SharedRootController();
            int box = ctrrootController.GetBox();
            int pack = ctrrootController.GetPack();
            int level = ctrrootController.GetLevel();
            if (level < CTRPreferences.GetLevelsInPackCount(pack) - 1 && CTRPreferences.GetUnlockedForPackLevel(box, pack, level + 1) == UNLOCKEDSTATE.LOCKED)
            {
                CTRPreferences.SetUnlockedForPackLevel(box, UNLOCKEDSTATE.UNLOCKED, pack, level + 1);
            }
        }

        /// <summary>
        /// Handles pause, result, audio, restart, and navigation buttons for the game controller.
        /// </summary>
        /// <param name="n">Game controller button identifier.</param>
        public void OnButtonPressed(GameControllerButtonId n)
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            CTRSoundMgr.PlaySound(Resources.Snd.Tap);
            View view = GetView(0);
            switch (n)
            {
                case var id when id == GameControllerButtonId.Continue:
                    ((GameScene)view.GetChild(0)).dimTime = tmpDimTime;
                    tmpDimTime = 0;
                    SetPaused(false);
                    CTRRootController.LogEvent("IM_CONTINUE_PRESSED");
                    return;
                case var id when id == GameControllerButtonId.Restart:
                    break;
                case var id when id == GameControllerButtonId.SkipLevel:
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
                case var id when id == GameControllerButtonId.LevelSelect:
                    exitCode = 1;
                    CTRSoundMgr.StopAll();
                    LevelQuit();
                    CTRRootController.LogEvent("IM_LEVEL_SELECT_PRESSED");
                    return;
                case var id when id == GameControllerButtonId.MainMenu:
                    if (CustomLevelSession.IsActive)
                    {
                        CTRSoundMgr.StopAll();
                        Global.XnaGame.Exit();
                        return;
                    }
                    exitCode = 0;
                    CTRSoundMgr.StopAll();
                    LevelQuit();
                    CTRRootController.LogEvent("IM_MAIN_MENU");
                    return;
                case var id when id == GameControllerButtonId.ExitFromWin:
                    exitCode = 1;
                    CTRSoundMgr.StopAll();
                    if (!boxCloseHandled)
                    {
                        BoxClosed();
                    }
                    CTRRootController.LogEvent("LC_MENU_PRESSED");
                    Deactivate();
                    return;
                case var id when id == GameControllerButtonId.Pause:
                    {
                        GameScene gameScene4 = (GameScene)view.GetChild(0);
                        if (!GameControllerInput.CanPauseFromGameplay(
                            view.GetChild(1).touchable,
                            gameScene4.outcomeTransitionActive))
                        {
                            return;
                        }
                        tmpDimTime = (int)gameScene4.dimTime;
                        gameScene4.dimTime = 0f;
                        SetPaused(true);
                        CTRRootController.LogEvent("IG_MENU_PRESSED");
                        CTRRootController.LogEvent("IM_SHOWN");
                        return;
                    }
                case var id when id == GameControllerButtonId.WinContinue:
                    if (LastLevelInPack() && !cTRRootController.IsPicker())
                    {
                        Deactivate();
                        return;
                    }
                    ((GameScene)view.GetChild(0)).LoadNextMap();
                    LevelStart();
                    SetPaused(false);
                    return;
                case var id when id == GameControllerButtonId.ExitFromLose:
                    if (!boxCloseHandled)
                    {
                        BoxClosed();
                    }
                    break;
                case var id when id == GameControllerButtonId.NextLevel:
                    CTRSoundMgr.StopLoopedSounds();
                    if (!boxCloseHandled)
                    {
                        BoxClosed();
                    }
                    CTRRootController.LogEvent("LC_NEXT_PRESSED");
                    if (LastLevelInPack() && !cTRRootController.IsPicker())
                    {
                        Deactivate();
                        return;
                    }
                    ((GameScene)view.GetChild(0)).LoadNextMap();
                    LevelStart();
                    SetPaused(false);
                    return;
                case var id when id == GameControllerButtonId.ToggleMusic:
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
                        PlayMusic();
                        return;
                    }
                case var id when id == GameControllerButtonId.ToggleSound:
                    {
                        bool flag2 = Preferences.GetBooleanForKey("SOUND_ON");
                        Preferences.SetBooleanForKey(!flag2, "SOUND_ON", true);
                        if (flag2)
                        {
                            CTRSoundMgr.SuspendSoundEffects();
                            CTRRootController.LogEvent("IM_SOUND_OFF_PRESSED");
                            return;
                        }
                        CTRSoundMgr.RestoreSoundEffects();
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
            gameScene5.animateRestartDim = n == GameControllerButtonId.Restart;
            gameScene5.Reload();
            SetPaused(false);
            CTRRootController.LogEvent(n != GameControllerButtonId.ExitFromLose ? "IG_REPLAY_PRESSED" : "LC_REPLAY_PRESSED");
        }

        /// <inheritdoc />
        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(GameControllerButtonId.FromButtonId(buttonId));
        }

        /// <summary>
        /// Sets pause state, toggles HUD/menu visibility, and updates audio pause state.
        /// </summary>
        /// <param name="p"><see langword="true"/> to pause the game; <see langword="false"/> to resume it.</param>
        public void SetPaused(bool p)
        {
            View view = GetView(0);
            if (!p)
            {
                DeactivateAllButtons();
            }
            else
            {
                // Cancel any in-progress game-scene gesture
                // before the scene stops receiving input. Otherwise the matching touch-up is
                // dropped while paused, stranding the button in its pressed state until restart.
                ReleaseAllTouches((GameScene)view.GetChild(0));
            }
            isGamePaused = p;
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
            int scoreForPackLevel = CTRPreferences.GetScoreForPackLevel(cTRRootController.GetBox(), cTRRootController.GetPack(), cTRRootController.GetLevel());
            mapNameLabel.SetString(Application.GetString("BEST_SCORE") + ": " + scoreForPackLevel);
        }

        /// <inheritdoc />
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
                    int touchSlot = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (touchAddressMap[i] == 0)
                        {
                            touchAddressMap[i] = touch.Id;
                            touchSlot = i;
                            break;
                        }
                    }
                    if (touchSlot != -1)
                    {
                        _ = gameScene.TouchDownXYIndex(CtrRenderer.TransformX(touch.Position.X), CtrRenderer.TransformY(touch.Position.Y), touchSlot);
                    }
                }
            }
            return true;
        }

        /// <inheritdoc />
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
                    int touchSlot = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (touchAddressMap[i] == touch.Id)
                        {
                            touchAddressMap[i] = 0;
                            touchSlot = i;
                            break;
                        }
                    }
                    if (touchSlot != -1)
                    {
                        _ = gameScene.TouchUpXYIndex(CtrRenderer.TransformX(touch.Position.X), CtrRenderer.TransformY(touch.Position.Y), touchSlot);
                    }
                    else
                    {
                        ReleaseAllTouches(gameScene);
                    }
                }
            }
            return true;
        }

        /// <inheritdoc />
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
                    int touchSlot = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        if (touchAddressMap[i] == touch.Id)
                        {
                            touchSlot = i;
                            break;
                        }
                    }
                    if (touchSlot != -1)
                    {
                        _ = gameScene.TouchMoveXYIndex(CtrRenderer.TransformX(touch.Position.X), CtrRenderer.TransformY(touch.Position.Y), touchSlot);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Posts a level analytics event.
        /// </summary>
        /// <param name="_">Analytics event name.</param>
        /// <remarks>
        /// No-op code.
        /// </remarks>
        private static void PostFlurryLevelEvent(string _)
        {
        }

        /// <inheritdoc />
        public override bool BackButtonPressed()
        {
            View view = GetView(0);
            GameScene gameScene = (GameScene)view.GetChild(0);
            if (GameControllerInput.CanPauseFromGameplay(
                view.GetChild(1).touchable,
                gameScene.outcomeTransitionActive))
            {
                OnButtonPressed(GameControllerButtonId.Pause);
            }
            else if (view.GetChild(3).IsEnabled())
            {
                OnButtonPressed(GameControllerButtonId.Continue);
            }
            else if (GameControllerInput.CanExitResultWithBack(
                view.GetChild(4).touchable,
                gameScene.outcomeTransitionActive))
            {
                OnButtonPressed(GameControllerButtonId.ExitFromWin);
            }
            return true;
        }

        /// <inheritdoc />
        public override bool MenuButtonPressed()
        {
            View view = GetView(0);
            GameScene gameScene = (GameScene)view.GetChild(0);
            if (GameControllerInput.CanPauseFromGameplay(
                view.GetChild(1).touchable,
                gameScene.outcomeTransitionActive))
            {
                OnButtonPressed(GameControllerButtonId.Pause);
            }
            else if (view.GetChild(3).IsEnabled())
            {
                OnButtonPressed(GameControllerButtonId.Continue);
            }
            return true;
        }

        /// <summary>
        /// Advances to the next level or deactivates the controller at the end of a non-picker pack.
        /// </summary>
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
            SetPaused(false);
        }

        /// <summary>
        /// Clears all tracked touch slots and sends release events to the game scene.
        /// </summary>
        /// <param name="gs">Game scene that should receive synthetic touch releases.</param>
        public void ReleaseAllTouches(GameScene gs)
        {
            for (int i = 0; i < 5; i++)
            {
                touchAddressMap[i] = 0;
                _ = gs.TouchUpXYIndex(-500f, -500f, i);
            }
        }

        /// <summary>
        /// Initializes ad-skipper state for the active game view.
        /// </summary>
        public void SetAdSkipper()
        {
            _ = (GameView)GetView(0);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void FullscreenToggled(bool isFullscreen)
        {
            View view = GetView(0);
            // Reposition the HUD buttons using the same edge offsets applied at construction,
            // otherwise the restart button collapses onto the pause button and they overlap.
            Button pauseButton = (Button)view.GetChild(1);
            Button restartButton = (Button)view.GetChild(2);
            pauseButton.x = -Canvas.xOffsetScaled - 8f;
            restartButton.x = -Canvas.xOffsetScaled - pauseButton.width - 16f;
            float labelXOffset = LanguageHelper.IsCurrent(Language.LANGJA) ? 200f : 256f;
            mapNameLabel.x = RTD(-10) - Canvas.xOffsetScaled + labelXOffset;
            GameScene gameScene = (GameScene)view.GetChild(0);
            gameScene?.FullscreenToggled(isFullscreen);
        }

        /// <summary>
        /// Plays the appropriate gameplay music for the active pack and seasonal event.
        /// </summary>
        private static void PlayMusic()
        {
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (SpecialEvents.IsXmas)
            {
                CTRSoundMgr.PlayMusic(Resources.Music.GameMusicXmas);
            }
            else
            {
                string musicPack = PackConfig.GetMusicPackOrDefault(cTRRootController.GetPack());
                switch (musicPack)
                {
                    case null:
                        string[] musicList = PackConfig.GetMusicListOrDefault(cTRRootController.GetPack());
                        if (musicList.Length > 0)
                        {
                            CTRSoundMgr.PlayRandomMusic(musicList);
                        }
                        else
                        {
                            Console.WriteLine($"[Game music] missing either musicPack or musicList for pack {cTRRootController.GetPack()}.");
                        }
                        break;
                    case var p when p == MusicPackNames.CtROriginal:
                        CTRSoundMgr.PlayRandomMusic(MusicPacks.CtROriginal);
                        break;
                    default:
                        Console.WriteLine($"[Game music] Unknown musicPack '{musicPack}'");
                        break;
                }
            }
        }

        /// <summary>Button ID for exiting from the win result panel.</summary>
        public const int BUTTON_WIN_EXIT = 5;

        /// <summary>Button ID for restarting from the win result panel.</summary>
        public const int BUTTON_WIN_RESTART = 8;

        /// <summary>Button ID for advancing to the next level from the win result panel.</summary>
        public const int BUTTON_WIN_NEXT_LEVEL = 9;

        /// <summary>Exit code for returning to the main menu from the pause menu.</summary>
        public const int EXIT_CODE_FROM_PAUSE_MENU = 0;

        /// <summary>Exit code for returning to level select from the pause menu.</summary>
        public const int EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT = 1;

        /// <summary>Exit code for returning to level select and advancing to the next pack.</summary>
        public const int EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT_NEXT_PACK = 2;

        /// <summary>Exit code: reload the custom level through the loading screen.</summary>
        public const int EXIT_CODE_CUSTOM_RELOAD = 3;

        /// <summary>Watches the custom level file for external edits, or <see langword="null"/> in normal play.</summary>
        private CustomLevelWatcher levelWatcher;

        /// <summary>Whether gameplay is currently paused.</summary>
        private bool isGamePaused;

        /// <summary>Exit code describing the selected controller deactivation route.</summary>
        public int exitCode;

        /// <summary>Pause-menu label that displays the active map or best score.</summary>
        private Text mapNameLabel;

        /// <summary>Maps tracked touch slots to platform touch IDs.</summary>
        private readonly int[] touchAddressMap = new int[5];

        /// <summary>Temporary dim-time value saved while the pause menu is open.</summary>
        private int tmpDimTime;

        /// <summary>Whether the result box close flow has already persisted score and achievement state.</summary>
        private bool boxCloseHandled;

        /// <summary>
        /// Achievement identifiers for perfect pack completion by pack index.
        /// </summary>
        /// <remarks>
        /// Todo: Remove
        /// </remarks>
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
                    "com.zeptolab.ctr.djboxperfect",
                    "com.zeptolab.ctr.spookyboxperfect",
                    "com.zeptolab.ctr.steamboxperfect"
                ];

        /// <summary>
        /// Achievement identifiers for pack completion by pack index.
        /// </summary>
        /// <remarks>
        /// Todo: Remove
        /// </remarks>
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
                    "com.zeptolab.ctr.djboxcompleted",
                    "com.zeptolab.ctr.spookyboxcompleted",
                    "com.zeptolab.ctr.steamboxcompleted"
                ];
    }
}
