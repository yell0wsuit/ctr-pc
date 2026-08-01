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
        public void Pause_IsRefused_DuringRestartDim()
        {
            (GameController controller, GameScene scene) = Load();

            scene.AnimateLevelRestart();
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            // SetPaused freezes the scene via updateable; still true means the pause was refused.
            Assert.True(scene.updateable);
        }

        [Fact]
        public void Pause_IsAllowed_OnceRestartDimFinishes()
        {
            (GameController controller, GameScene scene) = Load();

            scene.AnimateLevelRestart();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.False(scene.updateable);
        }

        [Fact]
        public void Pause_IsAllowed_DuringNormalPlay()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.False(scene.updateable);
        }
    }
}
