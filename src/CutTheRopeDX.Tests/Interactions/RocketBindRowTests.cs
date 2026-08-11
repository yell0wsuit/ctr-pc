using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Rocket bind" row: the rocket steals from nobody. It coexists with a
    /// hand or a mouse on a zero-rest FLY bind, keeps ropes and snail, pops a bubble it cannot
    /// share the point with, and burns out any rocket already on the candy.
    /// </summary>
    public sealed class RocketBindRowTests
    {
        [Fact]
        public void RocketBindKeepsItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));

            _ = Act.BindRocket(scene, candy);

            Assert.Equal(1, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void RocketBindCoexistsWithTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Rocket rocket = Act.BindRocket(scene, candy);

            Assert.Same(hand, candy.Lifecycle.Attachments.Hand);
            Assert.Same(rocket, candy.Lifecycle.Attachments.Rocket);

            // A holder's rocket skips the reel-in and goes straight to FLY, straining at the claw
            // until the hand lets go.
            Assert.Equal(Rocket.STATE_ROCKET_FLY, rocket.state);
        }

        [Fact]
        public void RocketBindExhaustsTheRocketAlreadyOnTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(60, 200, impulse: 0f));
            Rocket first = Act.BindRocket(scene, candy, rocketIndex: 1);

            Rocket second = Act.BindRocket(scene, candy, rocketIndex: 0);

            Assert.Same(second, candy.Lifecycle.Attachments.Rocket);
            Assert.Equal(Rocket.STATE_ROCKET_EXAUST, first.state);
        }

        [Fact]
        public void RocketBindPopsItsBubble()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            Bubble bubble = Act.CaptureInBubble(scene, candy);

            _ = Act.BindRocket(scene, candy);

            Assert.Null(candy.WholeBody.Bubble);
            Assert.True(bubble.popped);
        }

        [Fact]
        public void RocketBindTakesTheSnailAlong()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            _ = Act.BindRocket(scene, candy);

            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void RocketBindTakesTheCandyOffTheAntsByFlyingItAway()
        {
            // "Implicit release" is literal here: binding the rocket detaches nothing, and the ant
            // carry keeps overwriting the candy's position (and the rocket's) for a good two
            // seconds. Only once thrust has dragged the candy out of the segment do the ants lose
            // it - hence the long flight before the assertion.
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"), rocketImpulse: 200f);
            Act.CarryByAnts(scene, candy);

            _ = Act.BindRocket(scene, candy);
            HeadlessGame.StepFrames(scene, 150);

            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
        }

        [Fact]
        public void RocketBindCoexistsWithTheMouseCarryingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Rocket rocket = Act.BindRocket(scene, candy);

            Assert.True(scene.MouseCarries(candy));
            Assert.Same(rocket, candy.Lifecycle.Attachments.Rocket);
            Assert.Equal(Rocket.STATE_ROCKET_FLY, rocket.state);
        }

        [Fact]
        public void RocketFliesStraightAfterReleaseFromSecondMouse()
        {
            GameScene scene = Scenario.New()
                .MapSize(640, 480)
                .Special(0)
                .Design("nightLevel", "false")
                .Design("useMobilePhysics", "true")
                .Candy(82, 100, number: "0")
                .OmNom(106, 346)
                .Mouse(85, 194, radius: 50, index: 1, activeTime: 1f)
                .Mouse(246, 142, radius: 50, index: 2, activeTime: 1f)
                .Rocket(245, 115, angle: 0f, impulse: 20f, impulseFactor: 0.6f)
                .Build();
            CandyContext candy = scene.Candy();

            Assert.True(
                Interaction.StepUntil(scene, () => scene.Mice()[1].IsActive && scene.MouseCarries(candy), maxFrames: 360),
                "the second mouse never received the candy");
            Assert.NotNull(candy.Lifecycle.Attachments.Rocket);

            Mouse second = scene.Mice()[1];
            Rocket rocket = candy.Lifecycle.Attachments.Rocket;
            Assert.True(scene.TouchDownXYIndex((int)second.x, (int)second.y, 0));
            float releaseX = candy.WholeBody.Point.pos.X;
            float releaseY = candy.WholeBody.Point.pos.Y;
            float rocketReleaseX = rocket.x;

            HeadlessGame.StepFrames(scene, 60);

            Assert.True(
                candy.WholeBody.Point.pos.X > releaseX + 20f,
                "the rocket did not fly forward after release");
            Assert.True(rocket.x > rocketReleaseX + 20f, "the rocket visual did not follow the candy");
            Assert.InRange(candy.WholeBody.Point.pos.Y, releaseY - 5f, releaseY + 5f);
            Assert.InRange(rocket.y, releaseY - 5f, releaseY + 5f);
            Assert.InRange(rocket.x - candy.WholeBody.Point.pos.X, -2f, 2f);
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment, float rocketImpulse = 0f)
        {
            // Rocket 0 is the one under test and parks in a corner until Act.BindRocket brings it
            // over. Thrust is off by default so the candy stays put while the cell is checked.
            GameScene scene = attachment(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .Rocket(20, 40, impulse: rocketImpulse))
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
