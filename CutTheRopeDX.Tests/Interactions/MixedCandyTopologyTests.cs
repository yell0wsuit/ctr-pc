using System.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// A split candy and an ordinary whole candy in one level - three physical bodies owned by two
    /// logical candies. The shipped maps never author this, because the old code could not express
    /// it: split state lived in scene singletons, so a second candy would have shared or overwritten
    /// it. These drive real scenarios to prove the lifecycle carries the mixed topology on its own,
    /// and that win and loss still answer once per logical candy however many bodies it has.
    /// </summary>
    public sealed class MixedCandyTopologyTests
    {
        /// <summary>World Y well past the kill line of a scenario-sized map.</summary>
        private const float BelowTheWorld = 4000f;

        /// <summary>World gap the halves are held at: well inside their hitboxes, but not zero.</summary>
        private const float TouchingGap = 8f;

        [Fact]
        public void SplitPlusWholeExposesThreePhysicalBodiesAndTwoLogicalCandies()
        {
            GameScene scene = Scenario.New()
                .SplitCandy(100, 200, 140, 200)
                .Candy(260, 200, number: "2")
                .OmNom(20, 460)
                .Build();

            Assert.Equal(2, scene.Candies().Count);
            Assert.Equal(3, scene.Candies().SelectMany(c => c.Lifecycle.ActiveBodies).Count());
        }

        /// <summary>
        /// The split candy has to stay the primary. A whole <c>&lt;candy&gt;</c> element that claimed
        /// <c>candies[0]</c> would hand the halves to a context that is already somebody else's.
        /// </summary>
        [Fact]
        public void SplitPlusWholeKeepsTheSplitCandyPrimaryAndBuildsTheWholeOneBesideIt()
        {
            (GameScene scene, CandyContext split, CandyContext whole) = MixedScene();

            Assert.Same(scene.Candies()[0], split);
            Assert.Equal(CandyPresence.Split, split.Lifecycle.Presence);
            Assert.Null(split.candyNumber);
            Assert.Equal(CandyPresence.Present, whole.Lifecycle.Presence);
            Assert.Equal("2", whole.candyNumber);
        }

        [Fact]
        public void SplitPlusWholeGivesTheSceneTheTwoHalvesAndTheWholeBodyInOrder()
        {
            (GameScene scene, CandyContext split, CandyContext whole) = MixedScene();

            Assert.Equal(
                [split.Lifecycle.Split.Left.Body, split.Lifecycle.Split.Right.Body, whole.WholeBody],
                scene.ActiveBodies());
            Assert.DoesNotContain(split.WholeBody, scene.ActiveBodies());
        }

        /// <summary>
        /// Physical systems act on every body; the carrier and outcome systems only ever see a whole
        /// candy, so in a mixed level they see the extra candy's body and neither half.
        /// </summary>
        [Theory]
        [InlineData((int)CandyInteraction.Physics)]
        [InlineData((int)CandyInteraction.Hazard)]
        [InlineData((int)CandyInteraction.Bouncer)]
        [InlineData((int)CandyInteraction.Bubble)]
        [InlineData((int)CandyInteraction.OffScreen)]
        public void SplitPlusWholeOffersEveryBodyToThePhysicalSystems(int interactionValue)
        {
            (GameScene scene, CandyContext split, CandyContext whole) = MixedScene();

            Assert.Equal(
                [split.Lifecycle.Split.Left.Body, split.Lifecycle.Split.Right.Body, whole.WholeBody],
                scene.ActiveBodies((CandyInteraction)interactionValue));
        }

        [Theory]
        [InlineData((int)CandyInteraction.CandyCollision)]
        [InlineData((int)CandyInteraction.Mouse)]
        [InlineData((int)CandyInteraction.Lantern)]
        [InlineData((int)CandyInteraction.Rocket)]
        [InlineData((int)CandyInteraction.Ants)]
        [InlineData((int)CandyInteraction.Transport)]
        [InlineData((int)CandyInteraction.Hand)]
        [InlineData((int)CandyInteraction.Snail)]
        [InlineData((int)CandyInteraction.Eat)]
        public void SplitPlusWholeOffersOnlyTheWholeCandyToTheCarrierSystems(int interactionValue)
        {
            (GameScene scene, CandyContext _, CandyContext whole) = MixedScene();

            Assert.Equal([whole.WholeBody], scene.ActiveBodies((CandyInteraction)interactionValue));
        }

        /// <summary>
        /// Once the halves merge, the split candy is an ordinary whole candy again, so it rejoins the
        /// carrier systems - candy-to-candy collision included - beside the extra candy.
        /// </summary>
        [Fact]
        public void MergingTheSplitCandyLetsItCollideWithTheOtherWholeCandy()
        {
            (GameScene scene, CandyContext split, CandyContext whole) = MixedScene();
            Assert.Equal([whole.WholeBody], scene.ActiveBodies(CandyInteraction.CandyCollision));

            MergeHalves(scene, split);

            Assert.Equal(
                [split.WholeBody, whole.WholeBody],
                scene.ActiveBodies(CandyInteraction.CandyCollision));
        }

        [Fact]
        public void EatingWholeCandyDoesNotWinWhileSplitCandyRemains()
        {
            (GameScene scene, CandyContext split, CandyContext whole) = MixedScene();

            Act.Eat(scene, whole);
            HeadlessGame.StepFrames(scene, 30);

            Assert.Equal(CandyRemovalReason.Eaten, whole.Lifecycle.RemovalReason);
            Assert.Equal(CandyPresence.Split, split.Lifecycle.Presence);
            Assert.Equal(0, scene.Outcomes().WonCount);
            Assert.Equal(0, scene.Outcomes().LostCount);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LosingEitherSplitHalfLosesEvenWhenWholeCandyRemains(bool loseLeftHalf)
        {
            (GameScene scene, CandyContext split, CandyContext whole) = MixedScene();
            SplitCandyState halves = split.Lifecycle.Split;
            CandyHalf lost = loseLeftHalf ? halves.Left : halves.Right;
            CandyHalf kept = loseLeftHalf ? halves.Right : halves.Left;

            DropOffScreen(scene, lost.Body);

            Assert.Equal(CandyRemovalReason.OffScreen, lost.RemovalReason);
            Assert.Null(kept.RemovalReason);
            Assert.Equal([kept.Body], split.Lifecycle.ActiveBodies);
            Assert.Equal(CandyPresence.Present, whole.Lifecycle.Presence);
            Assert.Equal(1, scene.Outcomes().LostCount);
            Assert.Equal(0, scene.Outcomes().WonCount);
        }

        [Fact]
        public void LosingBothSplitHalvesRecordsOneLossAndZeroWins()
        {
            (GameScene scene, CandyContext split, CandyContext whole) = MixedScene();
            SplitCandyState halves = split.Lifecycle.Split;

            Interaction.PlaceBodyAt(halves.Left.Body, BelowTheWorldFrom(halves.Left.Body));
            Interaction.PlaceBodyAt(halves.Right.Body, BelowTheWorldFrom(halves.Right.Body));
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().LostCount > 0),
                "the halves that left the screen never lost the level");
            HeadlessGame.StepFrames(scene, 30);

            Assert.Empty(split.Lifecycle.ActiveBodies);
            Assert.True(split.Lifecycle.HasFailedSplitHalf);
            Assert.Equal(CandyPresence.Present, whole.Lifecycle.Presence);
            Assert.Equal(1, scene.Outcomes().LostCount);
            Assert.Equal(0, scene.Outcomes().WonCount);
        }

        /// <summary>
        /// The win gate is per logical candy, not per body: merging the halves and feeding the
        /// re-formed candy is one candy eaten out of two, and the level is only won once the other
        /// whole candy has been eaten too.
        /// </summary>
        [Fact]
        public void MergeThenEatSplitCandyWinsOnlyAfterOtherWholeCandyWasEaten()
        {
            (GameScene scene, CandyContext split, CandyContext whole) = TwoOmNomScene();
            MergeHalves(scene, split);

            Act.Eat(scene, split);
            HeadlessGame.StepFrames(scene, 30);

            Assert.Equal(CandyRemovalReason.Eaten, split.Lifecycle.RemovalReason);
            Assert.Equal(0, scene.Outcomes().WonCount);

            Act.Eat(scene, whole, targetIndex: 1);
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().WonCount > 0),
                "the level was never won after both candies were eaten");

            Assert.Equal(CandyRemovalReason.Eaten, whole.Lifecycle.RemovalReason);
            Assert.Equal(1, scene.Outcomes().WonCount);
            Assert.Equal(0, scene.Outcomes().LostCount);
        }

        /// <summary>
        /// A split candy plus one whole candy, every body held against gravity so a test acts where
        /// it placed things instead of chasing a fall.
        /// </summary>
        private static (GameScene Scene, CandyContext Split, CandyContext Whole) MixedScene()
        {
            return Hovered(Scenario.New()
                .SplitCandy(100, 200, 140, 200)
                .Candy(260, 200, number: "2")
                .OmNom(20, 460)
                .Build());
        }

        /// <summary>
        /// The same level with a second Om Nom, for the tests that feed both candies: an Om Nom
        /// falls asleep on its one candy and never opens its mouth again.
        /// </summary>
        private static (GameScene Scene, CandyContext Split, CandyContext Whole) TwoOmNomScene()
        {
            return Hovered(Scenario.New()
                .SplitCandy(100, 200, 140, 200)
                .Candy(260, 200, number: "2")
                .OmNom(20, 460)
                .OmNom(300, 460)
                .Build());
        }

        private static (GameScene Scene, CandyContext Split, CandyContext Whole) Hovered(GameScene scene)
        {
            foreach (CandyBody body in scene.ActiveBodies())
            {
                Interaction.Hover(body);
            }

            return (scene, scene.Candies()[0], scene.Candies()[1]);
        }

        /// <summary>
        /// Overlaps the two halves until the scene sees them touch, then lets the merge run. The
        /// halves are held a sliver apart rather than on the same point: the scene begins the merge
        /// with the gap it measures and then closes that gap, so halves pinned exactly together have
        /// nothing left to close and stay split forever.
        /// </summary>
        private static void MergeHalves(GameScene scene, CandyContext split)
        {
            SplitCandyState halves = split.Lifecycle.Split;
            Vector meetingPoint = halves.Left.Body.Point.pos;
            Vector aSliverAway = new(meetingPoint.X + TouchingGap, meetingPoint.Y);
            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () =>
                    {
                        if (halves.Phase != SplitPhase.Separate)
                        {
                            return;
                        }

                        Interaction.PlaceBodyAt(halves.Left.Body, meetingPoint);
                        Interaction.PlaceBodyAt(halves.Right.Body, aSliverAway);
                    },
                    () => halves.Phase == SplitPhase.Merging),
                "the two halves never began merging");
            Assert.True(
                Interaction.StepUntil(scene, () => split.Lifecycle.Presence == CandyPresence.Present),
                "the two halves never merged");
            Interaction.Hover(split.WholeBody);
        }

        /// <summary>Pushes one body past the kill line and waits for the scene to lose the level.</summary>
        private static void DropOffScreen(GameScene scene, CandyBody body)
        {
            Interaction.PlaceBodyAt(body, BelowTheWorldFrom(body));

            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().LostCount > 0),
                "the body that left the screen never lost the level");
            HeadlessGame.StepFrames(scene, 30);
        }

        private static Vector BelowTheWorldFrom(CandyBody body)
        {
            return new Vector(body.Point.pos.X, BelowTheWorld);
        }
    }
}
