using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Bubble capture" row: a bubble is the weakest claim on a candy. It keeps
    /// ropes and a carrying mouse, replaces an earlier bubble, and simply pops against a rocket or
    /// a snail rather than taking the candy from them.
    /// </summary>
    public sealed class BubbleCaptureRowTests
    {
        [Fact]
        public void BubbleCaptureKeepsItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));

            _ = Act.CaptureInBubble(scene, candy);

            Assert.Equal(1, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void BubbleCapturePopsAgainstARocketBoundCandyWithoutCapturingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Bubble bubble = Act.PushBubbleAgainst(scene, candy);

            Assert.True(bubble.popped);
            Assert.Null(candy.WholeBody.Bubble);
            Assert.Same(rocket, candy.Lifecycle.Attachments.Rocket);
        }

        [Fact]
        public void BubbleCaptureReplacesTheBubbleAlreadyCarryingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(60, 200));
            Bubble first = Act.CaptureInBubble(scene, candy, bubbleIndex: 1);

            Bubble second = Act.CaptureInBubble(scene, candy, bubbleIndex: 0);

            Assert.Same(second, candy.WholeBody.Bubble);
            Assert.NotSame(first, candy.WholeBody.Bubble);
        }

        [Fact]
        public void BubbleCapturePopsAgainstASnailRiddenCandy()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            Bubble bubble = Act.PushBubbleAgainst(scene, candy);

            Assert.True(bubble.popped);
            Assert.Null(candy.WholeBody.Bubble);
            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void BubbleCaptureTakesTheCandyOffTheAntsByFloatingItAway()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            Bubble bubble = Act.CaptureInBubble(scene, candy);
            HeadlessGame.StepFrames(scene, 150);

            // "Implicit release": capture detaches nothing by itself, and the ant carry keeps
            // overwriting the candy's position, but the bubble's lift eventually pulls it clear of
            // the segment and the lane lets go.
            Assert.Same(bubble, candy.WholeBody.Bubble);
            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
        }

        [Fact]
        public void BubbleCaptureLeavesTheMouseCarryingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Bubble bubble = Act.CaptureInBubble(scene, candy);

            Assert.Same(bubble, candy.WholeBody.Bubble);
            Assert.True(scene.MouseCarries(candy));
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment)
        {
            // Bubble 0 is the one under test and waits in a corner until Act brings it over.
            GameScene scene = attachment(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .Bubble(20, 40))
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
