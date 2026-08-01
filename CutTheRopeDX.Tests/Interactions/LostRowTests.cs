using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Lost" row: a candy destroyed on spikes must take its attachments down
    /// with it, both through the break itself and through the GameLost that follows it.
    /// </summary>
    public sealed class LostRowTests
    {
        [Fact]
        public void Lost_ReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            Act.BreakOnSpikes(scene, candy);

            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void Lost_DetachesTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Null(candy.capturingHand);
            Assert.NotEqual(MechanicalHand.STATE_HAND_CANDY, hand.state);
        }

        [Fact]
        public void Lost_ExhaustsItsRocket()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.False(candy.HasActiveRocket);
            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, rocket.state);
        }

        [Fact]
        public void Lost_PopsItsBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            _ = Act.CaptureInBubble(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Null(candy.bubble);
        }

        [Fact]
        public void Lost_DetachesItsSnail_ButKeepsTheSnailWeightOnTheRetiredPoint()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.Equal(0, scene.SnailCount(candy));

            // Matrix cell says "detach (+weight)"; BreakCandyFromHazard only detaches. Same shape as
            // the eaten row - the weight stays on a point nothing reads again.
            Assert.Equal(1 + SnailWeight.PerSnailWeight, candy.point.weight);
        }

        [Fact]
        public void Lost_LeavesTheBrokenCandyRidingTheAntLane()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            // Matrix cell says "detach". Neither the break nor GameLost takes the candy off the ant
            // conveyor, so it stays bound to the segment it died on.
            Assert.NotNull(candy.antSegment);
        }

        [Fact]
        public void Lost_MakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Act.BreakOnSpikes(scene, candy);

            Assert.False(scene.MouseCarries(candy));
            Assert.False(candy.carriedByMouse);
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment)
        {
            // Om Nom sits in the far corner so nothing is eaten; the spikes start off in another
            // corner and Act.BreakOnSpikes brings them to the candy.
            GameScene scene = attachment(Scenario.New().Candy(160, 200).OmNom(20, 460).Spikes(20, 40)).Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
