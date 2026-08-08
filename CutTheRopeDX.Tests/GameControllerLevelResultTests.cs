using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class GameControllerLevelResultTests
    {
        private const int Pack = 1;
        private const int Level = 4;

        private static (GameController Controller, GameScene Scene, BoxOpenClose Box, CTRRootController Root) Load()
        {
            _ = HeadlessGame.Boot();
            GameController controller = HeadlessGame.LoadLevelWithController(Pack, Level);
            View view = controller.GetView(0);
            return (
                controller,
                (GameScene)view.GetChild(0),
                (BoxOpenClose)view.GetChild(4),
                (CTRRootController)Application.SharedRootController());
        }

        private static void SetConflictingSceneResult(GameScene scene)
        {
            scene.time = 1f;
            scene.starsCollected = 3;
        }

        [Fact]
        public void SuppliedResultDrivesPresentationAnimationAndPersistence()
        {
            (GameController controller, GameScene scene, BoxOpenClose box, CTRRootController root) = Load();
            int saveBox = root.GetBox();
            int originalScore = CTRPreferences.GetScoreForPackLevel(saveBox, Pack, Level);
            int originalStars = CTRPreferences.GetStarsForPackLevel(saveBox, Pack, Level);
            LevelResult result = LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2);

            try
            {
                CTRPreferences.SetScoreForPackLevel(saveBox, 100, Pack, Level);
                CTRPreferences.SetStarsForPackLevel(saveBox, 1, Pack, Level);
                SetConflictingSceneResult(scene);

                controller.LevelWon(result);

                Assert.Equal(13, ((Image)box.result.GetChildWithName("star1")).quadToDraw);
                Assert.Equal(13, ((Image)box.result.GetChildWithName("star2")).quadToDraw);
                Assert.Equal(14, ((Image)box.result.GetChildWithName("star3")).quadToDraw);
                Assert.Equal(Application.GetString("LEVEL_CLEARED3"), ((Text)box.result.GetChildWithName("passText")).GetString());
                Assert.Equal(result, box.ActiveResult);
                Assert.True(box.shouldShowImprovedResult);
                Assert.False(box.shouldShowConfetti);
                Assert.Equal(result.FinalScore, CTRPreferences.GetScoreForPackLevel(saveBox, Pack, Level));
                Assert.Equal(result.StarsCollected, CTRPreferences.GetStarsForPackLevel(saveBox, Pack, Level));
            }
            finally
            {
                CTRPreferences.SetScoreForPackLevel(saveBox, originalScore, Pack, Level);
                CTRPreferences.SetStarsForPackLevel(saveBox, originalStars, Pack, Level);
            }
        }

        [Fact]
        public void SuppliedResultControlsImprovementComparisons()
        {
            (GameController controller, GameScene scene, BoxOpenClose box, CTRRootController root) = Load();
            int saveBox = root.GetBox();
            int originalScore = CTRPreferences.GetScoreForPackLevel(saveBox, Pack, Level);
            int originalStars = CTRPreferences.GetStarsForPackLevel(saveBox, Pack, Level);
            LevelResult result = LevelResultCalculator.Calculate(elapsedTime: 20f, starsCollected: 2);

            try
            {
                CTRPreferences.SetScoreForPackLevel(saveBox, 4000, Pack, Level);
                CTRPreferences.SetStarsForPackLevel(saveBox, 3, Pack, Level);
                SetConflictingSceneResult(scene);

                controller.LevelWon(result);

                Assert.False(box.shouldShowImprovedResult);
                Assert.Equal(4000, CTRPreferences.GetScoreForPackLevel(saveBox, Pack, Level));
                Assert.Equal(3, CTRPreferences.GetStarsForPackLevel(saveBox, Pack, Level));
            }
            finally
            {
                CTRPreferences.SetScoreForPackLevel(saveBox, originalScore, Pack, Level);
                CTRPreferences.SetStarsForPackLevel(saveBox, originalStars, Pack, Level);
            }
        }

        [Fact]
        public void CustomLevelDisplaysResultWithoutPersistingIt()
        {
            (GameController controller, GameScene scene, BoxOpenClose box, CTRRootController root) = Load();
            int saveBox = root.GetBox();
            int originalScore = CTRPreferences.GetScoreForPackLevel(saveBox, Pack, Level);
            int originalStars = CTRPreferences.GetStarsForPackLevel(saveBox, Pack, Level);
            LevelResult result = LevelResultCalculator.Calculate(elapsedTime: 12.5f, starsCollected: 3);

            try
            {
                CTRPreferences.SetScoreForPackLevel(saveBox, 100, Pack, Level);
                CTRPreferences.SetStarsForPackLevel(saveBox, 1, Pack, Level);
                SetConflictingSceneResult(scene);
                CustomLevelSession.Activate("controller-result-test.xml");

                controller.LevelWon(result);

                Assert.Equal(result, box.ActiveResult);
                Assert.Equal(Application.GetString("LEVEL_CLEARED4"), ((Text)box.result.GetChildWithName("passText")).GetString());
                Assert.True(box.shouldShowConfetti);
                Assert.Equal(100, CTRPreferences.GetScoreForPackLevel(saveBox, Pack, Level));
                Assert.Equal(1, CTRPreferences.GetStarsForPackLevel(saveBox, Pack, Level));
            }
            finally
            {
                CustomLevelSession.Clear();
                CTRPreferences.SetScoreForPackLevel(saveBox, originalScore, Pack, Level);
                CTRPreferences.SetStarsForPackLevel(saveBox, originalStars, Pack, Level);
            }
        }

        [Fact]
        public void RpcPayloadComesFromSuppliedResult()
        {
            LevelResult result = LevelResultCalculator.Calculate(elapsedTime: 29.875f, starsCollected: 2);

            LevelResultRpcPayload payload = LevelResultRpcPayload.From(result);

            Assert.Equal(2, payload.Stars);
            Assert.Equal(2013, payload.Score);
            Assert.Equal(29, payload.ElapsedSeconds);
        }
    }
}
