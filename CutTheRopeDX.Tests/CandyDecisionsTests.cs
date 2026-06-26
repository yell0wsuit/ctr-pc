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
        public void AllConsumed_TrueWhenEveryCandyEaten()
        {
            List<CandyView> candies = [Candy(0, 0, true), Candy(1, 1, true)];
            Assert.True(CandyDecisions.AllConsumed(candies));
        }

        [Fact]
        public void AllConsumed_FalseWhenAnyRemains()
        {
            List<CandyView> candies = [Candy(0, 0, true), Candy(1, 1, false)];
            Assert.False(CandyDecisions.AllConsumed(candies));
        }

        [Fact]
        public void AllConsumed_FalseWhenCandyIsTemporarilyInTransport()
        {
            List<CandyView> candies =
            [
                new CandyView(new Vector(0, 0), Consumed: true, InTransport: true),
                new CandyView(new Vector(1, 1), Consumed: true)
            ];

            Assert.False(CandyDecisions.AllConsumed(candies));
        }

        [Fact]
        public void AllConsumed_FalseWhenEmpty()
        {
            Assert.False(CandyDecisions.AllConsumed([]));
        }

        [Fact]
        public void AllConsumed_IgnoresObjectsThatCannotBeEaten()
        {
            List<CandyView> candies =
            [
                new CandyView(new Vector(0, 0), Consumed: true, InTransport: false, CandyCapabilities.Candy),
                new CandyView(new Vector(1, 1), Consumed: false, InTransport: false, CandyCapabilities.LightBulb)
            ];

            Assert.True(CandyDecisions.AllConsumed(candies));
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
            List<CandyView> candies = [];
            List<CandyView> splitCandies = [Candy(10, 10, false), Candy(20, 20, true)];

            Assert.True(CandyDecisions.AnyCandyBodyPresent(candies, splitCandies));
        }

        [Fact]
        public void AnyCandyBodyPresent_FalseWhenOnlyLightBulbRemains()
        {
            List<CandyView> candies =
            [
                new CandyView(new Vector(1, 1), Consumed: false, InTransport: false, CandyCapabilities.LightBulb)
            ];

            Assert.False(CandyDecisions.AnyCandyBodyPresent(candies, []));
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
    }
}
