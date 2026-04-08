using System;
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
    /// CTR2-style (PRODUCT)RED finger trace with dual particle layers (glow + core) and a warm-toned ribbon.
    /// </summary>
    internal sealed class RedFingerTrace : FingerTrace
    {
        /// <summary>The lifetime of each stored ribbon segment in seconds.</summary>
        private const float SegmentLife = 0.15f;

        /// <summary>The duration of the particle burst after each appended segment.</summary>
        private const float ParticleBurstDuration = 0.1f;

        /// <summary>The emission rate used while the burst timer is active.</summary>
        private const float ParticleEmissionRate = 35f;

        /// <summary>The base half-width contribution of the ribbon body.</summary>
        private const float RibbonBaseWidth = 12f;

        /// <summary>The minimum half-width preserved at the ribbon tip.</summary>
        private const float MinimumRibbonHalfWidth = 1f;

        /// <summary>The number of direction samples kept for head smoothing.</summary>
        private const int MaximumDirectionHistory = 10;

        /// <summary>The additive glow particle layer.</summary>
        private readonly RedTraceParticles glowParticles = new(0.3f, FingerTraceBlendMode.Additive);

        /// <summary>The alpha-blended core particle layer.</summary>
        private readonly RedTraceParticles coreParticles = new(0.75f, FingerTraceBlendMode.Alpha);

        /// <summary>Recent segment directions used to smooth the emitter rotation.</summary>
        private readonly List<Vector> directionHistory = [];

        /// <summary>The reusable ribbon vertex cache.</summary>
        private VertexPositionColor[] ribbonVerticesCache;

        /// <summary>The remaining time in the active particle burst.</summary>
        private float particleTimer;

        /// <summary>The smoothed head rotation in degrees.</summary>
        private float averageRotation;

        /// <summary>
        /// Initializes a red finger trace.
        /// </summary>
        public RedFingerTrace()
        {
        }

        /// <summary>
        /// Initializes a red trace for a touch slot.
        /// </summary>
        /// <param name="_">
        /// Unused touch-slot placeholder retained for parity with the existing per-touch construction API.
        /// </param>
        public RedFingerTrace(int _)
            : this()
        {
        }

        /// <inheritdoc />
        protected override bool HasLiveParticles => glowParticles.HasLiveParticles || coreParticles.HasLiveParticles;

        /// <inheritdoc />
        public override void AddSegment(float startX, float startY, float endX, float endY)
        {
            Vector start = new(startX, startY);
            Vector end = new(endX, endY);
            Vector delta = VectSub(end, start);

            particleTimer = ParticleBurstDuration;
            StoreSegment(start, end, SegmentLife);
            directionHistory.Add(delta);
            RefreshHeadState();
            glowParticles.SetPosition(end);
            coreParticles.SetPosition(end);
        }

        /// <inheritdoc />
        public override void Draw()
        {
            List<FingerTraceSpritePose> glowSprites = [];
            glowParticles.AppendSprites(glowSprites);
            if (glowSprites.Count > 0)
            {
                Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
                foreach (FingerTraceSpritePose sprite in glowSprites)
                {
                    DrawSpritePose(sprite);
                }
            }

            List<FingerTraceSpritePose> coreSprites = [];
            coreParticles.AppendSprites(coreSprites);
            if (coreSprites.Count > 0)
            {
                Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
                foreach (FingerTraceSpritePose sprite in coreSprites)
                {
                    DrawSpritePose(sprite);
                }
            }

            DrawRibbon();

            Renderer.SetColor(Color.White);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <inheritdoc />
        protected override void UpdateCore(float delta)
        {
            particleTimer -= delta;
            float rate = particleTimer > 0f ? ParticleEmissionRate : 0f;
            glowParticles.SetEmissionRate(rate);
            coreParticles.SetEmissionRate(rate);
            RefreshHeadState();
            glowParticles.Update(delta);
            coreParticles.Update(delta);
        }

        /// <inheritdoc />
        protected override void ResetCore()
        {
            directionHistory.Clear();
            glowParticles.Reset();
            coreParticles.Reset();
            particleTimer = 0f;
            averageRotation = 0f;
        }

        /// <inheritdoc />
        protected override void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites)
        {
            AppendRibbonSampledPoints(sampledPoints);
            glowParticles.AppendSprites(sprites);
            coreParticles.AppendSprites(sprites);
        }

        /// <summary>
        /// Draws the sampled red ribbon as a colored triangle strip.
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
                    ? MinimumRibbonHalfWidth
                    : MinimumRibbonHalfWidth + (RibbonBaseWidth * t);
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
        /// Returns the averaged head direction derived from recent segment deltas.
        /// </summary>
        /// <returns>The averaged direction vector.</returns>
        private Vector GetAverageDirection()
        {
            if (directionHistory.Count == 0)
            {
                return vectZero;
            }

            Vector total = vectZero;
            for (int i = 0; i < directionHistory.Count; i++)
            {
                total = VectAdd(total, directionHistory[i]);
            }

            return VectDiv(total, directionHistory.Count);
        }

        /// <summary>
        /// Refreshes the emitter rotation from the recent direction history.
        /// </summary>
        private void RefreshHeadState()
        {
            while (directionHistory.Count > MaximumDirectionHistory)
            {
                directionHistory.RemoveAt(0);
            }

            Vector averageDirection = GetAverageDirection();
            averageRotation = RADIANS_TO_DEGREES(MathF.Atan2(averageDirection.Y, averageDirection.X));
            float particleAngle = averageRotation + DEG_180;
            glowParticles.SetRotation(particleAngle);
            coreParticles.SetRotation(particleAngle);
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
        /// Returns a 2-phase warm gradient: dark red → bright red → warm orange.
        /// </summary>
        /// <summary>
        /// Returns the red ribbon gradient color for the normalized position along the strip.
        /// </summary>
        /// <param name="t">The normalized ribbon position in the range <c>[0, 1]</c>.</param>
        /// <returns>The ribbon color at <paramref name="t"/>.</returns>
        private static RGBAColor GetRibbonColor(float t)
        {
            if (t < 0.7f)
            {
                float blend = t * 2f;
                return RGBAColor.MakeRGBA(
                    MathHelper.Lerp(0.7882f, 0.98824f, blend),
                    MathHelper.Lerp(0.2157f, 0.20784f, blend),
                    MathHelper.Lerp(0.2980f, 0.16863f, blend),
                    MathHelper.Lerp(0f, 1f, blend));
            }

            float fade = (t - 0.7f) * 2f;
            return RGBAColor.MakeRGBA(
                MathHelper.Lerp(0.98824f, 0.95294f, fade),
                MathHelper.Lerp(0.20784f, 0.61961f, fade),
                MathHelper.Lerp(0.16863f, 0.41961f, fade),
                1f);
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
