using System;
using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.GameMain.Tutorials;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests.Tutorials
{
    /// <summary>
    /// The actorless tutorial events - player-operated machinery, the two level outcomes, and
    /// rocket ignition - are pushed from the owner of each transition, never inferred from state
    /// that happens to be true later.
    /// </summary>
    public sealed class TutorialControlEventTests
    {
        [Fact]
        public void PumpFireFiresWhenTheTappedPumpBlows()
        {
            GameScene scene = Rig("pumpFire", s => s.Pump(160, 300));
            Pump pump = scene.Pumps()[0];

            // Scenery keeps the draw position it was loaded with, and the pump's touch test runs off
            // that, so settle its hitbox before aiming at it.
            Act.MoveTo(pump, new Vector(pump.x, pump.y));
            Vector touch = scene.ScreenPositionOf(pump);

            Assert.True(scene.TouchDownXYIndex(touch.X, touch.Y, 0), "the tap on the pump was not handled");
            Assert.True(
                Interaction.StepUntil(scene, () => Prompt(scene).State != TutorialPromptState.Armed),
                "the tapped pump never blew");

            AssertPlaying(scene);
        }

        [Fact]
        public void SteamBurstFiresWhenTheValveIsTurned()
        {
            GameScene scene = Rig("steamBurst", s => s.SteamTube(160, 300));
            SteamTube tube = scene.SteamTubes()[0];
            Vector valve = scene.ScreenPositionOf(new Vector(tube.x, tube.y + (28f * tube.GetHeightScale())));

            Assert.True(scene.TouchDownXYIndex(valve.X, valve.Y, 0), "the tap on the steam valve was not handled");

            AssertPlaying(scene);
        }

        [Fact]
        public void DiscSpinFiresWhenAHandleIsFirstTaken()
        {
            GameScene scene = Rig("discSpin", s => s.Disc(160, 300));
            RotatedCircle disc = scene.Discs()[0];
            Vector handle = scene.ScreenPositionOf(disc.handle2);

            Assert.True(scene.TouchDownXYIndex(handle.X, handle.Y, 0), "the tap on the disc handle was not handled");

            Assert.NotEqual(-1, disc.operating);
            AssertPlaying(scene);
        }

        [Fact]
        public void TimeFreezeAndTimeUnfreezeAreSeparateEvents()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .PauseSwitcher(60, 440)
                .TutorialText(20, 20, attributes: [new XAttribute("showOn", "timeFreeze")])
                .TutorialText(20, 60, attributes: [new XAttribute("showOn", "timeUnfreeze")])
                .Build();

            PressPauseSwitcher(scene);

            Assert.True(scene.IsTimeFrozen());
            Assert.Equal(TutorialPromptState.Playing, scene.TutorialPrompts()[0].State);
            Assert.Equal(TutorialPromptState.Armed, scene.TutorialPrompts()[1].State);

            PressPauseSwitcher(scene);

            Assert.False(scene.IsTimeFrozen());
            Assert.Equal(TutorialPromptState.Playing, scene.TutorialPrompts()[1].State);
        }

        [Fact]
        public void GravityFlipFiresOnEveryToggleOfTheOwningState()
        {
            GameScene scene = Rig("gravityFlip", s => s);

            scene.OnButtonPressed(GameSceneButtonId.GravityToggle);

            Assert.True(scene.gravityState.IsInverted);
            AssertPlaying(scene);
        }

        [Fact]
        public void GameWonFiresOnlyWhenTheWinIsAccepted()
        {
            GameScene scene = Rig("gameWon", s => s);
            CandyContext candy = scene.Candy();

            Act.Eat(scene, candy);
            Assert.True(
                Interaction.StepUntil(scene, () => scene.Outcomes().WonCount > 0),
                "the eaten candy never won the level");

            AssertPlaying(scene);
        }

        [Fact]
        public void GameLostFiresOnlyWhenTheLossIsAccepted()
        {
            GameScene scene = Rig("gameLost", s => s);

            Act.LoseOffScreen(scene, scene.Candy());

            AssertPlaying(scene);
        }

        [Fact]
        public void RocketIgniteFiresWhenABoundRocketReachesFlight()
        {
            GameScene scene = Rig("rocketIgnite", s => s.Rocket(160, 300, impulse: 0f));
            CandyContext candy = scene.Candy();

            IgniteRocket(scene, candy, rocketIndex: 0);

            AssertPlaying(scene);
        }

        [Fact]
        public void RocketIgnitionIsKeyedPerRocketRatherThanSceneWide()
        {
            // Two rockets, and the prompt only accepts the primary candy's. A scene-wide "a rocket
            // is flying" flag would be consumed by the second candy's ignition and never fire for
            // the primary one afterwards.
            GameScene scene = Scenario.New()
                .Candy(100, 200, "first")
                .Candy(220, 200, "second")
                .OmNom(20, 460)
                .Rocket(100, 320, impulse: 0f)
                .Rocket(220, 320, impulse: 0f)
                .TutorialText(
                    20,
                    20,
                    attributes:
                    [
                        new XAttribute("showOn", "rocketIgnite"),
                        new XAttribute("subject", "primary"),
                    ])
                .Build();
            CandyContext primary = scene.Candies()[0];
            CandyContext second = scene.Candies()[1];
            Interaction.Hover(primary);
            Interaction.Hover(second);

            IgniteRocket(scene, second, rocketIndex: 1);
            Assert.Equal(TutorialPromptState.Armed, Prompt(scene).State);

            IgniteRocket(scene, primary, rocketIndex: 0);

            AssertPlaying(scene);
        }

        /// <summary>
        /// Binds one rocket and flies it. The candy has to be let go first: the reel-in that ends in
        /// flight only completes once the bound rocket and its candy are actually apart.
        /// </summary>
        /// <param name="scene">Scene under test.</param>
        /// <param name="candy">Candy to bind the rocket to.</param>
        /// <param name="rocketIndex">Index of the rocket in the scene.</param>
        private static void IgniteRocket(GameScene scene, CandyContext candy, int rocketIndex)
        {
            Rocket rocket = Act.BindRocket(scene, candy, rocketIndex);
            Interaction.Drop(candy);
            Assert.True(
                Interaction.StepUntil(scene, () => rocket.state == Rocket.STATE_ROCKET_FLY),
                "the bound rocket never reached flight");

            // Ignition is diffed on the director's own update, one frame behind the state change.
            HeadlessGame.StepFrames(scene, 1);
        }

        private static void PressPauseSwitcher(GameScene scene)
        {
            Vector button = scene.ScreenPositionOf(scene.PauseSwitchers()[0]);
            _ = scene.TouchDownXYIndex(button.X, button.Y, 0);
            _ = scene.TouchUpXYIndex(button.X, button.Y, 0);
        }

        /// <summary>
        /// Builds a scene holding one prompt armed on <paramref name="showOn"/>, so the prompt's
        /// state reads back whether that one event was pushed.
        /// </summary>
        /// <param name="showOn">Authored trigger event.</param>
        /// <param name="level">Adds whatever the control under test needs.</param>
        /// <returns>The built scene.</returns>
        private static GameScene Rig(string showOn, Func<Scenario, Scenario> level)
        {
            GameScene scene = level(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .TutorialText(20, 20, attributes: [new XAttribute("showOn", showOn)]))
                .Build();
            Interaction.Hover(scene.Candy());
            Assert.Equal(TutorialPromptState.Armed, Prompt(scene).State);
            return scene;
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
