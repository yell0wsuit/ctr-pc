using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
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

            Assert.Equal(MechanicalHandState.Releasing, hand.State);
            Assert.False(hand.DoRotateCandy);
            Assert.False(Act.SettleOwedDropSound(hand));
        }

        [Fact]
        public void DetachActiveHandsReleasesAndStillOwesTheDropSound()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            scene.DetachActiveHands();

            Assert.Equal(MechanicalHandState.Releasing, hand.State);
            Assert.False(hand.DoRotateCandy);
            Assert.True(Act.SettleOwedDropSound(hand));
        }

        [Fact]
        public void DetachHandsForPointReleasesAndOwesNoDropSound()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            scene.DetachHandsForPoint(candy.WholeBody.Point);

            Assert.Equal(MechanicalHandState.Releasing, hand.State);
            Assert.False(hand.DoRotateCandy);
            Assert.False(Act.SettleOwedDropSound(hand));
        }

        [Fact]
        public void CandyRemovalReleasesAndOwesNoDropSound()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            scene.BreakCandyBody(candy.WholeBody);

            Assert.Equal(MechanicalHandState.Releasing, hand.State);
            Assert.False(hand.DoRotateCandy);
            Assert.False(Act.SettleOwedDropSound(hand));
        }

        [Fact]
        public void StealClearsTheRivalHandsRotationFlag()
        {
            // Both reference decompiles omit this clear (iOS 38981, WP7 GameScene.cs:1569), which was
            // safe only because Experiments had one candy per level. DX supports many, so rotation
            // state must not outlive the hold it belongs to.
            (GameScene scene, CandyContext candy) = DuoRig(s => s.Rocket(160, 200, impulse: 0f));
            _ = Act.BindRocket(scene, candy);
            MechanicalHand first = Act.GrabWithHand(scene, candy, handIndex: 0);
            Assert.True(
                Interaction.StepUntil(scene, () => first.DoRotateCandy, maxFrames: 10),
                "the first hand never started rotating its rocket candy");

            MechanicalHand second = Act.GrabWithHand(scene, candy, handIndex: 1);

            Assert.Same(second, candy.Lifecycle.Attachments.Hand);
            Assert.Equal(MechanicalHandState.Releasing, first.State);
            Assert.False(first.DoRotateCandy);
        }

        [Fact]
        public void AStolenFromHandDoesNotSpinItsNextCandy()
        {
            // Rocket candy at 160,200; a second, plain candy parked away from it.
            (GameScene scene, CandyContext rocketCandy) = DuoRig(s => s
                .Rocket(160, 200, impulse: 0f)
                .Candy(60, 200, number: "2"));
            CandyContext plainCandy = scene.Candies()[1];
            Interaction.Hover(plainCandy);
            _ = Act.BindRocket(scene, rocketCandy);

            MechanicalHand first = Act.GrabWithHand(scene, rocketCandy, handIndex: 0);
            Assert.True(
                Interaction.StepUntil(scene, () => first.DoRotateCandy, maxFrames: 10),
                "the first hand never started rotating its rocket candy");
            _ = Act.GrabWithHand(scene, rocketCandy, handIndex: 1);

            // The stolen-from hand settles, then takes the plain candy instead.
            Interaction.PlaceCandyAt(rocketCandy, new Vector(2000f, 2000f));
            Assert.True(
                Interaction.StepUntil(scene, () => first.State == MechanicalHandState.Idle, maxFrames: 60),
                "the stolen-from hand never settled to idle");
            _ = Act.GrabWithHand(scene, plainCandy, handIndex: 0);

            // The update loop rotates Main when present, falling back to Visual, so watch the same one.
            GameObject spun = plainCandy.WholeBody.Main ?? plainCandy.WholeBody.Visual;
            float before = spun.rotation;
            Act.RotateSegment(scene, first);

            Assert.False(first.DoRotateCandy);
            Assert.Equal(before, spun.rotation);
        }

        [Fact]
        public void ARegrabbedRocketCandyStillRotates()
        {
            (GameScene scene, CandyContext candy) = DuoRig(s => s.Rocket(160, 200, impulse: 0f));
            _ = Act.BindRocket(scene, candy);
            MechanicalHand first = Act.GrabWithHand(scene, candy, handIndex: 0);
            Assert.True(
                Interaction.StepUntil(scene, () => first.DoRotateCandy, maxFrames: 10),
                "the first hand never started rotating its rocket candy");

            MechanicalHand second = Act.GrabWithHand(scene, candy, handIndex: 1);
            Act.TapClaw(scene, second);

            // A releasing hand settles only once the candy is outside MH_RELEASE_DISTANCE, and the
            // grab helper parks the claw on the candy, so the hand has to be walked clear first.
            Vector parked = new(
                candy.WholeBody.Point.pos.X + (MechanicalHand.MH_RELEASE_DISTANCE * 4f),
                candy.WholeBody.Point.pos.Y);
            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () => Act.MoveClawTo(first, parked),
                    () => first.State == MechanicalHandState.Idle,
                    maxFrames: 30),
                "the stolen-from hand never settled to idle");
            _ = Act.GrabWithHand(scene, candy, handIndex: 0);

            Assert.True(
                Interaction.StepUntil(scene, () => first.DoRotateCandy, maxFrames: 10),
                "rotation was not re-derived after the candy came back");
        }

        [Fact]
        public void GrabbingAPlainCandyNeverStartsRotation()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            HeadlessGame.StepFrames(scene, 10);

            Assert.Equal(MechanicalHandState.HoldingCandy, hand.State);
            Assert.False(hand.DoRotateCandy);
        }

        [Fact]
        public void GrabbingARocketCandyStartsRotation()
        {
            (GameScene scene, CandyContext candy) = SoloRig(s => s.Rocket(160, 200, impulse: 0f));
            _ = Act.BindRocket(scene, candy);

            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Assert.True(
                Interaction.StepUntil(scene, () => hand.DoRotateCandy, maxFrames: 10),
                "the hand never started rotating its rocket candy");
        }

        [Fact]
        public void AReleasingHandSettlesOnlyOnceTheCandyIsClear()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);
            Act.TapClaw(scene, hand);
            Assert.Equal(MechanicalHandState.Releasing, hand.State);

            // Parked on the claw: inside MH_RELEASE_DISTANCE, so the hand must hold its state.
            Vector claw = hand.ClawPosition();
            Interaction.PlaceCandyAt(candy, claw);
            HeadlessGame.StepFrames(scene, 5);
            Assert.Equal(MechanicalHandState.Releasing, hand.State);

            // Well clear of the claw, and far outside MH_GRAB_DISTANCE so it cannot be re-grabbed.
            Interaction.PlaceCandyAt(candy, new Vector(claw.X + (MechanicalHand.MH_RELEASE_DISTANCE * 3f), claw.Y));
            Assert.True(
                Interaction.StepUntil(scene, () => hand.State == MechanicalHandState.Idle, maxFrames: 30),
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

            Assert.Equal(MechanicalHandState.Idle, first.State);
            Assert.Equal(MechanicalHandState.Idle, second.State);
            Assert.True(first.ClapTimer > 0f, "the first hand's clap cooldown was not armed");
            Assert.True(second.ClapTimer > 0f, "the second hand's clap cooldown was not armed");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void EveryReleaseLeavesTheSameState(bool afterDropSound)
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            if (afterDropSound)
            {
                hand.ReleaseCandyAfterDropSound();
            }
            else
            {
                hand.ReleaseCandy();
            }

            Assert.Equal(MechanicalHandState.Releasing, hand.State);
            Assert.False(hand.DoRotateCandy);
        }

        [Fact]
        public void SettleReportsWhoOwesTheDropSound()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            hand.ReleaseCandy();
            Assert.Equal(HandSettle.Stayed, hand.TrySettleToIdle(MechanicalHand.MH_RELEASE_DISTANCE));
            Assert.Equal(HandSettle.SettledOwingDropSound, hand.TrySettleToIdle(MechanicalHand.MH_RELEASE_DISTANCE + 1f));
            Assert.Equal(MechanicalHandState.Idle, hand.State);

            hand.ReleaseCandyAfterDropSound();
            Assert.Equal(HandSettle.Settled, hand.TrySettleToIdle(MechanicalHand.MH_RELEASE_DISTANCE + 1f));
            Assert.Equal(MechanicalHandState.Idle, hand.State);
        }

        [Fact]
        public void GrabbingStartsAHoldWithNoInheritedRotation()
        {
            (GameScene scene, CandyContext candy) = SoloRig(s => s.Rocket(160, 200, impulse: 0f));
            _ = Act.BindRocket(scene, candy);
            MechanicalHand hand = Act.GrabWithHand(scene, candy);
            Assert.True(
                Interaction.StepUntil(scene, () => hand.DoRotateCandy, maxFrames: 10),
                "the hand never started rotating its rocket candy");

            hand.GrabCandy();

            Assert.Equal(MechanicalHandState.HoldingCandy, hand.State);
            Assert.False(hand.DoRotateCandy);
        }

        [Theory]
        [InlineData(true, true)]    // armed and cool: claps
        [InlineData(false, false)]  // neither armed: no clap
        public void ClapFiresOnlyWhenAHandIsArmed(bool armFirst, bool expectedClap)
        {
            (GameScene scene, CandyContext candy) = DuoRig();
            MechanicalHand first = scene.Hands()[0];
            MechanicalHand second = scene.Hands()[1];
            if (armFirst)
            {
                first.ArmClap();
            }

            Vector meeting = new(candy.WholeBody.Point.pos.X + 400f, candy.WholeBody.Point.pos.Y);
            Act.MoveClawTo(first, meeting);
            Act.MoveClawTo(second, new Vector(meeting.X + 50f, meeting.Y));

            Assert.Equal(expectedClap, first.TryClapWith(second));
            Assert.True(first.ClapTimer > 0f, "the first hand's cooldown was not armed");
            Assert.True(second.ClapTimer > 0f, "the second hand's cooldown was not armed");
        }

        [Fact]
        public void ClapDoesNotFireOutOfRangeAndLeavesCooldownsAlone()
        {
            (GameScene scene, CandyContext candy) = DuoRig();
            MechanicalHand first = scene.Hands()[0];
            MechanicalHand second = scene.Hands()[1];
            first.ArmClap();

            Vector meeting = new(candy.WholeBody.Point.pos.X + 400f, candy.WholeBody.Point.pos.Y);
            Act.MoveClawTo(first, meeting);
            Act.MoveClawTo(second, new Vector(meeting.X + (MechanicalHand.MH_CLAP_DISTANCE * 2f), meeting.Y));

            Assert.False(first.TryClapWith(second));
            Assert.Equal(0f, first.ClapTimer);
            Assert.Equal(0f, second.ClapTimer);
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
