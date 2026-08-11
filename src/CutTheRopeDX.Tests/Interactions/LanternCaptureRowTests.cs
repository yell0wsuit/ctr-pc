using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Lantern capture" row: capture is terminal for the candy's riders. It
    /// strips ropes, hand, rocket, bubble, snail (giving the snail weight back), ants and mouse.
    /// </summary>
    public sealed class LanternCaptureRowTests
    {
        [Fact]
        public void LanternCaptureReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            Act.CaptureInLantern(scene, candy);

            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void LanternCaptureDetachesTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.CaptureInLantern(scene, candy);

            Assert.Null(candy.Lifecycle.Attachments.Hand);
            Assert.NotEqual(MechanicalHandState.HoldingCandy, hand.State);
        }

        [Fact]
        public void LanternCaptureExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.CaptureInLantern(scene, candy);

            Assert.False(candy.Lifecycle.Attachments.HasActiveRocket);
            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, rocket.state);
        }

        [Fact]
        public void LanternCapturePopsItsBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            _ = Act.CaptureInBubble(scene, candy);

            Act.CaptureInLantern(scene, candy);

            Assert.Null(candy.WholeBody.Bubble);
        }

        [Fact]
        public void LanternCaptureDetachesItsSnailAndGivesTheWeightBack()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);
            Assert.Equal(1 + SnailWeight.PerSnailWeight, candy.WholeBody.Point.weight);

            Act.CaptureInLantern(scene, candy);

            Assert.Equal(0, scene.SnailCount(candy));
            Assert.Equal(SnailWeight.MinWeight, candy.WholeBody.Point.weight);
        }

        [Fact]
        public void LanternCaptureTakesTheCandyOffTheAnts()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            Act.CaptureInLantern(scene, candy);

            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
        }

        [Fact]
        public void LanternCaptureMakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Act.CaptureInLantern(scene, candy);

            Assert.False(scene.MouseCarries(candy));
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment)
        {
            // The lantern parks in a corner so it cannot capture during setup; Act.CaptureInLantern
            // brings it to the candy.
            GameScene scene = attachment(Scenario.New().Candy(160, 200).OmNom(20, 460).Lantern(20, 40)).Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
