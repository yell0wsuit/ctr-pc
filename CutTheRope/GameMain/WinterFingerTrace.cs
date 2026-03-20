using System;
using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// CTR2-style winter finger trace with an icy ribbon, winter glow, and snowflake particles.
    /// </summary>
    internal sealed class WinterFingerTrace : FingerTrace
    {
        private const int GlowQuadIndex = 1;
        private const float SegmentLife = 0.1f;
        private const float ParticleBurstDuration = 0.1f;
        private const float ParticleEmissionRate = 50f;
        private const float RibbonBaseWidth = 8f;
        private const float MinimumRibbonHalfWidth = 1f;
        private const int MaximumDirectionHistory = 10;

        private readonly WinterTraceParticles particles = new();
        private readonly List<Vector> directionHistory = [];

        private VertexPositionColor[] ribbonVerticesCache;
        private float particleTimer;
        private float averageRotation;
        private float headRotation;
        private float headScale;
        private Vector headPosition;

        public WinterFingerTrace()
        {
        }

        /// <summary>
        /// Initializes a winter trace for a touch slot.
        /// </summary>
        /// <param name="_">
        /// Unused touch-slot placeholder retained for parity with the existing per-touch construction API.
        /// </param>
        public WinterFingerTrace(int _)
            : this()
        {
        }

        protected override bool HasLiveParticles => particles.HasLiveParticles;

        /// <summary>
        /// Adds a new winter segment with the fixed CTR2 winter lifetime.
        /// </summary>
        public override void AddSegment(float startX, float startY, float endX, float endY)
        {
            Vector start = new(startX, startY);
            Vector end = new(endX, endY);
            Vector delta = VectSub(end, start);

            particleTimer = ParticleBurstDuration;
            headPosition = start;
            StoreSegment(start, end, SegmentLife);
            directionHistory.Add(delta);
            RefreshHeadState();
            particles.SetPosition(end);
        }

        /// <summary>
        /// Draws winter particles, the winter glow head, and the icy ribbon strip.
        /// </summary>
        public override void Draw()
        {
            List<FingerTraceSpritePose> particleSprites = [];
            particles.AppendSprites(particleSprites);
            foreach (FingerTraceSpritePose sprite in particleSprites)
            {
                DrawSpritePose(sprite);
            }

            if (TryCreateGlowSprite(out FingerTraceSpritePose glowSprite))
            {
                DrawSpritePose(glowSprite);
            }

            DrawRibbon();

            Renderer.SetColor(Color.White);
            Renderer.SetBlendFunc(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
        }

        /// <summary>
        /// Advances particle emission and the averaged head direction / scale state.
        /// </summary>
        protected override void UpdateCore(float delta)
        {
            particleTimer -= delta;
            particles.SetEmissionRate(particleTimer > 0f ? ParticleEmissionRate : 0f);
            RefreshHeadState();
            particles.Update(delta);
        }

        /// <summary>
        /// Clears winter-specific transient state.
        /// </summary>
        protected override void ResetCore()
        {
            directionHistory.Clear();
            particles.Reset();
            particleTimer = 0f;
            averageRotation = 0f;
            headRotation = 0f;
            headScale = 0f;
            headPosition = default;
        }

        /// <summary>
        /// Publishes the winter ribbon path together with glow and particle sprite metadata.
        /// </summary>
        protected override void BuildSnapshot(List<Vector> sampledPoints, List<FingerTraceSpritePose> sprites)
        {
            AppendRibbonSampledPoints(sampledPoints);

            if (TryCreateGlowSprite(out FingerTraceSpritePose glowSprite))
            {
                sprites.Add(glowSprite);
            }

            particles.AppendSprites(sprites);
        }

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
                float halfWidth = MinimumRibbonHalfWidth + (RibbonBaseWidth * t);
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

        private void AppendRibbonSampledPoints(List<Vector> sampledPoints)
        {
            if (!TryBuildRibbonGeometry(out List<Vector> centerLine))
            {
                return;
            }

            sampledPoints.AddRange(centerLine);
        }

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

        private void RefreshHeadState()
        {
            while (directionHistory.Count > MaximumDirectionHistory)
            {
                directionHistory.RemoveAt(0);
            }

            Vector averageDirection = GetAverageDirection();
            averageRotation = RADIANS_TO_DEGREES(MathF.Atan2(averageDirection.Y, averageDirection.X));
            headRotation = averageRotation + DEG_90;
            headScale = MIN(Segments.Count / 5f, VectLength(averageDirection) / 10f);
            particles.SetRotation(averageRotation + DEG_180);
        }

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

        private bool TryCreateGlowSprite(out FingerTraceSpritePose glowSprite)
        {
            if (Segments.Count == 0 || headScale <= 0f)
            {
                glowSprite = default;
                return false;
            }

            glowSprite = new FingerTraceSpritePose(
                FingerTraceSpriteKind.Glow,
                Resources.Img.FingerTraceGlow,
                GlowQuadIndex,
                headPosition,
                headRotation,
                headScale,
                1f,
                FingerTraceBlendMode.Alpha);
            return true;
        }

        private static RGBAColor GetRibbonColor(float t)
        {
            if (t < 0.5f)
            {
                float blend = t * 2f;
                return RGBAColor.MakeRGBA(
                    MathHelper.Lerp(0.7f, 0.51765f, blend),
                    MathHelper.Lerp(1f, 0.59608f, blend),
                    MathHelper.Lerp(1f, 0.75686f, blend),
                    MathHelper.Lerp(0f, 1f, blend));
            }

            float fade = (t - 0.5f) * 2f;
            return RGBAColor.MakeRGBA(
                MathHelper.Lerp(0.51765f, 1f, fade),
                MathHelper.Lerp(0.59608f, 1f, fade),
                MathHelper.Lerp(0.75686f, 1f, fade),
                MathHelper.Lerp(1f, 0f, fade));
        }

        private void EnsureRibbonCache(int vertexCount)
        {
            if (ribbonVerticesCache == null || ribbonVerticesCache.Length < vertexCount)
            {
                ribbonVerticesCache = new VertexPositionColor[vertexCount];
            }
        }
    }
}
