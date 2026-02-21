using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework
{
    public struct RGBAColor(float R, float G, float B, float A)
    {
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

        public static RGBAColor MakeRGBA(float r, float g, float b, float a)
        {
            return new RGBAColor(r, g, b, a);
        }

        public static bool RGBAEqual(RGBAColor a, RGBAColor b)
        {
            return a.RedColor == b.RedColor && a.GreenColor == b.GreenColor && a.BlueColor == b.BlueColor && a.AlphaChannel == b.AlphaChannel;
        }

        public readonly float[] ToFloatArray()
        {
            return [RedColor, GreenColor, BlueColor, AlphaChannel];
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

        public static readonly RGBAColor redRGBA = new(1, 0, 0, 1);

        public static readonly RGBAColor blueRGBA = new(0, 0, 1, 1);

        public static readonly RGBAColor greenRGBA = new(0, 1, 0, 1);

        public static readonly RGBAColor blackRGBA = new(0, 0, 0, 1);

        public static readonly RGBAColor whiteRGBA = new(1, 1, 1, 1);

        public float RedColor { get; set; } = R;

        public float GreenColor { get; set; } = G;

        public float BlueColor { get; set; } = B;

        public float AlphaChannel { get; set; } = A;
    }
}
