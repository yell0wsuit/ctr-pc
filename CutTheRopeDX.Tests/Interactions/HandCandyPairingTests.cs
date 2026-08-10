using System;
using System.Collections.Generic;
using System.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Pins the one hand fact the hand cannot enforce on its own: a hand's
    /// <see cref="MechanicalHandState"/> and the candy's <see cref="CandyAttachments.Hand"/> are two
    /// separate records of the same hold, written by two separate statements at every grab and
    /// release site. They must agree after every path, or a release can reach for the wrong candy -
    /// which is what the <c>?? star</c> fallbacks at the release sites quietly paper over.
    /// </summary>
    public sealed class HandCandyPairingTests
    {
        [Fact]
        public void AGrabPairsExactlyOneCandyToTheHand()
        {
            (GameScene scene, CandyContext candy) = DuoRig();

            _ = Act.GrabWithHand(scene, candy);

            AssertPairingConsistent(scene, "after a grab");
        }

        [Fact]
        public void AStealMovesThePairingToTheNewHand()
        {
            (GameScene scene, CandyContext candy) = DuoRig();
            MechanicalHand first = Act.GrabWithHand(scene, candy, handIndex: 0);

            MechanicalHand second = Act.GrabWithHand(scene, candy, handIndex: 1);

            Assert.NotSame(first, second);
            AssertPairingConsistent(scene, "after a steal");
        }

        [Fact]
        public void AClawTapClearsThePairing()
        {
            (GameScene scene, CandyContext candy) = DuoRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.TapClaw(scene, hand);

            AssertPairingConsistent(scene, "after a claw tap");
        }

        [Fact]
        public void DetachActiveHandsClearsEveryPairing()
        {
            (GameScene scene, CandyContext candy) = DuoRig();
            _ = Act.GrabWithHand(scene, candy);

            scene.DetachActiveHands();

            AssertPairingConsistent(scene, "after detaching all hands");
        }

        [Fact]
        public void DetachHandsForPointClearsOnlyThatCandysPairing()
        {
            (GameScene scene, CandyContext first) = DuoRig(s => s.Candy(60, 200, number: "2"));
            CandyContext second = scene.Candies()[1];
            Interaction.Hover(second);
            MechanicalHand firstHand = Act.GrabWithHand(scene, first, handIndex: 0);
            MechanicalHand secondHand = Act.GrabWithHand(scene, second, handIndex: 1);

            scene.DetachHandsForPoint(first.WholeBody.Point);

            Assert.Null(first.Lifecycle.Attachments.Hand);
            Assert.Same(secondHand, second.Lifecycle.Attachments.Hand);
            _ = firstHand;
            AssertPairingConsistent(scene, "after detaching one candy's hand");
        }

        [Fact]
        public void RemovingTheCandyClearsThePairing()
        {
            (GameScene scene, CandyContext candy) = DuoRig();
            _ = Act.GrabWithHand(scene, candy);

            scene.BreakCandyBody(candy.WholeBody);

            AssertPairingConsistent(scene, "after the candy was removed");
        }

        [Fact]
        public void SettlingBackToIdleLeavesNoPairing()
        {
            (GameScene scene, CandyContext candy) = DuoRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);
            Act.TapClaw(scene, hand);

            Vector claw = hand.ClawPosition();
            Interaction.PlaceCandyAt(candy, new Vector(claw.X + (MechanicalHand.MH_RELEASE_DISTANCE * 3f), claw.Y));
            Assert.True(
                Interaction.StepUntil(scene, () => hand.State == MechanicalHandState.Idle, maxFrames: 30),
                "the hand never settled to idle");

            AssertPairingConsistent(scene, "after settling to idle");
        }

        /// <summary>
        /// Asserts the hand-side and candy-side records of every hold agree: a holding hand is
        /// claimed by exactly one candy, any other hand by none, and every claimed candy names a
        /// hand of this scene that agrees it is holding.
        /// </summary>
        /// <param name="scene">Scene to check.</param>
        /// <param name="stage">What just happened, for the failure message.</param>
        private static void AssertPairingConsistent(GameScene scene, string stage)
        {
            List<MechanicalHand> hands = scene.Hands();
            List<CandyContext> candies = scene.Candies();

            foreach (MechanicalHand hand in hands)
            {
                int claims = candies.Count(c => c.Lifecycle.Attachments.Hand == hand);
                int expected = hand.State == MechanicalHandState.HoldingCandy ? 1 : 0;
                Assert.True(
                    claims == expected,
                    $"{stage}: a {hand.State} hand is claimed by {claims} candies, expected {expected}");
            }

            foreach (CandyContext candy in candies)
            {
                MechanicalHand holder = candy.Lifecycle.Attachments.Hand;
                if (holder == null)
                {
                    continue;
                }

                Assert.True(hands.Contains(holder), $"{stage}: a candy names a hand the scene does not own");
                Assert.True(
                    holder.State == MechanicalHandState.HoldingCandy,
                    $"{stage}: a candy is held by a hand in state {holder.State}");
            }
        }

        /// <summary>Two hands and a hovering candy, so steals and rivalry are reachable.</summary>
        private static (GameScene Scene, CandyContext Candy) DuoRig(Func<Scenario, Scenario> extra = null)
        {
            Scenario scenario = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Hand(20, 40, segmentLength: 20, segmentAngle: 90f)
                .Hand(300, 40, segmentLength: 20, segmentAngle: 90f);

            GameScene scene = (extra?.Invoke(scenario) ?? scenario).Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
