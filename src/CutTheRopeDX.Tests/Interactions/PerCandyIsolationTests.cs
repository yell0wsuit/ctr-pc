using System;

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
        public void EatingOneCandyLeavesTheOtherCandysRopeAttached()
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
        public void EatingOneCandyLeavesTheOtherCandysSnailRiding()
        {
            (GameScene scene, CandyContext eaten, CandyContext kept) = TwoCandies(s => s.Snail(260, 200));
            _ = Act.RideSnail(scene, kept);

            Act.Eat(scene, eaten);

            Assert.Equal(1, scene.SnailCount(kept));
        }

        [Fact]
        public void RetiringAnotherCandyDoesNotReleaseThePrimarysParkedGhostBubble()
        {
            (GameScene scene, CandyContext primary, CandyContext removed) = TwoCandies(s => s
                .Bubble(260, 200)
                .Bubble(20, 40));
            _ = Act.CaptureInBubble(scene, removed, bubbleIndex: 0);
            Bubble parked = scene.Bubbles()[1];
            scene.ParkSecondGhostBubble(primary.WholeBody, parked);

            scene.BreakCandyBody(removed.WholeBody);

            Assert.Same(parked, scene.PendingSecondGhostBubble());
        }

        [Fact]
        public void EatingOneOfTwoCandiesImmediatelyDetachesItsAntCarrier()
        {
            AssertPreWinEatenCleanup(
                s => s.Ants(20, 200, path: "80,0"),
                Act.CarryByAnts);
        }

        [Fact]
        public void EatingOneOfTwoCandiesImmediatelyDetachesItsHand()
        {
            AssertPreWinEatenCleanup(
                s => s.Hand(60, 120, segmentLength: 20, segmentAngle: 90f),
                (scene, candy) => _ = Act.GrabWithHand(scene, candy));
        }

        [Fact]
        public void EatingOneOfTwoCandiesImmediatelyDropsItsMouseCarrier()
        {
            AssertPreWinEatenCleanup(
                s => s.Mouse(60, 200),
                (scene, candy) => _ = Act.CarryByMouse(scene, candy));
        }

        [Fact]
        public void LanternCapturingOneCandyLeavesTheOtherCandysBubble()
        {
            (GameScene scene, CandyContext captured, CandyContext kept) = TwoCandies(s => s
                .Lantern(20, 40)
                .Bubble(260, 200));
            Bubble bubble = Act.CaptureInBubble(scene, kept);

            Act.CaptureInLantern(scene, captured);

            Assert.True(captured.Lifecycle.Attachments.InLantern);
            Assert.Same(bubble, kept.WholeBody.Bubble);
        }

        [Fact]
        public void AHandGrabbingOneCandyLeavesTheOtherCandysSnailAndWeight()
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
        public void AMouseTakingOneCandyLeavesTheOtherCandyInItsHand()
        {
            (GameScene scene, CandyContext stolen, CandyContext kept) = TwoCandies(s => s
                .Mouse(20, 40)
                .Hand(260, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, kept);

            _ = Act.CarryByMouse(scene, stolen);

            Assert.True(scene.MouseCarries(stolen));
            Assert.Same(hand, kept.Lifecycle.Attachments.Hand);
        }

        [Fact]
        public void ATubeSwallowingOneCandyLeavesTheOtherCandysRocket()
        {
            (GameScene scene, CandyContext swallowed, CandyContext kept) = TwoCandies(s => s
                .BambooTube(20, 40, TubeMouth.CatchesFalling)
                .Rocket(260, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, kept);

            Act.EnterBambooTube(scene, swallowed, TubeMouth.CatchesFalling);

            Assert.NotNull(swallowed.Lifecycle.Transport?.BambooTube);
            Assert.Same(rocket, kept.Lifecycle.Attachments.Rocket);
            Assert.True(rocket.visible);
        }

        private static (GameScene Scene, CandyContext First, CandyContext Second) TwoCandies(
            Func<Scenario, Scenario> extras)
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

        private static void AssertPreWinEatenCleanup(
            Func<Scenario, Scenario> extras,
            Action<GameScene, CandyContext> attach)
        {
            GameScene scene = extras(
                    Scenario.New()
                        .Candy(60, 200, number: "1")
                        .Candy(260, 200, number: "2")
                        .OmNom(20, 460)
                        .OmNom(300, 460))
                .Build();
            CandyContext eaten = scene.Candies()[0];
            CandyContext remaining = scene.Candies()[1];
            Interaction.Hover(eaten);
            Interaction.Hover(remaining);
            attach(scene, eaten);

            Act.Eat(scene, eaten);

            Assert.Equal(0, scene.Outcomes().WonCount);
            Assert.Equal(CandyPresence.Present, remaining.Lifecycle.Presence);
            scene.AssertNoLiveAttachments(eaten);
        }
    }
}
