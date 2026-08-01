using CutTheRopeDX.GameMain;

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
            HeadlessGame ctr = HeadlessGame.Boot();
            GameScene scene = ctr.LoadLevel(pack: 1, level);
            RecordingSceneDelegate recorder = new();
            scene.gameSceneDelegate = recorder;
            return (scene, recorder);
        }

        [Fact]
        public void SpikeContact_SchedulesThenTriggersLoss()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();

            scene.ReleaseAllRopes(false);
            for (int frame = 0; frame < MaxOutcomeFrames && !scene.outcomeTransitionActive; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
            }

            // Level 2-5 drops the candy onto a spike. Hazard loss is deliberately delayed so
            // the break animation can play, but the transition gate closes immediately.
            Assert.True(scene.outcomeTransitionActive);
            Assert.False(scene.gameLostTriggered);

            for (int frame = 0; frame < MaxOutcomeFrames && recorder.LostCount == 0; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
            }

            Assert.Equal(1, recorder.LostCount);
            Assert.Equal(0, recorder.WonCount);
            Assert.True(scene.gameLostTriggered);
        }

        [Fact]
        public void CandyLeavesScreen_TriggersLoss()
        {
            // Level 2-1 has no hazards and the target is outside the candy's fall line.
            (GameScene scene, RecordingSceneDelegate recorder) = Load(level: 0);

            scene.ReleaseAllRopes(false);
            for (int frame = 0; frame < MaxOutcomeFrames && recorder.LostCount == 0; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
            }

            Assert.Equal(1, recorder.LostCount);
            Assert.Equal(0, recorder.WonCount);
            Assert.True(scene.gameLostTriggered);
        }

        [Fact]
        public void HazardLoss_FiresExactlyOnce_WhenSteppedFurther()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();

            scene.ReleaseAllRopes(false);
            for (int frame = 0; frame < MaxOutcomeFrames && recorder.LostCount == 0; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
            }
            HeadlessGame.StepFrames(scene, 300);

            Assert.Equal(1, recorder.LostCount);
        }

        [Fact]
        public void Win_FiresExactlyOnce_WhenTriggeredAgain()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();

            scene.GameWon();
            scene.GameWon();
            HeadlessGame.StepFrames(scene, 150);

            Assert.Equal(1, recorder.WonCount);
            Assert.Equal(0, recorder.LostCount);
            Assert.True(scene.gameWonTriggered);
        }

        [Fact]
        public void RestartDim_WalksThroughBothPhasesBackToPlaying()
        {
            (GameScene scene, _) = Load();
            HeadlessGame.StepFrames(scene, 30);

            scene.AnimateLevelRestart();
            Assert.Equal(0, scene.restartState);

            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(-1, scene.restartState);
            Assert.Equal(0f, scene.dimTime);
        }

        [Fact]
        public void NoOutcome_FiresDuringNormalPlay()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();

            HeadlessGame.StepFrames(scene, 300);

            Assert.Equal(0, recorder.LostCount);
            Assert.Equal(0, recorder.WonCount);
        }

        [Fact]
        public void OutcomeTransition_IsInactiveDuringNormalPlay()
        {
            (GameScene scene, _) = Load();

            HeadlessGame.StepFrames(scene, 120);

            Assert.False(scene.outcomeTransitionActive);
        }
    }
}
