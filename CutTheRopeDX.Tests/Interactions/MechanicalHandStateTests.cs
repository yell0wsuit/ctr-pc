using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Pins mechanical hand state transitions: which fields each release path writes, when a
    /// releasing hand settles, and how rotation state is scoped to a single hold.
    /// </summary>
    public sealed class MechanicalHandStateTests
    {
        [Fact]
        public void ARotatableSegmentActuallyRotates()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = scene.Hands()[0];
            _ = candy;

            Act.RotateSegment(scene, hand);

            Assert.Same(hand.SegmentAtIndex(0), hand.rotatingSegment);
            Assert.NotEqual(0f, hand.SegmentAtIndex(0).RotationDelta());
        }

        [Fact]
        public void TappingTheClawReleasesTheHeldCandy()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.TapClaw(scene, hand);

            Assert.Null(candy.Lifecycle.Attachments.Hand);
        }

        [Fact]
        public void ClawTapReleasesAndOwesNoDropSound()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.TapClaw(scene, hand);

            Assert.Equal(MechanicalHand.STATE_HAND_RELEASE, hand.state);
            Assert.False(hand.doRotateCandy);
            Assert.False(Act.SettleOwedDropSound(hand));
        }

        [Fact]
        public void DetachActiveHandsReleasesAndStillOwesTheDropSound()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            scene.DetachActiveHands();

            Assert.Equal(MechanicalHand.STATE_HAND_RELEASE, hand.state);
            Assert.False(hand.doRotateCandy);
            Assert.True(Act.SettleOwedDropSound(hand));
        }

        [Fact]
        public void DetachHandsForPointReleasesAndOwesNoDropSound()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            scene.DetachHandsForPoint(candy.WholeBody.Point);

            Assert.Equal(MechanicalHand.STATE_HAND_RELEASE, hand.state);
            Assert.False(hand.doRotateCandy);
            Assert.False(Act.SettleOwedDropSound(hand));
        }

        [Fact]
        public void CandyRemovalReleasesAndOwesNoDropSound()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            scene.BreakCandyBody(candy.WholeBody);

            Assert.Equal(MechanicalHand.STATE_HAND_RELEASE, hand.state);
            Assert.False(hand.doRotateCandy);
            Assert.False(Act.SettleOwedDropSound(hand));
        }

        [Fact]
        public void StealLeavesTheRivalHandsRotationFlagSet()
        {
            // PINS TODAY'S BEHAVIOR. Task 3 flips this assertion deliberately: both reference
            // decompiles omit the clear here, but the omission was only safe under one candy.
            (GameScene scene, CandyContext candy) = DuoRig(s => s.Rocket(160, 200, impulse: 0f));
            _ = Act.BindRocket(scene, candy);
            MechanicalHand first = Act.GrabWithHand(scene, candy, handIndex: 0);
            Assert.True(
                Interaction.StepUntil(scene, () => first.doRotateCandy, maxFrames: 10),
                "the first hand never started rotating its rocket candy");

            MechanicalHand second = Act.GrabWithHand(scene, candy, handIndex: 1);

            Assert.Same(second, candy.Lifecycle.Attachments.Hand);
            Assert.Equal(MechanicalHand.STATE_HAND_RELEASE, first.state);
            Assert.True(first.doRotateCandy);
        }

        [Fact]
        public void GrabbingAPlainCandyNeverStartsRotation()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            HeadlessGame.StepFrames(scene, 10);

            Assert.Equal(MechanicalHand.STATE_HAND_CANDY, hand.state);
            Assert.False(hand.doRotateCandy);
        }

        [Fact]
        public void GrabbingARocketCandyStartsRotation()
        {
            (GameScene scene, CandyContext candy) = SoloRig(s => s.Rocket(160, 200, impulse: 0f));
            _ = Act.BindRocket(scene, candy);

            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Assert.True(
                Interaction.StepUntil(scene, () => hand.doRotateCandy, maxFrames: 10),
                "the hand never started rotating its rocket candy");
        }

        [Fact]
        public void AReleasingHandSettlesOnlyOnceTheCandyIsClear()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);
            Act.TapClaw(scene, hand);
            Assert.Equal(MechanicalHand.STATE_HAND_RELEASE, hand.state);

            // Parked on the claw: inside MH_RELEASE_DISTANCE, so the hand must hold its state.
            Vector claw = hand.ClawPosition();
            Interaction.PlaceCandyAt(candy, claw);
            HeadlessGame.StepFrames(scene, 5);
            Assert.Equal(MechanicalHand.STATE_HAND_RELEASE, hand.state);

            // Well clear of the claw, and far outside MH_GRAB_DISTANCE so it cannot be re-grabbed.
            Interaction.PlaceCandyAt(candy, new Vector(claw.X + (MechanicalHand.MH_RELEASE_DISTANCE * 3f), claw.Y));
            Assert.True(
                Interaction.StepUntil(scene, () => hand.state == MechanicalHand.STATE_HAND_IDLE, maxFrames: 30),
                "the hand never settled to idle");
        }

        [Fact]
        public void IdleHandsInRangeArmBothClapCooldowns()
        {
            (GameScene scene, CandyContext candy) = DuoRig();
            MechanicalHand first = scene.Hands()[0];
            MechanicalHand second = scene.Hands()[1];
            Act.ArmClap(first);

            // Park the pair together, far enough from the candy that neither can grab it.
            Vector meeting = new(candy.WholeBody.Point.pos.X + 400f, candy.WholeBody.Point.pos.Y);
            Act.MoveClawTo(first, meeting);
            Act.MoveClawTo(second, new Vector(meeting.X + 50f, meeting.Y));
            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(MechanicalHand.STATE_HAND_IDLE, first.state);
            Assert.Equal(MechanicalHand.STATE_HAND_IDLE, second.state);
            Assert.True(first.clapTimer > 0f, "the first hand's clap cooldown was not armed");
            Assert.True(second.clapTimer > 0f, "the second hand's clap cooldown was not armed");
        }

        /// <summary>One rotatable hand, parked out of reach; the candy hovers where it loads.</summary>
        private static (GameScene Scene, CandyContext Candy) SoloRig(Func<Scenario, Scenario> extra = null)
        {
            return Rig(extra, handCount: 1);
        }

        /// <summary>Two rotatable hands, both parked out of reach on opposite sides.</summary>
        private static (GameScene Scene, CandyContext Candy) DuoRig(Func<Scenario, Scenario> extra = null)
        {
            return Rig(extra, handCount: 2);
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> extra, int handCount)
        {
            Scenario scenario = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Hand(20, 40, segmentLength: 20, segmentAngle: 90f, rotatable: true);
            if (handCount > 1)
            {
                scenario = scenario.Hand(300, 40, segmentLength: 20, segmentAngle: 90f, rotatable: true);
            }

            GameScene scene = (extra?.Invoke(scenario) ?? scenario).Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
