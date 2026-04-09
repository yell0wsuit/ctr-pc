using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain.FingerTraces
{
    /// <summary>
    /// Shared ribbon trace base for named traces that only vary by gradient, glow, and particle presets.
    /// </summary>
    /// <param name="segmentLife">The lifetime in seconds assigned to each stored ribbon segment.</param>
    /// <param name="particleBurstDuration">The duration in seconds that particle emission remains active after a segment is appended.</param>
    /// <param name="particleEmissionRate">The particle emission rate used during the active burst window.</param>
    /// <param name="ribbonBaseWidth">The base half-width contribution applied along the ribbon body.</param>
    /// <param name="minimumRibbonHalfWidth">The minimum half-width preserved at the ribbon tip.</param>
    /// <param name="glowQuadIndex">
    /// The optional glow quad index. Pass <see langword="null"/> to disable glow sprite generation.
    /// </param>
    /// <param name="glowTranslateY">The local Y translation applied to the glow sprite pivot.</param>
    /// <param name="particles">The particle emitters owned by the trace.</param>
    internal abstract class RibbonFingerTrace(
        float segmentLife,
        float particleBurstDuration,
        float particleEmissionRate,
        float ribbonBaseWidth,
        float minimumRibbonHalfWidth,
        int? glowQuadIndex,
        float glowTranslateY,
        params FingerParticles[] particles) : ParticleFingerTrace(segmentLife, particleBurstDuration, particleEmissionRate, 10, particles)
    {
        /// <summary>
        /// Optional glow quad index used when appending the glow sprite to the snapshot.
        /// </summary>
        private readonly int? glowQuadIndex = glowQuadIndex;

        /// <summary>
        /// Local Y translation applied to the glow sprite pivot.
        /// </summary>
        private readonly float glowTranslateY = glowTranslateY;

        /// <summary>
        /// Minimum half-width preserved at the ribbon tip.
        /// </summary>
        private readonly float minimumRibbonHalfWidth = minimumRibbonHalfWidth;

        /// <summary>
        /// Base half-width contribution applied along the ribbon body.
        /// </summary>
        private readonly float ribbonBaseWidth = ribbonBaseWidth;

        /// <summary>
        /// Reusable ribbon vertex cache used for triangle-strip drawing.
        /// </summary>
        private VertexPositionColor[] ribbonVerticesCache;

        /// <summary>
        /// Current glow sprite position derived from the latest segment start point.
        /// </summary>
        private Vector glowPosition;

        /// <summary>
        /// Current glow alpha derived from segment count and smoothed direction magnitude.
        /// </summary>
        private float glowAlpha;

        /// <inheritdoc />
        public override void Draw()
        {
            base.Draw();
            DrawRibbon();
            Renderer.SetColor(Color.White);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <inheritdoc />
        protected override void AppendSampledPoints(List<Vector> sampledPoints)
        {
            if (!TryBuildRibbonGeometry(out List<Vector> centerLine))
            {
                return;
            }

            sampledPoints.AddRange(centerLine);
        }

        /// <inheritdoc />
        protected override void AppendSprites(List<FingerTraceSpritePose> sprites)
        {
            if (!glowQuadIndex.HasValue || glowAlpha <= 0f)
            {
                return;
            }

            sprites.Add(new FingerTraceSpritePose(
                FingerTraceSpriteKind.Glow,
                Resources.Img.FingerTraceGlow,
                glowQuadIndex.Value,
                glowPosition,
                AverageRotation + DEG_90,
                1f,
                glowAlpha,
                FingerTraceBlendMode.Alpha,
                glowTranslateY));
        }

        /// <inheritdoc />
        protected override void OnSegmentAdded(Vector start, Vector end, Vector delta)
        {
            glowPosition = start;
        }

        /// <inheritdoc />
        protected override void OnHeadStateUpdated(Vector averageDirection)
        {
            glowAlpha = MIN(Segments.Count / 5f, VectLength(averageDirection) / 10f);
        }

        /// <inheritdoc />
        protected override void OnReset()
        {
            glowPosition = vectZero;
            glowAlpha = 0f;
        }

        /// <summary>
        /// Gets the ribbon color for the normalized position <paramref name="t"/> along the strip.
        /// </summary>
        /// <param name="t">The normalized ribbon position in the range <c>[0, 1]</c>.</param>
        /// <returns>The ribbon color at <paramref name="t"/>.</returns>
        protected abstract RGBAColor GetRibbonColor(float t);

        /// <summary>
        /// Draws the sampled ribbon as a colored triangle strip.
        /// </summary>
        private void DrawRibbon()
        {
            if (!TryBuildRibbonGeometry(out List<Vector> sampledPoints))
            {
                return;
            }

            EnsureRibbonCache(sampledPoints.Count * 2);
            for (int i = 0; i < sampledPoints.Count; i++)
            {
                Vector point = sampledPoints[i];
                Vector direction = GetPointDirection(sampledPoints, i);
                float directionLength = MAX(0.0001f, VectLength(direction));
                Vector normal = new(-(direction.Y / directionLength), direction.X / directionLength);
                float t = sampledPoints.Count == 1 ? 1f : i / (float)(sampledPoints.Count - 1);
                float halfWidth = i == sampledPoints.Count - 1
                    ? minimumRibbonHalfWidth
                    : minimumRibbonHalfWidth + (ribbonBaseWidth * t);
                Vector left = VectSub(point, VectMult(normal, halfWidth));
                Vector right = VectAdd(point, VectMult(normal, halfWidth));
                Color color = GetRibbonColor(t).ToXNA();

                int vertexIndex = i * 2;
                ribbonVerticesCache[vertexIndex] = new VertexPositionColor(new Vector3(left.X, left.Y, 0f), color);
                ribbonVerticesCache[vertexIndex + 1] = new VertexPositionColor(new Vector3(right.X, right.Y, 0f), color);
            }

            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            Renderer.DrawTriangleStrip(ribbonVerticesCache, sampledPoints.Count * 2);
        }

        /// <summary>
        /// Builds the sampled ribbon center line from the stored live segments.
        /// </summary>
        /// <param name="sampledPoints">Receives the sampled ribbon center-line points.</param>
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
        /// Collects the bezier control points implied by the current live trace segments.
        /// </summary>
        /// <returns>The control-point list used for ribbon sampling.</returns>
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
        /// Returns the local tangent direction for one sampled ribbon point.
        /// </summary>
        /// <param name="sampledPoints">The sampled ribbon points.</param>
        /// <param name="index">The point index to evaluate.</param>
        /// <returns>The tangent direction at <paramref name="index"/>.</returns>
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
