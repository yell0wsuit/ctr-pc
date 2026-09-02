using System;
using System.Collections.Generic;
using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.GameMain.Tutorials;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    /// <summary>
    /// Each candy-scoped tutorial edge event is pushed from the interaction that owns its
    /// transition. Every case here arms one prompt, drives the real interaction, and asserts the
    /// prompt started, so a prompt that plays proves the production call site fired.
    /// </summary>
    public sealed class TutorialInteractionEventTests
    {
        [Fact]
        public void BubbleCaptureFiresWhenABubbleTakesTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig("bubbleCapture", s => s.Bubble(20, 40));

            _ = Act.CaptureInBubble(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void BubblePopFiresWhenThePlayerTapsTheBubbledCandy()
        {
            (GameScene scene, CandyContext candy) = Rig("bubblePop", s => s.Bubble(20, 40));
            _ = Act.CaptureInBubble(scene, candy);

            Vector touch = scene.ScreenPositionOf(candy.WholeBody.Point.pos);
            Assert.True(scene.TouchDownXYIndex(touch.X, touch.Y, 0), "the tap on the bubbled candy was not handled");

            Assert.Null(candy.WholeBody.Bubble);
            AssertPlaying(scene);
        }

        [Fact]
        public void LanternCatchFiresWhenTheLanternAcceptsTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig("lanternCatch", s => s.Lantern(20, 40));

            Act.CaptureInLantern(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void SockCatchFiresWhenAMagicHatSwallowsTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig(
                "sockCatch",
                s => s.Hat(20, 40, group: 1).Hat(300, 40, group: 1));

            Act.EnterHat(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void MouseGrabFiresWhenTheActiveMouseTakesTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig("mouseGrab", s => s.Mouse(160, 40));

            _ = Act.CarryByMouse(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void SpiderStealFiresWhenTheSpiderRetiresTheCandy()
        {
            (GameScene scene, _) = Rig(
                "spiderSteal",
                s => s.Grab(160, 120, radius: 100f, spider: true, moveLength: -1f));
            Grab spiderGrab = scene.Grabs()[0];
            Assert.True(
                Interaction.StepUntil(scene, () => spiderGrab.Rope != null),
                "the spider hook never attached to the candy");

            scene.SpiderWon(spiderGrab);

            AssertPlaying(scene);
        }

        [Fact]
        public void HandGrabFiresOnlyWhenTheClawCapturesTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig(
                "handGrab",
                s => s.Hand(20, 40, segmentLength: 20, segmentAngle: 90f));

            _ = Act.GrabWithHand(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void RopeCutFiresWhenASwipeSeversTheCandysRope()
        {
            (GameScene scene, _) = Rig("ropeCut", s => s.Rope(160, 120, length: 40));

            Act.CutRope(scene, scene.Grabs()[0]);

            AssertPlaying(scene);
        }

        [Fact]
        public void StarCollectedFiresWhenTheCandyTakesAStar()
        {
            (GameScene scene, CandyContext candy) = Rig("starCollected", s => s.Star(20, 40));

            Act.CollectStar(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void CandyEatenFiresWhenOmNomRetiresTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig("candyEaten", s => s);

            Act.Eat(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void PipeEnterFiresWhenTheBambooTubeSwallowsTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig(
                "pipeEnter",
                s => s.BambooTube(20, 40, TubeMouth.CatchesFalling));

            Act.EnterBambooTube(scene, candy, TubeMouth.CatchesFalling);

            AssertPlaying(scene);
        }

        [Fact]
        public void SpikeHitFiresWhenOrdinarySpikesBreakTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig("spikeHit", s => s.Spikes(20, 40));

            Act.BreakOnSpikes(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void ElectroHitFiresWhenLiveElectroSpikesBreakTheCandy()
        {
            (GameScene scene, CandyContext candy) = Rig("electroHit", s => s.ElectroSpikes(20, 40));

            Act.BreakOnSpikes(scene, candy);

            AssertPlaying(scene);
        }

        [Fact]
        public void SpikeHitAndElectroHitAreDistinctEvents()
        {
            (GameScene scene, CandyContext candy) = Rig("spikeHit", s => s.ElectroSpikes(20, 40));

            Act.BreakOnSpikes(scene, candy);

            Assert.Equal(TutorialPromptState.Armed, Prompt(scene).State);
        }

        [Fact]
        public void BouncerHitFiresWhenTheCandyBouncesOffIt()
        {
            (GameScene scene, CandyContext candy) = Rig("bouncerHit", s => s.Bouncer(160, 300));
            Interaction.Drop(candy);

            Assert.True(
                Interaction.StepUntil(scene, () => Prompt(scene).State != TutorialPromptState.Armed),
                "the falling candy never hit the bouncer");

            AssertPlaying(scene);
        }

        [Fact]
        public void ARepeatingCollisionStartsItsPromptOnlyOnce()
        {
            (GameScene scene, CandyContext candy) = Rig("bouncerHit", s => s.Bouncer(160, 300));
            Interaction.Drop(candy);
            TutorialPrompt prompt = Prompt(scene);
            Assert.True(
                Interaction.StepUntil(scene, () => prompt.State != TutorialPromptState.Armed),
                "the falling candy never hit the bouncer");

            // The bouncer accepts the same body on any later frame it crosses the strip; a finished
            // prompt must stay finished rather than replaying on the second bounce.
            Assert.True(
                Interaction.StepUntil(scene, () => prompt.State == TutorialPromptState.Done, maxFrames: 2000),
                "the prompt never finished playing");
            HeadlessGame.StepFrames(scene, 240);

            Assert.Equal(TutorialPromptState.Done, prompt.State);
        }

        [Fact]
        public void ScopedEventsCarryTheCausalHalfOfASplitCandy()
        {
            GameScene scene = Scenario.New()
                .SplitCandy(100, 200, 220, 200)
                .OmNom(20, 460)
                .Bubble(20, 40)
                .TutorialText(
                    20,
                    20,
                    attributes:
                    [
                        new XAttribute("showOn", "bubbleCapture"),
                        new XAttribute("subject", "left"),
                    ])
                .TutorialText(
                    20,
                    60,
                    attributes:
                    [
                        new XAttribute("showOn", "bubbleCapture"),
                        new XAttribute("subject", "right"),
                    ])
                .Build();
            CandyBody right = scene.Candy().Lifecycle.Split.Right.Body;
            Interaction.Hover(right);
            Bubble bubble = scene.Bubbles()[0];

            Assert.True(
                Interaction.StepUntil(
                    scene,
                    () => Act.MoveTo(bubble, right.Point.pos),
                    () => right.Bubble == bubble),
                "the bubble never captured the right half");

            IReadOnlyList<TutorialPrompt> prompts = scene.TutorialPrompts();
            Assert.Equal(TutorialPromptState.Armed, prompts[0].State);
            Assert.Equal(TutorialPromptState.Playing, prompts[1].State);
        }

        /// <summary>
        /// Builds a scene holding one prompt armed on <paramref name="showOn"/> and nothing else that
        /// could fire it, so the prompt's state is a direct readout of that one event.
        /// </summary>
        /// <param name="showOn">Authored trigger event.</param>
        /// <param name="level">Adds whatever the interaction under test needs.</param>
        /// <returns>The built scene and its candy.</returns>
        private static (GameScene Scene, CandyContext Candy) Rig(string showOn, Func<Scenario, Scenario> level)
        {
            GameScene scene = level(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .TutorialText(20, 20, attributes: [new XAttribute("showOn", showOn)]))
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            Assert.Equal(TutorialPromptState.Armed, Prompt(scene).State);
            return (scene, candy);
        }

        private static TutorialPrompt Prompt(GameScene scene)
        {
            return Assert.Single(scene.TutorialPrompts());
        }

        private static void AssertPlaying(GameScene scene)
        {
            Assert.Equal(TutorialPromptState.Playing, Prompt(scene).State);
        }
    }
}
