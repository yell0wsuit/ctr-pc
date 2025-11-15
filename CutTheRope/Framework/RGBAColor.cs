using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework
{
    public struct RGBAColor
    {
        public readonly Color ToXNA()
        {
            Color result = default;
            int num = (int)(r * 255f);
            int num2 = (int)(g * 255f);
            int num3 = (int)(b * 255f);
            int num4 = (int)(a * 255f);
            result.R = (byte)(num >= 0 ? num > 255 ? 255 : num : 0);
            result.G = (byte)(num2 >= 0 ? num2 > 255 ? 255 : num2 : 0);
            result.B = (byte)(num3 >= 0 ? num3 > 255 ? 255 : num3 : 0);
            result.A = (byte)(num4 >= 0 ? num4 > 255 ? 255 : num4 : 0);
            return result;
        }

        public readonly Color ToWhiteAlphaXNA()
        {
            Color result = default;
            int num = (int)(a * 255f);
            result.R = byte.MaxValue;
            result.G = byte.MaxValue;
            result.B = byte.MaxValue;
            result.A = (byte)(num >= 0 ? num > 255 ? 255 : num : 0);
            return result;
        }

        public static RGBAColor MakeRGBA(double r, double g, double b, double a)
        {
            return MakeRGBA((float)r, (float)g, (float)b, (float)a);
        }

        public static RGBAColor MakeRGBA(float r, float g, float b, float a)
        {
            return new RGBAColor(r, g, b, a);
        }

        public static bool RGBAEqual(RGBAColor a, RGBAColor b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

        public RGBAColor(double R, double G, double B, double A)
        {
            r = (float)R;
            g = (float)G;
            b = (float)B;
            a = (float)A;
        }

        public RGBAColor(float R, float G, float B, float A)
        {
            r = R;
            g = G;
            b = B;
            a = A;
        }

        public readonly float[] ToFloatArray()
        {
            return [r, g, b, a];
        }

        public static float[] ToFloatArray(RGBAColor[] colors)
        {
            List<float> list = [];
            for (int i = 0; i < colors.Length; i++)
            {
                list.AddRange(colors[i].ToFloatArray());
            }
            return [.. list];
        }

        public static readonly RGBAColor transparentRGBA = new(0f, 0f, 0f, 0f);

        public static readonly RGBAColor solidOpaqueRGBA = new(1f, 1f, 1f, 1f);

        public static readonly Color solidOpaqueRGBAXna = Color.White;

        public static readonly RGBAColor redRGBA = new(1.0, 0.0, 0.0, 1.0);

        public static readonly RGBAColor blueRGBA = new(0.0, 0.0, 1.0, 1.0);

        public static readonly RGBAColor greenRGBA = new(0.0, 1.0, 0.0, 1.0);

        public static readonly RGBAColor blackRGBA = new(0.0, 0.0, 0.0, 1.0);

        public static readonly RGBAColor whiteRGBA = new(1.0, 1.0, 1.0, 1.0);

        public float r { get; set; }

        public float g { get; set; }

        public float b { get; set; }

        public float a { get; set; }
    }
}
