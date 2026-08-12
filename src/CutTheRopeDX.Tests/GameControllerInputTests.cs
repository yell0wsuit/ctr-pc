using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GameControllerInputTests
    {
        private static (GameController Controller, GameScene Scene) Load()
        {
            _ = HeadlessGame.Boot();
            GameController controller = HeadlessGame.LoadLevelWithController(pack: 1, level: 4);
            return (controller, (GameScene)controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_GAME_SCENE));
        }

        private static BaseElement PauseMenu(GameController controller)
        {
            return controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU);
        }

        private static void Press(GameController controller, int input)
        {
            switch ((GameControllerInputKind)input)
            {
                case GameControllerInputKind.Back:
                    _ = controller.BackButtonPressed();
                    break;
                case GameControllerInputKind.Menu:
                    _ = controller.MenuButtonPressed();
                    break;
                case GameControllerInputKind.PauseButton:
                    controller.OnButtonPressed(GameControllerButtonId.Pause);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input));
            }
        }

        public static IEnumerable<object[]> StableInputCases()
        {
            yield return [(int)GameControllerInputKind.Back, (int)GameControllerOverlayMode.Gameplay, (int)GameControllerInputCommand.OpenPause];
            yield return [(int)GameControllerInputKind.Menu, (int)GameControllerOverlayMode.Gameplay, (int)GameControllerInputCommand.OpenPause];
            yield return [(int)GameControllerInputKind.PauseButton, (int)GameControllerOverlayMode.Gameplay, (int)GameControllerInputCommand.OpenPause];
            yield return [(int)GameControllerInputKind.Back, (int)GameControllerOverlayMode.Paused, (int)GameControllerInputCommand.Resume];
            yield return [(int)GameControllerInputKind.Menu, (int)GameControllerOverlayMode.Paused, (int)GameControllerInputCommand.Resume];
            yield return [(int)GameControllerInputKind.PauseButton, (int)GameControllerOverlayMode.Paused, (int)GameControllerInputCommand.Ignore];
            yield return [(int)GameControllerInputKind.Back, (int)GameControllerOverlayMode.Results, (int)GameControllerInputCommand.ExitResults];
            yield return [(int)GameControllerInputKind.Menu, (int)GameControllerOverlayMode.Results, (int)GameControllerInputCommand.Ignore];
            yield return [(int)GameControllerInputKind.PauseButton, (int)GameControllerOverlayMode.Results, (int)GameControllerInputCommand.Ignore];
        }

        public static IEnumerable<object[]> GatedInputCases()
        {
            foreach (GameControllerInputKind input in Enum.GetValues<GameControllerInputKind>())
            {
                foreach (GameControllerOverlayMode overlay in Enum.GetValues<GameControllerOverlayMode>())
                {
                    yield return [(int)input, (int)overlay, (int)RestartPhase.FadingOut];
                    yield return [(int)input, (int)overlay, (int)RestartPhase.FadingIn];
                }
            }
        }

        [Theory]
        [MemberData(nameof(StableInputCases))]
        public void ResolveMapsStableInputByOverlayMode(
            int input,
            int overlay,
            int expected)
        {
            Assert.Equal(
                (GameControllerInputCommand)expected,
                GameControllerInput.Resolve(
                    (GameControllerInputKind)input,
                    (GameControllerOverlayMode)overlay,
                    RestartPhase.Playing,
                    resultExitAllowed: true));
        }

        [Theory]
        [MemberData(nameof(GatedInputCases))]
        public void ResolveIgnoresInputDuringRestart(
            int input,
            int overlay,
            int restartPhase)
        {
            Assert.Equal(
                GameControllerInputCommand.Ignore,
                GameControllerInput.Resolve(
                    (GameControllerInputKind)input,
                    (GameControllerOverlayMode)overlay,
                    (RestartPhase)restartPhase,
                    resultExitAllowed: true));
        }

        [Fact]
        public void ResolveIgnoresResultBackWhenResultExitIsDisallowed()
        {
            Assert.Equal(
                GameControllerInputCommand.Ignore,
                GameControllerInput.Resolve(
                    GameControllerInputKind.Back,
                    GameControllerOverlayMode.Results,
                    RestartPhase.Playing,
                    resultExitAllowed: false));
        }

        [Fact]
        public void BackOpensPauseDuringNormalGameplay()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            _ = controller.BackButtonPressed();

            Assert.False(scene.updateable);
            Assert.True(PauseMenu(controller).IsEnabled());
        }

        [Fact]
        public void MenuOpensPauseDuringNormalGameplay()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            _ = controller.MenuButtonPressed();

            Assert.False(scene.updateable);
            Assert.True(PauseMenu(controller).IsEnabled());
        }

        [Fact]
        public void BackResumesFromPause()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            _ = controller.BackButtonPressed();

            Assert.True(scene.updateable);
            Assert.False(PauseMenu(controller).IsEnabled());
        }

        [Fact]
        public void MenuResumesFromPause()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            _ = controller.MenuButtonPressed();

            Assert.True(scene.updateable);
            Assert.False(PauseMenu(controller).IsEnabled());
        }

        [Fact]
        public void BackExitsStableResultsAndRepeatedInputRemainsGuarded()
        {
            (GameController controller, _) = Load();
            controller.LevelWon(LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2));

            _ = controller.BackButtonPressed();
            Assert.Equal(GameController.EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT, controller.exitCode);
            controller.exitCode = 42;
            _ = controller.BackButtonPressed();

            Assert.Equal(42, controller.exitCode);
        }

        [Fact]
        public void MenuIsIgnoredOnStableResults()
        {
            (GameController controller, GameScene scene) = Load();
            controller.LevelWon(LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2));

            _ = controller.MenuButtonPressed();

            Assert.Equal(GameController.EXIT_CODE_FROM_PAUSE_MENU, controller.exitCode);
            Assert.False(scene.touchable);
            Assert.False(PauseMenu(controller).IsEnabled());
        }

        [Fact]
        public void BackIsIgnoredOnCustomLevelResults()
        {
            (GameController controller, _) = Load();

            try
            {
                CustomLevelSession.Activate("custom-result-input-test.xml");
                controller.LevelWon(LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2));

                _ = controller.BackButtonPressed();

                Assert.Equal(GameController.EXIT_CODE_FROM_PAUSE_MENU, controller.exitCode);
            }
            finally
            {
                CustomLevelSession.Clear();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BackAndMenuOpenPauseDuringOutcomeTransition(bool useBack)
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryScheduleLoss());

            _ = useBack
                ? controller.BackButtonPressed()
                : controller.MenuButtonPressed();

            Assert.False(scene.updateable);
            Assert.True(PauseMenu(controller).IsEnabled());
            Assert.Equal(GameController.EXIT_CODE_FROM_PAUSE_MENU, controller.exitCode);
        }

        [Fact]
        public void RestartBailsOutOfTheWinPresentation()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryBeginWin());

            controller.OnButtonPressed(GameControllerButtonId.Restart);

            Assert.Equal(RestartPhase.FadingOut, scene.gameplayFlow.Phase);
            Assert.False(scene.gameplayFlow.CompleteWinTransition());
        }

        [Fact]
        public void RestartBailsOutOfTheLossPresentation()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryScheduleLoss());

            controller.OnButtonPressed(GameControllerButtonId.Restart);

            Assert.Equal(RestartPhase.FadingOut, scene.gameplayFlow.Phase);
        }

        [Theory]
        [InlineData((int)GameControllerInputKind.Back)]
        [InlineData((int)GameControllerInputKind.Menu)]
        [InlineData((int)GameControllerInputKind.PauseButton)]
        public void EveryPauseInputEntersTheSamePausedPresentation(int input)
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            Press(controller, input);

            Assert.False(scene.touchable);
            Assert.False(scene.updateable);
            Assert.True(PauseMenu(controller).IsEnabled());
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_BUTTON).IsEnabled());
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_RESTART_BUTTON).IsEnabled());
        }

        [Theory]
        [InlineData((int)GameControllerInputKind.Back)]
        [InlineData((int)GameControllerInputKind.Menu)]
        [InlineData((int)GameControllerInputKind.PauseButton)]
        public void EveryPauseInputOpensPauseDuringOutcomeTransition(int input)
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryScheduleLoss());

            Press(controller, input);

            Assert.False(scene.touchable);
            Assert.False(scene.updateable);
            Assert.True(PauseMenu(controller).IsEnabled());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BackAndMenuCannotReplaceAnExitRouteDuringLevelQuit(bool useBack)
        {
            (GameController controller, _) = Load();
            controller.OnButtonPressed(GameControllerButtonId.MainMenu);
            Assert.Equal(GameController.EXIT_CODE_FROM_PAUSE_MENU, controller.exitCode);

            _ = useBack
                ? controller.BackButtonPressed()
                : controller.MenuButtonPressed();

            Assert.Equal(GameController.EXIT_CODE_FROM_PAUSE_MENU, controller.exitCode);
        }
    }
}
