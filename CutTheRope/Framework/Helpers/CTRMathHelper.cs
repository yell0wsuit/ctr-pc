using System;
using System.Globalization;
using System.Security.Cryptography;

using CutTheRope.Framework.Core;
using CutTheRope.Helpers;

namespace CutTheRope.Framework.Helpers
{
    internal class CTRMathHelper
    {
        /// <summary>Random float in the range [-1, 1].</summary>
        public static float RND_MINUS1_1 => ((float)Arc4random() / ARC4RANDOM_MAX * 2f) - 1f;

        /// <summary>Random float in the range [0, 1].</summary>
        public static float RND_0_1 => (float)Arc4random() / ARC4RANDOM_MAX;

        /// <summary>Returns the smaller of two integers.</summary>
        public static int MIN(int a, int b)
        {
            return Math.Min(a, b);
        }

        /// <summary>Returns the smaller of two floats.</summary>
        public static float MIN(float a, float b)
        {
            return Math.Min(a, b);
        }

        /// <summary>Returns the larger of two integers.</summary>
        public static int MAX(int a, int b)
        {
            return Math.Max(a, b);
        }

        /// <summary>Returns the larger of two floats.</summary>
        public static float MAX(float a, float b)
        {
            return Math.Max(a, b);
        }

        /// <summary>Returns the absolute value of a float.</summary>
        public static float ABS(float a)
        {
            return Math.Abs(a);
        }

        /// <summary>Returns a random integer in the range [0, n].</summary>
        public static int RND(int n)
        {
            return RND_RANGE(0, n);
        }

        /// <summary>Returns a random integer in the range [n, m].</summary>
        public static int RND_RANGE(int n, int m)
        {
            return random_.Next(n, m + 1);
        }

        /// <summary>Returns a random uint, wrapping <see cref="Random"/>.</summary>
        public static uint Arc4random()
        {
            return (uint)random_.Next(int.MinValue, int.MaxValue);
        }

        /// <summary>Clamps <paramref name="V"/> to the range [<paramref name="MINV"/>, <paramref name="MAXV"/>].</summary>
        public static float FIT_TO_BOUNDARIES(float V, float MINV, float MAXV)
        {
            return Math.Max(Math.Min(V, MAXV), MINV);
        }

        /// <summary>Returns the ceiling of <paramref name="value"/> as a float.</summary>
        public static float Ceil(double value)
        {
            return (float)Math.Ceiling(value);
        }

        /// <summary>Returns <paramref name="value"/> rounded to the nearest integer as a float.</summary>
        public static float Round(double value)
        {
            return (float)Math.Round(value);
        }

        /// <summary>Returns the cosine of <paramref name="x"/> (radians) as a float.</summary>
        public static float Cosf(float x)
        {
            return (float)Math.Cos((double)x);
        }

        /// <summary>Returns the sine of <paramref name="x"/> (radians) as a float.</summary>
        public static float Sinf(float x)
        {
            return (float)Math.Sin((double)x);
        }

        /// <summary>Returns the tangent of <paramref name="x"/> (radians) as a float.</summary>
        public static float Tanf(float x)
        {
            return (float)Math.Tan((double)x);
        }

        /// <summary>Returns the arccosine of <paramref name="x"/> in radians as a float.</summary>
        public static float Acosf(float x)
        {
            return (float)Math.Acos((double)x);
        }

        /// <summary>
        /// Initializes the fast-math sine and cosine lookup tables.
        /// Must be called before using <see cref="FmSin"/> or <see cref="FmCos"/>.
        /// </summary>
        public static void FmInit()
        {
            if (fmSins == null)
            {
                fmSins = new float[FM_TRIG_TABLE_SIZE];
                for (int i = 0; i < FM_TRIG_TABLE_SIZE; i++)
                {
                    fmSins[i] = MathF.Sin(i * 2 * MathF.PI / FM_TRIG_TABLE_SIZE);
                }
            }
            if (fmCoss == null)
            {
                fmCoss = new float[FM_TRIG_TABLE_SIZE];
                for (int j = 0; j < FM_TRIG_TABLE_SIZE; j++)
                {
                    fmCoss[j] = MathF.Cos(j * 2 * MathF.PI / FM_TRIG_TABLE_SIZE);
                }
            }
        }

        /// <summary>
        /// Fast table-based sine. Quantizes <paramref name="angle"/> (radians) to
        /// <see cref="FM_TRIG_TABLE_SIZE"/> steps.
        /// </summary>
        public static float FmSin(float angle)
        {
            int index = (int)(angle * FM_TRIG_TABLE_SIZE / Math.Tau);
            index &= FM_TRIG_TABLE_MASK;
            return fmSins[index];
        }

        /// <summary>
        /// Fast table-based cosine. Quantizes <paramref name="angle"/> (radians) to
        /// <see cref="FM_TRIG_TABLE_SIZE"/> steps.
        /// </summary>
        public static float FmCos(float angle)
        {
            int index = (int)(angle * FM_TRIG_TABLE_SIZE / Math.Tau);
            index &= FM_TRIG_TABLE_MASK;
            return fmCoss[index];
        }

        /// <summary>Returns <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> have the same sign (both ≥ 0 or both &lt; 0).</summary>
        public static bool SameSign(float a, float b)
        {
            return (a >= 0f && b >= 0f) || (a < 0f && b < 0f);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the point (<paramref name="x"/>, <paramref name="y"/>) lies
        /// within the axis-aligned rectangle defined by its top-left corner, width, and height.
        /// </summary>
        public static bool PointInRect(float x, float y, float checkX, float checkY, float checkWidth, float checkHeight)
        {
            return x >= checkX && x < checkX + checkWidth && y >= checkY && y < checkY + checkHeight;
        }

        /// <summary>
        /// Returns <see langword="true"/> if two axis-aligned rectangles overlap.
        /// Each rectangle is supplied as its left, top, right, and bottom edges.
        /// </summary>
        public static bool RectInRect(float x1l, float y1t, float x1r, float y1b, float x2l, float y2t, float x2r, float y2b)
        {
            return x1l <= x2r && x1r >= x2l && y1t <= y2b && y1b >= y2t;
        }

        /// <summary>
        /// Tests whether two oriented bounding boxes (OBBs) overlap using the separating axis theorem.
        /// Each OBB is described by its four corner vertices in order: top-left, top-right, bottom-right, bottom-left.
        /// </summary>
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

        /// <summary>Converts degrees to radians.</summary>
        public static float DEGREES_TO_RADIANS(float D)
        {
            return D * MathF.PI / DEG_180;
        }

        /// <summary>Converts radians to degrees.</summary>
        public static float RADIANS_TO_DEGREES(float R)
        {
            return R * DEG_180 / MathF.PI;
        }

        /// <summary>
        /// Returns <see langword="true"/> if all corners of <paramref name="other"/> project onto
        /// the axes of <paramref name="corner"/> within the box's own extents (one-way SAT overlap test).
        /// </summary>
        private static bool Overlaps1Way(Vector[] corner, Vector[] other)
        {
            Vector[] axes = new Vector[2];
            float[] origins = new float[2];
            axes[0] = VectSub(corner[1], corner[0]);
            axes[1] = VectSub(corner[3], corner[0]);
            for (int i = 0; i < 2; i++)
            {
                axes[i] = VectDiv(axes[i], VectLengthsq(axes[i]));
                origins[i] = VectDot(corner[0], axes[i]);
            }
            for (int j = 0; j < 2; j++)
            {
                float proj = VectDot(other[0], axes[j]);
                float projMin = proj;
                float projMax = proj;
                for (int k = 1; k < 4; k++)
                {
                    proj = VectDot(other[k], axes[j]);
                    if (proj < projMin)
                    {
                        projMin = proj;
                    }
                    else if (proj > projMax)
                    {
                        projMax = proj;
                    }
                }
                if (projMin > 1f + origins[j] || projMax < origins[j])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the intersection of <paramref name="r2"/> clipped to <paramref name="r1"/>,
        /// with coordinates expressed relative to <paramref name="r1"/>'s origin.
        /// </summary>
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

        /// <summary>Normalizes an angle in degrees to the range [0, 360).</summary>
        public static float AngleTo0_360(float angle)
        {
            float result = angle;
            while (Math.Abs(result) > DEG_360)
            {
                result -= result > 0f ? DEG_360 : -DEG_360;
            }
            if (result < 0f)
            {
                result += DEG_360;
            }
            return result;
        }

        /// <summary>Creates a <see cref="Vector"/> from the given x and y components.</summary>
        public static Vector Vect(float x, float y)
        {
            return new Vector(x, y);
        }

        /// <summary>Returns <see langword="true"/> if both components of <paramref name="v1"/> and <paramref name="v2"/> are equal.</summary>
        public static bool VectEqual(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        /// <summary>Returns the component-wise sum of <paramref name="v1"/> and <paramref name="v2"/>.</summary>
        public static Vector VectAdd(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        /// <summary>Returns the negation of <paramref name="v"/>.</summary>
        public static Vector VectNeg(Vector v)
        {
            return new Vector(0f - v.X, 0f - v.Y);
        }

        /// <summary>Returns the component-wise difference <paramref name="v1"/> − <paramref name="v2"/>.</summary>
        public static Vector VectSub(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        /// <summary>Returns <paramref name="v"/> scaled by scalar <paramref name="s"/>.</summary>
        public static Vector VectMult(Vector v, float s)
        {
            return new Vector(v.X * s, v.Y * s);
        }

        /// <summary>Returns <paramref name="v"/> divided by scalar <paramref name="s"/>.</summary>
        public static Vector VectDiv(Vector v, float s)
        {
            return new Vector(v.X / s, v.Y / s);
        }

        /// <summary>Returns the dot product of <paramref name="v1"/> and <paramref name="v2"/>.</summary>
        public static float VectDot(Vector v1, Vector v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y);
        }

        /// <summary>Returns the left perpendicular of <paramref name="v"/>: (-y, x).</summary>
        public static Vector VectPerp(Vector v)
        {
            return new Vector(0f - v.Y, v.X);
        }

        /// <summary>Returns the right perpendicular of <paramref name="v"/>: (y, -x).</summary>
        public static Vector VectRperp(Vector v)
        {
            return new Vector(v.Y, 0f - v.X);
        }

        /// <summary>
        /// Returns the angle of <paramref name="v"/> in radians using <c>atan(y/x)</c>.
        /// Prefer <see cref="VectAngleNormalized"/> to handle all quadrants correctly.
        /// </summary>
        public static float VectAngle(Vector v)
        {
            return MathF.Atan(v.Y / v.X);
        }

        /// <summary>Returns the angle of <paramref name="v"/> in radians using <c>atan2(y, x)</c>, covering all quadrants.</summary>
        public static float VectAngleNormalized(Vector v)
        {
            return MathF.Atan2(v.Y, v.X);
        }

        /// <summary>Returns the magnitude (Euclidean length) of <paramref name="v"/>.</summary>
        public static float VectLength(Vector v)
        {
            return MathF.Sqrt(VectDot(v, v));
        }

        /// <summary>Returns the squared magnitude of <paramref name="v"/>. Cheaper than <see cref="VectLength"/> when only relative comparisons are needed.</summary>
        public static float VectLengthsq(Vector v)
        {
            return VectDot(v, v);
        }

        /// <summary>Returns a unit vector in the same direction as <paramref name="v"/>.</summary>
        public static Vector VectNormalize(Vector v)
        {
            return VectMult(v, 1f / VectLength(v));
        }

        /// <summary>Returns a unit vector pointing in the direction of angle <paramref name="a"/> (radians).</summary>
        public static Vector VectForAngle(float a)
        {
            return new Vector(FmCos(a), FmSin(a));
        }

        /// <summary>Returns the Euclidean distance between <paramref name="v1"/> and <paramref name="v2"/>.</summary>
        public static float VectDistance(Vector v1, Vector v2)
        {
            return VectLength(VectSub(v1, v2));
        }

        /// <summary>Rotates <paramref name="v"/> by <paramref name="rad"/> radians around the origin.</summary>
        public static Vector VectRotate(Vector v, double rad)
        {
            float cosA = FmCos((float)rad);
            float sinA = FmSin((float)rad);
            float nx = (v.X * cosA) - (v.Y * sinA);
            float ny = (v.X * sinA) + (v.Y * cosA);
            return new Vector(nx, ny);
        }

        /// <summary>Rotates <paramref name="v"/> by <paramref name="rad"/> radians around the point (<paramref name="cx"/>, <paramref name="cy"/>).</summary>
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

        /// <summary>
        /// Computes the Cohen–Sutherland outcode for point <paramref name="p"/> relative to
        /// the axis-aligned rectangle [<paramref name="x_min"/>, <paramref name="x_max"/>] × [<paramref name="y_min"/>, <paramref name="y_max"/>].
        /// </summary>
        private static int Vcode(float x_min, float y_min, float x_max, float y_max, Vector p)
        {
            return (p.X < x_min ? COHEN_LEFT : 0) + (p.X > x_max ? COHEN_RIGHT : 0) + (p.Y < y_min ? COHEN_BOT : 0) + (p.Y > y_max ? COHEN_TOP : 0);
        }

        /// <summary>
        /// Tests whether the line segment from (<paramref name="x1"/>, <paramref name="y1"/>) to
        /// (<paramref name="x2"/>, <paramref name="y2"/>) intersects the axis-aligned rectangle
        /// at (<paramref name="rx"/>, <paramref name="ry"/>) with dimensions <paramref name="w"/> × <paramref name="h"/>,
        /// using the Cohen–Sutherland clipping algorithm.
        /// </summary>
        public static bool LineInRect(float x1, float y1, float x2, float y2, float rx, float ry, float w, float h)
        {
            VectorClass a = new(new Vector(x1, y1));
            VectorClass b = new(new Vector(x2, y2));
            float xMax = rx + w;
            float yMax = ry + h;
            int codeA = Vcode(rx, ry, xMax, yMax, a.VectorPoint);
            int codeB = Vcode(rx, ry, xMax, yMax, b.VectorPoint);
            while (codeA != 0 || codeB != 0)
            {
                if ((codeA & codeB) != 0)
                {
                    return false;
                }
                int code;
                VectorClass current;
                if (codeA != 0)
                {
                    code = codeA;
                    current = a;
                }
                else
                {
                    code = codeB;
                    current = b;
                }
                if ((code & COHEN_LEFT) != 0)
                {
                    Vector temp = current.VectorPoint;
                    temp.Y += (y1 - y2) * (rx - temp.X) / (x1 - x2);
                    temp.X = rx;
                    current.VectorPoint = temp;
                }
                else if ((code & COHEN_RIGHT) != 0)
                {
                    Vector temp = current.VectorPoint;
                    temp.Y += (y1 - y2) * (xMax - temp.X) / (x1 - x2);
                    temp.X = xMax;
                    current.VectorPoint = temp;
                }
                if ((code & COHEN_BOT) != 0)
                {
                    Vector temp = current.VectorPoint;
                    temp.X += (x1 - x2) * (ry - temp.Y) / (y1 - y2);
                    temp.Y = ry;
                    current.VectorPoint = temp;
                }
                else if ((code & COHEN_TOP) != 0)
                {
                    Vector temp = current.VectorPoint;
                    temp.X += (x1 - x2) * (yMax - temp.Y) / (y1 - y2);
                    temp.Y = yMax;
                    current.VectorPoint = temp;
                }
                if (code == codeA)
                {
                    codeA = Vcode(rx, ry, xMax, yMax, a.VectorPoint);
                }
                else
                {
                    codeB = Vcode(rx, ry, xMax, yMax, b.VectorPoint);
                }
            }
            return true;
        }

        /// <summary>
        /// Tests whether two line segments intersect: segment 1 from
        /// (<paramref name="x1"/>, <paramref name="y1"/>) to (<paramref name="x2"/>, <paramref name="y2"/>) and
        /// segment 2 from (<paramref name="x3"/>, <paramref name="y3"/>) to (<paramref name="x4"/>, <paramref name="y4"/>).
        /// </summary>
        public static bool LineInLine(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            Vector dp = default;
            dp.X = x3 - x1 + x4 - x2;
            dp.Y = y3 - y1 + y4 - y2;
            Vector qa = default;
            qa.X = x2 - x1;
            qa.Y = y2 - y1;
            Vector qb = default;
            qb.X = x4 - x3;
            qb.Y = y4 - y3;
            float d = (qa.Y * qb.X) - (qb.Y * qa.X);
            float la = (qb.X * dp.Y) - (qb.Y * dp.X);
            float lb = (qa.X * dp.Y) - (qa.Y * dp.X);
            return Math.Abs(la) <= Math.Abs(d) && Math.Abs(lb) <= Math.Abs(d);
        }

        /// <summary>
        /// Returns a random float in the range [<paramref name="S"/>, <paramref name="F"/>],
        /// with precision to three decimal places.
        /// </summary>
        public static float FLOAT_RND_RANGE(int S, int F)
        {
            return RND_RANGE(S * FLOAT_RANDOM_SCALE, F * FLOAT_RANDOM_SCALE) / FLOAT_RANDOM_SCALE;
        }

        /// <summary>Returns the lowercase hex SHA-256 hash of <paramref name="input"/>.</summary>
        public static string GetSHA256Str(string input)
        {
            return GetSHA256(input.GetCharacters());
        }

        /// <summary>Returns the lowercase hex SHA-256 hash of a UTF-16 char array.</summary>
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

        public const float DEG_45 = 45f;
        public const float DEG_90 = 90f;
        public const float DEG_180 = 180f;
        public const float DEG_270 = 270f;
        public const float DEG_360 = 360f;
        public const float UNDEFINED_COORDINATE = int.MaxValue;

        private static readonly Random random_ = new();

        private static readonly long ARC4RANDOM_MAX = 4294967296L;

        private static float[] fmSins;

        private static float[] fmCoss;

        private const int FM_TRIG_TABLE_SIZE = 1024;
        private const int FM_TRIG_TABLE_MASK = FM_TRIG_TABLE_SIZE - 1;
        private const int FLOAT_RANDOM_SCALE = 1000;

        private const int COHEN_LEFT = 1;
        private const int COHEN_RIGHT = 2;
        private const int COHEN_BOT = 4;
        private const int COHEN_TOP = 8;

        public static readonly Vector vectZero = new(0f, 0f);

        public static readonly Vector vectUndefined = new(UNDEFINED_COORDINATE, UNDEFINED_COORDINATE);
    }
}
