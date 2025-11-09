using CutTheRope.game;
using CutTheRope.iframework.core;
using CutTheRope.ios;
using System;

namespace CutTheRope.iframework.helpers
{
    internal class CTRMathHelper : ResDataPhoneFull
    {
        // (get) Token: 0x0600034D RID: 845 RVA: 0x000137FA File Offset: 0x000119FA
        public static float RND_MINUS1_1
        {
            get
            {
                return (float)(arc4random() / (double)ARC4RANDOM_MAX * 2.0 - 1.0);
            }
        }

        // (get) Token: 0x0600034E RID: 846 RVA: 0x0001381F File Offset: 0x00011A1F
        public static float RND_0_1
        {
            get
            {
                return (float)(arc4random() / (double)ARC4RANDOM_MAX);
            }
        }

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

        public static uint arc4random()
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

        public static float ceil(double value)
        {
            return (float)Math.Ceiling(value);
        }

        public static float round(double value)
        {
            return (float)Math.Round(value);
        }

        public static float cosf(float x)
        {
            return (float)Math.Cos((double)x);
        }

        public static float sinf(float x)
        {
            return (float)Math.Sin((double)x);
        }

        public static float tanf(float x)
        {
            return (float)Math.Tan((double)x);
        }

        public static float acosf(float x)
        {
            return (float)Math.Acos((double)x);
        }

        public static void fmInit()
        {
            if (fmSins == null)
            {
                fmSins = new float[1024];
                for (int i = 0; i < 1024; i++)
                {
                    fmSins[i] = (float)Math.Sin((double)(i * 2) * 3.141592653589793 / 1024.0);
                }
            }
            if (fmCoss == null)
            {
                fmCoss = new float[1024];
                for (int j = 0; j < 1024; j++)
                {
                    fmCoss[j] = (float)Math.Cos((double)(j * 2) * 3.141592653589793 / 1024.0);
                }
            }
        }

        public static float fmSin(float angle)
        {
            int num = (int)((double)(angle * 1024f) / 3.141592653589793 / 2.0);
            num &= 1023;
            return fmSins[num];
        }

        public static float fmCos(float angle)
        {
            int num = (int)((double)(angle * 1024f) / 3.141592653589793 / 2.0);
            num &= 1023;
            return fmCoss[num];
        }

        public static bool sameSign(float a, float b)
        {
            return (a >= 0f && b >= 0f) || (a < 0f && b < 0f);
        }

        public static bool pointInRect(float x, float y, float checkX, float checkY, float checkWidth, float checkHeight)
        {
            return x >= checkX && x < checkX + checkWidth && y >= checkY && y < checkY + checkHeight;
        }

        public static bool rectInRect(float x1l, float y1t, float x1r, float y1b, float x2l, float y2t, float x2r, float y2b)
        {
            return x1l <= x2r && x1r >= x2l && y1t <= y2b && y1b >= y2t;
        }

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
            return overlaps1Way(array, array2) && overlaps1Way(array2, array);
        }

        public static float DEGREES_TO_RADIANS(float D)
        {
            return (float)((double)D * 3.141592653589793 / 180.0);
        }

        public static float RADIANS_TO_DEGREES(float R)
        {
            return (float)((double)(R * 180f) / 3.141592653589793);
        }

        private static bool overlaps1Way(Vector[] corner, Vector[] other)
        {
            Vector[] array = new Vector[2];
            float[] array2 = new float[2];
            array[0] = vectSub(corner[1], corner[0]);
            array[1] = vectSub(corner[3], corner[0]);
            for (int i = 0; i < 2; i++)
            {
                array[i] = vectDiv(array[i], vectLengthsq(array[i]));
                array2[i] = vectDot(corner[0], array[i]);
            }
            for (int j = 0; j < 2; j++)
            {
                float num = vectDot(other[0], array[j]);
                float num2 = num;
                float num3 = num;
                for (int k = 1; k < 4; k++)
                {
                    num = vectDot(other[k], array[j]);
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

        public static CTRRectangle rectInRectIntersection(CTRRectangle r1, CTRRectangle r2)
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

        public static float angleTo0_360(float angle)
        {
            float num = angle;
            while (Math.Abs(num) > 360f)
            {
                num -= (num > 0f) ? 360f : (-360f);
            }
            if (num < 0f)
            {
                num += 360f;
            }
            return num;
        }

        public static Vector vect(float x, float y)
        {
            return new Vector(x, y);
        }

        public static bool vectEqual(Vector v1, Vector v2)
        {
            return v1.x == v2.x && v1.y == v2.y;
        }

        public static Vector vectAdd(Vector v1, Vector v2)
        {
            return new Vector(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector vectNeg(Vector v)
        {
            return new Vector(0f - v.x, 0f - v.y);
        }

        public static Vector vectSub(Vector v1, Vector v2)
        {
            return new Vector(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector vectMult(Vector v, double s)
        {
            return vectMult(v, (float)s);
        }

        public static Vector vectMult(Vector v, float s)
        {
            return new Vector(v.x * s, v.y * s);
        }

        public static Vector vectDiv(Vector v, float s)
        {
            return new Vector(v.x / s, v.y / s);
        }

        public static float vectDot(Vector v1, Vector v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }

        private static float vectCross(Vector v1, Vector v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }

        public static Vector vectPerp(Vector v)
        {
            return new Vector(0f - v.y, v.x);
        }

        public static Vector vectRperp(Vector v)
        {
            return new Vector(v.y, 0f - v.x);
        }

        private static Vector vectProject(Vector v1, Vector v2)
        {
            return vectMult(v2, vectDot(v1, v2) / vectDot(v2, v2));
        }

        private static Vector vectRotateByVector(Vector v1, Vector v2)
        {
            return new Vector(v1.x * v2.x - v1.y * v2.y, v1.x * v2.y + v1.y * v2.x);
        }

        private static Vector vectUnrotateByVector(Vector v1, Vector v2)
        {
            return new Vector(v1.x * v2.x + v1.y * v2.y, v1.y * v2.x - v1.x * v2.y);
        }

        public static float vectAngle(Vector v)
        {
            return (float)Math.Atan((double)(v.y / v.x));
        }

        public static float vectAngleNormalized(Vector v)
        {
            return (float)Math.Atan2((double)v.y, (double)v.x);
        }

        public static float vectLength(Vector v)
        {
            return (float)Math.Sqrt((double)vectDot(v, v));
        }

        public static float vectLengthsq(Vector v)
        {
            return vectDot(v, v);
        }

        public static Vector vectNormalize(Vector v)
        {
            return vectMult(v, 1f / vectLength(v));
        }

        public static Vector vectForAngle(float a)
        {
            return new Vector(fmCos(a), fmSin(a));
        }

        private static float vectToAngle(Vector v)
        {
            return (float)Math.Atan2((double)v.x, (double)v.y);
        }

        public static float vectDistance(Vector v1, Vector v2)
        {
            return vectLength(vectSub(v1, v2));
        }

        public static Vector vectRotate(Vector v, double rad)
        {
            float num = fmCos((float)rad);
            float num2 = fmSin((float)rad);
            float num3 = v.x * num - v.y * num2;
            float yParam = v.x * num2 + v.y * num;
            return new Vector(num3, yParam);
        }

        public static Vector vectRotateAround(Vector v, double rad, float cx, float cy)
        {
            Vector v2 = v;
            v2.x -= cx;
            v2.y -= cy;
            v2 = vectRotate(v2, rad);
            v2.x += cx;
            v2.y += cy;
            return v2;
        }

        private static Vector vectSidePerp(Vector v1, Vector v2)
        {
            return vectNormalize(vectRperp(vectSub(v2, v1)));
        }

        private static int vcode(float x_min, float y_min, float x_max, float y_max, Vector p)
        {
            return ((p.x < x_min) ? 1 : 0) + ((p.x > x_max) ? 2 : 0) + ((p.y < y_min) ? 4 : 0) + ((p.y > y_max) ? 8 : 0);
        }

        public static bool lineInRect(float x1, float y1, float x2, float y2, float rx, float ry, float w, float h)
        {
            VectorClass vectorClass = new(new Vector(x1, y1));
            VectorClass vectorClass2 = new(new Vector(x2, y2));
            float num = rx + w;
            float num2 = ry + h;
            int num3 = vcode(rx, ry, num, num2, vectorClass.v);
            int num4 = vcode(rx, ry, num, num2, vectorClass2.v);
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
                    num3 = vcode(rx, ry, num, num2, vectorClass.v);
                }
                else
                {
                    num4 = vcode(rx, ry, num, num2, vectorClass2.v);
                }
            }
            return true;
        }

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

        public static float FLOAT_RND_RANGE(int S, int F)
        {
            return (float)RND_RANGE(S * 1000, F * 1000) / 1000f;
        }

        public static NSString getMD5Str(NSString input)
        {
            return getMD5(input.getCharacters());
        }

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

        private const int fmSinCosSize = 1024;

        public const double M_PI = 3.141592653589793;

        private const int COHEN_LEFT = 1;

        private const int COHEN_RIGHT = 2;

        private const int COHEN_BOT = 4;

        private const int COHEN_TOP = 8;

        private static Random random_ = new();

        private static long ARC4RANDOM_MAX = 4294967296L;

        private static float[] fmSins;

        private static float[] fmCoss;

        public static readonly Vector vectZero = new(0f, 0f);

        public static readonly Vector vectUndefined = new(2.1474836E+09f, 2.1474836E+09f);
    }
}
