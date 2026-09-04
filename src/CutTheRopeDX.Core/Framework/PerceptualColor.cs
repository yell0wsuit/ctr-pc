using System;

namespace CutTheRopeDX.Framework
{
    /// <summary>A color in a Lab-style opponent space: lightness plus two chroma axes.</summary>
    /// <param name="L">Lightness.</param>
    /// <param name="A">Green-red axis.</param>
    /// <param name="B">Blue-yellow axis.</param>
    internal readonly record struct LabColor(double L, double A, double B);

    /// <summary>
    /// Perceptual color math: how far apart two colors look, rather than how far apart their
    /// channel values are. sRGB distance is useless for this - a pair of blues separated by the
    /// same channel deltas as a pair of greens looks far closer.
    /// </summary>
    internal static class PerceptualColor
    {
        /// <summary>Converts a color to CIELAB under a D65 white point.</summary>
        /// <param name="color">Straight sRGB color; its alpha is ignored.</param>
        /// <returns>The color in CIELAB.</returns>
        internal static LabColor ToCieLab(RGBAColor color)
        {
            double r = Linearize(color.RedColor);
            double g = Linearize(color.GreenColor);
            double b = Linearize(color.BlueColor);

            // sRGB primaries to CIEXYZ, then normalized by the D65 white point.
            double x = ((0.4124 * r) + (0.3576 * g) + (0.1805 * b)) / 0.95047;
            double y = (0.2126 * r) + (0.7152 * g) + (0.0722 * b);
            double z = ((0.0193 * r) + (0.1192 * g) + (0.9505 * b)) / 1.08883;

            double fx = LabCurve(x);
            double fy = LabCurve(y);
            double fz = LabCurve(z);

            return new LabColor((116.0 * fy) - 16.0, 500.0 * (fx - fy), 200.0 * (fy - fz));
        }

        /// <summary>Converts a color to OKLab.</summary>
        /// <param name="color">Straight sRGB color; its alpha is ignored.</param>
        /// <returns>The color in OKLab.</returns>
        internal static LabColor ToOkLab(RGBAColor color)
        {
            double r = Linearize(color.RedColor);
            double g = Linearize(color.GreenColor);
            double b = Linearize(color.BlueColor);

            double l = Math.Cbrt((0.4122214708 * r) + (0.5363325363 * g) + (0.0514459929 * b));
            double m = Math.Cbrt((0.2119034982 * r) + (0.6806995451 * g) + (0.1073969566 * b));
            double s = Math.Cbrt((0.0883024619 * r) + (0.2817188376 * g) + (0.6299787005 * b));

            return new LabColor(
                (0.2104542553 * l) + (0.7936177850 * m) - (0.0040720468 * s),
                (1.9779984951 * l) - (2.4285922050 * m) + (0.4505937099 * s),
                (0.0259040371 * l) + (0.7827717662 * m) - (0.8086757660 * s));
        }

        /// <summary>
        /// The CIEDE2000 color difference between two CIELAB colors. Roughly, 1 is the smallest
        /// difference a person can see and 2.3 is the classic "just noticeable" threshold; the two
        /// authored hat bands sit about 69 apart.
        /// </summary>
        /// <param name="first">First color, in CIELAB.</param>
        /// <param name="second">Second color, in CIELAB.</param>
        /// <returns>The perceptual difference between them.</returns>
        internal static double DeltaE2000(LabColor first, LabColor second)
        {
            double c1 = Math.Sqrt((first.A * first.A) + (first.B * first.B));
            double c2 = Math.Sqrt((second.A * second.A) + (second.B * second.B));
            double meanC = (c1 + c2) / 2.0;

            // Chroma is stretched near the neutral axis so that grays separate the way eyes see them.
            double meanC7 = Math.Pow(meanC, 7.0);
            double g = 0.5 * (1.0 - Math.Sqrt(meanC7 / (meanC7 + Pow25To7)));

            double a1 = (1.0 + g) * first.A;
            double a2 = (1.0 + g) * second.A;
            double cp1 = Math.Sqrt((a1 * a1) + (first.B * first.B));
            double cp2 = Math.Sqrt((a2 * a2) + (second.B * second.B));

            double hp1 = HueAngle(first.B, a1);
            double hp2 = HueAngle(second.B, a2);

            double deltaL = second.L - first.L;
            double deltaC = cp2 - cp1;

            bool neutral = cp1 * cp2 == 0.0;
            double deltaHueAngle = HueDifference(hp1, hp2, neutral);
            double deltaH = 2.0 * Math.Sqrt(cp1 * cp2) * Math.Sin(DegreesToRadians(deltaHueAngle) / 2.0);

            double meanL = (first.L + second.L) / 2.0;
            double meanCp = (cp1 + cp2) / 2.0;
            double meanHp = MeanHueAngle(hp1, hp2, neutral);

            double t = 1.0
                - (0.17 * Math.Cos(DegreesToRadians(meanHp - 30.0)))
                + (0.24 * Math.Cos(DegreesToRadians(2.0 * meanHp)))
                + (0.32 * Math.Cos(DegreesToRadians((3.0 * meanHp) + 6.0)))
                - (0.20 * Math.Cos(DegreesToRadians((4.0 * meanHp) - 63.0)));

            double meanLOffset = meanL - 50.0;
            double sl = 1.0 + (0.015 * meanLOffset * meanLOffset / Math.Sqrt(20.0 + (meanLOffset * meanLOffset)));
            double sc = 1.0 + (0.045 * meanCp);
            double sh = 1.0 + (0.015 * meanCp * t);

            // Blues rotate toward purple as they darken, and the formula corrects for it here.
            double hueOffset = (meanHp - 275.0) / 25.0;
            double meanCp7 = Math.Pow(meanCp, 7.0);
            double rotation = -2.0
                * Math.Sqrt(meanCp7 / (meanCp7 + Pow25To7))
                * Math.Sin(DegreesToRadians(60.0 * Math.Exp(-hueOffset * hueOffset)));

            double lightness = deltaL / sl;
            double chroma = deltaC / sc;
            double hue = deltaH / sh;

            return Math.Sqrt(
                (lightness * lightness)
                + (chroma * chroma)
                + (hue * hue)
                + (rotation * chroma * hue));
        }

        private const double Pow25To7 = 6103515625.0;

        private static double Linearize(float channel)
        {
            double v = Math.Clamp(channel, 0f, 1f);
            return v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }

        private static double LabCurve(double t)
        {
            return t > 0.008856 ? Math.Cbrt(t) : ((7.787 * t) + (16.0 / 116.0));
        }

        /// <summary>The signed hue step from one angle to another, taking the short way around.</summary>
        /// <param name="hp1">First hue angle in degrees.</param>
        /// <param name="hp2">Second hue angle in degrees.</param>
        /// <param name="neutral">Whether either color sits on the neutral axis, where hue means nothing.</param>
        /// <returns>The hue difference in degrees.</returns>
        private static double HueDifference(double hp1, double hp2, bool neutral)
        {
            if (neutral)
            {
                return 0.0;
            }

            double difference = hp2 - hp1;
            return difference > 180.0 ? difference - 360.0
                : difference < -180.0 ? difference + 360.0
                : difference;
        }

        /// <summary>The hue angle halfway between two others, taking the short way around.</summary>
        /// <param name="hp1">First hue angle in degrees.</param>
        /// <param name="hp2">Second hue angle in degrees.</param>
        /// <param name="neutral">Whether either color sits on the neutral axis, where hue means nothing.</param>
        /// <returns>The mean hue angle in degrees.</returns>
        private static double MeanHueAngle(double hp1, double hp2, bool neutral)
        {
            double sum = hp1 + hp2;
            return neutral ? sum
                : Math.Abs(hp1 - hp2) <= 180.0 ? sum / 2.0
                : sum < 360.0 ? (sum + 360.0) / 2.0
                : (sum - 360.0) / 2.0;
        }

        private static double HueAngle(double b, double a)
        {
            if (a == 0.0 && b == 0.0)
            {
                return 0.0;
            }

            double degrees = Math.Atan2(b, a) * 180.0 / Math.PI;
            return degrees < 0.0 ? degrees + 360.0 : degrees;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
