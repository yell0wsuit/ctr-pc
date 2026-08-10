using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Eaten" row: an Om Nom swallowing a candy must leave nothing attached
    /// to it - the rope is released, the hand lets go, the rocket burns out, the bubble pops, the
    /// snail hops off, the ants drop it, and a mouse carrying it gives it up.
    /// </summary>
    public sealed class EatenRowTests
    {
        [Fact]
        public void EatenReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));
            Assert.Equal(1, scene.AttachedRopeCount(candy));
            Act.Eat(scene, candy);
            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void EatenDetachesTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);
            Act.Eat(scene, candy);
            Assert.Null(candy.Lifecycle.Attachments.Hand);
            Assert.NotEqual(MechanicalHandState.HoldingCandy, hand.State);
        }

        [Fact]
        public void EatenExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);
            Act.Eat(scene, candy);
            Assert.False(candy.Lifecycle.Attachments.HasActiveRocket);
            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, rocket.state);
        }

        [Fact]
        public void EatenPopsItsBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            _ = Act.CaptureInBubble(scene, candy);
            Act.Eat(scene, candy);
            Assert.Null(candy.WholeBody.Bubble);
        }

        [Fact]
        public void EatenDetachesItsSnailButKeepsTheSnailWeightOnTheRetiredPoint()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);
            Assert.Equal(1 + SnailWeight.PerSnailWeight, candy.WholeBody.Point.weight);
            Act.Eat(scene, candy);
            Assert.Equal(0, scene.SnailCount(candy));
            Assert.Equal(1 + SnailWeight.PerSnailWeight, candy.WholeBody.Point.weight);
        }

        [Fact]
        public void EatenDetachesTheCandyFromTheAntLane()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);
            Act.Eat(scene, candy);
            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
            Assert.Null(candy.Lifecycle.Attachments.LastAntSegment);
            Assert.False(candy.Lifecycle.Attachments.AntWaitingForExit);
            Assert.Equal(0f, candy.Lifecycle.Attachments.AntCooldown);
        }

        [Fact]
        public void EatenMakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);
            Act.Eat(scene, candy);
            Assert.False(scene.MouseCarries(candy));
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment)
        {
            GameScene scene = attachment(Scenario.New().Candy(160, 200).OmNom(20, 460)).Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
