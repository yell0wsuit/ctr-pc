using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// CTR2-style classic finger trace that renders a solid white bezier ribbon with no particles.
    /// </summary>
    internal sealed class ClassicFingerTrace : FingerTrace
    {
        /// <summary>The base half-width contribution of the ribbon body.</summary>
        private const float RibbonBaseWidth = 12f;

        /// <summary>The minimum half-width preserved at the ribbon tip.</summary>
        private const float MinimumRibbonHalfWidth = 1f;

        /// <summary>The reusable ribbon vertex cache.</summary>
        private VertexPositionColor[] ribbonVerticesCache;

        /// <summary>
        /// Initializes a classic finger trace.
        /// </summary>
        public ClassicFingerTrace()
        {
        }

        /// <summary>
        /// Initializes a classic trace for a touch slot.
        /// </summary>
        /// <param name="_">
        /// Unused touch-slot placeholder retained for parity with the existing per-touch construction API.
        /// </param>
        public ClassicFingerTrace(int _)
            : this()
        {
        }

        /// <inheritdoc />
        public override void Draw()
        {
            DrawRibbon();

            Renderer.SetColor(Color.White);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <inheritdoc />
        protected override void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites)
        {
            AppendRibbonSampledPoints(sampledPoints);
        }

        /// <summary>
        /// Draws the sampled classic ribbon as a solid triangle strip.
        /// </summary>
        private void DrawRibbon()
        {
            if (!TryBuildRibbonGeometry(out List<Vector> sampledPoints))
            {
                return;
            }

            EnsureRibbonCache(sampledPoints.Count * 2);
            Color white = Color.White;
            for (int i = 0; i < sampledPoints.Count; i++)
            {
                Vector point = sampledPoints[i];
                Vector direction = GetPointDirection(sampledPoints, i);
                float directionLength = MAX(0.0001f, VectLength(direction));
                Vector normal = new(-(direction.Y / directionLength), direction.X / directionLength);
                float t = sampledPoints.Count == 1 ? 1f : i / (float)(sampledPoints.Count - 1);
                float halfWidth = i == sampledPoints.Count - 1
                    ? MinimumRibbonHalfWidth
                    : MinimumRibbonHalfWidth + (RibbonBaseWidth * t);
                Vector left = VectSub(point, VectMult(normal, halfWidth));
                Vector right = VectAdd(point, VectMult(normal, halfWidth));

                int vertexIndex = i * 2;
                ribbonVerticesCache[vertexIndex] = new VertexPositionColor(new Vector3(left.X, left.Y, 0f), white);
                ribbonVerticesCache[vertexIndex + 1] = new VertexPositionColor(new Vector3(right.X, right.Y, 0f), white);
            }

            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.DrawTriangleStrip(ribbonVerticesCache, sampledPoints.Count * 2);
        }

        /// <summary>
        /// Appends the sampled ribbon center line to the snapshot path.
        /// </summary>
        /// <param name="sampledPoints">The destination sampled-point list.</param>
        private void AppendRibbonSampledPoints(List<Vector> sampledPoints)
        {
            if (!TryBuildRibbonGeometry(out List<Vector> centerLine))
            {
                return;
            }

            sampledPoints.AddRange(centerLine);
        }

        /// <summary>
        /// Builds the sampled ribbon center line from the stored trace segments.
        /// </summary>
        /// <param name="sampledPoints">Receives the sampled center-line points.</param>
        /// <returns><see langword="true"/> when enough points exist to draw the ribbon; otherwise, <see langword="false"/>.</returns>
        private bool TryBuildRibbonGeometry(out List<Vector> sampledPoints)
        {
            sampledPoints = [];
            List<Vector> controlPoints = GetControlPoints();
            if (controlPoints.Count < 2)
            {
                return false;
            }

            int sampleCount = MAX(2, (controlPoints.Count * 2) - 1);
            Vector[] controlPointArray = [.. controlPoints];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = sampleCount == 1 ? 1f : i / (float)(sampleCount - 1);
                sampledPoints.Add(DrawHelper.CalcPathBezier(controlPointArray, controlPointArray.Length, t));
            }

            return sampledPoints.Count >= 2;
        }

        /// <summary>
        /// Collects the bezier control points implied by the live trace segments.
        /// </summary>
        /// <returns>The control-point list for ribbon sampling.</returns>
        private List<Vector> GetControlPoints()
        {
            List<Vector> controlPoints = [];
            if (Segments.Count == 0)
            {
                return controlPoints;
            }

            for (int i = 0; i < Segments.Count; i++)
            {
                controlPoints.Add(Segments[i].Start);
            }

            controlPoints.Add(Segments[^1].End);
            return controlPoints;
        }

        /// <summary>
        /// Returns the local tangent direction for a sampled ribbon point.
        /// </summary>
        /// <param name="sampledPoints">The sampled ribbon points.</param>
        /// <param name="index">The point index to evaluate.</param>
        /// <returns>The tangent direction at the requested sample.</returns>
        private static Vector GetPointDirection(List<Vector> sampledPoints, int index)
        {
            return sampledPoints.Count == 1
                ? vectZero
                : index == 0
                ? VectSub(sampledPoints[1], sampledPoints[0])
                : index == sampledPoints.Count - 1
                ? VectSub(sampledPoints[^1], sampledPoints[^2])
                : VectSub(sampledPoints[index + 1], sampledPoints[index - 1]);
        }

        /// <summary>
        /// Ensures the reusable ribbon vertex cache can hold the requested number of vertices.
        /// </summary>
        /// <param name="vertexCount">The required vertex capacity.</param>
        private void EnsureRibbonCache(int vertexCount)
        {
            if (ribbonVerticesCache == null || ribbonVerticesCache.Length < vertexCount)
            {
                ribbonVerticesCache = new VertexPositionColor[vertexCount];
            }
        }
    }
}
