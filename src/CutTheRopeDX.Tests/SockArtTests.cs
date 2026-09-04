using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers which art a magic hat draws, which differs by group and by season.
    /// </summary>
    /// <remarks>
    /// The loader and the level resource scanner both have to agree on this. They read it from
    /// here rather than each deciding for itself, because a level that loads one texture and draws
    /// another stalls the game thread reading the missing one mid-play.
    /// </remarks>
    public class SockArtTests
    {
        [Fact]
        public void OutsideChristmasEveryGroupWearsTheMagicHat()
        {
            Assert.Equal(Resources.Img.ObjHat, SockArt.TextureFor(0, isXmas: false));
            Assert.Equal(Resources.Img.ObjHat, SockArt.TextureFor(1, isXmas: false));
            Assert.Equal(Resources.Img.ObjHat, SockArt.TextureFor(2, isXmas: false));
        }

        [Fact]
        public void AtChristmasTheAuthoredGroupsWearSocks()
        {
            Assert.Equal(Resources.Img.ObjSock, SockArt.TextureFor(0, isXmas: true));
            Assert.Equal(Resources.Img.ObjSock, SockArt.TextureFor(1, isXmas: true));
        }

        [Fact]
        public void AtChristmasAGroupPastTheSocksFallsBackToTheMagicHat()
        {
            // The Christmas art draws two socks and no more, so a third group has nothing to wear.
            // The magic hat does, because its band can be generated for any group.
            Assert.Equal(Resources.Img.ObjHat, SockArt.TextureFor(2, isXmas: true));
            Assert.Equal(Resources.Img.ObjHat, SockArt.TextureFor(7, isXmas: true));
        }

        [Fact]
        public void OnlyGroupsPastTheAuthoredArtWearAGeneratedBand()
        {
            Assert.False(SockArt.WearsGeneratedBand(0));
            Assert.False(SockArt.WearsGeneratedBand(1));
            Assert.True(SockArt.WearsGeneratedBand(2));
            Assert.True(SockArt.WearsGeneratedBand(3));
        }

        [Fact]
        public void AMalformedGroupIsTreatedAsTheFirstOne()
        {
            // Level XML is data, and a negative group would otherwise index off the front of the
            // art. Drawing the first hat is wrong quietly; a crash is wrong loudly.
            Assert.Equal(0, SockArt.NormalizeGroup(-3));
            Assert.False(SockArt.WearsGeneratedBand(-3));
            Assert.Equal(Resources.Img.ObjSock, SockArt.TextureFor(-3, isXmas: true));
        }

        [Fact]
        public void ABandedHatDrawsTheBaseFrameItsMaskWasAuthoredOver()
        {
            Assert.Equal(0, SockArt.PatternFor(2));
            Assert.Equal(1, SockArt.PatternFor(3));
            Assert.Equal(0, SockArt.PatternFor(4));
        }
    }
}
