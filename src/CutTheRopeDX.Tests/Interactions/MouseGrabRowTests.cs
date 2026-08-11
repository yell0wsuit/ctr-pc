using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Mouse grab" row: the mouse snatches the candy, cutting its ropes and
    /// taking it off a hand, but it steals from nobody else - a rocket, a bubble and a snail all
    /// travel into the hole with it.
    /// </summary>
    public sealed class MouseGrabRowTests
    {
        [Fact]
        public void MouseGrabReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            _ = Act.CarryByMouse(scene, candy);

            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void MouseGrabSnatchesTheCandyFromTheHand()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            _ = Act.CarryByMouse(scene, candy);

            Assert.Null(candy.Lifecycle.Attachments.Hand);
            Assert.NotEqual(MechanicalHandState.HoldingCandy, hand.State);
            Assert.True(scene.MouseCarries(candy));
        }

        [Fact]
        public void MouseGrabKeepsTheRocketOnTheStolenCandy()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            _ = Act.CarryByMouse(scene, candy);

            Assert.True(candy.Lifecycle.Attachments.HasActiveRocket);
            Assert.Same(rocket, candy.Lifecycle.Attachments.Rocket);
        }

        [Fact]
        public void MouseGrabKeepsTheBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            Bubble bubble = Act.CaptureInBubble(scene, candy);

            _ = Act.CarryByMouse(scene, candy);

            Assert.Same(bubble, candy.WholeBody.Bubble);
        }

        [Fact]
        public void MouseGrabTakesTheSnailAlong()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            _ = Act.CarryByMouse(scene, candy);

            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void MouseGrabTakesTheCandyOffTheAntsByDraggingItAway()
        {
            // The hole stays put above the lane: the ants only let go because the mouse pulls the
            // candy out of the segment, which is what "implicit release" means for this cell.
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"), mouseY: 140);
            Act.CarryByAnts(scene, candy);

            _ = Act.CarryByMouseWithoutMovingIt(scene, candy);
            HeadlessGame.StepFrames(scene, 30);

            Assert.True(scene.MouseCarries(candy));
            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
        }

        [Fact]
        public void ClickingTheCarryingMouseImmediatelyClearsCandyOwnership()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s);
            Mouse mouse = Act.CarryByMouse(scene, candy);

            Assert.True(scene.TouchDownXYIndex((int)mouse.x, (int)mouse.y, 0));

            Assert.False(scene.MouseCarries(candy));
            Assert.Equal(candy.Lifecycle.IsGravitySuppressed, candy.WholeBody.Point.disableGravity);
        }

        [Fact]
        public void RetreatTransfersTheSameCarryToTheNextMouse()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Mouse(160, 200, index: 1)
                .Mouse(300, 200, index: 2)
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            Mouse first = Act.CarryByMouse(scene, candy);
            Mouse second = scene.Mice()[1];

            first.BeginRetreat();

            Assert.True(
                Interaction.StepUntil(scene, () => second.IsActive && scene.MouseCarries(candy)),
                "the next mouse never received the carried candy");
            Assert.False(first.HasCandy);
            Assert.True(second.HasCandy);
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment, int mouseY = 40)
        {
            // The mouse hole starts away from the candy so nothing is stolen during setup, unless a
            // test parks it deliberately close (mouseY) to steal from where it stands.
            GameScene scene = attachment(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .Mouse(160, mouseY))
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
