using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Hand grab" row: the claw takes ownership of the candy. Ropes and a
    /// rocket stay on, a rival hand lets go, and everything that would fight the claw for the
    /// point - bubble, snail, ants, mouse - is stripped.
    /// </summary>
    public sealed class HandGrabRowTests
    {
        [Fact]
        public void HandGrabKeepsItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));

            _ = Act.GrabWithHand(scene, candy);

            Assert.Equal(1, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void HandGrabMakesTheRivalHandLetGo()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(60, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand rival = Act.GrabWithHand(scene, candy, handIndex: 1);

            MechanicalHand claimant = Act.GrabWithHand(scene, candy, handIndex: 0);

            Assert.Same(claimant, candy.Lifecycle.Attachments.Hand);
            Assert.NotEqual(MechanicalHandState.HoldingCandy, rival.State);
        }

        [Fact]
        public void HandGrabKeepsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            _ = Act.GrabWithHand(scene, candy);

            Assert.True(candy.Lifecycle.Attachments.HasActiveRocket);
            Assert.Same(rocket, candy.Lifecycle.Attachments.Rocket);
        }

        [Fact]
        public void HandGrabPopsItsBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            Bubble bubble = Act.CaptureInBubble(scene, candy);

            _ = Act.GrabWithHand(scene, candy);

            Assert.Null(candy.WholeBody.Bubble);
            Assert.True(bubble.popped);
        }

        [Fact]
        public void HandGrabDetachesItsSnailAndGivesTheWeightBack()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);
            Assert.Equal(1 + SnailWeight.PerSnailWeight, candy.WholeBody.Point.weight);

            _ = Act.GrabWithHand(scene, candy);

            Assert.Equal(0, scene.SnailCount(candy));
            Assert.Equal(SnailWeight.MinWeight, candy.WholeBody.Point.weight);
        }

        [Fact]
        public void HandGrabTakesTheCandyOffTheAnts()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            _ = Act.GrabWithHand(scene, candy);

            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
        }

        [Fact]
        public void HandGrabMakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            _ = Act.GrabWithHand(scene, candy);

            Assert.False(scene.MouseCarries(candy));
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment)
        {
            // Hand 0 is the grabber and starts out of reach in a corner; Act.GrabWithHand walks it
            // to the candy. A scenario that needs a rival hand adds its own as hand 1.
            GameScene scene = attachment(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .Hand(20, 40, segmentLength: 20, segmentAngle: 90f))
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
