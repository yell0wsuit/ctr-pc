using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins the level-flow behavior that exists today. These must stay green through the
    /// LevelFlowState refactor; a red test means behavior changed, not that the test is wrong.
    /// </summary>
    public sealed class LevelFlowCharacterizationTests
    {
        private const int MaxOutcomeFrames = 900;

        private static (GameScene Scene, RecordingSceneDelegate Delegate) Load(int level = 4)
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(pack: 1, level);
            RecordingSceneDelegate recorder = new();
            scene.gameSceneDelegate = recorder;
            return (scene, recorder);
        }

        [Fact]
        public void SpikeContactSchedulesThenTriggersLoss()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();

            scene.ReleaseRopesForBody(scene.Candy().WholeBody);
            for (int frame = 0; frame < MaxOutcomeFrames && !scene.gameplayFlow.TransitionActive; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
            }

            // Level 2-5 drops the candy onto a spike. Hazard loss is deliberately delayed so
            // the break animation can play, but the transition gate closes immediately.
            Assert.True(scene.gameplayFlow.TransitionActive);
            Assert.False(scene.gameplayFlow.LostTriggered);

            for (int frame = 0; frame < MaxOutcomeFrames && recorder.LostCount == 0; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
            }

            Assert.Equal(1, recorder.LostCount);
            Assert.Equal(0, recorder.WonCount);
            Assert.True(scene.gameplayFlow.LostTriggered);
        }

        [Fact]
        public void CandyLeavesScreenTriggersLoss()
        {
            // Level 2-1 has no hazards and the target is outside the candy's fall line.
            (GameScene scene, RecordingSceneDelegate recorder) = Load(level: 0);

            scene.ReleaseRopesForBody(scene.Candy().WholeBody);
            for (int frame = 0; frame < MaxOutcomeFrames && recorder.LostCount == 0; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
            }

            Assert.Equal(1, recorder.LostCount);
            Assert.Equal(0, recorder.WonCount);
            Assert.True(scene.gameplayFlow.LostTriggered);
        }

        [Fact]
        public void HazardLossFiresExactlyOnceWhenSteppedFurther()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();

            scene.ReleaseRopesForBody(scene.Candy().WholeBody);
            for (int frame = 0; frame < MaxOutcomeFrames && recorder.LostCount == 0; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
            }
            HeadlessGame.StepFrames(scene, 300);

            Assert.Equal(1, recorder.LostCount);
        }

        [Fact]
        public void WinFiresExactlyOnceWhenTriggeredAgain()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();

            scene.GameWon();
            scene.GameWon();
            HeadlessGame.StepFrames(scene, 150);

            Assert.Equal(1, recorder.WonCount);
            Assert.Equal(0, recorder.LostCount);
            Assert.True(scene.gameplayFlow.WonTriggered);
        }

        [Fact]
        public void RestartDimWalksThroughBothPhasesBackToPlaying()
        {
            (GameScene scene, _) = Load();
            HeadlessGame.StepFrames(scene, 30);

            scene.AnimateLevelRestart();
            Assert.Equal(RestartPhase.FadingOut, scene.gameplayFlow.Phase);

            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(RestartPhase.Playing, scene.gameplayFlow.Phase);
            Assert.Equal(0f, scene.gameplayFlow.DimTime);
        }

        [Fact]
        public void NoOutcomeFiresDuringNormalPlay()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();

            HeadlessGame.StepFrames(scene, 300);

            Assert.Equal(0, recorder.LostCount);
            Assert.Equal(0, recorder.WonCount);
        }

        [Fact]
        public void OutcomeTransitionIsInactiveDuringNormalPlay()
        {
            (GameScene scene, _) = Load();

            HeadlessGame.StepFrames(scene, 120);

            Assert.False(scene.gameplayFlow.TransitionActive);
        }
    }
}
