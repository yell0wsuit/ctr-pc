using System.Runtime.InteropServices;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Packed RGBA color, byte-per-channel, memory-layout-identical to XNA's Color
    /// (R,G,B,A ascending addresses) so vertex arrays reinterpret zero-copy.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Color(byte r, byte g, byte b, byte a)
    {
        public byte R = r; public byte G = g; public byte B = b; public byte A = a;

        public static readonly Color White = new(255, 255, 255, 255);
        public static readonly Color Black = new(0, 0, 0, 255);
        public static readonly Color Transparent = new(0, 0, 0, 0);

        public static Color FromNonPremultiplied(byte r, byte g, byte b, byte a)
        {
            return new Color((byte)(r * a / 255), (byte)(g * a / 255), (byte)(b * a / 255), a);
        }
    }
}
