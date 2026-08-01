using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class HeadlessSceneTests
    {
        [Fact]
        public void Scene_LoadsAndSteps_WithoutGraphicsDevice()
        {
            HeadlessGame ctr = HeadlessGame.Boot();

            // 2-5 has no tutorial text, so it exercises the plain load path.
            GameScene scene = ctr.LoadLevel(pack: 1, level: 4);
            HeadlessGame.StepFrames(scene, 60);

            Assert.NotNull(scene);
        }

        [Fact]
        public void RestartDim_CompletesAndReturnsToPlaying()
        {
            HeadlessGame ctr = HeadlessGame.Boot();
            GameScene scene = ctr.LoadLevel(pack: 1, level: 4);
            HeadlessGame.StepFrames(scene, 60);

            scene.AnimateLevelRestart();
            Assert.Equal(0, scene.restartState);

            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(-1, scene.restartState);
        }

        [Fact]
        public void Sound_IsSilentAndDoesNotThrow_WithoutContentManager()
        {
            HeadlessGame ctr = HeadlessGame.Boot();
            GameScene scene = ctr.LoadLevel(pack: 1, level: 4);

            // Gameplay fires PlayOmNomSound/PlaySound constantly; SoundMgr.GetSound swallows
            // the null ContentManager. This pins that assumption rather than trusting it.
            HeadlessGame.StepFrames(scene, 120);
        }
    }
}
