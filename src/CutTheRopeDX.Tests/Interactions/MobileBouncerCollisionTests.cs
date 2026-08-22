using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>Mobile-reference collision sampling for bouncers.</summary>
    public sealed class MobileBouncerCollisionTests
    {
        [Fact]
        public void CrossingPathDoesNotBounceAfterCurrentCandyBoxHasPassed()
        {
            GameScene scene = Scenario.New()
                .Design("useMobilePhysics", "true")
                .Candy(160, 100)
                .Bouncer(160, 300)
                .OmNom(20, 460)
                .Build();
            CandyBody body = scene.Candy().WholeBody;
            Bouncer bouncer = scene.Bouncers()[0];
            float expectedY = bouncer.y + 100f;

            body.Point.disableGravity = true;
            body.Point.pos = new Vector(bouncer.x, bouncer.y - 100f);
            body.Point.prevPos = new Vector(bouncer.x, bouncer.y - 300f);
            body.Visual.x = body.Point.pos.X;
            body.Visual.y = body.Point.pos.Y;

            HeadlessGame.StepFrames(scene, 1);

            Assert.Equal(expectedY, body.Point.pos.Y, precision: 3);
        }

        [Fact]
        public void CandyFromRightBambooTubeBouncesAtLargeBouncerLeftEdge()
        {
            GameScene scene = Scenario.New()
                .MapSize(320, 480)
                .Design("useMobilePhysics", "true")
                .Candy(291, 143)
                .Rope(289, 56, length: 80)
                .BambooTube(289, 268, TubeMouth.CatchesFalling)
                .Bouncer(157, 331, size: 2)
                .OmNom(156, 425)
                .Build();
            CandyContext candy = scene.Candy();

            Act.CutRope(scene, scene.Grabs()[0]);
            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Transport?.BambooTube != null, maxFrames: 180),
                "the falling candy never entered the right bamboo tube");
            Assert.True(
                Interaction.StepUntil(scene, () => candy.Lifecycle.Presence == CandyPresence.Present, maxFrames: 180),
                "the bamboo tube never released the candy");

            bool movedDownward = false;
            bool reversedUpward = false;
            for (int frame = 0; frame < 90 && candy.Lifecycle.Presence == CandyPresence.Present; frame++)
            {
                HeadlessGame.StepFrames(scene, 1);
                CandyBody body = candy.WholeBody;
                float verticalDelta = body.Point.pos.Y - body.Point.prevPos.Y;
                movedDownward |= verticalDelta > 0.01f;
                if (movedDownward && verticalDelta < -0.01f)
                {
                    reversedUpward = true;
                    break;
                }
            }

            Assert.True(reversedUpward, "the candy crossed the bouncer's left edge without bouncing upward");
        }
    }
}
