using System;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// The color each magic hat group wears on its band, drawn from the player's seed.
    /// </summary>
    /// <remarks>
    /// Groups 0 and 1 are the two hats the game ships art for, and their colors are baked into the
    /// frames. Everything past them is generated: the band's grayscale mask is tinted at draw time,
    /// so any color is available, and the only real problem is choosing colors a player can tell
    /// apart at a glance on a band this small.
    /// <para>
    /// Candidates are judged in CIELAB with CIEDE2000 rather than by RGB distance, because the
    /// usable range is squeezed on three sides - the navy hat body, the blue-gray brim right above
    /// the band, and the mask shading that darkens every tint - and RGB distance badly misjudges
    /// how close two colors look inside it.
    /// </para>
    /// </remarks>
    internal sealed class SockBandPalette
    {
        /// <summary>How many colors are generated before the palette repeats.</summary>
        /// <remarks>
        /// Past roughly this many, the colors a seed can still reach are ones a player would have to
        /// compare side by side, so a repeat of an obviously different color reads better than a
        /// seventh that nearly matches a hat already on screen.
        /// </remarks>
        internal const int GeneratedCount = 4;

        /// <summary>How many groups wear a distinct color before the palette repeats.</summary>
        internal const int GroupCount = AuthoredCount + GeneratedCount;

        /// <summary>
        /// The palette for this run, built once from the player's seed.
        /// </summary>
        /// <remarks>
        /// Built on first use rather than with the rest of the static state: generating it reads
        /// the seed file and the anchors it measures against, neither of which exists yet while
        /// this type is still initializing.
        /// </remarks>
        internal static SockBandPalette Shared => shared.Value;

        /// <summary>Builds the palette for one seed.</summary>
        /// <param name="seed">The player's hat seed.</param>
        internal SockBandPalette(ulong seed)
        {
            colors = Generate(seed);
        }

        /// <summary>The band color for a hat group.</summary>
        /// <param name="group">Teleport group the hat belongs to.</param>
        /// <returns>The tint that group's band wears.</returns>
        internal RGBAColor ColorForGroup(int group)
        {
            return group < AuthoredCount
                ? colors[Math.Max(group, 0)]
                : colors[AuthoredCount + ((group - AuthoredCount) % GeneratedCount)];
        }

        /// <summary>
        /// How a tint reads once the band's mask shading is applied to it.
        /// </summary>
        /// <remarks>
        /// The mask is grayscale, so the renderer multiplies it by the tint and every band comes out
        /// darker than the color asked for. Separation has to be measured on what reaches the
        /// screen, not on the tint, or a set of colors that looks well spread on paper collapses
        /// once it is drawn.
        /// </remarks>
        /// <param name="tint">The color the band is tinted with.</param>
        /// <returns>The color the band appears in.</returns>
        internal static RGBAColor BandTone(RGBAColor tint)
        {
            return RGBAColor.MakeRGBA(
                tint.RedColor * MaskShading,
                tint.GreenColor * MaskShading,
                tint.BlueColor * MaskShading,
                1f);
        }

        /// <summary>Colors the shipped art already bakes into the group 0 and group 1 frames.</summary>
        private static readonly RGBAColor[] Authored =
        [
            RGBAColor.MakeRGBA(255f / 255f, 48f / 255f, 20f / 255f, 1f),
            RGBAColor.MakeRGBA(135f / 255f, 255f / 255f, 0f / 255f, 1f),
        ];

        /// <summary>How many groups the shipped art already bakes a color into.</summary>
        internal const int AuthoredCount = 2;

        /// <summary>
        /// Mean of the mask's gray levels across both band patterns, which is how much of a tint
        /// survives to the screen.
        /// </summary>
        private const float MaskShading = 0.763f;

        /// <summary>The hat body a band sits inside; a band the same color as it would vanish.</summary>
        private static readonly RGBAColor HatBody = RGBAColor.MakeRGBA(23f / 255f, 35f / 255f, 49f / 255f, 1f);

        /// <summary>The brim shadow directly above the band, which rules out most blues.</summary>
        private static readonly RGBAColor HatBrim = RGBAColor.MakeRGBA(90f / 255f, 110f / 255f, 149f / 255f, 1f);

        /// <summary>How far a band must sit from the hat body to read as a band at all.</summary>
        private const double BodyFloor = 30.0;

        /// <summary>How far a band must sit from the brim above it.</summary>
        private const double BrimFloor = 24.0;

        /// <summary>Candidates drawn per slot; the one that sits farthest from its neighbors wins.</summary>
        private const int CandidatesPerSlot = 48;

        /// <summary>Whole palettes built before the most evenly spread one is kept.</summary>
        private const int PaletteAttempts = 12;

        /// <summary>Passes that try to reseat each generated color once the palette exists.</summary>
        private const int ImprovementRounds = 2;

        private static RGBAColor[] Generate(ulong seed)
        {
            SeededRandom random = new(seed);

            RGBAColor[] best = null;
            double bestSpread = double.NegativeInfinity;

            for (int attempt = 0; attempt < PaletteAttempts; attempt++)
            {
                RGBAColor[] candidate = BuildOne(random);
                Improve(candidate, random);

                double spread = Spread(candidate);
                if (spread > bestSpread)
                {
                    bestSpread = spread;
                    best = candidate;
                }
            }

            return best;
        }

        private static RGBAColor[] BuildOne(SeededRandom random)
        {
            RGBAColor[] palette = new RGBAColor[GroupCount];
            Authored.CopyTo(palette, 0);

            for (int slot = AuthoredCount; slot < GroupCount; slot++)
            {
                palette[slot] = PickFarthest(palette, slot, random);
            }

            return palette;
        }

        /// <summary>
        /// Reseats each generated color in turn, keeping a replacement only when it spreads the
        /// whole palette wider. A color chosen early can box in the ones after it, and this is what
        /// lets the palette recover from that.
        /// </summary>
        private static void Improve(RGBAColor[] palette, SeededRandom random)
        {
            for (int round = 0; round < ImprovementRounds; round++)
            {
                for (int slot = AuthoredCount; slot < GroupCount; slot++)
                {
                    RGBAColor previous = palette[slot];
                    double before = Spread(palette);

                    palette[slot] = PickFarthest(palette, slot, random);
                    if (Spread(palette) <= before)
                    {
                        palette[slot] = previous;
                    }
                }
            }
        }

        /// <summary>
        /// Draws candidates for one slot and keeps whichever sits farthest from every other color
        /// in the palette, ignoring the slot being filled.
        /// </summary>
        private static RGBAColor PickFarthest(RGBAColor[] palette, int slot, SeededRandom random)
        {
            RGBAColor best = default;
            double bestScore = double.NegativeInfinity;
            RGBAColor fallback = default;
            double bestClearance = double.NegativeInfinity;

            for (int i = 0; i < CandidatesPerSlot; i++)
            {
                RGBAColor tint = DrawTint(random);
                LabColor tone = PerceptualColor.ToCieLab(BandTone(tint));

                double bodyClearance = PerceptualColor.DeltaE2000(tone, BodyLab);
                double brimClearance = PerceptualColor.DeltaE2000(tone, BrimLab);
                double clearance = Math.Min(bodyClearance - BodyFloor, brimClearance - BrimFloor);
                if (clearance > bestClearance)
                {
                    bestClearance = clearance;
                    fallback = tint;
                }

                if (bodyClearance < BodyFloor || brimClearance < BrimFloor)
                {
                    continue;
                }

                double score = NearestNeighbor(palette, slot, tone);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = tint;
                }
            }

            // Nothing cleared the hat's own colors this time, so take whatever came closest to.
            return bestScore == double.NegativeInfinity ? fallback : best;
        }

        /// <summary>How far a tone sits from the nearest other color already in the palette.</summary>
        private static double NearestNeighbor(RGBAColor[] palette, int slot, LabColor tone)
        {
            double nearest = double.PositiveInfinity;
            for (int other = 0; other < palette.Length; other++)
            {
                if (other == slot || (other > slot && palette[other].AlphaChannel == 0f))
                {
                    continue;
                }

                double distance = PerceptualColor.DeltaE2000(
                    tone,
                    PerceptualColor.ToCieLab(BandTone(palette[other])));
                nearest = Math.Min(nearest, distance);
            }

            return nearest;
        }

        /// <summary>The closest any two colors in the palette come to each other.</summary>
        private static double Spread(RGBAColor[] palette)
        {
            double worst = double.PositiveInfinity;
            for (int a = 0; a < palette.Length; a++)
            {
                if (palette[a].AlphaChannel == 0f)
                {
                    continue;
                }

                for (int b = a + 1; b < palette.Length; b++)
                {
                    if (palette[b].AlphaChannel == 0f)
                    {
                        continue;
                    }

                    worst = Math.Min(
                        worst,
                        PerceptualColor.DeltaE2000(
                            PerceptualColor.ToCieLab(BandTone(palette[a])),
                            PerceptualColor.ToCieLab(BandTone(palette[b]))));
                }
            }

            return worst;
        }

        /// <summary>
        /// Draws one candidate tint: any hue, but kept bright and saturated, since the mask shading
        /// darkens whatever comes out and a dim tint lands on top of the navy hat body.
        /// </summary>
        private static RGBAColor DrawTint(SeededRandom random)
        {
            return FromHsv(
                random.NextDouble(0.0, 360.0),
                random.NextDouble(0.55, 1.0),
                random.NextDouble(0.60, 1.0));
        }

        private static RGBAColor FromHsv(double hue, double saturation, double value)
        {
            double chroma = value * saturation;
            double sector = hue / 60.0;
            double second = chroma * (1.0 - Math.Abs((sector % 2.0) - 1.0));
            double offset = value - chroma;

            (double red, double green, double blue) = (int)sector switch
            {
                0 => (chroma, second, 0.0),
                1 => (second, chroma, 0.0),
                2 => (0.0, chroma, second),
                3 => (0.0, second, chroma),
                4 => (second, 0.0, chroma),
                _ => (chroma, 0.0, second),
            };

            return RGBAColor.MakeRGBA(
                (float)(red + offset),
                (float)(green + offset),
                (float)(blue + offset),
                1f);
        }

        private static readonly LabColor BodyLab = PerceptualColor.ToCieLab(HatBody);

        private static readonly LabColor BrimLab = PerceptualColor.ToCieLab(HatBrim);

        private static readonly Lazy<SockBandPalette> shared =
            new(() => new SockBandPalette(SockBandSeed.Read(PlatformServices.Preferences)));

        private readonly RGBAColor[] colors;
    }
}
