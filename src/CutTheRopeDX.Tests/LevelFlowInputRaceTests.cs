using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Verification tests for pause/restart races around outcome and restart-dim boundaries.
    /// These characterize behavior that already holds; they exist so lifting the pause and restart
    /// blockers cannot silently reintroduce a race.
    /// </summary>
    public class LevelFlowInputRaceTests
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

        /// <summary>
        /// Advances the scene the way its parent does: <c>BaseElement.Update</c> only recurses into
        /// children whose <c>updateable</c> is set, so a paused scene must not tick. The harness's
        /// <c>StepFrames</c> calls <c>Update</c> directly and would bypass that gate.
        /// </summary>
        private static void StepHonoringUpdateable(GameScene scene, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                if (scene.updateable)
                {
                    scene.Update(0.016f);
                }
            }
        }

        [Fact]
        public void PauseRequestedAfterRestartInTheSameFrameIsIgnored()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Restart);
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.Equal(RestartPhase.FadingOut, scene.gameplayFlow.Phase);
            Assert.False(PauseMenu(controller).IsEnabled());
            Assert.True(scene.updateable);
        }

        [Fact]
        public void RestartRequestedAfterPauseInTheSameFrameIsIgnored()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);

            controller.OnButtonPressed(GameControllerButtonId.Pause);
            controller.OnButtonPressed(GameControllerButtonId.Restart);

            Assert.Equal(RestartPhase.Playing, scene.gameplayFlow.Phase);
            Assert.True(PauseMenu(controller).IsEnabled());
        }

        [Fact]
        public void PauseSpammedThroughAWholeDimNeverOpensAndTheDimStillCompletes()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            controller.OnButtonPressed(GameControllerButtonId.Restart);

            for (int i = 0; i < 120 && scene.gameplayFlow.Phase != RestartPhase.Playing; i++)
            {
                controller.OnButtonPressed(GameControllerButtonId.Pause);
                Assert.False(PauseMenu(controller).IsEnabled());
                StepHonoringUpdateable(scene, 1);
            }

            Assert.Equal(RestartPhase.Playing, scene.gameplayFlow.Phase);
            Assert.Equal(LevelOutcomeState.Playing, scene.gameplayFlow.Outcome);
        }

        [Fact]
        public void RestartSpammedThroughADimCannotExtendIt()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            controller.OnButtonPressed(GameControllerButtonId.Restart);
            float previousDim = scene.gameplayFlow.DimTime;

            while (scene.gameplayFlow.Phase == RestartPhase.FadingOut)
            {
                controller.OnButtonPressed(GameControllerButtonId.Restart);
                Assert.True(scene.gameplayFlow.DimTime <= previousDim);
                previousDim = scene.gameplayFlow.DimTime;
                StepHonoringUpdateable(scene, 1);
            }

            Assert.NotEqual(RestartPhase.FadingOut, scene.gameplayFlow.Phase);
        }

        [Fact]
        public void PausingDuringALossStopsTheSceneSoItsCutsceneCannotAdvance()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryBeginLoss());

            controller.OnButtonPressed(GameControllerButtonId.Pause);
            StepHonoringUpdateable(scene, 300);

            Assert.False(scene.updateable);
            Assert.Equal(LevelOutcomeState.Losing, scene.gameplayFlow.Outcome);
            Assert.Equal(RestartPhase.Playing, scene.gameplayFlow.Phase);
        }

        [Fact]
        public void ResumingAfterPausingDuringALossKeepsTheLevelLost()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryBeginLoss());
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            _ = controller.BackButtonPressed();

            // The outcome must survive the round trip. If it reset to Playing the level would be
            // lost while Om Nom went back to reacting to candy.
            Assert.Equal(LevelOutcomeState.Losing, scene.gameplayFlow.Outcome);
            Assert.False(scene.gameplayFlow.CanReactToCandy());
            Assert.True(scene.updateable);
        }

        [Fact]
        public void ResumingAfterPausingDuringAWinKeepsTheWinPending()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryBeginWin());
            controller.OnButtonPressed(GameControllerButtonId.Pause);

            _ = controller.BackButtonPressed();

            Assert.Equal(LevelOutcomeState.Winning, scene.gameplayFlow.Outcome);
            Assert.True(scene.gameplayFlow.CompleteWinTransition());
        }

        [Fact]
        public void ADimStartedByALossStillRefusesPause()
        {
            (GameController controller, GameScene scene) = Load();
            HeadlessGame.StepFrames(scene, 60);
            Assert.True(scene.gameplayFlow.TryBeginLoss());
            Assert.True(scene.gameplayFlow.TryBeginRestartDim());

            controller.OnButtonPressed(GameControllerButtonId.Pause);

            Assert.False(PauseMenu(controller).IsEnabled());
            Assert.Equal(LevelOutcomeState.Lost, scene.gameplayFlow.Outcome);
        }
    }
}
