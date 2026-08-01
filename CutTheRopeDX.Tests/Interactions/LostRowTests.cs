using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Lost" row: a candy destroyed on spikes takes its attachments down with
    /// it, partly through the break itself and partly through the GameLost that follows. The row
    /// names a second trigger - leaving the screen - which the engine handles on its own, much
    /// thinner path; the LostOffScreen tests below pin where the two diverge.
    /// </summary>
    public sealed class LostRowTests
    {
        /// <summary>World Y far past the kill line, for the tests that push a candy out of play.</summary>
        private const float BelowTheWorld = 4000f;

        [Fact]
        public void Lost_ReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            Act.BreakOnSpikes(scene, candy);

            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void Lost_DetachesTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Null(candy.capturingHand);
            Assert.NotEqual(MechanicalHand.STATE_HAND_CANDY, hand.state);
        }

        [Fact]
        public void Lost_ExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.False(candy.HasActiveRocket);
            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, rocket.state);
        }

        [Fact]
        public void Lost_PopsItsBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            _ = Act.CaptureInBubble(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Null(candy.bubble);
        }

        [Fact]
        public void Lost_DetachesItsSnail_ButKeepsTheSnailWeightOnTheRetiredPoint()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Equal(0, scene.SnailCount(candy));

            // Matrix cell says "detach (+weight)"; BreakCandyFromHazard only detaches. Same shape as
            // the eaten row - the weight stays on a point nothing reads again.
            Assert.Equal(1 + SnailWeight.PerSnailWeight, candy.point.weight);
        }

        [Fact]
        public void Lost_LeavesTheBrokenCandyRidingTheAntLane()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            // Matrix cell says "detach". Neither the break nor GameLost takes the candy off the ant
            // conveyor, so it stays bound to the segment it died on.
            Assert.NotNull(candy.antSegment);
        }

        [Fact]
        public void Lost_MakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.False(scene.MouseCarries(candy));
            Assert.False(candy.carriedByMouse);
        }

        [Fact]
        public void LostOffScreen_ExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.LoseOffScreen(scene, candy);

            Assert.False(candy.HasActiveRocket);
            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, rocket.state);
        }

        [Fact]
        public void LostOffScreen_LeavesTheRopeOnTheLostCandy()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));

            Act.LoseOffScreen(scene, candy);

            // The matrix folds "spikes/off-screen" into one row, but the two paths differ: leaving
            // the screen only exhausts the rocket, and the GameLost that follows releases no ropes.
            // A candy broken on spikes has its ropes cut; one that falls out of the world does not.
            Assert.Equal(1, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void LostOffScreen_LeavesItsSnailAttached()
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
            Interaction.PlaceCandyAt(candy, new Vector(candy.point.pos.X, BelowTheWorld));
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
            Interaction.PlaceCandyAt(candy, new Vector(candy.point.pos.X, BelowTheWorld));
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
