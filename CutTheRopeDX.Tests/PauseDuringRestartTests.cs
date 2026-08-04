using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class PauseDuringRestartTests
    {
        private static (GameController Controller, GameScene Scene) Load()
        {
            HeadlessGame ctr = HeadlessGame.Boot();
            GameController controller = ctr.LoadLevelWithController(pack: 1, level: 4);
            return (controller, (GameScene)controller.GetView(0).GetChild(0));
        }

        [Fact]
        public void PauseIsRefusedDuringRestartDim()
        {
            (GameController controller, GameScene scene) = Load();

            scene.AnimateLevelRestart();
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            // SetPaused freezes the scene via updateable; still true means the pause was refused.
            Assert.True(scene.updateable);
        }

        [Fact]
        public void PauseIsAllowedOnceRestartDimFinishes()
        {
            (GameController controller, GameScene scene) = Load();

            scene.AnimateLevelRestart();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.False(scene.updateable);
        }

        [Fact]
        public void PauseIsAllowedDuringNormalPlay()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.False(scene.updateable);
        }

        [Fact]
        public void PauseAndRestartSameFramePauseDispatchedFirstKeepsGamePaused()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Pause);
            controller.OnButtonPressed(GameControllerButtonId.Restart);

            Assert.False(scene.updateable);
            Assert.Equal(RestartPhase.Playing, scene.gameplayFlow.Phase);
            Assert.True(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
        }

        [Fact]
        public void PauseAndRestartSameFrameRestartDispatchedFirstKeepsRestartRunning()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Restart);
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.True(scene.updateable);
            Assert.Equal(RestartPhase.FadingOut, scene.gameplayFlow.Phase);
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
        }

        [Fact]
        public void BackInputDuringRestartFadeOutIsIgnored()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Restart);
            _ = controller.BackButtonPressed();

            Assert.Equal(0, controller.exitCode);
            Assert.True(scene.updateable);
            Assert.Equal(RestartPhase.FadingOut, scene.gameplayFlow.Phase);
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
        }

        [Fact]
        public void BackInputDuringRestartFadeInIsIgnored()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            scene.gameplayFlow.BeginRestartDim();
            Assert.Equal(RestartStep.SwapScene, scene.gameplayFlow.Advance(1f));

            _ = controller.BackButtonPressed();

            Assert.Equal(0, controller.exitCode);
            Assert.True(scene.updateable);
            Assert.Equal(RestartPhase.FadingIn, scene.gameplayFlow.Phase);
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
        }
    }
}
