using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>Mobile-reference trajectories involving a candy binding to a rocket.</summary>
    public sealed class MobileRocketTrajectoryTests
    {
        [Fact]
        public void RightSideApproachMovesLowerRocketDuringReelIn()
        {
            GameScene scene = ReferenceScene(includeUpperRocket: false);
            CandyContext candy = scene.Candy();
            Rocket rocket = scene.Rockets()[0];
            float authoredRocketX = rocket.x;
            float maximumRocketX = authoredRocketX;
            StartReferencePath(scene, candy);

            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Attachments.Rocket == rocket, maxFrames: 180),
                "the right-side approach never bound the candy to the lower rocket");

            for (int frame = 0; frame < 45; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
                maximumRocketX = System.MathF.Max(maximumRocketX, rocket.x);
            }

            // The iOS footage shows the lower rocket move down-right while reeling in the candy;
            // its red body is not pinned at the authored location.
            Assert.True(maximumRocketX > authoredRocketX + Scenario.Scale);
        }

        [Fact]
        public void CandyCarriedByLowerRocketDoesNotBindUpperRocket()
        {
            GameScene scene = ReferenceScene(includeUpperRocket: true);
            CandyContext candy = scene.Candy();
            Rocket lowerRocket = scene.Rockets()[0];
            Rocket upperRocket = scene.Rockets()[1];
            StartReferencePath(scene, candy);

            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Attachments.Rocket == lowerRocket, maxFrames: 180),
                "the candy never bound the lower rocket");

            bool upperRocketBound = false;
            string upperBind = null;
            for (int frame = 0; frame < 180; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
                if (!upperRocketBound && candy.Lifecycle.Attachments.Rocket == upperRocket)
                {
                    upperRocketBound = true;
                    upperBind = $"frame={frame}, candy=({candy.WholeBody.Point.pos.X},{candy.WholeBody.Point.pos.Y}), "
                        + $"visual=({candy.WholeBody.Visual.x},{candy.WholeBody.Visual.y}), "
                        + $"upper=({upperRocket.point.pos.X},{upperRocket.point.pos.Y}), "
                        + $"lower=({lowerRocket.point.pos.X},{lowerRocket.point.pos.Y})";
                }
            }

            Assert.False(upperRocketBound, $"the upper rocket caught the candy carried past it: {upperBind}");
        }

        private static GameScene ReferenceScene(bool includeUpperRocket)
        {
            Scenario scenario = Scenario.New()
                .MapSize(320, 480)
                .Design("useMobilePhysics", "true")
                .Candy(291, 143)
                .Rope(289, 56, length: 80)
                .BambooTube(289, 268, TubeMouth.CatchesFalling)
                .Rocket(108, 265, angle: -90f, impulse: 25f, time: 0.58f, impulseFactor: 0.6f);
            if (includeUpperRocket)
            {
                _ = scenario.Rocket(124, 182, angle: -90f, impulse: 25f, time: 0.27f, impulseFactor: 0.6f, isRotatable: true);
            }
            return scenario
                .Bouncer(157, 331, size: 2)
                .OmNom(156, 425)
                .Build();
        }

        private static void StartReferencePath(GameScene scene, CandyContext candy)
        {
            Act.CutRope(scene, scene.Grabs()[0]);
            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Transport?.BambooTube != null, maxFrames: 180),
                "the falling candy never entered the right bamboo tube");
        }
    }
}
