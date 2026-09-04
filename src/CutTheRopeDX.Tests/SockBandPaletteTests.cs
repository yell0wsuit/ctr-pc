using CutTheRopeDX.Framework;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the colors a seed paints magic hat bands with.
    /// </summary>
    public class SockBandPaletteTests
    {
        /// <summary>The hat body the band sits inside, sampled from the art.</summary>
        private static readonly RGBAColor HatBody = Bytes(23, 35, 49);

        /// <summary>The brim shadow directly above the band, sampled from the art.</summary>
        private static readonly RGBAColor HatBrim = Bytes(90, 110, 149);

        /// <summary>Seeds the separation guarantees are checked against.</summary>
        public static TheoryData<ulong> Seeds =>
            [1UL, 2UL, 7UL, 42UL, 99UL, 2024UL, 123456789UL, ulong.MaxValue];

        [Fact]
        public void TheSameSeedRepaintsTheSameHats()
        {
            SockBandPalette first = new(2024UL);
            SockBandPalette second = new(2024UL);

            for (int group = 0; group < 10; group++)
            {
                Assert.Equal(first.ColorForGroup(group), second.ColorForGroup(group));
            }
        }

        [Fact]
        public void DifferentSeedsPaintDifferentHats()
        {
            SockBandPalette first = new(1UL);
            SockBandPalette second = new(2UL);

            Assert.NotEqual(first.ColorForGroup(2), second.ColorForGroup(2));
        }

        [Fact]
        public void TheFirstTwoGroupsKeepTheirAuthoredColors()
        {
            // Groups 0 and 1 draw the baked frames, so the palette has to report what that art
            // already shows or it would measure separation against colors nobody sees.
            SockBandPalette palette = new(2024UL);

            AssertClose(Bytes(255, 48, 20), palette.ColorForGroup(0));
            AssertClose(Bytes(135, 255, 0), palette.ColorForGroup(1));
        }

        [Fact]
        public void ColorsWrapOnceTheGeneratedRangeRunsOut()
        {
            SockBandPalette palette = new(2024UL);

            Assert.Equal(
                palette.ColorForGroup(2),
                palette.ColorForGroup(2 + SockBandPalette.GeneratedCount));
        }

        [Fact]
        public void BandsAreFullyOpaque()
        {
            SockBandPalette palette = new(2024UL);

            for (int group = 0; group < 6; group++)
            {
                Assert.Equal(1f, palette.ColorForGroup(group).AlphaChannel);
            }
        }

        [Theory]
        [MemberData(nameof(Seeds))]
        public void EveryHatLooksDifferentFromEveryOther(ulong seed)
        {
            SockBandPalette palette = new(seed);

            for (int a = 0; a < SockBandPalette.GroupCount; a++)
            {
                for (int b = a + 1; b < SockBandPalette.GroupCount; b++)
                {
                    // Measured floor across 200 seeds is 26.6; this leaves a little headroom
                    // without letting a regression that halves the spread slip through.
                    Assert.True(
                        Separation(palette, a, b) >= 24.0,
                        $"groups {a} and {b} are only {Separation(palette, a, b):F1} apart");
                }
            }
        }

        [Theory]
        [MemberData(nameof(Seeds))]
        public void ThirdHatStandsWellClearOfTheTwoAuthoredOnes(ulong seed)
        {
            // Every shipped level uses at most three groups, so this is the case that matters most.
            SockBandPalette palette = new(seed);

            Assert.True(Separation(palette, 2, 0) >= 26.0, $"only {Separation(palette, 2, 0):F1} from red");
            Assert.True(Separation(palette, 2, 1) >= 26.0, $"only {Separation(palette, 2, 1):F1} from lime");
        }

        [Theory]
        [MemberData(nameof(Seeds))]
        public void EveryBandStandsOutAgainstTheHatItself(ulong seed)
        {
            SockBandPalette palette = new(seed);

            for (int group = 2; group < SockBandPalette.GroupCount; group++)
            {
                LabColor band = ToneOf(palette, group);

                Assert.True(
                    PerceptualColor.DeltaE2000(band, PerceptualColor.ToCieLab(HatBody)) >= 28.0,
                    $"group {group} disappears into the hat body");
                Assert.True(
                    PerceptualColor.DeltaE2000(band, PerceptualColor.ToCieLab(HatBrim)) >= 22.0,
                    $"group {group} muddies into the brim above it");
            }
        }

        private static double Separation(SockBandPalette palette, int first, int second)
        {
            return PerceptualColor.DeltaE2000(ToneOf(palette, first), ToneOf(palette, second));
        }

        private static LabColor ToneOf(SockBandPalette palette, int group)
        {
            return PerceptualColor.ToCieLab(SockBandPalette.BandTone(palette.ColorForGroup(group)));
        }

        private static RGBAColor Bytes(int r, int g, int b)
        {
            return RGBAColor.MakeRGBA(r / 255f, g / 255f, b / 255f, 1f);
        }

        private static void AssertClose(RGBAColor expected, RGBAColor actual)
        {
            Assert.Equal(expected.RedColor, actual.RedColor, 3);
            Assert.Equal(expected.GreenColor, actual.GreenColor, 3);
            Assert.Equal(expected.BlueColor, actual.BlueColor, 3);
        }
    }
}
