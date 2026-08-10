using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// The primary candy used to answer "am I gone?" from two unsynchronised booleans - the scene's
    /// own singleton and its context flag - so hazards, spiders, transport, and splits each reached
    /// only one of them and different systems disagreed about the same candy. These drive real
    /// scenarios and prove one lifecycle now answers for every one of them.
    /// </summary>
    public sealed class CandyLifecycleOutcomeTests
    {
        [Fact]
        public void SpikesBreakingThePrimaryRecordHazardAndLoseOnce()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(30, 440)
                .Spikes(160, 260)
                .Build();
            CandyContext candy = scene.Candy();

            Act.BreakOnSpikes(scene, candy);

            Assert.Equal(CandyPresence.Removed, candy.Lifecycle.Presence);
            Assert.Equal(CandyRemovalReason.Hazard, candy.Lifecycle.RemovalReason);
            Assert.True(candy.Lifecycle.HasFailedRemoval);
            Assert.False(candy.Lifecycle.WasEaten);
            Assert.Equal(1, scene.Outcomes().LostCount);
            Assert.Equal(0, scene.Outcomes().WonCount);
        }

        [Fact]
        public void ThePrimaryLeavingTheScreenRecordsOffScreenAndLosesOnce()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(30, 200)
                .Build();
            CandyContext candy = scene.Candy();

            Act.LoseOffScreen(scene, candy);

            Assert.Equal(CandyPresence.Removed, candy.Lifecycle.Presence);
            Assert.Equal(CandyRemovalReason.OffScreen, candy.Lifecycle.RemovalReason);
            Assert.Equal(1, scene.Outcomes().LostCount);
            Assert.Equal(0, scene.Outcomes().WonCount);
        }

        [Fact]
        public void OmNomEatingThePrimaryRecordsEatenAndWins()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(160, 300)
                .Build();
            CandyContext candy = scene.Candy();

            Act.Eat(scene, candy);
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().WonCount > 0),
                "the eaten candy never won the level");

            Assert.Equal(CandyPresence.Removed, candy.Lifecycle.Presence);
            Assert.Equal(CandyRemovalReason.Eaten, candy.Lifecycle.RemovalReason);
            Assert.True(candy.Lifecycle.WasEaten);
            Assert.False(candy.Lifecycle.HasFailedRemoval);
            Assert.Equal(1, scene.Outcomes().WonCount);
            Assert.Equal(0, scene.Outcomes().LostCount);
        }

        /// <summary>
        /// A hazard break used to leave <c>candies[0]</c> looking present to every per-candy system,
        /// because only the scene singleton was raised. The dead body must leave play for all of them.
        /// </summary>
        [Fact]
        public void ABrokenPrimaryLeavesPlayForEveryPerCandySystem()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(30, 440)
                .Spikes(160, 260)
                .Build();
            CandyContext candy = scene.Candy();

            Act.BreakOnSpikes(scene, candy);

            Assert.True(candy.HasNoWholeBodyInPlay);
            Assert.False(candy.IsHandGrabbable);
            Assert.False(candy.IsAntAttachable);
            Assert.Empty(candy.Lifecycle.ActiveBodies);
        }

        /// <summary>
        /// A spider stealing the primary used to raise the singleton alone, so the stolen candy still
        /// counted as a live body for every per-candy system.
        /// </summary>
        /// <remarks>
        /// The capture is invoked directly rather than crawled to. A spider only advances along
        /// <c>Bungee.drawPts</c>, and those are filled by <c>DrawBungee</c> alone, so a headless run
        /// leaves the rope untessellated and the spider parked at the hook forever. Task 9 owns that
        /// draw-path dependency; this test is about which state the capture writes.
        /// </remarks>
        [Fact]
        public void ASpiderStealingThePrimaryRecordsSpiderRemoval()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(160, 120, length: 100, spider: true)
                .OmNom(30, 440)
                .Build();
            CandyContext candy = scene.Candy();
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            scene.SpiderWon(scene.Grabs()[0]);

            Assert.Equal(CandyPresence.Removed, candy.Lifecycle.Presence);
            Assert.Equal(CandyRemovalReason.Spider, candy.Lifecycle.RemovalReason);
            Assert.True(candy.HasNoWholeBodyInPlay);
            Assert.Empty(candy.Lifecycle.ActiveBodies);
            Assert.True(candy.Lifecycle.HasFailedRemoval);
        }

        [Fact]
        public void ASpiderStealingCandyImmediatelyFreesEveryRopeFromItsPoint()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(120, 120, length: 100, spider: true)
                .Grab(200, 120, length: 100)
                .OmNom(30, 440)
                .Build();
            ConstraintedPoint candyPoint = scene.Candy().WholeBody.Point;
            Bungee[] ropes = [scene.Grabs()[0].RopeOf(), scene.Grabs()[1].RopeOf()];
            ConstraintedPoint[] ropeEnds = [ropes[0].parts[^2], ropes[1].parts[^2]];
            Assert.All(ropeEnds, ropeEnd => Assert.True(candyPoint.HasConstraintTo(ropeEnd)));

            scene.SpiderWon(scene.Grabs()[0]);

            Assert.All(ropes, rope => Assert.NotEqual(-1, rope.cut));
            Assert.All(ropeEnds, ropeEnd => Assert.False(candyPoint.HasConstraintTo(ropeEnd)));
        }

        /// <summary>
        /// A spider whose rope no longer ends on a live body has nothing to steal. The capture used to
        /// fall back to <c>candies[0]</c>, so a spider on an already-lost candy stole the primary one
        /// instead - a loss the player never caused.
        /// </summary>
        [Fact]
        public void ASpiderOnAnAlreadyLostCandyStealsNothing()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 200, number: "1")
                .Candy(260, 200, number: "2")
                .Grab(260, 120, length: 100, spider: true, candyNumber: "2")
                .OmNom(30, 440)
                .Build();
            CandyContext primary = scene.Candies()[0];
            CandyContext spidered = scene.Candies()[1];
            Interaction.Hover(primary);
            Assert.Equal(1, scene.AttachedRopeCount(spidered));

            Act.LoseOffScreen(scene, spidered);
            Assert.Equal(CandyRemovalReason.OffScreen, spidered.Lifecycle.RemovalReason);

            scene.SpiderWon(scene.Grabs()[0]);

            Assert.Equal(CandyPresence.Present, primary.Lifecycle.Presence);
            Assert.Null(primary.Lifecycle.RemovalReason);
            Assert.Equal(CandyRemovalReason.OffScreen, spidered.Lifecycle.RemovalReason);
        }

        /// <summary>
        /// Transport hid the whole body for extras but not for the primary, whose singleton stayed
        /// clear - so an invisible candy inside a tube still integrated and still met the scene.
        /// </summary>
        [Fact]
        public void ThePrimaryInsideABambooTubeHasNoWholeBodyInPlay()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(160, 440)
                .BambooTube(160, 220, TubeMouth.CatchesFalling)
                .Build();
            CandyContext candy = scene.Candy();

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.Equal(CandyPresence.Hidden, candy.Lifecycle.Presence);
            Assert.True(candy.HasNoWholeBodyInPlay);
            Assert.Empty(candy.Lifecycle.ActiveBodies);
            Assert.Null(candy.Lifecycle.RemovalReason);
        }

        /// <summary>Transport is not removal: leaving the tube restores the whole body.</summary>
        [Fact]
        public void ThePrimaryLeavingABambooTubeHasItsWholeBodyBack()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(160, 440)
                .BambooTube(160, 220, TubeMouth.CatchesFalling)
                .Build();
            CandyContext candy = scene.Candy();
            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.True(
                Interaction.StepUntil(scene, () => !candy.HasNoWholeBodyInPlay),
                "the candy never came out of the tube");
            Assert.Equal(CandyPresence.Present, candy.Lifecycle.Presence);
            Assert.Equal([candy.WholeBody], candy.Lifecycle.ActiveBodies);
        }
    }
}
