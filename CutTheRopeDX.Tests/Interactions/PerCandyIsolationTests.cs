using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// The matrix's standing claim above every row: each cell is per-candy, so candy B's
    /// attachments survive candy A's event. These are the cross-checks - one per teardown path
    /// that used to be written against the singleton candy.
    /// </summary>
    public sealed class PerCandyIsolationTests
    {
        [Fact]
        public void EatingOneCandy_LeavesTheOtherCandysRopeAttached()
        {
            (GameScene scene, CandyContext eaten, CandyContext kept) = TwoCandies(s => s
                .Rope(60, 120, length: 40, candyNumber: "1")
                .Rope(260, 120, length: 40, candyNumber: "2"));
            Assert.Equal(1, scene.AttachedRopeCount(kept));

            Act.Eat(scene, eaten);

            Assert.Equal(0, scene.AttachedRopeCount(eaten));
            Assert.Equal(1, scene.AttachedRopeCount(kept));
        }

        [Fact]
        public void EatingOneCandy_LeavesTheOtherCandysSnailRiding()
        {
            (GameScene scene, CandyContext eaten, CandyContext kept) = TwoCandies(s => s.Snail(260, 200));
            _ = Act.RideSnail(scene, kept);

            Act.Eat(scene, eaten);

            Assert.Equal(1, scene.SnailCount(kept));
        }

        [Fact]
        public void LanternCapturingOneCandy_LeavesTheOtherCandysBubble()
        {
            (GameScene scene, CandyContext captured, CandyContext kept) = TwoCandies(s => s
                .Lantern(20, 40)
                .Bubble(260, 200));
            Bubble bubble = Act.CaptureInBubble(scene, kept);

            Act.CaptureInLantern(scene, captured);

            Assert.True(captured.inLantern);
            Assert.Same(bubble, kept.WholeBody.Bubble);
        }

        [Fact]
        public void AHandGrabbingOneCandy_LeavesTheOtherCandysSnailAndWeight()
        {
            (GameScene scene, CandyContext grabbed, CandyContext kept) = TwoCandies(s => s
                .Hand(20, 40, segmentLength: 20, segmentAngle: 90f)
                .Snail(260, 200));
            _ = Act.RideSnail(scene, kept);

            _ = Act.GrabWithHand(scene, grabbed);

            Assert.Equal(1, scene.SnailCount(kept));
            Assert.Equal(1 + SnailWeight.PerSnailWeight, kept.WholeBody.Point.weight);
        }

        [Fact]
        public void AMouseTakingOneCandy_LeavesTheOtherCandyInItsHand()
        {
            (GameScene scene, CandyContext stolen, CandyContext kept) = TwoCandies(s => s
                .Mouse(20, 40)
                .Hand(260, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, kept);

            _ = Act.CarryByMouse(scene, stolen);

            Assert.True(stolen.carriedByMouse);
            Assert.Same(hand, kept.capturingHand);
        }

        [Fact]
        public void ATubeSwallowingOneCandy_LeavesTheOtherCandysRocket()
        {
            (GameScene scene, CandyContext swallowed, CandyContext kept) = TwoCandies(s => s
                .BambooTube(20, 40, TubeMouth.CatchesFalling)
                .Rocket(260, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, kept);

            Act.EnterBambooTube(scene, swallowed, TubeMouth.CatchesFalling);

            Assert.NotNull(swallowed.Lifecycle.Transport?.BambooTube);
            Assert.Same(rocket, kept.activeRocket);
            Assert.True(rocket.visible);
        }

        private static (GameScene Scene, CandyContext First, CandyContext Second) TwoCandies(
            System.Func<Scenario, Scenario> extras)
        {
            // Two independently keyed candies, far enough apart that an event aimed at one cannot
            // reach the other by proximity.
            GameScene scene = extras(
                Scenario.New()
                    .Candy(60, 200, number: "1")
                    .Candy(260, 200, number: "2")
                    .OmNom(20, 460))
                .Build();
            CandyContext first = scene.Candies()[0];
            CandyContext second = scene.Candies()[1];
            Interaction.Hover(first);
            Interaction.Hover(second);
            return (scene, first, second);
        }
    }
}
