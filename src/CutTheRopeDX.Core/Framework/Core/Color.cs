using System.Runtime.InteropServices;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Packed RGBA color, byte-per-channel, memory-layout-identical to XNA's Color
    /// (R,G,B,A ascending addresses) so vertex arrays reinterpret zero-copy.
    /// </summary>
    /// <param name="r">Red channel.</param>
    /// <param name="g">Green channel.</param>
    /// <param name="b">Blue channel.</param>
    /// <param name="a">Alpha channel.</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct Color(byte r, byte g, byte b, byte a)
    {
        /// <summary>Red channel.</summary>
        public byte R = r;

        /// <summary>Green channel.</summary>
        public byte G = g;

        /// <summary>Blue channel.</summary>
        public byte B = b;

        /// <summary>Alpha channel.</summary>
        public byte A = a;

        /// <summary>
        /// Builds a color from 0–1 channel values. The per-channel conversion is copied verbatim
        /// from XNA's <c>Color(float, float, float, float)</c> constructor
        /// (<c>(byte)MathHelper.Clamp(value * 255, byte.MinValue, byte.MaxValue)</c>) so colors
        /// authored as floats pack to the exact same bytes the XNA build produced.
        /// </summary>
        /// <param name="r">Red channel, 0–1.</param>
        /// <param name="g">Green channel, 0–1.</param>
        /// <param name="b">Blue channel, 0–1.</param>
        /// <param name="a">Alpha channel, 0–1.</param>
        public Color(float r, float g, float b, float a)
            : this(
                (byte)Helpers.CTRMathHelper.Clamp(r * 255, byte.MinValue, byte.MaxValue),
                (byte)Helpers.CTRMathHelper.Clamp(g * 255, byte.MinValue, byte.MaxValue),
                (byte)Helpers.CTRMathHelper.Clamp(b * 255, byte.MinValue, byte.MaxValue),
                (byte)Helpers.CTRMathHelper.Clamp(a * 255, byte.MinValue, byte.MaxValue))
        {
        }

        /// <summary>Opaque white (255, 255, 255, 255).</summary>
        public static readonly Color White = new(255, 255, 255, 255);

        /// <summary>Opaque black (0, 0, 0, 255).</summary>
        public static readonly Color Black = new(0, 0, 0, 255);

        /// <summary>Fully transparent black (0, 0, 0, 0).</summary>
        public static readonly Color Transparent = new(0, 0, 0, 0);

        /// <summary>
        /// Premultiplies the RGB channels by <paramref name="a"/>. Formula copied verbatim from
        /// XNA's <c>Color.FromNonPremultiplied</c>.
        /// </summary>
        /// <param name="r">Red channel.</param>
        /// <param name="g">Green channel.</param>
        /// <param name="b">Blue channel.</param>
        /// <param name="a">Alpha channel.</param>
        /// <returns>The premultiplied color.</returns>
        public static Color FromNonPremultiplied(byte r, byte g, byte b, byte a)
        {
            return new Color((byte)(r * a / 255), (byte)(g * a / 255), (byte)(b * a / 255), a);
        }
    }
}
