using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework
{
    /// <summary>
    /// Represents a color with red, green, blue, and alpha channels as floats in the 0–1 range.
    /// </summary>
    /// <param name="R">Red channel value.</param>
    /// <param name="G">Green channel value.</param>
    /// <param name="B">Blue channel value.</param>
    /// <param name="A">Alpha channel value.</param>
    public struct RGBAColor(float R, float G, float B, float A)
    {
        /// <summary>
        /// Converts this color to an XNA <see cref="Color"/>.
        /// </summary>
        /// <returns>The converted XNA color.</returns>
        public readonly Color ToXNA()
        {
            Color result = default;
            int redByte = (int)(RedColor * 255f);
            int greenByte = (int)(GreenColor * 255f);
            int blueByte = (int)(BlueColor * 255f);
            int alphaByte = (int)(AlphaChannel * 255f);
            result.R = (byte)(redByte >= 0 ? redByte > 255 ? 255 : redByte : 0);
            result.G = (byte)(greenByte >= 0 ? greenByte > 255 ? 255 : greenByte : 0);
            result.B = (byte)(blueByte >= 0 ? blueByte > 255 ? 255 : blueByte : 0);
            result.A = (byte)(alphaByte >= 0 ? alphaByte > 255 ? 255 : alphaByte : 0);
            return result;
        }

        /// <summary>
        /// Converts to an XNA <see cref="Color"/> with white RGB and this color's alpha.
        /// </summary>
        /// <returns>An XNA color with white RGB and this instance's alpha channel.</returns>
        public readonly Color ToWhiteAlphaXNA()
        {
            Color result = default;
            int alphaByte = (int)(AlphaChannel * 255f);
            result.R = byte.MaxValue;
            result.G = byte.MaxValue;
            result.B = byte.MaxValue;
            result.A = (byte)(alphaByte >= 0 ? alphaByte > 255 ? 255 : alphaByte : 0);
            return result;
        }

        /// <summary>
        /// Creates a new <see cref="RGBAColor"/> from the specified channel values.
        /// </summary>
        /// <param name="r">Red channel.</param>
        /// <param name="g">Green channel.</param>
        /// <param name="b">Blue channel.</param>
        /// <param name="a">Alpha channel.</param>
        /// <returns>A new <see cref="RGBAColor"/> instance.</returns>
        public static RGBAColor MakeRGBA(float r, float g, float b, float a)
        {
            return new RGBAColor(r, g, b, a);
        }

        /// <summary>
        /// Returns <see langword="true"/> if all four channels of <paramref name="a"/> and <paramref name="b"/> are equal.
        /// </summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color.</param>
        /// <returns><see langword="true"/> when all channels match; otherwise <see langword="false"/>.</returns>
        public static bool RGBAEqual(RGBAColor a, RGBAColor b)
        {
            return a.RedColor == b.RedColor && a.GreenColor == b.GreenColor && a.BlueColor == b.BlueColor && a.AlphaChannel == b.AlphaChannel;
        }

        /// <summary>
        /// Returns the four channel values as a float array [R, G, B, A].
        /// </summary>
        /// <returns>A float array in RGBA order.</returns>
        public readonly float[] ToFloatArray()
        {
            return [RedColor, GreenColor, BlueColor, AlphaChannel];
        }

        /// <summary>
        /// Converts an array of <paramref name="colors"/> to a flat float array of channel values.
        /// </summary>
        /// <param name="colors">Colors to convert.</param>
        /// <returns>A flat float array containing RGBA values for each input color.</returns>
        public static float[] ToFloatArray(RGBAColor[] colors)
        {
            List<float> list = [];
            for (int i = 0; i < colors.Length; i++)
            {
                list.AddRange(colors[i].ToFloatArray());
            }
            return [.. list];
        }

        /// <summary>
        /// Fully transparent black (0, 0, 0, 0).
        /// </summary>
        public static readonly RGBAColor transparentRGBA = new(0f, 0f, 0f, 0f);

        /// <summary>
        /// Fully opaque white (1, 1, 1, 1).
        /// </summary>
        public static readonly RGBAColor solidOpaqueRGBA = new(1f, 1f, 1f, 1f);

        /// <summary>
        /// XNA <see cref="Color.White"/> equivalent of <see cref="solidOpaqueRGBA"/>.
        /// </summary>
        public static readonly Color solidOpaqueRGBAXna = Color.White;

        /// <summary>
        /// Fully opaque red (1, 0, 0, 1).
        /// </summary>
        public static readonly RGBAColor redRGBA = new(1, 0, 0, 1);

        /// <summary>
        /// Fully opaque blue (0, 0, 1, 1).
        /// </summary>
        public static readonly RGBAColor blueRGBA = new(0, 0, 1, 1);

        /// <summary>
        /// Fully opaque green (0, 1, 0, 1).
        /// </summary>
        public static readonly RGBAColor greenRGBA = new(0, 1, 0, 1);

        /// <summary>
        /// Fully opaque black (0, 0, 0, 1).
        /// </summary>
        public static readonly RGBAColor blackRGBA = new(0, 0, 0, 1);

        /// <summary>
        /// Fully opaque white (1, 1, 1, 1).
        /// </summary>
        public static readonly RGBAColor whiteRGBA = new(1, 1, 1, 1);

        /// <summary>
        /// Red channel value (0–1).
        /// </summary>
        public float RedColor { get; set; } = R;

        /// <summary>
        /// Green channel value (0–1).
        /// </summary>
        public float GreenColor { get; set; } = G;

        /// <summary>
        /// Blue channel value (0–1).
        /// </summary>
        public float BlueColor { get; set; } = B;

        /// <summary>
        /// Alpha channel value (0–1).
        /// </summary>
        public float AlphaChannel { get; set; } = A;
    }
}
