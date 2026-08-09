using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class PauseDuringRestartTests
    {
        private static (GameController Controller, GameScene Scene) Load()
        {
            _ = HeadlessGame.Boot();
            GameController controller = HeadlessGame.LoadLevelWithController(pack: 1, level: 4);
            return (controller, (GameScene)controller.GetView(0).GetChild(0));
        }

        private static Text PauseMapNameLabel(GameController controller)
        {
            return (Text)controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).GetChildWithName("mapNameLabel");
        }

        [Fact]
        public void PausingNamedCustomLevelShowsResolvedLevelName()
        {
            (GameController controller, GameScene scene) = Load();
            scene.levelName = "My Custom Level";

            try
            {
                CustomLevelSession.Activate("pause-name-test.xml");

                controller.OnButtonPressed(GameControllerButtonId.Pause);

                Assert.Equal("My Custom Level", PauseMapNameLabel(controller).GetString());
            }
            finally
            {
                CustomLevelSession.Clear();
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void PausingUnnamedCustomLevelShowsBlankLabel(string levelName)
        {
            (GameController controller, GameScene scene) = Load();
            scene.levelName = levelName;

            try
            {
                CustomLevelSession.Activate("pause-name-test.xml");

                controller.OnButtonPressed(GameControllerButtonId.Pause);

                Assert.Equal(string.Empty, PauseMapNameLabel(controller).GetString());
            }
            finally
            {
                CustomLevelSession.Clear();
            }
        }

        [Fact]
        public void PausingNormalLevelShowsBestScore()
        {
            (GameController controller, _) = Load();
            CTRRootController root = (CTRRootController)Application.SharedRootController();
            int score = CTRPreferences.GetScoreForPackLevel(root.GetBox(), root.GetPack(), root.GetLevel());

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.Equal(Application.GetString("BEST_SCORE") + ": " + score, PauseMapNameLabel(controller).GetString());
        }

        [Fact]
        public void PauseIsRefusedDuringRestartDim()
        {
            (GameController controller, GameScene scene) = Load();

            scene.AnimateLevelRestart();
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            // Entering pause freezes the scene via updateable; still true means the pause was refused.
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
            Assert.True(scene.gameplayFlow.TryBeginRestartDim());
            Assert.Equal(RestartStep.SwapScene, scene.gameplayFlow.Advance(1f));

            _ = controller.BackButtonPressed();

            Assert.Equal(0, controller.exitCode);
            Assert.True(scene.updateable);
            Assert.Equal(RestartPhase.FadingIn, scene.gameplayFlow.Phase);
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
        }

        [Fact]
        public void MenuInputDuringRestartFadeOutIsIgnored()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Restart);
            _ = controller.MenuButtonPressed();

            Assert.True(scene.updateable);
            Assert.Equal(RestartPhase.FadingOut, scene.gameplayFlow.Phase);
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
        }

        [Fact]
        public void MenuInputDuringRestartFadeInIsIgnored()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryBeginRestartDim());
            Assert.Equal(RestartStep.SwapScene, scene.gameplayFlow.Advance(1f));

            _ = controller.MenuButtonPressed();

            Assert.True(scene.updateable);
            Assert.Equal(RestartPhase.FadingIn, scene.gameplayFlow.Phase);
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
        }

        [Fact]
        public void PauseButtonInputDuringRestartFadeOutIsIgnored()
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
        public void PauseButtonInputDuringRestartFadeInIsIgnored()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryBeginRestartDim());
            Assert.Equal(RestartStep.SwapScene, scene.gameplayFlow.Advance(1f));

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.True(scene.updateable);
            Assert.Equal(RestartPhase.FadingIn, scene.gameplayFlow.Phase);
            Assert.False(controller.GetView(0).GetChild(GameView.VIEW_ELEMENT_PAUSE_MENU).IsEnabled());
        }
    }
}
