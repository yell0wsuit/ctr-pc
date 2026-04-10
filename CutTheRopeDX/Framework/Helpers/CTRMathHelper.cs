using System;
using System.Globalization;
using System.Security.Cryptography;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.Framework.Helpers
{
    /// <summary>
    /// Provides math utilities, vector operations, random number generation, collision tests, and fast trigonometry lookups.
    /// </summary>
    internal class CTRMathHelper
    {
        /// <summary>Random float in the range [-1, 1].</summary>
        public static float RND_MINUS1_1 => ((float)Arc4random() / ARC4RANDOM_MAX * 2f) - 1f;

        /// <summary>Random float in the range [0, 1].</summary>
        public static float RND_0_1 => (float)Arc4random() / ARC4RANDOM_MAX;

        /// <summary>Returns the smaller of two integers.</summary>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <returns>The smaller of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static int MIN(int a, int b)
        {
            return Math.Min(a, b);
        }

        /// <summary>Returns the smaller of two floats.</summary>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <returns>The smaller of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static float MIN(float a, float b)
        {
            return MathF.Min(a, b);
        }

        /// <summary>Returns the larger of two integers.</summary>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <returns>The larger of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static int MAX(int a, int b)
        {
            return Math.Max(a, b);
        }

        /// <summary>Returns the larger of two floats.</summary>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <returns>The larger of <paramref name="a"/> and <paramref name="b"/>.</returns>
        public static float MAX(float a, float b)
        {
            return MathF.Max(a, b);
        }

        /// <summary>Returns the absolute value of a float.</summary>
        /// <param name="a">The input value.</param>
        /// <returns>The absolute value of <paramref name="a"/>.</returns>
        public static float ABS(float a)
        {
            return MathF.Abs(a);
        }

        /// <summary>Returns a random integer in the range [0, n].</summary>
        /// <param name="n">Upper bound (inclusive).</param>
        /// <returns>A random integer between 0 and <paramref name="n"/>.</returns>
        public static int RND(int n)
        {
            return RND_RANGE(0, n);
        }

        /// <summary>Returns a random integer in the range [n, m].</summary>
        /// <param name="n">Lower bound (inclusive).</param>
        /// <param name="m">Upper bound (inclusive).</param>
        /// <returns>A random integer between <paramref name="n"/> and <paramref name="m"/>.</returns>
        public static int RND_RANGE(int n, int m)
        {
            return random_.Next(n, m + 1);
        }

        /// <summary>Returns a random uint, wrapping <see cref="Random"/>.</summary>
        /// <returns>A random unsigned integer.</returns>
        public static uint Arc4random()
        {
            return (uint)random_.Next(int.MinValue, int.MaxValue);
        }

        /// <summary>Clamps <paramref name="V"/> to the range [<paramref name="MINV"/>, <paramref name="MAXV"/>].</summary>
        /// <param name="V">The value to clamp.</param>
        /// <param name="MINV">Minimum bound.</param>
        /// <param name="MAXV">Maximum bound.</param>
        /// <returns>The clamped value.</returns>
        public static float FIT_TO_BOUNDARIES(float V, float MINV, float MAXV)
        {
            return MathF.Max(MathF.Min(V, MAXV), MINV);
        }

        /// <summary>Returns the ceiling of <paramref name="value"/> as a float.</summary>
        /// <param name="value">The input value.</param>
        /// <returns>The smallest integer greater than or equal to <paramref name="value"/>.</returns>
        public static float Ceil(float value)
        {
            return MathF.Ceiling(value);
        }

        /// <summary>Returns <paramref name="value"/> rounded to the nearest integer as a float.</summary>
        /// <param name="value">The input value.</param>
        /// <returns>The rounded <paramref name="value"/>.</returns>
        public static float Round(float value)
        {
            return MathF.Round(value);
        }

        /// <summary>Returns the cosine of <paramref name="x"/> (radians) as a float.</summary>
        /// <param name="x">Angle in radians.</param>
        /// <returns>The cosine of <paramref name="x"/>.</returns>
        public static float Cosf(float x)
        {
            return MathF.Cos(x);
        }

        /// <summary>Returns the sine of <paramref name="x"/> (radians) as a float.</summary>
        /// <param name="x">Angle in radians.</param>
        /// <returns>The sine of <paramref name="x"/>.</returns>
        public static float Sinf(float x)
        {
            return MathF.Sin(x);
        }

        /// <summary>Returns the tangent of <paramref name="x"/> (radians) as a float.</summary>
        /// <param name="x">Angle in radians.</param>
        /// <returns>The tangent of <paramref name="x"/>.</returns>
        public static float Tanf(float x)
        {
            return MathF.Tan(x);
        }

        /// <summary>Returns the arccosine of <paramref name="x"/> in radians as a float.</summary>
        /// <param name="x">Value in the range [-1, 1].</param>
        /// <returns>The arccosine of <paramref name="x"/> in radians.</returns>
        public static float Acosf(float x)
        {
            return MathF.Acos(x);
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
        /// <param name="angle">Angle in radians.</param>
        /// <returns>The approximate sine of <paramref name="angle"/>.</returns>
        public static float FmSin(float angle)
        {
            int index = (int)(angle * FM_TRIG_TABLE_SIZE / MathF.Tau);
            index &= FM_TRIG_TABLE_MASK;
            return fmSins[index];
        }

        /// <summary>
        /// Fast table-based cosine. Quantizes <paramref name="angle"/> (radians) to
        /// <see cref="FM_TRIG_TABLE_SIZE"/> steps.
        /// </summary>
        /// <param name="angle">Angle in radians.</param>
        /// <returns>The approximate cosine of <paramref name="angle"/>.</returns>
        public static float FmCos(float angle)
        {
            int index = (int)(angle * FM_TRIG_TABLE_SIZE / MathF.Tau);
            index &= FM_TRIG_TABLE_MASK;
            return fmCoss[index];
        }

        /// <summary>Returns <see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> have the same sign (both ≥ 0 or both &lt; 0).</summary>
        /// <param name="a">First value.</param>
        /// <param name="b">Second value.</param>
        /// <returns><see langword="true"/> if both values share the same sign.</returns>
        public static bool SameSign(float a, float b)
        {
            return (a >= 0f && b >= 0f) || (a < 0f && b < 0f);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the point (<paramref name="x"/>, <paramref name="y"/>) lies
        /// within the axis-aligned rectangle defined by its top-left corner, width, and height.
        /// </summary>
        /// <param name="x">Point X coordinate.</param>
        /// <param name="y">Point Y coordinate.</param>
        /// <param name="checkX">Rectangle left edge.</param>
        /// <param name="checkY">Rectangle top edge.</param>
        /// <param name="checkWidth">Rectangle width.</param>
        /// <param name="checkHeight">Rectangle height.</param>
        /// <returns><see langword="true"/> if the point is inside the rectangle.</returns>
        public static bool PointInRect(float x, float y, float checkX, float checkY, float checkWidth, float checkHeight)
        {
            return x >= checkX && x < checkX + checkWidth && y >= checkY && y < checkY + checkHeight;
        }

        /// <summary>
        /// Returns <see langword="true"/> if two axis-aligned rectangles overlap.
        /// Each rectangle is supplied as its left, top, right, and bottom edges.
        /// </summary>
        /// <param name="x1l">First rectangle left edge.</param>
        /// <param name="y1t">First rectangle top edge.</param>
        /// <param name="x1r">First rectangle right edge.</param>
        /// <param name="y1b">First rectangle bottom edge.</param>
        /// <param name="x2l">Second rectangle left edge.</param>
        /// <param name="y2t">Second rectangle top edge.</param>
        /// <param name="x2r">Second rectangle right edge.</param>
        /// <param name="y2b">Second rectangle bottom edge.</param>
        /// <returns><see langword="true"/> if the rectangles overlap.</returns>
        public static bool RectInRect(float x1l, float y1t, float x1r, float y1b, float x2l, float y2t, float x2r, float y2b)
        {
            return x1l <= x2r && x1r >= x2l && y1t <= y2b && y1b >= y2t;
        }

        /// <summary>
        /// Tests whether two oriented bounding boxes (OBBs) overlap using the separating axis theorem.
        /// Each OBB is described by its four corner vertices in order: top-left, top-right, bottom-right, bottom-left.
        /// </summary>
        /// <param name="tl1">First OBB top-left corner.</param>
        /// <param name="tr1">First OBB top-right corner.</param>
        /// <param name="br1">First OBB bottom-right corner.</param>
        /// <param name="bl1">First OBB bottom-left corner.</param>
        /// <param name="tl2">Second OBB top-left corner.</param>
        /// <param name="tr2">Second OBB top-right corner.</param>
        /// <param name="br2">Second OBB bottom-right corner.</param>
        /// <param name="bl2">Second OBB bottom-left corner.</param>
        /// <returns><see langword="true"/> if the two OBBs overlap.</returns>
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
        /// <param name="D">Angle in degrees.</param>
        /// <returns>Angle in radians.</returns>
        public static float DEGREES_TO_RADIANS(float D)
        {
            return D * MathF.PI / DEG_180;
        }

        /// <summary>Converts radians to degrees.</summary>
        /// <param name="R">Angle in radians.</param>
        /// <returns>Angle in degrees.</returns>
        public static float RADIANS_TO_DEGREES(float R)
        {
            return R * DEG_180 / MathF.PI;
        }

        /// <summary>
        /// Returns <see langword="true"/> if all corners of <paramref name="other"/> project onto
        /// the axes of <paramref name="corner"/> within the box's own extents (one-way SAT overlap test).
        /// </summary>
        /// <param name="corner">The four corners of the reference OBB.</param>
        /// <param name="other">The four corners of the OBB being tested.</param>
        /// <returns><see langword="true"/> if all projections overlap on the reference axes.</returns>
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
        /// <param name="r1">The clipping rectangle.</param>
        /// <param name="r2">The rectangle to clip.</param>
        /// <returns>The intersection rectangle relative to <paramref name="r1"/>.</returns>
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

        /// <summary>Normalizes an <paramref name="angle"/> in degrees to the range [0, 360).</summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>The normalized <paramref name="angle"/>.</returns>
        public static float AngleTo0_360(float angle)
        {
            float result = angle;
            while (MathF.Abs(result) > DEG_360)
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
        /// <param name="x">The X component.</param>
        /// <param name="y">The Y component.</param>
        /// <returns>A new vector with the specified components.</returns>
        public static Vector Vect(float x, float y)
        {
            return new Vector(x, y);
        }

        /// <summary>Returns <see langword="true"/> if both components of <paramref name="v1"/> and <paramref name="v2"/> are equal.</summary>
        /// <param name="v1">First vector.</param>
        /// <param name="v2">Second vector.</param>
        /// <returns><see langword="true"/> if the vectors have identical components.</returns>
        public static bool VectEqual(Vector v1, Vector v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }

        /// <summary>Returns the component-wise sum of <paramref name="v1"/> and <paramref name="v2"/>.</summary>
        /// <param name="v1">First vector.</param>
        /// <param name="v2">Second vector.</param>
        /// <returns>The sum of the two vectors.</returns>
        public static Vector VectAdd(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        /// <summary>Returns the negation of <paramref name="v"/>.</summary>
        /// <param name="v">The vector to negate.</param>
        /// <returns>The negated vector.</returns>
        public static Vector VectNeg(Vector v)
        {
            return new Vector(0f - v.X, 0f - v.Y);
        }

        /// <summary>Returns the component-wise difference <paramref name="v1"/> − <paramref name="v2"/>.</summary>
        /// <param name="v1">Minuend vector.</param>
        /// <param name="v2">Subtrahend vector.</param>
        /// <returns>The difference of the two vectors.</returns>
        public static Vector VectSub(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        /// <summary>Returns <paramref name="v"/> scaled by scalar <paramref name="s"/>.</summary>
        /// <param name="v">The vector to scale.</param>
        /// <param name="s">The scalar multiplier.</param>
        /// <returns>The scaled vector.</returns>
        public static Vector VectMult(Vector v, float s)
        {
            return new Vector(v.X * s, v.Y * s);
        }

        /// <summary>Returns <paramref name="v"/> divided by scalar <paramref name="s"/>.</summary>
        /// <param name="v">The vector to divide.</param>
        /// <param name="s">The scalar divisor.</param>
        /// <returns>The divided vector.</returns>
        public static Vector VectDiv(Vector v, float s)
        {
            return new Vector(v.X / s, v.Y / s);
        }

        /// <summary>Returns the dot product of <paramref name="v1"/> and <paramref name="v2"/>.</summary>
        /// <param name="v1">First vector.</param>
        /// <param name="v2">Second vector.</param>
        /// <returns>The dot product of the two vectors.</returns>
        public static float VectDot(Vector v1, Vector v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y);
        }

        /// <summary>Returns the left perpendicular of <paramref name="v"/>: (-y, x).</summary>
        /// <param name="v">The input vector.</param>
        /// <returns>The left perpendicular vector.</returns>
        public static Vector VectPerp(Vector v)
        {
            return new Vector(0f - v.Y, v.X);
        }

        /// <summary>Returns the right perpendicular of <paramref name="v"/>: (y, -x).</summary>
        /// <param name="v">The input vector.</param>
        /// <returns>The right perpendicular vector.</returns>
        public static Vector VectRperp(Vector v)
        {
            return new Vector(v.Y, 0f - v.X);
        }

        /// <summary>
        /// Returns the angle of <paramref name="v"/> in radians using <c>atan(y/x)</c>.
        /// Prefer <see cref="VectAngleNormalized"/> to handle all quadrants correctly.
        /// </summary>
        /// <param name="v">The input vector.</param>
        /// <returns>The angle in radians.</returns>
        public static float VectAngle(Vector v)
        {
            return MathF.Atan(v.Y / v.X);
        }

        /// <summary>Returns the angle of <paramref name="v"/> in radians using <c>atan2(y, x)</c>, covering all quadrants.</summary>
        /// <param name="v">The input vector.</param>
        /// <returns>The angle in radians in the range [-π, π].</returns>
        public static float VectAngleNormalized(Vector v)
        {
            return MathF.Atan2(v.Y, v.X);
        }

        /// <summary>Returns the magnitude (Euclidean length) of <paramref name="v"/>.</summary>
        /// <param name="v">The input vector.</param>
        /// <returns>The length of the vector.</returns>
        public static float VectLength(Vector v)
        {
            return MathF.Sqrt(VectDot(v, v));
        }

        /// <summary>Returns the squared magnitude of <paramref name="v"/>. Cheaper than <see cref="VectLength"/> when only relative comparisons are needed.</summary>
        /// <param name="v">The input vector.</param>
        /// <returns>The squared length of the vector.</returns>
        public static float VectLengthsq(Vector v)
        {
            return VectDot(v, v);
        }

        /// <summary>Returns a unit vector in the same direction as <paramref name="v"/>.</summary>
        /// <param name="v">The vector to normalize.</param>
        /// <returns>A unit vector in the direction of <paramref name="v"/>.</returns>
        public static Vector VectNormalize(Vector v)
        {
            return VectMult(v, 1f / VectLength(v));
        }

        /// <summary>Returns a unit vector pointing in the direction of angle <paramref name="a"/> (radians).</summary>
        /// <param name="a">Angle in radians.</param>
        /// <returns>A unit vector pointing at the specified angle.</returns>
        public static Vector VectForAngle(float a)
        {
            return new Vector(FmCos(a), FmSin(a));
        }

        /// <summary>Returns the Euclidean distance between <paramref name="v1"/> and <paramref name="v2"/>.</summary>
        /// <param name="v1">First point.</param>
        /// <param name="v2">Second point.</param>
        /// <returns>The distance between the two points.</returns>
        public static float VectDistance(Vector v1, Vector v2)
        {
            return VectLength(VectSub(v1, v2));
        }

        /// <summary>Rotates <paramref name="v"/> by <paramref name="rad"/> radians around the origin.</summary>
        /// <param name="v">The vector to rotate.</param>
        /// <param name="rad">Rotation angle in radians.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector VectRotate(Vector v, float rad)
        {
            float cosA = FmCos(rad);
            float sinA = FmSin(rad);
            float nx = (v.X * cosA) - (v.Y * sinA);
            float ny = (v.X * sinA) + (v.Y * cosA);
            return new Vector(nx, ny);
        }

        /// <summary>Rotates <paramref name="v"/> by <paramref name="rad"/> radians around the point (<paramref name="cx"/>, <paramref name="cy"/>).</summary>
        /// <param name="v">The vector to rotate.</param>
        /// <param name="rad">Rotation angle in radians.</param>
        /// <param name="cx">Pivot point X coordinate.</param>
        /// <param name="cy">Pivot point Y coordinate.</param>
        /// <returns>The rotated vector.</returns>
        public static Vector VectRotateAround(Vector v, float rad, float cx, float cy)
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
        /// <param name="x_min">Rectangle minimum X.</param>
        /// <param name="y_min">Rectangle minimum Y.</param>
        /// <param name="x_max">Rectangle maximum X.</param>
        /// <param name="y_max">Rectangle maximum Y.</param>
        /// <param name="p">The point to classify.</param>
        /// <returns>A bitmask of <c>COHEN_*</c> region codes.</returns>
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
        /// <param name="x1">Segment start X.</param>
        /// <param name="y1">Segment start Y.</param>
        /// <param name="x2">Segment end X.</param>
        /// <param name="y2">Segment end Y.</param>
        /// <param name="rx">Rectangle left edge.</param>
        /// <param name="ry">Rectangle top edge.</param>
        /// <param name="w">Rectangle width.</param>
        /// <param name="h">Rectangle height.</param>
        /// <returns><see langword="true"/> if the segment intersects the rectangle.</returns>
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
        /// <param name="x1">Segment 1 start X.</param>
        /// <param name="y1">Segment 1 start Y.</param>
        /// <param name="x2">Segment 1 end X.</param>
        /// <param name="y2">Segment 1 end Y.</param>
        /// <param name="x3">Segment 2 start X.</param>
        /// <param name="y3">Segment 2 start Y.</param>
        /// <param name="x4">Segment 2 end X.</param>
        /// <param name="y4">Segment 2 end Y.</param>
        /// <returns><see langword="true"/> if the segments intersect.</returns>
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
            return MathF.Abs(la) <= MathF.Abs(d) && MathF.Abs(lb) <= MathF.Abs(d);
        }

        /// <summary>
        /// Returns a random float in the range [<paramref name="S"/>, <paramref name="F"/>],
        /// with precision to three decimal places.
        /// </summary>
        /// <param name="S">Lower bound (inclusive).</param>
        /// <param name="F">Upper bound (inclusive).</param>
        /// <returns>A random float between <paramref name="S"/> and <paramref name="F"/>.</returns>
        public static float FLOAT_RND_RANGE(int S, int F)
        {
            return RND_RANGE(S * FLOAT_RANDOM_SCALE, F * FLOAT_RANDOM_SCALE) / FLOAT_RANDOM_SCALE;
        }

        /// <summary>Returns the lowercase hex SHA-256 hash of <paramref name="input"/>.</summary>
        /// <param name="input">The string to hash.</param>
        /// <returns>The lowercase hex SHA-256 hash.</returns>
        public static string GetSHA256Str(string input)
        {
            return GetSHA256(input.ToCharArray());
        }

        /// <summary>Returns the lowercase hex SHA-256 hash of a UTF-16 char array.</summary>
        /// <param name="data">The character array to hash.</param>
        /// <returns>The lowercase hex SHA-256 hash.</returns>
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

        /// <summary>Constant for 45 degrees in float.</summary>
        public const float DEG_45 = 45f;

        /// <summary>Constant for 90 degrees in float.</summary>
        public const float DEG_90 = 90f;

        /// <summary>Constant for 180 degrees in float.</summary>
        public const float DEG_180 = 180f;

        /// <summary>Constant for 270 degrees in float.</summary>
        public const float DEG_270 = 270f;

        /// <summary>Constant for 360 degrees in float.</summary>
        public const float DEG_360 = 360f;

        /// <summary>Sentinel value indicating an undefined coordinate.</summary>
        public const float UNDEFINED_COORDINATE = int.MaxValue;

        /// <summary>Shared random number generator used by all RND methods.</summary>
        private static readonly Random random_ = new();

        /// <summary>Maximum value for <see cref="Arc4random"/> range normalization (2^32).</summary>
        private static readonly long ARC4RANDOM_MAX = 4294967296L;

        /// <summary>Fast-math sine lookup table populated by <see cref="FmInit"/>.</summary>
        private static float[] fmSins;

        /// <summary>Fast-math cosine lookup table populated by <see cref="FmInit"/>.</summary>
        private static float[] fmCoss;

        /// <summary>Number of entries in the fast-math trig lookup tables.</summary>
        private const int FM_TRIG_TABLE_SIZE = 1024;

        /// <summary>Bitmask for wrapping trig table indices.</summary>
        private const int FM_TRIG_TABLE_MASK = FM_TRIG_TABLE_SIZE - 1;

        /// <summary>Precision multiplier for <see cref="FLOAT_RND_RANGE"/>.</summary>
        private const int FLOAT_RANDOM_SCALE = 1000;

        /// <summary>Cohen–Sutherland region code: left of rectangle.</summary>
        private const int COHEN_LEFT = 1;

        /// <summary>Cohen–Sutherland region code: right of rectangle.</summary>
        private const int COHEN_RIGHT = 2;

        /// <summary>Cohen–Sutherland region code: below rectangle.</summary>
        private const int COHEN_BOT = 4;

        /// <summary>Cohen–Sutherland region code: above rectangle.</summary>
        private const int COHEN_TOP = 8;

        /// <summary>Zero vector (0, 0).</summary>
        public static readonly Vector vectZero = new(0f, 0f);

        /// <summary>Sentinel vector indicating an undefined position.</summary>
        public static readonly Vector vectUndefined = new(UNDEFINED_COORDINATE, UNDEFINED_COORDINATE);
    }
}
