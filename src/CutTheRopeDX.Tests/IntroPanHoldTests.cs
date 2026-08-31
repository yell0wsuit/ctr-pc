using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the hold on gameplay while the opening camera pan is still crossing the level.
    /// </summary>
    public sealed class IntroPanHoldTests
    {
        /// <summary>Authored height that fills the design box exactly, so no pan is staged.</summary>
        private const int UnpannedHeight = 480;

        /// <summary>Authored height of a level two design boxes tall, which is panned across.</summary>
        private const int PannedHeight = 960;

        [Fact]
        public void NothingMovesWhileTheIntroPanIsStillRunning()
        {
            // The same unroped candy in a level small enough to need no pan falls straight away,
            // so the level below is held rather than merely having nothing to do.
            Assert.True(FallAfter(UnpannedHeight, frames: 30) > 1f);

            Assert.Equal(0f, FallAfter(PannedHeight, frames: 30), 0.001);
        }

        [Fact]
        public void GameplayResumesOnceTheIntroPanHandsInputBack()
        {
            Assert.True(FallAfter(PannedHeight, frames: 240) > 1f);
        }

        [Fact]
        public void TheRestartFadeFinishesOverTheIntroPanRatherThanWaitingForIt()
        {
            GameScene scene = FreeCandyLevel(PannedHeight);
            scene.AnimateLevelRestart();

            // Long enough for both dim phases and the scene swap between them, and far short of
            // the pan this level stages - which is the point: the fade must not be held by it.
            HeadlessGame.StepFrames(scene, 40);

            Assert.Equal(0f, scene.gameplayFlow.DimTime, 0.001);
            Assert.Equal(RestartPhase.Playing, scene.gameplayFlow.Phase);
        }

        /// <summary>
        /// Builds a level whose candy hangs from nothing, and reports how far it fell.
        /// </summary>
        /// <param name="mapHeight">Authored map height, which decides whether a pan is staged.</param>
        /// <param name="frames">Frames to advance.</param>
        /// <returns>World units the candy descended.</returns>
        private static float FallAfter(int mapHeight, int frames)
        {
            GameScene scene = FreeCandyLevel(mapHeight);
            float start = scene.Candy().WholeBody.Point.pos.Y;
            HeadlessGame.StepFrames(scene, frames);
            return scene.Candy().WholeBody.Point.pos.Y - start;
        }

        /// <summary>Builds a level whose candy hangs from nothing.</summary>
        /// <param name="mapHeight">Authored map height, which decides whether a pan is staged.</param>
        /// <returns>The loaded scene.</returns>
        private static GameScene FreeCandyLevel(int mapHeight)
        {
            return Scenario.New()
                .MapSize(320, mapHeight)
                .Candy(160, 60)
                .OmNom(160, mapHeight - 60)
                .Build();
        }
    }
}
