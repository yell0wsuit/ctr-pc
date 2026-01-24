using System;
using System.Globalization;
using System.Security.Cryptography;

using CutTheRope.Framework.Core;
using CutTheRope.GameMain;
using CutTheRope.Helpers;

namespace CutTheRope.Framework.Helpers
{
    internal class CTRMathHelper : ResDataPhoneFull
    {
        // (get) Token: 0x0600034D RID: 845 RVA: 0x000137FA File Offset: 0x000119FA
        public static float RND_MINUS1_1 => (float)((Arc4random() / (double)ARC4RANDOM_MAX * 2.0) - 1.0);

        // (get) Token: 0x0600034E RID: 846 RVA: 0x0001381F File Offset: 0x00011A1F
        public static float RND_0_1 => (float)(Arc4random() / (double)ARC4RANDOM_MAX);

        public static int MIN(int a, int b)
        {
            return Math.Min(a, b);
        }

        public static float MIN(float a, float b)
        {
            return Math.Min(a, b);
        }

        public static float MIN(double a, double b)
        {
            return (float)Math.Min(a, b);
        }

        public static int MAX(int a, int b)
        {
            return Math.Max(a, b);
        }

        public static float MAX(float a, float b)
        {
            return Math.Max(a, b);
        }

        public static float MAX(double a, double b)
        {
            return (float)Math.Max(a, b);
        }

        public static int ABS(int a)
        {
            return Math.Abs(a);
        }

        public static float ABS(float a)
        {
            return Math.Abs(a);
        }

        public static float ABS(double a)
        {
            return (float)Math.Abs(a);
        }

        public static int RND(int n)
        {
            return RND_RANGE(0, n);
        }

        public static int RND_RANGE(int n, int m)
        {
            return random_.Next(n, m + 1);
        }

        public static uint Arc4random()
        {
            return (uint)random_.Next(int.MinValue, int.MaxValue);
        }

        public static float FIT_TO_BOUNDARIES(double V, double MINV, double MAXV)
        {
            return FIT_TO_BOUNDARIES((float)V, (float)MINV, (float)MAXV);
        }

        public static float FIT_TO_BOUNDARIES(float V, float MINV, float MAXV)
        {
            return Math.Max(Math.Min(V, MAXV), MINV);
        }

        public static float Ceil(double value)
        {
            return (float)Math.Ceiling(value);
        }

        public static float Round(double value)
        {
            return (float)Math.Round(value);
        }

        public static float Cosf(float x)
        {
            return (float)Math.Cos((double)x);
        }

        public static float Sinf(float x)
        {
            return (float)Math.Sin((double)x);
        }

        public static float Tanf(float x)
        {
            return (float)Math.Tan((double)x);
        }

        public static float Acosf(float x)
        {
            return (float)Math.Acos((double)x);
        }

        public static void FmInit()
        {
            if (fmSins == null)
            {
                fmSins = new float[1024];
                for (int i = 0; i < 1024; i++)
                {
                    fmSins[i] = (float)Math.Sin(i * 2 * 3.141592653589793 / 1024.0);
                }
            }
            if (fmCoss == null)
            {
                fmCoss = new float[1024];
                for (int j = 0; j < 1024; j++)
                {
                    fmCoss[j] = (float)Math.Cos(j * 2 * 3.141592653589793 / 1024.0);
                }
            }
        }

        public static float FmSin(float angle)
        {
            int num = (int)((double)(angle * 1024f) / 3.141592653589793 / 2.0);
            num &= 1023;
            return fmSins[num];
        }

        public static float FmCos(float angle)
        {
            int num = (int)((double)(angle * 1024f) / 3.141592653589793 / 2.0);
            num &= 1023;
            return fmCoss[num];
        }

        public static bool SameSign(float a, float b)
        {
            return (a >= 0f && b >= 0f) || (a < 0f && b < 0f);
        }

        public static bool PointInRect(float x, float y, float checkX, float checkY, float checkWidth, float checkHeight)
        {
            return x >= checkX && x < checkX + checkWidth && y >= checkY && y < checkY + checkHeight;
        }

        public static bool RectInRect(float x1l, float y1t, float x1r, float y1b, float x2l, float y2t, float x2r, float y2b)
        {
            return x1l <= x2r && x1r >= x2l && y1t <= y2b && y1b >= y2t;
        }

        public static bool ObbInOBB(Vector tl1, Vector tr1, Vector br1, Vector bl1, Vector tl2, Vector tr2, Vector br2, Vector bl2)
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
            return Overlaps1Way(array, array2) && Overlaps1Way(array2, array);
        }

        public static float DEGREES_TO_RADIANS(float D)
        {
            return (float)((double)D * 3.141592653589793 / 180.0);
        }

        public static float RADIANS_TO_DEGREES(float R)
        {
            return (float)((double)(R * 180f) / 3.141592653589793);
        }

        private static bool Overlaps1Way(Vector[] corner, Vector[] other)
        {
            Vector[] array = new Vector[2];
            float[] array2 = new float[2];
            array[0] = VectSub(corner[1], corner[0]);
            array[1] = VectSub(corner[3], corner[0]);
            for (int i = 0; i < 2; i++)
            {
                array[i] = VectDiv(array[i], VectLengthsq(array[i]));
                array2[i] = VectDot(corner[0], array[i]);
            }
            for (int j = 0; j < 2; j++)
            {
                float num = VectDot(other[0], array[j]);
                float num2 = num;
                float num3 = num;
                for (int k = 1; k < 4; k++)
                {
                    num = VectDot(other[k], array[j]);
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

        public static CTRRectangle RectInRectIntersection(CTRRectangle r1, CTRRectangle r2)
        {
            CTRRectangle result = r2;
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

        public static float AngleTo0_360(float angle)
        {
            float num = angle;
            while (Math.Abs(num) > 360f)
            {
                num -= num > 0f ? 360f : -360f;
            }
            if (num < 0f)
            {
                num += 360f;
            }
            return num;
        }

        public static Vector Vect(float x, float y)
        {
            return new Vector(x, y);
        }

        public static bool VectEqual(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        public static Vector VectAdd(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector VectNeg(Vector v)
        {
            return new Vector(0f - v.X, 0f - v.Y);
        }

        public static Vector VectSub(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector VectMult(Vector v, double s)
        {
            return VectMult(v, (float)s);
        }

        public static Vector VectMult(Vector v, float s)
        {
            return new Vector(v.X * s, v.Y * s);
        }

        public static Vector VectDiv(Vector v, float s)
        {
            return new Vector(v.X / s, v.Y / s);
        }

        public static float VectDot(Vector v1, Vector v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y);
        }

        public static Vector VectPerp(Vector v)
        {
            return new Vector(0f - v.Y, v.X);
        }

        public static Vector VectRperp(Vector v)
        {
            return new Vector(v.Y, 0f - v.X);
        }

        public static float VectAngle(Vector v)
        {
            return (float)Math.Atan((double)(v.Y / v.X));
        }

        public static float VectAngleNormalized(Vector v)
        {
            return (float)Math.Atan2(v.Y, v.X);
        }

        public static float VectLength(Vector v)
        {
            return (float)Math.Sqrt((double)VectDot(v, v));
        }

        public static float VectLengthsq(Vector v)
        {
            return VectDot(v, v);
        }

        public static Vector VectNormalize(Vector v)
        {
            return VectMult(v, 1f / VectLength(v));
        }

        public static Vector VectForAngle(float a)
        {
            return new Vector(FmCos(a), FmSin(a));
        }

        public static float VectDistance(Vector v1, Vector v2)
        {
            return VectLength(VectSub(v1, v2));
        }

        public static Vector VectRotate(Vector v, double rad)
        {
            float num = FmCos((float)rad);
            float num2 = FmSin((float)rad);
            float num3 = (v.X * num) - (v.Y * num2);
            float yParam = (v.X * num2) + (v.Y * num);
            return new Vector(num3, yParam);
        }

        public static Vector VectRotateAround(Vector v, double rad, float cx, float cy)
        {
            Vector v2 = v;
            v2.X -= cx;
            v2.Y -= cy;
            v2 = VectRotate(v2, rad);
            v2.X += cx;
            v2.Y += cy;
            return v2;
        }

        private static int Vcode(float x_min, float y_min, float x_max, float y_max, Vector p)
        {
            return (p.X < x_min ? 1 : 0) + (p.X > x_max ? 2 : 0) + (p.Y < y_min ? 4 : 0) + (p.Y > y_max ? 8 : 0);
        }

        public static bool LineInRect(float x1, float y1, float x2, float y2, float rx, float ry, float w, float h)
        {
            VectorClass vectorClass = new(new Vector(x1, y1));
            VectorClass vectorClass2 = new(new Vector(x2, y2));
            float num = rx + w;
            float num2 = ry + h;
            int num3 = Vcode(rx, ry, num, num2, vectorClass.VectorPoint);
            int num4 = Vcode(rx, ry, num, num2, vectorClass2.VectorPoint);
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
                    Vector temp = vectorClass3.VectorPoint;
                    temp.Y += (y1 - y2) * (rx - temp.X) / (x1 - x2);
                    temp.X = rx;
                    vectorClass3.VectorPoint = temp;
                }
                else if ((num5 & 2) != 0)
                {
                    Vector temp = vectorClass3.VectorPoint;
                    temp.Y += (y1 - y2) * (num - temp.X) / (x1 - x2);
                    temp.X = num;
                    vectorClass3.VectorPoint = temp;
                }
                if ((num5 & 4) != 0)
                {
                    Vector temp = vectorClass3.VectorPoint;
                    temp.X += (x1 - x2) * (ry - temp.Y) / (y1 - y2);
                    temp.Y = ry;
                    vectorClass3.VectorPoint = temp;
                }
                else if ((num5 & 8) != 0)
                {
                    Vector temp = vectorClass3.VectorPoint;
                    temp.X += (x1 - x2) * (num2 - temp.Y) / (y1 - y2);
                    temp.Y = num2;
                    vectorClass3.VectorPoint = temp;
                }
                if (num5 == num3)
                {
                    num3 = Vcode(rx, ry, num, num2, vectorClass.VectorPoint);
                }
                else
                {
                    num4 = Vcode(rx, ry, num, num2, vectorClass2.VectorPoint);
                }
            }
            return true;
        }

        public static bool LineInLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            Vector vector = default;
            vector.X = x3 - x1 + x4 - x2;
            vector.Y = y3 - y1 + y4 - y2;
            Vector vector2 = default;
            vector2.X = x2 - x1;
            vector2.Y = y2 - y1;
            Vector vector3 = default;
            vector3.X = x4 - x3;
            vector3.Y = y4 - y3;
            float value = (vector2.Y * vector3.X) - (vector3.Y * vector2.X);
            float num = (vector3.X * vector.Y) - (vector3.Y * vector.X);
            float value2 = (vector2.X * vector.Y) - (vector2.Y * vector.X);
            return Math.Abs(num) <= Math.Abs(value) && Math.Abs(value2) <= Math.Abs(value);
        }

        public static float FLOAT_RND_RANGE(int S, int F)
        {
            return RND_RANGE(S * 1000, F * 1000) / 1000f;
        }

        public static string GetSHA256Str(string input)
        {
            return GetSHA256(input.GetCharacters());
        }

        public static string GetSHA256(char[] data)
        {
            byte[] array = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                array[i * 2] = (byte)((data[i] & '\uff00') >> 8);
                array[(i * 2) + 1] = (byte)(data[i] & 'ÿ');
            }
            byte[] hash = SHA256.HashData(array);
            return new string(Convert.ToHexString(hash).ToLower(CultureInfo.InvariantCulture));
        }

        public const double M_PI = 3.141592653589793;
        private static readonly Random random_ = new();

        private static readonly long ARC4RANDOM_MAX = 4294967296L;

        private static float[] fmSins;

        private static float[] fmCoss;

        public static readonly Vector vectZero = new(0f, 0f);

        public static readonly Vector vectUndefined = new(2.1474836E+09f, 2.1474836E+09f);
    }
}
