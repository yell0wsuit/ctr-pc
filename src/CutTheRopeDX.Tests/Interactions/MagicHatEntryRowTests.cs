using System;

using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Interaction matrix, "Magic hat entry" row: a hat teleports the candy, so it strips what
    /// cannot survive a teleport (ropes, hand, mouse, ants) and carries the rest through - the
    /// rocket rides along hidden for the transit, and bubble and snail stay attached.
    /// </summary>
    public sealed class MagicHatEntryRowTests
    {
        [Fact]
        public void HatEntryReleasesItsRopes()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rope(160, 120, length: 40));
            Assert.Equal(1, scene.AttachedRopeCount(candy));

            Act.EnterHat(scene, candy);

            Assert.Equal(0, scene.AttachedRopeCount(candy));
        }

        [Fact]
        public void HatEntryDetachesTheHandHoldingIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Hand(160, 120, segmentLength: 20, segmentAngle: 90f));
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.EnterHat(scene, candy);

            Assert.Null(candy.Lifecycle.Attachments.Hand);
            Assert.NotEqual(MechanicalHandState.HoldingCandy, hand.State);
        }

        [Fact]
        public void HatEntryCarriesTheRocketThroughHiddenForTheTransit()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Rocket(160, 200, impulse: 0f));
            Rocket rocket = Act.BindRocket(scene, candy);

            Act.EnterHat(scene, candy);

            Assert.True(candy.Lifecycle.Attachments.HasActiveRocket);
            Assert.Same(rocket, candy.Lifecycle.Attachments.Rocket);
            Assert.False(rocket.visible);
        }

        [Fact]
        public void HatEntryCarriesTheBubbleThrough()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Bubble(160, 200));
            Bubble bubble = Act.CaptureInBubble(scene, candy);

            Act.EnterHat(scene, candy);

            Assert.Same(bubble, candy.WholeBody.Bubble);
        }

        [Fact]
        public void HatEntryCarriesTheSnailThrough()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Snail(160, 200));
            _ = Act.RideSnail(scene, candy);

            Act.EnterHat(scene, candy);

            Assert.Equal(1, scene.SnailCount(candy));
        }

        [Fact]
        public void HatEntryTakesTheCandyOffTheAnts()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Ants(120, 200, path: "80,0"));
            Act.CarryByAnts(scene, candy);

            Act.EnterHat(scene, candy);

            Assert.Null(candy.Lifecycle.Attachments.AntSegment);
        }

        [Fact]
        public void HatEntryMakesTheMouseDropIt()
        {
            (GameScene scene, CandyContext candy) = Rig(s => s.Mouse(160, 200));
            _ = Act.CarryByMouse(scene, candy);

            Act.EnterHat(scene, candy);

            Assert.False(scene.MouseCarries(candy));
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> attachment)
        {
            // Two hats of one group: a hat throws to its partner, so a lone hat swallows nothing.
            // Both park away from the candy; Act.EnterHat brings the first one to it.
            GameScene scene = attachment(
                Scenario.New()
                    .Candy(160, 200)
                    .OmNom(20, 460)
                    .Hat(20, 40, group: 1)
                    .Hat(300, 40, group: 1))
                .Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
