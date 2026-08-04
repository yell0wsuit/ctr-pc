using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyDecisionsTests
    {
        private static CandyView Candy(float x, float y)
        {
            return new CandyView(new Vector(x, y));
        }

        [Fact]
        public void ShouldOpenMouthTrueWhenCandyInRange()
        {
            Vector target = new(100, 100);
            List<CandyView> candies = [Candy(150, 100)]; // 50px away
            Assert.True(CandyDecisions.ShouldOpenMouth(target, candies, 200f));
        }

        [Fact]
        public void ShouldOpenMouthFalseForCandyLikeObjectThatCannotOpenMouth()
        {
            Vector target = new(100, 100);
            List<CandyView> candies = [new CandyView(new Vector(150, 100), CandyCapabilities.LightBulb)];

            Assert.False(CandyDecisions.ShouldOpenMouth(target, candies, 200f));
        }

        [Fact]
        public void ShouldOpenMouthFalseWhenCandyOutOfRange()
        {
            Vector target = new(100, 100);
            List<CandyView> candies = [Candy(400, 100)]; // 300px away
            Assert.False(CandyDecisions.ShouldOpenMouth(target, candies, 200f));
        }

        [Theory]
        [InlineData(nameof(CandyRemovalReason.Eaten), true)]
        [InlineData(nameof(CandyRemovalReason.Hazard), false)]
        [InlineData(nameof(CandyRemovalReason.Spider), false)]
        [InlineData(nameof(CandyRemovalReason.OffScreen), false)]
        public void AllEatenAcceptsOnlyEatenRemoval(string reasonName, bool expected)
        {
            CandyRemovalReason reason = Enum.Parse<CandyRemovalReason>(reasonName);

            CandyOutcomeView candy = new(
                CandyPresence.Removed,
                reason,
                CanBeEaten: true,
                HasFailedSplitHalf: false);

            Assert.Equal(expected, CandyDecisions.AllEaten([candy]));
        }

        [Theory]
        [InlineData(nameof(CandyPresence.Hidden))]
        [InlineData(nameof(CandyPresence.Split))]
        public void AllEatenRejectsIntactNonRemovedLifecycleStates(string presenceName)
        {
            CandyPresence presence = Enum.Parse<CandyPresence>(presenceName);

            CandyOutcomeView candy = new(
                presence,
                RemovalReason: null,
                CanBeEaten: true,
                HasFailedSplitHalf: false);

            Assert.False(CandyDecisions.AllEaten([candy]));
        }

        [Theory]
        [InlineData(nameof(CandyRemovalReason.Hazard))]
        [InlineData(nameof(CandyRemovalReason.Spider))]
        [InlineData(nameof(CandyRemovalReason.OffScreen))]
        public void AnyFailedRemovalAcceptsEveryLossReason(string reasonName)
        {
            CandyRemovalReason reason = Enum.Parse<CandyRemovalReason>(reasonName);

            CandyOutcomeView candy = new(
                CandyPresence.Removed,
                reason,
                CanBeEaten: true,
                HasFailedSplitHalf: false);

            Assert.True(CandyDecisions.AnyFailedRemoval([candy]));
        }

        [Fact]
        public void SplitHalfFailureRequestsLossAndNeverWin()
        {
            CandyOutcomeView candy = new(
                CandyPresence.Split,
                RemovalReason: null,
                CanBeEaten: true,
                HasFailedSplitHalf: true);

            Assert.True(CandyDecisions.AnyFailedRemoval([candy]));
            Assert.False(CandyDecisions.AllEaten([candy]));
        }

        [Fact]
        public void EatenRemovalDoesNotRequestLoss()
        {
            CandyOutcomeView candy = new(
                CandyPresence.Removed,
                CandyRemovalReason.Eaten,
                CanBeEaten: true,
                HasFailedSplitHalf: false);

            Assert.False(CandyDecisions.AnyFailedRemoval([candy]));
        }

        /// <summary>
        /// <see cref="CandyDecisions.AllEaten"/> is the win gate, and "every candy in an empty list
        /// was eaten" is vacuously true - so the guard has to be explicit or a candy-less level wins
        /// on its first frame.
        /// </summary>
        [Fact]
        public void NoCandiesAtAllNeverWins()
        {
            Assert.False(CandyDecisions.AllEaten([]));
        }

        /// <summary>
        /// Only the eatable candies gate the win, so a level holding nothing but light bulbs is
        /// won immediately - exactly what the helper this replaced did with its candy-count guard.
        /// </summary>
        [Fact]
        public void InedibleBodiesAloneSatisfyTheWinVacuously()
        {
            CandyOutcomeView bulb = new(
                CandyPresence.Present,
                RemovalReason: null,
                CanBeEaten: false,
                HasFailedSplitHalf: false);

            Assert.True(CandyDecisions.AllEaten([bulb]));
        }
    }
}
