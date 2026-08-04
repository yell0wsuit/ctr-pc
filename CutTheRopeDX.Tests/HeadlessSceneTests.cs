using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class HeadlessSceneTests
    {
        [Fact]
        public void SceneLoadsAndStepsWithoutGraphicsDevice()
        {
            _ = HeadlessGame.Boot();

            // 2-5 has no tutorial text, so it exercises the plain load path.
            GameScene scene = HeadlessGame.LoadLevel(pack: 1, level: 4);
            HeadlessGame.StepFrames(scene, 60);

            Assert.NotNull(scene);
        }

        [Fact]
        public void RestartDimCompletesAndReturnsToPlaying()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(pack: 1, level: 4);
            HeadlessGame.StepFrames(scene, 60);

            scene.AnimateLevelRestart();
            Assert.Equal(RestartPhase.FadingOut, scene.gameplayFlow.Phase);

            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(RestartPhase.Playing, scene.gameplayFlow.Phase);
        }

        [Fact]
        public void TutorialTextLevelLoadsAndSteps()
        {
            _ = HeadlessGame.Boot();

            // 1-1 carries tutorial text, so it drives Text.UpdateDrawerValues through
            // HeadlessFont's single-charmap stub — the one font path the stub could break.
            GameScene scene = HeadlessGame.LoadLevel(pack: 0, level: 0);
            HeadlessGame.StepFrames(scene, 60);

            Assert.NotNull(scene);
        }

        [Fact]
        public void SoundIsSilentAndDoesNotThrowWithoutContentManager()
        {
            _ = HeadlessGame.Boot();
            GameScene scene = HeadlessGame.LoadLevel(pack: 1, level: 4);

            // Gameplay fires PlayOmNomSound/PlaySound constantly; SoundMgr.GetSound swallows
            // the null ContentManager. This pins that assumption rather than trusting it.
            HeadlessGame.StepFrames(scene, 120);
        }
    }
}
