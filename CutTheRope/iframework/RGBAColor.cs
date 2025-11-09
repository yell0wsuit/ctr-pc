using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework
{
    // Token: 0x02000025 RID: 37
    public struct RGBAColor
    {
        // Token: 0x06000139 RID: 313 RVA: 0x00006B94 File Offset: 0x00004D94
        public Color toXNA()
        {
            Color result = default(Color);
            int num = (int)(this.r * 255f);
            int num2 = (int)(this.g * 255f);
            int num3 = (int)(this.b * 255f);
            int num4 = (int)(this.a * 255f);
            result.R = (byte)((num >= 0) ? ((num > 255) ? 255 : num) : 0);
            result.G = (byte)((num2 >= 0) ? ((num2 > 255) ? 255 : num2) : 0);
            result.B = (byte)((num3 >= 0) ? ((num3 > 255) ? 255 : num3) : 0);
            result.A = (byte)((num4 >= 0) ? ((num4 > 255) ? 255 : num4) : 0);
            return result;
        }

        // Token: 0x0600013A RID: 314 RVA: 0x00006C64 File Offset: 0x00004E64
        public Color toWhiteAlphaXNA()
        {
            Color result = default(Color);
            int num = (int)(this.a * 255f);
            result.R = byte.MaxValue;
            result.G = byte.MaxValue;
            result.B = byte.MaxValue;
            result.A = (byte)((num >= 0) ? ((num > 255) ? 255 : num) : 0);
            return result;
        }

        // Token: 0x0600013B RID: 315 RVA: 0x00006CCB File Offset: 0x00004ECB
        public static RGBAColor MakeRGBA(double r, double g, double b, double a)
        {
            return RGBAColor.MakeRGBA((float)r, (float)g, (float)b, (float)a);
        }

        // Token: 0x0600013C RID: 316 RVA: 0x00006CDA File Offset: 0x00004EDA
        public static RGBAColor MakeRGBA(float r, float g, float b, float a)
        {
            return new RGBAColor(r, g, b, a);
        }

        // Token: 0x0600013D RID: 317 RVA: 0x00006CE5 File Offset: 0x00004EE5
        public static bool RGBAEqual(RGBAColor a, RGBAColor b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

        // Token: 0x0600013E RID: 318 RVA: 0x00006D21 File Offset: 0x00004F21
        public RGBAColor(double _r, double _g, double _b, double _a)
        {
            this.r = (float)_r;
            this.g = (float)_g;
            this.b = (float)_b;
            this.a = (float)_a;
        }

        // Token: 0x0600013F RID: 319 RVA: 0x00006D44 File Offset: 0x00004F44
        public RGBAColor(float _r, float _g, float _b, float _a)
        {
            this.r = _r;
            this.g = _g;
            this.b = _b;
            this.a = _a;
        }

        // Token: 0x06000140 RID: 320 RVA: 0x00006D63 File Offset: 0x00004F63
        private void init(float _r, float _g, float _b, float _a)
        {
            this.r = _r;
            this.g = _g;
            this.b = _b;
            this.a = _a;
        }

        // Token: 0x06000141 RID: 321 RVA: 0x00006D82 File Offset: 0x00004F82
        public float[] toFloatArray()
        {
            return new float[] { this.r, this.g, this.b, this.a };
        }

        // Token: 0x06000142 RID: 322 RVA: 0x00006DB0 File Offset: 0x00004FB0
        public static float[] toFloatArray(RGBAColor[] colors)
        {
            List<float> list = new();
            for (int i = 0; i < colors.Length; i++)
            {
                list.AddRange(colors[i].toFloatArray());
            }
            return list.ToArray();
        }

        // Token: 0x040000EA RID: 234
        public static readonly RGBAColor transparentRGBA = new(0f, 0f, 0f, 0f);

        // Token: 0x040000EB RID: 235
        public static readonly RGBAColor solidOpaqueRGBA = new(1f, 1f, 1f, 1f);

        // Token: 0x040000EC RID: 236
        public static readonly Color solidOpaqueRGBA_Xna = Color.White;

        // Token: 0x040000ED RID: 237
        public static readonly RGBAColor redRGBA = new(1.0, 0.0, 0.0, 1.0);

        // Token: 0x040000EE RID: 238
        public static readonly RGBAColor blueRGBA = new(0.0, 0.0, 1.0, 1.0);

        // Token: 0x040000EF RID: 239
        public static readonly RGBAColor greenRGBA = new(0.0, 1.0, 0.0, 1.0);

        // Token: 0x040000F0 RID: 240
        public static readonly RGBAColor blackRGBA = new(0.0, 0.0, 0.0, 1.0);

        // Token: 0x040000F1 RID: 241
        public static readonly RGBAColor whiteRGBA = new(1.0, 1.0, 1.0, 1.0);

        // Token: 0x040000F2 RID: 242
        public float r;

        // Token: 0x040000F3 RID: 243
        public float g;

        // Token: 0x040000F4 RID: 244
        public float b;

        // Token: 0x040000F5 RID: 245
        public float a;
    }
}
