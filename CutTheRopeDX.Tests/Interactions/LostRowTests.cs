using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Lost" row: a candy destroyed on spikes takes its attachments down with
    /// it, partly through the break itself and partly through the GameLost that follows. The row
    /// names a second trigger - leaving the screen - which runs a thinner path of its own: it cuts
    /// the ropes and exhausts the rocket like the break does, but leaves the snail riding, exactly
    /// as iOS does (breakCandy: detaches snails and hands; the off-screen block does neither).
    /// </summary>
    public sealed class LostRowTests
    {
        /// <summary>World Y far past the kill line, for the tests that push a candy out of play.</summary>
        private const float BelowTheWorld = 4000f;

        [Fact]
        public void LostReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            Act.BreakOnSpikes(scene, candy);

            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void LostDetachesTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Null(candy.capturingHand);
            Assert.NotEqual(MechanicalHand.STATE_HAND_CANDY, hand.state);
        }

        [Fact]
        public void LostExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.False(candy.HasActiveRocket);
            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, rocket.state);
        }

        [Fact]
        public void LostPopsItsBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            _ = Act.CaptureInBubble(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Null(candy.WholeBody.Bubble);
        }

        [Fact]
        public void LostDetachesItsSnailButKeepsTheSnailWeightOnTheRetiredPoint()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Equal(0, scene.SnailCount(candy));

            Assert.Equal(1 + SnailWeight.PerSnailWeight, candy.WholeBody.Point.weight);
        }

        [Fact]
        public void LostLeavesTheBrokenCandyRidingTheAntLane()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.NotNull(candy.antSegment);
        }

        [Fact]
        public void LostMakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.False(scene.MouseCarries(candy));
            Assert.False(candy.carriedByMouse);
        }

        [Fact]
        public void LostOffScreenExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.LoseOffScreen(scene, candy);

            Assert.False(candy.HasActiveRocket);
            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, rocket.state);
        }

        [Fact]
        public void LostOffScreenReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));

            Act.LoseOffScreen(scene, candy);

            // Both loss triggers cut the ropes, and both do it themselves - iOS releases them inside
            // the off-screen block, not in gameLost, which touches no attachments at all.
            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void LostOffScreenLeavesItsSnailAttached()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            Act.LoseOffScreen(scene, candy);

            // Same split as the rope: the off-screen path never calls DetachSnailsForPoint, and
            // GameLost only tears down hands and mice.
            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void AHandHeldCandyCannotBeLostOffScreen()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Interaction.Drop(candy);
            Interaction.PlaceCandyAt(candy, new Vector(candy.WholeBody.Point.pos.X, BelowTheWorld));
            HeadlessGame.StepFrames(scene, 60);

            // The claw pins the candy back every frame, ahead of the off-screen check, so a held
            // candy never reaches the kill line - the hand has to let go first.
            Assert.Equal(0, scene.Outcomes().LostCount);
            Assert.Same(hand, candy.capturingHand);
        }

        [Fact]
        public void AMouseCarriedCandyCannotBeLostOffScreen()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Interaction.Drop(candy);
            Interaction.PlaceCandyAt(candy, new Vector(candy.WholeBody.Point.pos.X, BelowTheWorld));
            HeadlessGame.StepFrames(scene, 60);

            Assert.Equal(0, scene.Outcomes().LostCount);
            Assert.True(candy.carriedByMouse);
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment)
        {
            // Om Nom sits in the far corner so nothing is eaten; the spikes start off in another
            // corner and Act.BreakOnSpikes brings them to the candy.
            GameScene scene = attachment(Scenario.New().Candy(160, 200).OmNom(20, 460).Spikes(20, 40)).Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
