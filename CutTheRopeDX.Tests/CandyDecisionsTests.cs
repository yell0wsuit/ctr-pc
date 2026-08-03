using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class CandyDecisionsTests
    {
        private static CandyView Candy(float x, float y, bool consumed)
        {
            return new CandyView(new Vector(x, y), consumed);
        }

        [Fact]
        public void AnyConsumablePresent_TrueWhenSecondaryCandyRemainsAfterPrimaryConsumed()
        {
            List<CandyView> candies =
            [
                Candy(0, 0, true),
                Candy(1, 1, false)
            ];

            Assert.True(CandyDecisions.AnyConsumablePresent(candies));
        }

        [Fact]
        public void AnyConsumablePresent_FalseWhenOnlyLightBulbRemains()
        {
            List<CandyView> candies =
            [
                Candy(0, 0, true),
                new CandyView(new Vector(1, 1), Consumed: false, InTransport: false, CandyCapabilities.LightBulb)
            ];

            Assert.False(CandyDecisions.AnyConsumablePresent(candies));
        }

        [Fact]
        public void AnyCandyBodyPresent_TrueWhenSplitHalfRemains()
        {
            // A split candy contributes one snapshot per surviving half, not a whole-body snapshot.
            List<CandyView> splitHalves = [Candy(10, 10, false), Candy(20, 20, true)];

            Assert.True(CandyDecisions.AnyCandyBodyPresent(splitHalves));
        }

        [Fact]
        public void AnyCandyBodyPresent_FalseWhenOnlyLightBulbRemains()
        {
            List<CandyView> candies =
            [
                new CandyView(new Vector(1, 1), Consumed: false, InTransport: false, CandyCapabilities.LightBulb)
            ];

            Assert.False(CandyDecisions.AnyCandyBodyPresent(candies));
        }

        [Fact]
        public void AnyUneatenOutOfScreen_TrueOnlyForUneatenOutside()
        {
            List<CandyView> candies = [Candy(0, 0, false), Candy(999, 999, true)];
            // Out-of-screen predicate: anything with |coord| >= 500.
            static bool IsOut(Vector p)
            {
                return p.X >= 500 || p.Y >= 500 || p.X <= -500 || p.Y <= -500;
            }

            Assert.False(CandyDecisions.AnyUneatenOutOfScreen(candies, IsOut)); // (0,0) inside; (999,999) eaten
        }

        [Fact]
        public void AnyUneatenOutOfScreen_TrueWhenUneatenCandyLeaves()
        {
            List<CandyView> candies = [Candy(0, 0, false), Candy(999, 0, false)];
            static bool IsOut(Vector p)
            {
                return p.X >= 500;
            }

            Assert.True(CandyDecisions.AnyUneatenOutOfScreen(candies, IsOut));
        }

        [Fact]
        public void AnyUneatenOutOfScreen_FalseForCandyLikeObjectThatCannotLoseLevel()
        {
            List<CandyView> candies =
            [
                new CandyView(new Vector(999, 0), Consumed: false, InTransport: false, CandyCapabilities.LightBulb)
            ];
            static bool IsOut(Vector p)
            {
                return p.X >= 500;
            }

            Assert.False(CandyDecisions.AnyUneatenOutOfScreen(candies, IsOut));
        }

        [Fact]
        public void AnyUneatenOutOfScreen_TrueWhenUneatenSplitHalfLeaves()
        {
            List<CandyView> candies = [];
            List<CandyView> splitCandies = [Candy(999, 0, false), Candy(0, 0, false)];
            static bool IsOut(Vector p)
            {
                return p.X >= 500;
            }

            Assert.True(CandyDecisions.AnyUneatenOutOfScreen(candies, splitCandies, IsOut));
        }

        [Fact]
        public void ShouldOpenMouth_TrueWhenUneatenCandyInRange()
        {
            Vector target = new(100, 100);
            List<CandyView> candies = [Candy(150, 100, false)]; // 50px away
            Assert.True(CandyDecisions.ShouldOpenMouth(target, candies, 200f));
        }

        [Fact]
        public void ShouldOpenMouth_FalseWhenOnlyEatenCandyInRange()
        {
            Vector target = new(100, 100);
            List<CandyView> candies = [Candy(150, 100, true)];
            Assert.False(CandyDecisions.ShouldOpenMouth(target, candies, 200f));
        }

        [Fact]
        public void ShouldOpenMouth_FalseForCandyLikeObjectThatCannotOpenMouth()
        {
            Vector target = new(100, 100);
            List<CandyView> candies =
            [
                new CandyView(new Vector(150, 100), Consumed: false, InTransport: false, CandyCapabilities.LightBulb)
            ];

            Assert.False(CandyDecisions.ShouldOpenMouth(target, candies, 200f));
        }

        [Fact]
        public void ShouldOpenMouth_FalseWhenCandyOutOfRange()
        {
            Vector target = new(100, 100);
            List<CandyView> candies = [Candy(400, 100, false)]; // 300px away
            Assert.False(CandyDecisions.ShouldOpenMouth(target, candies, 200f));
        }

        [Theory]
        [InlineData(nameof(CandyRemovalReason.Eaten), true)]
        [InlineData(nameof(CandyRemovalReason.Hazard), false)]
        [InlineData(nameof(CandyRemovalReason.Spider), false)]
        [InlineData(nameof(CandyRemovalReason.OffScreen), false)]
        public void AllEaten_AcceptsOnlyEatenRemoval(string reasonName, bool expected)
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
        public void AllEaten_RejectsIntactNonRemovedLifecycleStates(string presenceName)
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
        public void AnyFailedRemoval_AcceptsEveryLossReason(string reasonName)
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
        public void SplitHalfFailure_RequestsLossAndNeverWin()
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
        public void EatenRemoval_DoesNotRequestLoss()
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
        public void NoCandiesAtAll_NeverWins()
        {
            Assert.False(CandyDecisions.AllEaten([]));
        }

        /// <summary>
        /// Only the eatable candies gate the win, so a level holding nothing but light bulbs is
        /// won immediately - exactly what the helper this replaced did with its candy-count guard.
        /// </summary>
        [Fact]
        public void InedibleBodiesAlone_SatisfyTheWinVacuously()
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
