using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class BoxOpenCloseLevelResultTests
    {
        private static readonly LevelResult FractionalResult = new(
            ElapsedTime: 29.875f,
            StarsCollected: 2,
            StarBonus: 2000,
            TimeBonus: 12.5f,
            FinalScore: 2013);

        private static BoxOpenClose CreateBox()
        {
            _ = HeadlessGame.Boot();
            return new BoxOpenClose().InitWithButtonDelegate(new NoOpButtonDelegate());
        }

        private static void AdvanceToTimeCountdown(BoxOpenClose box)
        {
            for (int frame = 0; frame < 100 && box.raState != BoxOpenClose.RESULT_STATE_COUNTDOWN_TIME_BONUS; frame++)
            {
                box.Update(0.1f);
            }

            Assert.Equal(BoxOpenClose.RESULT_STATE_COUNTDOWN_TIME_BONUS, box.raState);
        }

        [Fact]
        public void InitialAnimationStateComesFromLevelResult()
        {
            BoxOpenClose box = CreateBox();

            box.LevelWon(FractionalResult);
            box.Update(0f);

            Assert.Equal(FractionalResult, box.ActiveResult);
            Assert.Equal(FractionalResult.StarBonus, box.cstarBonus);
            Assert.Equal(FractionalResult.ElapsedTime, box.ctime);
        }

        [Fact]
        public void TimeCountdownUsesElapsedTimeAndPreciseTimeBonus()
        {
            BoxOpenClose box = CreateBox();
            box.LevelWon(FractionalResult);
            AdvanceToTimeCountdown(box);

            box.Update(0.5f);

            Assert.Equal(FractionalResult.ElapsedTime * 0.5f, box.ctime);
            Assert.Equal(2006, box.cscore);
        }

        [Fact]
        public void CompletedCountdownUsesExactFinalScore()
        {
            BoxOpenClose box = CreateBox();
            box.LevelWon(FractionalResult);
            AdvanceToTimeCountdown(box);

            box.Update(1f);

            Assert.Equal(FractionalResult.FinalScore, box.cscore);
        }

        private sealed class NoOpButtonDelegate : IButtonDelegation
        {
            public void OnButtonPressed(ButtonId buttonId)
            {
            }
        }
    }
}
