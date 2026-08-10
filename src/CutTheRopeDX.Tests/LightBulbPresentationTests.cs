using System.Linq;
using System.Reflection;

using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class LightBulbPresentationTests
    {
        [Fact]
        public void UpdateReadsVisibilityFromTheOwningLifecycle()
        {
            GameScene scene = Scenario.New()
                .LightBulb(240, 200)
                .OmNom(20, 460)
                .Build();
            CandyContext context = scene.Candies().Single(candy => candy.emitsLight);

            Assert.True(context.Lifecycle.TryRemove(CandyRemovalReason.OffScreen, out _));

            context.LightBulb.Update(0.016f);

            Assert.False(context.LightBulb.visible);
        }

        [Fact]
        public void PrepareToDrawReadsSameFrameBubbleTransitions()
        {
            GameScene scene = Scenario.New()
                .LightBulb(240, 200)
                .Bubble(240, 200)
                .OmNom(20, 460)
                .Build();
            CandyContext context = scene.Candies().Single(candy => candy.emitsLight);
            Interaction.Hover(context);

            _ = Act.CaptureInBubble(scene, context);

            context.LightBulb.PrepareToDraw();

            Assert.True(BubbleAnimation(context.LightBulb).visible);
            Assert.False(GhostBubbleAnimation(context.LightBulb).visible);

            context.WholeBody.BubbleHasGhost = true;

            context.LightBulb.PrepareToDraw();

            Assert.False(BubbleAnimation(context.LightBulb).visible);
            Assert.True(GhostBubbleAnimation(context.LightBulb).visible);

            context.WholeBody.Bubble = null;
            context.WholeBody.BubbleHasGhost = false;

            context.LightBulb.PrepareToDraw();

            Assert.False(BubbleAnimation(context.LightBulb).visible);
            Assert.False(GhostBubbleAnimation(context.LightBulb).visible);
        }

        [Fact]
        public void PrepareToDrawReadsTransportVisibilityFromTheOwningLifecycle()
        {
            GameScene scene = Scenario.New()
                .LightBulb(160, 200)
                .Hat(20, 40, group: 1)
                .Hat(300, 40, group: 1)
                .OmNom(20, 460)
                .Build();
            CandyContext context = scene.Candies().Single(candy => candy.emitsLight);
            Interaction.Hover(context);

            Act.EnterHat(scene, context);
            context.LightBulb.PrepareToDraw();

            Assert.Equal(CandyPresence.Hidden, context.Lifecycle.Presence);
            Assert.False(context.LightBulb.visible);

            Assert.True(
                Interaction.StepUntil(scene, () => context.Lifecycle.Presence == CandyPresence.Present),
                "the hat never released the light bulb");
            context.LightBulb.PrepareToDraw();

            Assert.True(context.LightBulb.visible);
        }

        private static Animation BubbleAnimation(LightBulb bulb)
        {
            return (Animation)typeof(LightBulb)
                .GetField("bubbleAnimation", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(bulb);
        }

        private static CandyInGhostBubbleAnimation GhostBubbleAnimation(LightBulb bulb)
        {
            return (CandyInGhostBubbleAnimation)typeof(LightBulb)
                .GetField("ghostBubbleAnimation", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(bulb);
        }
    }
}
