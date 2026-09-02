using System.Linq;

using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;
using CutTheRopeDX.GameMain;
using CutTheRopeDX.Tests.Interactions;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the colored band a magic hat wears once its group runs past the two the game
    /// ships art for.
    /// </summary>
    public sealed class MagicHatBandTests
    {
        [Fact]
        public void TheTwoAuthoredGroupsKeepTheirBakedFrames()
        {
            GameScene scene = Scenario.New()
                .Hat(20, 40, group: 0)
                .Hat(300, 40, group: 1)
                .OmNom(20, 460)
                .Build();

            Sock red = scene.Hats()[0];
            Sock lime = scene.Hats()[1];

            Assert.Equal(0, red.quadToDraw);
            Assert.Equal(1, lime.quadToDraw);
            Assert.Null(red.Band);
            Assert.Null(lime.Band);
        }

        [Fact]
        public void AGroupPastTheAuthoredArtWearsAGeneratedBand()
        {
            Sock hat = FirstHatOfGroup(2);

            Assert.NotNull(hat.BandBackdrop);
            Assert.NotNull(hat.Band);
        }

        [Fact]
        public void TheBandIsCarriedByTheHat()
        {
            // The band has to be a child, or it would not follow the hat as it rotates, scales
            // down onto a transporter, or moves along a path.
            Sock hat = FirstHatOfGroup(2);

            Assert.Same(hat, hat.Band.parent);
            Assert.Same(hat, hat.BandBackdrop.parent);
        }

        [Fact]
        public void TheBackdropIsDrawnUnderTheColoredMask()
        {
            // The backdrop paints out the band the base frame already has; drawn the other way
            // round it would hide the color instead.
            Sock hat = FirstHatOfGroup(2);

            Assert.True(DrawOrderOf(hat, hat.BandBackdrop) < DrawOrderOf(hat, hat.Band));
        }

        [Fact]
        public void TheBandIsDrawnUnderTheTeleportFlash()
        {
            // The band belongs to the hat's own art, so the flash that plays when something comes
            // out has to pass over it the way it passes over the rest of the hat.
            Sock hat = FirstHatOfGroup(2);

            Assert.True(DrawOrderOf(hat, hat.Band) < DrawOrderOf(hat, hat.light));
        }

        [Fact]
        public void HatsOfOneGroupShareABandColor()
        {
            GameScene scene = Scenario.New()
                .Hat(20, 40, group: 2)
                .Hat(300, 40, group: 2)
                .OmNom(20, 460)
                .Build();

            Assert.Equal(scene.Hats()[0].Band.color, scene.Hats()[1].Band.color);
        }

        [Fact]
        public void SeparateGroupsWearSeparateBandColors()
        {
            GameScene scene = Scenario.New()
                .Hat(20, 40, group: 2)
                .Hat(300, 40, group: 3)
                .OmNom(20, 460)
                .Build();

            Assert.NotEqual(scene.Hats()[0].Band.color, scene.Hats()[1].Band.color);
        }

        [Fact]
        public void EachGroupUsesTheMaskAuthoredForItsBaseFrame()
        {
            // The two masks were drawn over different base frames, so a group takes the pair that
            // belongs to the frame it draws.
            Sock even = FirstHatOfGroup(2);
            Sock odd = FirstHatOfGroup(3);

            Assert.Equal(0, even.BandBackdrop.quadToDraw);
            Assert.Equal(1, even.Band.quadToDraw);
            Assert.Equal(2, odd.BandBackdrop.quadToDraw);
            Assert.Equal(3, odd.Band.quadToDraw);
        }

        [Fact]
        public void ABandIsOpaque()
        {
            // The band replaces art that is already there, so anything less would let the
            // authored color underneath show through.
            Sock hat = FirstHatOfGroup(2);

            Assert.Equal(1f, hat.Band.color.AlphaChannel);
            Assert.Equal(1f, hat.BandBackdrop.color.AlphaChannel);
        }

        [Fact]
        public void TheBandDrawsWithItsGeneratedColor()
        {
            Sock hat = FirstHatOfGroup(2);
            RecordingRenderBackend renderer = new();
            PlatformServices.Render = renderer;

            try
            {
                hat.Band.Draw();

                Assert.Equal(hat.Band.color.ToColor(), renderer.LastTexturedDrawColor);
            }
            finally
            {
                PlatformServices.Render = new ThrowingRenderBackend();
            }
        }

        [Fact]
        public void TheBandStartsFromTheSameCornerAsTheHat()
        {
            // Both atlases place their frames inside the same source drawing, so the layers line up
            // only while they share an origin and each applies its own frame offset from there.
            Sock hat = FirstHatOfGroup(2);

            BaseElement.CalculateTopLeft(hat);
            BaseElement.CalculateTopLeft(hat.Band);
            BaseElement.CalculateTopLeft(hat.BandBackdrop);

            Assert.Equal(hat.drawX, hat.Band.drawX);
            Assert.Equal(hat.drawY, hat.Band.drawY);
            Assert.Equal(hat.drawX, hat.BandBackdrop.drawX);
            Assert.Equal(hat.drawY, hat.BandBackdrop.drawY);
        }

        /// <summary>Where a child sits in the order its parent draws children in.</summary>
        private static int DrawOrderOf(Sock hat, BaseElement child)
        {
            return hat.GetChilds().Single(entry => ReferenceEquals(entry.Value, child)).Key;
        }

        private static Sock FirstHatOfGroup(int group)
        {
            GameScene scene = Scenario.New()
                .Hat(20, 40, group: group)
                .Hat(300, 40, group: group)
                .OmNom(20, 460)
                .Build();

            return scene.Hats()[0];
        }
    }
}
