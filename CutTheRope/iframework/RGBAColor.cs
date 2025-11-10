using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CutTheRope.iframework
{
    public struct RGBAColor
    {
        public Color toXNA()
        {
            Color result = default(Color);
            int num = (int)(r * 255f);
            int num2 = (int)(g * 255f);
            int num3 = (int)(b * 255f);
            int num4 = (int)(a * 255f);
            result.R = (byte)((num >= 0) ? ((num > 255) ? 255 : num) : 0);
            result.G = (byte)((num2 >= 0) ? ((num2 > 255) ? 255 : num2) : 0);
            result.B = (byte)((num3 >= 0) ? ((num3 > 255) ? 255 : num3) : 0);
            result.A = (byte)((num4 >= 0) ? ((num4 > 255) ? 255 : num4) : 0);
            return result;
        }

        public Color toWhiteAlphaXNA()
        {
            Color result = default(Color);
            int num = (int)(a * 255f);
            result.R = byte.MaxValue;
            result.G = byte.MaxValue;
            result.B = byte.MaxValue;
            result.A = (byte)((num >= 0) ? ((num > 255) ? 255 : num) : 0);
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

        public RGBAColor(double _r, double _g, double _b, double _a)
        {
            r = (float)_r;
            g = (float)_g;
            b = (float)_b;
            a = (float)_a;
        }

        public RGBAColor(float _r, float _g, float _b, float _a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }

        private void init(float _r, float _g, float _b, float _a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }

        public float[] toFloatArray()
        {
            return [r, g, b, a];
        }

        public static float[] toFloatArray(RGBAColor[] colors)
        {
            List<float> list = [];
            for (int i = 0; i < colors.Length; i++)
            {
                list.AddRange(colors[i].toFloatArray());
            }
            return list.ToArray();
        }

        public static readonly RGBAColor transparentRGBA = new(0f, 0f, 0f, 0f);

        public static readonly RGBAColor solidOpaqueRGBA = new(1f, 1f, 1f, 1f);

        public static readonly Color solidOpaqueRGBA_Xna = Color.White;

        public static readonly RGBAColor redRGBA = new(1.0, 0.0, 0.0, 1.0);

        public static readonly RGBAColor blueRGBA = new(0.0, 0.0, 1.0, 1.0);

        public static readonly RGBAColor greenRGBA = new(0.0, 1.0, 0.0, 1.0);

        public static readonly RGBAColor blackRGBA = new(0.0, 0.0, 0.0, 1.0);

        public static readonly RGBAColor whiteRGBA = new(1.0, 1.0, 1.0, 1.0);

        public float r;

        public float g;

        public float b;

        public float a;
    }
}
