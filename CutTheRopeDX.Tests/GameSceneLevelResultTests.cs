using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class GameSceneLevelResultTests
    {
        private static (GameScene Scene, RecordingSceneDelegate Delegate) Load()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(pack: 1, level: 4);
            RecordingSceneDelegate recorder = new();
            scene.gameSceneDelegate = recorder;
            return (scene, recorder);
        }

        [Fact]
        public void WinCallbackReceivesResultCapturedWhenGameWonBegins()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();
            scene.time = 29.875f;
            scene.starsCollected = 2;

            scene.GameWon();
            scene.time = 100f;
            scene.starsCollected = 0;
            HeadlessGame.StepFrames(scene, 150);

            Assert.Equal(1, recorder.WonCount);
            LevelResult result = Assert.IsType<LevelResult>(recorder.LastResult);
            Assert.Equal(29.875f, result.ElapsedTime);
            Assert.Equal(2, result.StarsCollected);
            Assert.Equal(12.5f, result.TimeBonus);
            Assert.Equal(2000, result.StarBonus);
            Assert.Equal(2013, result.FinalScore);
        }

        [Fact]
        public void RepeatedGameWonDeliversOnlyTheFirstResult()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();
            scene.time = 29.875f;
            scene.starsCollected = 2;

            scene.GameWon();
            scene.time = 5f;
            scene.starsCollected = 3;
            scene.GameWon();
            HeadlessGame.StepFrames(scene, 150);

            Assert.Equal(1, recorder.WonCount);
            Assert.Equal(2013, recorder.LastResult?.FinalScore);
        }

        [Fact]
        public void RestartClearsResultAwaitingDelayedDelivery()
        {
            (GameScene scene, RecordingSceneDelegate recorder) = Load();
            scene.GameWon();

            scene.Restart();
            HeadlessGame.StepFrames(scene, 150);

            Assert.Equal(0, recorder.WonCount);
            Assert.Null(recorder.LastResult);
        }
    }
}
