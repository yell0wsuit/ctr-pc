using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests.Interactions
{
    /// <summary>Verifies cuts that race a spider's first update on an automatic rope.</summary>
    public sealed class SpiderCutRaceTests
    {
        [Fact]
        public void CuttingAnAutoRopeBeforeTheSpiderStartsBustsItImmediately()
        {
            GameScene scene = Scenario.New()
                .Candy(160, 120)
                .Grab(160, 60, radius: 100f, spider: true, moveLength: -1f)
                .OmNom(30, 440)
                .Build();
            Grab grab = scene.Grabs()[0];

            HeadlessGame.StepFrames(scene, 1);

            Bungee rope = grab.RopeOf();
            Assert.NotNull(rope);
            Assert.Equal(SpiderRiderState.Arming, grab.Spider.State);

            Vector from = rope.parts[0].pos;
            Vector to = rope.parts[1].pos;
            Vector midpoint = new((from.X + to.X) / 2f, (from.Y + to.Y) / 2f);
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float length = MathF.Sqrt((dx * dx) + (dy * dy));
            Vector swipeStart = new(midpoint.X + (dy * 40f / length), midpoint.Y - (dx * 40f / length));
            Vector swipeEnd = new(midpoint.X - (dy * 40f / length), midpoint.Y + (dx * 40f / length));

            Assert.Equal(1, scene.CutWithRazorOrLine1Line2Immediate(null, swipeStart, swipeEnd, false));

            Assert.Equal(SpiderRiderState.Busted, grab.Spider.State);
            Assert.False(grab.Spider.IsAttached);
            _ = Assert.Single(DescendantImages(scene), image =>
                ReferenceEquals(Application.GetTexture(Resources.Img.ObjSpider), image.texture)
                && image.quadToDraw == 11);

            HeadlessGame.StepFrames(scene, 1);
            Assert.Equal(SpiderRiderState.Busted, grab.Spider.State);
        }

        private static IEnumerable<Image> DescendantImages(BaseElement parent)
        {
            foreach (BaseElement child in parent.GetChilds().Values)
            {
                if (child is Image image)
                {
                    yield return image;
                }

                foreach (Image descendant in DescendantImages(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
