using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>Spider capture must retire the exact candy and all of its live attachment owners.</summary>
    public sealed class SpiderRemovalInvariantTests
    {
        [Fact]
        public void SpiderCaptureClearsBubbleOwnership()
        {
            AssertSpiderRetires(s => s.Bubble(160, 200), (scene, candy) => _ = Act.CaptureInBubble(scene, candy));
        }

        [Fact]
        public void SpiderCaptureClearsRocketOwnership()
        {
            AssertSpiderRetires(s => s.Rocket(160, 200, impulse: 0f), (scene, candy) => _ = Act.BindRocket(scene, candy));
        }

        [Fact]
        public void SpiderCaptureClearsSnailOwnership()
        {
            AssertSpiderRetires(s => s.Snail(160, 200), (scene, candy) => _ = Act.RideSnail(scene, candy));
        }

        [Fact]
        public void SpiderCaptureClearsAntOwnership()
        {
            // A pre-attached fixed rope pulls the candy off the narrow ant lane before pickup. Use
            // the real radius-hook path instead: let the ants acquire it first, then move the
            // spider hook into range so the reachable ant-plus-spider-rope composition is formed.
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(20, 20, radius: 100f, spider: true, moveLength: -1f)
                .Ants(120, 200, path: "80,0")
                .OmNom(20, 460)
                .Build();
            CandyContext candy = scene.Candy();
            Grab spiderGrab = scene.Grabs()[0];
            Interaction.Hover(candy);
            Act.CarryByAnts(scene, candy);
            Act.MoveTo(spiderGrab, candy.WholeBody.Point.pos);
            Assert.True(
                Interaction.StepUntil(scene, () => spiderGrab.Rope != null),
                "the radius spider hook never attached to the ant-carried candy");

            scene.SpiderWon(spiderGrab);

            Assert.Equal(SpiderRiderState.Won, spiderGrab.Spider.State);
            scene.AssertNoLiveAttachments(candy);
        }

        [Fact]
        public void SpiderCaptureClearsHandOwnership()
        {
            AssertSpiderRetires(
                s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f),
                (scene, candy) => _ = Act.GrabWithHand(scene, candy));
        }

        [Fact]
        public void SpiderCaptureClearsMouseOwnership()
        {
            // Mouse pickup always severs every rope on the candy, including the spider's, so a
            // same-body mouse-plus-spider capture is not reachable through the update loop. The
            // nearest reachable composition proves that retiring the spidered candy leaves the
            // mouse's exact-point ownership of another candy intact.
            GameScene scene = Scenario.New()
                .Candy(80, 200, number: "1")
                .Candy(240, 200, number: "2")
                .Grab(80, 120, length: 100, spider: true, candyNumber: "1")
                .Mouse(240, 200)
                .OmNom(20, 460)
                .Build();
            CandyContext captured = scene.Candies()[0];
            CandyContext mouseCandy = scene.Candies()[1];
            Interaction.Hover(mouseCandy);
            _ = Act.CarryByMouse(scene, mouseCandy);

            scene.SpiderWon(scene.Grabs()[0]);

            scene.AssertNoLiveAttachments(captured);
            Assert.True(scene.MouseCarries(mouseCandy));
        }

        [Fact]
        public void SpiderCaptureDoesNotAlterAnotherCandysAttachments()
        {
            GameScene scene = Scenario.New()
                .Candy(80, 200, number: "1")
                .Candy(240, 200, number: "2")
                .Grab(80, 120, length: 100, spider: true, candyNumber: "1")
                .Rope(240, 120, length: 100, candyNumber: "2")
                .Bubble(240, 200)
                .OmNom(20, 460)
                .Build();
            CandyContext captured = scene.Candies()[0];
            CandyContext other = scene.Candies()[1];
            Interaction.Hover(other);
            Bubble bubble = Act.CaptureInBubble(scene, other);
            Assert.Equal(1, scene.AttachedRopeCount(other));

            scene.SpiderWon(scene.Grabs()[0]);

            scene.AssertNoLiveAttachments(captured);
            Assert.Equal(CandyPresence.Present, other.Lifecycle.Presence);
            Assert.Same(bubble, other.WholeBody.Bubble);
            Assert.Equal(1, scene.AttachedRopeCount(other));
        }

        [Fact]
        public void WinningSpiderSurvivesRopeRetirementWhileOtherSpidersBust()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .Grab(120, 120, length: 100, spider: true)
                .Grab(200, 120, length: 100, spider: true)
                .OmNom(20, 460)
                .Build();
            Grab winner = scene.Grabs()[0];
            Grab other = scene.Grabs()[1];
            HeadlessGame.StepFrames(scene, 1);
            Assert.True(winner.Spider.ShouldBustOnRopeCut);
            Assert.True(other.Spider.ShouldBustOnRopeCut);

            scene.SpiderWon(winner);

            Assert.Equal(SpiderRiderState.Won, winner.Spider.State);
            Assert.Equal(SpiderRiderState.Busted, other.Spider.State);
        }

        [Fact]
        public void SpiderOnAnAlreadyRemovedBodyCreatesNoVictoryOrCrossCandyDamage()
        {
            GameScene scene = Scenario.New()
                .Candy(60, 200, number: "1")
                .Candy(260, 200, number: "2")
                .Rope(60, 120, length: 100, candyNumber: "1")
                .Grab(260, 120, length: 100, spider: true, candyNumber: "2")
                .OmNom(20, 460)
                .Build();
            CandyContext kept = scene.Candies()[0];
            CandyContext removed = scene.Candies()[1];
            Grab staleSpider = scene.Grabs()[1];
            Interaction.Hover(removed);
            Act.LoseOffScreen(scene, removed);
            int victoryEffects = scene.SpiderVictoryEffectCount();
            Assert.Equal(1, scene.AttachedRopeCount(kept));

            scene.SpiderWon(staleSpider);

            Assert.Equal(victoryEffects, scene.SpiderVictoryEffectCount());
            Assert.Equal(CandyRemovalReason.OffScreen, removed.Lifecycle.RemovalReason);
            Assert.Equal(CandyPresence.Present, kept.Lifecycle.Presence);
            Assert.Equal(1, scene.AttachedRopeCount(kept));
        }

        private static void AssertSpiderRetires(
            Func<Scenario, Scenario> addAttachment,
            Action<GameScene, CandyContext> attach)
        {
            GameScene scene = addAttachment(
                    Scenario.New()
                        .Candy(160, 200)
                        .Grab(160, 120, length: 100, spider: true)
                        .OmNom(20, 460))
                .Build();
            CandyContext candy = scene.Candy();
            Grab spiderGrab = scene.Grabs()[0];
            Interaction.Hover(candy);
            attach(scene, candy);
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            scene.SpiderWon(spiderGrab);

            Assert.Equal(CandyRemovalReason.Spider, candy.Lifecycle.RemovalReason);
            Assert.Equal(SpiderRiderState.Won, spiderGrab.Spider.State);
            scene.AssertNoLiveAttachments(candy);
        }
    }
}
