using System;
using System.Collections.Generic;

using CutTheRope.Framework.Core;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Stores one or more quadratic spline segments.
    /// </summary>
    internal sealed class QuadraticBezierSpline
    {
        /// <summary>
        /// Quadratic Bezier segments that make up this spline.
        /// </summary>
        private readonly SplineSegment[] segments;

        /// <summary>
        /// Initializes a spline from one or more precomputed quadratic Bezier segments.
        /// </summary>
        /// <param name="segments">Segments to evaluate in order.</param>
        private QuadraticBezierSpline(params SplineSegment[] segments)
        {
            this.segments = segments;
        }

        /// <summary>
        /// Creates the default multi-segment preview path used by the non-lightning trace skins.
        /// </summary>
        /// <returns>The default preview spline path.</returns>
        public static QuadraticBezierSpline CreateDefaultTracePath()
        {
            return new(
                new SplineSegment(-95f, -5f, 65f, 25f, 30f, 30f),
                new SplineSegment(175f, -45f, -80f, 30f, 95f, -95f),
                new SplineSegment(-325f, 5f, 120f, 30f, 120f, 135f));
        }

        /// <summary>
        /// Creates the single quadratic segment used by the CTR2 lightning trace preview.
        /// </summary>
        /// <returns>The lightning preview spline path.</returns>
        public static QuadraticBezierSpline CreateLightningTracePath()
        {
            return new(new SplineSegment(-500f, -40f, 250f, 90f, 10f, 20f));
        }

        /// <summary>
        /// Evaluates the spline at the provided parameter using the CTR2 segment-selection rules.
        /// </summary>
        /// <param name="param">
        /// Segment-local parameter. Values greater than <c>1</c> advance across segments and, once the
        /// last segment is reached, continue evaluating that last segment with the overflow preserved.
        /// </param>
        /// <returns>The evaluated spline point.</returns>
        public Vector GetPointForParam(float param)
        {
            uint index = 0;
            while (param > 1f)
            {
                param -= 1f;
                index++;
            }

            if (index >= segments.Length)
            {
                param += 1f;
                index--;
            }

            SplineSegment segment = segments[index];
            return new Vector(
                segment.Cx + (param * ((param * segment.Ax) + (segment.Bx + segment.Bx))),
                segment.Cy + (param * ((param * segment.Ay) + (segment.By + segment.By))));
        }

        /// <summary>
        /// Evaluates a standard quadratic Bezier curve for the given control points.
        /// </summary>
        /// <param name="start">Curve start point.</param>
        /// <param name="control">Quadratic control point.</param>
        /// <param name="end">Curve end point.</param>
        /// <param name="t">Normalized interpolation parameter.</param>
        /// <returns>The evaluated curve point.</returns>
        public static Vector Evaluate(Vector start, Vector control, Vector end, float t)
        {
            float clampedT = Math.Clamp(t, 0f, 1f);
            float inverseT = 1f - clampedT;
            float startWeight = inverseT * inverseT;
            float controlWeight = 2f * inverseT * clampedT;
            float endWeight = clampedT * clampedT;

            return new Vector(
                (start.X * startWeight) + (control.X * controlWeight) + (end.X * endWeight),
                (start.Y * startWeight) + (control.Y * controlWeight) + (end.Y * endWeight));
        }

        /// <summary>
        /// Samples a smooth path through a control-point list for snapshot/debug visualization.
        /// </summary>
        /// <param name="controlPoints">The points to interpolate through.</param>
        /// <param name="subdivisionsPerCurve">How many samples to emit for each curve span.</param>
        /// <returns>Sampled points along the generated path.</returns>
        public static Vector[] SamplePath(IReadOnlyList<Vector> controlPoints, int subdivisionsPerCurve)
        {
            ArgumentNullException.ThrowIfNull(controlPoints);

            if (controlPoints.Count == 0)
            {
                return [];
            }

            if (controlPoints.Count == 1)
            {
                return [controlPoints[0]];
            }

            int subdivisions = Math.Max(1, subdivisionsPerCurve);
            if (controlPoints.Count == 2)
            {
                return SampleLine(controlPoints[0], controlPoints[1], subdivisions);
            }

            List<Vector> sampled = [controlPoints[0]];
            for (int i = 1; i < controlPoints.Count - 1; i++)
            {
                Vector start = i == 1
                    ? controlPoints[0]
                    : Midpoint(controlPoints[i - 1], controlPoints[i]);
                Vector end = i == controlPoints.Count - 2
                    ? controlPoints[^1]
                    : Midpoint(controlPoints[i], controlPoints[i + 1]);

                for (int step = 1; step <= subdivisions; step++)
                {
                    float t = step / (float)subdivisions;
                    sampled.Add(Evaluate(start, controlPoints[i], end, t));
                }
            }

            return [.. sampled];
        }

        /// <summary>
        /// Samples evenly spaced points along a straight line.
        /// </summary>
        /// <param name="start">Line start point.</param>
        /// <param name="end">Line end point.</param>
        /// <param name="subdivisions">Number of line subdivisions to sample.</param>
        /// <returns>The sampled line points, including both endpoints.</returns>
        private static Vector[] SampleLine(Vector start, Vector end, int subdivisions)
        {
            List<Vector> sampled = [start];
            for (int step = 1; step <= subdivisions; step++)
            {
                float t = step / (float)subdivisions;
                sampled.Add(new Vector(
                    start.X + ((end.X - start.X) * t),
                    start.Y + ((end.Y - start.Y) * t)));
            }

            return [.. sampled];
        }

        /// <summary>
        /// Computes the midpoint between two vectors.
        /// </summary>
        /// <param name="a">First point.</param>
        /// <param name="b">Second point.</param>
        /// <returns>The midpoint between <paramref name="a" /> and <paramref name="b" />.</returns>
        private static Vector Midpoint(Vector a, Vector b)
        {
            return new Vector((a.X + b.X) * 0.5f, (a.Y + b.Y) * 0.5f);
        }

        /// <summary>
        /// Stores the quadratic coefficients for one spline segment.
        /// </summary>
        /// <param name="Ax">Quadratic coefficient for the X axis.</param>
        /// <param name="Ay">Quadratic coefficient for the Y axis.</param>
        /// <param name="Bx">Linear coefficient for the X axis.</param>
        /// <param name="By">Linear coefficient for the Y axis.</param>
        /// <param name="Cx">Constant coefficient for the X axis.</param>
        /// <param name="Cy">Constant coefficient for the Y axis.</param>
        private readonly record struct SplineSegment(float Ax, float Ay, float Bx, float By, float Cx, float Cy);
    }
}
