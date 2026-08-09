using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>
    /// Pins mechanical hand state transitions: which fields each release path writes, when a
    /// releasing hand settles, and how rotation state is scoped to a single hold.
    /// </summary>
    public sealed class MechanicalHandStateTests
    {
        [Fact]
        public void ARotatableSegmentActuallyRotates()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = scene.Hands()[0];
            _ = candy;

            Act.RotateSegment(scene, hand);

            Assert.Same(hand.SegmentAtIndex(0), hand.rotatingSegment);
            Assert.NotEqual(0f, hand.SegmentAtIndex(0).RotationDelta());
        }

        [Fact]
        public void TappingTheClawReleasesTheHeldCandy()
        {
            (GameScene scene, CandyContext candy) = SoloRig();
            MechanicalHand hand = Act.GrabWithHand(scene, candy);

            Act.TapClaw(scene, hand);

            Assert.Null(candy.Lifecycle.Attachments.Hand);
        }

        /// <summary>One rotatable hand, parked out of reach; the candy hovers where it loads.</summary>
        private static (GameScene Scene, CandyContext Candy) SoloRig(Func<Scenario, Scenario> extra = null)
        {
            return Rig(extra, handCount: 1);
        }

        /// <summary>Two rotatable hands, both parked out of reach on opposite sides.</summary>
        private static (GameScene Scene, CandyContext Candy) DuoRig(Func<Scenario, Scenario> extra = null)
        {
            return Rig(extra, handCount: 2);
        }

        private static (GameScene Scene, CandyContext Candy) Rig(Func<Scenario, Scenario> extra, int handCount)
        {
            Scenario scenario = Scenario.New()
                .Candy(160, 200)
                .OmNom(20, 460)
                .Hand(20, 40, segmentLength: 20, segmentAngle: 90f, rotatable: true);
            if (handCount > 1)
            {
                scenario = scenario.Hand(300, 40, segmentLength: 20, segmentAngle: 90f, rotatable: true);
            }

            GameScene scene = (extra?.Invoke(scenario) ?? scenario).Build();
            CandyContext candy = scene.Candy();
            Interaction.Hover(candy);
            return (scene, candy);
        }
    }
}
