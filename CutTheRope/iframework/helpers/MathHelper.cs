using CutTheRope.game;
using CutTheRope.iframework.core;
using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.helpers
{
    // Token: 0x02000062 RID: 98
    internal class MathHelper : ResDataPhoneFull
    {
        // Token: 0x17000024 RID: 36
        // (get) Token: 0x0600034D RID: 845 RVA: 0x000137FA File Offset: 0x000119FA
        public static float RND_MINUS1_1
        {
            get
            {
                return (float)(MathHelper.arc4random() / (double)MathHelper.ARC4RANDOM_MAX * 2.0 - 1.0);
            }
        }

        // Token: 0x17000025 RID: 37
        // (get) Token: 0x0600034E RID: 846 RVA: 0x0001381F File Offset: 0x00011A1F
        public static float RND_0_1
        {
            get
            {
                return (float)(MathHelper.arc4random() / (double)MathHelper.ARC4RANDOM_MAX);
            }
        }

        // Token: 0x0600034F RID: 847 RVA: 0x00013830 File Offset: 0x00011A30
        public static int MIN(int a, int b)
        {
            return Math.Min(a, b);
        }

        // Token: 0x06000350 RID: 848 RVA: 0x00013839 File Offset: 0x00011A39
        public static float MIN(float a, float b)
        {
            return Math.Min(a, b);
        }

        // Token: 0x06000351 RID: 849 RVA: 0x00013842 File Offset: 0x00011A42
        public static float MIN(double a, double b)
        {
            return (float)Math.Min(a, b);
        }

        // Token: 0x06000352 RID: 850 RVA: 0x0001384C File Offset: 0x00011A4C
        public static int MAX(int a, int b)
        {
            return Math.Max(a, b);
        }

        // Token: 0x06000353 RID: 851 RVA: 0x00013855 File Offset: 0x00011A55
        public static float MAX(float a, float b)
        {
            return Math.Max(a, b);
        }

        // Token: 0x06000354 RID: 852 RVA: 0x0001385E File Offset: 0x00011A5E
        public static float MAX(double a, double b)
        {
            return (float)Math.Max(a, b);
        }

        // Token: 0x06000355 RID: 853 RVA: 0x00013868 File Offset: 0x00011A68
        public static int ABS(int a)
        {
            return Math.Abs(a);
        }

        // Token: 0x06000356 RID: 854 RVA: 0x00013870 File Offset: 0x00011A70
        public static float ABS(float a)
        {
            return Math.Abs(a);
        }

        // Token: 0x06000357 RID: 855 RVA: 0x00013878 File Offset: 0x00011A78
        public static float ABS(double a)
        {
            return (float)Math.Abs(a);
        }

        // Token: 0x06000358 RID: 856 RVA: 0x00013881 File Offset: 0x00011A81
        public static int RND(int n)
        {
            return MathHelper.RND_RANGE(0, n);
        }

        // Token: 0x06000359 RID: 857 RVA: 0x0001388A File Offset: 0x00011A8A
        public static int RND_RANGE(int n, int m)
        {
            return MathHelper.random_.Next(n, m + 1);
        }

        // Token: 0x0600035A RID: 858 RVA: 0x0001389A File Offset: 0x00011A9A
        public static uint arc4random()
        {
            return (uint)MathHelper.random_.Next(int.MinValue, int.MaxValue);
        }

        // Token: 0x0600035B RID: 859 RVA: 0x000138B0 File Offset: 0x00011AB0
        public static float FIT_TO_BOUNDARIES(double V, double MINV, double MAXV)
        {
            return MathHelper.FIT_TO_BOUNDARIES((float)V, (float)MINV, (float)MAXV);
        }

        // Token: 0x0600035C RID: 860 RVA: 0x000138BD File Offset: 0x00011ABD
        public static float FIT_TO_BOUNDARIES(float V, float MINV, float MAXV)
        {
            return Math.Max(Math.Min(V, MAXV), MINV);
        }

        // Token: 0x0600035D RID: 861 RVA: 0x000138CC File Offset: 0x00011ACC
        public static float ceil(double value)
        {
            return (float)Math.Ceiling(value);
        }

        // Token: 0x0600035E RID: 862 RVA: 0x000138D5 File Offset: 0x00011AD5
        public static float round(double value)
        {
            return (float)Math.Round(value);
        }

        // Token: 0x0600035F RID: 863 RVA: 0x000138DE File Offset: 0x00011ADE
        public static float cosf(float x)
        {
            return (float)Math.Cos((double)x);
        }

        // Token: 0x06000360 RID: 864 RVA: 0x000138E8 File Offset: 0x00011AE8
        public static float sinf(float x)
        {
            return (float)Math.Sin((double)x);
        }

        // Token: 0x06000361 RID: 865 RVA: 0x000138F2 File Offset: 0x00011AF2
        public static float tanf(float x)
        {
            return (float)Math.Tan((double)x);
        }

        // Token: 0x06000362 RID: 866 RVA: 0x000138FC File Offset: 0x00011AFC
        public static float acosf(float x)
        {
            return (float)Math.Acos((double)x);
        }

        // Token: 0x06000363 RID: 867 RVA: 0x00013908 File Offset: 0x00011B08
        public static void fmInit()
        {
            if (MathHelper.fmSins == null)
            {
                MathHelper.fmSins = new float[1024];
                for (int i = 0; i < 1024; i++)
                {
                    MathHelper.fmSins[i] = (float)Math.Sin((double)(i * 2) * 3.141592653589793 / 1024.0);
                }
            }
            if (MathHelper.fmCoss == null)
            {
                MathHelper.fmCoss = new float[1024];
                for (int j = 0; j < 1024; j++)
                {
                    MathHelper.fmCoss[j] = (float)Math.Cos((double)(j * 2) * 3.141592653589793 / 1024.0);
                }
            }
        }

        // Token: 0x06000364 RID: 868 RVA: 0x000139AC File Offset: 0x00011BAC
        public static float fmSin(float angle)
        {
            int num = (int)((double)(angle * 1024f) / 3.141592653589793 / 2.0);
            num &= 1023;
            return MathHelper.fmSins[num];
        }

        // Token: 0x06000365 RID: 869 RVA: 0x000139E8 File Offset: 0x00011BE8
        public static float fmCos(float angle)
        {
            int num = (int)((double)(angle * 1024f) / 3.141592653589793 / 2.0);
            num &= 1023;
            return MathHelper.fmCoss[num];
        }

        // Token: 0x06000366 RID: 870 RVA: 0x00013A22 File Offset: 0x00011C22
        public static bool sameSign(float a, float b)
        {
            return (a >= 0f && b >= 0f) || (a < 0f && b < 0f);
        }

        // Token: 0x06000367 RID: 871 RVA: 0x00013A48 File Offset: 0x00011C48
        public static bool pointInRect(float x, float y, float checkX, float checkY, float checkWidth, float checkHeight)
        {
            return x >= checkX && x < checkX + checkWidth && y >= checkY && y < checkY + checkHeight;
        }

        // Token: 0x06000368 RID: 872 RVA: 0x00013A62 File Offset: 0x00011C62
        public static bool rectInRect(float x1l, float y1t, float x1r, float y1b, float x2l, float y2t, float x2r, float y2b)
        {
            return x1l <= x2r && x1r >= x2l && y1t <= y2b && y1b >= y2t;
        }

        // Token: 0x06000369 RID: 873 RVA: 0x00013A80 File Offset: 0x00011C80
        public static bool obbInOBB(Vector tl1, Vector tr1, Vector br1, Vector bl1, Vector tl2, Vector tr2, Vector br2, Vector bl2)
        {
            Vector[] array = new Vector[4];
            Vector[] array2 = new Vector[4];
            array[0] = tl1;
            array[1] = tr1;
            array[2] = br1;
            array[3] = bl1;
            array2[0] = tl2;
            array2[1] = tr2;
            array2[2] = br2;
            array2[3] = bl2;
            return MathHelper.overlaps1Way(array, array2) && MathHelper.overlaps1Way(array2, array);
        }

        // Token: 0x0600036A RID: 874 RVA: 0x00013AF1 File Offset: 0x00011CF1
        public static float DEGREES_TO_RADIANS(float D)
        {
            return (float)((double)D * 3.141592653589793 / 180.0);
        }

        // Token: 0x0600036B RID: 875 RVA: 0x00013B0A File Offset: 0x00011D0A
        public static float RADIANS_TO_DEGREES(float R)
        {
            return (float)((double)(R * 180f) / 3.141592653589793);
        }

        // Token: 0x0600036C RID: 876 RVA: 0x00013B20 File Offset: 0x00011D20
        private static bool overlaps1Way(Vector[] corner, Vector[] other)
        {
            Vector[] array = new Vector[2];
            float[] array2 = new float[2];
            array[0] = MathHelper.vectSub(corner[1], corner[0]);
            array[1] = MathHelper.vectSub(corner[3], corner[0]);
            for (int i = 0; i < 2; i++)
            {
                array[i] = MathHelper.vectDiv(array[i], MathHelper.vectLengthsq(array[i]));
                array2[i] = MathHelper.vectDot(corner[0], array[i]);
            }
            for (int j = 0; j < 2; j++)
            {
                float num = MathHelper.vectDot(other[0], array[j]);
                float num2 = num;
                float num3 = num;
                for (int k = 1; k < 4; k++)
                {
                    num = MathHelper.vectDot(other[k], array[j]);
                    if (num < num2)
                    {
                        num2 = num;
                    }
                    else if (num > num3)
                    {
                        num3 = num;
                    }
                }
                if (num2 > 1f + array2[j] || num3 < array2[j])
                {
                    return false;
                }
            }
            return true;
        }

        // Token: 0x0600036D RID: 877 RVA: 0x00013C2C File Offset: 0x00011E2C
        public static Rectangle rectInRectIntersection(Rectangle r1, Rectangle r2)
        {
            Rectangle result = r2;
            result.x = r2.x - r1.x;
            result.y = r2.y - r1.y;
            if (result.x < 0f)
            {
                result.w += result.x;
                result.x = 0f;
            }
            if (result.x + result.w > r1.w)
            {
                result.w = r1.w - result.x;
            }
            if (result.y < 0f)
            {
                result.h += result.y;
                result.y = 0f;
            }
            if (result.y + result.h > r1.h)
            {
                result.h = r1.h - result.y;
            }
            return result;
        }

        // Token: 0x0600036E RID: 878 RVA: 0x00013D0C File Offset: 0x00011F0C
        public static float angleTo0_360(float angle)
        {
            float num = angle;
            while (Math.Abs(num) > 360f)
            {
                num -= ((num > 0f) ? 360f : (-360f));
            }
            if (num < 0f)
            {
                num += 360f;
            }
            return num;
        }

        // Token: 0x0600036F RID: 879 RVA: 0x00013D52 File Offset: 0x00011F52
        public static Vector vect(float x, float y)
        {
            return new Vector(x, y);
        }

        // Token: 0x06000370 RID: 880 RVA: 0x00013D5B File Offset: 0x00011F5B
        public static bool vectEqual(Vector v1, Vector v2)
        {
            return v1.x == v2.x && v1.y == v2.y;
        }

        // Token: 0x06000371 RID: 881 RVA: 0x00013D7B File Offset: 0x00011F7B
        public static Vector vectAdd(Vector v1, Vector v2)
        {
            return new Vector(v1.x + v2.x, v1.y + v2.y);
        }

        // Token: 0x06000372 RID: 882 RVA: 0x00013D9C File Offset: 0x00011F9C
        public static Vector vectNeg(Vector v)
        {
            return new Vector(0f - v.x, 0f - v.y);
        }

        // Token: 0x06000373 RID: 883 RVA: 0x00013DBB File Offset: 0x00011FBB
        public static Vector vectSub(Vector v1, Vector v2)
        {
            return new Vector(v1.x - v2.x, v1.y - v2.y);
        }

        // Token: 0x06000374 RID: 884 RVA: 0x00013DDC File Offset: 0x00011FDC
        public static Vector vectMult(Vector v, double s)
        {
            return MathHelper.vectMult(v, (float)s);
        }

        // Token: 0x06000375 RID: 885 RVA: 0x00013DE6 File Offset: 0x00011FE6
        public static Vector vectMult(Vector v, float s)
        {
            return new Vector(v.x * s, v.y * s);
        }

        // Token: 0x06000376 RID: 886 RVA: 0x00013DFD File Offset: 0x00011FFD
        public static Vector vectDiv(Vector v, float s)
        {
            return new Vector(v.x / s, v.y / s);
        }

        // Token: 0x06000377 RID: 887 RVA: 0x00013E14 File Offset: 0x00012014
        public static float vectDot(Vector v1, Vector v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }

        // Token: 0x06000378 RID: 888 RVA: 0x00013E31 File Offset: 0x00012031
        private static float vectCross(Vector v1, Vector v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        // Token: 0x06000379 RID: 889 RVA: 0x00013E4E File Offset: 0x0001204E
        public static Vector vectPerp(Vector v)
        {
            return new Vector(0f - v.y, v.x);
        }

        // Token: 0x0600037A RID: 890 RVA: 0x00013E67 File Offset: 0x00012067
        public static Vector vectRperp(Vector v)
        {
            return new Vector(v.y, 0f - v.x);
        }

        // Token: 0x0600037B RID: 891 RVA: 0x00013E80 File Offset: 0x00012080
        private static Vector vectProject(Vector v1, Vector v2)
        {
            return MathHelper.vectMult(v2, MathHelper.vectDot(v1, v2) / MathHelper.vectDot(v2, v2));
        }

        // Token: 0x0600037C RID: 892 RVA: 0x00013E97 File Offset: 0x00012097
        private static Vector vectRotateByVector(Vector v1, Vector v2)
        {
            return new Vector(v1.x * v2.x - v1.y * v2.y, v1.x * v2.y + v1.y * v2.x);
        }

        // Token: 0x0600037D RID: 893 RVA: 0x00013ED4 File Offset: 0x000120D4
        private static Vector vectUnrotateByVector(Vector v1, Vector v2)
        {
            return new Vector(v1.x * v2.x + v1.y * v2.y, v1.y * v2.x - v1.x * v2.y);
        }

        // Token: 0x0600037E RID: 894 RVA: 0x00013F11 File Offset: 0x00012111
        public static float vectAngle(Vector v)
        {
            return (float)Math.Atan((double)(v.y / v.x));
        }

        // Token: 0x0600037F RID: 895 RVA: 0x00013F27 File Offset: 0x00012127
        public static float vectAngleNormalized(Vector v)
        {
            return (float)Math.Atan2((double)v.y, (double)v.x);
        }

        // Token: 0x06000380 RID: 896 RVA: 0x00013F3D File Offset: 0x0001213D
        public static float vectLength(Vector v)
        {
            return (float)Math.Sqrt((double)MathHelper.vectDot(v, v));
        }

        // Token: 0x06000381 RID: 897 RVA: 0x00013F4D File Offset: 0x0001214D
        public static float vectLengthsq(Vector v)
        {
            return MathHelper.vectDot(v, v);
        }

        // Token: 0x06000382 RID: 898 RVA: 0x00013F56 File Offset: 0x00012156
        public static Vector vectNormalize(Vector v)
        {
            return MathHelper.vectMult(v, 1f / MathHelper.vectLength(v));
        }

        // Token: 0x06000383 RID: 899 RVA: 0x00013F6A File Offset: 0x0001216A
        public static Vector vectForAngle(float a)
        {
            return new Vector(MathHelper.fmCos(a), MathHelper.fmSin(a));
        }

        // Token: 0x06000384 RID: 900 RVA: 0x00013F7D File Offset: 0x0001217D
        private static float vectToAngle(Vector v)
        {
            return (float)Math.Atan2((double)v.x, (double)v.y);
        }

        // Token: 0x06000385 RID: 901 RVA: 0x00013F93 File Offset: 0x00012193
        public static float vectDistance(Vector v1, Vector v2)
        {
            return MathHelper.vectLength(MathHelper.vectSub(v1, v2));
        }

        // Token: 0x06000386 RID: 902 RVA: 0x00013FA4 File Offset: 0x000121A4
        public static Vector vectRotate(Vector v, double rad)
        {
            float num = MathHelper.fmCos((float)rad);
            float num2 = MathHelper.fmSin((float)rad);
            float num3 = v.x * num - v.y * num2;
            float yParam = v.x * num2 + v.y * num;
            return new Vector(num3, yParam);
        }

        // Token: 0x06000387 RID: 903 RVA: 0x00013FEC File Offset: 0x000121EC
        public static Vector vectRotateAround(Vector v, double rad, float cx, float cy)
        {
            Vector v2 = v;
            v2.x -= cx;
            v2.y -= cy;
            v2 = MathHelper.vectRotate(v2, rad);
            v2.x += cx;
            v2.y += cy;
            return v2;
        }

        // Token: 0x06000388 RID: 904 RVA: 0x00014034 File Offset: 0x00012234
        private static Vector vectSidePerp(Vector v1, Vector v2)
        {
            return MathHelper.vectNormalize(MathHelper.vectRperp(MathHelper.vectSub(v2, v1)));
        }

        // Token: 0x06000389 RID: 905 RVA: 0x00014047 File Offset: 0x00012247
        private static int vcode(float x_min, float y_min, float x_max, float y_max, Vector p)
        {
            return ((p.x < x_min) ? 1 : 0) + ((p.x > x_max) ? 2 : 0) + ((p.y < y_min) ? 4 : 0) + ((p.y > y_max) ? 8 : 0);
        }

        // Token: 0x0600038A RID: 906 RVA: 0x00014080 File Offset: 0x00012280
        public static bool lineInRect(float x1, float y1, float x2, float y2, float rx, float ry, float w, float h)
        {
            VectorClass vectorClass = new(new Vector(x1, y1));
            VectorClass vectorClass2 = new(new Vector(x2, y2));
            float num = rx + w;
            float num2 = ry + h;
            int num3 = MathHelper.vcode(rx, ry, num, num2, vectorClass.v);
            int num4 = MathHelper.vcode(rx, ry, num, num2, vectorClass2.v);
            while (num3 != 0 || num4 != 0)
            {
                if ((num3 & num4) != 0)
                {
                    return false;
                }
                int num5;
                VectorClass vectorClass3;
                if (num3 != 0)
                {
                    num5 = num3;
                    vectorClass3 = vectorClass;
                }
                else
                {
                    num5 = num4;
                    vectorClass3 = vectorClass2;
                }
                if ((num5 & 1) != 0)
                {
                    VectorClass vectorClass4 = vectorClass3;
                    vectorClass4.v.y = vectorClass4.v.y + (y1 - y2) * (rx - vectorClass3.v.x) / (x1 - x2);
                    vectorClass3.v.x = rx;
                }
                else if ((num5 & 2) != 0)
                {
                    VectorClass vectorClass5 = vectorClass3;
                    vectorClass5.v.y = vectorClass5.v.y + (y1 - y2) * (num - vectorClass3.v.x) / (x1 - x2);
                    vectorClass3.v.x = num;
                }
                if ((num5 & 4) != 0)
                {
                    VectorClass vectorClass6 = vectorClass3;
                    vectorClass6.v.x = vectorClass6.v.x + (x1 - x2) * (ry - vectorClass3.v.y) / (y1 - y2);
                    vectorClass3.v.y = ry;
                }
                else if ((num5 & 8) != 0)
                {
                    VectorClass vectorClass7 = vectorClass3;
                    vectorClass7.v.x = vectorClass7.v.x + (x1 - x2) * (num2 - vectorClass3.v.y) / (y1 - y2);
                    vectorClass3.v.y = num2;
                }
                if (num5 == num3)
                {
                    num3 = MathHelper.vcode(rx, ry, num, num2, vectorClass.v);
                }
                else
                {
                    num4 = MathHelper.vcode(rx, ry, num, num2, vectorClass2.v);
                }
            }
            return true;
        }

        // Token: 0x0600038B RID: 907 RVA: 0x00014224 File Offset: 0x00012424
        public bool lineInLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            Vector vector = default(Vector);
            vector.x = x3 - x1 + x4 - x2;
            vector.y = y3 - y1 + y4 - y2;
            Vector vector2 = default(Vector);
            vector2.x = x2 - x1;
            vector2.y = y2 - y1;
            Vector vector3 = default(Vector);
            vector3.x = x4 - x3;
            vector3.y = y4 - y3;
            float value = vector2.y * vector3.x - vector3.y * vector2.x;
            float num = vector3.x * vector.y - vector3.y * vector.x;
            float value2 = vector2.x * vector.y - vector2.y * vector.x;
            return Math.Abs(num) <= Math.Abs(value) && Math.Abs(value2) <= Math.Abs(value);
        }

        // Token: 0x0600038C RID: 908 RVA: 0x0001430C File Offset: 0x0001250C
        public static float FLOAT_RND_RANGE(int S, int F)
        {
            return (float)MathHelper.RND_RANGE(S * 1000, F * 1000) / 1000f;
        }

        // Token: 0x0600038D RID: 909 RVA: 0x00014328 File Offset: 0x00012528
        public static NSString getMD5Str(NSString input)
        {
            return MathHelper.getMD5(input.getCharacters());
        }

        // Token: 0x0600038E RID: 910 RVA: 0x00014338 File Offset: 0x00012538
        public static NSString getMD5(char[] data)
        {
            byte[] array = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                array[i * 2] = (byte)((data[i] & '\uff00') >> 8);
                array[i * 2 + 1] = (byte)(data[i] & 'ÿ');
            }
            md5.md5_context ctx = new();
            md5.md5_starts(ref ctx);
            md5.md5_update(ref ctx, array, (uint)array.Length);
            byte[] array2 = new byte[16];
            md5.md5_finish(ref ctx, array2);
            char[] array3 = new char[33];
            int num = 0;
            for (int j = 0; j < 16; j++)
            {
                int num2 = (int)array2[j];
                int num3 = (num2 >> 4) & 15;
                array3[num++] = (char)((num3 < 10) ? (48 + num3) : (97 + num3 - 10));
                num3 = num2 & 15;
                array3[num++] = (char)((num3 < 10) ? (48 + num3) : (97 + num3 - 10));
            }
            array3[num++] = '\0';
            return new NSString(new string(array3));
        }

        // Token: 0x04000280 RID: 640
        private const int fmSinCosSize = 1024;

        // Token: 0x04000281 RID: 641
        public const double M_PI = 3.141592653589793;

        // Token: 0x04000282 RID: 642
        private const int COHEN_LEFT = 1;

        // Token: 0x04000283 RID: 643
        private const int COHEN_RIGHT = 2;

        // Token: 0x04000284 RID: 644
        private const int COHEN_BOT = 4;

        // Token: 0x04000285 RID: 645
        private const int COHEN_TOP = 8;

        // Token: 0x04000286 RID: 646
        private static Random random_ = new();

        // Token: 0x04000287 RID: 647
        private static long ARC4RANDOM_MAX = 4294967296L;

        // Token: 0x04000288 RID: 648
        private static float[] fmSins;

        // Token: 0x04000289 RID: 649
        private static float[] fmCoss;

        // Token: 0x0400028A RID: 650
        public static readonly Vector vectZero = new(0f, 0f);

        // Token: 0x0400028B RID: 651
        public static readonly Vector vectUndefined = new(2.1474836E+09f, 2.1474836E+09f);
    }
}
