using System;
using System.Collections.Generic;

using CutTheRopeDX.Commons;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.Helpers;

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
            // Host is the desktop host and is absent headless, where there is no keyboard.
            if (overlayMode == GameControllerOverlayMode.Gameplay && PlatformServices.Host?.IsKeyPressed(KeyCode.F5) == true)
            {
                OnButtonPressed(GameControllerButtonId.Restart);
            }
            base.Update(t);

            GameScene gameScene = (GameScene)GetView(0)?.GetChild(GameView.VIEW_ELEMENT_GAME_SCENE);
            if (gameScene?.AcceptsVisualOnlyPointerInput == true && !gameScene.updateable)
            {
                gameScene.UpdatePointerGestureVisuals(t);
            }

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
                if (overlayMode != GameControllerOverlayMode.Gameplay)
                {
                    LevelStart();
                }
                // Flash the restart dim, matching what the restart button does, so an
                // external edit reads as a deliberate restart rather than a glitch.
                scene.animateRestartDim = true;
                scene.Reload();
                EnterOverlayMode(GameControllerOverlayMode.Gameplay);
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
        public override void Deactivate()
        {
            navigationExitActive = true;
            gameplayAudioPaused = false;
            if (GetView(0)?.GetChild(GameView.VIEW_ELEMENT_GAME_SCENE) is GameScene gameScene)
            {
                gameScene.touchable = false;
                gameScene.updateable = false;
            }
            base.Deactivate();
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
            button.x = -8f;
            button.y = 8f;
            _ = gameView.AddChildwithID(button, 1);
            const int HudUiRestartQuad = 0;
            Button button2 = MenuController.CreateButtonWithImageQuadIDDelegate(Resources.Img.HudUi, HudUiRestartQuad, GameControllerButtonId.Restart, this);
            button2.anchor = button2.parentAnchor = 12;
            button2.x = -button.width - 16f;
            button2.y = 8f;
            _ = gameView.AddChildwithID(button2, 2);
            Image image = Image.Image_createWithResIDQuad(Resources.Img.MenuPause, 0);
            image.anchor = image.parentAnchor = 10;
            image.scaleX = image.scaleY = PausePlateScale;
            image.rotationCenterY = -image.height / 2;
            image.passTransformationsToChilds = false;
            mapNameLabel = new Text().InitWithFont(Application.GetFont(Resources.Fnt.SmallFont));
            mapNameLabel.SetName("mapNameLabel");
            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            _ = CTRPreferences.GetScoreForPackLevel(cTRRootController.GetBox(), cTRRootController.GetPack(), cTRRootController.GetLevel());
            mapNameLabel.anchor = mapNameLabel.parentAnchor = 12;
            float labelXOffset = LanguageHelper.IsCurrent(Language.LANGJA) ? 200f : 256f;
            mapNameLabel.x = RTD(-10) + labelXOffset;
            mapNameLabel.y = RTD(-5);
            _ = image.AddChild(mapNameLabel);
            VBox vBox = new VBox().InitWithOffsetAlignWidth(5, 2, DesignBox.w);
            pauseMenuPlate = image;
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
            vBox.y = (DesignBox.h - vBox.height) / 2f;
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
            _ = gameView.AddChildwithID(image, 3);

            // The button column is composed in design space and fitted on its own, rather than
            // hanging off the plate: the plate stays sized to its own art and the best-score
            // label pinned against it (PlaceBestScoreLabel) reasons in logical units, so folding
            // FittedScale into the plate too would throw that math off.
            FittedGroup buttonsGroup = new() { anchor = 9, parentAnchor = 9 };
            pauseButtonsGroup = buttonsGroup;
            _ = buttonsGroup.AddChild(vBox);
            _ = gameView.AddChildwithID(buttonsGroup, GameView.VIEW_ELEMENT_PAUSE_BUTTONS);
            BoxOpenClose boxOpenClose = new BoxOpenClose().InitWithButtonDelegate(this);
            boxOpenClose.delegateboxClosed = new BoxOpenClose.boxClosed(BoxClosed);
            _ = gameView.AddChildwithID(boxOpenClose, 4);
            SnowfallOverlay overlay = SnowfallOverlay.CreateIfEnabled();
            if (overlay != null)
            {
                overlay.anchor = overlay.parentAnchor = 9;
                overlay.Start();
                _ = gameView.AddChildwithID(overlay, 6);
            }
            AddViewwithID(gameView, 0);
        }

        /// <summary>
        /// Initializes the game view for a fresh level start.
        /// </summary>
        public void InitGameView()
        {
            LevelFirstStart();
        }

        /// <summary>
        /// Starts the first-level open transition and enables gameplay input.
        /// </summary>
        public void LevelFirstStart()
        {
            View view = GetView(0);
            navigationExitActive = false;
            ((BoxOpenClose)view.GetChild(4)).LevelFirstStart();
            EnterOverlayMode(GameControllerOverlayMode.Gameplay);
        }

        /// <summary>
        /// Starts a normal level open transition and enables gameplay input.
        /// </summary>
        public void LevelStart()
        {
            View view = GetView(0);
            navigationExitActive = false;
            ((BoxOpenClose)view.GetChild(4)).LevelStart();
            EnterOverlayMode(GameControllerOverlayMode.Gameplay);
        }

        /// <summary>
        /// Starts the level quit transition and disables gameplay input.
        /// </summary>
        public void LevelQuit()
        {
            View view = GetView(0);
            navigationExitActive = true;
            EnterOverlayMode(GameControllerOverlayMode.Results);
            ((BoxOpenClose)view.GetChild(4)).LevelQuit();
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
        /// <param name="result">The completed level's immutable result.</param>
        public void LevelWon(LevelResult result)
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
            GameScene gameScene = (GameScene)view.GetChild(0);
            BoxOpenClose boxOpenClose = (BoxOpenClose)view.GetChild(4);
            Image image = (Image)boxOpenClose.result.GetChildWithName("star1");
            Image image2 = (Image)boxOpenClose.result.GetChildWithName("star2");
            Image image3 = (Image)boxOpenClose.result.GetChildWithName("star3");
            image.SetDrawQuad(result.StarsCollected > 0 ? 13 : 14);
            image2.SetDrawQuad(result.StarsCollected > 1 ? 13 : 14);
            image3.SetDrawQuad(result.StarsCollected > 2 ? 13 : 14);
            string clearText = result.StarsCollected switch
            {
                1 => "LEVEL_CLEARED2",
                2 => "LEVEL_CLEARED3",
                3 => "LEVEL_CLEARED4",
                _ => "LEVEL_CLEARED1"
            };
            ((Text)boxOpenClose.result.GetChildWithName("passText")).SetString(Application.GetString(clearText));
            EnterOverlayMode(GameControllerOverlayMode.Results);
            int box = cTRRootController.GetBox();
            int pack = cTRRootController.GetPack();
            int level = cTRRootController.GetLevel();
            int scoreForPackLevel = CTRPreferences.GetScoreForPackLevel(box, pack, level);
            int starsForPackLevel = CTRPreferences.GetStarsForPackLevel(box, pack, level);
            boxOpenClose.shouldShowImprovedResult = false;
            if (LevelProgressPersistence.ShouldPersist(CustomLevelSession.IsActive, result.FinalScore, scoreForPackLevel))
            {
                CTRPreferences.SetScoreForPackLevel(box, result.FinalScore, pack, level);
                if (scoreForPackLevel > 0)
                {
                    boxOpenClose.shouldShowImprovedResult = true;
                }
            }
            if (LevelProgressPersistence.ShouldPersist(CustomLevelSession.IsActive, result.StarsCollected, starsForPackLevel))
            {
                CTRPreferences.SetStarsForPackLevel(box, result.StarsCollected, pack, level);
                if (starsForPackLevel > 0)
                {
                    boxOpenClose.shouldShowImprovedResult = true;
                }
            }
            boxOpenClose.shouldShowConfetti = result.StarsCollected == 3;
            boxOpenClose.delegateboxClosed = () =>
            {
                // Freeze the game scene a bit after the door closing animation finishes
                TimerManager.RegisterDelayedObjectCall(
                    (_) =>
                    {
                        // Only freeze if still in result screen (not when replaying/moving to next level)
                        if (overlayMode == GameControllerOverlayMode.Results)
                        {
                            gameScene.updateable = false;
                        }
                    },
                    gameScene,
                    0.5f);
            };
            boxOpenClose.LevelWon(result);

            // Update RPC to show win state with stars and score
            CTRRootController ctrRoot = (CTRRootController)Application.SharedRootController();
            LevelResultRpcPayload rpcPayload = LevelResultRpcPayload.From(result);
            PlatformServices.RichPresence?.SetLevelPresence(ctrRoot.GetPack(), ctrRoot.GetLevel(), rpcPayload.Stars, true, gameScene.levelName, rpcPayload.Score, rpcPayload.ElapsedSeconds);

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
            EnterOverlayMode(GameControllerOverlayMode.Results);
            ((BoxOpenClose)GetView(0).GetChild(4)).LevelLost();
        }

        /// <summary>
        /// Handles the game-scene win callback.
        /// </summary>
        /// <param name="result">The completed level's immutable result.</param>
        public void GameWon(LevelResult result)
        {
            PostFlurryLevelEvent("LEVEL_WON");
            LevelWon(result);
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
            if (n == GameControllerButtonId.Pause)
            {
                ExecuteInputCommand(ResolveInput(GameControllerInputKind.PauseButton));
                return;
            }
            if (n == GameControllerButtonId.Continue)
            {
                ExecuteInputCommand(GameControllerInputCommand.Resume);
                return;
            }
            if (n == GameControllerButtonId.ExitFromWin)
            {
                ExecuteInputCommand(GameControllerInputCommand.ExitResults);
                return;
            }

            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            CTRSoundMgr.PlaySound(Resources.Snd.Tap);
            View view = GetView(0);
            switch (n)
            {
                case var id when id == GameControllerButtonId.Restart:
                    GameScene restartScene = (GameScene)view.GetChild(GameView.VIEW_ELEMENT_GAME_SCENE);
                    if (overlayMode != GameControllerOverlayMode.Gameplay
                        || !restartScene.gameplayFlow.CanRestart)
                    {
                        return;
                    }
                    break;
                case var id when id == GameControllerButtonId.SkipLevel:
                    PostFlurryLevelEvent("LEVEL_SKIPPED");
                    if (LastLevelInPack() && !cTRRootController.IsPicker())
                    {
                        LevelQuit();
                        return;
                    }
                    UnlockNextLevel();
                    EnterOverlayMode(GameControllerOverlayMode.Gameplay);
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
                        PlatformServices.Host?.Exit();
                        return;
                    }
                    exitCode = 0;
                    CTRSoundMgr.StopAll();
                    LevelQuit();
                    CTRRootController.LogEvent("IM_MAIN_MENU");
                    return;
                case var id when id == GameControllerButtonId.WinContinue:
                    if (LastLevelInPack() && !cTRRootController.IsPicker())
                    {
                        Deactivate();
                        return;
                    }
                    ((GameScene)view.GetChild(0)).LoadNextMap();
                    LevelStart();
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
            if (overlayMode != GameControllerOverlayMode.Gameplay)
            {
                LevelStart();
            }
            gameScene5.animateRestartDim = n == GameControllerButtonId.Restart;
            gameScene5.Reload();
            EnterOverlayMode(GameControllerOverlayMode.Gameplay);
            CTRRootController.LogEvent(n != GameControllerButtonId.ExitFromLose ? "IG_REPLAY_PRESSED" : "LC_REPLAY_PRESSED");
        }

        /// <summary>Resolves an input source against the authoritative controller and level-flow state.</summary>
        /// <param name="input">Input source to resolve.</param>
        /// <returns>The semantic command allowed in the current state.</returns>
        private GameControllerInputCommand ResolveInput(GameControllerInputKind input)
        {
            if (navigationExitActive)
            {
                return GameControllerInputCommand.Ignore;
            }

            GameScene gameScene = (GameScene)GetView(0).GetChild(GameView.VIEW_ELEMENT_GAME_SCENE);
            return GameControllerInput.Resolve(
                input,
                overlayMode,
                gameScene.gameplayFlow.Phase,
                resultExitAllowed: !CustomLevelSession.IsActive);
        }

        /// <summary>Executes one semantic controller input command.</summary>
        /// <param name="command">Command selected by the pure input resolver.</param>
        private void ExecuteInputCommand(GameControllerInputCommand command)
        {
            switch (command)
            {
                case GameControllerInputCommand.Ignore:
                    return;
                case GameControllerInputCommand.OpenPause:
                    CTRSoundMgr.PlaySound(Resources.Snd.Tap);
                    EnterOverlayMode(GameControllerOverlayMode.Paused);
                    CTRRootController.LogEvent("IG_MENU_PRESSED");
                    CTRRootController.LogEvent("IM_SHOWN");
                    return;
                case GameControllerInputCommand.Resume:
                    CTRSoundMgr.PlaySound(Resources.Snd.Tap);
                    EnterOverlayMode(GameControllerOverlayMode.Gameplay);
                    CTRRootController.LogEvent("IM_CONTINUE_PRESSED");
                    return;
                case GameControllerInputCommand.ExitResults:
                    navigationExitActive = true;
                    CTRSoundMgr.PlaySound(Resources.Snd.Tap);
                    exitCode = EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT;
                    CTRSoundMgr.StopAll();
                    if (!boxCloseHandled)
                    {
                        BoxClosed();
                    }
                    CTRRootController.LogEvent("LC_MENU_PRESSED");
                    Deactivate();
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command));
            }
        }

        /// <inheritdoc />
        void IButtonDelegation.OnButtonPressed(ButtonId buttonId)
        {
            OnButtonPressed(GameControllerButtonId.FromButtonId(buttonId));
        }

        /// <summary>
        /// Applies the complete scene, HUD, menu, gesture, and audio policy for an overlay mode.
        /// </summary>
        /// <param name="mode">Overlay mode to enter.</param>
        private void EnterOverlayMode(GameControllerOverlayMode mode)
        {
            if (overlayModeApplied && overlayMode == mode)
            {
                return;
            }

            View view = GetView(0);
            GameScene gameScene = (GameScene)view.GetChild(GameView.VIEW_ELEMENT_GAME_SCENE);
            GameControllerOverlayMode previousMode = overlayMode;
            overlayMode = mode;
            overlayModeApplied = true;

            bool gameplay = mode == GameControllerOverlayMode.Gameplay;
            bool paused = mode == GameControllerOverlayMode.Paused;
            bool results = mode == GameControllerOverlayMode.Results;
            bool leavingLiveGameplay = results && previousMode == GameControllerOverlayMode.Gameplay;
            bool resultCloseAnimation = leavingLiveGameplay && !navigationExitActive;

            if (mode == GameControllerOverlayMode.Gameplay)
            {
                DeactivateAllButtons();
            }
            else if (mode == GameControllerOverlayMode.Paused)
            {
                // Cancel any in-progress game-scene gesture
                // before the scene stops receiving input. Otherwise the matching touch-up is
                // dropped while paused, stranding the button in its pressed state until restart.
                ReleaseAllTouches(gameScene);
            }
            else if (leavingLiveGameplay)
            {
                // The HUD buttons are about to stop taking input while staying on screen, so a
                // press held at the moment gameplay ended has to be cancelled here, while they
                // can still receive the touch-up. Left alone it would strand a button in its
                // pressed state for the whole closing animation, in full view.
                DeactivateAllButtons();
            }

            view.GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).SetEnabled(paused);
            view.GetChild(GameView.VIEW_ELEMENT_PAUSE_BUTTONS).SetEnabled(paused);

            // The HUD buttons belong to the gameplay screen, so they are on show whenever that
            // screen is - which is every mode except the paused one, where the menu owns the
            // display. That covers both ways a level ends: finishing it, and quitting from the
            // pause menu, where dismissing the menu reveals the gameplay screen again for the
            // box to close over. Either way they ride the animation out inert, exactly as the
            // collected-star row does, and it is the box drawing over them that takes them off
            // screen rather than them blinking out from under it.
            bool hudButtonsVisible = !paused;
            SetHudButton(view.GetChild(GameView.VIEW_ELEMENT_PAUSE_BUTTON), hudButtonsVisible, gameplay);
            SetHudButton(view.GetChild(GameView.VIEW_ELEMENT_RESTART_BUTTON), hudButtonsVisible, gameplay);
            view.GetChild(GameView.VIEW_ELEMENT_RESULTS).touchable = mode == GameControllerOverlayMode.Results;
            gameScene.touchable = gameplay;
            gameScene.updateable = gameplay || resultCloseAnimation;

            if (mode == GameControllerOverlayMode.Results && navigationExitActive)
            {
                // Navigation callers stop all audio before starting the quit animation, which
                // also resets the global pause depth. Do not leave stale controller ownership.
                gameplayAudioPaused = false;
            }
            else if (paused && !gameplayAudioPaused)
            {
                CTRSoundMgr.Pause();
                gameplayAudioPaused = true;
            }
            else if (gameplay && gameplayAudioPaused)
            {
                CTRSoundMgr.Unpause();
                gameplayAudioPaused = false;
            }

            if (!paused)
            {
                return;
            }

            CTRRootController cTRRootController = (CTRRootController)Application.SharedRootController();
            if (cTRRootController.IsPicker())
            {
                mapNameLabel.SetString("");
            }
            else if (CustomLevelSession.IsActive)
            {
                mapNameLabel.SetString(gameScene.ResolveLevelDisplayName() ?? string.Empty);
            }
            else
            {
                int scoreForPackLevel = CTRPreferences.GetScoreForPackLevel(cTRRootController.GetBox(), cTRRootController.GetPack(), cTRRootController.GetLevel());
                mapNameLabel.SetString(Application.GetString("BEST_SCORE") + ": " + scoreForPackLevel);
            }

            // The label's width used to keep it on screen (PlaceBestScoreLabel) is only known
            // once its string is actually set, which happens here rather than at the Relayout
            // that otherwise places it - so it needs placing again now, using the width the text
            // just settled into instead of whatever it carried in before (typically unset).
            PlaceBestScoreLabel(ScreenPresentation.Instance.Snapshot.VisibleBounds);
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
            if (!CanRoutePointerInput(gameScene))
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
            if (!CanRoutePointerInput(gameScene))
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
            if (!CanRoutePointerInput(gameScene))
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
            ExecuteInputCommand(ResolveInput(GameControllerInputKind.Back));
            return true;
        }

        /// <inheritdoc />
        public override bool MenuButtonPressed()
        {
            ExecuteInputCommand(ResolveInput(GameControllerInputKind.Menu));
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
            if (gameScene == null || !CanRoutePointerInput(gameScene))
            {
                return false;
            }
            _ = gameScene.TouchDraggedXYIndex(x, y, 0);
            return true;
        }

        /// <summary>Allows gameplay input normally and visual-only trails during a stable outcome.</summary>
        private bool CanRoutePointerInput(GameScene gameScene)
        {
            return gameScene.touchable
                || (!navigationExitActive && gameScene.AcceptsVisualOnlyPointerInput);
        }

        /// <inheritdoc />
        public override void FullscreenToggled(bool isFullscreen)
        {
            _ = isFullscreen;
            Relayout(ScreenPresentation.Instance.Snapshot);
        }

        /// <inheritdoc />
        protected override void Relayout(ViewportLayoutSnapshot snapshot)
        {
            base.Relayout(snapshot);

            View view = GetView(0);
            if (view == null)
            {
                return;
            }

            CTRRectangle visible = snapshot.VisibleBounds;

            // Reposition the HUD buttons using the same edge offsets applied at construction,
            // otherwise the restart button collapses onto the pause button and they overlap.
            // Both are grown from the screen's top-right corner by the same boost menu content
            // gets on a narrow viewport, same idea as RelayoutHud's star row: the corner-relative
            // offset scales along with the button itself, so the pair grows as one composition
            // anchored to that corner instead of just getting bigger in place.
            Button pauseButton = (Button)view.GetChild(1);
            Button restartButton = (Button)view.GetChild(2);
            PlaceCornerAnchoredHudButton(pauseButton, -8f, 8f, FittedScale);
            PlaceCornerAnchoredHudButton(restartButton, -pauseButton.width - 16f, 8f, FittedScale);

            PlacePausePlate(visible);
            PlaceBestScoreLabel(visible);

            // The button column is a design-space composition like a menu's, so it is fitted the
            // same way one is rather than merely stretched to the viewport width.
            PlaceFittedGroup(pauseButtonsGroup);

            // Camera first: the HUD pass re-covers the background, and how much world a screen of
            // this shape sees - which is what the background covers - is measured through the
            // camera's own scale.
            GameScene scene = (GameScene)view.GetChild(0);
            scene?.RelayoutCamera();
            scene?.RelayoutHud();

            BoxOpenClose results = (BoxOpenClose)view.GetChild(4);
            results?.RelayoutBox(visible);

            // The result panel is a design-space composition like a menu's, so it is fitted the
            // same way one is: fit the group it hangs from and the whole panel lands with it,
            // centered on the viewport it is shown over at every shape of window.
            PlaceFittedGroup(results?.result);

            // Fitting the group centers the design box, which is not the same as centering the
            // composition inside it - the panel is authored off-center in a taller canvas than
            // the box it is read in. See BoxOpenClose.PanelCenteringOffset. Applied after the fit
            // rather than folded into it, so the offset stays a property of the art it was
            // measured from instead of a special case inside the shared placement rule.
            if (results?.result != null)
            {
                Vector centering = BoxOpenClose.PanelCenteringOffset();
                results.result.x += centering.X * FittedScale;
                results.result.y += centering.Y * FittedScale;
            }
        }

        /// <summary>
        /// Pulls the pause plate out to the sides of the viewport.
        /// </summary>
        /// <remarks>
        /// Widened rather than covered. Covering scales the sheet whole, and a viewport half again
        /// as wide as the design shape would then hang its torn edge half again as far down the
        /// screen, with the first button of the pause column underneath it. Widening leaves the
        /// sheet the depth it was drawn at and only pulls its edges out; what stretches with it is
        /// the tear along the bottom, which is irregular to begin with. The authored scale is the
        /// floor, so the design shape is drawn exactly as it was composed, and a viewport narrower
        /// than the sheet keeps overhanging the sides as it always has.
        /// </remarks>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private void PlacePausePlate(CTRRectangle visible)
        {
            if (pauseMenuPlate == null || pauseMenuPlate.width <= 0)
            {
                return;
            }

            pauseMenuPlate.scaleX = MathF.Max(PausePlateScale, visible.w / pauseMenuPlate.width);
            pauseMenuPlate.scaleY = PausePlateScale;
        }

        /// <summary>
        /// Scale the pause plate is drawn at on the design shape, where the sheet's own width
        /// reaches the sides exactly.
        /// </summary>
        private const float PausePlateScale = 1.25f;

        /// <summary>
        /// Shows or hides a HUD button independently of whether it accepts input, which
        /// <see cref="BaseElement.SetEnabled"/> cannot express because it ties the two together.
        /// </summary>
        /// <param name="button">HUD button to configure.</param>
        /// <param name="visible">Whether the button is drawn.</param>
        /// <param name="interactive">
        /// Whether the button responds to touches. Never <see langword="true"/> while
        /// <paramref name="visible"/> is <see langword="false"/>, which would leave an invisible
        /// button taking presses.
        /// </param>
        private static void SetHudButton(BaseElement button, bool visible, bool interactive)
        {
            button.visible = visible;
            button.updateable = visible;
            button.touchable = visible && interactive;
        }

        /// <summary>
        /// Grows a top-right-anchored HUD button from the screen's top-right corner, so its
        /// authored edge offsets scale along with it rather than the button just getting bigger
        /// in place.
        /// </summary>
        /// <param name="button">Button to place, anchored top-right (12) to the view.</param>
        /// <param name="baseX">Authored X offset from the right edge, at scale one.</param>
        /// <param name="baseY">Authored Y offset from the top edge, at scale one.</param>
        /// <param name="scale">Uniform scale to grow the button and its corner offset by.</param>
        private static void PlaceCornerAnchoredHudButton(Button button, float baseX, float baseY, float scale)
        {
            button.scaleX = button.scaleY = scale;
            button.x = LayoutMath.CornerAnchoredOffset(baseX, button.width, scale, farEdge: true);
            button.y = LayoutMath.CornerAnchoredOffset(baseY, button.height, scale, farEdge: false);
        }

        /// <summary>
        /// Places the pause menu's best-score label, keeping it on screen.
        /// </summary>
        /// <remarks>
        /// The label is authored hanging well past the right edge of the pause plate, which the
        /// design width has room for. A viewport that exposes fewer logical units across does not,
        /// so the offset gives way rather than letting the label run off the screen. It reduces to
        /// the authored offset wherever there is room, which includes the shipped shape.
        /// </remarks>
        /// <param name="visible">The logical region the viewport exposes.</param>
        private void PlaceBestScoreLabel(CTRRectangle visible)
        {
            if (mapNameLabel == null)
            {
                return;
            }

            float scale = FittedScale;
            mapNameLabel.scaleX = mapNameLabel.scaleY = scale;

            float authored = RTD(-10) + (LanguageHelper.IsCurrent(Language.LANGJA) ? 200f : 256f);
            if (pauseMenuPlate == null)
            {
                mapNameLabel.x = authored;
                return;
            }

            // The authored offset hangs the label off the right edge of the plate, which lands it
            // a fixed distance from the right edge of the screen - but only on the design shape,
            // where the plate is exactly one screen wide. What was composed is that distance from
            // the corner, so that is what follows the viewport, and the offset which produces it
            // is worked back out for whatever shape the window is. On a wide one the label used to
            // be left stranded in the middle of the top edge, a plate's width in from the corner.
            float plateEdgeAtDesignWidth = (ViewportLayout.DesignWidth + pauseMenuPlate.width) / 2f;
            float insetFromRight = ViewportLayout.DesignWidth - plateEdgeAtDesignWidth - authored;

            // Right-anchored by CalculateTopLeft (drawX = parentDrawX + parentWidth + x - width)
            // and scaled about its own center, so half of what the boost adds falls outside that
            // edge; only that half has to come back out of the offset.
            float plateEdge = (visible.w + pauseMenuPlate.width) / 2f;
            float halfTheBoost = mapNameLabel.width * (1f - scale) / 2f;
            mapNameLabel.x = visible.w - insetFromRight - plateEdge + halfTheBoost;
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

        /// <summary>Authoritative controller overlay mode.</summary>
        private GameControllerOverlayMode overlayMode = GameControllerOverlayMode.Gameplay;

        /// <summary>Whether the initial overlay presentation has been applied to the created view.</summary>
        private bool overlayModeApplied;

        /// <summary>Whether controller navigation has begun and further Back/Menu input must be ignored.</summary>
        private bool navigationExitActive;

        /// <summary>Whether this controller currently owns one pause on the shared audio manager.</summary>
        private bool gameplayAudioPaused;

        /// <summary>Exit code describing the selected controller deactivation route.</summary>
        public int exitCode;

        /// <summary>Pause-menu label that displays the active map or best score.</summary>
        private Text mapNameLabel;

        /// <summary>The plate the pause menu is drawn on; the best-score label hangs off its edge.</summary>
        private Image pauseMenuPlate;

        /// <summary>
        /// Design-space group the pause menu's button column is composed in, so it fits and
        /// scales the same way a menu view's content does rather than staying pinned at 1x.
        /// </summary>
        private FittedGroup pauseButtonsGroup;

        /// <summary>Maps tracked touch slots to platform touch IDs.</summary>
        private readonly int[] touchAddressMap = new int[5];

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
