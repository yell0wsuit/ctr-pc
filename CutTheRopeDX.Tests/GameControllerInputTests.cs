using CutTheRopeDX.GameMain;
using CutTheRopeDX.Framework.Visual;

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

        [Fact]
        public void CanPauseFromGameplayFalseDuringOutcomeTransition()
        {
            Assert.False(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: true,
                restartDimActive: false));
        }

        [Fact]
        public void CanPauseFromGameplayTrueWhenOutcomeTransitionInactive()
        {
            Assert.True(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: false));
        }

        [Fact]
        public void CannotPauseWhileRestartDimIsPlaying()
        {
            Assert.False(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: true));
        }

        [Fact]
        public void CanPauseOnceRestartDimHasFinished()
        {
            Assert.True(GameControllerInput.CanPauseFromGameplay(
                gameplayHudTouchable: true,
                outcomeTransitionActive: false,
                restartDimActive: false));
        }

        [Fact]
        public void CanExitResultWithBackFalseDuringOutcomeTransition()
        {
            Assert.False(GameControllerInput.CanExitResultWithBack(
                resultTouchable: true,
                outcomeTransitionActive: true));
        }

        [Fact]
        public void CanExitResultWithBackTrueAfterTransition()
        {
            Assert.True(GameControllerInput.CanExitResultWithBack(
                resultTouchable: true,
                outcomeTransitionActive: false));
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
            _ = controller.BackButtonPressed();

            Assert.Equal(GameController.EXIT_CODE_FROM_PAUSE_MENU_LEVEL_SELECT, controller.exitCode);
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BackAndMenuAreIgnoredDuringOutcomeTransition(bool useBack)
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            scene.gameplayFlow.MarkTransitionActive();

            _ = useBack
                ? controller.BackButtonPressed()
                : controller.MenuButtonPressed();

            Assert.True(scene.updateable);
            Assert.False(PauseMenu(controller).IsEnabled());
            Assert.Equal(GameController.EXIT_CODE_FROM_PAUSE_MENU, controller.exitCode);
        }
    }
}
