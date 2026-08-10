using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Pins the post-grab grace window that keeps a bouncer from instantly stripping a candy out of
    /// a hand that just caught it. iOS Experiments gates its bouncer detach on
    /// <c>time - candyHandCatchTime &gt; 0.1</c>; DX scopes the same window to the individual hold.
    /// </summary>
    public sealed class BouncerHandGraceTests
    {
        [Fact]
        public void ABouncerBouncesAFreeCandy()
        {
            (GameScene scene, CandyContext candy, Bouncer bouncer) = Rig();

            Interaction.PlaceCandyAt(candy, new Vector(bouncer.x, bouncer.y));
            HeadlessGame.StepFrames(scene, 2);

            Assert.NotEqual(0f, candy.WholeBody.Point.v.Y);
        }

        [Fact]
        public void AFreshGrabIsProtectedFromTheBouncer()
        {
            (GameScene scene, CandyContext candy, _) = Rig();
            MechanicalHand hand = scene.Hands()[0];
            _ = candy;

            hand.GrabCandy();

            Assert.False(hand.CanBeDetachedByBouncer);
            _ = scene;
        }

        [Fact]
        public void TheGraceWindowExpires()
        {
            (GameScene scene, CandyContext candy, _) = Rig();
            MechanicalHand hand = scene.Hands()[0];
            _ = candy;

            hand.GrabCandy();
            hand.Update(MechanicalHand.MH_BOUNCER_GRACE);

            Assert.True(hand.CanBeDetachedByBouncer);
        }

        [Fact]
        public void ABouncerDoesNotStripAJustGrabbedCandy()
        {
            (GameScene scene, CandyContext candy, Bouncer bouncer) = Rig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            // Re-stamp the hold, then walk the claw onto the bouncer: this is a hand carrying a candy
            // into a bouncer the instant after catching it.
            hand.GrabCandy();
            Act.MoveClawTo(hand, new Vector(bouncer.x, bouncer.y));
            HeadlessGame.StepFrames(scene, 1);

            Assert.Same(hand, candy.Lifecycle.Attachments.Hand);
        }

        [Fact]
        public void ABouncerStripsTheCandyOnceTheWindowExpires()
        {
            (GameScene scene, CandyContext candy, Bouncer bouncer) = Rig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () => Act.MoveClawTo(hand, new Vector(bouncer.x, bouncer.y)),
                    () => candy.Lifecycle.Attachments.Hand == null,
                    maxFrames: 30),
                "the bouncer never took the candy off the hand");
        }

        [Fact]
        public void OneHandsGraceDoesNotShieldAnotherHandsCandy()
        {
            // The iOS window is a single scene-wide timestamp, which only worked because Experiments
            // had one candy. Here two holds run on different clocks and must be judged separately.
            GameScene scene = Scenario.New()
                .Candy(100, 200)
                .Candy(400, 200, number: "2")
                .OmNom(20, 460)
                .Hand(60, 40, segmentLength: 20, segmentAngle: 90f)
                .Hand(440, 40, segmentLength: 20, segmentAngle: 90f)
                .Bouncer(100, 330)
                .Bouncer(400, 330)
                .Build();

            CandyContext stale = scene.Candies()[0];
            CandyContext fresh = scene.Candies()[1];
            Interaction.Hover(stale);
            Interaction.Hover(fresh);

            MechanicalHand staleHand = Act.GrabWithHand(scene, stale, handIndex: 0);
            MechanicalHand freshHand = Act.GrabWithHand(scene, fresh, handIndex: 1);

            // Age both holds well past the window while the claws are still clear of the bouncers,
            // then re-arm only the second one.
            HeadlessGame.StepFrames(scene, 10);
            freshHand.GrabCandy();

            Act.MoveClawTo(staleHand, new Vector(scene.Bouncers()[0].x, scene.Bouncers()[0].y));
            Act.MoveClawTo(freshHand, new Vector(scene.Bouncers()[1].x, scene.Bouncers()[1].y));
            HeadlessGame.StepFrames(scene, 1);

            Assert.Null(stale.Lifecycle.Attachments.Hand);
            Assert.Same(freshHand, fresh.Lifecycle.Attachments.Hand);
        }

        /// <summary>One hand, one hovering candy, and a bouncer parked clear of both.</summary>
        private static (GameScene Scene, CandyContext Candy, Bouncer Bouncer) Rig()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Hand(20, 40, segmentLength: 20, segmentAngle: 90f)
                .Bouncer(160, 330)
                .Build();

            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy, scene.Bouncers()[0]);
        }
    }
}
