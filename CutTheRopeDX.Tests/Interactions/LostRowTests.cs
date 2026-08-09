using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Lost" row: a candy destroyed on spikes takes its attachments down with
    /// it. Both spikes and leaving the screen must retire every attachment owned by that candy.
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

            Assert.Null(candy.Lifecycle.Attachments.Hand);
            Assert.NotEqual(MechanicalHandState.HoldingCandy, hand.State);
        }

        [Fact]
        public void LostExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.False(candy.Lifecycle.Attachments.HasActiveRocket);
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
        public void LostDetachesTheBrokenCandyFromTheAntLane()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
            Assert.Null(candy.Lifecycle.Attachments.LastAntSegment);
            Assert.False(candy.Lifecycle.Attachments.AntWaitingForExit);
            Assert.Equal(0f, candy.Lifecycle.Attachments.AntCooldown);
        }

        [Fact]
        public void LostMakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.False(scene.MouseCarries(candy));
        }

        [Fact]
        public void LevelLossClearsMouseOwnershipFromASurvivingCandy()
        {
            GameScene scene = Scenario.New()
                .Candy(240, 200)
                .Mouse(240, 200)
                .OmNom(20, 460)
                .Build();
            CandyContext surviving = scene.Candy();
            Interaction.Hover(surviving);
            _ = Act.CarryByMouse(scene, surviving);

            scene.GameLost();

            Assert.False(scene.MouseCarries(surviving));
            Assert.False(surviving.Lifecycle.IsGravitySuppressed);
            Assert.False(surviving.WholeBody.Point.disableGravity);
        }

        [Fact]
        public void RepeatedHazardOverlapRetiresAndPresentsTheCandyOnlyOnce()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            // Two hazards may discover the same body before the next active-body enumeration. Drive
            // the real hazard entry point twice to require its lifecycle transition to be the gate
            // for cleanup, break presentation, and delayed loss scheduling.
            scene.BreakCandyBody(candy.WholeBody);
            scene.BreakCandyBody(candy.WholeBody);

            Assert.Equal(CandyRemovalReason.Hazard, candy.Lifecycle.RemovalReason);
            Assert.Equal(1, scene.CandyBreakEffectCount());
            scene.AssertNoLiveAttachments(candy);
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().LostCount > 0),
                "the broken candy never lost the level");
            HeadlessGame.StepFrames(scene, 60);
            Assert.Equal(1, scene.Outcomes().LostCount);
        }

        [Fact]
        public void HazardRetirementClearsAntCooldownStateAfterTheCarrierLetsGo()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(150, 200, path: "20,0", moveSpeed: 600f));
            Act.CarryByAnts(scene, candy);
            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Attachments.AntSegment == null && candy.Lifecycle.Attachments.LastAntSegment != null),
                "the candy never entered the ant reattachment-cooldown state");

            scene.BreakCandyBody(candy.WholeBody);

            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
            Assert.Null(candy.Lifecycle.Attachments.LastAntSegment);
            Assert.False(candy.Lifecycle.Attachments.AntWaitingForExit);
            Assert.Equal(0f, candy.Lifecycle.Attachments.AntCooldown);
        }

        [Fact]
        public void HazardRetirementClearsMouseContextOwnershipImmediately()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            scene.BreakCandyBody(candy.WholeBody);

            Assert.False(scene.MouseCarries(candy));
        }

        [Fact]
        public void HazardRetirementCancelsPendingLanternCapture()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Lantern(20, 40));
            Lantern lantern = Lantern.GetAllLanterns()[0];
            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () => Act.MoveTo(lantern, candy.WholeBody.Point.pos),
                    () => candy.Lifecycle.Attachments.InLantern),
                "the lantern never began capturing the candy");

            scene.BreakCandyBody(candy.WholeBody);

            Assert.False(candy.Lifecycle.Attachments.InLantern);
            Assert.False(candy.WholeBody.Point.disableGravity);
            HeadlessGame.StepFrames(scene, 10);
            Assert.False(candy.Lifecycle.Attachments.InLantern);
            Assert.False(candy.WholeBody.Point.disableGravity);
        }

        [Fact]
        public void HazardRetirementCancelsCompletedLanternCapture()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Lantern(20, 40));
            Act.CaptureInLantern(scene, candy);

            scene.BreakCandyBody(candy.WholeBody);

            Assert.False(candy.Lifecycle.Attachments.InLantern);
            Assert.False(candy.WholeBody.Point.disableGravity);
            HeadlessGame.StepFrames(scene, 10);
            Assert.False(candy.WholeBody.Point.disableGravity);
        }

        [Fact]
        public void LostOffScreenExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.LoseOffScreen(scene, candy);

            Assert.False(candy.Lifecycle.Attachments.HasActiveRocket);
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
        public void LostOffScreenClearsItsBubbleOwnership()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            _ = Act.CaptureInBubble(scene, candy);

            Act.LoseOffScreen(scene, candy);

            Assert.Null(candy.WholeBody.Bubble);
            Assert.False(candy.WholeBody.Point.disableGravity);
        }

        [Fact]
        public void LostOffScreenDetachesItsSnail()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            Act.LoseOffScreen(scene, candy);

            Assert.Equal(0, scene.SnailCount(candy));
        }

        [Fact]
        public void LosingOneCandyOffScreenDoesNotAlterAnotherCandysBubble()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 200, number: "1")
                .Candy(260, 200, number: "2")
                .Snail(60, 200)
                .Bubble(260, 200)
                .OmNom(20, 460)
                .Build();
            CandyContext lost = scene.Candies()[0];
            CandyContext kept = scene.Candies()[1];
            Interaction.Hover(lost);
            Interaction.Hover(kept);
            _ = Act.RideSnail(scene, lost);
            Bubble bubble = Act.CaptureInBubble(scene, kept);

            Act.LoseOffScreen(scene, lost);

            Assert.Equal(0, scene.SnailCount(lost));
            Assert.Same(bubble, kept.WholeBody.Bubble);
            Assert.Equal(CandyPresence.Present, kept.Lifecycle.Presence);
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
            Assert.Same(hand, candy.Lifecycle.Attachments.Hand);
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
            Assert.True(scene.MouseCarries(candy));
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
