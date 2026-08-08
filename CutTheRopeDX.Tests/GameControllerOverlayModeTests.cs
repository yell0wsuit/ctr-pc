using System.Reflection;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Helpers;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class GameControllerOverlayModeTests
    {
        private static (GameController Controller, GameScene Scene) Load()
        {
            _ = HeadlessGame.Boot();
            CTRSoundMgr.StopAll();
            GameController controller = HeadlessGame.LoadLevelWithController(pack: 1, level: 4);
            return (controller, (GameScene)controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_GAME_SCENE));
        }

        private static int AudioPauseDepth()
        {
            FieldInfo field = typeof(SoundMgr).GetField("pauseDepth", BindingFlags.Instance | BindingFlags.NonPublic);
            return Assert.IsType<int>(field?.GetValue(Application.SharedSoundMgr()));
        }

        private static Button FindButton(BaseElement root, GameControllerButtonId buttonId)
        {
            if (root is Button button && button.buttonID == (ButtonId)buttonId)
            {
                return button;
            }

            foreach (BaseElement child in root.GetChilds().Values)
            {
                Button match = child == null ? null : FindButton(child, buttonId);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        [Fact]
        public void GameplayModeEnablesSceneAndHudWithoutPauseAudio()
        {
            (GameController controller, GameScene scene) = Load();
            View view = controller.GetView(0);

            Assert.True(scene.touchable);
            Assert.True(scene.updateable);
            Assert.False(view.GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
            Assert.True(view.GetChild(GameView.VIEW_ELEMENT_PAUSE_BUTTON).IsEnabled());
            Assert.True(view.GetChild(GameView.VIEW_ELEMENT_RESTART_BUTTON).IsEnabled());
            Assert.Equal(0, AudioPauseDepth());
        }

        [Fact]
        public void PausedModeFreezesSceneShowsMenuDisablesHudAndPausesAudio()
        {
            (GameController controller, GameScene scene) = Load();
            View view = controller.GetView(0);

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.False(scene.touchable);
            Assert.False(scene.updateable);
            Assert.True(view.GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
            Assert.False(view.GetChild(GameView.VIEW_ELEMENT_PAUSE_BUTTON).IsEnabled());
            Assert.False(view.GetChild(GameView.VIEW_ELEMENT_RESTART_BUTTON).IsEnabled());
            Assert.Equal(1, AudioPauseDepth());
        }

        [Fact]
        public void ResultsModeKeepsSceneUpdatingDuringCloseAnimation()
        {
            (GameController controller, GameScene scene) = Load();
            View view = controller.GetView(0);

            controller.LevelWon(LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2));

            Assert.False(scene.touchable);
            Assert.True(scene.updateable);
            BoxOpenClose results = Assert.IsType<BoxOpenClose>(view.GetChild(GameView.VIEW_ELEMENT_RESULTS));
            Assert.True(results.updateable);
            controller.Update(0.1f);
            Assert.Equal(1, results.raState);
            Assert.False(view.GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
            Assert.False(view.GetChild(GameView.VIEW_ELEMENT_PAUSE_BUTTON).IsEnabled());
            Assert.False(view.GetChild(GameView.VIEW_ELEMENT_RESTART_BUTTON).IsEnabled());
            Assert.Equal(0, AudioPauseDepth());
        }

        [Fact]
        public void ResultsModeFreezesSceneAfterBoxCloseDelay()
        {
            (GameController controller, GameScene scene) = Load();
            BoxOpenClose box = (BoxOpenClose)controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_RESULTS);
            controller.LevelWon(LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2));

            box.PostBoxClosed();
            TimerManager.Update(0.51f);

            Assert.False(scene.updateable);
        }

        [Fact]
        public void DelayedResultFreezeDoesNotRefreezeReplayedLevel()
        {
            (GameController controller, GameScene scene) = Load();
            BoxOpenClose box = (BoxOpenClose)controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_RESULTS);
            controller.LevelWon(LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2));

            box.PostBoxClosed();
            controller.OnButtonPressed(GameControllerButtonId.ExitFromLose);
            Assert.True(scene.updateable);
            TimerManager.Update(0.51f);

            Assert.True(scene.updateable);
        }

        [Fact]
        public void EnteringResultsFromPauseKeepsGameplaySuspended()
        {
            (GameController controller, GameScene scene) = Load();
            controller.OnButtonPressed(GameControllerButtonId.Pause);
            Assert.Equal(1, AudioPauseDepth());

            controller.LevelWon(LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2));

            Assert.False(scene.updateable);
            Assert.Equal(1, AudioPauseDepth());
        }

        [Fact]
        public void LeavingPausedGameplayForMenuDoesNotRestartSceneDuringCloseAnimation()
        {
            (GameController controller, GameScene scene) = Load();
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            controller.OnButtonPressed(GameControllerButtonId.MainMenu);

            Assert.False(scene.touchable);
            Assert.False(scene.updateable);
            Assert.Equal(0, AudioPauseDepth());
        }

        [Fact]
        public void LeavingActiveGameplayForMenuDoesNotCreateANewAudioPause()
        {
            (GameController controller, GameScene scene) = Load();

            controller.OnButtonPressed(GameControllerButtonId.MainMenu);

            Assert.False(scene.touchable);
            Assert.False(scene.updateable);
            Assert.Equal(0, AudioPauseDepth());
        }

        [Fact]
        public void ExitingResultsDirectlyFreezesSceneAndClearsAudioPauseOwnership()
        {
            (GameController controller, GameScene scene) = Load();
            controller.LevelWon(LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2));
            Assert.True(scene.updateable);

            controller.OnButtonPressed(GameControllerButtonId.ExitFromWin);

            Assert.False(scene.touchable);
            Assert.False(scene.updateable);
            Assert.Equal(0, AudioPauseDepth());
        }

        [Fact]
        public void EnteringPausedReleasesSceneGesturesOnlyOnTheTransition()
        {
            (GameController controller, _) = Load();
            FieldInfo field = typeof(GameController).GetField("touchAddressMap", BindingFlags.Instance | BindingFlags.NonPublic);
            int[] touchAddressMap = Assert.IsType<int[]>(field?.GetValue(controller));
            touchAddressMap[0] = 42;

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.Equal(0, touchAddressMap[0]);
            touchAddressMap[0] = 43;

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.Equal(43, touchAddressMap[0]);
        }

        [Fact]
        public void EnteringGameplayDeactivatesPressedMenuButtons()
        {
            (GameController controller, GameScene scene) = Load();
            BaseElement pauseMenu = controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU);
            controller.OnButtonPressed(GameControllerButtonId.Pause);
            Button continueButton = Assert.IsType<Button>(FindButton(pauseMenu, GameControllerButtonId.Continue));
            continueButton.SetState(Button.BUTTON_STATE.BUTTON_DOWN);

            controller.OnButtonPressed(GameControllerButtonId.Continue);

            Assert.Equal(Button.BUTTON_STATE.BUTTON_UP, continueButton.state);
            Assert.True(scene.touchable);
            Assert.True(scene.updateable);
            Assert.Equal(0, AudioPauseDepth());
        }
    }
}
