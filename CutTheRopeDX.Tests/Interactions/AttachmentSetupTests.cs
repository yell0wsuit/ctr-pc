using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Every attachment the matrix talks about, formed by the real update loop. The matrix rows
    /// build on these, so a break here explains breakage everywhere else.
    /// </summary>
    public sealed class AttachmentSetupTests
    {
        [Fact]
        public void RocketBinds_WhenTheCandyTouchesAnIdleRocket()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Rocket(160, 200)
                .Build();

            CandyContext candy = scene.Candy();
            Rocket rocket = scene.Rockets()[0];
            Interaction.Hover(candy);
            Interaction.PlaceCandyAt(candy, Interaction.At(rocket.x, rocket.y));

            Assert.True(Interaction.StepUntil(scene, () => candy.HasActiveRocket));
        }

        [Fact]
        public void BubbleCaptures_WhenTheCandyEntersItsRadius()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Bubble(160, 200)
                .Build();

            CandyContext candy = scene.Candy();
            Bubble bubble = scene.Bubbles()[0];
            Interaction.Hover(candy);
            Interaction.PlaceCandyAt(candy, Interaction.At(bubble.x, bubble.y));

            Assert.True(Interaction.StepUntil(scene, () => candy.bubble != null));
        }

        [Fact]
        public void SnailAttaches_WhenItReachesTheCandy()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Snail(160, 200)
                .Build();

            CandyContext candy = scene.Candy();
            Snail snail = scene.Snails()[0];
            Interaction.Hover(candy);
            Interaction.PlaceCandyAt(candy, Interaction.At(snail.x, snail.y));

            Assert.True(Interaction.StepUntil(scene, () => scene.SnailCount(candy) == 1));
        }

        [Fact]
        public void HandGrabs_WhenTheCandySitsAtTheClaw()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Hand(160, 120, segmentLength: 20, segmentAngle: 90f)
                .Build();

            CandyContext candy = scene.Candy();
            MechanicalHand hand = scene.Hands()[0];
            Interaction.Hover(candy);
            Interaction.PlaceCandyAt(candy, hand.ClawPosition());

            Assert.True(Interaction.StepUntil(scene, () => candy.capturingHand == hand));
        }

        [Fact]
        public void MouseGrabs_OnceItHasPoppedOutOfItsHole()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Mouse(160, 200)
                .Build();

            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            Interaction.PlaceCandyAt(candy, Interaction.At(Scenario.New().WorldX(160), Scenario.WorldY(200)));

            Assert.True(Interaction.StepUntil(scene, () => candy.carriedByMouse));
        }

        [Fact]
        public void AntsCarry_WhenTheCandyLandsOnASegment()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Ants(120, 200, path: "80,0")
                .Build();

            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);

            Assert.True(Interaction.StepUntil(scene, () => candy.antSegment != null));
        }

        [Fact]
        public void LanternCaptures_WhenTheCandyDriftsIntoIt()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(160, 440)
                .Lantern(160, 200)
                .Build();

            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);

            Assert.True(Interaction.StepUntil(scene, () => candy.inLantern));
        }

        [Fact]
        public void MagicHatSwallowsTheCandy_WhenItFallsThroughTheMouth()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(160, 440)
                .Hat(160, 200, group: 1)
                .Hat(60, 300, group: 1)
                .Build();

            CandyContext candy = scene.Candy();

            Assert.True(Interaction.StepUntil(scene, () => candy.Lifecycle.Transport?.Sock != null));
        }

        [Fact]
        public void BambooTubeSwallowsTheCandy_WhenItFallsIn()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(160, 440)
                // The mouth faces up, so the falling candy moves *into* it - a tube only catches a
                // candy travelling toward the hole, never one leaving it.
                .BambooTube(160, 220, TubeMouth.CatchesFalling)
                .Build();

            CandyContext candy = scene.Candy();

            Assert.True(Interaction.StepUntil(scene, () => candy.Lifecycle.Transport?.BambooTube != null));
        }

        [Fact]
        public void OmNomEatsTheCandy_WhenItFallsIntoAnOpenMouth()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(160, 300)
                .Build();

            CandyContext candy = scene.Candy();

            Assert.True(Interaction.StepUntil(scene, () => candy.noCandy));
        }

        [Fact]
        public void SpikesBreakTheCandy_OnContact()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 140)
                .OmNom(30, 440)
                .Spikes(160, 260)
                .Build();

            // A hazard break of the *primary* candy goes down the singleton path, which sets the
            // scene's own gone-flag rather than the context's (BreakCandyFromHazard, index 0).
            Assert.True(Interaction.StepUntil(scene, scene.PrimaryCandyGone));
        }
    }
}
