using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Snail attach" row: a snail rides along with almost everything - ropes,
    /// hand, rocket, ants, mouse - but never shares a candy with a bubble or with another snail.
    /// </summary>
    public sealed class SnailAttachRowTests
    {
        [Fact]
        public void SnailAttachKeepsItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));

            _ = Act.RideSnail(scene, candy);

            Assert.Equal(1, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void SnailAttachCoexistsWithTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            _ = Act.RideSnail(scene, candy);

            Assert.Same(hand, candy.Lifecycle.Attachments.Hand);
            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void SnailAttachKeepsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            _ = Act.RideSnail(scene, candy);

            Assert.Same(rocket, candy.Lifecycle.Attachments.Rocket);
            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void SnailAttachPopsItsBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            Bubble bubble = Act.CaptureInBubble(scene, candy);

            _ = Act.RideSnail(scene, candy);

            Assert.Null(candy.WholeBody.Bubble);
            Assert.True(bubble.popped);
        }

        [Fact]
        public void SnailAttachDetachesTheSnailAlreadyOnTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(60, 200));
            Snail first = Act.RideSnail(scene, candy, snailIndex: 1);

            Snail second = Act.RideSnail(scene, candy, snailIndex: 0);

            Assert.Equal(1, scene.SnailCount(candy));
            Assert.Equal(Snail.SNAIL_STATE_ACTIVE, second.state);
            Assert.NotEqual(Snail.SNAIL_STATE_ACTIVE, first.state);
        }

        [Fact]
        public void SnailAttachCoexistsWithTheAntsCarryingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            _ = Act.RideSnail(scene, candy);

            Assert.NotNull(candy.Lifecycle.Attachments.AntSegment);
            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void SnailAttachLeavesTheMouseCarryingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            _ = Act.RideSnail(scene, candy);

            Assert.True(scene.MouseCarries(candy));
            Assert.Equal(1, scene.SnailCount(candy));
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment)
        {
            // Snail 0 is the one under test and waits in a corner until Act brings it over.
            GameScene scene = attachment(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .Snail(20, 40))
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
