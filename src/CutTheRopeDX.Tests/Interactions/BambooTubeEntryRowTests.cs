using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Bamboo tube entry" row: like the magic hat, the tube teleports the
    /// candy - ropes, hand, mouse and ants are stripped, while the rocket (hidden), the bubble and
    /// the snail travel with it.
    /// </summary>
    public sealed class BambooTubeEntryRowTests
    {
        [Fact]
        public void TubeEntryReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(TubeMouth.CatchesFalling, s => s.Rope(160, 120, length: 40));
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void TubeEntryDetachesTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(TubeMouth.CatchesFalling, s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.Null(candy.Lifecycle.Attachments.Hand);
            Assert.NotEqual(MechanicalHandState.HoldingCandy, hand.State);
        }

        [Fact]
        public void TubeEntryCarriesTheRocketThroughHiddenForTheTransit()
        {
            (GameScene scene, CandyContext candy) = Rig(TubeMouth.CatchesFalling, s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.True(candy.Lifecycle.Attachments.HasActiveRocket);
            Assert.Same(rocket, candy.Lifecycle.Attachments.Rocket);
            Assert.False(rocket.visible);
        }

        [Fact]
        public void TubeEntryKeepsItsBubble()
        {
            // A bubbled candy rises, so the tube has to face down to catch it.
            (GameScene scene, CandyContext candy) = Rig(TubeMouth.CatchesRising, s => s.Bubble(160, 200));
            Bubble bubble = Act.CaptureInBubble(scene, candy);
            Interaction.Drop(candy);

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesRising);

            Assert.Same(bubble, candy.WholeBody.Bubble);
        }

        [Fact]
        public void TubeEntryCarriesTheSnailThrough()
        {
            (GameScene scene, CandyContext candy) = Rig(TubeMouth.CatchesFalling, s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void TubeEntryTakesTheCandyOffTheAnts()
        {
            // The lane runs left to right, so the tube faces left to meet the candy head on.
            (GameScene scene, CandyContext candy) = Rig(TubeMouth.CatchesRightward, s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesRightward);

            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
        }

        [Fact]
        public void TubeEntryMakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(TubeMouth.CatchesFalling, s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            Assert.False(scene.MouseCarries(candy));
        }

        private static (GameScene Scene, CandyContext Candy) Rig(TubeMouth mouth, Func<Scenario, Scenario> attachment)
        {
            // The tube parks in a corner; Act.EnterBambooTube brings it to the candy.
            GameScene scene = attachment(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .BambooTube(20, 40, mouth))
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
